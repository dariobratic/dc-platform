# @dc-platform/ui-kit

Shared Vue 3 component library for DC Platform frontend applications.

## Description

A collection of reusable, accessible UI components built with Vue 3 Composition API, TypeScript, and Tailwind CSS. These components provide a consistent design system across all DC Platform frontend apps.

## Installation

Within the monorepo, add as a dependency:

```bash
pnpm add @dc-platform/ui-kit --filter <your-app>
```

Or add to your app's `package.json`:

```json
{
  "dependencies": {
    "@dc-platform/ui-kit": "workspace:*"
  }
}
```

## Requirements

- Vue 3.5+
- Tailwind CSS configured in consuming application
- TypeScript (strict mode recommended)

## Usage

Import components from the package:

```vue
<script setup lang="ts">
import { DcButton, DcInput, DcModal } from '@dc-platform/ui-kit'
import { ref } from 'vue'

const email = ref('')
const showModal = ref(false)
</script>

<template>
  <div>
    <DcInput
      v-model="email"
      type="email"
      label="Email Address"
      placeholder="you@example.com"
    />
    <DcButton variant="primary" @click="showModal = true">
      Open Modal
    </DcButton>
    <DcModal :open="showModal" title="Example Modal" @close="showModal = false">
      <p>Modal content goes here.</p>
    </DcModal>
  </div>
</template>
```

## Components

### DcButton

A versatile button component with multiple variants and sizes.

**Props:**

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `variant` | `'primary' \| 'secondary' \| 'danger' \| 'ghost'` | `'primary'` | Visual style variant |
| `size` | `'sm' \| 'md' \| 'lg'` | `'md'` | Button size |
| `disabled` | `boolean` | `false` | Disables the button |
| `loading` | `boolean` | `false` | Shows spinner and disables button |
| `type` | `'button' \| 'submit' \| 'reset'` | `'button'` | HTML button type |

**Emits:**
- `click(event: MouseEvent)` - Emitted when button is clicked

**Slots:**
- `default` - Button content

### DcInput

Text input field with label, validation states, and helper text.

**Props:**

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `modelValue` | `string` | `''` | Input value (v-model) |
| `type` | `'text' \| 'email' \| 'password' \| 'number' \| 'tel' \| 'url'` | `'text'` | Input type |
| `label` | `string` | - | Label text |
| `placeholder` | `string` | - | Placeholder text |
| `helperText` | `string` | - | Helper text below input |
| `error` | `string` | - | Error message (also triggers error styling) |
| `success` | `boolean` | `false` | Shows success state styling |
| `disabled` | `boolean` | `false` | Disables the input |
| `required` | `boolean` | `false` | Marks field as required |
| `id` | `string` | auto-generated | HTML input id |

**Emits:**
- `update:modelValue(value: string)` - Emitted on input change

### DcSelect

Native select dropdown with label and validation states.

**Props:**

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `modelValue` | `string \| number \| null` | `null` | Selected value (v-model) |
| `options` | `Array<{ value: string \| number; label: string; disabled?: boolean }>` | required | Select options |
| `label` | `string` | - | Label text |
| `placeholder` | `string` | `'Select an option'` | Placeholder text |
| `error` | `string` | - | Error message |
| `helperText` | `string` | - | Helper text below select |
| `disabled` | `boolean` | `false` | Disables the select |
| `required` | `boolean` | `false` | Marks field as required |
| `id` | `string` | auto-generated | HTML select id |

**Emits:**
- `update:modelValue(value: string | number | null)` - Emitted on selection change

### DcModal

Modal dialog with backdrop, transitions, and slots.

**Props:**

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `open` | `boolean` | required | Controls modal visibility |
| `title` | `string` | - | Modal title |
| `size` | `'sm' \| 'md' \| 'lg' \| 'xl'` | `'md'` | Modal width |
| `persistent` | `boolean` | `false` | Prevents closing on backdrop click |

**Emits:**
- `close()` - Emitted when modal should close

**Slots:**
- `header` - Custom header content (default shows title)
- `default` - Modal body content
- `footer` - Modal footer content

**Features:**
- Closes on Escape key
- Closes on backdrop click (unless persistent)
- Teleports to body
- Smooth transitions

### DcSpinner

Animated loading spinner.

**Props:**

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `size` | `'sm' \| 'md' \| 'lg'` | `'md'` | Spinner size |
| `color` | `string` | `'text-indigo-600'` | Tailwind color class |

### DcAlert

Alert/notification component with icons and variants.

**Props:**

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `variant` | `'success' \| 'error' \| 'warning' \| 'info'` | `'info'` | Alert type |
| `dismissible` | `boolean` | `false` | Shows close button |
| `title` | `string` | - | Alert title |

**Emits:**
- `dismiss()` - Emitted when close button is clicked

**Slots:**
- `default` - Alert message content

### DcCard

Container card with optional header and footer.

**Props:**

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `padding` | `boolean` | `true` | Adds padding to body |

**Slots:**
- `header` - Card header content
- `default` - Card body content
- `footer` - Card footer content

### DcBadge

Small badge for status indicators and labels.

**Props:**

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `variant` | `'default' \| 'success' \| 'error' \| 'warning' \| 'info'` | `'default'` | Badge color variant |
| `size` | `'sm' \| 'md'` | `'md'` | Badge size |
| `dot` | `boolean` | `false` | Shows colored dot before text |

**Slots:**
- `default` - Badge text content

## Styling

All components use Tailwind CSS utility classes exclusively. The consuming application must have Tailwind CSS configured.

### Color Scheme

Primary color is indigo (indigo-600, indigo-700). This can be customized via your Tailwind config:

```js
// tailwind.config.js
export default {
  theme: {
    extend: {
      colors: {
        primary: {
          // Override with your brand colors
          600: '#your-color',
          700: '#your-darker-color',
        },
      },
    },
  },
}
```

### Accessibility

Components follow accessibility best practices:
- Semantic HTML elements
- ARIA attributes where needed
- Keyboard navigation support
- Focus management
- Screen reader labels

## Development

Type-check the package:

```bash
cd packages/ui-kit
pnpm exec tsc --noEmit
```

## Integration

### With Shell App

```vue
<script setup lang="ts">
import { DcButton, DcCard } from '@dc-platform/ui-kit'
</script>

<template>
  <DcCard>
    <template #header>
      <h2>Dashboard</h2>
    </template>
    <p>Welcome to DC Platform</p>
    <template #footer>
      <DcButton variant="primary">Get Started</DcButton>
    </template>
  </DcCard>
</template>
```

### With Admin/Client Apps

Same import pattern works across all microfrontend applications in the monorepo.

## TypeScript

All components are fully typed with TypeScript. Props and emits are typed for IDE autocomplete and type safety.

```typescript
import type { Component } from 'vue'
import { DcButton } from '@dc-platform/ui-kit'

const ButtonComponent: Component = DcButton
```

## Notes

- This package has no build step - consuming apps import TypeScript source directly
- Components are pure presentational UI - no API calls, no store access, no business logic
- All component names are prefixed with `Dc` to avoid naming conflicts
- Uses named exports exclusively (no default exports except Vue SFCs)
