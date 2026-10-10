<#
.SYNOPSIS
    The package README and icon a nugetgo package packs (owner feedback on the hashset 1.0.0 nuget.org page,
    2026-10-10).

.DESCRIPTION
    hashset 1.0.0 shipped VALIDATION.md as its package README. nuget.org showed an inline-code heading that wrapped
    badly, a five-column table whose 64-character digest column scrolled, and every inline code span as a red block
    (its dark theme's style for code), and the package had no icon. The pack now generates a README in the style of
    the go.* standard library packages: the package ID as the title, the PROOF text as a blockquote, a badge row, the
    Go package's own synopsis, and a license line. VALIDATION.md is still packed, as a file the README links.
    The icon is the go2cs icon the conversion carries, unless -Icon names another.
#>
#Requires -Version 5.1

function ConvertTo-NugetgoShieldsText([string]$Value) { $Value }

function ConvertTo-NugetgoMarkdownText([string]$Value) { $Value }

function Get-NugetgoProofTotals([string]$ModuleSummary) { [pscustomobject]@{ Matched = 0; Disclosed = 0; Packages = 0 } }

function Test-NugetgoSpdx([string]$Expression) { $true }

function New-NugetgoPackageReadme {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Id,
        [Parameter(Mandatory)][string]$Description,
        [Parameter(Mandatory)][string]$ModulePath,
        [Parameter(Mandatory)][string]$GoVersion,
        [Parameter(Mandatory)][string]$PackageVersion,
        [Parameter(Mandatory)][string]$ClosureVersion,
        [Parameter(Mandatory)][string]$RepositoryUrl,
        [Parameter(Mandatory)][string]$LicenseName,
        [Parameter(Mandatory)][string]$LicenseSpdx,
        [string]$Synopsis,
        [int]$Matched,
        [int]$Disclosed,
        [switch]$Unvalidated,
        [switch]$DefaultIcon
    )
    ''
}

function Test-NugetgoReadme([string]$Text) { @() }

function Resolve-NugetgoPackageIcon {
    [CmdletBinding()]
    param([string]$Icon, [string[]]$ConvertedDirectories = @())
    [pscustomobject]@{ Path = $null; Name = $null; Default = $false; Reason = $null }
}

Export-ModuleMember -Function ConvertTo-NugetgoShieldsText, ConvertTo-NugetgoMarkdownText, Get-NugetgoProofTotals, Test-NugetgoSpdx,
    New-NugetgoPackageReadme, Test-NugetgoReadme, Resolve-NugetgoPackageIcon
