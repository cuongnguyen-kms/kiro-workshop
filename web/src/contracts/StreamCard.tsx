import { useController, useFormContext, useWatch } from 'react-hook-form'
import { FREQUENCIES, type Frequency } from '@/api/types'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { DateField } from '@/components/DateField'
import { FieldError } from '@/components/FieldError'
import { MoneyField } from '@/components/MoneyField'
import { FREQUENCY_LABELS } from '@/contracts/display'
import { fieldId, type ContractFormValues } from '@/contracts/validation'

interface StreamCardProps {
  index: number
  canRemove: boolean
  onRemove: () => void
  readOnly: boolean
}

function FrequencyField({ index, readOnly }: { index: number; readOnly: boolean }) {
  const name = `paymentStreams.${index}.frequency` as const
  const { control, setValue, clearErrors } = useFormContext<ContractFormValues>()
  const { field, fieldState } = useController({ name, control })
  const id = fieldId(name)
  const errorId = `${id}-error`
  return (
    <div className="grid gap-1.5">
      <Label htmlFor={id}>Frequency *</Label>
      <Select
        name={field.name}
        value={field.value === '' ? undefined : field.value}
        disabled={readOnly}
        onValueChange={(value) => {
          field.onChange(value as Frequency)
          if (value === 'OneTime') {
            // One-time has no end date: clear it so a hidden value is never sent (AC2.1.5).
            setValue(`paymentStreams.${index}.endDate`, '')
            clearErrors(`paymentStreams.${index}.endDate`)
          }
          field.onBlur()
        }}
      >
        <SelectTrigger
          id={id}
          ref={field.ref}
          className="w-44"
          aria-required="true"
          aria-invalid={fieldState.error ? true : undefined}
          aria-describedby={fieldState.error ? errorId : undefined}
          data-testid={`${name}-input`}
        >
          <SelectValue placeholder="Choose" />
        </SelectTrigger>
        <SelectContent>
          {FREQUENCIES.map((f) => (
            <SelectItem key={f} value={f}>
              {FREQUENCY_LABELS[f]}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      <FieldError id={errorId} message={fieldState.error?.message} />
    </div>
  )
}

/** One payment stream's fields in a fieldset whose legend is "Payment stream N". */
export function StreamCard({ index, canRemove, onRemove, readOnly }: StreamCardProps) {
  const { control } = useFormContext<ContractFormValues>()
  const frequency = useWatch({ control, name: `paymentStreams.${index}.frequency` })
  const oneTime = frequency === 'OneTime'

  return (
    <Card className="py-4" data-testid={`stream-card-${index}`}>
      <CardContent className="px-4">
        <fieldset className="grid gap-4">
          <legend className="float-left pt-1 font-medium">Payment stream {index + 1}</legend>
          <div className="flex justify-end">
            <Button
              type="button"
              variant="outline"
              size="sm"
              className="text-destructive border-destructive/50 hover:bg-destructive/10"
              onClick={onRemove}
              disabled={!canRemove || readOnly}
              aria-label={`Remove payment stream ${index + 1}`}
              data-testid={`remove-stream-${index}`}
            >
              Remove
            </Button>
          </div>
          <div className="grid gap-4 sm:grid-cols-2">
            <MoneyField<ContractFormValues>
              name={`paymentStreams.${index}.amount`}
              label="Amount"
              readOnly={readOnly}
            />
            <FrequencyField index={index} readOnly={readOnly} />
          </div>
          <div className="grid gap-4 sm:grid-cols-2">
            <DateField<ContractFormValues>
              name={`paymentStreams.${index}.startDate`}
              label={oneTime ? 'Payment date' : 'Stream start'}
              readOnly={readOnly}
            />
            {!oneTime && (
              <DateField<ContractFormValues>
                name={`paymentStreams.${index}.endDate`}
                label="Stream end"
                readOnly={readOnly}
              />
            )}
          </div>
        </fieldset>
      </CardContent>
    </Card>
  )
}
