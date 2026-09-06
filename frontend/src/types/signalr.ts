import type { RobotSequenceStatus } from './robot'

export interface RobotCommandUpdatedEvent {
  commandId: number
  name: string
  status: string
  createdAt: string
  startedAt?: string | null
  completedAt?: string | null
  updatedAt: string
  failureReason?: string | null
}

export interface RobotPositionUpdatedEvent {
  commandId: number
  x: number
  y: number
  timestamp: string
}

export interface RobotSequenceUpdatedEvent {
  sequenceId: number
  status: RobotSequenceStatus
  currentStep: number
  totalSteps: number
  timestamp: string
  failureReason?: string | null
}

export interface RobotSequencePositionUpdatedEvent {
  sequenceId: number
  mapId: number
  x: number
  y: number
  step: number
  totalSteps: number
  timestamp: string
}

export interface HubConnectionStateSnapshot {
  status: 'disconnected' | 'connecting' | 'connected' | 'reconnecting'
}
