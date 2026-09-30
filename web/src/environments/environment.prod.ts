// Lo reescribe scripts/set-env.mjs en cada despliegue con API_URL (npm run build:deploy).
// El valor versionado solo sirve para compilar en local detrás de un proxy.
export const environment = {
  production: true,
  apiUrl: '/api',
};
