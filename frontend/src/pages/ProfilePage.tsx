import { useState } from 'react'
import { useAuth } from '../hooks/useAuth'
import { getErrorMessage } from '../utils/httpError'
import { usersApi } from '../api/usersApi'

export const ProfilePage = () => {
  const { user, logoutFromAllDevices } = useAuth()
  const [busy, setBusy] = useState(false)
  const [message, setMessage] = useState<string | null>(null)
  const [email, setEmail] = useState(user?.email ?? '')
  const [password, setPassword] = useState('')

  const handleCredentialsUpdate = async () => {
    if (!user?.id) {
      setMessage('User context is unavailable.')
      return
    }

    setBusy(true)
    setMessage(null)
    try {
      await usersApi.patchUserCredentials(user.id, { email, password })
      setMessage('Credentials updated.')
      setPassword('')
    } catch (e) {
      setMessage(getErrorMessage(e))
    } finally {
      setBusy(false)
    }
  }

  const handleLogoutAll = async () => {
    setBusy(true)
    setMessage(null)
    try {
      await logoutFromAllDevices()
    } catch (e) {
      setMessage(getErrorMessage(e))
    } finally {
      setBusy(false)
    }
  }

  return (
    <section className="space-y-4">
      <h2 className="text-xl font-semibold">Profile</h2>
      <div className="rounded-lg border border-slate-800 bg-slate-900 p-4 text-sm">
        <p><span className="text-slate-400">Name:</span> {user?.firstName} {user?.lastName}</p>
        <p><span className="text-slate-400">Email:</span> {user?.email}</p>
        <p><span className="text-slate-400">Role:</span> {user?.role}</p>
      </div>
      <div className="rounded-lg border border-slate-800 bg-slate-900 p-4 text-sm">
        <h3 className="mb-2 text-sm font-semibold">Update email/password</h3>
        <div className="grid gap-2 md:grid-cols-2">
          <input value={email} onChange={(event) => setEmail(event.target.value)} placeholder="Email" className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2" />
          <input value={password} onChange={(event) => setPassword(event.target.value)} type="password" placeholder="New password" className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2" />
        </div>
        <button type="button" disabled={busy || !password || !email} onClick={() => void handleCredentialsUpdate()} className="mt-3 rounded-md bg-cyan-500 px-3 py-2 text-sm font-medium text-slate-950 disabled:opacity-60">
          Save credentials
        </button>
      </div>
      <button type="button" disabled={busy} onClick={() => void handleLogoutAll()} className="rounded-md border border-slate-700 px-3 py-2 text-sm hover:bg-slate-800 disabled:opacity-60">
        {busy ? 'Processing...' : 'Logout from all devices'}
      </button>
      {message && <p className="text-sm text-rose-300">{message}</p>}
    </section>
  )
}
