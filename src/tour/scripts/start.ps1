[CmdletBinding()]
param(
    [string]$ListenAddress = "127.0.0.1:4000",
    [ValidateSet("core", "deployed", "nuget")]
    [string]$Runtime,
    [switch]$NoOpen,
    # Anything else is handed to the server, so every option it takes is
    # reachable from here -- as start.sh already passes "$@" through.
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$ServerArguments
)

$ErrorActionPreference = "Stop"

# The x/website revision the offline Tour is installed from: 52d470bcf26e (2026-10-21), whose go.mod
# says go 1.26.0, which the converter's Go 1.26.10 satisfies. Never @latest: that can
# declare a newer Go than the converter's, so the install fails under GOTOOLCHAIN=local and
# silently downloads another toolchain under auto. This moves with the corpus's Go release (the
# Go 1.27 hop bumps it); keep it equal to start.sh and to tourModule in tour.go.
$TourVersion = "v0.0.0-20261021155534-52d470bcf26e"

# The executable suffix comes from the shared definition at src\_paths.ps1. start.sh remains the
# Linux entry point (§A5 of docs/PLAN-linux-operation.md keeps the twin deliberately -- it is 19
# lines of `go install` + `go run` with no invariants to drift), but there is no reason for THIS
# script to be the one file in the tree that still cannot run under pwsh.
. (Join-Path $PSScriptRoot '../../_paths.ps1')

$tourRoot = Split-Path -Parent $PSScriptRoot
$repoRoot = (Resolve-Path (Join-Path $tourRoot '../..')).Path

# A machine with only the go2cs dotnet tool installed has no go on PATH. The Go the tool downloads
# for its conversions is the release the tour needs too, so use it (`go2cs toolchain env`).
if (-not (Get-Command go -ErrorAction SilentlyContinue) -and (Get-Command go2cs -ErrorAction SilentlyContinue)) {
    $goCommand = (& go2cs toolchain env | Where-Object { $_ -like 'GO=*' } | Select-Object -First 1) -replace '^GO=', ''
    if ($goCommand) {
        Write-Host "No go on PATH: using the Go the go2cs dotnet tool provides, $goCommand"
        $env:PATH = (Split-Path -Parent $goCommand) + [System.IO.Path]::PathSeparator + $env:PATH
    }
}

Write-Host "Checking Go..."
& go version

Write-Host "Checking .NET..."
& dotnet --version

$goPath = (& go env GOPATH).Trim()
$tourBinary = Join-Path $goPath "bin/tour$ExeSuffix"
if (-not (Test-Path -LiteralPath $tourBinary)) {
    Write-Host "Installing the official offline Tour of Go..."
    & go install golang.org/x/website/tour@$TourVersion
}

$env:GO_TOUR_BIN = $tourBinary

Push-Location $tourRoot
try {
    $arguments = @("run", ".", "-addr=$ListenAddress", "-repo=$repoRoot")
    if ($Runtime) {
        $arguments += "-runtime=$Runtime"
    }
    if ($NoOpen) {
        $arguments += "-no-open"
    }
    if ($ServerArguments) {
        $arguments += $ServerArguments
    }
    & go @arguments
}
finally {
    Pop-Location
}
