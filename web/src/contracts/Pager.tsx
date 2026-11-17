import { PAGE_SIZES } from '@/api/types'
import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'

interface PagerProps {
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  onPageChange: (page: number) => void
  onPageSizeChange: (pageSize: number) => void
}

/** Previous, Next, the page and total, and the page-size select (10, 20, 50, 100). */
export function Pager({ page, pageSize, totalCount, totalPages, onPageChange, onPageSizeChange }: PagerProps) {
  return (
    <nav aria-label="Pagination" className="flex flex-wrap items-center justify-between gap-4" data-testid="pager">
      <p className="text-sm" aria-live="polite">
        Page {page} of {Math.max(totalPages, 1)} · {totalCount} {totalCount === 1 ? 'contract' : 'contracts'}
      </p>
      <div className="flex flex-wrap items-center gap-4">
        <div className="flex items-center gap-2">
          <Label htmlFor="page-size">Rows per page</Label>
          <Select value={String(pageSize)} onValueChange={(value) => onPageSizeChange(Number(value))}>
            <SelectTrigger id="page-size" className="w-20" data-testid="page-size-select">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {PAGE_SIZES.map((size) => (
                <SelectItem key={size} value={String(size)}>
                  {size}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="flex gap-2">
          <Button
            type="button"
            variant="outline"
            onClick={() => onPageChange(page - 1)}
            disabled={page <= 1}
            data-testid="pager-prev"
          >
            Previous
          </Button>
          <Button
            type="button"
            variant="outline"
            onClick={() => onPageChange(page + 1)}
            disabled={page >= totalPages}
            data-testid="pager-next"
          >
            Next
          </Button>
        </div>
      </div>
    </nav>
  )
}
