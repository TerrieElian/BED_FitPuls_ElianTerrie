import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, useNavigate } from 'react-router-dom'
import { Auth0Provider } from '@auth0/auth0-react'
import './index.css'
import App from './App.tsx'
import { AUTH0_DOMAIN, AUTH0_CLIENT_ID, AUTH0_AUDIENCE } from './config'

function Auth0ProviderWithRedirect({ children }: { children: React.ReactNode }) {
  const navigate = useNavigate()

  return (
    <Auth0Provider
      domain={AUTH0_DOMAIN}
      clientId={AUTH0_CLIENT_ID}
      authorizationParams={{
        redirect_uri: `${window.location.origin}/callback`,
        audience: AUTH0_AUDIENCE,
      }}
      onRedirectCallback={(appState) => {
        navigate(appState?.returnTo || '/')
      }}
    >
      {children}
    </Auth0Provider>
  )
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <Auth0ProviderWithRedirect>
        <App />
      </Auth0ProviderWithRedirect>
    </BrowserRouter>
  </StrictMode>,
)
