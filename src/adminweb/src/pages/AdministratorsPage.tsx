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

const STATUS_CLASSES: Record<string, string> = {
  active: 'bg-green-500',
  inactive: 'bg-slate-400',
  pending: 'bg-amber-400',
  pending_expired: 'bg-red-500',
}

function StatusBadge({ status }: { status: string }) {
  return (
    <span
      className={`${STATUS_CLASSES[status] ?? 'bg-slate-400'} text-white px-2.5 py-0.5 rounded-full text-xs font-semibold`}
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
    <div>
      <div className="flex justify-between items-center px-6 py-4 border-b border-gray-200">
        <h1 className="m-0 text-2xl font-semibold">Administrators</h1>
        <Link to="/" className="text-gray-500 text-sm no-underline hover:text-gray-700">
          ← Dashboard
        </Link>
      </div>

      <div className="p-6">
        <div className="flex gap-2 mb-6 max-w-md">
          <input
            type="text"
            placeholder="Search by email (min 3 characters)"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="flex-1 px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          {search && (
            <button
              onClick={() => setSearch('')}
              className="px-3 py-2 border border-gray-300 rounded-md bg-white hover:bg-gray-50 cursor-pointer text-sm"
            >
              Clear
            </button>
          )}
        </div>

        {error && <p className="text-red-500 mb-4">{error}</p>}

        {loading ? (
          <p className="text-gray-500">Loading...</p>
        ) : (
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b-2 border-gray-200">
                <th className="text-left px-3 py-2.5 text-gray-700 font-medium">Email</th>
                <th className="text-left px-3 py-2.5 text-gray-700 font-medium">First Name</th>
                <th className="text-left px-3 py-2.5 text-gray-700 font-medium">Last Name</th>
                <th className="text-left px-3 py-2.5 text-gray-700 font-medium">Status</th>
              </tr>
            </thead>
            <tbody>
              {administrators.map((a) => (
                <tr key={a.id} className="border-b border-gray-100">
                  <td className="px-3 py-2.5">{a.email}</td>
                  <td className="px-3 py-2.5">{a.firstName}</td>
                  <td className="px-3 py-2.5">{a.lastName}</td>
                  <td className="px-3 py-2.5">
                    <StatusBadge status={a.status} />
                  </td>
                </tr>
              ))}
              {administrators.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-3 py-6 text-center text-gray-400">
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
