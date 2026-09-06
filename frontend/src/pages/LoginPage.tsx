import { useState } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { getErrorMessage } from '../utils/httpError'

export const LoginPage = () => {
  const { login, isAuthenticated, isLoading } = useAuth()
  const location = useLocation()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  if (!isLoading && isAuthenticated) {
    const from = (location.state as { from?: { pathname?: string } } | undefined)?.from?.pathname ?? '/app'
    return <Navigate to={from} replace />
  }

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    setError(null)
    setSubmitting(true)

    try {
      await login({ email, password })
    } catch (e) {
      setError(getErrorMessage(e))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-950 p-4">
      <form onSubmit={handleSubmit} className="w-full max-w-md rounded-lg border border-slate-800 bg-slate-900 p-6 shadow-xl shadow-black/20">
        <h1 className="mb-1 text-xl font-semibold text-slate-100">Robot Controller</h1>
        <p className="mb-6 text-sm text-slate-400">Sign in to access operations.</p>

        <label className="mb-2 block text-sm text-slate-300" htmlFor="email">Email</label>
        <input id="email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required className="mb-4 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100" />

        <label className="mb-2 block text-sm text-slate-300" htmlFor="password">Password</label>
        <input id="password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} required className="mb-4 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100" />

        {error && <p className="mb-3 rounded-md border border-rose-500/30 bg-rose-500/10 p-2 text-sm text-rose-200">{error}</p>}

        <button disabled={submitting} className="w-full rounded-md bg-cyan-500 px-3 py-2 font-medium text-slate-950 hover:bg-cyan-400 disabled:opacity-60" type="submit">
          {submitting ? 'Signing in...' : 'Sign In'}
        </button>
      </form>
    </div>
  )
}
