import { memo, useState } from 'react'
import { Button } from '@/components/ui/button'

/** Long schedules show this many rows per page; the total row is always visible (NFR5.3). */
export const SCHEDULE_PAGE_SIZE = 100

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
  /** Total already formatted for display; it comes from the API, never summed here. No total row when absent. */
  total?: string
  emptyMessage?: string
  /** Shows the first cell of each body row as a row header (`th scope="row"`). */
  rowHeader?: boolean
  /** Keeps the first column fixed at the left while the region scrolls sideways. */
  stickyFirstColumn?: boolean
  /** Accessible name of the scrolling region; defaults to the caption. */
  regionLabel?: string
  /** Tailwind max-height class for the region, so the header row can stay at the top while scrolling down. */
  maxHeightClass?: string
}

/**
 * A schedule as a native table with a caption, column headers and an optional total row in tfoot.
 * Rows are plain elements with no per-row components. A schedule longer than
 * SCHEDULE_PAGE_SIZE rows is shown a page at a time, so even 5,200 rows render quickly (NFR5.3).
 * The table sits in a focusable scroll region; the pager is below the region so it stays in view.
 */
function ScheduleTableImpl({
  name,
  caption,
  columns,
  rows,
  totalLabel = 'Total',
  total,
  emptyMessage,
  rowHeader = false,
  stickyFirstColumn = false,
  regionLabel,
  maxHeightClass,
}: ScheduleTableProps) {
  const [page, setPage] = useState(1)
  const pageCount = Math.max(1, Math.ceil(rows.length / SCHEDULE_PAGE_SIZE))
  const currentPage = Math.min(page, pageCount)
  const paged = rows.length > SCHEDULE_PAGE_SIZE
  const firstRow = (currentPage - 1) * SCHEDULE_PAGE_SIZE
  const visibleRows = paged ? rows.slice(firstRow, firstRow + SCHEDULE_PAGE_SIZE) : rows

  if (rows.length === 0 && emptyMessage) {
    return (
      <p className="text-muted-foreground py-4" data-testid={`schedule-table-${name}`}>
        {emptyMessage}
      </p>
    )
  }

  const alignClass = (col: ScheduleColumn) => (col.align === 'right' ? 'text-right tabular-nums' : 'text-left')
  // Cell styling comes from the table's class (descendant selectors), so the 5,200 rows carry no
  // per-row or per-cell attributes. With a row header, every body cell after the first is an amount
  // column; otherwise the right-aligned last column is.
  const lastRight = columns[columns.length - 1]?.align === 'right'
  const amountClass = rowHeader
    ? ' [&_tbody_td]:text-right [&_tbody_td]:tabular-nums'
    : lastRight
      ? ' [&_tbody_td:last-child]:text-right [&_tbody_td:last-child]:tabular-nums'
      : ''
  const rowHeaderClass = rowHeader ? ' [&_tbody_th]:px-3 [&_tbody_th]:py-2 [&_tbody_th]:text-left [&_tbody_th]:font-normal' : ''
  const stickyClass = stickyFirstColumn
    ? ' [&_tbody_th]:sticky [&_tbody_th]:left-0 [&_tbody_th]:z-10 [&_tbody_th]:bg-background [&_tbody_th]:border-r'
    : ''
  const tableClass = `w-full caption-bottom text-sm [&_tbody_td]:px-3 [&_tbody_td]:py-2 [&_tbody_tr]:border-b [&_tbody_tr:last-child]:border-0${amountClass}${rowHeaderClass}${stickyClass}`

  return (
    <div className="w-full rounded-md border">
      <div
        role="region"
        aria-label={regionLabel ?? caption}
        tabIndex={0}
        className={`focus-visible:ring-ring/50 w-full overflow-auto focus-visible:ring-[3px] focus-visible:outline-none${
          maxHeightClass ? ` ${maxHeightClass}` : ''
        }`}
      >
        <table className={tableClass} data-testid={`schedule-table-${name}`}>
          <caption className="text-muted-foreground sr-only">{caption}</caption>
          <thead className="bg-muted">
            <tr className="border-b">
              {columns.map((col, i) => (
                <th
                  key={col.key}
                  scope="col"
                  className={`bg-muted sticky top-0 h-10 px-3 font-medium whitespace-nowrap ${
                    stickyFirstColumn && i === 0 ? 'left-0 z-30 border-r' : 'z-20'
                  } ${alignClass(col)}`}
                >
                  {col.label}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {visibleRows.map((row, i) => (
              <tr key={firstRow + i}>
                {columns.map((col, c) =>
                  rowHeader && c === 0 ? (
                    <th key={col.key} scope="row">
                      {row[col.key]}
                    </th>
                  ) : (
                    <td key={col.key}>{row[col.key]}</td>
                  ),
                )}
              </tr>
            ))}
          </tbody>
          {total !== undefined && (
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
          )}
        </table>
      </div>
      {paged && (
        <div
          className="flex items-center justify-between gap-2 border-t px-3 py-2 text-sm"
          data-testid={`schedule-pager-${name}`}
        >
          <span aria-live="polite">
            Rows {firstRow + 1}–{Math.min(firstRow + SCHEDULE_PAGE_SIZE, rows.length)} of {rows.length}
          </span>
          <div className="flex gap-2">
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => setPage(currentPage - 1)}
              disabled={currentPage <= 1}
              data-testid={`schedule-prev-${name}`}
            >
              Previous
            </Button>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => setPage(currentPage + 1)}
              disabled={currentPage >= pageCount}
              data-testid={`schedule-next-${name}`}
            >
              Next
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}

export const ScheduleTable = memo(ScheduleTableImpl)
