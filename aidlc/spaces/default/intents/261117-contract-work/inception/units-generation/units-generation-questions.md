# Units Generation Questions

Context: Domain Design produced five components: ContractWebApp (React), ContractApi, ContractService, ScheduleGenerator and ContractRepository (.NET). A unit of work is a piece that is designed, built and tested on its own in Construction.

## Q1. [Architect] How should the components be grouped into units?

More units means smaller, separately tested pieces; fewer units means less coordination.

A. Three units: the schedule rules library (ScheduleGenerator), the backend service (ContractApi, ContractService, ContractRepository), and the React frontend (ContractWebApp)
B. Two units: the backend (all four .NET components) and the React frontend
C. One unit holding everything
X. Other (please specify)

[Answer]: A

## Q2. [Developer] How should the system be deployed?

This sets how many things run in production and how the frontend reaches the API.

A. One deployable: the .NET API also serves the built React app from the same address
B. Two deployables: the .NET API and the React app (static files) are deployed separately, and the API allows the frontend's address to call it
X. Other (please specify)

[Answer]: B
