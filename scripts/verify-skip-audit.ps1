# Same checks as verify.ps1, without `npm audit`.
# Used while the accepted picomatch advisory (GHSA-fc5q-39xc-vjpf, through vite)
# is still open, so lint, typecheck, web tests, build and size still run.
# Stops at the first failing step.
$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    $defaultDotnet = Join-Path $env:ProgramFiles 'dotnet'
    if (Test-Path (Join-Path $defaultDotnet 'dotnet.exe')) {
        $env:Path = "$defaultDotnet;$env:Path"
    } else {
        Write-Error 'dotnet was not found. Install the .NET 10 SDK, then reopen the terminal.'
        exit 1
    }
}

dotnet restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet build --no-restore -warnaserror
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet test --no-build
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (-not (Get-Command npm.cmd -ErrorAction SilentlyContinue)) {
    $defaultNode = Join-Path $env:ProgramFiles 'nodejs'
    if (Test-Path (Join-Path $defaultNode 'npm.cmd')) {
        $env:Path = "$defaultNode;$env:Path"
    } else {
        Write-Error 'npm was not found. The frontend needs Node.js (current LTS, version 20 or later) with npm. Install it, then reopen the terminal.'
        exit 1
    }
}

Push-Location (Join-Path $PSScriptRoot '..\web')
try {
    $frontendSteps = @(
        @('i'),
        @('run', 'lint'),
        @('run', 'typecheck'),
        @('test', '--', '--run'),
        @('run', 'build'),
        @('run', 'size')
    )
    foreach ($step in $frontendSteps) {
        Write-Host "npm $($step -join ' ')"
        & npm.cmd @step
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Frontend step failed: npm $($step -join ' ')"
            exit $LASTEXITCODE
        }
    }
} finally {
    Pop-Location
}

exit 0
