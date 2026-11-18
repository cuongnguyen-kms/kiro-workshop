# Reverse Engineering Timestamp

## Run Record

- Date: 2026-11-18
- Commit: 49b7e5c16a00295eca08a6df997592ebce5e8a5a

## Scope of Analysis

```yaml
scope_version: 1
kind: partial
intent: expense-balance-columns
fingerprint: 033804a12abc2eddf73a5d0c66cd7eda0b033aea
analyzed:
  paths:
    - src/ContractManagement.Scheduling/
    - src/ContractManagement.Api/Contracts/
    - src/ContractManagement.Api/Services/
    - src/ContractManagement.Api/Persistence/
    - web/src/contracts/
    - web/src/components/
    - web/src/api/
  components:
    - Scheduling Library
    - Contract API
    - Contract Persistence
    - Contract Web App
shallow:
  paths:
    - ./
    - src/ContractManagement.Api/Persistence/Migrations/
    - src/ContractManagement.Api/Endpoints/
    - src/ContractManagement.Api/Domain/
    - web/src/components/ui/
    - tests/
```
