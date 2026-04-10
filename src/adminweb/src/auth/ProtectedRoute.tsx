import { type ReactNode, useEffect } from 'react'
import { useAuth } from 'react-oidc-context'

export function ProtectedRoute({ children }: { children: ReactNode }) {
  const auth = useAuth()

  useEffect(() => {
    if (!auth.isLoading && !auth.isAuthenticated) {
      auth.signinRedirect()
    }
  }, [auth.isLoading, auth.isAuthenticated, auth])

  if (auth.isLoading || !auth.isAuthenticated) return null

  return <>{children}</>
}
