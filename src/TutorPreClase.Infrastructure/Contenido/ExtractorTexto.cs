using System.Text;
using DocumentFormat.OpenXml.Packaging;
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

        return documento.GetPages()
            .Select(p => new PaginaExtraida(p.Number, p.Text ?? ""))
            .ToList();
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
