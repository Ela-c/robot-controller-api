import type { User } from '../types/user'

export const isAdmin = (user: User | null | undefined): boolean =>
  user?.role?.toLowerCase() === 'admin'

export const isUser = (user: User | null | undefined): boolean =>
  user?.role?.toLowerCase() === 'user'
