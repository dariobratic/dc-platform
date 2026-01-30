# @dc-platform/ui-kit - Claude Agent Instructions

Shared Vue 3 component library for DC Platform. Pure presentational UI components with no business logic.

## Package Scope

**This package contains ONLY Vue 3 presentational components:**
- Reusable UI components (buttons, inputs, modals, cards, etc.)
- Built with Vue 3 Composition API (`<script setup lang="ts">`)
- Styled exclusively with Tailwind CSS (no custom CSS)
- TypeScript strict mode with full type safety
- Zero business logic, no API calls, no store access
- Minimal dependencies (only Vue 3)

## File Structure

```
packages/ui-kit/
├── src/
│   ├── components/
│   │   ├── DcAlert.vue
│   │   ├── DcBadge.vue
│   │   ├── DcButton.vue
│   │   ├── DcCard.vue
│   │   ├── DcInput.vue
│   │   ├── DcModal.vue
│   │   ├── DcSelect.vue
│   │   └── DcSpinner.vue
│   └── index.ts             # Barrel export all components
├── package.json
├── tsconfig.json
├── README.md
└── CLAUDE.md
```

## Component Conventions

### Vue 3 SFC Pattern

Every component MUST follow this exact pattern:

```vue
<script setup lang="ts">
import { computed, ref } from 'vue'

interface Props {
  variant?: 'primary' | 'secondary'  // Optional with default
  label: string                       // Required
  disabled?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'primary',
  disabled: false,
})

const emit = defineEmits<{
  click: [event: MouseEvent]          // Typed emit
  change: [value: string]
}>()

// Component logic here
const computedClass = computed(() => {
  return props.variant === 'primary' ? 'bg-indigo-600' : 'bg-gray-600'
})
</script>

<template>
  <div :class="computedClass">
    <slot />
  </div>
</template>
```

### TypeScript Strict Typing

**Props:**
```typescript
// Use interface for props, NOT type alias
interface Props {
  // Required prop
  id: string

  // Optional prop with default
  size?: 'sm' | 'md' | 'lg'

  // Optional prop without default
  label?: string

  // Union types for variants
  variant?: 'primary' | 'secondary' | 'danger'

  // Complex types
  options: Array<{ value: string; label: string }>
}

// Always use withDefaults for optional props
const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  variant: 'primary',
})
```

**Emits:**
```typescript
// Typed emits with event payload types
const emit = defineEmits<{
  click: [event: MouseEvent]
  change: [value: string]
  submit: [data: { name: string; email: string }]
  close: []  // No payload
}>()

// Usage
emit('change', 'new value')
emit('close')
```

**Computed:**
```typescript
// Always type computed return values when not obvious
const isDisabled = computed<boolean>(() => props.disabled || props.loading)
const classes = computed<string>(() => `base ${props.variant}`)
```

### Tailwind CSS Only

**NO `<style>` blocks allowed.** All styling via Tailwind utilities.

**Color Palette:**
- Primary: `indigo-50` through `indigo-900` (focus: `indigo-500`)
- Secondary: `gray-50` through `gray-900`
- Success: `green-50` through `green-900`
- Error/Danger: `red-50` through `red-900`
- Warning: `yellow-50` through `yellow-900`
- Info: `blue-50` through `blue-900`

**Common Patterns:**

```vue
<!-- Dynamic classes with computed -->
<button :class="[baseClasses, variantClasses, sizeClasses]">

<!-- Conditional classes -->
<div :class="['base-class', { 'active-class': isActive }]">

<!-- State-based classes -->
<input :class="error ? 'border-red-300' : 'border-gray-300'">
```

**Standard Class Patterns:**

```typescript
// Base classes for interactive elements
const baseClasses = 'inline-flex items-center justify-center font-medium rounded-md transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2'

// Button variants
const primaryClasses = 'bg-indigo-600 hover:bg-indigo-700 text-white focus:ring-indigo-500'
const secondaryClasses = 'bg-white border border-gray-300 text-gray-700 hover:bg-gray-50'
const dangerClasses = 'bg-red-600 hover:bg-red-700 text-white focus:ring-red-500'

// Input base
const inputBase = 'block w-full rounded-md border px-3 py-2 text-sm shadow-sm transition-colors focus:outline-none focus:ring-2 focus:ring-offset-0'
const inputDefault = 'border-gray-300 focus:border-indigo-500 focus:ring-indigo-500'
const inputError = 'border-red-300 focus:border-red-500 focus:ring-red-500'
```

### Slot Patterns

```vue
<!-- Default slot -->
<template>
  <div class="wrapper">
    <slot />  <!-- Default content -->
  </div>
</template>

<!-- Named slots -->
<template>
  <div class="card">
    <div v-if="$slots.header" class="header">
      <slot name="header" />
    </div>
    <div class="body">
      <slot />  <!-- Default slot for body -->
    </div>
    <div v-if="$slots.footer" class="footer">
      <slot name="footer" />
    </div>
  </div>
</template>

<!-- Slot with fallback -->
<template>
  <div>
    <slot name="title">
      <h2>{{ title }}</h2>  <!-- Fallback if slot not provided -->
    </slot>
  </div>
</template>

<!-- Conditional rendering based on slot presence -->
<div v-if="$slots.actions">
  <slot name="actions" />
</div>
```

### v-model Pattern

For two-way binding components (inputs, selects):

```vue
<script setup lang="ts">
interface Props {
  modelValue: string  // v-model binding
}

defineProps<Props>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const handleInput = (event: Event) => {
  const target = event.target as HTMLInputElement
  emit('update:modelValue', target.value)
}
</script>

<template>
  <input
    :value="modelValue"
    @input="handleInput"
  />
</template>

<!-- Usage in parent -->
<DcInput v-model="email" />
```

### Teleport Pattern (Modals, Overlays)

```vue
<template>
  <Teleport to="body">
    <div v-if="open" class="fixed inset-0 z-50">
      <!-- Modal content -->
    </div>
  </Teleport>
</template>
```

### Transition Pattern

```vue
<template>
  <Transition
    enter-active-class="transition-opacity duration-200"
    enter-from-class="opacity-0"
    enter-to-class="opacity-100"
    leave-active-class="transition-opacity duration-200"
    leave-from-class="opacity-100"
    leave-to-class="opacity-0"
  >
    <div v-if="show">
      <!-- Animated content -->
    </div>
  </Transition>
</template>
```

## Adding New Components

### Step-by-Step Process

1. **Create the Vue SFC** in `src/components/`
   - Filename: `Dc<ComponentName>.vue` (PascalCase with `Dc` prefix)
   - Use `<script setup lang="ts">`
   - Define Props interface
   - Define Emits type
   - No `<style>` block

2. **Implement component logic**
   - Computed classes for variants/states
   - Event handlers with proper typing
   - Accessibility attributes (ARIA, roles)
   - Keyboard navigation if interactive

3. **Add barrel export** to `src/index.ts`
   ```typescript
   export { default as DcNewComponent } from './components/DcNewComponent.vue'
   ```

4. **Document in README.md**
   - Add to component list
   - Props table
   - Emits list
   - Slots list
   - Usage example

5. **Type-check**
   ```bash
   cd packages/ui-kit
   pnpm exec tsc --noEmit
   ```

### Component Checklist

Before considering a component complete, verify:

- [ ] Props interface defined with proper types
- [ ] Emits typed (even if empty: `defineEmits<{}>()`)
- [ ] Uses Tailwind classes only (no `<style>`)
- [ ] Proper TypeScript strict mode (no `any`)
- [ ] Accessibility attributes where needed
- [ ] Keyboard navigation for interactive elements
- [ ] Focus management (focus rings, focus trapping in modals)
- [ ] Responsive design (mobile-first Tailwind breakpoints)
- [ ] Error/loading/disabled states handled
- [ ] Exported from `src/index.ts`
- [ ] Documented in README.md

## What Belongs Here

### YES - Include in ui-kit

- Buttons, links, navigation elements
- Form inputs, selects, checkboxes, radios
- Modals, dialogs, overlays
- Cards, panels, containers
- Badges, tags, labels
- Alerts, notifications, toasts
- Loading spinners, skeletons, progress bars
- Tables, lists, grids (presentational only)
- Icons (SVG components)
- Tooltips, popovers, dropdowns
- Tabs, accordions, collapsible sections

### NO - Keep Elsewhere

- **Business logic components** → App-specific `src/components/`
- **Page-level components** → `apps/*/src/pages/`
- **Layout components** → `apps/shell/src/layouts/`
- **API-connected components** → App-specific with composables
- **Domain-specific forms** → App-specific (use ui-kit inputs as building blocks)
- **Navigation components** → Shell app (uses ui-kit primitives)
- **Auth-related UI** → Shell app auth pages
- **Tenant-specific UI** → App-specific

## Accessibility Requirements

Every interactive component MUST implement:

### Keyboard Navigation
- Buttons/links: Enter/Space to activate
- Modals: Escape to close, focus trap inside modal
- Dropdowns: Arrow keys for navigation
- Forms: Tab order, Enter to submit

### ARIA Attributes
```vue
<!-- Buttons -->
<button
  type="button"
  :aria-label="label"
  :aria-disabled="disabled"
  :aria-busy="loading"
>

<!-- Modals -->
<div
  role="dialog"
  aria-modal="true"
  :aria-labelledby="titleId"
>

<!-- Form fields -->
<input
  :id="inputId"
  :aria-describedby="error ? errorId : helperTextId"
  :aria-invalid="!!error"
  :aria-required="required"
>
```

### Focus Management
```typescript
// Focus trap in modal
onMounted(() => {
  if (props.open) {
    // Focus first focusable element
    const firstFocusable = modalRef.value?.querySelector('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])')
    firstFocusable?.focus()
  }
})

// Restore focus on close
onUnmounted(() => {
  previouslyFocusedElement?.focus()
})
```

### Screen Reader Support
- Use semantic HTML (`<button>` not `<div role="button">`)
- Provide text alternatives for icons
- Use `<label>` for form fields
- Add `aria-live` for dynamic content
- Use `sr-only` class for screen-reader-only text

## Integration with Consuming Apps

### Import Pattern

```typescript
// Named imports from barrel export
import { DcButton, DcInput, DcModal } from '@dc-platform/ui-kit'

// Individual component import (avoid, use barrel)
import DcButton from '@dc-platform/ui-kit/components/DcButton.vue'
```

### Styling Requirements

Consuming apps MUST have Tailwind CSS configured. Minimal required config:

```js
// tailwind.config.js
export default {
  content: [
    './index.html',
    './src/**/*.{vue,js,ts,jsx,tsx}',
    '../packages/ui-kit/src/**/*.vue',  // Include ui-kit components
  ],
  // ... rest of config
}
```

### Type Safety

Components export full TypeScript types:

```typescript
import type { Component } from 'vue'
import { DcButton } from '@dc-platform/ui-kit'

// Component type inference
const ButtonComponent: Component = DcButton

// Props type extraction
import type { ComponentProps } from 'vue-component-type-helpers'
type ButtonProps = ComponentProps<typeof DcButton>
```

## Build Strategy

**No build step.** Consuming apps import TypeScript source directly.

- `package.json` `exports` points to `.ts` and `.vue` source files
- Consuming apps compile via their own Vite/tsup configuration
- Benefits: Hot reload, tree-shaking, single compilation step
- Drawback: None in monorepo setup with workspace protocol

## Naming Conventions

### Component Names
- **File**: `Dc<ComponentName>.vue` (PascalCase, `Dc` prefix)
- **Export**: `DcComponentName` (matches filename)
- **Usage**: `<DcComponentName>` in templates

### Props
- `camelCase` for prop names
- Boolean props: `is*`, `has*`, `show*`, `disabled`, `loading`, `required`
- Event handler props: DO NOT use (use emits instead)

### Emits
- `kebab-case` in template usage
- `camelCase` in TypeScript emit definition
- Common: `click`, `change`, `input`, `submit`, `close`, `open`, `dismiss`

### Slots
- `default` for main content
- `kebab-case` for named slots: `header`, `footer`, `actions`, `icon`

### CSS Classes
- Use Tailwind utilities
- Computed class names: `*Classes` suffix
- State-based: `activeClasses`, `disabledClasses`, `errorClasses`

## Common Patterns

### Variant Pattern

```typescript
interface Props {
  variant?: 'primary' | 'secondary' | 'danger' | 'success'
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'primary',
})

const variantClasses = computed(() => {
  switch (props.variant) {
    case 'primary':
      return 'bg-indigo-600 text-white'
    case 'secondary':
      return 'bg-gray-600 text-white'
    case 'danger':
      return 'bg-red-600 text-white'
    case 'success':
      return 'bg-green-600 text-white'
    default:
      return 'bg-indigo-600 text-white'
  }
})
```

### Size Pattern

```typescript
interface Props {
  size?: 'sm' | 'md' | 'lg'
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
})

const sizeClasses = computed(() => {
  switch (props.size) {
    case 'sm':
      return 'px-3 py-1.5 text-sm'
    case 'md':
      return 'px-4 py-2 text-sm'
    case 'lg':
      return 'px-6 py-3 text-base'
    default:
      return 'px-4 py-2 text-sm'
  }
})
```

### Loading Pattern

```typescript
interface Props {
  loading?: boolean
  disabled?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  disabled: false,
})

const isInteractive = computed(() => !props.loading && !props.disabled)

const handleClick = (event: MouseEvent) => {
  if (isInteractive.value) {
    emit('click', event)
  }
}
```

### ID Generation Pattern

```typescript
import { computed } from 'vue'

interface Props {
  id?: string
}

const props = defineProps<Props>()

const componentId = computed(() =>
  props.id ?? `component-${Math.random().toString(36).substring(2, 11)}`
)
```

## What NOT to Do

- **NO Options API** - Use Composition API `<script setup>` exclusively
- **NO `<style>` blocks** - Tailwind CSS only
- **NO business logic** - Pure presentation, no API calls, no domain logic
- **NO store access** - No Pinia, no Vuex, no global state
- **NO routing** - No `vue-router` imports or route-aware logic
- **NO environment variables** - Components are environment-agnostic
- **NO default exports** - Named exports only (Vue SFCs are exception)
- **NO `any` type** - Use `unknown` and narrow, or define proper types
- **NO inline styles** - Use Tailwind classes or computed classes
- **NO global CSS** - Components must be self-contained
- **NO heavy dependencies** - Keep package light (only Vue 3)
- **NO framework-specific logic** - Pure Vue 3, no Nuxt/Vite/etc. assumptions

## Testing Strategy (Future)

When tests are added:

### Unit Tests (Vitest + @vue/test-utils)
- Props validation
- Emit behavior
- Computed properties
- Slot rendering
- Conditional logic

### Visual Tests (Storybook)
- All variants rendered
- All sizes rendered
- Interactive states (hover, focus, active)
- Error/success states
- Loading states

### Accessibility Tests (axe-core)
- ARIA attributes correct
- Keyboard navigation works
- Focus management proper
- Color contrast sufficient

## Commands

```bash
# Type-check the package
cd packages/ui-kit
pnpm exec tsc --noEmit

# Install dependencies
pnpm install

# Type-check from monorepo root
pnpm --filter @dc-platform/ui-kit exec tsc --noEmit

# No build, no dev server, no tests yet
```

## Decision Making

### When adding a new component, ask:

1. **Is this a pure UI component or domain-specific?**
   - Pure UI (button, input, card) → ui-kit
   - Domain-specific (user profile card, org selector) → app component

2. **Does it need API data or business logic?**
   - No → ui-kit
   - Yes → app component (use ui-kit primitives)

3. **Is it reusable across multiple apps?**
   - Yes → ui-kit
   - No → keep in specific app

4. **Can it be built with Tailwind alone?**
   - Yes → ui-kit
   - No (needs custom CSS/animations) → reconsider design or use Tailwind plugins

5. **Does it depend on app-specific state?**
   - No → ui-kit
   - Yes → app component

### When modifying existing components, ask:

1. **Does this change break existing usage?**
   - If yes, consider deprecation or versioning
   - Add new prop with default to maintain compatibility

2. **Does this add a new dependency?**
   - Avoid if possible
   - Discuss with team if necessary

3. **Does this require custom CSS?**
   - Find Tailwind solution first
   - If impossible, reconsider the design

## Troubleshooting

### Component not found in consuming app

**Symptom**: `Cannot find module '@dc-platform/ui-kit'`

**Fix**: Ensure consuming app has dependency:
```json
{
  "dependencies": {
    "@dc-platform/ui-kit": "workspace:*"
  }
}
```

Run `pnpm install` from monorepo root.

### Tailwind classes not applied

**Symptom**: Components render but have no styling

**Fix**: Ensure consuming app's `tailwind.config.js` includes ui-kit source:
```js
content: [
  './src/**/*.{vue,js,ts}',
  '../packages/ui-kit/src/**/*.vue',  // Add this
]
```

### TypeScript errors in consuming app

**Symptom**: Type errors when importing components

**Fix**: Ensure consuming app's `tsconfig.json` has proper Vue support:
```json
{
  "compilerOptions": {
    "jsx": "preserve",
    "moduleResolution": "bundler"
  }
}
```

### v-model not working

**Symptom**: Two-way binding doesn't update parent

**Fix**: Ensure component emits `update:modelValue` and parent uses `v-model`:
```vue
<!-- Parent -->
<DcInput v-model="email" />

<!-- NOT v-model:value or :model-value -->
```
