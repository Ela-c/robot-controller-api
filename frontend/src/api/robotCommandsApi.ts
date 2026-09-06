import { apiClient } from './client'
import type {
  RobotCommand,
  RobotCommandSubmissionResponse,
  RobotCommandSubmitRequest,
} from '../types/robotCommand'

export const robotCommandsApi = {
  async getCommands(): Promise<RobotCommand[]> {
    const response = await apiClient.get('/api/robot-commands')
    return response.data
  },

  async getMoveCommands(): Promise<RobotCommand[]> {
    const response = await apiClient.get('/api/robot-commands/move')
    return response.data
  },

  async getCommandById(id: number): Promise<RobotCommand> {
    const response = await apiClient.get(`/api/robot-commands/${id}`)
    return response.data
  },

  async submitCommand(payload: RobotCommandSubmitRequest): Promise<RobotCommandSubmissionResponse> {
    const response = await apiClient.post('/api/robot-commands', payload)
    return response.data
  },

  async updateCommand(id: number, payload: RobotCommandSubmitRequest): Promise<void> {
    await apiClient.put(`/api/robot-commands/${id}`, payload)
  },

  async deleteCommand(id: number): Promise<void> {
    await apiClient.delete(`/api/robot-commands/${id}`)
  },
}
