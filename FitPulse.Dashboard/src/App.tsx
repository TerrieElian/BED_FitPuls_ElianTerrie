import { useEffect, useState } from 'react'
import { Routes, Route, NavLink, Navigate } from 'react-router-dom'
import { useAuth0 } from '@auth0/auth0-react'
import { api, decodePermissions } from './api'
import DevicesPage from './pages/DevicesPage'
import SessionsPage from './pages/SessionsPage'
import TicketsPage from './pages/TicketsPage'
import SensorDiagnosticsPage from './pages/SensorDiagnosticsPage'
import './App.css'

function App() {
  const { isAuthenticated, isLoading, loginWithRedirect, logout, user, getAccessTokenSilently } = useAuth0()
  const [isAdmin, setIsAdmin] = useState<boolean | null>(null)

  useEffect(() => {
    if (!isAuthenticated) return
    getAccessTokenSilently()
      .then((token) => setIsAdmin(decodePermissions(token).includes('manage:devices')))
      .catch(() => setIsAdmin(false))
  }, [isAuthenticated, getAccessTokenSilently])

  useEffect(() => {
    if (isAdmin !== false || !user?.email) return
    getAccessTokenSilently()
      .then((token) => api.put('/members/me', token, { email: user.email }))
      .catch(() => {})
  }, [isAdmin, user?.email, getAccessTokenSilently])

  if (isLoading) {
    return <div className="center-screen">Laden...</div>
  }

  if (!isAuthenticated) {
    return (
      <div className="center-screen">
        <h1>FitPulse Dashboard</h1>
        <p>Log in om verder te gaan.</p>
        <button className="btn-primary" onClick={() => loginWithRedirect()}>
          Inloggen
        </button>
      </div>
    )
  }

  return (
    <div className="app-shell">
      <header className="app-header">
        <h1>FitPulse</h1>
        <nav>
          {isAdmin === true && <NavLink to="/" end>Toestellen</NavLink>}
          <NavLink to="/sessions">Sessies</NavLink>
          <NavLink to="/tickets">Support Tickets</NavLink>
          {isAdmin === true && <NavLink to="/diagnostics">Sensor Diagnostiek</NavLink>}
        </nav>
        <div className="user-info">
          <span>{user?.name ?? user?.email}</span>
          <button className="btn-secondary" onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}>
            Uitloggen
          </button>
        </div>
      </header>

      <main className="app-content">
        <Routes>
          <Route
            path="/"
            element={
              isAdmin === null
                ? <div className="center-screen">Laden...</div>
                : isAdmin
                  ? <DevicesPage />
                  : <Navigate to="/sessions" replace />
            }
          />
          <Route path="/sessions" element={<SessionsPage />} />
          <Route path="/tickets" element={<TicketsPage />} />
          <Route
            path="/diagnostics"
            element={
              isAdmin === null
                ? <div className="center-screen">Laden...</div>
                : isAdmin
                  ? <SensorDiagnosticsPage />
                  : <Navigate to="/sessions" replace />
            }
          />
          <Route path="/callback" element={<div className="center-screen">Bezig met inloggen...</div>} />
        </Routes>
      </main>
    </div>
  )
}

export default App
