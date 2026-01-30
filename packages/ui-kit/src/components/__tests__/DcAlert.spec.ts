import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DcAlert from '../DcAlert.vue'

describe('DcAlert', () => {
  it('renders with default info variant', () => {
    const wrapper = mount(DcAlert, { slots: { default: 'Info message' } })
    expect(wrapper.text()).toContain('Info message')
    expect(wrapper.classes()).toContain('bg-blue-50')
  })

  it('has alert role', () => {
    const wrapper = mount(DcAlert)
    expect(wrapper.attributes('role')).toBe('alert')
  })

  it('applies success variant classes', () => {
    const wrapper = mount(DcAlert, { props: { variant: 'success' } })
    expect(wrapper.classes()).toContain('bg-green-50')
    expect(wrapper.classes()).toContain('text-green-800')
  })

  it('applies error variant classes', () => {
    const wrapper = mount(DcAlert, { props: { variant: 'error' } })
    expect(wrapper.classes()).toContain('bg-red-50')
    expect(wrapper.classes()).toContain('text-red-800')
  })

  it('applies warning variant classes', () => {
    const wrapper = mount(DcAlert, { props: { variant: 'warning' } })
    expect(wrapper.classes()).toContain('bg-yellow-50')
    expect(wrapper.classes()).toContain('text-yellow-800')
  })

  it('renders title when provided', () => {
    const wrapper = mount(DcAlert, { props: { title: 'Alert Title' } })
    expect(wrapper.find('h3').text()).toBe('Alert Title')
  })

  it('does not render title when not provided', () => {
    const wrapper = mount(DcAlert)
    expect(wrapper.find('h3').exists()).toBe(false)
  })

  it('shows dismiss button when dismissible', () => {
    const wrapper = mount(DcAlert, { props: { dismissible: true } })
    expect(wrapper.find('button').exists()).toBe(true)
    expect(wrapper.find('.sr-only').text()).toBe('Dismiss')
  })

  it('does not show dismiss button by default', () => {
    const wrapper = mount(DcAlert)
    expect(wrapper.find('button').exists()).toBe(false)
  })

  it('emits dismiss when dismiss button clicked', async () => {
    const wrapper = mount(DcAlert, { props: { dismissible: true } })
    await wrapper.find('button').trigger('click')
    expect(wrapper.emitted('dismiss')).toHaveLength(1)
  })

  it('renders correct icon per variant', () => {
    const successWrapper = mount(DcAlert, { props: { variant: 'success' } })
    expect(successWrapper.find('svg').exists()).toBe(true)

    const errorWrapper = mount(DcAlert, { props: { variant: 'error' } })
    expect(errorWrapper.find('svg').exists()).toBe(true)
  })

  it('renders slot content', () => {
    const wrapper = mount(DcAlert, { slots: { default: 'Custom body text' } })
    expect(wrapper.text()).toContain('Custom body text')
  })
})
