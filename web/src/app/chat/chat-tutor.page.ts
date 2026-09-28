import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { EventoTutor, TutorStreamService } from '../datos/tutor-stream.service';
import { SesionService } from '../datos/sesion.service';
import {
  Agente,
  ClaseResumen,
  Credencial,
  Mensaje,
  ModoChat,
  Progreso,
  separarAmpliacion,
} from '../datos/tutor.modelos';

/**
 * Única pantalla del alumno (SDD §8): lista de clases y el chat. No hay formulario de
 * examen ni botones de alternativas; todo entra como texto de la conversación.
 */
@Component({
  selector: 'app-chat-tutor',
  imports: [FormsModule, DatePipe],
  templateUrl: './chat-tutor.page.html',
  styleUrl: './chat-tutor.page.css',
})
export class ChatTutorPage {
  private readonly api = inject(TutorStreamService);
  private readonly sesion = inject(SesionService);

  readonly clases = signal<ClaseResumen[]>([]);
  readonly agentes = signal<Agente[]>([]);
  readonly claseActiva = signal<ClaseResumen | null>(null);
  readonly agenteActivo = signal<string>('');
  readonly conversacionId = signal<string>('');
  readonly modo = signal<ModoChat>('Consulta');
  readonly ampliacionPermitida = signal(true);
  readonly progreso = signal<Progreso | null>(null);
  readonly mensajes = signal<Mensaje[]>([]);
  readonly enviando = signal(false);
  readonly error = signal<string>('');
  readonly borrador = signal('');

  readonly credenciales = signal<Credencial[]>([]);
  readonly conectando = signal(false);
  readonly errorCredencial = signal('');
  readonly panelCredenciales = signal(false);

  readonly usuarioId = this.sesion.usuarioId;
  readonly enExamen = computed(() => this.modo() === 'Evaluacion');

  /** Sin ningún agente conectado no hay tutor con quien hablar (RF-27). */
  readonly agentesConectados = computed(() => this.agentes().filter((a) => a.conectado));
  readonly sinAgente = computed(() => this.agentesConectados().length === 0);

  readonly etiquetaModo = computed(() => {
    switch (this.modo()) {
      case 'Evaluacion':
        return 'Examen en curso';
      case 'Revision':
        return 'Revisión de errores';
      default:
        return 'Consulta libre';
    }
  });

  async ngOnInit(): Promise<void> {
    if (!this.usuarioId()) return;
    await this.cargar();
  }

  async identificarse(usuarioId: string): Promise<void> {
    if (!usuarioId.trim()) return;
    this.sesion.establecer(usuarioId.trim(), 'Alumno');
    await this.cargar();
  }

  private async cargar(): Promise<void> {
    try {
      const [clases, agentes, credenciales] = await Promise.all([
        this.api.clases(),
        this.api.agentes(),
        this.api.credenciales(),
      ]);

      this.clases.set(clases);
      this.agentes.set(agentes);
      this.credenciales.set(credenciales);
      this.agenteActivo.set(this.agentesConectados()[0]?.id ?? agentes[0]?.id ?? '');

      // Sin credencial, lo primero es conectarla: el chat no sirve de nada aún.
      this.panelCredenciales.set(this.sinAgente());

      if (clases.length > 0 && !this.sinAgente()) await this.abrir(clases[0]);
    } catch {
      this.error.set('No se pudo cargar tus clases. ¿Está levantada la API?');
    }
  }

  alternarPanelCredenciales(): void {
    this.panelCredenciales.update((abierto) => !abierto || this.sinAgente());
  }

  credencialDe(agenteId: string): Credencial | undefined {
    return this.credenciales().find((c) => c.agenteId === agenteId);
  }

  async conectar(agenteId: string, clave: string, campo: HTMLInputElement): Promise<void> {
    if (!clave.trim() || this.conectando()) return;

    this.conectando.set(true);
    this.errorCredencial.set('');

    try {
      await this.api.conectarCredencial(agenteId, clave.trim());
      campo.value = '';
      await this.refrescarCredenciales();

      if (this.claseActiva() === null && this.clases().length > 0) await this.abrir(this.clases()[0]);
      this.panelCredenciales.set(false);
    } catch (e: any) {
      this.errorCredencial.set(e?.error?.mensaje ?? 'No se pudo conectar esa credencial.');
    } finally {
      this.conectando.set(false);
    }
  }

  async desconectar(agenteId: string): Promise<void> {
    await this.api.desconectarCredencial(agenteId);
    await this.refrescarCredenciales();
    this.panelCredenciales.set(this.sinAgente());
  }

  private async refrescarCredenciales(): Promise<void> {
    const [agentes, credenciales] = await Promise.all([this.api.agentes(), this.api.credenciales()]);
    this.agentes.set(agentes);
    this.credenciales.set(credenciales);

    if (!this.agentesConectados().some((a) => a.id === this.agenteActivo()))
      this.agenteActivo.set(this.agentesConectados()[0]?.id ?? '');
  }

  async abrir(clase: ClaseResumen): Promise<void> {
    this.claseActiva.set(clase);
    this.error.set('');
    this.progreso.set(null);

    const conversacion = await this.api.abrirConversacion(clase.claseId, this.agenteActivo());
    this.conversacionId.set(conversacion.conversacionId);
    this.modo.set(conversacion.modo);
    this.agenteActivo.set(conversacion.agenteId);

    const historial = await this.api.historial(conversacion.conversacionId);

    this.mensajes.set(
      historial.map((m) => {
        const { cuerpo, ampliacion } = separarAmpliacion(m.texto);
        return {
          rol: m.rol === 'Alumno' ? 'Alumno' : 'Agente',
          texto: cuerpo,
          ampliacion,
          fuentes: m.fuentes ? JSON.parse(m.fuentes) : undefined,
        } satisfies Mensaje;
      }),
    );
  }

  async cambiarAgente(agenteId: string): Promise<void> {
    if (this.enExamen()) {
      this.error.set('No puedes cambiar de agente durante el examen.');
      return;
    }

    await this.api.cambiarAgente(this.conversacionId(), agenteId);
    this.agenteActivo.set(agenteId);
  }

  async comenzarExamen(): Promise<void> {
    try {
      const inicio = await this.api.iniciarExamen(this.conversacionId());
      this.modo.set(inicio.modo);
      await this.enviar('Estoy listo para empezar el examen.');
    } catch {
      this.error.set('No se pudo iniciar el examen: revisa la ventana y tus intentos.');
    }
  }

  async enviarBorrador(): Promise<void> {
    const texto = this.borrador().trim();
    if (!texto) return;

    this.borrador.set('');
    await this.enviar(texto);
  }

  private async enviar(texto: string): Promise<void> {
    if (this.enviando() || !this.conversacionId()) return;

    this.enviando.set(true);
    this.error.set('');

    this.mensajes.update((ms) => [
      ...ms,
      { rol: 'Alumno', texto },
      { rol: 'Agente', texto: '', enCurso: true },
    ]);

    try {
      for await (const evento of this.api.enviar(
        this.conversacionId(),
        texto,
        this.sesion.cabeceras(),
      )) {
        this.aplicar(evento);
      }
    } catch {
      this.error.set('Se perdió la conexión con el tutor.');
    } finally {
      this.enviando.set(false);
      this.actualizarUltimo((m) => ({ ...m, enCurso: false }));
      this.repartirAmpliacion();
    }
  }

  private aplicar(evento: EventoTutor): void {
    switch (evento.tipo) {
      case 'modo':
        this.modo.set(evento.modo);
        this.ampliacionPermitida.set(evento.ampliacionPermitida);
        break;

      case 'token':
        this.actualizarUltimo((m) => ({ ...m, texto: m.texto + evento.texto }));
        break;

      case 'progreso':
        this.progreso.set(evento.progreso);
        break;

      case 'fuentes':
        this.actualizarUltimo((m) => ({ ...m, fuentes: evento.fuentes }));
        break;

      case 'aviso':
        this.actualizarUltimo((m) => ({ ...m, aviso: evento.mensaje }));
        // Credencial ausente o rechazada: hay que reconectarla antes de seguir.
        if (evento.mensaje.includes('credencial')) {
          this.panelCredenciales.set(true);
          void this.refrescarCredenciales();
        }
        break;

      case 'error':
        this.error.set(evento.mensaje);
        break;
    }
  }

  /** La ampliación se separa para pintarla en su propio recuadro, sin chip de fuente. */
  private repartirAmpliacion(): void {
    this.actualizarUltimo((m) => {
      const { cuerpo, ampliacion } = separarAmpliacion(m.texto);
      return { ...m, texto: cuerpo, ampliacion };
    });
  }

  private actualizarUltimo(cambio: (m: Mensaje) => Mensaje): void {
    this.mensajes.update((ms) => {
      if (ms.length === 0) return ms;

      const copia = [...ms];
      copia[copia.length - 1] = cambio(copia[copia.length - 1]);
      return copia;
    });
  }
}
