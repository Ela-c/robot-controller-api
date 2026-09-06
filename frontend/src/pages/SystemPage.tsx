import { apiBaseUrl } from '../api/client'
import { useAuth } from '../hooks/useAuth'
import { useRealtime } from '../realtime/RealtimeContext'
import { useState } from 'react'

export const SystemPage = () => {
  const { user } = useAuth()
  const { status } = useRealtime()
  const [open, setOpen] = useState(false)

  return (
    <section className="space-y-4">
      <h2 className="text-xl font-semibold">System</h2>
      <div className="rounded-lg border border-slate-800 bg-slate-900 p-4 text-sm">
        <p><span className="text-slate-400">Backend:</span> Connected</p>
        <p><span className="text-slate-400">Realtime:</span> {status}</p>
        <p><span className="text-slate-400">API URL:</span> {apiBaseUrl}</p>
        <p><span className="text-slate-400">Authenticated as:</span> {user?.email ?? 'None'}</p>
        <p><span className="text-slate-400">Role:</span> {user?.role ?? 'None'}</p>
      </div>

      <div className="rounded-lg border border-slate-800 bg-slate-900 p-4 text-sm">
        <button
          type="button"
          onClick={() => setOpen((current) => !current)}
          className="mb-2 text-sm font-semibold text-cyan-300"
        >
          Developer details {open ? '▲' : '▼'}
        </button>
        {open && (
          <div className="space-y-2 text-xs text-slate-300">
            <p>Hub: /hubs/robot</p>
            <p>Hub methods: SubscribeToCommand, UnsubscribeFromCommand, SubscribeToSequence, UnsubscribeFromSequence</p>
            <p>Events: RobotCommandUpdated, RobotPositionUpdated, RobotSequenceUpdated</p>
            <p>Primary APIs: /users/me, /api/maps, /api/robot/state, /api/robot-commands, /api/robot/sequences</p>
          </div>
        )}
      </div>
    </section>
  )
}
