import type { PaymentStream } from '@/api/types'
import { FREQUENCY_LABELS, formatDate, formatMoney } from '@/contracts/display'

/** Frequency, amount and dates of each stream. */
export function StreamsSummary({ streams }: { streams: PaymentStream[] }) {
  return (
    <section aria-labelledby="streams-title" className="grid gap-2" data-testid="streams-summary">
      <h2 id="streams-title" className="text-lg font-semibold">
        Payment streams
      </h2>
      <ul className="grid gap-1">
        {streams.map((s) => (
          <li key={s.id} className="flex flex-wrap gap-x-4 text-sm">
            <span className="w-24">{FREQUENCY_LABELS[s.frequency]}</span>
            <span className="w-24 text-right tabular-nums">{formatMoney(s.amount)}</span>
            <span>
              {s.frequency === 'OneTime'
                ? formatDate(s.startDate)
                : `${formatDate(s.startDate)} to ${formatDate(s.endDate)}`}
            </span>
          </li>
        ))}
      </ul>
    </section>
  )
}
