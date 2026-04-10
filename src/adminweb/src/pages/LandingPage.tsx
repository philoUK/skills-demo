import { useAuth } from 'react-oidc-context'

export function LandingPage() {
  const auth = useAuth()

  return (
    <div>
      <p>landing page</p>
      <button onClick={() => auth.signoutRedirect()}>Sign out</button>
    </div>
  )
}
