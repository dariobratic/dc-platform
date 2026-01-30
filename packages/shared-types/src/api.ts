// API response wrappers — matches cross-service patterns

export interface PagedResponse<T> {
  items: T[]
  totalCount: number
  skip: number
  take: number
  hasMore: boolean
}

export interface ApiError {
  message: string
  code?: string
  details?: Record<string, unknown>
}

// Standard HTTP error response shape
export interface ApiErrorResponse {
  error: string
  statusCode?: number
  traceId?: string
}

// Generic result wrapper for frontend state management
export type ApiResult<T> =
  | { success: true; data: T }
  | { success: false; error: ApiError }
