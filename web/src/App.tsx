import { Suspense, lazy } from 'react'
import { BrowserRouter, Link, Navigate, Route, Routes } from 'react-router-dom'
import { Skeleton } from '@/components/ui/skeleton'

// Each screen loads on demand, keeping the list screen's first load small (NFR5.5).
const ContractsListPage = lazy(() => import('@/pages/ContractsListPage'))
const NewContractPage = lazy(() => import('@/pages/NewContractPage'))
const ContractDetailPage = lazy(() => import('@/pages/ContractDetailPage'))

function NotFoundPage() {
  return (
    <main className="mx-auto grid max-w-5xl gap-2 p-6" data-testid="not-found-page">
      <h1 className="text-2xl font-semibold">Page not found</h1>
      <Link to="/contracts" className="underline underline-offset-2">
        Go to contracts
      </Link>
    </main>
  )
}

function ScreenFallback() {
  return (
    <div className="mx-auto grid max-w-5xl gap-4 p-6" aria-busy="true" aria-label="Loading">
      <Skeleton className="h-8 w-64" />
      <Skeleton className="h-40 w-full" />
    </div>
  )
}

/** The routes, without a router, so tests can render them in a MemoryRouter. */
export function AppRoutes() {
  return (
    <Suspense fallback={<ScreenFallback />}>
      <Routes>
        <Route path="/" element={<Navigate to="/contracts" replace />} />
        <Route path="/contracts" element={<ContractsListPage />} />
        <Route path="/contracts/new" element={<NewContractPage />} />
        <Route path="/contracts/:id" element={<ContractDetailPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </Suspense>
  )
}

export default function App() {
  return (
    <div data-testid="app" className="bg-background text-foreground min-h-screen">
      <BrowserRouter>
        <AppRoutes />
      </BrowserRouter>
    </div>
  )
}
