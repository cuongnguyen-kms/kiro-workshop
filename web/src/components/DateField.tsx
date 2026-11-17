import { useState } from 'react'
import type { FieldPath, FieldValues } from 'react-hook-form'
import { useController, useFormContext } from 'react-hook-form'
import { CalendarIcon } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Calendar } from '@/components/ui/calendar'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { FieldError } from '@/components/FieldError'
import { fieldId, isIsoDate } from '@/contracts/validation'

interface DateFieldProps<T extends FieldValues> {
  name: FieldPath<T>
  label: string
  readOnly?: boolean
}

function toIso(date: Date): string {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

function fromIso(value: string): Date | undefined {
  if (!isIsoDate(value)) return undefined
  const [y, m, d] = value.split('-').map(Number)
  return new Date(y, m - 1, d)
}

/** A date input that accepts typed YYYY-MM-DD, with a calendar picker beside it. */
export function DateField<T extends FieldValues>({ name, label, readOnly }: DateFieldProps<T>) {
  const { control } = useFormContext<T>()
  const { field, fieldState } = useController({ name, control })
  const [open, setOpen] = useState(false)
  const id = fieldId(name)
  const errorId = `${id}-error`
  const value = (field.value as string) ?? ''
  const selected = fromIso(value)

  return (
    <div className="grid gap-1.5">
      <Label htmlFor={id}>{label} *</Label>
      <div className="flex gap-2">
        <Input
          id={id}
          ref={field.ref}
          name={field.name}
          value={value}
          onChange={(e) => field.onChange(e.target.value)}
          onBlur={field.onBlur}
          placeholder="YYYY-MM-DD"
          autoComplete="off"
          readOnly={readOnly}
          aria-required="true"
          aria-invalid={fieldState.error ? true : undefined}
          aria-describedby={fieldState.error ? errorId : undefined}
          data-testid={`${name}-input`}
          className="max-w-44"
        />
        <Popover open={open} onOpenChange={setOpen}>
          <PopoverTrigger asChild>
            <Button
              type="button"
              variant="outline"
              size="icon"
              aria-label={`Choose ${label.toLowerCase()}`}
              disabled={readOnly}
            >
              <CalendarIcon aria-hidden="true" />
            </Button>
          </PopoverTrigger>
          <PopoverContent className="w-auto p-0" align="start">
            <Calendar
              mode="single"
              selected={selected}
              defaultMonth={selected}
              onSelect={(date) => {
                if (date) field.onChange(toIso(date))
                setOpen(false)
                field.onBlur()
              }}
            />
          </PopoverContent>
        </Popover>
      </div>
      <FieldError id={errorId} message={fieldState.error?.message} />
    </div>
  )
}
