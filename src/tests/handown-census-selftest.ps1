<#
.SYNOPSIS
    Guards handown-census.ps1's hand-own -> upstream mapping (src/handown-upstream.ps1) on FABRICATED GOROOTs.

.DESCRIPTION
    Builds two temporary GOROOTs, From and To, and checks Resolve-HandOwnUpstream on six layouts:
      plain       pkg/x_impl.cs, upstream pkg/x.go in both                     -> pkg/x.go both, not relocated
      l3          pkg/linux/x_impl.cs, upstream pkg/x.go in both (layout L3)   -> pkg/x.go both, not relocated
      moved       pkg/linux/x_impl.cs, pkg/x.go at From, pkg/linux/x.go at To  -> RELOCATED pkg/x.go -> pkg/linux/x.go
                  (Go 1.26's internal/runtime/syscall -> internal/runtime/syscall/linux)
      moved-back  pkg/linux/x_impl.cs, pkg/linux/x.go at From, pkg/x.go at To  -> RELOCATED, the other direction
      real-goos   pkg/linux/x_impl.cs, pkg/linux/x.go in both, pkg/x.go in none -> pkg/linux/x.go both, not relocated
                  (a real package named after a GOOS, read where the L3 fold finds nothing)
      vanished    pkg/linux/x_impl.cs, pkg/x.go at From only, nothing at To    -> pkg/x.go both, not relocated
                  (a true deletion stays a deletion)

    Exit 0 clean, 1 on any violation. Writes only under the host's temp directory.
#>
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '../handown-upstream.ps1')

$failures = New-Object System.Collections.Generic.List[string]
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("handown-census-selftest-" + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
$from = Join-Path $work 'from'
$to = Join-Path $work 'to'

function Plant([string] $Root, [string] $Rel) {
    $path = Join-Path (Join-Path $Root 'src') $Rel
    New-Item -ItemType Directory -Force -Path (Split-Path $path) | Out-Null
    Set-Content -LiteralPath $path -Value "package x // $Rel" -Encoding ascii
}

function Expect([string] $Case, [string] $Rel, [string] $WantFrom, [string] $WantTo, [bool] $WantRelocated) {
    $r = Resolve-HandOwnUpstream $Rel $from $to
    $relocated = [bool]($r.PSObject.Properties.Name -contains 'Relocated' -and $r.Relocated)
    if ($r.From -ne $WantFrom -or $r.To -ne $WantTo -or $relocated -ne $WantRelocated) {
        $failures.Add("${Case}: got From=$($r.From) To=$($r.To) Relocated=$relocated; want From=$WantFrom To=$WantTo Relocated=$WantRelocated")
    }
    else {
        Write-Host "  ok  $Case  $($r.From) -> $($r.To)$(if ($relocated) { '  (relocated)' })"
    }
}

try {
    Plant $from 'plain/x.go';        Plant $to 'plain/x.go'
    Plant $from 'l3/x.go';           Plant $to 'l3/x.go'
    Plant $from 'moved/x.go';        Plant $to 'moved/linux/x.go'
    Plant $from 'back/linux/x.go';   Plant $to 'back/x.go'
    Plant $from 'real/linux/x.go';   Plant $to 'real/linux/x.go'
    Plant $from 'gone/x.go'

    Expect 'plain'      'plain\x_impl.cs'       'plain/x.go'       'plain/x.go'       $false
    Expect 'l3'         'l3\linux\x_impl.cs'    'l3/x.go'          'l3/x.go'          $false
    Expect 'moved'      'moved\linux\x_impl.cs' 'moved/x.go'       'moved/linux/x.go' $true
    Expect 'moved-back' 'back\linux\x_impl.cs'  'back/linux/x.go'  'back/x.go'        $true
    Expect 'real-goos'  'real\linux\x_impl.cs'  'real/linux/x.go'  'real/linux/x.go'  $false
    Expect 'vanished'   'gone\linux\x_impl.cs'  'gone/x.go'        'gone/x.go'        $false
}
finally {
    Remove-Item -Recurse -Force -LiteralPath $work -ErrorAction SilentlyContinue
}

if ($failures.Count) {
    foreach ($f in $failures) { Write-Host "FAIL  $f" }
    Write-Host "handown-census-selftest: $($failures.Count) failure(s)"
    exit 1
}
Write-Host 'handown-census-selftest: clean'
exit 0
