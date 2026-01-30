import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DcBadge from '../DcBadge.vue'

describe('DcBadge', () => {
  it('renders with default variant', () => {
    const wrapper = mount(DcBadge, { slots: { default: 'Active' } })
    expect(wrapper.text()).toContain('Active')
    expect(wrapper.classes()).toContain('bg-gray-100')
    expect(wrapper.classes()).toContain('text-gray-800')
  })

  it('applies success variant', () => {
    const wrapper = mount(DcBadge, { props: { variant: 'success' } })
    expect(wrapper.classes()).toContain('bg-green-100')
    expect(wrapper.classes()).toContain('text-green-800')
  })

  it('applies error variant', () => {
    const wrapper = mount(DcBadge, { props: { variant: 'error' } })
    expect(wrapper.classes()).toContain('bg-red-100')
    expect(wrapper.classes()).toContain('text-red-800')
  })

  it('applies warning variant', () => {
    const wrapper = mount(DcBadge, { props: { variant: 'warning' } })
    expect(wrapper.classes()).toContain('bg-yellow-100')
    expect(wrapper.classes()).toContain('text-yellow-800')
  })

  it('applies info variant', () => {
    const wrapper = mount(DcBadge, { props: { variant: 'info' } })
    expect(wrapper.classes()).toContain('bg-blue-100')
    expect(wrapper.classes()).toContain('text-blue-800')
  })

  it('applies sm size classes', () => {
    const wrapper = mount(DcBadge, { props: { size: 'sm' } })
    expect(wrapper.classes()).toContain('px-2')
    expect(wrapper.classes()).toContain('text-xs')
  })

  it('applies md size classes by default', () => {
    const wrapper = mount(DcBadge)
    expect(wrapper.classes()).toContain('px-2.5')
    expect(wrapper.classes()).toContain('text-sm')
  })

  it('shows dot indicator when dot prop is true', () => {
    const wrapper = mount(DcBadge, { props: { dot: true } })
    expect(wrapper.find('.rounded-full.w-1\\.5').exists()).toBe(true)
  })

  it('does not show dot by default', () => {
    const wrapper = mount(DcBadge)
    expect(wrapper.findAll('span').length).toBe(1)
  })

  it('applies correct dot color for variant', () => {
    const wrapper = mount(DcBadge, { props: { dot: true, variant: 'success' } })
    const dotEl = wrapper.findAll('span')[1]
    expect(dotEl.classes()).toContain('bg-green-600')
  })
})
