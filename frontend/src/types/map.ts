export interface RobotMap {
  id: number
  name: string
  description?: string | null
  rows: number
  columns: number
  isSquare?: boolean | null
  createdDate?: string
  modifiedDate?: string
}

export interface UpsertMapRequest {
  name: string
  description?: string
  rows: number
  columns: number
}
