import { createContext, useContext, useMemo, useState, type ReactNode } from 'react'
import type { RealtimeStatus } from './realtimeStatus'

interface RealtimeContextValue {
  status: RealtimeStatus
  setStatus: (status: RealtimeStatus) => void
}

const RealtimeContext = createContext<RealtimeContextValue | undefined>(undefined)

export const RealtimeProvider = ({ children }: { children: ReactNode }) => {
  const [status, setStatus] = useState<RealtimeStatus>('disconnected')

  const value = useMemo(() => ({ status, setStatus }), [status])

  return <RealtimeContext.Provider value={value}>{children}</RealtimeContext.Provider>
}

export const useRealtime = () => {
  const context = useContext(RealtimeContext)
  if (!context) {
    throw new Error('useRealtime must be used within RealtimeProvider')
  }

  return context
}
