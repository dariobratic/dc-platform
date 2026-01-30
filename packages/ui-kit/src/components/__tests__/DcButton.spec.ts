import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DcButton from '../DcButton.vue'

describe('DcButton', () => {
  it('renders with default props', () => {
    const wrapper = mount(DcButton, { slots: { default: 'Click me' } })
    expect(wrapper.text()).toContain('Click me')
    expect(wrapper.find('button').exists()).toBe(true)
  })

  it('applies primary variant classes by default', () => {
    const wrapper = mount(DcButton)
    expect(wrapper.classes()).toContain('bg-indigo-600')
    expect(wrapper.classes()).toContain('text-white')
  })

  it('applies secondary variant classes', () => {
    const wrapper = mount(DcButton, { props: { variant: 'secondary' } })
    expect(wrapper.classes()).toContain('bg-white')
    expect(wrapper.classes()).toContain('text-gray-700')
  })

  it('applies danger variant classes', () => {
    const wrapper = mount(DcButton, { props: { variant: 'danger' } })
    expect(wrapper.classes()).toContain('bg-red-600')
    expect(wrapper.classes()).toContain('text-white')
  })

  it('applies ghost variant classes', () => {
    const wrapper = mount(DcButton, { props: { variant: 'ghost' } })
    expect(wrapper.classes()).toContain('bg-transparent')
    expect(wrapper.classes()).toContain('text-gray-700')
  })

  it('applies size sm classes', () => {
    const wrapper = mount(DcButton, { props: { size: 'sm' } })
    expect(wrapper.classes()).toContain('px-3')
    expect(wrapper.classes()).toContain('py-1.5')
  })

  it('applies size md classes by default', () => {
    const wrapper = mount(DcButton)
    expect(wrapper.classes()).toContain('px-4')
    expect(wrapper.classes()).toContain('py-2')
  })

  it('applies size lg classes', () => {
    const wrapper = mount(DcButton, { props: { size: 'lg' } })
    expect(wrapper.classes()).toContain('px-6')
    expect(wrapper.classes()).toContain('py-3')
  })

  it('emits click event when clicked', async () => {
    const wrapper = mount(DcButton)
    await wrapper.trigger('click')
    expect(wrapper.emitted('click')).toHaveLength(1)
  })

  it('does not emit click when disabled', async () => {
    const wrapper = mount(DcButton, { props: { disabled: true } })
    await wrapper.trigger('click')
    expect(wrapper.emitted('click')).toBeUndefined()
  })

  it('does not emit click when loading', async () => {
    const wrapper = mount(DcButton, { props: { loading: true } })
    await wrapper.trigger('click')
    expect(wrapper.emitted('click')).toBeUndefined()
  })

  it('shows spinner when loading', () => {
    const wrapper = mount(DcButton, { props: { loading: true } })
    expect(wrapper.find('svg').exists()).toBe(true)
  })

  it('sets disabled attribute when disabled', () => {
    const wrapper = mount(DcButton, { props: { disabled: true } })
    expect(wrapper.find('button').attributes('disabled')).toBeDefined()
  })

  it('sets disabled attribute when loading', () => {
    const wrapper = mount(DcButton, { props: { loading: true } })
    expect(wrapper.find('button').attributes('disabled')).toBeDefined()
  })

  it('applies opacity-50 when disabled', () => {
    const wrapper = mount(DcButton, { props: { disabled: true } })
    expect(wrapper.classes().join(' ')).toContain('opacity-50')
  })

  it('sets button type attribute', () => {
    const wrapper = mount(DcButton, { props: { type: 'submit' } })
    expect(wrapper.find('button').attributes('type')).toBe('submit')
  })

  it('defaults to button type', () => {
    const wrapper = mount(DcButton)
    expect(wrapper.find('button').attributes('type')).toBe('button')
  })
})
