<#
.SYNOPSIS
    The module license file a nugetgo package packs (docs/PLAN-nugetgo.md B6): the upstream file, verbatim, under
    its own name. Nothing is ever inferred from its contents.

.DESCRIPTION
    The accepted names, and their order of preference, are the converter's own: moduleLicenseNames in
    src/go2cs/licensing.go, which a -recurse conversion uses to copy a dependency module's license. This module
    keeps a copy because PowerShell cannot read the Go list; Test-NugetgoIdentity.ps1 pins the two against each
    other, so the copy cannot drift.
#>
#Requires -Version 5.1

$script:NugetgoLicenseNames = @(
    'LICENSE', 'LICENSE.md', 'LICENSE.txt',
    'LICENCE', 'LICENCE.md', 'LICENCE.txt',
    'COPYING', 'COPYING.md', 'COPYING.txt'
)

function Get-NugetgoLicenseNames { $script:NugetgoLicenseNames }

# Returns @{ Name; Path; Reason }: the module root's license file in preference order, matched case-insensitively and
# spelled as the directory spells it (Name and Path set, Reason $null), or Name/Path $null with a Reason naming the
# directory and the names looked for.
function Find-NugetgoModuleLicense {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ModuleDir)

    if (-not (Test-Path -LiteralPath $ModuleDir -PathType Container)) {
        return [pscustomobject]@{ Name = $null; Path = $null; Reason = "no module directory at $ModuleDir" }
    }

    $files = @(Get-ChildItem -LiteralPath $ModuleDir -File)

    foreach ($want in $script:NugetgoLicenseNames) {
        foreach ($file in $files) {
            if ([string]::Equals($file.Name, $want, [StringComparison]::OrdinalIgnoreCase)) {
                return [pscustomobject]@{ Name = $file.Name; Path = $file.FullName; Reason = $null }
            }
        }
    }

    [pscustomobject]@{ Name = $null; Path = $null; Reason = "no license file in $ModuleDir (looked for $($script:NugetgoLicenseNames -join ', '))" }
}

Export-ModuleMember -Function Get-NugetgoLicenseNames, Find-NugetgoModuleLicense
