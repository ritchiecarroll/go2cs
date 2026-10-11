<#
.SYNOPSIS
    The package README and icon a nugetgo package packs (owner feedback on the hashset 1.0.0 nuget.org page,
    2026-10-10).

.DESCRIPTION
    hashset 1.0.0 shipped VALIDATION.md as its package README. nuget.org showed an inline-code heading that wrapped
    badly, a five-column table whose 64-character digest column scrolled, and every inline code span as a red block
    (its dark theme's style for code), and the package had no icon. The pack now generates a README in the style of
    the go.* standard library packages: the package ID as the title, the description as a blockquote, a badge row, the
    Go package's own synopsis, and a license line. VALIDATION.md is still packed, as a file the README links.
    The owner's review of the hashset revision-1 preview (2026-10-10): the description carries no "PROOF:" prefix (unexplained to
    a nuget.org reader; the registry tier and the Tests badge carry the proof), and the blockquote's module path, and
    the license line's, link the Go module's source at its version, so the README's first link is back to the Go.
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
# The fence around the block the pack owns in an author's package README (COORD ruling 2026-10-10). A CommonMark link
# reference definition renders nothing whether a renderer allows HTML or not; an HTML comment shows as text where HTML
# is disabled (measured with Markdig 0.37, the engine nuget.org's gallery uses). Each must stand alone between blank
# lines: without the blank line before it, it continues the paragraph above as visible text (measured).
$script:FenceBegin = '[//]: # (nugetgo:generated:begin)'
$script:FenceEnd = '[//]: # (nugetgo:generated:end)'

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

# The Go module's source at its version, the link a README's module path carries (owner review, 2026-10-10). A module
# at the root of a github.com repository -- its path github.com/<org>/<repo>, or that plus a /vN major-version suffix,
# whose tag Go reads at the repository root -- links GitHub's tree at its tag: the version itself, or a pseudo-version's
# commit, and never "+incompatible", which is not part of the tag. Every other module (another host, or a module in a
# repository subdirectory, whose tag is prefixed by that directory) links pkg.go.dev at its version, which names its
# source.
function Get-NugetgoModuleSourceUrl([string]$ModulePath, [string]$GoVersion) {
    $tag = if ($GoVersion.StartsWith('v')) { $GoVersion } else { "v$GoVersion" }
    $parts = @($ModulePath.Trim('/') -split '/')
    $atRoot = $parts.Count -eq 3 -or ($parts.Count -eq 4 -and $parts[3] -cmatch '^v[2-9][0-9]*$')
    if ($parts[0] -ceq 'github.com' -and $atRoot) {
        $ref = $tag -replace '\+incompatible$', ''
        if ($ref -cmatch '\d{14}-(?<commit>[0-9a-f]{12})$') { $ref = $Matches['commit'] }
        return "https://github.com/$($parts[1])/$($parts[2])/tree/$ref"
    }
    "https://pkg.go.dev/$ModulePath@$tag"
}

function New-NugetgoBadge([string]$Alt, [string]$Label, [string]$Message, [string]$Color, [string]$Logo, [string]$Target) {
    "[![$Alt]($script:ShieldsHost/badge/$(ConvertTo-NugetgoShieldsText $Label)-$(ConvertTo-NugetgoShieldsText $Message)-$Color`?logo=$Logo)]($Target)"
}

# The block the pack owns in a package README (COORD ruling 2026-10-10): the description as a blockquote (its module path
# a link to the module's source), two lines of badges, the license line and, with the go2cs icon, its artwork line.
# Every link is absolute, because nuget.org resolves no relative link. VALIDATION.md and the license file are linked in
# the conversion repository, which carries both at its root (docs/NugetgoPublish.md, 4.5), at -RepositoryTag: the tag
# nuget-<package version> COORD mints on the conversion commit before the package is signed (COORD ruling 2026-10-10),
# so an older version's page keeps its own proof. HEAD would follow the repository to a later version's files.
function New-NugetgoGeneratedBlock {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Description,
        [Parameter(Mandatory)][string]$ModulePath,
        [Parameter(Mandatory)][string]$GoVersion,
        [Parameter(Mandatory)][string]$PackageVersion,
        [Parameter(Mandatory)][string]$ClosureVersion,
        [Parameter(Mandatory)][string]$RepositoryUrl,
        [Parameter(Mandatory)][string]$RepositoryTag,
        [Parameter(Mandatory)][string]$LicenseName,
        [Parameter(Mandatory)][string]$LicenseSpdx,
        [int]$Matched,
        [int]$Disclosed,
        [switch]$Unvalidated,
        [switch]$DefaultIcon
    )
    $repo = $RepositoryUrl.TrimEnd('/')
    $goTag = if ($GoVersion.StartsWith('v')) { $GoVersion } else { "v$GoVersion" }
    $moduleSource = Get-NugetgoModuleSourceUrl $ModulePath $GoVersion
    $moduleLink = "[$ModulePath]($moduleSource)"
    $at = $Description.IndexOf($ModulePath, [StringComparison]::Ordinal)
    if ($at -lt 0) { throw "the description does not name the module path $ModulePath, so its blockquote cannot link the module's source" }
    $callout = $Description.Substring(0, $at) + $moduleLink + $Description.Substring($at + $ModulePath.Length)
    $tests = if ($Unvalidated) { New-NugetgoBadge 'Tests' 'Tests' 'not validated' 'orange' 'go' "$repo/blob/$RepositoryTag/VALIDATION.md" }
             else { New-NugetgoBadge 'Tests' 'Tests' "$Matched matched / $Disclosed disclosed" 'brightgreen' 'go' "$repo/blob/$RepositoryTag/VALIDATION.md" }
    $source = New-NugetgoBadge 'C# Source' 'C# Source' "@$PackageVersion" $script:DotnetPurple 'dotnet' "$repo/tree/$RepositoryTag"
    $module = New-NugetgoBadge 'Go module' 'Go module' "@$goTag" $script:GoBlue 'go' "https://pkg.go.dev/$ModulePath@$goTag"
    $release = New-NugetgoBadge 'go2cs' 'go2cs' "@$ClosureVersion" $script:DotnetPurple 'dotnet' "$script:Go2csRepository/tree/nuget-$ClosureVersion"

    $blocks = New-Object System.Collections.Generic.List[string]
    $blocks.Add("> $callout")
    $blocks.Add("$tests $source\`n$module $release")
    $blocks.Add("Converted from $moduleLink source; licensed under $LicenseSpdx — see [$LicenseName]($repo/blob/$RepositoryTag/$LicenseName).")
    if ($DefaultIcon) { $blocks.Add($script:ArtworkLine) }
    $blocks -join "`n`n"
}

# The package README the pack SEEDS when the conversion has none (COORD ruling 2026-10-10): the package ID as the
# title, then the fenced generated block, then the Go package's synopsis. From then on the file is the author's: the
# title, the synopsis and anything added outside the fence are never touched, and every pack rewrites the fence.
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
        [Parameter(Mandatory)][string]$RepositoryTag,
        [Parameter(Mandatory)][string]$LicenseName,
        [Parameter(Mandatory)][string]$LicenseSpdx,
        [string]$Synopsis,
        [int]$Matched,
        [int]$Disclosed,
        [switch]$Unvalidated,
        [switch]$DefaultIcon
    )
    $blockArgs = @{} + $PSBoundParameters
    [void]$blockArgs.Remove('Id'); [void]$blockArgs.Remove('Synopsis')
    $parts = New-Object System.Collections.Generic.List[string]
    $parts.Add("# $Id")
    $parts.Add($script:FenceBegin)
    $parts.Add((New-NugetgoGeneratedBlock @blockArgs))
    $parts.Add($script:FenceEnd)
    if ($Synopsis -and $Synopsis.Trim()) { $parts.Add((ConvertTo-NugetgoMarkdownText $Synopsis.Trim())) }
    ($parts -join "`n`n") + "`n"
}

# The reasons a README's fence is refused (COORD ruling 2026-10-10): no fence; a marker given twice; an end without a
# begin, a begin without an end, or the end first; a marker not on its own line between blank lines; and a line that
# names nugetgo:generated without being the marker (the HTML comment form, which nuget.org would show as text). Lines
# inside a fenced code block are none of these. No reason means one well-formed fence.
function Test-NugetgoReadmeFence([string]$Text) {
    $reasons = New-Object System.Collections.Generic.List[string]
    $lines = @($Text -split "`r?`n")
    $begins = New-Object System.Collections.Generic.List[int]
    $ends = New-Object System.Collections.Generic.List[int]
    $code = $null
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($line -match '^\s{0,3}(?<f>`{3,}|~{3,})') {
            if (-not $code) { $code = $Matches['f'] } elseif ($line.Trim() -eq $code) { $code = $null }
            continue
        }
        if ($code) { continue }
        if ($line -ceq $script:FenceBegin) { $begins.Add($i) }
        elseif ($line -ceq $script:FenceEnd) { $ends.Add($i) }
        elseif ($line.Contains('nugetgo:generated')) { $reasons.Add("line $($i + 1) names nugetgo:generated but is not the marker: the fence is exactly '$($script:FenceBegin)' and '$($script:FenceEnd)', each on its own line") }
    }
    if ($begins.Count -eq 0 -and $ends.Count -eq 0) { $reasons.Add("no generated block: the README has no '$($script:FenceBegin)' line; the pack fills the block between it and '$($script:FenceEnd)'") }
    if ($begins.Count -gt 1) { $reasons.Add("the begin marker appears more than once (lines $(($begins | ForEach-Object { $_ + 1 }) -join ', ')): one fence only") }
    if ($ends.Count -gt 1) { $reasons.Add("the end marker appears more than once (lines $(($ends | ForEach-Object { $_ + 1 }) -join ', ')): one fence only") }
    if ($begins.Count -ge 1 -and $ends.Count -eq 0) { $reasons.Add("the begin marker (line $($begins[0] + 1)) has no end marker after it") }
    if ($ends.Count -ge 1 -and $begins.Count -eq 0) { $reasons.Add("the end marker (line $($ends[0] + 1)) has no begin marker before it") }
    if ($begins.Count -eq 1 -and $ends.Count -eq 1 -and $ends[0] -lt $begins[0]) { $reasons.Add("the end marker (line $($ends[0] + 1)) comes before the begin marker (line $($begins[0] + 1))") }
    foreach ($i in @($begins) + @($ends)) {
        if ($i -gt 0 -and $lines[$i - 1].Trim()) { $reasons.Add("line $($i + 1): the marker needs a blank line before it, or it shows as text in the paragraph above") }
        if ($i -lt $lines.Count - 1 -and $lines[$i + 1].Trim()) { $reasons.Add("line $($i + 1): the marker needs a blank line after it") }
    }
    $reasons.ToArray()
}

# An author's README with its fenced block replaced by -Block. Returns @{ Text; Rewritten; Reasons }: Reasons from
# Test-NugetgoReadmeFence and Text $null when the fence is refused; otherwise Text is the file with only the lines
# between the markers replaced (a blank line, the block, a blank line), in the file's own line endings, and Rewritten
# says whether that changed anything. Nothing outside the fence is touched.
function Merge-NugetgoReadme([string]$Text, [string]$Block) {
    $reasons = @(Test-NugetgoReadmeFence $Text)
    if ($reasons.Count) { return [pscustomobject]@{ Text = $null; Rewritten = $false; Reasons = $reasons } }
    $nl = if ($Text.Contains("`r`n")) { "`r`n" } else { "`n" }
    $lines = @($Text -split "`r?`n")
    $b = [Array]::IndexOf([string[]]$lines, $script:FenceBegin)
    $e = [Array]::IndexOf([string[]]$lines, $script:FenceEnd)
    $merged = @($lines[0..$b]) + @('') + @($Block -split "`r?`n") + @('') + @($lines[$e..($lines.Count - 1)])
    $out = $merged -join $nl
    [pscustomobject]@{ Text = $out; Rewritten = ($out -cne $Text); Reasons = @() }
}

# The icon a pack takes (COORD ruling 2026-10-10): -Icon, or the author's nuget/icon.png in the conversion, or neither
# (Path $null: the go2cs icon the conversion carries). Both is refused by name: two sources would let one silently win.
function Select-NugetgoIconSource([string]$Icon, [string]$AuthorIcon) {
    if ($Icon -and $AuthorIcon) {
        return [pscustomobject]@{ Path = $null; Reason = "two icons: -Icon $Icon and $AuthorIcon in the conversion; the file is the author's choice, so drop -Icon, or delete the file" }
    }
    [pscustomobject]@{ Path = $(if ($Icon) { $Icon } elseif ($AuthorIcon) { $AuthorIcon } else { $null }); Reason = $null }
}

# The reasons a README is refused as a nuget.org package README (owner, 2026-10-10): inline code in any heading (ATX or
# setext), any table wider than three columns (its width is the cell count of its delimiter row, as GitHub-flavored
# Markdown defines a table), and any link naming blob/HEAD or tree/HEAD (COORD ruling 2026-10-10: a package's links name
# its conversion tag). Lines inside a fenced code block are none of these. The owner's review (2026-10-10) adds two: a
# "PROOF:" anywhere, and, given -ModuleSourceUrl, a first link that is not that URL (the README's first link is back to
# the Go module's source). No reason means accepted.
function Test-NugetgoReadme([string]$Text, [string]$ModuleSourceUrl) {
    $reasons = New-Object System.Collections.Generic.List[string]
    if ($Text.Contains('PROOF:')) { $reasons.Add('"PROOF:" appears in the README (owner review 2026-10-10: the registry tier and the Tests badge carry the proof)') }
    if ($ModuleSourceUrl) {
        # A link's text is plain text or a badge's image ([![alt](img)](target)): the target is the link, the image is not.
        $first = [regex]::Match($Text, '(?<!!)\[(?:[^\[\]]*|!\[[^\[\]]*\]\([^)\s]*\))\]\((?<url>[^)\s]+)\)')
        if (-not $first.Success) { $reasons.Add("the first link is missing: it must name the module source $ModuleSourceUrl") }
        elseif ($first.Groups['url'].Value -cne $ModuleSourceUrl) { $reasons.Add("the first link names $($first.Groups['url'].Value), not the module source $ModuleSourceUrl") }
    }
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
        foreach ($link in [regex]::Matches($line, '\]\((?<url>[^)\s]+)\)')) {
            if ($link.Groups['url'].Value -match '/(blob|tree)/HEAD(/|$)') { $reasons.Add("line ${n}: a link names HEAD, not the conversion tag: $($link.Groups['url'].Value)") }
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

# The closing section of a THIRD-PARTY conversion repository's root README (owner item, COORD ruling 2026-10-11): the
# conversion is the go2cs project's, not the module maintainers', and the repository and its NuGet package ID are
# theirs for the asking. COORD wrote it by hand into uuid-cs (491e0a1b2f) and jwt-cs (dffc7d6aa3); these lines are that
# text, line breaks included. {0} is the module path, in code; {1} is who maintains it. That is a judgement, so it is
# an input: uuid-cs names the module path (the default), jwt-cs the org, golang-jwt. The author form (hashset-cs)
# carries no closing section.
$script:MaintainersHeading = "## For the module's maintainers"
$script:MaintainersLines = @(
    'This conversion is published by the go2cs project, not by the maintainers of `{0}`. If you',
    'maintain {1} and would like to own it, open an issue here and we will gladly transfer this',
    'repository, and the NuGet package ID with it, to you.')

function New-NugetgoMaintainersSection {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ModulePath, [string]$Maintainer, [switch]$Author)
    if ($Author) { throw "an author conversion carries no maintainers section: $ModulePath is published by its author" }
    if (-not $Maintainer) { $Maintainer = $ModulePath }
    $script:MaintainersHeading + "`n`n" + ((@($script:MaintainersLines) -join "`n") -f $ModulePath, $Maintainer) + "`n"
}

# The reasons a conversion repository's root README is refused (COORD ruling 2026-10-11). A third-party README must END
# with the maintainers section for -ModulePath, with any maintainer named. An author README (-Author) must not carry the
# heading at all. Line endings are not compared. No reason means accepted.
function Test-NugetgoConversionReadme([string]$Text, [string]$ModulePath, [switch]$Author) {
    $text = $Text -replace "`r`n", "`n"
    $headings = [regex]::Matches($text, '(?m)^' + [regex]::Escape($script:MaintainersHeading) + '\s*$').Count
    if ($Author) {
        if ($headings) { return @("an author conversion carries no maintainers section, and this README has '$($script:MaintainersHeading)'") }
        return @()
    }
    $slot = 'NUGETGO-MAINTAINER-SLOT'
    $lines = @($script:MaintainersLines | ForEach-Object { [regex]::Escape(($_ -f $ModulePath, $slot)) })
    $lines[1] = $lines[1].Replace($slot, '(?<maintainer>[^\n]+)')
    $section = '(?:^|\n)' + [regex]::Escape($script:MaintainersHeading) + '\n\n' + ($lines -join '\n') + '\s*$'
    if ([regex]::IsMatch($text, $section)) { return @() }
    if (-not $headings) { return @("the README does not end with '$($script:MaintainersHeading)', the offer to the maintainers of $ModulePath") }
    @("the README's '$($script:MaintainersHeading)' is not the section for $ModulePath as the template words it, or it is not the README's last section")
}

Export-ModuleMember -Function ConvertTo-NugetgoShieldsText, ConvertTo-NugetgoMarkdownText, Get-NugetgoProofTotals, Test-NugetgoSpdx,
    Get-NugetgoModuleSourceUrl, New-NugetgoGeneratedBlock, New-NugetgoPackageReadme, Test-NugetgoReadmeFence, Merge-NugetgoReadme,
    Select-NugetgoIconSource, Test-NugetgoReadme, Resolve-NugetgoPackageIcon, New-NugetgoMaintainersSection,
    Test-NugetgoConversionReadme
