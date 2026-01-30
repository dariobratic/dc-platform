import type { AxiosInstance } from 'axios'
import type {
  TokenRequest,
  TokenResponse,
  RefreshTokenRequest,
  LogoutRequest,
  UserInfoResponse,
  SigninRequest,
  SignupRequest,
  SignupResponse,
} from '@dc-platform/shared-types'

export async function exchangeCodeForToken(client: AxiosInstance, request: TokenRequest): Promise<TokenResponse> {
  const { data } = await client.post('/api/v1/auth/token', request)
  return data
}

export async function refreshToken(client: AxiosInstance, request: RefreshTokenRequest): Promise<TokenResponse> {
  const { data } = await client.post('/api/v1/auth/refresh', request)
  return data
}

export async function getUserInfo(client: AxiosInstance): Promise<UserInfoResponse> {
  const { data } = await client.get('/api/v1/auth/userinfo')
  return data
}

export async function logout(client: AxiosInstance, request: LogoutRequest): Promise<void> {
  await client.post('/api/v1/auth/logout', request)
}

export async function signin(client: AxiosInstance, request: SigninRequest): Promise<TokenResponse> {
  const { data } = await client.post('/api/v1/auth/signin', request)
  return data
}

export async function signup(client: AxiosInstance, request: SignupRequest): Promise<SignupResponse> {
  const { data } = await client.post('/api/v1/auth/signup', request)
  return data
}
