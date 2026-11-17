import type { FieldPath, FieldValues } from 'react-hook-form'
import { useController, useFormContext } from 'react-hook-form'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { FieldError } from '@/components/FieldError'
import { fieldId } from '@/contracts/validation'

interface MoneyFieldProps<T extends FieldValues> {
  name: FieldPath<T>
  label: string
  readOnly?: boolean
}

const PLAIN_AMOUNT = /^\d+(\.\d{0,2})?$/

/** Pads an entered amount to 2 decimals for display. It never rounds: longer input is left as typed. */
export function padAmount(text: string): string {
  const trimmed = text.trim()
  if (!PLAIN_AMOUNT.test(trimmed)) return text
  const [whole, fraction = ''] = trimmed.split('.')
  return `${whole}.${fraction.padEnd(2, '0')}`
}

/** Amount input that formats to 2 decimals on blur. */
export function MoneyField<T extends FieldValues>({ name, label, readOnly }: MoneyFieldProps<T>) {
  const { control } = useFormContext<T>()
  const { field, fieldState } = useController({ name, control })
  const id = fieldId(name)
  const errorId = `${id}-error`
  return (
    <div className="grid gap-1.5">
      <Label htmlFor={id}>{label} *</Label>
      <Input
        id={id}
        ref={field.ref}
        name={field.name}
        value={(field.value as string) ?? ''}
        onChange={(e) => field.onChange(e.target.value)}
        onBlur={() => {
          const padded = padAmount((field.value as string) ?? '')
          if (padded !== field.value) field.onChange(padded)
          field.onBlur()
        }}
        inputMode="decimal"
        autoComplete="off"
        readOnly={readOnly}
        aria-required="true"
        aria-invalid={fieldState.error ? true : undefined}
        aria-describedby={fieldState.error ? errorId : undefined}
        data-testid={`${name}-input`}
      />
      <FieldError id={errorId} message={fieldState.error?.message} />
    </div>
  )
}
