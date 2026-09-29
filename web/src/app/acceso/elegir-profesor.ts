import { Component, input, model } from '@angular/core';
import { Profesor } from '../datos/acceso.service';

/**
 * Lista de profesores entre los que elige el alumno (RF-33): verá solo los cursos del que
 * elija. Cada opción muestra sus cursos para que lo reconozca.
 */
@Component({
  selector: 'app-elegir-profesor',
  template: `
    <fieldset class="profesores">
      <legend>Elige a tu profesor</legend>
      @for (p of profesores(); track p.id) {
        <label class="opcion" [class.elegida]="p.id === elegido()">
          <input type="radio" name="profesor" [value]="p.id" [checked]="p.id === elegido()" (change)="elegido.set(p.id)" />
          <span class="nombre">{{ p.nombre }}</span>
          <span class="cursos">{{ p.cursos.length ? p.cursos.join(', ') : 'Todavía sin cursos' }}</span>
        </label>
      } @empty {
        <p class="vacio">Todavía no hay profesores en la plataforma. Vuelve cuando tu profesor haya entrado.</p>
      }
    </fieldset>
  `,
  styles: `
    .profesores {
      display: grid;
      gap: 0.5rem;
      margin: 0 0 1.25rem;
      padding: 0;
      border: 0;
    }
    legend {
      margin-bottom: 0.5rem;
      font-weight: 600;
    }
    .opcion {
      display: grid;
      grid-template-columns: auto 1fr;
      column-gap: 0.75rem;
      padding: 0.7rem 0.9rem;
      border: 1px solid var(--pauta);
      border-radius: 6px;
      cursor: pointer;
    }
    .opcion.elegida {
      border-color: var(--tapa);
      box-shadow: inset 3px 0 0 var(--tapa);
    }
    .opcion input {
      grid-row: span 2;
      align-self: center;
      accent-color: var(--tapa);
    }
    .nombre {
      font-weight: 600;
    }
    .cursos,
    .vacio {
      font-size: var(--t-pequeno);
      color: var(--grafito);
    }
  `,
})
export class ElegirProfesor {
  readonly profesores = input.required<Profesor[]>();
  readonly elegido = model<string>('');
}
