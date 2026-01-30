// Authentication types — matches services/authentication DTOs

export interface TokenRequest {
  code: string
  redirectUri: string
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface LogoutRequest {
  refreshToken: string
}

export interface TokenResponse {
  accessToken: string
  refreshToken: string
  expiresIn: number
  tokenType: string
}

export interface UserInfoResponse {
  sub: string
  email: string
  emailVerified: boolean
  preferredUsername: string
}

// Frontend-only auth types (not from backend)
export interface AuthState {
  isAuthenticated: boolean
  accessToken: string | null
  user: UserInfoResponse | null
}
