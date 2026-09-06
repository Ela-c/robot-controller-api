import { Navigate, useLocation } from 'react-router-dom'
import type { ReactNode } from 'react'
import { useAuth } from '../../hooks/useAuth'

interface RoleGateProps {
  allowed: string[]
  children: ReactNode
}

export const RoleGate = ({ allowed, children }: RoleGateProps) => {
  const { user } = useAuth()
  const location = useLocation()
  const role = user?.role?.toLowerCase() ?? ''

  if (!allowed.map((value) => value.toLowerCase()).includes(role)) {
    return <Navigate to="/app" replace state={{ from: location, forbidden: true }} />
  }

  return <>{children}</>
}
