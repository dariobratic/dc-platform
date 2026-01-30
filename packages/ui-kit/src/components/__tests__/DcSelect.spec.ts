import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DcSelect from '../DcSelect.vue'

const options = [
  { value: '1', label: 'Option 1' },
  { value: '2', label: 'Option 2' },
  { value: '3', label: 'Option 3', disabled: true },
]

describe('DcSelect', () => {
  it('renders with options', () => {
    const wrapper = mount(DcSelect, { props: { options } })
    const optionElements = wrapper.findAll('option')
    // 3 options + 1 placeholder
    expect(optionElements).toHaveLength(4)
  })

  it('renders label when provided', () => {
    const wrapper = mount(DcSelect, { props: { options, label: 'Category' } })
    expect(wrapper.find('label').text()).toContain('Category')
  })

  it('shows required asterisk', () => {
    const wrapper = mount(DcSelect, { props: { options, label: 'Category', required: true } })
    expect(wrapper.find('label').text()).toContain('*')
  })

  it('renders placeholder', () => {
    const wrapper = mount(DcSelect, { props: { options, placeholder: 'Pick one' } })
    const placeholder = wrapper.find('option[disabled]')
    expect(placeholder.text()).toBe('Pick one')
  })

  it('emits update:modelValue on change', async () => {
    const wrapper = mount(DcSelect, { props: { options, modelValue: null } })
    await wrapper.find('select').setValue('1')
    expect(wrapper.emitted('update:modelValue')).toBeTruthy()
    expect(wrapper.emitted('update:modelValue')![0]).toEqual(['1'])
  })

  it('shows error message', () => {
    const wrapper = mount(DcSelect, { props: { options, error: 'Required' } })
    expect(wrapper.text()).toContain('Required')
    expect(wrapper.find('.text-red-600').exists()).toBe(true)
  })

  it('shows helper text', () => {
    const wrapper = mount(DcSelect, { props: { options, helperText: 'Choose wisely' } })
    expect(wrapper.text()).toContain('Choose wisely')
  })

  it('applies error border classes', () => {
    const wrapper = mount(DcSelect, { props: { options, error: 'Error' } })
    expect(wrapper.find('select').classes()).toContain('border-red-300')
  })

  it('applies disabled state', () => {
    const wrapper = mount(DcSelect, { props: { options, disabled: true } })
    expect(wrapper.find('select').attributes('disabled')).toBeDefined()
    expect(wrapper.find('select').classes()).toContain('bg-gray-50')
  })

  it('disables individual options', () => {
    const wrapper = mount(DcSelect, { props: { options } })
    const optionEls = wrapper.findAll('option')
    // Fourth option (index 3) is the disabled one
    expect(optionEls[3].attributes('disabled')).toBeDefined()
  })
})
