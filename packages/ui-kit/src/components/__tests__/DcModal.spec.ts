import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DcModal from '../DcModal.vue'

describe('DcModal', () => {
  it('does not render content when closed', () => {
    const wrapper = mount(DcModal, {
      props: { open: false },
      slots: { default: 'Modal content' },
    })
    expect(wrapper.text()).not.toContain('Modal content')
  })

  it('renders content when open', () => {
    const wrapper = mount(DcModal, {
      props: { open: true },
      slots: { default: 'Modal content' },
      global: { stubs: { teleport: true } },
    })
    expect(wrapper.text()).toContain('Modal content')
  })

  it('renders title when provided', () => {
    const wrapper = mount(DcModal, {
      props: { open: true, title: 'My Modal' },
      global: { stubs: { teleport: true } },
    })
    expect(wrapper.text()).toContain('My Modal')
  })

  it('has dialog role', () => {
    const wrapper = mount(DcModal, {
      props: { open: true },
      global: { stubs: { teleport: true } },
    })
    expect(wrapper.find('[role="dialog"]').exists()).toBe(true)
  })

  it('has aria-modal attribute', () => {
    const wrapper = mount(DcModal, {
      props: { open: true },
      global: { stubs: { teleport: true } },
    })
    expect(wrapper.find('[aria-modal="true"]').exists()).toBe(true)
  })

  it('emits close when close button clicked', async () => {
    const wrapper = mount(DcModal, {
      props: { open: true, title: 'Test' },
      global: { stubs: { teleport: true } },
    })
    await wrapper.find('button').trigger('click')
    expect(wrapper.emitted('close')).toBeTruthy()
  })

  it('applies size sm classes', () => {
    const wrapper = mount(DcModal, {
      props: { open: true, size: 'sm' },
      global: { stubs: { teleport: true } },
    })
    expect(wrapper.find('[role="dialog"]').classes()).toContain('max-w-sm')
  })

  it('applies size lg classes', () => {
    const wrapper = mount(DcModal, {
      props: { open: true, size: 'lg' },
      global: { stubs: { teleport: true } },
    })
    expect(wrapper.find('[role="dialog"]').classes()).toContain('max-w-lg')
  })

  it('applies size xl classes', () => {
    const wrapper = mount(DcModal, {
      props: { open: true, size: 'xl' },
      global: { stubs: { teleport: true } },
    })
    expect(wrapper.find('[role="dialog"]').classes()).toContain('max-w-xl')
  })

  it('renders footer slot', () => {
    const wrapper = mount(DcModal, {
      props: { open: true },
      slots: { footer: '<button>Save</button>' },
      global: { stubs: { teleport: true } },
    })
    expect(wrapper.text()).toContain('Save')
  })

  it('renders header slot over title', () => {
    const wrapper = mount(DcModal, {
      props: { open: true, title: 'Default Title' },
      slots: { header: '<span>Custom Header</span>' },
      global: { stubs: { teleport: true } },
    })
    expect(wrapper.text()).toContain('Custom Header')
  })
})
