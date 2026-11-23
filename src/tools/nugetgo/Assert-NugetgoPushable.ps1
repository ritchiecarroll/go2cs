<#
.SYNOPSIS
    Refuse, by name, a nugetgo package that must not be pushed: one packed against a local go.* closure, and a -Cgo pack
    whose go.lib floor lacks the run-time target check. Exit code = the number of packages refused.

.DESCRIPTION
    src/_nugetpush.ps1 refuses both on the go.* release's own push (Invoke-GoNuGetPush). A nugetgo package is pushed by
    hand from an owner sheet (docs/NugetgoPublish.md, section 3 step 1 and section 4 step 3), past that function, so the
    sheet runs this first, in the same block, and Test-SheetPaste.ps1 refuses a sheet whose push has no such line
    (COORD's INC 4 review, fold 2). It reads the package's go2cs/source-metadata.txt with _nugetpush.ps1's own helpers:
    - "# local-closure: " (nugetgo-pack.ps1 -LocalClosure): a rehearsal against go.* packages built from the tree;
    - a -Cgo pack (its build/<id>.targets carries NUGETGOCGO001, or its lib/ holds reference assemblies beside
      runtimes/) with no "# a6: " record.

    pwsh -NoProfile -File src/tools/nugetgo/Assert-NugetgoPushable.ps1 <package.nupkg> [<package.nupkg> ...]
#>
param([Parameter(ValueFromRemainingArguments)][string[]]$Package)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '../../_nugetpush.ps1')

$Package = @($Package | Where-Object { $_ })
if (-not $Package.Count) { Write-Host 'REFUSED: name the package(s) to check'; exit 1 }
$refused = 0
foreach ($path in $Package) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { Write-Host "REFUSED: no package at $path"; $refused++; continue }
    $name = Split-Path -Leaf $path
    if (Test-GoNuGetSelfDescriptionNote $path 'local-closure') {
        Write-Host "REFUSED: $name was packed against a local go.* closure (its go2cs/source-metadata.txt records `"# local-closure: `"): a rehearsal, never pushed"
        $refused++
    }
    elseif ((Test-GoNuGetCgoPackage $path) -and -not (Test-GoNuGetCgoA6Note $path)) {
        Write-Host "REFUSED: $name is a -Cgo pack whose go2cs/source-metadata.txt does not record the run-time target check (no `"# a6: `"), so its go.lib floor may not refuse its C bindings at load"
        $refused++
    }
    else { Write-Host "pushable: $name" }
}
exit $refused
