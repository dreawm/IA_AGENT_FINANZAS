import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { Agente, ClaseResumen, Credencial, Fuente, ModoChat, Progreso } from './tutor.modelos';

export type EventoTutor =
  | { tipo: 'modo'; modo: ModoChat; ampliacionPermitida: boolean }
  | { tipo: 'token'; texto: string }
  | { tipo: 'herramienta'; nombre: string; estado: string }
  | { tipo: 'progreso'; progreso: Progreso }
  | { tipo: 'fuentes'; fuentes: Fuente[] }
  | { tipo: 'aviso'; mensaje: string }
  | { tipo: 'fin'; usaAmpliacion: boolean }
  | { tipo: 'error'; mensaje: string; reintentable: boolean };

const API = '/api/v1';

/**
 * Consume los eventos SSE del tutor con `fetch` streaming (SDD §8.1). La web no
 * interpreta el texto para construir controles: solo pinta lo que llega.
 */
@Injectable({ providedIn: 'root' })
export class TutorStreamService {
  private readonly http = inject(HttpClient);

  agentes(): Promise<Agente[]> {
    return firstValueFrom(this.http.get<Agente[]>(`${API}/agentes`));
  }

  clases(): Promise<ClaseResumen[]> {
    return firstValueFrom(this.http.get<ClaseResumen[]>(`${API}/alumno/clases`));
  }

  credenciales(): Promise<Credencial[]> {
    return firstValueFrom(this.http.get<Credencial[]>(`${API}/alumno/credenciales`));
  }

  /** La clave se envía y se olvida: no pasa por localStorage (SDD §8.1). */
  conectarCredencial(agenteId: string, clave: string) {
    return firstValueFrom(
      this.http.put<Credencial>(`${API}/alumno/credenciales/${agenteId}`, { clave }),
    );
  }

  desconectarCredencial(agenteId: string) {
    return firstValueFrom(this.http.delete<void>(`${API}/alumno/credenciales/${agenteId}`));
  }

  abrirConversacion(claseId: string, agenteId: string) {
    return firstValueFrom(
      this.http.post<{ conversacionId: string; modo: ModoChat; agenteId: string }>(
        `${API}/clases/${claseId}/conversacion`,
        { agenteId },
      ),
    );
  }

  historial(conversacionId: string) {
    return firstValueFrom(
      this.http.get<
        { rol: string; texto: string; fuentes: string | null; usaAmpliacion: boolean }[]
      >(`${API}/conversaciones/${conversacionId}/mensajes`),
    );
  }

  iniciarExamen(conversacionId: string) {
    return firstValueFrom(
      this.http.post<{ intentoId: string; modo: ModoChat }>(
        `${API}/conversaciones/${conversacionId}/examen`,
        {},
      ),
    );
  }

  cambiarAgente(conversacionId: string, agenteId: string) {
    return firstValueFrom(
      this.http.put<{ agenteId: string }>(`${API}/conversaciones/${conversacionId}/agente`, {
        agenteId,
      }),
    );
  }

  /** Envía un mensaje y va entregando los eventos a medida que llegan. */
  async *enviar(
    conversacionId: string,
    texto: string,
    cabeceras: Record<string, string>,
  ): AsyncGenerator<EventoTutor> {
    const respuesta = await fetch(`${API}/conversaciones/${conversacionId}/mensajes`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', ...cabeceras },
      body: JSON.stringify({ texto }),
    });

    if (!respuesta.ok || !respuesta.body) {
      yield { tipo: 'error', mensaje: `La API respondió ${respuesta.status}.`, reintentable: true };
      return;
    }

    const lector = respuesta.body.getReader();
    const decodificador = new TextDecoder();
    let pendiente = '';

    while (true) {
      const { done, value } = await lector.read();
      if (done) break;

      pendiente += decodificador.decode(value, { stream: true });

      // Los eventos SSE se separan por línea en blanco.
      const bloques = pendiente.split('\n\n');
      pendiente = bloques.pop() ?? '';

      for (const bloque of bloques) {
        const evento = this.interpretar(bloque);
        if (evento) yield evento;
      }
    }
  }

  private interpretar(bloque: string): EventoTutor | null {
    let nombre = '';
    let datos = '';

    for (const linea of bloque.split('\n')) {
      if (linea.startsWith('event:')) nombre = linea.slice(6).trim();
      else if (linea.startsWith('data:')) datos += linea.slice(5).trim();
    }

    if (!nombre || !datos) return null;

    let cuerpo: any;
    try {
      cuerpo = JSON.parse(datos);
    } catch {
      return null;
    }

    switch (nombre) {
      case 'modo':
        return { tipo: 'modo', modo: cuerpo.modo, ampliacionPermitida: cuerpo.ampliacionPermitida };
      case 'token':
        return { tipo: 'token', texto: cuerpo.texto };
      case 'herramienta':
        return { tipo: 'herramienta', nombre: cuerpo.nombre, estado: cuerpo.estado };
      case 'progreso':
        return { tipo: 'progreso', progreso: cuerpo };
      case 'fuentes':
        return { tipo: 'fuentes', fuentes: cuerpo };
      case 'aviso':
        return { tipo: 'aviso', mensaje: cuerpo.mensaje };
      case 'fin':
        return { tipo: 'fin', usaAmpliacion: cuerpo.usaAmpliacion };
      case 'error':
        return { tipo: 'error', mensaje: cuerpo.mensaje, reintentable: cuerpo.reintentable };
      default:
        return null;
    }
  }
}
