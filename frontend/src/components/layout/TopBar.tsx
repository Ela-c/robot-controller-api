import { LogOut } from 'lucide-react'
import { useAuth } from '../../hooks/useAuth'

export const TopBar = () => {
  const { user, logout } = useAuth()

  return (
    <header className="flex items-center justify-between border-b border-slate-800 bg-slate-950 px-4 py-3 md:px-6">
      <h1 className="text-base font-semibold tracking-wide text-slate-100">Robot Operations Console</h1>
      <div className="flex items-center gap-3">
        <div className="text-right text-sm">
          <div className="font-medium text-slate-100">{user ? `${user.firstName} ${user.lastName}` : 'Anonymous'}</div>
          <div className="text-slate-400">{user?.role ?? 'No role'}</div>
        </div>
        <button
          type="button"
          onClick={() => void logout()}
          className="inline-flex items-center gap-1 rounded-md border border-slate-700 px-3 py-2 text-xs text-slate-200 hover:bg-slate-800"
        >
          <LogOut size={14} /> Logout
        </button>
      </div>
    </header>
  )
}
