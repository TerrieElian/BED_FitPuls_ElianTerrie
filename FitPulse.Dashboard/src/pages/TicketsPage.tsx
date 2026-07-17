import { useCallback, useEffect, useState } from 'react'
import { useAuth0 } from '@auth0/auth0-react'
import { api, decodePermissions } from '../api'

interface Ticket {
  id: number
  memberId: number
  subject: string
  description: string
  priority: string
  status: string
  createdAt: string
}

const PRIORITY_LABELS: Record<string, { label: string; badge: string }> = {
  LOW: { label: 'Laag', badge: 'badge-neutral' },
  MEDIUM: { label: 'Medium', badge: 'badge-warning' },
  HIGH: { label: 'Hoog', badge: 'badge-danger' },
  CRITICAL: { label: 'Kritiek', badge: 'badge-danger' },
}

const STATUS_LABELS: Record<string, { label: string; badge: string }> = {
  OPEN: { label: 'Open', badge: 'badge-neutral' },
  IN_PROGRESS: { label: 'Bezig', badge: 'badge-warning' },
  INPROGRESS: { label: 'Bezig', badge: 'badge-warning' },
  RESOLVED: { label: 'Opgelost', badge: 'badge-success' },
  CLOSED: { label: 'Gesloten', badge: 'badge-success' },
}

export default function TicketsPage() {
  const { getAccessTokenSilently } = useAuth0()
  const [isAdmin, setIsAdmin] = useState<boolean | null>(null)
  const [myTickets, setMyTickets] = useState<Ticket[] | null>(null)
  const [allTickets, setAllTickets] = useState<Ticket[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [subject, setSubject] = useState('')
  const [description, setDescription] = useState('')
  const [priority, setPriority] = useState('1')
  const [busy, setBusy] = useState(false)

  const loadTickets = useCallback(async () => {
    try {
      const token = await getAccessTokenSilently()
      const admin = decodePermissions(token).includes('manage:tickets')
      setIsAdmin(admin)

      if (admin) {
        setAllTickets(await api.get('/supporttickets', token))
      } else {
        setMyTickets(await api.get('/supporttickets/mine', token))
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Onbekende fout')
    }
  }, [getAccessTokenSilently])

  useEffect(() => {
    loadTickets()
  }, [loadTickets])

  async function createTicket() {
    if (!subject.trim() || !description.trim()) return
    setBusy(true)
    setError(null)
    try {
      const token = await getAccessTokenSilently()
      await api.post('/supporttickets', token, { subject, description, priority: Number(priority) })
      setSubject('')
      setDescription('')
      await loadTickets()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Kon ticket niet aanmaken')
    } finally {
      setBusy(false)
    }
  }

  async function updateStatus(id: number, status: number) {
    setBusy(true)
    setError(null)
    try {
      const token = await getAccessTokenSilently()
      await api.put(`/supporttickets/${id}/status`, token, { status })
      await loadTickets()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Kon status niet wijzigen')
    } finally {
      setBusy(false)
    }
  }

  function renderTicketRows(tickets: Ticket[], showActions: boolean) {
    return tickets.map((ticket) => {
      const priorityInfo = PRIORITY_LABELS[ticket.priority] ?? { label: ticket.priority, badge: 'badge-neutral' }
      const statusInfo = STATUS_LABELS[ticket.status] ?? { label: ticket.status, badge: 'badge-neutral' }
      return (
        <tr key={ticket.id}>
          <td>{ticket.id}</td>
          <td>{ticket.subject}</td>
          <td><span className={`badge ${priorityInfo.badge}`}>{priorityInfo.label}</span></td>
          <td><span className={`badge ${statusInfo.badge}`}>{statusInfo.label}</span></td>
          <td>{new Date(ticket.createdAt).toLocaleDateString('nl-BE')}</td>
          {showActions && (
            <td>
              <div className="session-actions">
                <button className="btn-secondary" disabled={busy} onClick={() => updateStatus(ticket.id, 1)}>In behandeling</button>
                <button className="btn-secondary" disabled={busy} onClick={() => updateStatus(ticket.id, 2)}>Opgelost</button>
              </div>
            </td>
          )}
        </tr>
      )
    })
  }

  return (
    <div>
      <div className="page-header">
        <h2>Support Tickets</h2>
      </div>

      {error && <div className="error-box">{error}</div>}

      {isAdmin === null && <div className="empty-state">Laden...</div>}

      {isAdmin === false && (
        <>
          <div className="card" style={{ marginBottom: 20 }}>
            <h3 style={{ marginBottom: 12 }}>Nieuw ticket aanmaken</h3>
            <div className="form-row">
              <input placeholder="Onderwerp" value={subject} onChange={(e) => setSubject(e.target.value)} />
              <select value={priority} onChange={(e) => setPriority(e.target.value)}>
                <option value="0">Laag</option>
                <option value="1">Medium</option>
                <option value="2">Hoog</option>
                <option value="3">Kritiek</option>
              </select>
            </div>
            <div className="form-row">
              <textarea
                placeholder="Beschrijving"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                rows={3}
                style={{ flex: 1, minWidth: 260 }}
              />
            </div>
            <button className="btn-primary" disabled={busy} onClick={createTicket}>Ticket aanmaken</button>
          </div>

          <h3 style={{ marginBottom: 12 }}>Mijn tickets</h3>
          {!myTickets && <div className="empty-state">Laden...</div>}
          {myTickets && myTickets.length === 0 && <div className="empty-state">Nog geen tickets.</div>}
          {myTickets && myTickets.length > 0 && (
            <table>
              <thead>
                <tr><th>#</th><th>Onderwerp</th><th>Prioriteit</th><th>Status</th><th>Datum</th></tr>
              </thead>
              <tbody>{renderTicketRows(myTickets, false)}</tbody>
            </table>
          )}
        </>
      )}

      {isAdmin === true && (
        <>
          <h3 style={{ marginBottom: 12 }}>Alle tickets</h3>
          {!allTickets && <div className="empty-state">Laden...</div>}
          {allTickets && allTickets.length === 0 && (
            <div className="empty-state">Geen tickets in het systeem.</div>
          )}
          {allTickets && allTickets.length > 0 && (
            <table>
              <thead>
                <tr><th>#</th><th>Onderwerp</th><th>Prioriteit</th><th>Status</th><th>Datum</th><th>Actie</th></tr>
              </thead>
              <tbody>{renderTicketRows(allTickets, true)}</tbody>
            </table>
          )}
        </>
      )}
    </div>
  )
}
