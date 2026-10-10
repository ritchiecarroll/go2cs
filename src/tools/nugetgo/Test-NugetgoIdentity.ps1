<#
.SYNOPSIS
    Table-driven arms for NugetgoIdentity.psm1 (owner rulings B2 and B3). Every expectation is a LITERAL; a case
    that should refuse names a fragment of its reason, so a refusal for the wrong cause fails too. Exit code =
    the number of failed cases.
#>
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NugetgoIdentity.psm1') -Force
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}

Write-Host 'B2 -- package IDs'
$ids = @(
    # module path, existing IDs, expected Id (literal) or $null, expected Alternate, reason fragment when refused/alternate
    @('github.com/google/uuid', @(), 'nugetgo.github.com.google.uuid', $false, $null),
    @('github.com/golang-jwt/jwt/v5', @(), 'nugetgo.github.com.golang-jwt.jwt.v5', $false, $null),
    @('gopkg.in/yaml.v3', @(), 'nugetgo.gopkg.in.yaml.v3', $false, $null),
    @('github.com/BurntSushi/toml', @(), 'nugetgo.github.com.BurntSushi.toml', $false, $null),
    # The alternates (COORD's ruling within B2). Every hash is the first 8 hex digits of SHA-256 over the exact module
    # path, computed INDEPENDENTLY of the module under test (coreutils sha256sum), and pinned here as a literal.
    @('example.com/a~b', @(), 'nugetgo.example.com.a-b.42086d51', $true, 'breaks nuget.org''s ID rule'),
    @('github.com/a/b.c', @('nugetgo.github.com.a.b.c'), 'nugetgo.github.com.a.b.c.22485230', $true, 'collides'),
    @('github.com/a.b/c', @('NUGETGO.GITHUB.COM.A.B.C'), 'nugetgo.github.com.a.b.c.14244b44', $true, 'collides'),
    @('github.com/Foo/x', @('nugetgo.github.com.foo.x'), 'nugetgo.github.com.Foo.x.0b06ffe9', $true, 'collides'),
    @('github.com/foo/x', @('nugetgo.github.com.Foo.x'), 'nugetgo.github.com.foo.x.27641fec', $true, 'collides'),
    # over 100: the stem is cut at the separator boundary before the 95 x's, never mid-segment
    @(('example.com/' + ('x' * 95)), @(), 'nugetgo.example.com.8a8ab77e', $true, 'over nuget.org''s 100'),
    # the alternate collides too: refused by name, the hash never extended
    @('github.com/a/b.c', @('nugetgo.github.com.a.b.c', 'nugetgo.github.com.a.b.c.22485230'), $null, $true, 'refused by name'),
    @('not a path', @(), $null, $false, 'not a Go module path')
)
foreach ($c in $ids) {
    $r = Get-NugetgoPackageId -ModulePath $c[0] -ExistingIds $c[1]
    $label = "$($c[0]) [existing: $($c[1] -join ',')]"
    if ($c[2]) {
        $reasonOk = if ($c[3]) { $r.Reason -like "*$($c[4])*" } else { $null -eq $r.Reason }
        Check $label ($r.Id -ceq $c[2] -and $r.Alternate -eq $c[3] -and $reasonOk -and $r.Id.Length -le 100) "got Id '$($r.Id)' Alternate $($r.Alternate) Reason '$($r.Reason)'"
    }
    else {
        Check $label ($null -eq $r.Id -and $r.Alternate -eq $c[3] -and $r.Reason -like "*$($c[4])*") "got Id '$($r.Id)' Alternate $($r.Alternate) Reason '$($r.Reason)'"
    }
}
# a/b.c and a.b/c must not share an alternate (the hash is over the exact path)
$ab = Get-NugetgoPackageId -ModulePath 'github.com/a/b.c' -ExistingIds @('nugetgo.github.com.a.b.c')
$ba = Get-NugetgoPackageId -ModulePath 'github.com/a.b/c' -ExistingIds @('nugetgo.github.com.a.b.c')
Check 'a/b.c and a.b/c take DIFFERENT alternates' ($ab.Id -and $ba.Id -and $ab.Id -ne $ba.Id) "a/b.c '$($ab.Id)', a.b/c '$($ba.Id)'"

Write-Host 'B3 -- package versions'
$uuidList = @('v1.0.0', 'v1.6.0')
$rcList = @('v1.0.0-rc.1', 'v1.0.0-rc.2')
$rcClash = @('v1.0.0-rc.1', 'v1.0.0-rc.1.0.3')
$vs = @(
    # Go version, revision, @v/list, expected version (literal) or $null, refusal fragment
    @('v1.6.0', 0, $null, '1.6.0', $null),
    # Release rebuilds (X.Y.Z.N). A component above 255 keeps each four-part literal out of the identifier census's
    # IPv4 shape (strict mode admits only the corpus's own release versions); the rule under test is the same.
    @('v1.6.300', 1, $uuidList, '1.6.300.1', $null),
    @('v5.3.256', 2, $null, '5.3.256.2', $null),
    @('v2.0.0+incompatible', 0, $null, '2.0.0', $null),
    @('v1.0.0-rc.1', 0, $null, '1.0.0-rc.1', $null),
    @('v1.0.0-rc.1', 1, $rcList, '1.0.0-rc.1.0.1', $null),
    @('v1.0.0-rc.1', 2, $rcList, '1.0.0-rc.1.0.2', $null),
    @('v0.0.0-20251001235044-fca9a0999f15', 0, $null, '0.0.0-20251001235044-fca9a0999f15', $null),
    @('v0.0.0-20251001235044-fca9a0999f15', 1, @(), '0.0.0-20251001235044-fca9a0999f15.0.1', $null),
    @('v1.0.0-rc.1', 1, $rcClash, $null, 'could equal'),
    @('v1.0.0-rc.1', 1, $null, $null, 'needs the module''s @v/list'),
    @('v1.0.0-RC.1', 0, $null, $null, 'uppercase'),
    @('v2147483648.0.0', 0, $null, $null, 'overflows Int32'),
    @(('v1.0.0-' + ('a' * 60)), 0, $null, $null, 'over 64'),
    @('1.6.0', 0, $null, $null, 'not a Go module version'),
    @('v1.6.0', -1, $null, $null, 'not -1')
)
foreach ($c in $vs) {
    $r = Get-NugetgoVersion -GoVersion $c[0] -Revision $c[1] -VList $c[2]
    $label = "$($c[0]) rev $($c[1])"
    if ($c[3]) { Check $label (-not $r.Refused -and $r.Version -ceq $c[3]) "got '$($r.Version)' refused $($r.Refused) '$($r.Reason)'" }
    else { Check $label ($r.Refused -and $null -eq $r.Version -and $r.Reason -like "*$($c[4])*") "got '$($r.Version)' refused $($r.Refused) '$($r.Reason)'" }
}

Write-Host 'B6 -- the description: the third-party form or the author''s (owner ruling 2026-10-02)'
$sec = 'Security: that standard library carries no Go security fixes issued after Go 1.24.13; review before any production use.'
$third = "unofficial go2cs C# conversion of github.com/acme/widget v1.2.3, built on the Go 1.24.13 standard library; not affiliated with or endorsed by Acme or the Go project. $sec"
$author = "go2cs C# conversion of github.com/acme/widget v1.2.3, published by its author, built on the Go 1.24.13 standard library; not affiliated with or endorsed by the Go project. $sec"
$ds = @(
    # module path, repository URL, -UpstreamPublishes, expected description (literal) or $null, reason fragment when refused
    @('github.com/acme/widget', 'https://github.com/someone/widget-cs', $false, $third, $null),
    @('github.com/acme/widget', 'https://github.com/acme/widget-cs', $true, $author, $null),
    @('github.com/acme/widget', 'https://github.com/someone/widget-cs', $true, $null, 'is not under github.com/acme'),
    @('github.com/acme/widget', 'https://github.com/acme/widget-cs', $false, $null, 'pass -UpstreamPublishes'),
    @('gopkg.in/acme/widget.v1', 'https://github.com/acme/widget-cs', $true, $null, 'cannot corroborate authorship for gopkg.in/acme/widget.v1'),
    @('example.com/widget', 'https://example.com/widget-cs', $true, $null, 'cannot corroborate authorship'),
    # host and org compare case-insensitively, as the hosts themselves do
    @('github.com/Acme/widget', 'https://GitHub.com/acme/widget-cs', $true, $author.Replace('github.com/acme/widget', 'github.com/Acme/widget'), $null)
)
foreach ($c in $ds) {
    $module = $c[0]; $label = "$module from $($c[1])$(if ($c[2]) { ' -UpstreamPublishes' })"
    $r = Get-NugetgoDescription -ModulePath $module -GoVersion 'v1.2.3' -GoRelease '1.24.13' -Upstream 'Acme' -RepositoryUrl $c[1] -UpstreamPublishes:$c[2]
    if ($c[3]) { Check $label (-not $r.Refused -and $r.Description -ceq $c[3] -and $r.Author -eq $c[2]) "got refused $($r.Refused) '$($r.Reason)' '$($r.Description)'" }
    else { Check $label ($r.Refused -and $null -eq $r.Description -and $r.Reason -like "*$($c[4])*") "got refused $($r.Refused) '$($r.Reason)' '$($r.Description)'" }
}
Check 'neither form carries "PROOF:" (owner review of the hashset revision-1 preview, 2026-10-10)' (-not ($third + $author).Contains('PROOF')) 'PROOF in a description'
$pack = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot 'nugetgo-pack.ps1'))
Check 'nugetgo-pack.ps1 refuses a description carrying "PROOF:", before the pack and in the read-back' (
    $pack.Contains('if ($description.Contains(''PROOF:''))') -and $pack.Contains('if ($md.description.Contains(''PROOF:''))')) 'no such refusal'
Check 'the packed tags are go2cs;golang;go, without PROOF (COORD ruling C, 2026-10-10: a chip a nuget.org reader cannot read)' (
    $pack.Contains('<PackageTags>go2cs;golang;go</PackageTags>') -and -not ($pack -match '<PackageTags>[^<]*PROOF')) 'PackageTags still carries PROOF'
Check 'nugetgo-pack.ps1 refuses PROOF in the read-back tags' ($pack.Contains('if ($md.tags -match ''\bPROOF\b'')')) 'no read-back refusal of the tag'
Check 'nugetgo-pack.ps1 takes its description from Get-NugetgoDescription, under -UpstreamPublishes' ($pack.Contains('Get-NugetgoDescription -ModulePath') -and
    $pack.Contains('-UpstreamPublishes:$UpstreamPublishes') -and $pack.Contains('[switch]$UpstreamPublishes') -and -not $pack.Contains('$description = "PROOF')) 'the pack script still spells the description itself'

Write-Host 'B6 -- the module license file'
Import-Module (Join-Path $PSScriptRoot 'NugetgoLicense.psm1') -Force
# ONE list: the converter's moduleLicenseNames (src/go2cs/licensing.go) and this module's copy, same names, same order.
$licensingGo = Join-Path (Split-Path (Split-Path $PSScriptRoot)) 'go2cs/licensing.go'
$goBlock = [regex]::Match([System.IO.File]::ReadAllText($licensingGo), '(?s)var moduleLicenseNames = \[\]string\{(.*?)\}')
$goNames = @([regex]::Matches($goBlock.Groups[1].Value, '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value })
$psNames = @(Get-NugetgoLicenseNames)
Check 'license names: the pack module''s list IS licensing.go''s moduleLicenseNames, in order' ($goBlock.Success -and $goNames.Count -gt 0 -and
    $goNames.Count -eq $psNames.Count -and -not @(for ($i = 0; $i -lt $goNames.Count; $i++) { if ($goNames[$i] -cne $psNames[$i]) { $i } }).Count) "licensing.go [$($goNames -join ', ')] vs pack [$($psNames -join ', ')]"

$licenseRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('nugetgo-license-' + [guid]::NewGuid().ToString('N'))
try {
    $mdOnly = New-Item -ItemType Directory -Force (Join-Path $licenseRoot 'md-only')
    Set-Content -LiteralPath (Join-Path $mdOnly 'LICENSE.md') -Value 'Upstream terms'
    Set-Content -LiteralPath (Join-Path $mdOnly 'go.mod') -Value 'module example.com/mdonly'
    $r = Find-NugetgoModuleLicense -ModuleDir $mdOnly
    Check 'a module shipping LICENSE.md only: accepted under its own name' ($r.Name -ceq 'LICENSE.md' -and $r.Path -eq (Join-Path $mdOnly 'LICENSE.md') -and $null -eq $r.Reason) "got Name '$($r.Name)' Reason '$($r.Reason)'"

    $both = New-Item -ItemType Directory -Force (Join-Path $licenseRoot 'both')
    Set-Content -LiteralPath (Join-Path $both 'COPYING') -Value 'Upstream terms'
    Set-Content -LiteralPath (Join-Path $both 'License.txt') -Value 'Upstream terms'
    $r = Find-NugetgoModuleLicense -ModuleDir $both
    Check 'preference order, spelled as the directory spells it: License.txt before COPYING' ($r.Name -ceq 'License.txt') "got Name '$($r.Name)'"

    $none = New-Item -ItemType Directory -Force (Join-Path $licenseRoot 'none')
    Set-Content -LiteralPath (Join-Path $none 'go.mod') -Value 'module example.com/none'
    New-Item -ItemType Directory -Force (Join-Path $none 'LICENSE') | Out-Null   # a DIRECTORY named LICENSE is not a license file
    $r = Find-NugetgoModuleLicense -ModuleDir $none
    Check 'a module shipping no license file: refused by name' ($null -eq $r.Name -and $r.Reason -like "*no license file in $none*" -and $r.Reason -like '*COPYING.txt*') "got Name '$($r.Name)' Reason '$($r.Reason)'"
}
finally { Remove-Item -Recurse -Force -LiteralPath $licenseRoot -ErrorAction SilentlyContinue }

# The pack script takes its license from the shared lookup and packs it under the upstream file's own name.
$pack = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot 'nugetgo-pack.ps1'))
Check 'nugetgo-pack.ps1 resolves the module license through Find-NugetgoModuleLicense' ($pack.Contains('Find-NugetgoModuleLicense -ModuleDir') -and -not $pack.Contains("'LICENSE') }")) 'the pack script still hard-codes LICENSE'
Check 'nugetgo-pack.ps1 packs the license under its own name' ($pack.Contains('<PackageLicenseFile>$(& $esc $licenseName)</PackageLicenseFile>') -and
    $pack.Contains('<None Include="$(& $esc $licenseName)" Pack="true" PackagePath="" />') -and -not $pack.Contains('<None Include="LICENSE" Pack="true"')) 'the pack project still names LICENSE literally'

Write-Host 'B6 -- the packed assemblies'' copyright (owner ruling 2026-10-04: the upstream holder)'
# Every expected value is a LITERAL. The license texts are the fixture modules' own Copyright lines.
$scaffold = 'go2cs scaffolding: Copyright (c) 2018-2026 The go2cs Authors'
$copyRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('nugetgo-copyright-' + [guid]::NewGuid().ToString('N'))
function Write-License([string]$dir, [string]$name, [string[]]$lines) {
    $d = New-Item -ItemType Directory -Force (Join-Path $copyRoot $dir)
    $path = Join-Path $d $name
    [System.IO.File]::WriteAllText($path, (($lines + '') -join "`n"))
    return $path
}
try {
    # github.com/google/uuid v1.6.0: one line, a comma inside the holder, and a lower-case "copyright notice" clause.
    $r = Get-NugetgoAssemblyCopyright -LicenseFile (Write-License 'uuid' 'LICENSE' @('Copyright (c) 2009,2014 Google Inc. All rights reserved.', '',
        'Redistribution and use in source and binary forms, with or without', 'modification, are permitted provided that the following conditions are', 'met:', '',
        '   * Redistributions of source code must retain the above', 'copyright notice, this list of conditions and the following disclaimer.'))
    Check 'one upstream line (uuid): the holder, then the scaffolding line; the "copyright notice" clause is not a line' ($r.Copyright -ceq "Copyright (c) 2009,2014 Google Inc. All rights reserved.; $scaffold" -and -not $r.Skip) "got '$($r.Copyright)' skip $($r.Skip)"
    # github.com/golang-jwt/jwt/v5 v5.3.1: TWO Copyright lines, kept in file order.
    $r = Get-NugetgoAssemblyCopyright -LicenseFile (Write-License 'jwt' 'LICENSE' @('Copyright (c) 2012 Dave Grijalva', 'Copyright (c) 2021 golang-jwt maintainers', '',
        'Permission is hereby granted, free of charge, to any person obtaining a copy'))
    Check 'two upstream lines (jwt/v5): both, in file order, then the scaffolding line' ($r.Copyright -ceq "Copyright (c) 2012 Dave Grijalva; Copyright (c) 2021 golang-jwt maintainers; $scaffold") "got '$($r.Copyright)'"
    # github.com/joho/godotenv v1.5.1 spells its file LICENCE: found by the shared lookup, read the same way.
    [void](Write-License 'godotenv' 'LICENCE' @('Copyright (c) 2013 John Barton', '', 'MIT License'))
    $found = Find-NugetgoModuleLicense -ModuleDir (Join-Path $copyRoot 'godotenv')
    $r = if ($found.Path) { Get-NugetgoAssemblyCopyright -LicenseFile $found.Path } else { $null }
    Check 'a LICENCE file (godotenv): found under its own name and read' ($found.Name -ceq 'LICENCE' -and $r.Copyright -ceq "Copyright (c) 2013 John Barton; $scaffold") "got Name '$($found.Name)', '$($r.Copyright)'"
    # github.com/ritchiecarroll/hashset: its holder IS The go2cs Authors, so the template's value stands.
    $r = Get-NugetgoAssemblyCopyright -LicenseFile (Write-License 'hashset' 'LICENSE' @('MIT License', '', 'Copyright (c) 2021-2026 The go2cs Authors'))
    Check 'every line names The go2cs Authors (hashset): skipped, the template''s value stands' ($r.Skip -and $null -eq $r.Copyright -and $r.Reason -like '*The go2cs Authors*') "got skip $($r.Skip), '$($r.Copyright)', reason '$($r.Reason)'"
    $r = Get-NugetgoAssemblyCopyright -LicenseFile (Write-License 'mixed' 'LICENSE' @('Copyright (c) 2021-2026 The go2cs Authors', 'Copyright (c) 2024 Someone Else'))
    Check 'a go2cs line beside another holder: not skipped, both lines kept' (-not $r.Skip -and $r.Copyright -ceq "Copyright (c) 2021-2026 The go2cs Authors; Copyright (c) 2024 Someone Else; $scaffold") "got skip $($r.Skip), '$($r.Copyright)'"
    $r = Get-NugetgoAssemblyCopyright -LicenseFile (Write-License 'nocopy' 'LICENSE' @('Public domain.'))
    Check 'a license with no Copyright line: refused by name' ($null -eq $r.Copyright -and -not $r.Skip -and $r.Reason -like '*carries no Copyright line*') "got '$($r.Copyright)' skip $($r.Skip) reason '$($r.Reason)'"

    # The escaping case: a semicolon, a comma and every MSBuild or XML special character survive as one property value.
    $hostile = "Copyright (c) 2024 Alpha; Beta, Gamma & Delta <x> 100% `$(Evil) @(Items) it's *?; $scaffold"
    $targets = New-NugetgoAssemblyMetadataTargets -ModulePath 'example.com/mod' -Copyright $hostile -Company 'go2cs conversion' -Authors 'go2cs conversion'
    $xml = $null; try { $xml = [xml]$targets } catch { }
    Check 'the generated Directory.Build.targets is well-formed XML carrying the pack''s marker' ($null -ne $xml -and $null -ne $xml.Project -and $targets.Contains('<!-- Generated by nugetgo-pack.ps1. -->') -and $targets.Contains((Get-NugetgoTargetsMarker))) 'not XML, or no marker'
    Check 'a semicolon, a comma and MSBuild''s special characters are escaped as one literal value' ($targets.Contains('<Copyright>Copyright (c) 2024 Alpha%3B Beta, Gamma &amp; Delta &lt;x&gt; 100%25 %24(Evil) %40(Items) it%27s %2A%3F%3B go2cs scaffolding: Copyright (c) 2018-2026 The go2cs Authors</Copyright>')) $targets
    Check 'company and authors are the given value' ($targets.Contains('<Company>go2cs conversion</Company>') -and $targets.Contains('<Authors>go2cs conversion</Authors>')) $targets
    Check 'it imports the output root''s Directory.Build.targets first (MSBuild imports only the nearest one)' ($targets.Contains('<NugetgoParentBuildTargets>$([MSBuild]::GetPathOfFileAbove(''Directory.Build.targets'', ''$(MSBuildThisFileDirectory)../''))</NugetgoParentBuildTargets>') -and
        $targets.Contains('<Import Project="$(NugetgoParentBuildTargets)" Condition="''$(NugetgoParentBuildTargets)'' != ''''" />')) $targets
    Check 'it applies to the packed projects only: a library, never a converted test project' ($targets.Contains("<PropertyGroup Condition=`"'`$(OutputType)' == 'Library' and !`$(MSBuildProjectName.EndsWith('.tests'))`">")) $targets
}
finally { Remove-Item -Recurse -Force -LiteralPath $copyRoot -ErrorAction SilentlyContinue }

# The pack script writes the targets beside the packed projects, shares the nupkg's Copyright extraction, refuses a
# foreign file, and reads the attribute back from each dll taken out of the nupkg.
Check 'nugetgo-pack.ps1 writes the assembly copyright beside the packed projects and reads it back from each dll' (
    $pack.Contains('Get-NugetgoAssemblyCopyright -LicenseFile $LicenseFile') -and $pack.Contains('$moduleTargets = Join-Path $moduleSrc ''Directory.Build.targets''') -and
    $pack.Contains('New-NugetgoAssemblyMetadataTargets -ModulePath $ModulePath') -and $pack.Contains('was not written by nugetgo-pack.ps1') -and
    $pack.Contains('$copyright = (@(Get-NugetgoCopyrightLines -LicenseFile $LicenseFile) -join ''; '')') -and
    $pack.Contains('[System.Diagnostics.FileVersionInfo]::GetVersionInfo($dll)')) 'the pack script does not wire the assembly copyright end to end'
# The nupkg's own Copyright field takes the same MSBuild escaping (XML escaping alone let MSBuild expand a '$(...)' or
# '@(...)' inside a holder string), and the read-back holds it to the license file's lines exactly.
Check 'nugetgo-pack.ps1 escapes the nupkg Copyright for MSBuild and reads it back exactly' ($pack.Contains('<Copyright>$(ConvertTo-NugetgoMSBuildLiteral $copyright)</Copyright>') -and
    $pack.Contains('if ($md.copyright -cne $copyright)')) 'the nupkg Copyright is not MSBuild-escaped or not read back'
# The same for every other free-text value the generated project carries: the description (it quotes -Upstream), the
# release notes, the authors, and both URLs (a URL's '%20' is an escape MSBuild would otherwise decode).
Check 'nugetgo-pack.ps1 escapes the description, authors and URLs for MSBuild and reads authors and URLs back' (
    $pack.Contains('<Authors>$(ConvertTo-NugetgoMSBuildLiteral $Authors)</Authors>') -and
    $pack.Contains('<Description>$(ConvertTo-NugetgoMSBuildLiteral $description)</Description>') -and
    $pack.Contains('<PackageReleaseNotes>$(ConvertTo-NugetgoMSBuildLiteral $description)</PackageReleaseNotes>') -and
    $pack.Contains('<RepositoryUrl>$(ConvertTo-NugetgoMSBuildLiteral $RepositoryUrl)</RepositoryUrl>') -and
    $pack.Contains('<PackageProjectUrl>$(ConvertTo-NugetgoMSBuildLiteral $RepositoryUrl)</PackageProjectUrl>') -and
    $pack.Contains('if ($md.authors -cne $Authors)') -and $pack.Contains('$md.repository.url -cne $RepositoryUrl -or ([Uri]$md.projectUrl).AbsoluteUri -cne ([Uri]$RepositoryUrl).AbsoluteUri')) 'a free-text value is not MSBuild-escaped or not read back'

Write-Host "ran $ran, failed $failed"
exit $failed
