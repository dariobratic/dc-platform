export function generateSlug(input: string): string {
  let slug = input
    .toLowerCase()
    .replace(/\s+/g, '-')
    .replace(/_/g, '-')
    .replace(/[^a-z0-9-]/g, '')
    .replace(/-{2,}/g, '-')
    .replace(/^-|-$/g, '')

  if (slug.length > 50) {
    slug = slug.substring(0, 50).replace(/-$/, '')
  }

  return slug
}
