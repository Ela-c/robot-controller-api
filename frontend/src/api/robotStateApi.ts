import { apiClient } from './client'
import type { PlaceRobotRequest, RobotState, SelectRobotMapRequest } from '../types/robot'

export const robotStateApi = {
  async getState(): Promise<RobotState> {
    const response = await apiClient.get('/api/robot/state')
    return response.data
  },

  async selectMap(payload: SelectRobotMapRequest): Promise<RobotState> {
    const response = await apiClient.put('/api/robot/state/map', payload)
    return response.data
  },

  async placeRobot(payload: PlaceRobotRequest): Promise<RobotState> {
    const response = await apiClient.put('/api/robot/state/position', payload)
    return response.data
  },
}
