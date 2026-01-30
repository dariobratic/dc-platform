import type { AxiosInstance } from 'axios'
import type {
  TokenRequest,
  TokenResponse,
  RefreshTokenRequest,
  LogoutRequest,
  UserInfoResponse,
} from '@dc-platform/shared-types'

export async function exchangeCodeForToken(client: AxiosInstance, request: TokenRequest): Promise<TokenResponse> {
  const { data } = await client.post('/api/auth/token', request)
  return data
}

export async function refreshToken(client: AxiosInstance, request: RefreshTokenRequest): Promise<TokenResponse> {
  const { data } = await client.post('/api/auth/refresh', request)
  return data
}

export async function getUserInfo(client: AxiosInstance): Promise<UserInfoResponse> {
  const { data } = await client.get('/api/auth/userinfo')
  return data
}

export async function logout(client: AxiosInstance, request: LogoutRequest): Promise<void> {
  await client.post('/api/auth/logout', request)
}
