import type { AxiosError } from 'axios'
import type { ApiError, ProblemDetails } from '../types/api'

const asObject = (value: unknown): Record<string, unknown> | null => {
  if (!value || typeof value !== 'object') {
    return null
  }
  return value as Record<string, unknown>
}

export const toApiError = (error: unknown): ApiError => {
  const axiosError = error as AxiosError
  const status = axiosError.response?.status ?? 0
  const data = axiosError.response?.data

  if (typeof data === 'string') {
    return { status, message: data, raw: error }
  }

  const objectData = asObject(data)
  if (objectData) {
    const detail = typeof objectData.detail === 'string' ? objectData.detail : undefined
    const title = typeof objectData.title === 'string' ? objectData.title : undefined
    const message = detail ?? title ?? axiosError.message ?? 'Unexpected API error'

    return {
      status,
      message,
      details: objectData as unknown as ProblemDetails,
      raw: error,
    }
  }

  return {
    status,
    message: axiosError.message ?? 'Unexpected API error',
    raw: error,
  }
}

export const getErrorMessage = (error: unknown): string => {
  const apiError = toApiError(error)

  if (apiError.status === 401) {
    return 'Your session has expired. Please sign in again.'
  }

  if (apiError.status === 403) {
    return "You don't have permission to perform this action."
  }

  return apiError.message
}
