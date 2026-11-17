import { forwardRef } from 'react'
import { AlertCircle } from 'lucide-react'

export interface ErrorSummaryItem {
  /** DOM id of the field the message belongs to; absent when no field matches. */
  fieldId?: string
  message: string
}

interface ErrorSummaryProps {
  errors: ErrorSummaryItem[]
}

/** Lists every error, each linking to its field. Receives focus after a failed save. */
export const ErrorSummary = forwardRef<HTMLDivElement, ErrorSummaryProps>(function ErrorSummary(
  { errors },
  ref,
) {
  if (errors.length === 0) return null
  return (
    <div
      ref={ref}
      role="alert"
      tabIndex={-1}
      aria-labelledby="error-summary-title"
      data-testid="error-summary"
      className="border-destructive text-destructive focus-visible:ring-ring/50 rounded-lg border p-4 focus-visible:ring-[3px] focus-visible:outline-none"
    >
      <h2 id="error-summary-title" className="flex items-center gap-2 font-semibold">
        <AlertCircle aria-hidden="true" className="size-4" />
        Fix the following before saving
      </h2>
      <ul className="mt-2 list-disc pl-6">
        {errors.map((error, i) => (
          <li key={`${error.fieldId ?? 'general'}-${i}`}>
            {error.fieldId ? (
              <a
                href={`#${error.fieldId}`}
                className="underline underline-offset-2"
                onClick={(event) => {
                  event.preventDefault()
                  document.getElementById(error.fieldId!)?.focus()
                }}
              >
                {error.message}
              </a>
            ) : (
              error.message
            )}
          </li>
        ))}
      </ul>
    </div>
  )
})
