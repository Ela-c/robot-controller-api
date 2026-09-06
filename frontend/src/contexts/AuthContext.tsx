import { createContext, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { authApi } from '../api/authApi'
import type { LoginRequest, User } from '../types/user'
import { toApiError } from '../utils/httpError'

interface AuthContextValue {
  user: User | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (payload: LoginRequest) => Promise<void>
  logout: () => Promise<void>
  logoutFromAllDevices: () => Promise<void>
  refreshUser: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined)

interface AuthProviderProps {
  children: ReactNode
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  const refreshUser = useCallback(async () => {
    try {
      const me = await authApi.getCurrentUser()
      setUser(me)
    } catch (error) {
      const apiError = toApiError(error)
      if (apiError.status === 401) {
        setUser(null)
        return
      }
      throw error
    }
  }, [])

  useEffect(() => {
    let mounted = true

    const bootstrap = async () => {
      try {
        await refreshUser()
      } finally {
        if (mounted) {
          setIsLoading(false)
        }
      }
    }

    bootstrap()

    return () => {
      mounted = false
    }
  }, [refreshUser])

  const login = useCallback(async (payload: LoginRequest) => {
    await authApi.login(payload)
    await refreshUser()
  }, [refreshUser])

  const logout = useCallback(async () => {
    await authApi.logout()
    setUser(null)
  }, [])

  const logoutFromAllDevices = useCallback(async () => {
    await authApi.logoutFromAllDevices()
    setUser(null)
  }, [])

  const value = useMemo<AuthContextValue>(() => ({
    user,
    isAuthenticated: Boolean(user),
    isLoading,
    login,
    logout,
    logoutFromAllDevices,
    refreshUser,
  }), [isLoading, login, logout, logoutFromAllDevices, refreshUser, user])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
