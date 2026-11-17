import { memo } from 'react'

export interface ScheduleColumn {
  key: string
  label: string
  align?: 'left' | 'right'
}

export type ScheduleRow = Record<string, string>

interface ScheduleTableProps {
  /** Used in the data-testid: schedule-table-{name}. */
  name: string
  caption: string
  columns: ScheduleColumn[]
  rows: ScheduleRow[]
  totalLabel?: string
  /** Total already formatted for display; it comes from the API, never summed here. */
  total: string
  emptyMessage?: string
}

/**
 * A schedule as a native table with a caption, column headers and a total row in tfoot.
 * Rows are plain elements with no per-row components, so 5,200 rows render quickly (NFR5.3).
 */
function ScheduleTableImpl({
  name,
  caption,
  columns,
  rows,
  totalLabel = 'Total',
  total,
  emptyMessage,
}: ScheduleTableProps) {
  if (rows.length === 0 && emptyMessage) {
    return (
      <p className="text-muted-foreground py-4" data-testid={`schedule-table-${name}`}>
        {emptyMessage}
      </p>
    )
  }

  const alignClass = (col: ScheduleColumn) => (col.align === 'right' ? 'text-right tabular-nums' : 'text-left')

  return (
    <div
      role="region"
      aria-label={caption}
      tabIndex={0}
      className="focus-visible:ring-ring/50 w-full overflow-x-auto rounded-md border focus-visible:ring-[3px] focus-visible:outline-none"
    >
      <table className="w-full caption-bottom text-sm" data-testid={`schedule-table-${name}`}>
        <caption className="text-muted-foreground sr-only">{caption}</caption>
        <thead className="bg-muted">
          <tr className="border-b">
            {columns.map((col) => (
              <th key={col.key} scope="col" className={`h-10 px-3 font-medium ${alignClass(col)}`}>
                {col.label}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row, i) => (
            <tr key={i} className="border-b last:border-0">
              {columns.map((col) => (
                <td key={col.key} className={`px-3 py-2 ${alignClass(col)}`}>
                  {row[col.key]}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
        <tfoot className="bg-muted font-medium">
          <tr>
            <th scope="row" colSpan={columns.length - 1} className="px-3 py-2 text-right">
              {totalLabel}
            </th>
            <td className="px-3 py-2 text-right tabular-nums" data-testid={`schedule-total-${name}`}>
              {total}
            </td>
          </tr>
        </tfoot>
      </table>
    </div>
  )
}

export const ScheduleTable = memo(ScheduleTableImpl)
