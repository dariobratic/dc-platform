import type { RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
  {
    path: '',
    name: 'client-dashboard',
    component: () => import('./pages/DashboardPage.vue'),
    meta: { breadcrumb: 'Dashboard' },
  },
  {
    path: 'workspace/:id',
    name: 'client-workspace',
    component: () => import('./pages/WorkspacePage.vue'),
    meta: { breadcrumb: 'Workspace' },
  },
  {
    path: 'profile',
    name: 'client-profile',
    component: () => import('./pages/ProfilePage.vue'),
    meta: { breadcrumb: 'Profile' },
  },
  {
    path: 'notifications',
    name: 'client-notifications',
    component: () => import('./pages/NotificationsPage.vue'),
    meta: { breadcrumb: 'Notifications' },
  },
  {
    path: 'invitations',
    name: 'client-invitations',
    component: () => import('./pages/InvitationsPage.vue'),
    meta: { breadcrumb: 'Invitations' },
  },
]

export default routes
