import { Component, inject, signal } from '@angular/core';
import { AccesoService, Profesor } from '../datos/acceso.service';
import { SesionService } from '../datos/sesion.service';
import { ElegirProfesor } from './elegir-profesor';

/**
 * El alumno elige o cambia a su profesor (RF-33). Al cambiarlo deja de ver los cursos del
 * anterior; sus intentos y conversaciones se conservan por si vuelve.
 */
@Component({
  selector: 'app-profesor',
  imports: [ElegirProfesor],
  template: `
    <header class="cabecera">
      <div>
        <h1>Tu profesor</h1>
        <p class="quien">{{ sesion.usuario()?.nombre }}</p>
      </div>
      <button class="enlace" (click)="sesion.salir()">Salir</button>
    </header>

    <main class="contenido">
      <p class="bajada">Verás solo los cursos del profesor que elijas.</p>

      @if (error()) {
        <p class="error" role="alert">{{ error() }}</p>
      }

      <form (submit)="$event.preventDefault(); guardar()">
        <app-elegir-profesor [profesores]="profesores()" [(elegido)]="elegido" />
        <div class="acciones">
          <button type="submit" class="principal" [disabled]="!elegido() || guardando()">Ver sus cursos</button>
          @if (actual()) {
            <button type="button" class="enlace" (click)="sesion.eligiendoProfesor.set(false)">Volver sin cambiar</button>
          }
        </div>
      </form>
    </main>
  `,
  styles: `
    :host {
      display: block;
      min-height: 100vh;
      min-height: 100dvh;
    }
    .quien {
      margin: 0.15rem 0 0;
      font-size: var(--t-pequeno);
      color: var(--upc-suave);
    }
    .contenido {
      max-width: 34rem;
      padding: 2rem 1.5rem 3rem;
    }
    .bajada {
      margin: 0 0 1.5rem;
      font-family: var(--serif);
      font-size: var(--t-lectura);
      color: var(--grafito);
    }
    .acciones {
      display: flex;
      align-items: center;
      gap: 1.25rem;
    }
  `,
})
export class ProfesorPage {
  private readonly acceso = inject(AccesoService);
  readonly sesion = inject(SesionService);

  readonly profesores = signal<Profesor[]>([]);
  readonly elegido = signal('');
  readonly actual = signal('');
  readonly guardando = signal(false);
  readonly error = signal('');

  async ngOnInit(): Promise<void> {
    try {
      const [profesores, { profesor }] = await Promise.all([this.acceso.profesores(), this.acceso.miProfesor()]);
      this.profesores.set(profesores);
      this.actual.set(profesor?.id ?? '');
      this.elegido.set(profesor?.id ?? '');
    } catch {
      this.error.set('No se pudo cargar la lista de profesores.');
    }
  }

  async guardar(): Promise<void> {
    this.guardando.set(true);
    this.error.set('');
    try {
      await this.acceso.elegirProfesor(this.elegido());
      this.sesion.eligiendoProfesor.set(false);
    } catch (e: any) {
      this.error.set(e?.error?.mensaje ?? 'No se pudo guardar.');
    } finally {
      this.guardando.set(false);
    }
  }
}
