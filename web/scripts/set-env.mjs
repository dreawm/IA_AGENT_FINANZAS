/**
 * Escribe `src/environments/environment.prod.ts` a partir de API_URL.
 *
 * Angular fija su configuración al compilar, y la URL de la API cambia según el
 * despliegue (producción y desarrollo son el mismo código contra APIs distintas).
 * Generar el archivo justo antes de `ng build` permite que un commit sirva para los
 * dos, con el valor tomado de las variables de Vercel y no escrito en el repositorio.
 *
 * Se ejecuta con `npm run build:deploy`; `npm run build` usa el archivo versionado.
 */
import { writeFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const destino = resolve(
  dirname(fileURLToPath(import.meta.url)),
  '../src/environments/environment.prod.ts',
);

const apiUrl = process.env['API_URL']?.trim();

// Fallar aquí es el objetivo: sin la variable saldría una web que parece correcta y
// responde 404 a todo, que cuesta mucho más de diagnosticar que un despliegue en rojo.
if (!apiUrl) {
  console.error(
    '\n✗ Falta API_URL.\n' +
      '  Pon la URL pública de la API terminada en /api — por ejemplo\n' +
      '  https://tutor-api-production.up.railway.app/api — en las variables de Vercel.\n',
  );
  process.exit(1);
}

if (!/^https?:\/\/.+/.test(apiUrl) && !apiUrl.startsWith('/')) {
  console.error(`\n✗ API_URL debe ser una URL http(s) absoluta o una ruta desde la raíz; llegó "${apiUrl}".\n`);
  process.exit(1);
}

// Una barra final daría "…/api//v1" al unirla con las rutas.
const normalizada = apiUrl.replace(/\/+$/, '');

if (!normalizada.endsWith('/api')) {
  console.warn(`⚠ API_URL no termina en /api ("${normalizada}"): todas las peticiones darán 404.`);
}

writeFileSync(
  destino,
  `// Generado por scripts/set-env.mjs al compilar: no editar a mano.\n` +
    `// El valor viene de la variable API_URL del despliegue.\n` +
    `export const environment = {\n` +
    `  production: true,\n` +
    `  apiUrl: '${normalizada}',\n` +
    `};\n`,
);

console.log(`✓ environment.prod.ts escrito con apiUrl='${normalizada}'`);
