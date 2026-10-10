<#
.SYNOPSIS
    Table-driven arms for NugetgoReadme.psm1: the package README and icon a nugetgo package packs (owner feedback on
    the hashset 1.0.0 nuget.org page, 2026-10-10). hashset 1.0.0 shipped VALIDATION.md as its README, so nuget.org
    showed an inline-code heading, a five-column table and red code blocks, and the package had no icon. The pack now
    generates a README in the go.* standard library packages' style and refuses one with inline code in a heading or a
    table wider than three columns, and refuses a pack with no icon. The owner's review of the hashset revision-1 preview
    (2026-10-10): no "PROOF:" in the README or the description, and the README's first link names the Go module's
    source at its version. Exit code = the number of failed cases.
#>
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NugetgoReadme.psm1') -Force
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}

Write-Host 'shields.io text'
foreach ($case in @(
        @('54 matched / 0 disclosed', '54_matched_%2F_0_disclosed'),
        @('C# Source', 'C%23_Source'),
        @('go-cmp_x', 'go--cmp__x'),
        @('@v1.6.0', '@v1.6.0'))) {
    $got = ConvertTo-NugetgoShieldsText $case[0]
    Check "'$($case[0])'" ($got -ceq $case[1]) "got '$got', want '$($case[1])'"
}

Write-Host 'markdown text'
foreach ($case in @(
        @('Package uuid generates and inspects UUIDs.', 'Package uuid generates and inspects UUIDs.'),
        @('Package p uses *T, `x` and [a]_b.', 'Package p uses \*T, \`x\` and \[a\]\_b.'),
        @('a <b> c\d', 'a \<b\> c\\d'))) {
    $got = ConvertTo-NugetgoMarkdownText $case[0]
    Check "'$($case[0])'" ($got -ceq $case[1]) "got '$got', want '$($case[1])'"
}

Write-Host 'the proof totals'
$a = 'sha256-' + ('a' * 64)
$b = 'sha256-' + ('b' * 64)
$summary = @"
# ``example.com/mod`` — module validation summary

| Package | Matched | Disclosed | Input digest | Proof |
|:--|--:|--:|:--|:--|
| ``example.com/mod`` | 37 | 0 | ``$a`` | [index.md](index.md) |
| ``example.com/mod/sub`` | 3 | 1 | ``$b`` | [sub.md](sub.md) |

**Total: 40 matched · 1 disclosed** across 2 package(s).
"@
$t = Get-NugetgoProofTotals -ModuleSummary $summary
Check 'two rows sum' ($t.Matched -eq 40 -and $t.Disclosed -eq 1 -and $t.Packages -eq 2) "read $($t.Matched)/$($t.Disclosed)/$($t.Packages)"
$t = Get-NugetgoProofTotals -ModuleSummary "# nothing`n"
Check 'no rows read as no packages' ($t.Packages -eq 0 -and $t.Matched -eq 0) "read $($t.Matched)/$($t.Disclosed)/$($t.Packages)"

Write-Host 'SPDX expressions'
foreach ($ok in @('MIT', 'BSD-3-Clause', 'Apache-2.0', 'MIT OR Apache-2.0', 'GPL-2.0-only WITH Classpath-exception-2.0')) {
    Check "'$ok' is accepted" (Test-NugetgoSpdx $ok) 'refused'
}
foreach ($bad in @('', 'BSD 3-Clause', 'see LICENSE', 'MIT;rm')) {
    Check "'$bad' is refused" (-not (Test-NugetgoSpdx $bad)) 'accepted'
}

Write-Host 'the Go module source a README links'
foreach ($case in @(
        @('github.com/ritchiecarroll/hashset', 'v1.0.0', 'https://github.com/ritchiecarroll/hashset/tree/v1.0.0'),
        @('github.com/google/uuid', '1.6.0', 'https://github.com/google/uuid/tree/v1.6.0'),
        @('github.com/golang-jwt/jwt/v5', 'v5.3.1', 'https://github.com/golang-jwt/jwt/tree/v5.3.1'),
        @('github.com/o/r', 'v0.0.0-20240102030405-0123456789ab', 'https://github.com/o/r/tree/0123456789ab'),
        @('github.com/o/r', 'v2.1.0+incompatible', 'https://github.com/o/r/tree/v2.1.0'),
        @('github.com/o/r/sub', 'v1.0.0', 'https://pkg.go.dev/github.com/o/r/sub@v1.0.0'),
        @('gopkg.in/yaml.v3', 'v3.0.1', 'https://pkg.go.dev/gopkg.in/yaml.v3@v3.0.1'),
        @('example.com/mod', 'v1.2.3', 'https://pkg.go.dev/example.com/mod@v1.2.3'))) {
    $got = Get-NugetgoModuleSourceUrl -ModulePath $case[0] -GoVersion $case[1]
    Check "$($case[0]) $($case[1])" ($got -ceq $case[2]) "got '$got', want '$($case[2])'"
}

$repo = 'https://github.com/example/mod-cs'
$descText = 'unofficial go2cs C# conversion of example.com/mod v1.2.3, built on the Go 1.24.13 standard library.'
$modSource = 'https://pkg.go.dev/example.com/mod@v1.2.3'
$readmeArgs = @{
    Id = 'nugetgo.example.com.mod'; Description = $descText; ModulePath = 'example.com/mod'; GoVersion = 'v1.2.3'
    PackageVersion = '1.2.3'; ClosureVersion = '1.24.13.5'; RepositoryUrl = $repo; RepositoryTag = 'nuget-1.2.3'; LicenseName = 'LICENSE'
    LicenseSpdx = 'MIT'; Synopsis = 'Package mod does *one* thing.'; Matched = 40; Disclosed = 1; DefaultIcon = $true
}
$readme = New-NugetgoPackageReadme @readmeArgs
$lines = @($readme -split "`n")

Write-Host 'the generated README'
Check 'the title is the package ID, as plain text' ($lines[0] -ceq '# nugetgo.example.com.mod') "line 1 '$($lines[0])'"
Check 'the description is a blockquote, its module path linked to the module source' (
    $lines -ccontains "> unofficial go2cs C# conversion of [example.com/mod]($modSource) v1.2.3, built on the Go 1.24.13 standard library.") 'no such blockquote line'
Check 'no PROOF: anywhere (owner review 2026-10-10)' (-not $readme.Contains('PROOF')) 'PROOF in the README'
Check 'the Tests badge reads matched / disclosed and links VALIDATION.md at the conversion tag' (
    $readme.Contains("[![Tests](https://img.shields.io/badge/Tests-40_matched_%2F_1_disclosed-brightgreen?logo=go)]($repo/blob/nuget-1.2.3/VALIDATION.md)")) 'no such badge'
Check 'the C# Source badge links the conversion repository at its tag' (
    $readme.Contains("[![C# Source](https://img.shields.io/badge/C%23_Source-@1.2.3-512BD4?logo=dotnet)]($repo/tree/nuget-1.2.3)")) 'no such badge'
Check 'the Go module badge links pkg.go.dev at the module version' (
    $readme.Contains('[![Go module](https://img.shields.io/badge/Go_module-@v1.2.3-00ADD8?logo=go)](https://pkg.go.dev/example.com/mod@v1.2.3)')) 'no such badge'
Check 'the go2cs badge links the release it was built on' (
    $readme.Contains('[![go2cs](https://img.shields.io/badge/go2cs-@1.24.13.5-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.24.13.5)')) 'no such badge'
Check 'the synopsis is its own paragraph, escaped' ($lines -ccontains 'Package mod does \*one\* thing.') 'no synopsis line'
Check 'the license line links the module source too' ($lines -ccontains "Converted from [example.com/mod]($modSource) source; licensed under MIT — see [LICENSE]($repo/blob/nuget-1.2.3/LICENSE).") 'no license line'
Check 'the go2cs icon brings its artwork line' ($readme.Contains('Artwork licensed under Creative Commons Attribution 3.0')) 'no artwork line'
Check 'no inline code anywhere' (-not $readme.Contains('`')) 'a backtick'
Check 'no link names HEAD (COORD ruling 2026-10-10: links name the conversion tag)' ($readme -notmatch '/(blob|tree)/HEAD\b') 'a HEAD link'
Check 'the README passes its own guard' (@(Test-NugetgoReadme $readme $modSource).Count -eq 0) "refused: $(@(Test-NugetgoReadme $readme $modSource) -join '; ')"
$author = @{} + $readmeArgs
$author.ModulePath = 'github.com/ritchiecarroll/hashset'; $author.GoVersion = 'v1.0.0'
$author.Description = 'go2cs C# conversion of github.com/ritchiecarroll/hashset v1.0.0, published by its author, built on the Go 1.24.13 standard library; not affiliated with or endorsed by the Go project.'
$authorReadme = New-NugetgoPackageReadme @author
Check 'the author form links the module at its version tag on GitHub (the owner''s callout)' (
    @($authorReadme -split "`n") -ccontains '> go2cs C# conversion of [github.com/ritchiecarroll/hashset](https://github.com/ritchiecarroll/hashset/tree/v1.0.0) v1.0.0, published by its author, built on the Go 1.24.13 standard library; not affiliated with or endorsed by the Go project.') 'no such callout'
$noPath = @{} + $readmeArgs
$noPath.Description = 'a description that never names its module'
$threw = $false; try { New-NugetgoPackageReadme @noPath | Out-Null } catch { $threw = $_.Exception.Message.Contains('does not name the module path') }
Check 'a description that does not name the module path is refused' $threw 'not refused by name'
$readmeArgs.DefaultIcon = $false
Check 'an author''s own icon brings no artwork line' (-not (New-NugetgoPackageReadme @readmeArgs).Contains('Artwork')) 'an artwork line'
$readmeArgs.Synopsis = ''
Check 'no synopsis, no empty paragraph' (-not (New-NugetgoPackageReadme @readmeArgs).Contains("`n`n`n")) 'a blank run'
$readmeArgs.Remove('Matched'); $readmeArgs.Remove('Disclosed'); $readmeArgs.Unvalidated = $true
Check 'an unvalidated pack says so in its Tests badge' ((New-NugetgoPackageReadme @readmeArgs).Contains('/badge/Tests-not_validated-orange?logo=go')) 'no not_validated badge'

Write-Host 'the README guard'
$hashsetPage = @"
> PROOF: unofficial go2cs C# conversion of github.com/ritchiecarroll/hashset v1.0.0.

# ``github.com/ritchiecarroll/hashset`` — module validation summary

| Package | Matched | Disclosed | Input digest | Proof |
|:--|--:|--:|:--|:--|
| ``github.com/ritchiecarroll/hashset`` | 16 | 0 | ``$a`` | [index.md](index.md) |
"@
$cases = @(
    # name, text, expected reason fragments (none = accepted)
    @('the hashset 1.0.0 README as published', $hashsetPage, @('inline code in the heading', 'a table 5 columns wide', 'PROOF:')),
    @('PROOF: in a description callout', "> PROOF: go2cs C# conversion of example.com/mod v1.2.3.`n", @('PROOF:')),
    @('inline code in an ATX heading', "## The ``Set`` type`n", @('inline code in the heading')),
    @('inline code in a setext heading', "The ``Set`` type`n---------------`n", @('inline code in the heading')),
    @('a heading-shaped line inside a code fence', (('```text', '# `x`', '```', '') -join "`n"), @()),
    @('a thematic break after a blank line is not a heading', "Some ``x`` prose.`n`n---`n", @()),
    @('a four-column table', "| a | b | c | d |`n|--|--|--|--|`n| 1 | 2 | 3 | 4 |`n", @('a table 4 columns wide')),
    @('a three-column table', "| a | b | c |`n|--|--|--|`n| 1 | 2 | 3 |`n", @()),
    @('an escaped pipe is not a column', "| a \| b | c |`n|--|--|`n", @()),
    @('a badge linking blob/HEAD', "[![Tests](https://img.shields.io/badge/Tests-1-green)](https://github.com/o/r-cs/blob/HEAD/VALIDATION.md)`n", @('names HEAD')),
    @('a link to tree/HEAD', "See [the source](https://github.com/o/r-cs/tree/HEAD).`n", @('names HEAD')),
    @('a link naming the conversion tag', "See [LICENSE](https://github.com/o/r-cs/blob/nuget-1.2.3/LICENSE).`n", @()),
    @('the go.* artwork line, which names go2cs master', "Artwork; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).`n", @())
)
foreach ($case in $cases) {
    $reasons = @(Test-NugetgoReadme $case[1])
    $want = @($case[2])
    if ($want.Count -eq 0) {
        Check $case[0] ($reasons.Count -eq 0) "refused: $($reasons -join '; ')"
    }
    else {
        $missing = @($want | Where-Object { $f = $_; -not @($reasons | Where-Object { $_.Contains($f) }).Count })
        Check $case[0] ($missing.Count -eq 0) "reasons [$($reasons -join '; ')] lack [$($missing -join '; ')]"
    }
}

Write-Host 'the first link names the module source'
$src = 'https://github.com/o/r/tree/v1.0.0'
foreach ($case in @(
        @('the callout links the module source first', "> go2cs C# conversion of [github.com/o/r]($src) v1.0.0.`n`n[![Tests](https://img.shields.io/badge/x-y-green)](https://github.com/o/r-cs/blob/nuget-1.0.0/VALIDATION.md)`n", @()),
        @('a badge before the callout link', "[![Tests](https://img.shields.io/badge/x-y-green)](https://github.com/o/r-cs/blob/nuget-1.0.0/VALIDATION.md)`n`n> conversion of [github.com/o/r]($src).`n", @('the first link')),
        @('the callout links another version', "> conversion of [github.com/o/r](https://github.com/o/r/tree/v0.9.0).`n", @('the first link')),
        @('no link at all', "> conversion of github.com/o/r v1.0.0.`n", @('the first link')))) {
    $reasons = @(Test-NugetgoReadme $case[1] $src)
    $want = @($case[2])
    if ($want.Count -eq 0) { Check $case[0] ($reasons.Count -eq 0) "refused: $($reasons -join '; ')" }
    else { Check $case[0] (@($reasons | Where-Object { $_.Contains($want[0]) }).Count -gt 0) "reasons [$($reasons -join '; ')]" }
}

Write-Host 'the icon'
$dir = Join-Path ([System.IO.Path]::GetTempPath()) ("nugetgo-readme-$PID-" + [DateTime]::UtcNow.Ticks)
New-Item -ItemType Directory -Force (Join-Path $dir 'conv'), (Join-Path $dir 'bare') | Out-Null
try {
    $png = [byte[]](0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A) + [byte[]]::new(64)
    [System.IO.File]::WriteAllBytes((Join-Path $dir 'conv/go2cs.png'), $png)
    [System.IO.File]::WriteAllBytes((Join-Path $dir 'logo.png'), $png)
    [System.IO.File]::WriteAllBytes((Join-Path $dir 'logo.gif'), $png)
    [System.IO.File]::WriteAllBytes((Join-Path $dir 'huge.png'), [byte[]]::new(1024 * 1024 + 1))

    $icon = Resolve-NugetgoPackageIcon -ConvertedDirectories @((Join-Path $dir 'bare'), (Join-Path $dir 'conv'))
    Check 'the default is the go2cs icon the conversion carries' ($icon.Default -and $icon.Name -ceq 'go2cs.png' -and $icon.Path -eq (Join-Path $dir 'conv/go2cs.png') -and -not $icon.Reason) "read $($icon | ConvertTo-Json -Compress)"
    $icon = Resolve-NugetgoPackageIcon -ConvertedDirectories @((Join-Path $dir 'bare'))
    Check 'no icon anywhere is refused' (-not $icon.Path -and $icon.Reason -and $icon.Reason.Contains('no icon')) "read $($icon | ConvertTo-Json -Compress)"
    $icon = Resolve-NugetgoPackageIcon -Icon (Join-Path $dir 'logo.png') -ConvertedDirectories @((Join-Path $dir 'conv'))
    Check '-Icon overrides the default' (-not $icon.Default -and $icon.Name -ceq 'logo.png' -and -not $icon.Reason) "read $($icon | ConvertTo-Json -Compress)"
    foreach ($bad in @(@('logo.gif', 'PNG or JPEG'), @('huge.png', '1 MB'), @('missing.png', 'no file'))) {
        $icon = Resolve-NugetgoPackageIcon -Icon (Join-Path $dir $bad[0]) -ConvertedDirectories @((Join-Path $dir 'conv'))
        Check "-Icon $($bad[0]) is refused" (-not $icon.Path -and $icon.Reason -and $icon.Reason.Contains($bad[1])) "read $($icon | ConvertTo-Json -Compress)"
    }
}
finally { Remove-Item -Recurse -Force $dir }

Write-Host "$($ran - $failed)/$ran passed"
exit $failed
