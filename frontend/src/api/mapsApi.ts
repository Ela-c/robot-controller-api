import { apiClient } from './client'
import type { RobotMap, UpsertMapRequest } from '../types/map'

export const mapsApi = {
  async getMaps(): Promise<RobotMap[]> {
    const response = await apiClient.get('/api/maps')
    return response.data
  },

  async getSquareMaps(): Promise<RobotMap[]> {
    const response = await apiClient.get('/api/maps/square')
    return response.data
  },

  async getMapById(id: number): Promise<RobotMap> {
    const response = await apiClient.get(`/api/maps/${id}`)
    return response.data
  },

  async createMap(payload: UpsertMapRequest): Promise<RobotMap> {
    const response = await apiClient.post('/api/maps', payload)
    return response.data
  },

  async updateMap(id: number, payload: UpsertMapRequest): Promise<void> {
    await apiClient.put(`/api/maps/${id}`, payload)
  },

  async deleteMap(id: number): Promise<void> {
    await apiClient.delete(`/api/maps/${id}`)
  },

  async checkCoordinate(id: number, x: number, y: number): Promise<boolean> {
    const response = await apiClient.get(`/api/maps/${id}/${x}-${y}`)
    return response.data
  },
}
