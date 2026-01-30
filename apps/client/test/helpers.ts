import { mount, type ComponentMountingOptions } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import { type Component, defineComponent } from 'vue'

const DcSpinner = defineComponent({
  name: 'DcSpinner',
  props: { size: String, color: String },
  template: '<div data-testid="dc-spinner" />',
})

const DcAlert = defineComponent({
  name: 'DcAlert',
  props: { variant: String, title: String, dismissible: Boolean },
  emits: ['dismiss'],
  template: '<div data-testid="dc-alert" role="alert"><span>{{ title }}</span><slot /></div>',
})

const DcButton = defineComponent({
  name: 'DcButton',
  props: { variant: String, size: String, disabled: Boolean, loading: Boolean, type: String },
  emits: ['click'],
  template: '<button data-testid="dc-button" @click="$emit(\'click\', $event)"><slot /></button>',
})

const DcCard = defineComponent({
  name: 'DcCard',
  props: { padding: Boolean },
  template: '<div data-testid="dc-card"><slot name="header" /><slot /><slot name="footer" /></div>',
})

const DcBadge = defineComponent({
  name: 'DcBadge',
  props: { variant: String, size: String, dot: Boolean },
  template: '<span data-testid="dc-badge"><slot /></span>',
})

const DcInput = defineComponent({
  name: 'DcInput',
  props: { modelValue: String, label: String, placeholder: String, error: String },
  emits: ['update:modelValue'],
  template: '<div data-testid="dc-input"><input :value="modelValue" /></div>',
})

const PageHeader = defineComponent({
  name: 'PageHeader',
  props: { title: String, description: String },
  template: '<div data-testid="page-header"><h1>{{ title }}</h1><slot name="actions" /></div>',
})

const WorkspaceCard = defineComponent({
  name: 'WorkspaceCard',
  props: { workspace: Object },
  template: '<div data-testid="workspace-card">{{ workspace?.name }}</div>',
})

const ActivityFeed = defineComponent({
  name: 'ActivityFeed',
  props: { entries: Array },
  template: '<div data-testid="activity-feed">{{ entries?.length ?? 0 }} entries</div>',
})

const QuickActions = defineComponent({
  name: 'QuickActions',
  template: '<div data-testid="quick-actions" />',
})

const EmptyState = defineComponent({
  name: 'EmptyState',
  props: { title: String, description: String },
  template: '<div data-testid="empty-state">{{ title }}<slot name="action" /></div>',
})

const InvitationCard = defineComponent({
  name: 'InvitationCard',
  props: { invitation: Object },
  template: '<div data-testid="invitation-card" />',
})

export const defaultStubs = {
  DcSpinner,
  DcAlert,
  DcButton,
  DcCard,
  DcBadge,
  DcInput,
  PageHeader,
  WorkspaceCard,
  ActivityFeed,
  QuickActions,
  EmptyState,
  InvitationCard,
}

export function mountPage(component: Component, options: ComponentMountingOptions<any> = {}) {
  const { global: globalOptions, ...restOptions } = options
  const { stubs: extraStubs, plugins: extraPlugins, ...restGlobal } = globalOptions ?? {}

  return mount(component, {
    global: {
      plugins: [createTestingPinia({ createSpy: () => vi.fn() }), ...(extraPlugins ?? [])],
      stubs: {
        ...defaultStubs,
        ...(extraStubs ?? {}),
      },
      ...restGlobal,
    },
    ...restOptions,
  })
}
