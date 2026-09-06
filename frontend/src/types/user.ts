export type UserRole = 'admin' | 'user' | string

export interface User {
  id: number
  email: string
  firstName: string
  lastName: string
  description?: string | null
  role?: UserRole | null
  createdDate?: string
  modifiedDate?: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface CreateUserRequest {
  firstName: string
  lastName: string
  email: string
  passwordHash: string
  description?: string
  role?: UserRole
}

export interface UpdateUserRequest {
  firstName: string
  lastName: string
  description?: string
  role?: UserRole
}

export interface PatchUserCredentialsRequest {
  email: string
  password: string
}
