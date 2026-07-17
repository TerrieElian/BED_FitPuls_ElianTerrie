import { useEffect, useState } from 'react'
import { useAuth0 } from '@auth0/auth0-react'
import { graphql } from '../api'

interface SensorDiagnostic {
  sensorType: string
  errorCode: string
  severity: string
  timestamp: string
  rawSensorDataJson: string
}

interface DeviceWithDiagnostics {
  id: number
  serialNumber: string
  sensorDiagnostics: SensorDiagnostic[]
}

const SEVERITY_BADGES: Record<string, string> = {
  Warning: 'badge-warning',
  Critical: 'badge-danger',
}

export default function SensorDiagnosticsPage() {
  const { getAccessTokenSilently } = useAuth0()
  const [devices, setDevices] = useState<DeviceWithDiagnostics[] | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false

    async function load() {
      try {
        const token = await getAccessTokenSilently()
        const data = await graphql(
          `query { devices(order: { serialNumber: ASC }) { id serialNumber sensorDiagnostics { sensorType errorCode severity timestamp rawSensorDataJson } } }`,
          token,
        )
        if (!cancelled) setDevices(data.devices)
      } catch (err) {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Onbekende fout')
      }
    }

    load()
    return () => {
      cancelled = true
    }
  }, [getAccessTokenSilently])

  const rows = devices?.flatMap((device) =>
    device.sensorDiagnostics.map((d) => ({ ...d, deviceLabel: device.serialNumber || `Toestel #${device.id}` })),
  ) ?? []
  rows.sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime())

  return (
    <div>
      <div className="page-header">
        <h2>Sensor Diagnostiek</h2>
      </div>

      {error && <div className="error-box">{error}</div>}

      {!devices && !error && <div className="empty-state">Laden...</div>}

      {devices && rows.length === 0 && (
        <div className="empty-state">Nog geen sensordiagnostiek ontvangen.</div>
      )}

      {rows.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>Toestel</th>
              <th>Sensor</th>
              <th>Foutcode</th>
              <th>Ernst</th>
              <th>Tijdstip</th>
              <th>Ruwe data</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((d, i) => (
              <tr key={i}>
                <td>{d.deviceLabel}</td>
                <td>{d.sensorType}</td>
                <td>{d.errorCode}</td>
                <td><span className={`badge ${SEVERITY_BADGES[d.severity] ?? 'badge-neutral'}`}>{d.severity}</span></td>
                <td>{new Date(d.timestamp).toLocaleString('nl-BE')}</td>
                <td><code>{d.rawSensorDataJson}</code></td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
