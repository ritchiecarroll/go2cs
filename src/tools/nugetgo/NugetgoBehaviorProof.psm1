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

$script:RowKeys = @('module', 'version', 'package', 'rid', 'runtime-rid', 'host', 'os', 'go', 'dotnet', 'go-output', 'dotnet-output', 'outputs-equal', 'assembly', 'date')
$script:ShippedRids = @('win-x64', 'linux-x64', 'osx-x64', 'osx-arm64')

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
        if ($row.module -cne $ModulePath -or $row.version -cne $GoVersion) { $reasons.Add("[$rid] the row is for $($row.module) $($row.version), not $ModulePath $GoVersion") }
        if ($row.'outputs-equal' -cne 'true' -or $row.'go-output' -cne $row.'dotnet-output') { $reasons.Add("[$rid] dotnet run did not print what go run printed (go $($row.'go-output'), dotnet $($row.'dotnet-output'))") }
        $packed = $Assemblies[$rid]
        if ($null -eq $packed) { $reasons.Add("[$rid] the package carries no assembly for $rid"); continue }
        if ([string]$row.assembly -notmatch '^(?<name>\S+) (?<sha>sha256-[0-9a-f]{64})$') { $reasons.Add("[$rid] the row's assembly '$($row.assembly)' is not '<file> sha256-<hex>'"); continue }
        $name = $Matches['name']; $sha = $Matches['sha']
        if (-not $packed.ContainsKey($name)) { $reasons.Add("[$rid] the row ran $name, which the package does not carry for $rid") }
        elseif ((Get-NugetgoSha256 $packed[$name]) -cne $sha) { $reasons.Add("[$rid] the row ran $name at $sha, which is not the packed bytes for $rid ($(Get-NugetgoSha256 $packed[$name]))") }
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
    $lines.Add('program run on that RID''s own host: the program converted by go2cs against this package and run with `dotnet run`,')
    $lines.Add('and the Go program run with `go run`, printed the same bytes. Each row names the assembly that loaded, which is the')
    $lines.Add('package''s own for that RID, and the host it ran on. No row stands for another RID.')
    $lines.Add('')
    $lines.Add('| RID | Host | Assembly | Outputs equal |')
    $lines.Add('|:--|:--|:--|:--|')
    foreach ($rid in $script:ShippedRids) {
        $row = $Rows[$rid]
        if ($null -eq $row) { continue }
        $lines.Add("| $rid | $($row.host) ($($row.os)) | ``$($row.assembly)`` | $($row.'outputs-equal') (go $($row.go), dotnet $($row.dotnet)) |")
    }
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

Export-ModuleMember -Function Get-NugetgoBehaviorRowKeys, Read-NugetgoBehaviorRow, Test-NugetgoBehaviorRows, New-NugetgoBehaviorPage, Test-NugetgoModuleHasGoTests
