import type { AuthProviderProps } from 'react-oidc-context'

const keycloakUrl = import.meta.env.VITE_KEYCLOAK_URL as string
const keycloakRealm = import.meta.env.VITE_KEYCLOAK_REALM as string

export const oidcConfig: AuthProviderProps = {
  authority: `${keycloakUrl}/realms/${keycloakRealm}`,
  client_id: 'adminweb',
  redirect_uri: `${window.location.origin}/callback`,
  onSigninCallback: () => {
    window.history.replaceState({}, document.title, '/')
  },
}
