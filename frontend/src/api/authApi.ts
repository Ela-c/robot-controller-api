import { apiClient } from './client'
import type { LoginRequest, User } from '../types/user'

export const authApi = {
  async login(payload: LoginRequest): Promise<{ message: string }> {
    const response = await apiClient.post('/users/login', payload)
    return response.data
  },

  async logout(): Promise<void> {
    await apiClient.post('/users/logout')
  },

  async logoutFromAllDevices(): Promise<void> {
    await apiClient.post('/users/logout-from-all-devices')
  },

  async getCurrentUser(): Promise<User> {
    const response = await apiClient.get('/users/me')
    return response.data
  },
}
