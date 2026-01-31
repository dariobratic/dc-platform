import type { RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
  {
    path: '',
    name: 'admin-dashboard',
    component: () => import('./pages/DashboardPage.vue'),
    meta: { breadcrumb: 'Dashboard' },
  },
  {
    path: 'organizations',
    name: 'admin-organizations',
    component: () => import('./pages/OrganizationsPage.vue'),
    meta: { breadcrumb: 'Organizations' },
  },
  {
    path: 'organizations/:id',
    name: 'admin-organization-detail',
    component: () => import('./pages/OrganizationDetailPage.vue'),
    meta: { breadcrumb: 'Organization Detail' },
  },
  {
    path: 'workspaces',
    name: 'admin-workspaces',
    component: () => import('./pages/WorkspacesPage.vue'),
    meta: { breadcrumb: 'Workspaces' },
  },
  {
    path: 'workspaces/:id',
    name: 'admin-workspace-detail',
    component: () => import('./pages/WorkspaceDetailPage.vue'),
    meta: { breadcrumb: 'Workspace Detail' },
  },
  {
    path: 'users',
    name: 'admin-users',
    component: () => import('./pages/UsersPage.vue'),
    meta: { breadcrumb: 'Users' },
  },
  {
    path: 'roles',
    name: 'admin-roles',
    component: () => import('./pages/RolesPage.vue'),
    meta: { breadcrumb: 'Roles' },
  },
  {
    path: 'audit',
    name: 'admin-audit',
    component: () => import('./pages/AuditLogPage.vue'),
    meta: { breadcrumb: 'Audit Log' },
  },
]

export default routes
