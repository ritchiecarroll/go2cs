<#
.SYNOPSIS
    The pack's path guard: every assembly and symbol file in every package of a feed is read, and one that embeds an
    ABSOLUTE path is named by package and entry.

.DESCRIPTION
    A .NET assembly built with symbols records its .pdb's path in its PE debug directory, whether or not the .pdb ships,
    and a [CallerFilePath] literal records a source path in its user-string heap. Unmapped, both are the pack box's own
    tree: read 2026-10-06 on the published 1.24.13.4, each of its 492 assemblies embedded exactly one absolute path, its
    .pdb path under the pack tree (no profile root, no account token, but a path the packages should not carry).
    push-nuget.ps1 builds with ContinuousIntegrationBuild=true, which maps the source root to /_/, and calls
    Get-GoPackagePathLeaks after packing: any finding refuses the pack.

    What counts as absolute: a drive-letter or UNC path ending in .pdb or .cs, in an entry's bytes read as ASCII and as
    UTF-16 (both alignments). A portable .pdb stores a document path as separator-joined PARTS, so its whole path is
    never in the bytes: its document names are DECODED by check-pack-pdbs.cs, run once per feed through `dotnet` (the
    release pack runs under Windows PowerShell 5.1, which has no System.Reflection.Metadata, and the SDK is on every
    pack host), and any absolute one is a finding; so is a .pdb it cannot read. The reader's count of .pdb entries must
    equal this script's, or the guard throws: a reader that read less must not pass. A mapped /_/ path is not a
    finding. (A byte heuristic for the drive part was tried and rejected: it matched random bytes of a fully mapped
    .pdb, measured 2026-10-06.)

    Standalone: pwsh ./check-pack-paths.ps1 -Feed <folder of .nupkg>   (exit 1 and the findings when any; 0 when clean)
    Dot-sourced (push-nuget.ps1): only the function is defined.

.PARAMETER Feed
    A folder of .nupkg files.
#>
#Requires -Version 5.1
param([string] $Feed)

Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem

# An absolute path ending in .pdb or .cs: a drive letter or a UNC root, then path characters.
$script:GoAbsolutePathPattern = [regex] '(?:(?<![A-Za-z0-9])[A-Za-z]:[\\/]|\\\\[A-Za-z0-9])[^\x00-\x1F"<>|*?]{1,240}?\.(?:pdb|cs)(?![A-Za-z0-9])'

# The symbol-file reader beside this script, resolved now: inside a function dot-sourced into push-nuget.ps1 the
# script root would be the caller's.
$script:GoPackPdbReader = Join-Path $PSScriptRoot 'check-pack-pdbs.cs'

# The DECODED .pdb verdicts of a feed: check-pack-pdbs.cs, run once through `dotnet` from a temporary copy (so no
# Directory.Build file of the tree it sits in applies to it). Returns the findings, "<nupkg> :: <entry>" -> reason, and
# the number of .pdb entries the reader read. Its stderr is left on the console: under Windows PowerShell 5.1 a
# redirected native stderr line becomes an error record, which the caller's Stop preference would throw on.
function Invoke-GoPdbReader([string] $Feed) {
    $work = Join-Path ([System.IO.Path]::GetTempPath()) ("go-pack-pdbs-" + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
    New-Item -ItemType Directory -Path $work | Out-Null

    try {
        $app = Join-Path $work 'check-pack-pdbs.cs'
        Copy-Item -LiteralPath $script:GoPackPdbReader -Destination $app
        $lines = @(& dotnet run --file $app -- (Resolve-Path -LiteralPath $Feed).ProviderPath)
        if ($LASTEXITCODE -ne 0) {
            $lines | ForEach-Object { Write-Host "  $_" }
            throw "pack paths: the symbol-file reader failed (exit $LASTEXITCODE)"
        }
    }
    finally { Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue }

    $findings = @{}
    $read = @()

    foreach ($line in $lines) {
        if ("$line" -match '^(ABSOLUTE|UNREADABLE)\t([^\t]+)\t(.+)$') {
            $findings["$($Matches[2]) :: $($Matches[3])"] = if ($Matches[1] -eq 'ABSOLUTE') { 'absolute path' } else { 'symbol file unreadable' }
        }
        elseif ("$line" -match '^READ\t(\d+)$') { $read += [int]$Matches[1] }
    }

    if ($read.Count -ne 1) { throw "pack paths: the symbol-file reader reported no count -- nothing it said can be trusted" }
    return [pscustomobject]@{ Findings = $findings; Read = $read[0] }
}

# 'absolute path' when an entry's bytes hold one, read as ASCII and as UTF-16; $null when they do not.
function Get-GoEntryPathFinding([byte[]] $Bytes) {
    $latin1 = [System.Text.Encoding]::GetEncoding(28591).GetString($Bytes)
    if ($script:GoAbsolutePathPattern.IsMatch($latin1)) { return 'absolute path' }

    foreach ($offset in 0, 1) {
        if ($Bytes.Length - $offset -lt 2) { continue }
        $utf16 = [System.Text.Encoding]::Unicode.GetString($Bytes, $offset, $Bytes.Length - $offset - (($Bytes.Length - $offset) % 2))
        if ($script:GoAbsolutePathPattern.IsMatch($utf16)) { return 'absolute path' }
    }

    return $null
}

# Every (package, entry) of a feed whose assembly or symbol file embeds an absolute path. Never prints a path.
function Get-GoPackagePathLeaks([string] $Feed) {
    # A guard that reads nothing must not pass: a missing feed, or one with no package, is an error.
    if (-not $Feed -or -not (Test-Path -LiteralPath $Feed -PathType Container)) { throw "pack paths: no feed folder at '$Feed'" }
    $nupkgs = @(Get-ChildItem -LiteralPath $Feed -Filter '*.nupkg' -File | Sort-Object Name)
    if ($nupkgs.Count -eq 0) { throw "pack paths: no .nupkg in '$Feed' -- nothing was read, so nothing can be called clean" }

    $findings = New-Object System.Collections.Generic.List[string]
    $named = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
    $symbolFiles = 0

    foreach ($nupkg in $nupkgs) {
        $zip = [System.IO.Compression.ZipFile]::OpenRead($nupkg.FullName)
        try {
            foreach ($entry in $zip.Entries) {
                $extension = [System.IO.Path]::GetExtension($entry.FullName).ToLowerInvariant()
                if ($extension -notin '.dll', '.exe', '.winmd', '.pdb') { continue }

                $stream = $entry.Open()
                try {
                    $buffer = New-Object System.IO.MemoryStream
                    $stream.CopyTo($buffer)
                }
                finally { $stream.Dispose() }

                if ($extension -eq '.pdb') { $symbolFiles++ }

                $reason = Get-GoEntryPathFinding $buffer.ToArray()
                if ($reason -and $named.Add("$($nupkg.Name) :: $($entry.FullName)")) { $findings.Add("$($nupkg.Name) :: $($entry.FullName) ($reason)") }
            }
        }
        finally { $zip.Dispose() }
    }

    if ($symbolFiles -gt 0) {
        $pdbs = Invoke-GoPdbReader $Feed
        if ($pdbs.Read -ne $symbolFiles) { throw "pack paths: the symbol-file reader read $($pdbs.Read) .pdb of the $symbolFiles in the feed" }

        foreach ($key in @($pdbs.Findings.Keys | Sort-Object)) {
            if ($named.Add($key)) { $findings.Add("$key ($($pdbs.Findings[$key]))") }
        }
    }

    return , $findings.ToArray()
}

if ($MyInvocation.InvocationName -ne '.' -and $Feed) {
    $found = Get-GoPackagePathLeaks $Feed
    $count = @(Get-ChildItem -LiteralPath $Feed -Filter '*.nupkg' -File).Count
    if ($found.Count -eq 0) {
        Write-Host "Pack paths: CLEAN -- no absolute path in any assembly or symbol file of $count package(s)."
        exit 0
    }
    Write-Host "Pack paths: REFUSED -- $($found.Count) entr(y/ies) of $count package(s):"
    $found | ForEach-Object { Write-Host "  $_" }
    exit 1
}
