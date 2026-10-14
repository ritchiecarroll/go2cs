<#
.SYNOPSIS
    Maps one hand-owned file to the upstream Go source it replaces, in each of two releases.
    Dot-sourced by handown-census.ps1 and by src/tests/handown-census-selftest.ps1.
.DESCRIPTION
    src/core/<pkg-path>/[<goos>/]<name>[_impl].cs  ->  <GOROOT>/src/<pkg-path>/<name>.go

    Resolve-HandOwnUpstream returns an object with:
      From, To      the upstream path (relative to <GOROOT>/src) used for each release
      FromExists, ToExists
#>

function Resolve-HandOwnUpstream([string] $Rel, [string] $FromGoRoot, [string] $ToGoRoot) {
    $goosDirs = @('windows', 'linux', 'darwin')
    $parts = $Rel -split '[\\/]'
    $file = $parts[-1]
    # DROP THE LEAF via Select-Object, never `$x[0..($x.Count - 2)]`. On a SINGLE-element array that
    # range is 0..-1, which PowerShell counts DOWN to @(0, -1) -- and -1 is the LAST index, so the
    # slice returns the one element TWICE instead of nothing. This script sets no StrictMode, so the
    # result is not an error: it is a silently DOUBLED path segment, and the row it mismaps lands in
    # `no-upstream-counterpart`, the largest bucket, looking like an ordinary finding.
    $dirParts = @($parts | Select-Object -SkipLast 1)
    if ($dirParts.Count -gt 0 -and $goosDirs -contains $dirParts[-1]) {
        # layout L3: per-GOOS folder is routing, not package path. `-gt 0` is the right guard HERE
        # only because -SkipLast 1 yields @() on a one-element array; the old slice yielded @(x, x).
        $dirParts = @($dirParts | Select-Object -SkipLast 1)
    }
    $goName = ([System.IO.Path]::GetFileNameWithoutExtension($file) -replace '_impl$', '') + '.go'
    $upRel = (@($dirParts) + $goName) -join '/'

    return [pscustomobject]@{
        From       = $upRel
        To         = $upRel
        FromExists = Test-Path (Join-Path (Join-Path $FromGoRoot 'src') $upRel)
        ToExists   = Test-Path (Join-Path (Join-Path $ToGoRoot 'src') $upRel)
    }
}
