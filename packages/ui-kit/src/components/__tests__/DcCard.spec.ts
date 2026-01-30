import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DcCard from '../DcCard.vue'

describe('DcCard', () => {
  it('renders default slot content', () => {
    const wrapper = mount(DcCard, { slots: { default: 'Card body' } })
    expect(wrapper.text()).toContain('Card body')
  })

  it('has padding by default', () => {
    const wrapper = mount(DcCard, { slots: { default: 'Content' } })
    expect(wrapper.find('.p-6').exists()).toBe(true)
  })

  it('removes padding when padding is false', () => {
    const wrapper = mount(DcCard, {
      props: { padding: false },
      slots: { default: 'Content' },
    })
    expect(wrapper.find('.p-6').exists()).toBe(false)
  })

  it('renders header slot', () => {
    const wrapper = mount(DcCard, {
      slots: {
        header: '<h2>Header</h2>',
        default: 'Body',
      },
    })
    expect(wrapper.text()).toContain('Header')
    expect(wrapper.text()).toContain('Body')
  })

  it('does not render header section when no header slot', () => {
    const wrapper = mount(DcCard, { slots: { default: 'Body' } })
    expect(wrapper.find('.border-b').exists()).toBe(false)
  })

  it('renders footer slot', () => {
    const wrapper = mount(DcCard, {
      slots: {
        default: 'Body',
        footer: '<button>Action</button>',
      },
    })
    expect(wrapper.text()).toContain('Action')
  })

  it('does not render footer section when no footer slot', () => {
    const wrapper = mount(DcCard, { slots: { default: 'Body' } })
    expect(wrapper.find('.bg-gray-50').exists()).toBe(false)
  })

  it('has card styling classes', () => {
    const wrapper = mount(DcCard, { slots: { default: 'Body' } })
    expect(wrapper.classes()).toContain('bg-white')
    expect(wrapper.classes()).toContain('rounded-lg')
    expect(wrapper.classes()).toContain('shadow-sm')
  })
})
