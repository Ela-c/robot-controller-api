export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  [key: string]: unknown
}

export interface ApiError {
  status: number
  message: string
  details?: ProblemDetails
  raw?: unknown
}

export interface DeveloperTrace {
  request: {
    method: string
    url: string
    payload?: unknown
  }
  response?: {
    status: number
    payload?: unknown
  }
}
