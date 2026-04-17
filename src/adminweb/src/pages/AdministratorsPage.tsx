import { useReducer, useEffect, useState } from 'react'
import { useAuth } from 'react-oidc-context'
import { Link, useNavigate } from 'react-router-dom'
import {
  listAdministrators,
  deactivateAdministrator,
  reactivateAdministrator,
  inviteAdministrator,
  resendInvitation,
  cancelInvitation,
  type Administrator,
} from '../api/administrators'
import { InactiveAccountError } from '../api/errors'

type State =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; administrators: Administrator[] }
  | { status: 'error'; message: string }

type Action =
  | { type: 'fetch' }
  | { type: 'success'; administrators: Administrator[] }
  | { type: 'error'; message: string }

function reduce(_: State, action: Action): State {
  switch (action.type) {
    case 'fetch':   return { status: 'loading' }
    case 'success': return { status: 'success', administrators: action.administrators }
    case 'error':   return { status: 'error', message: action.message }
  }
}

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

interface InviteFormState {
  firstName: string
  lastName: string
  email: string
}

interface InviteModalProps {
  onClose: () => void
  onSuccess: () => void
  token: string
}

function InviteModal({ onClose, onSuccess, token }: InviteModalProps) {
  const navigate = useNavigate()
  const [form, setForm] = useState<InviteFormState>({ firstName: '', lastName: '', email: '' })
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault()
    setSubmitting(true)
    setError(null)
    try {
      await inviteAdministrator(token, {
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
      })
      onSuccess()
    } catch (e: unknown) {
      if (e instanceof InactiveAccountError) {
        navigate('/inactive', { replace: true })
      } else {
        setError(e instanceof Error ? e.message : 'Failed to send invitation.')
      }
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div className="bg-white text-gray-900 rounded-lg shadow-xl w-full max-w-md mx-4">
        <div className="flex justify-between items-center px-6 py-4 border-b border-gray-200">
          <h2 className="text-lg font-semibold">Invite Administrator</h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 text-xl leading-none cursor-pointer"
          >
            &times;
          </button>
        </div>
        <form onSubmit={handleSubmit} className="px-6 py-4 space-y-4">
          {error && <p className="text-red-500 text-sm">{error}</p>}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">First Name</label>
            <input
              type="text"
              required
              value={form.firstName}
              onChange={(e) => setForm((f) => ({ ...f, firstName: e.target.value }))}
              className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Last Name</label>
            <input
              type="text"
              required
              value={form.lastName}
              onChange={(e) => setForm((f) => ({ ...f, lastName: e.target.value }))}
              className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input
              type="email"
              required
              value={form.email}
              onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))}
              className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button
              type="button"
              onClick={onClose}
              disabled={submitting}
              className="px-4 py-2 text-sm border border-gray-300 rounded-md bg-white hover:bg-gray-50 cursor-pointer disabled:opacity-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={submitting}
              className="px-4 py-2 text-sm bg-blue-600 text-white rounded-md hover:bg-blue-700 cursor-pointer disabled:opacity-50"
            >
              {submitting ? 'Sending…' : 'Send Invitation'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

export function AdministratorsPage() {
  const auth = useAuth()
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [state, dispatch] = useReducer(reduce, { status: 'idle' })
  const [actionError, setActionError] = useState<string | null>(null)
  const [pendingAction, setPendingAction] = useState<string | null>(null)
  const [showInviteModal, setShowInviteModal] = useState(false)

  const currentUserSub = auth.user?.profile?.sub as string | undefined

  function handleInactive() {
    navigate('/inactive', { replace: true })
  }

  useEffect(() => {
    const token = auth.user?.access_token
    if (!token) return

    dispatch({ type: 'fetch' })
    listAdministrators(token, search)
      .then((r) => dispatch({ type: 'success', administrators: r.administrators }))
      .catch((e: unknown) => {
        if (e instanceof InactiveAccountError) handleInactive()
        else dispatch({ type: 'error', message: e instanceof Error ? e.message : 'Failed to fetch administrators.' })
      })
  }, [search, auth.user?.access_token])

  function reload() {
    const token = auth.user?.access_token
    if (!token) return
    dispatch({ type: 'fetch' })
    listAdministrators(token, search)
      .then((r) => dispatch({ type: 'success', administrators: r.administrators }))
      .catch((e: unknown) => {
        if (e instanceof InactiveAccountError) handleInactive()
        else dispatch({ type: 'error', message: e instanceof Error ? e.message : 'Failed to fetch administrators.' })
      })
  }

  async function handleDeactivate(id: string) {
    const token = auth.user?.access_token
    if (!token) return
    setActionError(null)
    setPendingAction(id)
    try {
      await deactivateAdministrator(token, id)
      reload()
    } catch (e: unknown) {
      if (e instanceof InactiveAccountError) handleInactive()
      else setActionError(e instanceof Error ? e.message : 'Failed to deactivate administrator.')
    } finally {
      setPendingAction(null)
    }
  }

  async function handleReactivate(id: string) {
    const token = auth.user?.access_token
    if (!token) return
    setActionError(null)
    setPendingAction(id)
    try {
      await reactivateAdministrator(token, id)
      reload()
    } catch (e: unknown) {
      if (e instanceof InactiveAccountError) handleInactive()
      else setActionError(e instanceof Error ? e.message : 'Failed to reactivate administrator.')
    } finally {
      setPendingAction(null)
    }
  }

  async function handleResendInvitation(id: string) {
    const token = auth.user?.access_token
    if (!token) return
    setActionError(null)
    setPendingAction(id)
    try {
      await resendInvitation(token, id)
      reload()
    } catch (e: unknown) {
      if (e instanceof InactiveAccountError) handleInactive()
      else setActionError(e instanceof Error ? e.message : 'Failed to resend invitation.')
    } finally {
      setPendingAction(null)
    }
  }

  async function handleCancelInvitation(id: string) {
    const token = auth.user?.access_token
    if (!token) return
    setActionError(null)
    setPendingAction(id)
    try {
      await cancelInvitation(token, id)
      reload()
    } catch (e: unknown) {
      if (e instanceof InactiveAccountError) handleInactive()
      else setActionError(e instanceof Error ? e.message : 'Failed to cancel invitation.')
    } finally {
      setPendingAction(null)
    }
  }

  function handleInviteSuccess() {
    setShowInviteModal(false)
    reload()
  }

  const administrators = state.status === 'success' ? state.administrators : []

  return (
    <div>
      {showInviteModal && auth.user?.access_token && (
        <InviteModal
          token={auth.user.access_token}
          onClose={() => setShowInviteModal(false)}
          onSuccess={handleInviteSuccess}
        />
      )}

      <div className="flex justify-between items-center px-6 py-4 border-b border-gray-200">
        <h1 className="m-0 text-2xl font-semibold">Administrators</h1>
        <Link to="/" className="text-gray-500 text-sm no-underline hover:text-gray-700">
          ← Dashboard
        </Link>
      </div>

      <div className="p-6">
        <div className="flex justify-between items-center mb-6">
          <div className="flex gap-2 max-w-md flex-1">
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
          <button
            onClick={() => setShowInviteModal(true)}
            className="ml-4 px-4 py-2 text-sm bg-blue-600 text-white rounded-md hover:bg-blue-700 cursor-pointer"
          >
            Invite Administrator
          </button>
        </div>

        {state.status === 'error' && (
          <p className="text-red-500 mb-4">{state.message}</p>
        )}

        {actionError && (
          <p className="text-red-500 mb-4">{actionError}</p>
        )}

        {state.status === 'loading' ? (
          <p className="text-gray-500">Loading...</p>
        ) : (
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b-2 border-gray-200">
                <th className="text-left px-3 py-2.5 text-gray-700 font-medium">Email</th>
                <th className="text-left px-3 py-2.5 text-gray-700 font-medium">First Name</th>
                <th className="text-left px-3 py-2.5 text-gray-700 font-medium">Last Name</th>
                <th className="text-left px-3 py-2.5 text-gray-700 font-medium">Status</th>
                <th className="text-left px-3 py-2.5 text-gray-700 font-medium">Actions</th>
              </tr>
            </thead>
            <tbody>
              {administrators.map((a: Administrator) => {
                const isSelf = currentUserSub !== undefined && a.id === currentUserSub
                const isPending = pendingAction === a.id
                return (
                  <tr key={a.id} className="border-b border-gray-100">
                    <td className="px-3 py-2.5">{a.email}</td>
                    <td className="px-3 py-2.5">{a.firstName}</td>
                    <td className="px-3 py-2.5">{a.lastName}</td>
                    <td className="px-3 py-2.5">
                      <StatusBadge status={a.status} />
                    </td>
                    <td className="px-3 py-2.5">
                      {a.status === 'active' && (
                        <button
                          onClick={() => handleDeactivate(a.id)}
                          disabled={isPending || isSelf}
                          title={isSelf ? 'You cannot deactivate your own account' : undefined}
                          className="px-3 py-1 text-xs border border-red-300 text-red-600 rounded hover:bg-red-50 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer"
                        >
                          {isPending ? 'Deactivating…' : 'Deactivate'}
                        </button>
                      )}
                      {a.status === 'inactive' && (
                        <button
                          onClick={() => handleReactivate(a.id)}
                          disabled={isPending}
                          className="px-3 py-1 text-xs border border-green-300 text-green-600 rounded hover:bg-green-50 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer"
                        >
                          {isPending ? 'Reactivating…' : 'Reactivate'}
                        </button>
                      )}
                      {(a.status === 'pending' || a.status === 'pending_expired') && (
                        <span className="flex gap-2">
                          <button
                            onClick={() => handleResendInvitation(a.id)}
                            disabled={isPending}
                            className="px-3 py-1 text-xs border border-amber-300 text-amber-600 rounded hover:bg-amber-50 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer"
                          >
                            {isPending ? 'Resending…' : 'Resend'}
                          </button>
                          <button
                            onClick={() => handleCancelInvitation(a.id)}
                            disabled={isPending}
                            className="px-3 py-1 text-xs border border-red-300 text-red-600 rounded hover:bg-red-50 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer"
                          >
                            {isPending ? 'Cancelling…' : 'Cancel'}
                          </button>
                        </span>
                      )}
                    </td>
                  </tr>
                )
              })}
              {administrators.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-3 py-6 text-center text-gray-400">
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
