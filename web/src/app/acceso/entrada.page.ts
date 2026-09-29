import { Component, inject, signal } from '@angular/core';
import { AccesoService, UsuarioAdmin } from '../datos/acceso.service';
import { SesionService } from '../datos/sesion.service';

/**
 * Página de entrada de todos los roles (RF-31). El usuario elige con qué cuenta de la
 * universidad entra; al volver del proveedor (/entrar/{proveedor}?code=…) se canjea el
 * código por la sesión y cada rol va a su pantalla.
 */
@Component({
  selector: 'app-entrada',
  templateUrl: './entrada.page.html',
  styleUrl: './entrada.page.css',
})
export class EntradaPage {
  private readonly acceso = inject(AccesoService);
  private readonly sesion = inject(SesionService);

  readonly proveedores = signal<{ id: string; nombre: string }[]>([]);
  readonly desarrollo = signal(false);
  readonly deprueba = signal<UsuarioAdmin[]>([]);
  readonly ocupado = signal(false);
  readonly error = signal('');

  async ngOnInit(): Promise<void> {
    if (await this.completarRetorno()) return;

    try {
      const { proveedores, desarrollo } = await this.acceso.proveedores();
      this.proveedores.set(proveedores);
      this.desarrollo.set(desarrollo);
      if (desarrollo) this.deprueba.set(await this.acceso.usuariosDePrueba());
    } catch {
      this.error.set('No se pudo conectar con la plataforma. Inténtalo en un momento.');
    }
  }

  async entrarCon(proveedor: string): Promise<void> {
    this.ocupado.set(true);
    this.error.set('');

    try {
      const { url } = await this.acceso.iniciar(proveedor);
      window.location.assign(url);
    } catch (e: any) {
      this.error.set(e?.error?.mensaje ?? 'No se pudo iniciar sesión.');
      this.ocupado.set(false);
    }
  }

  async entrarDePrueba(usuarioId: string): Promise<void> {
    this.ocupado.set(true);
    try {
      this.sesion.establecer(await this.acceso.entrarDePrueba(usuarioId));
    } finally {
      this.ocupado.set(false);
    }
  }

  /** El proveedor vuelve con ?code=…&state=…: se canjea y se limpia la URL. */
  private async completarRetorno(): Promise<boolean> {
    const ruta = /^\/entrar\/([\w-]+)\/?$/.exec(location.pathname);
    if (!ruta) return false;

    const parametros = new URLSearchParams(location.search);
    const code = parametros.get('code');
    const state = parametros.get('state');
    history.replaceState(null, '', '/');

    if (!code || !state) {
      this.error.set('No se completó el inicio de sesión. Vuelve a intentarlo.');
      void this.ngOnInit();
      return true;
    }

    this.ocupado.set(true);
    try {
      this.sesion.establecer(await this.acceso.canjear(ruta[1], code, state));
    } catch (e: any) {
      this.error.set(e?.error?.mensaje ?? 'No se pudo iniciar sesión.');
      this.ocupado.set(false);
      void this.ngOnInit();
    }

    return true;
  }
}
