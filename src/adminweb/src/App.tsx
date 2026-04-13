import { AuthProvider } from 'react-oidc-context'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { oidcConfig } from './auth/auth-config'
import { ProtectedRoute } from './auth/ProtectedRoute'
import { CallbackPage } from './pages/CallbackPage'
import { LandingPage } from './pages/LandingPage'
import { AdministratorsPage } from './pages/AdministratorsPage'

function App() {
  return (
    <AuthProvider {...oidcConfig}>
      <BrowserRouter>
        <Routes>
          <Route path="/callback" element={<CallbackPage />} />
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <LandingPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/administrators"
            element={
              <ProtectedRoute>
                <AdministratorsPage />
              </ProtectedRoute>
            }
          />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}

export default App
