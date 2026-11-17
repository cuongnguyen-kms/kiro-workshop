# Verification for the whole solution. Stops at the first failing step.
# Later units add their own checks to this file.
$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')

# A terminal opened before the .NET SDK was installed may not have it on PATH yet.
# Fall back to the default install folder for this run only.
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

# ---------------------------------------------------------------------------
# Frontend (u3-contract-web): runs in web/ after the .NET steps.
# ---------------------------------------------------------------------------

# A terminal opened before Node.js was installed may not have it on PATH yet.
# Fall back to the default install folder for this run only.
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
        @('ci'),
        @('audit', '--omit=dev', '--audit-level=high'),
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
