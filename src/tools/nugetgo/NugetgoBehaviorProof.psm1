# NugetgoBehaviorProof.psm1 - Gbtc
# Copyright (c) 2026 The go2cs Authors. All rights reserved.
#
# SPDX-License-Identifier: AGPL-3.0-only
# Use of this source code is governed by the GNU Affero General Public License
# version 3 only, which can be found in the LICENSE file.

# THE BEHAVIORAL PROOF OF A MODULE WITH NO GO TESTS (COORD ruling 2026-10-12, the cobra increment; mousetrap first).
#
# `go2cs -recurse -tests` refuses a module with no Go test files, so such a module has no comparison to ship as
# VALIDATION.md. It is proven by behavior instead, and the page says so plainly. Each shipped RID has one ROW, written
# ON that RID's own host (a hosted runner of that OS) by Invoke-NugetgoBehaviorRow.ps1: a sample program converted
# against the package is run with dotnet and with go, and the row records both outputs' hashes, the host, and the hash
# of the module assembly that actually loaded. A row never stands for another RID: identical IL can still behave
# differently on another OS, and the page would claim a run that never happened.
#
# A row is a small text file, <rid>.md: a title line, then one "key: value" per line.

# Without -Force: nugetgo-pack.ps1 has already imported NugetgoFlavors.psm1, and a forced re-import from inside this
# module would unload the pack's copy (its functions then read "not recognized"). The map is read once, at import.
Import-Module (Join-Path $PSScriptRoot 'NugetgoFlavors.psm1')

$script:RowKeys = @('module', 'version', 'package', 'rid', 'runtime-rid', 'goos', 'host', 'os', 'go', 'dotnet', 'go-output', 'dotnet-output', 'outputs-equal', 'assembly', 'date')
# The release's RID map (NugetgoFlavors.psm1): the RIDs a row is owed for, and the GOOS each one's row is recorded on.
$script:RidGoos = Get-NugetgoRidFlavors
$script:ShippedRids = @($script:RidGoos.Keys)
# The title of a row's annotation. The fetch script selects by it, so it never collides with a leg's summary chunks or
# a runner warning in the same step (C1, 2026-10-12).
$script:RowTitle = 'nugetgo-row'

function Get-NugetgoBehaviorRowKeys { $script:RowKeys }

function Read-NugetgoBehaviorRow {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Path)
    $row = [ordered]@{ path = $Path; file = [System.IO.Path]::GetFileNameWithoutExtension($Path) }
    foreach ($line in [System.IO.File]::ReadAllLines($Path)) {
        if ($line -match '^(?<k>[a-z-]+): (?<v>.*)$') { $row[$Matches['k']] = $Matches['v'] }
    }
    [pscustomobject]$row
}

function Get-NugetgoSha256([byte[]]$Bytes) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try { 'sha256-' + ([BitConverter]::ToString($sha.ComputeHash($Bytes)) -replace '-', '').ToLowerInvariant() }
    finally { $sha.Dispose() }
}

# Every reason a set of rows does not prove the packed bytes. -Rows maps each row file's RID to its row;
# -Assemblies maps each shipped RID to the assembly file name -> bytes the package carries for it (its runtimes/<rid>
# folder, or lib/ for every RID when the package is neutral). No reason means accepted.
function Test-NugetgoBehaviorRows {
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Rows, [Parameter(Mandatory)]$Assemblies, [Parameter(Mandatory)][string]$ModulePath, [Parameter(Mandatory)][string]$GoVersion)

    $reasons = New-Object System.Collections.Generic.List[string]
    foreach ($rid in $script:ShippedRids) {
        if (-not $Rows.ContainsKey($rid) -or $null -eq $Rows[$rid]) { $reasons.Add("no row for ${rid}: every shipped RID is run on its own host"); continue }
        $row = $Rows[$rid]
        foreach ($key in $script:RowKeys) {
            if ([string]::IsNullOrWhiteSpace([string]$row.$key)) { $reasons.Add("[$rid] the row has no $key") }
        }
        if ($row.rid -cne $rid) { $reasons.Add("[$rid] the row's rid is '$($row.rid)', not its file's") }
        if ($row.'runtime-rid' -cne $rid) { $reasons.Add("[$rid] the row was recorded on $($row.'runtime-rid'), not on ${rid}: a row never stands for another RID") }
        if ($row.goos -and $row.goos -cne $script:RidGoos[$rid]) { $reasons.Add("[$rid] the row was recorded on GOOS $($row.goos), not ${rid}'s $($script:RidGoos[$rid]): a row never stands for another OS") }
        if ($row.module -cne $ModulePath -or $row.version -cne $GoVersion) { $reasons.Add("[$rid] the row is for $($row.module) $($row.version), not $ModulePath $GoVersion") }
        if ($row.'outputs-equal' -cne 'true' -or $row.'go-output' -cne $row.'dotnet-output') { $reasons.Add("[$rid] dotnet run did not print what go run printed (go $($row.'go-output'), dotnet $($row.'dotnet-output'))") }
        $packed = $Assemblies[$rid]
        if ($null -eq $packed) { $reasons.Add("[$rid] the package carries no assembly for $rid"); continue }
        # One "<file> sha256-<hex>" entry per assembly, joined by "; ": a module of several packages binds them all.
        $ran = @{}
        $malformed = @([string]$row.assembly -split '; ' | Where-Object { $_ -notmatch '^\S+ sha256-[0-9a-f]{64}$' })
        if ($malformed.Count) { $reasons.Add("[$rid] the row's assembly '$($row.assembly)' is not '<file> sha256-<hex>' entries joined by '; '"); continue }
        foreach ($entry in [string]$row.assembly -split '; ') { $name, $sha = $entry -split ' ', 2; $ran[$name] = $sha }
        foreach ($name in @($ran.Keys | Sort-Object)) {
            if (-not $packed.ContainsKey($name)) { $reasons.Add("[$rid] the row ran $name, which the package does not carry for $rid") }
            elseif ((Get-NugetgoSha256 $packed[$name]) -cne $ran[$name]) { $reasons.Add("[$rid] the row ran $name at $($ran[$name]), which is not the packed bytes for $rid ($(Get-NugetgoSha256 $packed[$name]))") }
        }
        foreach ($name in @($packed.Keys | Sort-Object)) {
            if (-not $ran.ContainsKey($name)) { $reasons.Add("[$rid] the row does not list $name, which the package carries for ${rid}: a row binds every packed assembly") }
        }
    }
    foreach ($rid in $Rows.Keys) { if ($script:ShippedRids -notcontains $rid) { $reasons.Add("a row for $rid, a RID the package does not ship") } }
    $reasons.ToArray()
}

# The page a behaviorally proven package ships as its proof (the form COORD approved, 2026-10-12).
function New-NugetgoBehaviorPage {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ModulePath, [Parameter(Mandatory)][string]$GoVersion, [Parameter(Mandatory)]$Rows)

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("# ``$ModulePath`` -- behavioral proof (upstream has no Go tests)")
    $lines.Add('')
    $lines.Add("``$ModulePath`` $GoVersion ships no Go tests, so this page is not a test comparison. Each row below is a sample")
    $lines.Add('program run on that RID''s own host: the program converted by go2cs against this package, built for that RID and run')
    $lines.Add('with dotnet, and the Go program run with `go run`, printed the same bytes. Each row names the assembly that loaded,')
    $lines.Add('which is the package''s own for that RID, and the host it ran on. No row stands for another RID.')
    $lines.Add('')
    foreach ($line in Get-NugetgoRowTable -Rows $Rows) { $lines.Add($line) }
    ($lines -join "`n") + "`n"
}

# The rows as a table, one line per shipped RID that has a row, in the release's RID order.
function Get-NugetgoRowTable {
    param([Parameter(Mandatory)]$Rows)
    '| RID | Host | Assembly | Outputs equal |'
    '|:--|:--|:--|:--|'
    foreach ($rid in $script:ShippedRids) {
        $row = $Rows[$rid]
        if ($null -eq $row) { continue }
        "| $rid | $($row.host) ($($row.os)) | ``$($row.assembly)`` | $($row.'outputs-equal') (go $($row.go), dotnet $($row.dotnet)) |"
    }
}

# The hosted sample rows of a module WITH Go tests, as a section under its test comparison (COORD ruling 2026-10-12):
# additional evidence, never the validation.
function New-NugetgoSampleBehaviorSection {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ModulePath, [Parameter(Mandatory)][string]$GoVersion, [Parameter(Mandatory)]$Rows)

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("## Sample behavior on $(@($Rows.Keys).Count) RIDs")
    $lines.Add('')
    $lines.Add("In addition to the test comparison above, a sample program using ``$ModulePath`` $GoVersion was run on each RID's own")
    $lines.Add('host: converted by go2cs against this package, built for that RID and run with dotnet, it printed the same bytes as')
    $lines.Add('`go run`. Each row names the assembly that loaded, which is the package''s own for that RID, and the host it ran on.')
    $lines.Add('')
    foreach ($line in Get-NugetgoRowTable -Rows $Rows) { $lines.Add($line) }
    ($lines -join "`n") + "`n"
}

# Does the module have Go tests? The same predicate `go2cs -recurse -tests` refuses on: any _test.go file in the
# module's packages. Files under testdata/ belong to no package.
function Test-NugetgoModuleHasGoTests {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ModuleDir)
    $root = [System.IO.Path]::GetFullPath($ModuleDir).TrimEnd('\', '/')
    foreach ($f in Get-ChildItem -LiteralPath $root -Recurse -File -Filter '*_test.go') {
        $rel = $f.FullName.Substring($root.Length).TrimStart('\', '/')
        if (@($rel -split '[\\/]') -notcontains 'testdata') { return $true }
    }
    $false
}

# Each shipped RID's assemblies, file name -> bytes, as NuGet selects them for a framework-dependent build with that
# RID: runtimes/<rid>/lib/<tfm>/ when the package has that folder, otherwise lib/<tfm>/. A neutral package therefore
# gives every RID its lib/ bytes, and a RID-specific one each RID its own.
function Get-NugetgoPackedRidAssemblies {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Package, [Parameter(Mandatory)][string]$TargetFramework)

    Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
    $entries = @{}
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Package)
    try {
        foreach ($e in $zip.Entries) {
            if ($e.FullName -notlike '*.dll') { continue }
            $ms = New-Object System.IO.MemoryStream
            $st = $e.Open()
            try { $st.CopyTo($ms) } finally { $st.Dispose() }
            $entries[$e.FullName] = $ms.ToArray()
        }
    }
    finally { $zip.Dispose() }
    $byRid = @{}
    foreach ($rid in $script:ShippedRids) {
        $folder = "runtimes/$rid/lib/$TargetFramework/"
        if (-not @($entries.Keys | Where-Object { $_.StartsWith($folder, [StringComparison]::Ordinal) }).Count) { $folder = "lib/$TargetFramework/" }
        $set = @{}
        foreach ($name in $entries.Keys) {
            if ($name.StartsWith($folder, [StringComparison]::Ordinal) -and $name.Substring($folder.Length) -notmatch '/') { $set[$name.Substring($folder.Length)] = $entries[$name] }
        }
        $byRid[$rid] = $set
    }
    $byRid
}

# Which proof a pack carries (D7). A module with Go tests ships its test comparison (-ValidationDir); a module with no
# Go tests ships the behavioral proof (-BehaviorProofDir), because `go2cs -recurse -tests` has nothing to compare; and a
# pack-shape rehearsal ships neither (-UnvalidatedReason). A module WITH Go tests may also carry hosted sample rows
# (-BehaviorProofDir beside -ValidationDir, COORD ruling 2026-10-12): an ADDITIONAL row set, never the validation.
# Reason is a refusal, or $null; SampleRows says the validated mode carries those rows.
function Get-NugetgoProofMode {
    [CmdletBinding()]
    param([string]$ValidationDir, [string]$UnvalidatedReason, [string]$BehaviorProofDir, [switch]$Release, [Parameter(Mandatory)][bool]$ModuleHasGoTests)

    $result = { param($mode, $why, $sample) [pscustomobject]@{ Mode = $mode; Reason = $why; SampleRows = [bool]$sample } }
    if ($ValidationDir -and $BehaviorProofDir -and -not $UnvalidatedReason) {
        if (-not $ModuleHasGoTests) { return & $result $null '-BehaviorProofDir and -ValidationDir are exclusive for a module with no Go tests: its proof is the behavioral one alone' $false }
        return & $result 'validated' $null $true
    }
    $given = @(@{ '-ValidationDir' = $ValidationDir; '-UnvalidatedReason' = $UnvalidatedReason; '-BehaviorProofDir' = $BehaviorProofDir }.GetEnumerator() | Where-Object { $_.Value } | ForEach-Object { $_.Key } | Sort-Object)
    if ($given.Count -gt 1) { return & $result $null "$($given -join ' and ') are exclusive: a package carries one proof" $false }
    if ($UnvalidatedReason) {
        if ($Release) { return & $result $null 'an unvalidated module is never packed as a release (-UnvalidatedReason with -Release)' $false }
        return & $result 'unvalidated' $null $false
    }
    if ($BehaviorProofDir) {
        if ($ModuleHasGoTests) { return & $result $null 'the module has Go tests: its proof is the test comparison (-ValidationDir), and hosted sample rows go beside it, never in its place' $false }
        return & $result 'behavioral' $null $false
    }
    if ($ValidationDir) { return & $result 'validated' $null $false }
    if ($ModuleHasGoTests) { return & $result $null 'a validated module needs -ValidationDir (D7), or -UnvalidatedReason for a pack-shape rehearsal' $false }
    & $result $null 'a module with no Go tests needs -BehaviorProofDir (one row per shipped RID, each run on its own OS), or -UnvalidatedReason for a pack-shape rehearsal' $false
}

# A row as ONE annotation message: its UTF-8 bytes in base64, which needs no workflow-command escaping and fits one
# annotation chunk (a row is ~15 lines). The step passes it to .github/annotate-summary.ps1 with the title
# "nugetgo-row <rid>".
function ConvertTo-NugetgoRowAnnotationText {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Text)
    [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($Text))
}

# The rows a run's annotations carry, RID -> row text. -Annotations are the objects GET .../check-runs/{id}/annotations
# returns (title, message). An annotation whose title is not "nugetgo-row <rid>" is someone else's and is ignored; one
# that is, but does not decode to a row for that RID, is refused by name, never skipped.
function ConvertFrom-NugetgoRowAnnotations {
    [CmdletBinding()]
    param([Parameter(Mandatory)][AllowEmptyCollection()]$Annotations)

    $rows = @{}
    foreach ($a in @($Annotations)) {
        if ([string]$a.title -notmatch "^$([regex]::Escape($script:RowTitle)) (?<rid>\S+)$") { continue }
        $rid = $Matches['rid']
        try { $text = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String(([string]$a.message).Trim())) }
        catch { throw "the annotation '$($a.title)' is not a base64 row: $($_.Exception.Message)" }
        $ridLine = @($text -split "`n" | Where-Object { $_ -match '^rid: ' })
        if ($ridLine.Count -ne 1 -or $ridLine[0] -cne "rid: $rid") { throw "the annotation '$($a.title)' carries a row for '$(($ridLine -join ', ') -replace '^rid: ', '')', not $rid" }
        if ($rows.ContainsKey($rid)) { throw "two annotations carry a row for $rid" }
        $rows[$rid] = $text
    }
    $rows
}

# The host a row names. A hosted runner sets ImageOS and ImageVersion, and the workflow passes the matrix label as
# NUGETGO_RUNNER; a label such as macos-15-intel can move to a new image, so the image version is what makes a row
# reproducible (C1, 2026-10-12). Any other host is named by the caller, never guessed.
function Get-NugetgoRowHost {
    [CmdletBinding()]
    param([Parameter(Mandatory)][hashtable]$Environment, [string]$HostLabel)

    if ($HostLabel) { return $HostLabel }
    if ($Environment.ImageOS -and $Environment.ImageVersion -and $Environment.NUGETGO_RUNNER) { return "$($Environment.NUGETGO_RUNNER) (image $($Environment.ImageOS) $($Environment.ImageVersion))" }
    throw 'not a hosted runner (no ImageOS, ImageVersion and NUGETGO_RUNNER): name the host with -HostLabel'
}

# A row's text: the title line, then one "key: value" per line, in the order given.
function New-NugetgoBehaviorRowText {
    [CmdletBinding()]
    param([Parameter(Mandatory)][System.Collections.Specialized.OrderedDictionary]$Fields)
    "# nugetgo behavioral proof row`n" + ((@($Fields.Keys) | ForEach-Object { "${_}: $($Fields[$_])" }) -join "`n") + "`n"
}

Export-ModuleMember -Function Get-NugetgoBehaviorRowKeys, Read-NugetgoBehaviorRow, Test-NugetgoBehaviorRows, New-NugetgoBehaviorPage, Test-NugetgoModuleHasGoTests, Get-NugetgoPackedRidAssemblies, Get-NugetgoProofMode, ConvertTo-NugetgoRowAnnotationText, ConvertFrom-NugetgoRowAnnotations, Get-NugetgoRowHost, New-NugetgoBehaviorRowText, New-NugetgoSampleBehaviorSection
