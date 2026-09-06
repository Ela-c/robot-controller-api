import type { MovementDirection } from './robotCommand'

export interface RobotState {
  mapId?: number | null
  mapName?: string | null
  x?: number | null
  y?: number | null
  hasPosition: boolean
  modifiedDate: string
}

export interface SelectRobotMapRequest {
  mapId: number
}

export interface PlaceRobotRequest {
  x: number
  y: number
}

export type RobotSequenceStatus = 'Pending' | 'Queued' | 'Executing' | 'Completed' | 'Failed' | 'Cancelled'

export interface RobotSequenceItem {
  order: number
  commandId: number
  commandName: string
  executed: boolean
  executedDate?: string | null
}

export interface RobotSequence {
  id: number
  status: RobotSequenceStatus
  mapId: number
  startX: number
  startY: number
  finalX?: number | null
  finalY?: number | null
  currentStep: number
  totalSteps: number
  cancellationRequested: boolean
  createdDate: string
  startedDate?: string | null
  completedDate?: string | null
  failureReason?: string | null
  items: RobotSequenceItem[]
}

export interface RobotCoordinate {
  x: number
  y: number
}

export interface RobotSequenceSubmitRequest {
  commandIds: number[]
  targetX?: number
  targetY?: number
}

export interface RobotSequenceSubmissionResponse {
  sequenceId: number
  status: RobotSequenceStatus
  startPosition: RobotCoordinate
  predictedFinalPosition: RobotCoordinate
  totalSteps: number
  statusUrl: string
}

export interface SequenceCancelResponse {
  id: number
  status: RobotSequenceStatus
  cancellationRequested: boolean
}

export interface DirectionCommandOption {
  id: number
  name: string
  direction: MovementDirection
}
