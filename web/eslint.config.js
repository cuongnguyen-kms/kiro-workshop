import js from '@eslint/js'
import react from 'eslint-plugin-react'
import reactHooks from 'eslint-plugin-react-hooks'
import globals from 'globals'
import tseslint from 'typescript-eslint'

const storageMessage = 'The app stores nothing in the browser (NFR3.6).'

export default tseslint.config(
  { ignores: ['dist', 'coverage', 'node_modules'] },
  {
    files: ['**/*.{ts,tsx}'],
    extends: [js.configs.recommended, ...tseslint.configs.recommended],
    languageOptions: {
      ecmaVersion: 2023,
      globals: globals.browser,
    },
    plugins: {
      react,
      'react-hooks': reactHooks,
    },
    settings: { react: { version: 'detect' } },
    rules: {
      'react-hooks/rules-of-hooks': 'error',
      'react-hooks/exhaustive-deps': 'warn',
      // NFR3.5: text from the API is rendered as React text only.
      'react/no-danger': 'error',
      // NFR3.6: no browser storage.
      'no-restricted-globals': [
        'error',
        { name: 'localStorage', message: storageMessage },
        { name: 'sessionStorage', message: storageMessage },
        { name: 'indexedDB', message: storageMessage },
      ],
      'no-restricted-properties': [
        'error',
        { object: 'window', property: 'localStorage', message: storageMessage },
        { object: 'window', property: 'sessionStorage', message: storageMessage },
        { object: 'window', property: 'indexedDB', message: storageMessage },
        { object: 'globalThis', property: 'localStorage', message: storageMessage },
        { object: 'globalThis', property: 'sessionStorage', message: storageMessage },
        { object: 'globalThis', property: 'indexedDB', message: storageMessage },
        { object: 'document', property: 'cookie', message: storageMessage },
        { property: 'innerHTML', message: 'Render text through React (NFR3.5).' },
        { property: 'outerHTML', message: 'Render text through React (NFR3.5).' },
      ],
    },
  },
  {
    files: ['src/components/ui/**/*.{ts,tsx}'],
    rules: {
      // Generated shadcn/ui source exports variants next to components.
      '@typescript-eslint/no-empty-object-type': 'off',
    },
  },
  {
    files: ['scripts/**/*.mjs'],
    extends: [js.configs.recommended],
    languageOptions: { globals: globals.node },
  },
)
