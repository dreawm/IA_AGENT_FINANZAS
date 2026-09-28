import { Injectable, signal } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';

const CLAVE_USUARIO = 'tutor.usuarioId';
const CLAVE_ROL = 'tutor.rol';

/**
 * Identidad del usuario. En producción la pone el login OIDC y el token viaja en el
 * Authorization; en desarrollo se usan las cabeceras de prueba de la API.
 */
@Injectable({ providedIn: 'root' })
export class SesionService {
  readonly usuarioId = signal(localStorage.getItem(CLAVE_USUARIO) ?? '');
  readonly rol = signal(localStorage.getItem(CLAVE_ROL) ?? 'Alumno');

  establecer(usuarioId: string, rol: string): void {
    this.usuarioId.set(usuarioId);
    this.rol.set(rol);
    localStorage.setItem(CLAVE_USUARIO, usuarioId);
    localStorage.setItem(CLAVE_ROL, rol);
  }

  cabeceras(): Record<string, string> {
    const id = this.usuarioId();
    return id ? { 'X-Usuario-Id': id, 'X-Usuario-Rol': this.rol() } : {};
  }
}

export const interceptorSesion: HttpInterceptorFn = (peticion, siguiente) => {
  const usuarioId = localStorage.getItem(CLAVE_USUARIO);
  if (!usuarioId) return siguiente(peticion);

  return siguiente(
    peticion.clone({
      setHeaders: {
        'X-Usuario-Id': usuarioId,
        'X-Usuario-Rol': localStorage.getItem(CLAVE_ROL) ?? 'Alumno',
      },
    }),
  );
};
