import { AlertCircle } from 'lucide-react'

/** Error text under a field, with an icon so it is not shown by colour alone. */
export function FieldError({ id, message }: { id: string; message?: string }) {
  if (!message) return null
  return (
    <p id={id} className="text-destructive flex items-center gap-1 text-sm">
      <AlertCircle aria-hidden="true" className="size-4 shrink-0" />
      {message}
    </p>
  )
}
