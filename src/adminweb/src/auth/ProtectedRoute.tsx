import { type ReactNode, useEffect, useState } from 'react'
import { useAuth } from 'react-oidc-context'
import { useNavigate } from 'react-router-dom'
import { ping } from '../api/ping'
import { InactiveAccountError } from '../api/errors'

export function ProtectedRoute({ children }: { children: ReactNode }) {
  const auth = useAuth()
  const navigate = useNavigate()
  const [checked, setChecked] = useState(false)

  useEffect(() => {
    if (!auth.isLoading && !auth.isAuthenticated) {
      auth.signinRedirect()
    }
  }, [auth.isLoading, auth.isAuthenticated, auth])

  useEffect(() => {
    if (!auth.isAuthenticated || !auth.user?.access_token) return
    ping(auth.user.access_token)
      .then(() => setChecked(true))
      .catch((e) => {
        if (e instanceof InactiveAccountError) {
          navigate('/inactive', { replace: true })
        } else {
          setChecked(true)
        }
      })
  }, [auth.isAuthenticated, auth.user?.access_token, navigate])

  if (auth.isLoading || !auth.isAuthenticated || !checked) return null

  return <>{children}</>
}
