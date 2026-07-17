import { useCallback, useEffect, useState } from 'react'
import { useAuth0 } from '@auth0/auth0-react'
import { api, graphql } from '../api'

interface Device {
  id: number
  serialNumber: string
  locationCode: string
  model: string
  deviceType: string
  isActive: boolean
}

const DEVICE_TYPE_LABELS: Record<string, string> = {
  CARDIO: 'Cardio',
  STRENGTH: 'Strength',
  PREMIUM: 'Premium',
}

export default function DevicesPage() {
  const { getAccessTokenSilently } = useAuth0()
  const [devices, setDevices] = useState<Device[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)
  const [serialNumber, setSerialNumber] = useState('')
  const [locationCode, setLocationCode] = useState('')
  const [model, setModel] = useState('')
  const [deviceType, setDeviceType] = useState('0')
  const [installationYear, setInstallationYear] = useState(String(new Date().getFullYear()))

  const loadDevices = useCallback(async () => {
    try {
      const token = await getAccessTokenSilently()
      const data = await graphql(
        `query { devices(order: { serialNumber: ASC }) { id serialNumber locationCode model deviceType isActive } }`,
        token,
      )
      setDevices(data.devices)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Onbekende fout')
    }
  }, [getAccessTokenSilently])

  useEffect(() => {
    loadDevices()
  }, [loadDevices])

  async function createDevice() {
    if (!serialNumber.trim() || !locationCode.trim() || !model.trim()) return
    setBusy(true)
    setError(null)
    try {
      const token = await getAccessTokenSilently()
      await api.post('/devices', token, {
        serialNumber,
        locationCode,
        model,
        deviceType: Number(deviceType),
        installationYear: Number(installationYear),
        isActive: true,
      })
      setSerialNumber('')
      setLocationCode('')
      setModel('')
      await loadDevices()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Kon toestel niet aanmaken')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div>
      <div className="page-header">
        <h2>Toestellenpark</h2>
      </div>

      {error && (
        <div className="error-box">
          Kon toestellen niet ophalen — heb je de <code>manage:devices</code>-permission? ({error})
        </div>
      )}

      <div className="card" style={{ marginBottom: 20 }}>
        <h3 style={{ marginBottom: 12 }}>Nieuw toestel toevoegen</h3>
        <div className="form-row">
          <input placeholder="Serienummer" value={serialNumber} onChange={(e) => setSerialNumber(e.target.value)} />
          <input placeholder="Locatiecode (bv. Zaal A - Rij 3)" value={locationCode} onChange={(e) => setLocationCode(e.target.value)} />
        </div>
        <div className="form-row">
          <input placeholder="Model (bv. TechnoGym Skillrun)" value={model} onChange={(e) => setModel(e.target.value)} />
          <select value={deviceType} onChange={(e) => setDeviceType(e.target.value)}>
            <option value="0">Cardio</option>
            <option value="1">Strength</option>
            <option value="2">Premium</option>
          </select>
          <input
            type="number"
            placeholder="Bouwjaar"
            value={installationYear}
            onChange={(e) => setInstallationYear(e.target.value)}
            style={{ maxWidth: 120 }}
          />
        </div>
        <button className="btn-primary" disabled={busy} onClick={createDevice}>Toestel toevoegen</button>
      </div>

      {!devices && !error && <div className="empty-state">Laden...</div>}

      {devices && devices.length === 0 && (
        <div className="empty-state">Nog geen toestellen geregistreerd.</div>
      )}

      {devices && devices.length > 0 && (
        <div className="card-grid">
          {devices.map((device) => (
            <div key={device.id} className="card device-card">
              <h3>{device.serialNumber || `Toestel #${device.id}`}</h3>
              <div className="meta">{device.model} · {device.locationCode}</div>
              <span className="badge badge-neutral">{DEVICE_TYPE_LABELS[device.deviceType] ?? device.deviceType}</span>{' '}
              <span className={`badge ${device.isActive ? 'badge-success' : 'badge-danger'}`}>
                {device.isActive ? 'Actief' : 'Buiten dienst'}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
