import { Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { TutorStreamService } from '../datos/tutor-stream.service';
import { SesionService } from '../datos/sesion.service';
import { ClaseResumen } from '../datos/tutor.modelos';
import { ContenidoClase, DocenteService, ReporteClase } from '../datos/docente.service';

/**
 * Pantalla del docente (RF-34). El material no se sube aquí: se copia a la carpeta del
 * curso y aquí se ve cómo quedó, junto con el examen, la ampliación y el reporte.
 */
@Component({
  selector: 'app-docente',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './docente.page.html',
  styleUrl: './docente.page.css',
})
export class DocentePage {
  private readonly api = inject(TutorStreamService);
  private readonly docente = inject(DocenteService);
  readonly sesion = inject(SesionService);

  readonly clases = signal<ClaseResumen[]>([]);
  readonly activa = signal<ClaseResumen | null>(null);
  readonly contenido = signal<ContenidoClase | null>(null);
  readonly reporte = signal<ReporteClase | null>(null);
  readonly error = signal('');

  readonly tramosNota = ['0-10', '11-14', '15-17', '18-20'];

  async ngOnInit(): Promise<void> {
    try {
      const clases = await this.api.clases();
      this.clases.set(clases);
      if (clases.length) await this.abrir(clases[0]);
    } catch {
      this.error.set('No se pudo cargar tus clases.');
    }
  }

  async abrir(clase: ClaseResumen): Promise<void> {
    this.activa.set(clase);
    this.contenido.set(null);
    this.reporte.set(null);
    this.error.set('');

    try {
      const [contenido, reporte] = await Promise.all([
        this.docente.contenido(clase.claseId),
        this.docente.reporte(clase.claseId),
      ]);
      this.contenido.set(contenido);
      this.reporte.set(reporte);
    } catch {
      this.error.set('No se pudo cargar esta clase.');
    }
  }

  async alternarAmpliacion(permitida: boolean): Promise<void> {
    const clase = this.activa();
    const contenido = this.contenido();
    if (!clase || !contenido) return;

    const { ampliacionPermitida } = await this.docente.ampliacion(clase.claseId, permitida);
    this.contenido.set({ ...contenido, ampliacionPermitida });
  }

  maximo(distribucion: Partial<Record<string, number>>): number {
    return Math.max(1, ...Object.values(distribucion).map((v) => v ?? 0));
  }
}
