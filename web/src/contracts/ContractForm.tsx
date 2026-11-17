import { useEffect, useRef, useState, type ReactNode } from 'react'
import { FormProvider, useFieldArray, useForm, type FieldErrors } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Loader2, Plus } from 'lucide-react'
import type { ValidationErrors } from '@/api/types'
import { Alert, AlertDescription } from '@/components/ui/alert'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { DateField } from '@/components/DateField'
import { ErrorSummary, type ErrorSummaryItem } from '@/components/ErrorSummary'
import { FieldError } from '@/components/FieldError'
import { ExpenseSection } from '@/contracts/ExpenseSection'
import { StreamCard } from '@/contracts/StreamCard'
import {
  NAME_MAX_LENGTH,
  apiKeyToFieldPath,
  contractFormSchema,
  emptyStream,
  fieldId,
  formFieldPaths,
  initialFormValues,
  type ContractFormValues,
} from '@/contracts/validation'

export type SaveStatus = 'idle' | 'saving' | 'failed'

interface ContractFormProps {
  status: SaveStatus
  /** Field errors from an API 400, keyed as the API sends them. A new object means a new answer. */
  apiErrors?: ValidationErrors
  onSubmit: (values: ContractFormValues) => void
  onCancel: () => void
}

export const SAVE_FAILED_MESSAGE = 'The contract was not saved. Nothing was stored. Try again.'

function errorAt(errors: FieldErrors<ContractFormValues>, path: string): string | undefined {
  let node: unknown = errors
  for (const part of path.split('.')) {
    if (node === null || typeof node !== 'object') return undefined
    node = (node as Record<string, unknown>)[part]
  }
  if (node && typeof node === 'object' && 'message' in node) {
    const message = (node as { message?: unknown }).message
    return typeof message === 'string' && message !== '' ? message : undefined
  }
  return undefined
}

function Section({ number, title, children, action }: {
  number: number
  title: string
  children: ReactNode
  action?: ReactNode
}) {
  const headingId = `section-${number}-title`
  return (
    <section aria-labelledby={headingId} className="bg-card grid gap-4 rounded-xl border p-6 shadow-sm">
      <div className="flex items-center justify-between gap-2">
        <h2 id={headingId} className="text-lg font-semibold">
          <span className="text-muted-foreground mr-2" aria-hidden="true">{number}</span>
          {title}
        </h2>
        {action}
      </div>
      {children}
    </section>
  )
}

/** The whole entry form (S2). Validation on blur and on save; the API's answer wins. */
export function ContractForm({ status, apiErrors, onSubmit, onCancel }: ContractFormProps) {
  const form = useForm<ContractFormValues>({
    resolver: zodResolver(contractFormSchema),
    defaultValues: initialFormValues(),
    mode: 'onBlur',
    reValidateMode: 'onBlur',
    shouldFocusError: false,
  })
  const { control, register, handleSubmit, formState, setError, getValues } = form
  const { fields, append, remove } = useFieldArray({ control, name: 'paymentStreams' })

  const [submitAttempted, setSubmitAttempted] = useState(false)
  const [unmatchedApiErrors, setUnmatchedApiErrors] = useState<string[]>([])
  const [focusSummary, setFocusSummary] = useState(0)
  const [discardOpen, setDiscardOpen] = useState(false)
  const summaryRef = useRef<HTMLDivElement>(null)
  const addStreamRef = useRef<HTMLButtonElement>(null)
  const saving = status === 'saving'

  // Show API 400 messages under the matching fields; keys that match no field go to the summary only.
  useEffect(() => {
    if (!apiErrors) return
    const known = new Set(formFieldPaths(getValues()))
    const unmatched: string[] = []
    for (const [key, messages] of Object.entries(apiErrors)) {
      const path = apiKeyToFieldPath(key)
      const message = messages.join(' ')
      if (known.has(path)) {
        setError(path as never, { type: 'server', message })
      } else {
        unmatched.push(message)
      }
    }
    setUnmatchedApiErrors(unmatched)
    setSubmitAttempted(true)
    setFocusSummary((n) => n + 1)
  }, [apiErrors, getValues, setError])

  useEffect(() => {
    if (focusSummary > 0) summaryRef.current?.focus()
  }, [focusSummary])

  // The summary follows the live errors, so fixed fields drop out of it.
  const values = form.watch()
  const summaryItems: ErrorSummaryItem[] = []
  if (submitAttempted) {
    for (const path of formFieldPaths(values)) {
      const message = errorAt(formState.errors, path)
      if (message) summaryItems.push({ fieldId: fieldId(path), message })
    }
    for (const message of unmatchedApiErrors) summaryItems.push({ message })
  }

  const submit = handleSubmit(
    (valid) => {
      setUnmatchedApiErrors([])
      onSubmit(valid)
    },
    () => {
      setSubmitAttempted(true)
      setUnmatchedApiErrors([])
      setFocusSummary((n) => n + 1)
    },
  )

  const cancel = () => {
    if (formState.isDirty) setDiscardOpen(true)
    else onCancel()
  }

  const addStream = () => {
    append(emptyStream(), { shouldFocus: true, focusName: `paymentStreams.${fields.length}.amount` })
  }

  const removeStream = (index: number) => {
    remove(index)
    addStreamRef.current?.focus()
  }

  const nameError = formState.errors.name?.message
  const nameId = fieldId('name')

  return (
    <FormProvider {...form}>
      <form
        noValidate
        onSubmit={(e) => {
          e.preventDefault()
          if (!saving) void submit(e)
        }}
        className="grid gap-6"
        data-testid="contract-form"
        aria-busy={saving}
      >
        {status === 'failed' && (
          <Alert variant="destructive" role="alert" data-testid="save-failed-alert">
            <AlertDescription>{SAVE_FAILED_MESSAGE}</AlertDescription>
          </Alert>
        )}
        <ErrorSummary ref={summaryRef} errors={summaryItems} />

        <Section number={1} title="Details">
          <div className="grid gap-1.5">
            <Label htmlFor={nameId}>Contract name *</Label>
            <Input
              id={nameId}
              {...register('name')}
              maxLength={NAME_MAX_LENGTH}
              autoComplete="off"
              readOnly={saving}
              aria-required="true"
              aria-invalid={nameError ? true : undefined}
              aria-describedby={nameError ? `${nameId}-error` : undefined}
              data-testid="name-input"
            />
            <FieldError id={`${nameId}-error`} message={nameError} />
          </div>
          <div className="grid gap-4 sm:grid-cols-2">
            <DateField<ContractFormValues> name="startDate" label="Start date" readOnly={saving} />
            <DateField<ContractFormValues> name="endDate" label="End date" readOnly={saving} />
          </div>
        </Section>

        <Section
          number={2}
          title="Payment streams"
          action={
            <Button
              ref={addStreamRef}
              type="button"
              variant="outline"
              onClick={addStream}
              disabled={saving}
              data-testid="add-stream"
            >
              <Plus aria-hidden="true" /> Add stream
            </Button>
          }
        >
          {fields.map((f, index) => (
            <StreamCard
              key={f.id}
              index={index}
              canRemove={fields.length > 1}
              onRemove={() => removeStream(index)}
              readOnly={saving}
            />
          ))}
        </Section>

        <Section number={3} title="Expense">
          <ExpenseSection readOnly={saving} />
        </Section>

        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={cancel} disabled={saving} data-testid="cancel-contract">
            Cancel
          </Button>
          <Button type="submit" disabled={saving} data-testid="save-contract">
            {saving && <Loader2 aria-hidden="true" className="animate-spin" />}
            {saving ? 'Saving…' : 'Save contract'}
          </Button>
        </div>
      </form>

      <AlertDialog open={discardOpen} onOpenChange={setDiscardOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Discard this contract?</AlertDialogTitle>
            <AlertDialogDescription>Your entries will be lost.</AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel data-testid="keep-editing">Keep editing</AlertDialogCancel>
            <AlertDialogAction data-testid="discard-contract" onClick={onCancel}>
              Discard
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </FormProvider>
  )
}
