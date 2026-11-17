import { useController, useFormContext } from 'react-hook-form'
import { EXPENSE_METHODS, type ExpenseMethod } from '@/api/types'
import { Label } from '@/components/ui/label'
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group'
import { DateField } from '@/components/DateField'
import { EXPENSE_METHOD_LABELS } from '@/contracts/display'
import { fieldId, type ContractFormValues } from '@/contracts/validation'

/** The expense method choice and the date fields for the chosen method (AC3.1.1 to AC3.1.3). */
export function ExpenseSection({ readOnly }: { readOnly: boolean }) {
  const { control, setValue, clearErrors } = useFormContext<ContractFormValues>()
  const { field } = useController({ name: 'expense.method', control })
  const method = field.value

  const changeMethod = (value: string) => {
    // Hidden fields are cleared, so a hidden value is never sent.
    setValue('expense.startDate', '')
    setValue('expense.endDate', '')
    setValue('expense.date', '')
    clearErrors(['expense.startDate', 'expense.endDate', 'expense.date'])
    field.onChange(value as ExpenseMethod)
  }

  return (
    <div className="grid gap-4">
      <fieldset className="grid gap-2">
        <legend className="mb-2 text-sm font-medium">Expense method *</legend>
        <RadioGroup
          id={fieldId('expense.method')}
          ref={field.ref}
          value={method}
          onValueChange={changeMethod}
          disabled={readOnly}
          className="flex flex-wrap gap-6"
          data-testid="expense-method"
        >
          {EXPENSE_METHODS.map((m) => (
            <div key={m} className="flex items-center gap-2">
              <RadioGroupItem
                value={m}
                id={`expense-method-${m}`}
                data-testid={`expense-method-${m}`}
                className="size-5"
              />
              <Label htmlFor={`expense-method-${m}`}>{EXPENSE_METHOD_LABELS[m]}</Label>
            </div>
          ))}
        </RadioGroup>
      </fieldset>

      {method === 'Overtime' && (
        <div className="grid gap-4 sm:grid-cols-2">
          <DateField<ContractFormValues> name="expense.startDate" label="Expense start date" readOnly={readOnly} />
          <DateField<ContractFormValues> name="expense.endDate" label="Expense end date" readOnly={readOnly} />
        </div>
      )}
      {method === 'AllAtOnce' && (
        <div className="grid gap-4 sm:grid-cols-2">
          <DateField<ContractFormValues> name="expense.date" label="Expense date" readOnly={readOnly} />
        </div>
      )}
      {method === 'None' && (
        <p className="text-muted-foreground text-sm">No expense schedule will be created.</p>
      )}
    </div>
  )
}
