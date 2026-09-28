/** Contratos del chat del tutor (SDD §7). */

export type ModoChat = 'Consulta' | 'Evaluacion' | 'Revision';

export interface Agente {
  id: string;
  nombre: string;
  descripcion?: string;
  /** Consola del proveedor donde el alumno genera su credencial. */
  consola?: string;
  conectado: boolean;
}

/** Lo único que la web sabe de una credencial: nunca la clave (SDD §7). */
export interface Credencial {
  agenteId: string;
  ultimos4: string;
  estado: 'Valida' | 'Invalida';
  creadaEn: string;
  ultimoUsoEn: string | null;
}

export interface ClaseResumen {
  claseId: string;
  titulo: string;
  inicio: string;
  examen: { publicado: boolean; abreEn: string; cierraEn: string } | null;
}

export interface Fuente {
  archivo: string;
  pagina: number;
  valida: boolean;
}

export interface Progreso {
  respondidas: number;
  total: number;
  segundosRestantes: number | null;
}

/** Un mensaje del chat; el bloque de ampliación se separa para pintarlo aparte. */
export interface Mensaje {
  rol: 'Alumno' | 'Agente';
  texto: string;
  ampliacion?: string;
  fuentes?: Fuente[];
  aviso?: string;
  enCurso?: boolean;
}

export const MARCA_AMPLIACION = /Ampliaci[óo]n fuera del material\s*:/i;

/**
 * Separa la ampliación del resto para que la web nunca la muestre como si fuera
 * material del docente (SDD §8.1).
 */
export function separarAmpliacion(texto: string): { cuerpo: string; ampliacion?: string } {
  const marca = MARCA_AMPLIACION.exec(texto);
  if (!marca) return { cuerpo: texto };

  return {
    cuerpo: texto.slice(0, marca.index).trimEnd(),
    ampliacion: texto.slice(marca.index + marca[0].length).trim(),
  };
}
