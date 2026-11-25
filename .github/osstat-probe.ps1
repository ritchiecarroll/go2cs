<#
.SYNOPSIS
    MEASUREMENT ONLY, never merged (COORD 2026-11-25T02:01:40Z, loadavg W3 and W2): convert
    github.com/mackerelio/go-osstat v0.2.8 with -recurse=nuget -cgo on this runner's own target, with this leg's C
    compiler, and report the exit code, every refusal line, and which packages converted (golang.org/x/sys included).
#>
$ErrorActionPreference = 'Continue'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$work = Join-Path ([System.IO.Path]::GetTempPath()) "osstat-probe-$PID"
New-Item -ItemType Directory -Force -Path $work | Out-Null
$target = "$(& go env GOOS)/$(& go env GOARCH)".Trim()
$env:CGO_ENABLED = '1'
Write-Host "PROBE target $target; go env CC = $("$(& go env CC)".Trim())"

$conv = Join-Path $work $(if ($IsWindows) { 'go2cs.exe' } else { 'go2cs' })
Push-Location (Join-Path $repo 'src/go2cs'); & go build -o $conv .; $b = $LASTEXITCODE; Pop-Location
if ($b -ne 0) { Write-Host "PROBE converter build failed ($b)"; exit 1 }

$mod = Join-Path $work 'mod'; New-Item -ItemType Directory -Force -Path $mod | Out-Null
Push-Location $mod
& go mod init probe *> $null
$json = (& go mod download -json github.com/mackerelio/go-osstat@v0.2.8) -join "`n" | ConvertFrom-Json
Pop-Location
$src = Join-Path $work 'osstat'
Copy-Item -Recurse -LiteralPath $json.Dir -Destination $src
Get-ChildItem -LiteralPath $src -Recurse -File | ForEach-Object { $_.IsReadOnly = $false }
Push-Location $src; & go mod download *> $null; Pop-Location

foreach ($pkg in @('loadavg', 'cpu', 'memory', 'uptime', 'network', 'disk')) {
    Push-Location $src
    $files = "$(& go list -e -f '{{.CgoFiles}} {{.GoFiles}}' "./$pkg" 2>&1)"
    Pop-Location
    Write-Host "PROBE files $pkg $files"
}

$out = Join-Path $work 'out'
$log = Join-Path $work 'convert.log'
& $conv -recurse=nuget -cgo=true -platforms $target -go2cspath (Join-Path $repo 'src') $src $out *> $log
$rc = $LASTEXITCODE
Write-Host "PROBE convert rc=$rc"
Get-Content -LiteralPath $log | Where-Object { $_ -match 'GO2CS|Refus|refus|error|panic' } | ForEach-Object { Write-Host "PROBE line $_" }
$projects = @(Get-ChildItem -LiteralPath $out -Recurse -File -Filter '*.csproj' -ErrorAction SilentlyContinue | ForEach-Object { $_.Name })
Write-Host "PROBE projects $($projects.Count): $($projects -join ', ')"
$cgo = @(Get-ChildItem -LiteralPath $out -Recurse -File -Filter '*.cgo.cs' -ErrorAction SilentlyContinue | ForEach-Object { $_.Name })
Write-Host "PROBE cgo bindings: $($cgo -join ', ')"
foreach ($f in @(Get-ChildItem -LiteralPath $out -Recurse -File -Filter '*.cgo.cs' -ErrorAction SilentlyContinue)) {
    Select-String -LiteralPath $f.FullName -Pattern 'LibraryImport\(' | ForEach-Object { Write-Host "PROBE bind $($f.Name): $($_.Line.Trim())" }
}
"## osstat probe -- $target`n`nconvert rc=$rc, $($projects.Count) projects" | Add-Content -Path $env:GITHUB_STEP_SUMMARY
exit 0
