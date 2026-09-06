import { apiClient } from './client'
import type {
  CreateUserRequest,
  PatchUserCredentialsRequest,
  UpdateUserRequest,
  User,
} from '../types/user'

export const usersApi = {
  async getUsers(): Promise<User[]> {
    const response = await apiClient.get('/users')
    return response.data
  },

  async getAdminUsers(): Promise<User[]> {
    const response = await apiClient.get('/users/admin')
    return response.data
  },

  async getUserById(id: number): Promise<User> {
    const response = await apiClient.get(`/users/${id}`)
    return response.data
  },

  async createUser(payload: CreateUserRequest): Promise<User> {
    const response = await apiClient.post('/users', payload)
    return response.data
  },

  async updateUser(id: number, payload: UpdateUserRequest): Promise<void> {
    await apiClient.put(`/users/${id}`, payload)
  },

  async patchUserCredentials(id: number, payload: PatchUserCredentialsRequest): Promise<void> {
    await apiClient.patch(`/users/${id}`, payload)
  },

  async deleteUser(id: number): Promise<void> {
    await apiClient.delete(`/users/${id}`)
  },
}
