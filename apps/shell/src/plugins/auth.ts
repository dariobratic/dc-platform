import { UserManager, WebStorageStateStore } from 'oidc-client-ts'

const authority = `${import.meta.env.VITE_KEYCLOAK_URL}/realms/${import.meta.env.VITE_KEYCLOAK_REALM}`
const clientId = import.meta.env.VITE_KEYCLOAK_CLIENT_ID
const redirectUri = `${window.location.origin}/callback`
const postLogoutRedirectUri = window.location.origin

export const userManager = new UserManager({
  authority,
  client_id: clientId,
  redirect_uri: redirectUri,
  post_logout_redirect_uri: postLogoutRedirectUri,
  response_type: 'code',
  scope: 'openid profile email',
  userStore: new WebStorageStateStore({ store: sessionStorage }),
  automaticSilentRenew: true,
  loadUserInfo: true,
})

export function setupAuthErrorHandling(): void {
  userManager.events.addUserSignedOut(() => {
    console.log('User signed out')
  })

  userManager.events.addAccessTokenExpired(() => {
    console.log('Access token expired')
  })

  userManager.events.addSilentRenewError((error) => {
    console.error('Silent renew error:', error)
  })
}
