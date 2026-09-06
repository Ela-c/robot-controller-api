import { apiClient } from './client'
import type {
  RobotSequence,
  RobotSequenceStatus,
  RobotSequenceSubmissionResponse,
  RobotSequenceSubmitRequest,
  SequenceCancelResponse,
} from '../types/robot'

export const robotSequencesApi = {
  async submitSequence(payload: RobotSequenceSubmitRequest): Promise<RobotSequenceSubmissionResponse> {
    const response = await apiClient.post('/api/robot/sequences', payload)
    return response.data
  },

  async getSequences(status?: RobotSequenceStatus): Promise<RobotSequence[]> {
    const response = await apiClient.get('/api/robot/sequences', {
      params: status ? { status } : undefined,
    })
    return response.data
  },

  async getSequenceById(id: number): Promise<RobotSequence> {
    const response = await apiClient.get(`/api/robot/sequences/${id}`)
    return response.data
  },

  async cancelSequence(id: number): Promise<SequenceCancelResponse> {
    const response = await apiClient.post(`/api/robot/sequences/${id}/cancel`)
    return response.data
  },
}
