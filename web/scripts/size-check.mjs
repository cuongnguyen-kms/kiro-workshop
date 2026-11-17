// Fails when the list screen's built JavaScript is over 300 KB compressed (NFR5.5).
// "The list screen's JavaScript" is everything the browser downloads to show /contracts:
// the entry chunk, the lazily loaded ContractsListPage chunk, and every chunk either imports statically.
// Sizes are measured gzip-compressed (level 9), read from the Vite build manifest.
import { existsSync, readFileSync } from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { gzipSync } from 'node:zlib'

const LIMIT_BYTES = 300 * 1000
const ENTRY = 'index.html'
const LIST_SCREEN = 'src/pages/ContractsListPage.tsx'

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..')
const dist = path.join(root, 'dist')
const manifestPath = path.join(dist, '.vite', 'manifest.json')

if (!existsSync(manifestPath)) {
  console.error(`Size check: ${manifestPath} not found. Run "npm run build" first.`)
  process.exit(1)
}

const manifest = JSON.parse(readFileSync(manifestPath, 'utf8'))

for (const key of [ENTRY, LIST_SCREEN]) {
  if (!manifest[key]) {
    console.error(`Size check: "${key}" is not in the build manifest; the list screen cannot be measured.`)
    process.exit(1)
  }
}

const files = new Set()
const visit = (key) => {
  const chunk = manifest[key]
  if (!chunk || files.has(chunk.file)) return
  files.add(chunk.file)
  for (const dep of chunk.imports ?? []) visit(dep)
}
visit(ENTRY)
visit(LIST_SCREEN)

let total = 0
const rows = []
for (const file of [...files].filter((f) => f.endsWith('.js'))) {
  const bytes = gzipSync(readFileSync(path.join(dist, file)), { level: 9 }).length
  total += bytes
  rows.push(`  ${file}  ${(bytes / 1000).toFixed(1)} KB`)
}

console.log('List screen JavaScript (gzip):')
console.log(rows.join('\n'))
console.log(`  total  ${(total / 1000).toFixed(1)} KB (limit ${LIMIT_BYTES / 1000} KB)`)

if (total > LIMIT_BYTES) {
  console.error(`Size check failed: the list screen's JavaScript is ${(total / 1000).toFixed(1)} KB compressed, over the ${LIMIT_BYTES / 1000} KB limit.`)
  process.exit(1)
}
console.log('Size check passed.')
