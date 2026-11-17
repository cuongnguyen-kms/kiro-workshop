import { Link } from 'react-router-dom'
import type { ContractSummary } from '@/api/types'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { formatDate } from '@/contracts/display'

/** Rows of name, start and end dates; the name links to the contract. */
export function ContractsTable({ items }: { items: ContractSummary[] }) {
  return (
    <Table data-testid="contracts-table">
      <caption className="sr-only">Contracts</caption>
      <TableHeader>
        <TableRow>
          <TableHead scope="col">Name</TableHead>
          <TableHead scope="col">Start</TableHead>
          <TableHead scope="col">End</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {items.map((c) => (
          <TableRow key={c.id} data-testid={`contract-row-${c.id}`}>
            <TableCell>
              <Link to={`/contracts/${c.id}`} className="font-medium underline-offset-2 hover:underline">
                {c.name}
              </Link>
            </TableCell>
            <TableCell>{formatDate(c.startDate)}</TableCell>
            <TableCell>{formatDate(c.endDate)}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}
