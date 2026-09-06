import { apiBaseUrl } from '../../api/client'
import { useAuth } from '../../hooks/useAuth'
import { useRealtime } from '../../realtime/RealtimeContext'

const statusColor: Record<string, string> = {
  connected: 'bg-emerald-400',
  connecting: 'bg-amber-400',
  reconnecting: 'bg-amber-400',
  disconnected: 'bg-rose-400',
}

const formatRealtime = (status: string) => {
  if (status === 'connected') return 'Live'
  if (status === 'connecting') return 'Connecting...'
  if (status === 'reconnecting') return 'Reconnecting...'
  return 'Offline'
}

export const StatusBar = () => {
  const { status } = useRealtime()
  const { user, isAuthenticated } = useAuth()

  return (
    <footer className="flex flex-wrap items-center justify-between gap-3 border-t border-slate-800 bg-slate-950 px-4 py-2 text-xs text-slate-400 md:px-6">
      <div className="flex items-center gap-4">
        <div className="flex items-center gap-2"><span className="h-2 w-2 rounded-full bg-emerald-400" /> API Ready</div>
        <div className="flex items-center gap-2"><span className={`h-2 w-2 rounded-full ${statusColor[status] ?? statusColor.disconnected}`} /> Realtime {formatRealtime(status)}</div>
      </div>
      <div className="flex flex-wrap items-center gap-4">
        <span>{apiBaseUrl}</span>
        <span>{isAuthenticated ? user?.email : 'Not signed in'}</span>
      </div>
    </footer>
  )
}
