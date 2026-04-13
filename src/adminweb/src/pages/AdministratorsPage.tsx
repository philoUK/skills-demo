import { useState, useEffect } from 'react'
import { useAuth } from 'react-oidc-context'
import { Link } from 'react-router-dom'
import { listAdministrators, type Administrator } from '../api/administrators'

const STATUS_LABELS: Record<string, string> = {
  active: 'Active',
  inactive: 'Inactive',
  pending: 'Pending',
  pending_expired: 'Pending Expired',
}

const STATUS_COLORS: Record<string, string> = {
  active: '#22c55e',
  inactive: '#94a3b8',
  pending: '#f59e0b',
  pending_expired: '#ef4444',
}

function StatusBadge({ status }: { status: string }) {
  return (
    <span
      style={{
        backgroundColor: STATUS_COLORS[status] ?? '#94a3b8',
        color: 'white',
        padding: '2px 10px',
        borderRadius: '12px',
        fontSize: '0.75rem',
        fontWeight: 600,
      }}
    >
      {STATUS_LABELS[status] ?? status}
    </span>
  )
}

export function AdministratorsPage() {
  const auth = useAuth()
  const [administrators, setAdministrators] = useState<Administrator[]>([])
  const [search, setSearch] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const token = auth.user?.access_token
    if (!token) return

    setLoading(true)
    setError(null)
    listAdministrators(token, search)
      .then((r) => setAdministrators(r.administrators))
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false))
  }, [search, auth.user?.access_token])

  return (
    <div style={{ fontFamily: 'sans-serif' }}>
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          padding: '16px 24px',
          borderBottom: '1px solid #e5e7eb',
        }}
      >
        <h1 style={{ margin: 0, fontSize: '1.5rem' }}>Administrators</h1>
        <Link to="/" style={{ color: '#6b7280', fontSize: '0.9rem' }}>
          ← Dashboard
        </Link>
      </div>

      <div style={{ padding: '24px' }}>
        <div style={{ display: 'flex', gap: '8px', marginBottom: '24px', maxWidth: '480px' }}>
          <input
            type="text"
            placeholder="Search by email (min 3 characters)"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            style={{
              flex: 1,
              padding: '8px 12px',
              border: '1px solid #d1d5db',
              borderRadius: '6px',
              fontSize: '0.9rem',
            }}
          />
          {search && (
            <button
              onClick={() => setSearch('')}
              style={{
                padding: '8px 12px',
                cursor: 'pointer',
                border: '1px solid #d1d5db',
                borderRadius: '6px',
                background: 'white',
              }}
            >
              Clear
            </button>
          )}
        </div>

        {error && (
          <p style={{ color: '#ef4444', marginBottom: '16px' }}>{error}</p>
        )}

        {loading ? (
          <p style={{ color: '#6b7280' }}>Loading...</p>
        ) : (
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
            <thead>
              <tr style={{ borderBottom: '2px solid #e5e7eb' }}>
                <th style={{ textAlign: 'left', padding: '10px 12px', color: '#374151' }}>
                  Email
                </th>
                <th style={{ textAlign: 'left', padding: '10px 12px', color: '#374151' }}>
                  First Name
                </th>
                <th style={{ textAlign: 'left', padding: '10px 12px', color: '#374151' }}>
                  Last Name
                </th>
                <th style={{ textAlign: 'left', padding: '10px 12px', color: '#374151' }}>
                  Status
                </th>
              </tr>
            </thead>
            <tbody>
              {administrators.map((a) => (
                <tr key={a.id} style={{ borderBottom: '1px solid #f3f4f6' }}>
                  <td style={{ padding: '10px 12px' }}>{a.email}</td>
                  <td style={{ padding: '10px 12px' }}>{a.firstName}</td>
                  <td style={{ padding: '10px 12px' }}>{a.lastName}</td>
                  <td style={{ padding: '10px 12px' }}>
                    <StatusBadge status={a.status} />
                  </td>
                </tr>
              ))}
              {administrators.length === 0 && (
                <tr>
                  <td
                    colSpan={4}
                    style={{ padding: '24px 12px', textAlign: 'center', color: '#9ca3af' }}
                  >
                    No administrators found
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  )
}
