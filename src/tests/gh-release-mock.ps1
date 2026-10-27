#Requires -Version 5.1
<#
.SYNOPSIS
    A stand-in for the GitHub CLI, serving ONE draft Release from a local folder, for the release-signing selftest.

.DESCRIPTION
    Answers only the gh calls sign-release-binaries.ps1 makes, against the state under $env:GH_MOCK_ROOT:

      release.json   {"tag": "...", "isDraft": true}
      assets/        the Release's assets
      attested.txt   the SHA-256 digests (lower-case hex, one per line) the repository holds attestations for
      calls.log      every call, appended one line each

      gh release view <tag> --repo R --json isDraft,assets
      gh release download <tag> --repo R --dir D [--clobber]
      gh release upload <tag> <file>... --repo R --clobber
      gh release delete-asset <tag> <name> --repo R --yes
      gh attestation verify <file> --repo R

    Anything else exits 2. Nothing here reaches GitHub.
#>
$ErrorActionPreference = 'Stop'

$root = $env:GH_MOCK_ROOT
if (-not $root) { [Console]::Error.WriteLine('gh mock: GH_MOCK_ROOT is not set'); exit 2 }

Add-Content -Path (Join-Path $root 'calls.log') -Value ($args -join ' ')

$assets = Join-Path $root 'assets'
$release = Get-Content -Raw (Join-Path $root 'release.json') | ConvertFrom-Json

# The positional arguments, with each --flag and its value removed; --clobber and --yes take none.
function Get-Positionals([object[]] $List) {
    $out = @()
    for ($i = 0; $i -lt $List.Count; $i++) {
        $a = "$($List[$i])"
        if ($a -in @('--clobber', '--yes')) { continue }
        if ($a.StartsWith('--')) { $i++; continue }
        $out += $a
    }
    $out
}

function Get-Flag([object[]] $List, [string] $Name) {
    for ($i = 0; $i -lt $List.Count - 1; $i++) { if ("$($List[$i])" -eq $Name) { return "$($List[$i + 1])" } }
    $null
}

function Test-Tag([string] $Tag) {
    if ($Tag -ne $release.tag) { [Console]::Error.WriteLine("release not found"); exit 1 }
}

$verb = "$($args[0]) $($args[1])"
$rest = @($args | Select-Object -Skip 2)
$positionals = @(Get-Positionals $rest)

switch ($verb) {
    'release view' {
        Test-Tag $positionals[0]
        $list = @(Get-ChildItem $assets -File | Sort-Object Name | ForEach-Object { [ordered]@{ name = $_.Name; size = $_.Length } })
        [ordered]@{ isDraft = [bool]$release.isDraft; assets = $list } | ConvertTo-Json -Depth 4 -Compress
        exit 0
    }
    'release download' {
        Test-Tag $positionals[0]
        $dir = Get-Flag $rest '--dir'
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
        Get-ChildItem $assets -File | ForEach-Object { Copy-Item $_.FullName (Join-Path $dir $_.Name) -Force }
        exit 0
    }
    'release upload' {
        Test-Tag $positionals[0]
        foreach ($file in @($positionals | Select-Object -Skip 1)) { Copy-Item $file (Join-Path $assets (Split-Path $file -Leaf)) -Force }
        exit 0
    }
    'release delete-asset' {
        Test-Tag $positionals[0]
        $path = Join-Path $assets $positionals[1]
        if (-not (Test-Path $path)) { [Console]::Error.WriteLine("asset not found: $($positionals[1])"); exit 1 }
        Remove-Item $path
        exit 0
    }
    'attestation verify' {
        $digest = (Get-FileHash $positionals[0] -Algorithm SHA256).Hash.ToLowerInvariant()
        $attested = @(Get-Content (Join-Path $root 'attested.txt') | ForEach-Object { $_.Trim() })
        if ($attested -contains $digest) { Write-Output "Loaded 1 attestation from GitHub API"; exit 0 }
        [Console]::Error.WriteLine("no attestations found for subject sha256:$digest"); exit 1
    }
}

[Console]::Error.WriteLine("gh mock: unsupported call: $($args -join ' ')")
exit 2
