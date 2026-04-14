import { AuthProvider } from 'react-oidc-context'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { oidcConfig } from './auth/auth-config'
import { ProtectedRoute } from './auth/ProtectedRoute'
import { CallbackPage } from './pages/CallbackPage'
import { LandingPage } from './pages/LandingPage'
import { AdministratorsPage } from './pages/AdministratorsPage'
import { RegisterCompletePage } from './pages/RegisterCompletePage'
import { RegisterExpiredPage } from './pages/RegisterExpiredPage'
import { RegisterAlreadyUsedPage } from './pages/RegisterAlreadyUsedPage'
import { RegisterNotFoundPage } from './pages/RegisterNotFoundPage'

function App() {
  return (
    <AuthProvider {...oidcConfig}>
      <BrowserRouter>
        <Routes>
          <Route path="/callback" element={<CallbackPage />} />
          <Route path="/register/complete" element={<RegisterCompletePage />} />
          <Route path="/register/expired" element={<RegisterExpiredPage />} />
          <Route path="/register/already-used" element={<RegisterAlreadyUsedPage />} />
          <Route path="/register/not-found" element={<RegisterNotFoundPage />} />
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
