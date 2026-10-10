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

# The links the README's badges and lines name: the go2cs repository, and its artwork attribution page (the go2cs icon
# is that artwork, so a package that carries it says so, as the go.* packages do).
$script:Go2csRepository = 'https://github.com/ritchiecarroll/go2cs'
$script:ArtworkLine = 'Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).'
$script:ShieldsHost = 'https://img.shields.io'
$script:GoBlue = '00ADD8'
$script:DotnetPurple = '512BD4'
# nuget.org refuses a package icon over 1 MB, and shows only PNG and JPEG.
$script:IconMaxBytes = 1024 * 1024
$script:IconExtensions = @('.png', '.jpg', '.jpeg')
$script:MaxTableColumns = 3

# A value as shields.io's path form spells it, the converter's own rule (shieldsBadgeMessage in
# src/go2cs/readmeValidationBadge.go): '_' and '-' doubled, a space as '_', and '%', '/' and '#' percent-encoded so
# they cannot end or split the path segment.
function ConvertTo-NugetgoShieldsText([string]$Value) {
    $Value.Replace('%', '%25').Replace('_', '__').Replace('-', '--').Replace(' ', '_').Replace('/', '%2F').Replace('#', '%23')
}

# Plain text as Markdown text: each character Markdown would read as markup is backslash-escaped, so a Go doc
# sentence such as "uses *T" renders as written, and a backtick can never open inline code.
function ConvertTo-NugetgoMarkdownText([string]$Value) {
    $out = New-Object System.Text.StringBuilder
    foreach ($ch in $Value.ToCharArray()) {
        if ('\`*_[]<>'.IndexOf($ch) -ge 0) { [void]$out.Append('\') }
        [void]$out.Append($ch)
    }
    $out.ToString()
}

# The matched and disclosed totals of a MODULE.md, summed from its package rows (the rows the input-digest binding
# reads), and the number of rows.
function Get-NugetgoProofTotals([string]$ModuleSummary) {
    $matched = 0; $disclosed = 0; $packages = 0
    foreach ($line in ($ModuleSummary -split "`r?`n")) {
        if ($line -notmatch '^\|\s*`[^`]+`\s*\|') { continue }
        $cells = @($line.Trim().Trim('|') -split '\|' | ForEach-Object { $_.Trim() })
        $matched += [int]$cells[1]; $disclosed += [int]$cells[2]; $packages++
    }
    [pscustomobject]@{ Matched = $matched; Disclosed = $disclosed; Packages = $packages }
}

# An SPDX license expression: identifiers joined by AND, OR or WITH. The pack states the license the caller names; it
# never infers one from the license file's text (NugetgoLicense.psm1).
function Test-NugetgoSpdx([string]$Expression) {
    $Expression -cmatch '^[A-Za-z0-9][A-Za-z0-9.+-]*( (AND|OR|WITH) [A-Za-z0-9][A-Za-z0-9.+-]*)*$'
}

function New-NugetgoBadge([string]$Alt, [string]$Label, [string]$Message, [string]$Color, [string]$Logo, [string]$Target) {
    "[![$Alt]($script:ShieldsHost/badge/$(ConvertTo-NugetgoShieldsText $Label)-$(ConvertTo-NugetgoShieldsText $Message)-$Color`?logo=$Logo)]($Target)"
}

# The package README, in the go.* standard library packages' style: the package ID as the title, the PROOF text as a
# blockquote, two lines of badges, the Go package's synopsis, then the license line. Every link is absolute, because
# nuget.org resolves no relative link. VALIDATION.md and the license file are linked in the conversion repository,
# which carries both at its root (docs/NugetgoPublish.md, 4.5).
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
    $repo = $RepositoryUrl.TrimEnd('/')
    $goTag = if ($GoVersion.StartsWith('v')) { $GoVersion } else { "v$GoVersion" }
    $tests = if ($Unvalidated) { New-NugetgoBadge 'Tests' 'Tests' 'not validated' 'orange' 'go' "$repo/blob/HEAD/VALIDATION.md" }
             else { New-NugetgoBadge 'Tests' 'Tests' "$Matched matched / $Disclosed disclosed" 'brightgreen' 'go' "$repo/blob/HEAD/VALIDATION.md" }
    $source = New-NugetgoBadge 'C# Source' 'C# Source' "@$PackageVersion" $script:DotnetPurple 'dotnet' $repo
    $module = New-NugetgoBadge 'Go module' 'Go module' "@$goTag" $script:GoBlue 'go' "https://pkg.go.dev/$ModulePath@$goTag"
    $release = New-NugetgoBadge 'go2cs' 'go2cs' "@$ClosureVersion" $script:DotnetPurple 'dotnet' "$script:Go2csRepository/tree/nuget-$ClosureVersion"

    $blocks = New-Object System.Collections.Generic.List[string]
    $blocks.Add("# $Id")
    $blocks.Add("> $Description")
    $blocks.Add("$tests $source\`n$module $release")
    if ($Synopsis -and $Synopsis.Trim()) { $blocks.Add((ConvertTo-NugetgoMarkdownText $Synopsis.Trim())) }
    $blocks.Add('---')
    $blocks.Add("Converted from $ModulePath source; licensed under $LicenseSpdx — see [$LicenseName]($repo/blob/HEAD/$LicenseName).")
    if ($DefaultIcon) { $blocks.Add($script:ArtworkLine) }
    ($blocks -join "`n`n") + "`n"
}

# The reasons a README is refused as a nuget.org package README (owner, 2026-10-10): inline code in any heading (ATX or
# setext), and any table wider than three columns (its width is the cell count of its delimiter row, as GitHub-flavored
# Markdown defines a table). Lines inside a fenced code block are neither. No reason means accepted.
function Test-NugetgoReadme([string]$Text) {
    $reasons = New-Object System.Collections.Generic.List[string]
    $lines = @($Text -split "`r?`n")
    $fence = $null
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($line -match '^\s{0,3}(?<f>`{3,}|~{3,})') {
            $f = $Matches['f']
            if (-not $fence) { $fence = $f; continue }
            if ($f[0] -eq $fence[0] -and $f.Length -ge $fence.Length -and $line.Trim() -eq $f) { $fence = $null }
            continue
        }
        if ($fence) { continue }
        $n = $i + 1
        if ($line -match '^\s{0,3}#{1,6}(\s|$)' -and $line.Contains('`')) { $reasons.Add("line ${n}: inline code in the heading '$($line.Trim())'") }
        if ($i -gt 0 -and $line -match '^\s{0,3}(=+|-+)\s*$' -and $lines[$i - 1].Trim() -and $lines[$i - 1].Contains('`') -and $lines[$i - 1] -notmatch '\|') {
            $reasons.Add("line $($i): inline code in the heading '$($lines[$i - 1].Trim())'")
        }
        if ($line.Contains('|') -and $line.Contains('-') -and ($line -replace '[\s|:-]', '') -eq '') {
            $columns = @($line.Trim().Trim('|') -split '\|').Count
            if ($columns -gt $script:MaxTableColumns) { $reasons.Add("line ${n}: a table $columns columns wide (at most $($script:MaxTableColumns))") }
        }
    }
    $reasons.ToArray()
}

# Returns @{ Path; Name; Default; Reason }: the icon the package packs. -Icon names one (a module author's own, PNG or
# JPEG up to 1 MB); otherwise it is the go2cs icon the conversion carries beside each converted project (go2cs.png),
# the icon of every go.* package. No icon at all is refused (Path $null, Reason set).
function Resolve-NugetgoPackageIcon {
    [CmdletBinding()]
    param([string]$Icon, [string[]]$ConvertedDirectories = @())
    $none = { param($why) [pscustomobject]@{ Path = $null; Name = $null; Default = $false; Reason = $why } }
    if ($Icon) {
        if (-not (Test-Path -LiteralPath $Icon -PathType Leaf)) { return (& $none "-Icon: no file at $Icon") }
        $file = Get-Item -LiteralPath $Icon
        if ($script:IconExtensions -notcontains $file.Extension.ToLowerInvariant()) { return (& $none "-Icon must be a PNG or JPEG file: $($file.Name)") }
        if ($file.Length -gt $script:IconMaxBytes) { return (& $none "-Icon $($file.Name) is $($file.Length) bytes, over nuget.org's 1 MB limit") }
        return [pscustomobject]@{ Path = $file.FullName; Name = $file.Name; Default = $false; Reason = $null }
    }
    foreach ($dir in $ConvertedDirectories) {
        $candidate = Join-Path $dir 'go2cs.png'
        if (Test-Path -LiteralPath $candidate -PathType Leaf) { return [pscustomobject]@{ Path = $candidate; Name = 'go2cs.png'; Default = $true; Reason = $null } }
    }
    & $none 'no icon: the conversion carries no go2cs.png beside its projects, and no -Icon was given'
}

Export-ModuleMember -Function ConvertTo-NugetgoShieldsText, ConvertTo-NugetgoMarkdownText, Get-NugetgoProofTotals, Test-NugetgoSpdx,
    New-NugetgoPackageReadme, Test-NugetgoReadme, Resolve-NugetgoPackageIcon
