<#
.SYNOPSIS
    The validation proof a module pack ships as VALIDATION.md must be a proof of the packed bytes (the nugetgo rehearsal,
    2026-10-09, gap 4). The proof is made in a `-recurse -tests` root and the package is packed from a `-recurse=nuget`
    root, so the two emissions are never byte-identical. What binds them is the input both were converted from: each
    converted module package's project records it as GoInputDigest (src/go2cs/packageInputDigest.go), and MODULE.md
    carries the value each proof page's own conversion recorded. The pack compares the two per package.
#>

$script:digestPattern = '^sha256-[0-9a-f]{64}$'

<#
.SYNOPSIS
    The package rows of a MODULE.md, as import path -> input digest ('' for a row that records none: a summary written
    before the binding, or a page whose project recorded no digest).
#>
function Get-NugetgoProofInputDigests([string]$ModuleSummary) {
    $rows = @{}
    foreach ($line in ($ModuleSummary -split "`r?`n")) {
        if ($line -notmatch '^\|\s*`(?<path>[^`]+)`\s*\|') { continue }
        $cells = @($line.Trim().Trim('|') -split '\|' | ForEach-Object { $_.Trim() })
        $digest = ''
        foreach ($cell in $cells) {
            if ($cell -match '^`(?<digest>sha256-[0-9a-f]+)`$') { $digest = $Matches['digest'] }
        }
        $rows[$cells[0].Trim('`')] = $digest
    }
    return $rows
}

<#
.SYNOPSIS
    The GoInputDigest a converted project records, or '' when it records none.
#>
function Get-NugetgoProjectInputDigest([string]$ProjectFile) {
    [xml]$project = Get-Content -Raw -LiteralPath $ProjectFile
    $digest = @($project.Project.PropertyGroup | ForEach-Object { $_.GoInputDigest } | Where-Object { $_ })[0]
    if ($digest) { return ([string]$digest).Trim() }
    return ''
}

<#
.SYNOPSIS
    The reasons -Proof (MODULE.md's rows, import path -> digest) is not a proof of -Packed (each packed package's import
    path -> its project's digest); empty when it is. A packed package with no proof row is a reason only when it is in
    -Tested, the packed packages with Go tests (Get-NugetgoTestedImportPaths): a package with no Go tests has no proof
    page, and one WITH Go tests and no row is one whose tests the -tests run did not validate (COORD ruling 2026-10-13,
    the go-cmp dry run, where the module's root package lost its comparison to the package deadline and MODULE.md
    simply had no row for it).
#>
function Test-NugetgoValidationBinding([hashtable]$Proof, [hashtable]$Packed, [string[]]$Tested = @()) {
    $reasons = New-Object System.Collections.Generic.List[string]
    if ($Proof.Count -eq 0) { $reasons.Add('the proof lists no package'); return $reasons.ToArray() }
    foreach ($path in @($Proof.Keys | Sort-Object)) {
        $proven = [string]$Proof[$path]
        if (-not $Packed.ContainsKey($path)) { $reasons.Add("$path has a proof row and is not packed"); continue }
        $packedDigest = [string]$Packed[$path]
        if ($proven -notmatch $script:digestPattern) {
            $reasons.Add("the proof records no input digest for $path (validated by a converter before the binding: re-validate)")
        }
        elseif ($packedDigest -notmatch $script:digestPattern) {
            $reasons.Add("$path is packed from a conversion that records no input digest (reconvert the -recurse=nuget root)")
        }
        elseif ($proven -cne $packedDigest) {
            $reasons.Add("${path}: the proof was made from inputs $proven, the packed tree from $packedDigest")
        }
    }
    foreach ($path in @($Tested | Sort-Object -Unique)) {
        if ($Packed.ContainsKey($path) -and -not $Proof.ContainsKey($path)) {
            $reasons.Add("$path has Go tests and no proof row: the -tests run did not validate it. Runbook step 1.2 exits non-zero when a tested package fails, and MODULE.md names it under 'Packages that did not validate'; re-run step 1.2 until it exits 0")
        }
    }
    return $reasons.ToArray()
}

<#
.SYNOPSIS
    The import paths among -ImportPaths whose package directory in the module source (-ModuleDir, the module root for
    -ModulePath) holds Go test files: *_test.go in the package's own directory, the predicate the -tests -recurse driver
    uses to choose the packages it validates (mainModulePackagesWithTests).
#>
function Get-NugetgoTestedImportPaths([string]$ModuleDir, [string]$ModulePath, [string[]]$ImportPaths) {
    $tested = New-Object System.Collections.Generic.List[string]
    foreach ($path in @($ImportPaths | Sort-Object -Unique)) {
        if ($path -ceq $ModulePath) { $rel = '' }
        elseif ($path.StartsWith("$ModulePath/", [System.StringComparison]::Ordinal)) { $rel = $path.Substring($ModulePath.Length + 1) }
        else { continue }
        $dir = if ($rel) { Join-Path $ModuleDir $rel } else { $ModuleDir }
        if ((Test-Path -LiteralPath $dir -PathType Container) -and @(Get-ChildItem -LiteralPath $dir -Filter '*_test.go' -File).Count -gt 0) { $tested.Add($path) }
    }
    return $tested.ToArray()
}

Export-ModuleMember -Function Get-NugetgoProofInputDigests, Get-NugetgoProjectInputDigest, Test-NugetgoValidationBinding, Get-NugetgoTestedImportPaths
