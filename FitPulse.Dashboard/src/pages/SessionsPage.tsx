import { useCallback, useEffect, useState } from 'react'
import { useAuth0 } from '@auth0/auth0-react'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts'
import { api, graphql } from '../api'

interface Session {
  id: number
  memberId: number
  deviceId: number
  deviceSerialNumber: string
  status: string
  requestedAt: string
  startedAt: string | null
  completedAt: string | null
  durationMinutes: number | null
  caloriesBurned: number | null
  paymentAmount: number | null
}

interface TelemetryPoint {
  powerWatts: number
  heartRate: number
  timestamp: string
}

const STATUS_LABELS: Record<string, { label: string; badge: string }> = {
  REQUESTED: { label: 'Aangevraagd', badge: 'badge-neutral' },
  IN_PROGRESS: { label: 'Bezig', badge: 'badge-warning' },
  INPROGRESS: { label: 'Bezig', badge: 'badge-warning' },
  COMPLETED: { label: 'Voltooid', badge: 'badge-success' },
  CANCELLED: { label: 'Geannuleerd', badge: 'badge-danger' },
  ABORTED: { label: 'Afgebroken', badge: 'badge-danger' },
}

export default function SessionsPage() {
  const { getAccessTokenSilently } = useAuth0()
  const [sessions, setSessions] = useState<Session[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [deviceType, setDeviceType] = useState('0')
  const [busy, setBusy] = useState(false)
  const [selectedSessionId, setSelectedSessionId] = useState<number | null>(null)
  const [telemetry, setTelemetry] = useState<TelemetryPoint[] | null>(null)

  const loadSessions = useCallback(async () => {
    try {
      const token = await getAccessTokenSilently()
      const data = await api.get('/trainingsessions', token)
      setSessions(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Onbekende fout')
    }
  }, [getAccessTokenSilently])

  useEffect(() => {
    loadSessions()
  }, [loadSessions])

  async function requestSession() {
    setBusy(true)
    setError(null)
    try {
      const token = await getAccessTokenSilently()
      await api.post('/trainingsessions', token, { deviceType: Number(deviceType) })
      await loadSessions()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Kon sessie niet aanvragen')
    } finally {
      setBusy(false)
    }
  }

  async function startSession(id: number) {
    setBusy(true)
    setError(null)
    try {
      const token = await getAccessTokenSilently()
      await api.put(`/trainingsessions/${id}/start`, token)
      await loadSessions()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Kon sessie niet starten')
    } finally {
      setBusy(false)
    }
  }

  async function completeSession(id: number) {
    const input = window.prompt('Hoeveel kcal verbrand tijdens deze sessie?', '200')
    if (input === null) return
    const caloriesBurned = Number(input)
    if (!Number.isFinite(caloriesBurned) || caloriesBurned < 0) return

    setBusy(true)
    setError(null)
    try {
      const token = await getAccessTokenSilently()
      await api.put(`/trainingsessions/${id}/complete`, token, { caloriesBurned })
      await loadSessions()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Kon sessie niet voltooien')
    } finally {
      setBusy(false)
    }
  }

  async function viewTelemetry(id: number) {
    setSelectedSessionId(id)
    setTelemetry(null)
    try {
      const token = await getAccessTokenSilently()
      const data = await graphql(
        `query { trainingSessions(where: { id: { eq: ${id} } }) { id telemetryReadings { powerWatts heartRate timestamp } } }`,
        token,
      )
      setTelemetry(data.trainingSessions[0]?.telemetryReadings ?? [])
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Kon telemetrie niet ophalen')
    }
  }

  return (
    <div>
      <div className="page-header">
        <h2>Mijn Trainingssessies</h2>
      </div>

      {error && <div className="error-box">{error}</div>}

      <div className="card" style={{ marginBottom: 20 }}>
        <div className="form-row">
          <select value={deviceType} onChange={(e) => setDeviceType(e.target.value)}>
            <option value="0">Cardio</option>
            <option value="1">Strength</option>
            <option value="2">Premium</option>
          </select>
          <button className="btn-primary" disabled={busy} onClick={requestSession}>
            Sessie aanvragen
          </button>
        </div>
      </div>

      {!sessions && <div className="empty-state">Laden...</div>}
      {sessions && sessions.length === 0 && <div className="empty-state">Nog geen sessies.</div>}

      {sessions && sessions.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>#</th>
              <th>Toestel</th>
              <th>Status</th>
              <th>Aangevraagd</th>
              <th>Prijs</th>
              <th>Actie</th>
            </tr>
          </thead>
          <tbody>
            {sessions.map((session) => {
              const statusInfo = STATUS_LABELS[session.status] ?? { label: session.status, badge: 'badge-neutral' }
              return (
                <tr key={session.id}>
                  <td>{session.id}</td>
                  <td>{session.deviceSerialNumber}</td>
                  <td><span className={`badge ${statusInfo.badge}`}>{statusInfo.label}</span></td>
                  <td>{new Date(session.requestedAt).toLocaleString('nl-BE')}</td>
                  <td>{session.paymentAmount != null ? `€ ${session.paymentAmount.toFixed(2)}` : '—'}</td>
                  <td>
                    <div className="session-actions">
                      {session.status === 'REQUESTED' && (
                        <button className="btn-secondary" disabled={busy} onClick={() => startSession(session.id)}>Start</button>
                      )}
                      {session.status === 'IN_PROGRESS' || session.status === 'INPROGRESS' ? (
                        <button className="btn-secondary" disabled={busy} onClick={() => completeSession(session.id)}>Voltooi</button>
                      ) : null}
                      <button className="btn-secondary" onClick={() => viewTelemetry(session.id)}>Telemetrie</button>
                    </div>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      )}

      {selectedSessionId !== null && (
        <div className="card chart-card">
          <h3>Telemetrie — sessie #{selectedSessionId}</h3>
          {telemetry === null && <div className="empty-state">Laden...</div>}
          {telemetry !== null && telemetry.length === 0 && (
            <div className="empty-state">Geen telemetrie ontvangen voor deze sessie.</div>
          )}
          {telemetry !== null && telemetry.length > 0 && (
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={telemetry.map((t) => ({ ...t, time: new Date(t.timestamp).toLocaleTimeString('nl-BE') }))}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="time" fontSize={12} />
                <YAxis fontSize={12} />
                <Tooltip />
                <Legend />
                <Line type="monotone" dataKey="powerWatts" name="Vermogen (W)" stroke="#7c3aed" strokeWidth={2} dot={false} />
                <Line type="monotone" dataKey="heartRate" name="Hartslag (bpm)" stroke="#dc2626" strokeWidth={2} dot={false} />
              </LineChart>
            </ResponsiveContainer>
          )}
        </div>
      )}
    </div>
  )
}
