export type MovementDirection = 'Up' | 'Down' | 'Left' | 'Right'

export interface RobotCommand {
  id: number
  name: string
  description?: string | null
  isMoveCommand: boolean
  movementDirection?: MovementDirection | null
  movementDirections?: MovementDirection[]
  createdDate: string
  modifiedDate: string
}

export interface RobotCommandSubmitRequest {
  name: string
  description?: string
  isMoveCommand: boolean
  movementDirection?: MovementDirection | null
  movementDirections?: MovementDirection[]
}

export interface RobotCommandSubmissionResponse {
  id: number
  commandUrl: string
}
