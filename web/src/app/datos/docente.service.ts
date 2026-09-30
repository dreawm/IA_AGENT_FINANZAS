import { environment } from '../../environments/environment';
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

const API = `${environment.apiUrl}/v1`;

export interface ContenidoClase {
  archivos: { archivoId: string; nombre: string; estado: string; error: string | null; paginas: number }[];
  contextoClase: { tokens: number; truncado: boolean };
  ampliacionPermitida: boolean;
}

export interface ReporteClase {
  clase: string;
  alumnos: number;
  promedioNota: number;
  distribucionNotas: Partial<Record<string, number>>;
  distribucionNiveles: Record<string, number>;
  niveles: { alumnoId: string; nombre: string; nivel: string; temasDebiles: string[] }[];
  temasMasFallados: { tema: string; fallos: number; respondidas: number; porcentajeFallo: number }[];
  dudasFueraDelMaterial: { texto: string; tema: string | null; conAmpliacion: boolean; creadoEn: string }[];
}

/** Lo que ve y ajusta el docente de cada clase (RF-34, RF-13, RF-16, RF-20, RF-22). */
@Injectable({ providedIn: 'root' })
export class DocenteService {
  private readonly http = inject(HttpClient);

  contenido(claseId: string): Promise<ContenidoClase> {
    return firstValueFrom(this.http.get<ContenidoClase>(`${API}/clases/${claseId}/contenido`));
  }

  reporte(claseId: string): Promise<ReporteClase> {
    return firstValueFrom(this.http.get<ReporteClase>(`${API}/clases/${claseId}/reporte`));
  }

  ampliacion(claseId: string, permitida: boolean) {
    return firstValueFrom(
      this.http.put<{ ampliacionPermitida: boolean }>(`${API}/clases/${claseId}/ampliacion`, { permitida }),
    );
  }
}
