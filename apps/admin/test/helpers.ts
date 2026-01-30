import { mount, type ComponentMountingOptions } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import { type Component, defineComponent, h } from 'vue'

// Stubs for ui-kit components
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

const DcInput = defineComponent({
  name: 'DcInput',
  props: { modelValue: String, label: String, placeholder: String, error: String, type: String, disabled: Boolean, required: Boolean },
  emits: ['update:modelValue'],
  template: '<div data-testid="dc-input"><input :value="modelValue" @input="$emit(\'update:modelValue\', $event.target.value)" /></div>',
})

const DcModal = defineComponent({
  name: 'DcModal',
  props: { open: Boolean, title: String, size: String },
  emits: ['close'],
  template: '<div v-if="open" data-testid="dc-modal"><slot /><slot name="footer" /></div>',
})

const DcCard = defineComponent({
  name: 'DcCard',
  props: { padding: Boolean },
  template: '<div data-testid="dc-card"><slot name="header" /><slot /><slot name="footer" /></div>',
})

const DcSelect = defineComponent({
  name: 'DcSelect',
  props: { modelValue: [String, Number], options: Array, label: String },
  emits: ['update:modelValue'],
  template: '<div data-testid="dc-select"><select @change="$emit(\'update:modelValue\', $event.target.value)"></select></div>',
})

const DcBadge = defineComponent({
  name: 'DcBadge',
  props: { variant: String, size: String, dot: Boolean },
  template: '<span data-testid="dc-badge"><slot /></span>',
})

// Stubs for admin components
const PageHeader = defineComponent({
  name: 'PageHeader',
  props: { title: String, description: String },
  template: '<div data-testid="page-header"><h1>{{ title }}</h1><slot name="actions" /></div>',
})

const StatsCard = defineComponent({
  name: 'StatsCard',
  props: { title: String, value: [String, Number] },
  template: '<div data-testid="stats-card">{{ title }}: {{ value }}</div>',
})

const StatusBadge = defineComponent({
  name: 'StatusBadge',
  props: { status: String },
  template: '<span data-testid="status-badge">{{ status }}</span>',
})

const EmptyState = defineComponent({
  name: 'EmptyState',
  props: { title: String, description: String },
  template: '<div data-testid="empty-state">{{ title }}<slot name="action" /></div>',
})

const ConfirmDialog = defineComponent({
  name: 'ConfirmDialog',
  props: { open: Boolean, title: String, message: String, confirmLabel: String, variant: String, loading: Boolean },
  emits: ['confirm', 'cancel'],
  template: '<div v-if="open" data-testid="confirm-dialog">{{ message }}</div>',
})

export const defaultStubs = {
  DcSpinner,
  DcAlert,
  DcButton,
  DcInput,
  DcModal,
  DcCard,
  DcSelect,
  DcBadge,
  PageHeader,
  StatsCard,
  StatusBadge,
  EmptyState,
  ConfirmDialog,
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
