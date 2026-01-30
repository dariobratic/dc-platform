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

export interface SignupRequest {
  email: string
  password: string
  firstName: string
  lastName: string
  organizationName: string
}

export interface SigninRequest {
  email: string
  password: string
}

export interface SignupResponse {
  accessToken: string
  refreshToken: string
  expiresIn: number
  tokenType: string
  userId: string
  organizationId: string
  workspaceId: string
}

// Frontend-only auth types (not from backend)
export interface AuthState {
  isAuthenticated: boolean
  accessToken: string | null
  user: UserInfoResponse | null
}
