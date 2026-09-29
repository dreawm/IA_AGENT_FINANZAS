using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using UglyToad.PdfPig;

namespace TutorPreClase.Infrastructure.Contenido;

/// <summary>Una pagina o diapositiva de texto extraido.</summary>
public sealed record PaginaExtraida(int Pagina, string Texto);

public interface IExtractorTexto
{
    bool Soporta(string extension);
    IReadOnlyList<PaginaExtraida> Extraer(Stream archivo);
}

/// <summary>
/// Extrae texto conservando la pagina o numero de diapositiva (SDD §6.1). Sin OCR:
/// un PDF escaneado devuelve paginas vacias y el docente recibe el aviso.
/// </summary>
public sealed class ExtractorPdf : IExtractorTexto
{
    public bool Soporta(string extension) => extension is "pdf";

    public IReadOnlyList<PaginaExtraida> Extraer(Stream archivo)
    {
        using var documento = PdfDocument.Open(archivo);

        // Por palabras y no con Page.Text, que las pega entre si ("2024Autor").
        return documento.GetPages()
            .Select(p => new PaginaExtraida(
                p.Number, Normalizar(string.Join(" ", p.GetWords().Select(w => w.Text)))))
            .ToList();
    }

    /// <summary>
    /// Repara lo que muchos PDF pierden al extraerse: las ligaduras tipograficas (ﬁ, ﬂ)
    /// y el "ti" ligado que la fuente no sabe traducir y llega como U+FFFD
    /// ("ges�ón" → "gestión"). Sin esto el tutor citaria texto roto.
    /// </summary>
    public static string Normalizar(string texto)
    {
        texto = texto.Normalize(NormalizationForm.FormKC);
        return LigaduraTi.Replace(texto, "ti");
    }

    private static readonly System.Text.RegularExpressions.Regex LigaduraTi =
        new(@"�(?=\p{Ll})", System.Text.RegularExpressions.RegexOptions.Compiled);
}

/// <summary>
/// Hojas de calculo: cada hoja es una "pagina" y cada fila una linea con sus celdas
/// separadas por " | ", para que el tutor pueda citar [archivo, p. N] como con un PDF.
/// </summary>
public sealed class ExtractorXlsx : IExtractorTexto
{
    public bool Soporta(string extension) => extension is "xlsx";

    public IReadOnlyList<PaginaExtraida> Extraer(Stream archivo)
    {
        using var libro = SpreadsheetDocument.Open(archivo, false);

        var partes = libro.WorkbookPart;
        var hojas = partes?.Workbook?.Sheets?.Elements<Sheet>();
        if (partes is null || hojas is null) return [];

        var compartidas = partes.SharedStringTablePart?.SharedStringTable?
            .Elements<SharedStringItem>().Select(s => s.InnerText).ToList() ?? [];

        var paginas = new List<PaginaExtraida>();
        var numero = 0;

        foreach (var hoja in hojas)
        {
            numero++;
            if (hoja.Id?.Value is not string id || partes.GetPartById(id) is not WorksheetPart parte) continue;

            var sb = new StringBuilder().AppendLine($"Hoja: {hoja.Name}");

            foreach (var fila in parte.Worksheet?.Descendants<Row>() ?? [])
            {
                var celdas = fila.Elements<Cell>()
                    .Select(c => Valor(c, compartidas).Trim())
                    .Where(v => v.Length > 0)
                    .ToList();

                if (celdas.Count > 0) sb.AppendLine(string.Join(" | ", celdas));
            }

            paginas.Add(new PaginaExtraida(numero, sb.ToString()));
        }

        return paginas;
    }

    private static string Valor(Cell celda, List<string> compartidas)
    {
        var crudo = celda.CellValue?.Text ?? celda.InnerText;
        var tipo = celda.DataType?.Value;

        if (tipo == CellValues.SharedString)
            return int.TryParse(crudo, out var i) && i < compartidas.Count ? compartidas[i] : "";

        if (tipo == CellValues.InlineString)
            return celda.InlineString?.InnerText ?? "";

        // Los numeros se guardan con toda su precision binaria; se muestran como en la hoja.
        return double.TryParse(crudo, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var numero)
            ? numero.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture)
            : crudo;
    }
}

public sealed class ExtractorPptx : IExtractorTexto
{
    public bool Soporta(string extension) => extension is "pptx";

    public IReadOnlyList<PaginaExtraida> Extraer(Stream archivo)
    {
        using var presentacion = PresentationDocument.Open(archivo, false);

        var parteP = presentacion.PresentationPart;
        if (parteP?.Presentation?.SlideIdList is null) return [];

        var paginas = new List<PaginaExtraida>();
        var numero = 0;

        foreach (var slideId in parteP.Presentation.SlideIdList.Elements<DocumentFormat.OpenXml.Presentation.SlideId>())
        {
            numero++;
            if (slideId.RelationshipId?.Value is not string rel) continue;
            if (parteP.GetPartById(rel) is not SlidePart slide) continue;

            var texto = slide.Slide?.InnerText ?? "";
            paginas.Add(new PaginaExtraida(numero, texto));
        }

        return paginas;
    }
}

public sealed class ExtractorDocx : IExtractorTexto
{
    public bool Soporta(string extension) => extension is "docx";

    public IReadOnlyList<PaginaExtraida> Extraer(Stream archivo)
    {
        using var documento = WordprocessingDocument.Open(archivo, false);

        var cuerpo = documento.MainDocumentPart?.Document?.Body;
        if (cuerpo is null) return [];

        // DOCX no tiene paginacion fiable sin renderizar: se trata como una sola pagina.
        var sb = new StringBuilder();
        foreach (var parrafo in cuerpo.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
            sb.AppendLine(parrafo.InnerText);

        return [new PaginaExtraida(1, sb.ToString())];
    }
}

public sealed class ExtractorTextoPlano : IExtractorTexto
{
    public bool Soporta(string extension) => extension is "md" or "txt";

    public IReadOnlyList<PaginaExtraida> Extraer(Stream archivo)
    {
        using var lector = new StreamReader(archivo, Encoding.UTF8);
        return [new PaginaExtraida(1, lector.ReadToEnd())];
    }
}

public interface IExtractorTextoFactory
{
    IExtractorTexto? Para(string nombreArchivo);
}

public sealed class ExtractorTextoFactory(IEnumerable<IExtractorTexto> extractores) : IExtractorTextoFactory
{
    public IExtractorTexto? Para(string nombreArchivo)
    {
        var extension = Path.GetExtension(nombreArchivo).TrimStart('.').ToLowerInvariant();
        return extractores.FirstOrDefault(e => e.Soporta(extension));
    }
}
