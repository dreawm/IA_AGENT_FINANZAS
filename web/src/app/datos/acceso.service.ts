import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { Rol, Sesion, UsuarioSesion } from './sesion.service';

const API = '/api/v1';

export interface Proveedores {
  proveedores: { id: string; nombre: string }[];
  desarrollo: boolean;
}

export interface UsuarioAdmin {
  id: string;
  email: string;
  nombre: string;
  rol: Rol;
  cursos: string[];
}

export interface CursoAdmin {
  id: string;
  codigo: string;
  nombre: string;
}

/** Inicio de sesión (RF-31) y administración de usuarios (RF-33). */
@Injectable({ providedIn: 'root' })
export class AccesoService {
  private readonly http = inject(HttpClient);

  proveedores(): Promise<Proveedores> {
    return firstValueFrom(this.http.get<Proveedores>(`${API}/acceso/proveedores`));
  }

  iniciar(proveedor: string): Promise<{ url: string }> {
    return firstValueFrom(this.http.post<{ url: string }>(`${API}/acceso/${proveedor}/inicio`, {}));
  }

  canjear(proveedor: string, code: string, state: string): Promise<Sesion> {
    return firstValueFrom(this.http.post<Sesion>(`${API}/acceso/${proveedor}/canje`, { code, state }));
  }

  yo(): Promise<UsuarioSesion> {
    return firstValueFrom(this.http.get<UsuarioSesion>(`${API}/acceso/yo`));
  }

  usuariosDePrueba(): Promise<UsuarioAdmin[]> {
    return firstValueFrom(this.http.get<UsuarioAdmin[]>(`${API}/acceso/desarrollo/usuarios`));
  }

  entrarDePrueba(usuarioId: string): Promise<Sesion> {
    return firstValueFrom(this.http.post<Sesion>(`${API}/acceso/desarrollo`, { usuarioId }));
  }

  usuarios(): Promise<UsuarioAdmin[]> {
    return firstValueFrom(this.http.get<UsuarioAdmin[]>(`${API}/admin/usuarios`));
  }

  cursos(): Promise<CursoAdmin[]> {
    return firstValueFrom(this.http.get<CursoAdmin[]>(`${API}/admin/cursos`));
  }

  guardarUsuario(u: { email: string; nombre: string; rol: Rol; cursos: string[] }) {
    return firstValueFrom(this.http.put(`${API}/admin/usuarios`, u));
  }

  guardarLote(usuarios: { email: string; nombre: string; rol: Rol; cursos: string[] }[]) {
    return firstValueFrom(this.http.post<{ guardados: number }>(`${API}/admin/usuarios/lote`, { usuarios }));
  }
}
