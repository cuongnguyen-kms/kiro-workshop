import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { createContract } from '@/api/contracts'
import type { ValidationErrors } from '@/api/types'
import { ContractForm, type SaveStatus } from '@/contracts/ContractForm'
import { toCreateRequest, type ContractFormValues } from '@/contracts/validation'

export interface SavedState {
  saved: true
}

/** S2: hosts the form, sends the request once, and handles the outcome (workflow W1). */
export default function NewContractPage() {
  const navigate = useNavigate()
  const [status, setStatus] = useState<SaveStatus>('idle')
  const [apiErrors, setApiErrors] = useState<ValidationErrors | undefined>()
  const inFlight = useRef(false)
  const mounted = useRef(true)

  useEffect(() => {
    mounted.current = true
    return () => {
      mounted.current = false
    }
  }, [])

  const save = async (values: ContractFormValues) => {
    if (inFlight.current) return
    inFlight.current = true
    // The saving state is set before the request is sent, so feedback shows on the next render (NFR5.2).
    setStatus('saving')
    const result = await createContract(toCreateRequest(values))
    inFlight.current = false
    if (!mounted.current) return
    if (result.kind === 'ok') {
      const state: SavedState = { saved: true }
      navigate(`/contracts/${result.contract.id}`, { state })
      return
    }
    if (result.kind === 'validation') {
      setStatus('idle')
      setApiErrors(result.errors)
      return
    }
    // Any other failure: keep every entry and never resend on our own (AC6.1.2).
    setStatus('failed')
  }

  return (
    <main className="mx-auto grid max-w-4xl gap-6 p-6" data-testid="new-contract-page">
      <nav aria-label="Breadcrumb">
        <Link to="/contracts" className="text-sm underline underline-offset-2">
          Contracts
        </Link>
      </nav>
      <h1 className="text-2xl font-semibold">New contract</h1>
      <ContractForm
        status={status}
        apiErrors={apiErrors}
        onSubmit={(values) => void save(values)}
        onCancel={() => navigate('/contracts')}
      />
    </main>
  )
}
