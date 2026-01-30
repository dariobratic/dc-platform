import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DcInput from '../DcInput.vue'

describe('DcInput', () => {
  it('renders with default props', () => {
    const wrapper = mount(DcInput)
    expect(wrapper.find('input').exists()).toBe(true)
    expect(wrapper.find('input').attributes('type')).toBe('text')
  })

  it('renders label when provided', () => {
    const wrapper = mount(DcInput, { props: { label: 'Email' } })
    expect(wrapper.find('label').text()).toContain('Email')
  })

  it('does not render label when not provided', () => {
    const wrapper = mount(DcInput)
    expect(wrapper.find('label').exists()).toBe(false)
  })

  it('shows required asterisk when required', () => {
    const wrapper = mount(DcInput, { props: { label: 'Name', required: true } })
    expect(wrapper.find('label').text()).toContain('*')
  })

  it('sets placeholder', () => {
    const wrapper = mount(DcInput, { props: { placeholder: 'Enter email' } })
    expect(wrapper.find('input').attributes('placeholder')).toBe('Enter email')
  })

  it('binds modelValue to input value', () => {
    const wrapper = mount(DcInput, { props: { modelValue: 'test@example.com' } })
    expect(wrapper.find('input').element.value).toBe('test@example.com')
  })

  it('emits update:modelValue on input', async () => {
    const wrapper = mount(DcInput, { props: { modelValue: '' } })
    await wrapper.find('input').setValue('hello')
    expect(wrapper.emitted('update:modelValue')).toBeTruthy()
    expect(wrapper.emitted('update:modelValue')![0]).toEqual(['hello'])
  })

  it('shows error message', () => {
    const wrapper = mount(DcInput, { props: { error: 'Required field' } })
    expect(wrapper.text()).toContain('Required field')
    expect(wrapper.find('.text-red-600').exists()).toBe(true)
  })

  it('shows helper text when no error', () => {
    const wrapper = mount(DcInput, { props: { helperText: 'Enter your email' } })
    expect(wrapper.text()).toContain('Enter your email')
    expect(wrapper.find('.text-gray-500').exists()).toBe(true)
  })

  it('shows error over helper text', () => {
    const wrapper = mount(DcInput, { props: { error: 'Error', helperText: 'Help' } })
    expect(wrapper.text()).toContain('Error')
    expect(wrapper.text()).not.toContain('Help')
  })

  it('applies error border classes', () => {
    const wrapper = mount(DcInput, { props: { error: 'Error' } })
    expect(wrapper.find('input').classes()).toContain('border-red-300')
  })

  it('applies success border classes', () => {
    const wrapper = mount(DcInput, { props: { success: true } })
    expect(wrapper.find('input').classes()).toContain('border-green-300')
  })

  it('applies disabled state', () => {
    const wrapper = mount(DcInput, { props: { disabled: true } })
    expect(wrapper.find('input').attributes('disabled')).toBeDefined()
    expect(wrapper.find('input').classes()).toContain('bg-gray-50')
  })

  it('sets input type', () => {
    const wrapper = mount(DcInput, { props: { type: 'password' } })
    expect(wrapper.find('input').attributes('type')).toBe('password')
  })

  it('links label to input via id', () => {
    const wrapper = mount(DcInput, { props: { id: 'my-input', label: 'Name' } })
    expect(wrapper.find('label').attributes('for')).toBe('my-input')
    expect(wrapper.find('input').attributes('id')).toBe('my-input')
  })
})
