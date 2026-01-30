import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DcSpinner from '../DcSpinner.vue'

describe('DcSpinner', () => {
  it('renders svg element', () => {
    const wrapper = mount(DcSpinner)
    expect(wrapper.find('svg').exists()).toBe(true)
  })

  it('applies animate-spin class', () => {
    const wrapper = mount(DcSpinner)
    expect(wrapper.find('svg').classes()).toContain('animate-spin')
  })

  it('applies sm size', () => {
    const wrapper = mount(DcSpinner, { props: { size: 'sm' } })
    expect(wrapper.find('svg').classes()).toContain('h-4')
    expect(wrapper.find('svg').classes()).toContain('w-4')
  })

  it('applies md size by default', () => {
    const wrapper = mount(DcSpinner)
    expect(wrapper.find('svg').classes()).toContain('h-6')
    expect(wrapper.find('svg').classes()).toContain('w-6')
  })

  it('applies lg size', () => {
    const wrapper = mount(DcSpinner, { props: { size: 'lg' } })
    expect(wrapper.find('svg').classes()).toContain('h-8')
    expect(wrapper.find('svg').classes()).toContain('w-8')
  })

  it('applies default color', () => {
    const wrapper = mount(DcSpinner)
    expect(wrapper.find('svg').classes()).toContain('text-indigo-600')
  })

  it('applies custom color', () => {
    const wrapper = mount(DcSpinner, { props: { color: 'text-red-500' } })
    expect(wrapper.find('svg').classes()).toContain('text-red-500')
  })

  it('renders circle and path elements', () => {
    const wrapper = mount(DcSpinner)
    expect(wrapper.find('circle').exists()).toBe(true)
    expect(wrapper.find('path').exists()).toBe(true)
  })
})
