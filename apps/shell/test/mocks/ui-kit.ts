import { defineComponent, h } from 'vue'

export const DcCard = defineComponent({
  name: 'DcCard',
  setup(_, { slots }) {
    return () => h('div', { class: 'dc-card' }, [
      slots.default?.(),
      slots.footer?.(),
    ])
  },
})

export const DcInput = defineComponent({
  name: 'DcInput',
  props: {
    modelValue: String,
    type: { type: String, default: 'text' },
    label: String,
    placeholder: String,
    required: Boolean,
    disabled: Boolean,
    error: String,
  },
  emits: ['update:modelValue'],
  setup(props, { emit }) {
    return () => h('input', {
      type: props.type,
      value: props.modelValue,
      onInput: (e: Event) => emit('update:modelValue', (e.target as HTMLInputElement).value),
    })
  },
})

export const DcButton = defineComponent({
  name: 'DcButton',
  props: {
    type: { type: String, default: 'button' },
    variant: String,
    size: String,
    loading: Boolean,
    disabled: Boolean,
  },
  setup(props, { slots }) {
    return () => h('button', {
      type: props.type,
      disabled: props.disabled || props.loading,
    }, slots.default?.())
  },
})

export const DcAlert = defineComponent({
  name: 'DcAlert',
  props: {
    variant: String,
    dismissible: Boolean,
  },
  emits: ['dismiss'],
  setup(props, { slots, emit }) {
    return () => h('div', {
      class: `dc-alert dc-alert-${props.variant}`,
      onClick: () => props.dismissible && emit('dismiss'),
    }, slots.default?.())
  },
})

export const DcSpinner = defineComponent({
  name: 'DcSpinner',
  setup() {
    return () => h('div', { class: 'dc-spinner' }, 'Loading...')
  },
})
