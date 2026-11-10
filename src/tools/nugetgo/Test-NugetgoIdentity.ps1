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
$sec = 'Security: the Go standard library this package is built on includes no Go security fixes released after Go 1.24.13; review before any production use.'
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

    # github.com/spf13/cobra v1.10.2 (COORD ruling 2026-10-26): its Apache LICENSE.txt carries no Copyright line, and
    # the holder is in every source file's header. The caller states it with -Copyright, used ONLY when the license
    # carries none, and it must be one of the module's own source-header lines, which the pack prints as the evidence.
    $cobraDir = Join-Path $copyRoot 'cobra'
    [void](Write-License 'cobra' 'LICENSE.txt' @('                                 Apache License', '      "Licensor" shall mean the copyright owner', '   2. Grant of Copyright License.'))
    [void](Write-License 'cobra' 'command.go' @('// Copyright 2013-2023 The Cobra Authors', '//', '// Licensed under the Apache License', 'package cobra'))
    [void](Write-License (Join-Path 'cobra' 'doc') 'man_docs.go' @('// Copyright 2013-2023 The Cobra Authors', 'package doc'))
    [void](Write-License 'cobra' 'command_test.go' @('// Copyright 1999 Someone Testing', 'package cobra'))
    $cobraLicense = Join-Path $cobraDir 'LICENSE.txt'
    $headers = @(Get-NugetgoSourceCopyrightLines -ModuleDir $cobraDir)
    Check 'the source headers: one distinct line, counted over the non-test files' ($headers.Count -eq 1 -and $headers[0].Line -ceq 'Copyright 2013-2023 The Cobra Authors' -and $headers[0].Files -eq 2) "got $(@($headers | ForEach-Object { "$($_.Line) x$($_.Files)" }) -join ' | ')"
    $r = Resolve-NugetgoCopyright -LicenseFile $cobraLicense -Stated 'Copyright 2013-2023 The Cobra Authors' -ModuleDir $cobraDir
    Check 'a stated line for a license with none (cobra): used, with its header evidence' ($null -eq $r.Reason -and $r.Source -ceq 'stated' -and ($r.Lines -join '|') -ceq 'Copyright 2013-2023 The Cobra Authors' -and $r.Evidence -like '*2 source file*') "got reason '$($r.Reason)' source '$($r.Source)' lines '$($r.Lines -join '|')' evidence '$($r.Evidence)'"
    $r = Resolve-NugetgoCopyright -LicenseFile $cobraLicense -Stated '' -ModuleDir $cobraDir
    Check 'no stated line for a license with none: refused, naming -Copyright and the headers' ($r.Reason -like '*carries no Copyright line*-Copyright*Copyright 2013-2023 The Cobra Authors*') "got '$($r.Reason)'"
    $r = Resolve-NugetgoCopyright -LicenseFile $cobraLicense -Stated 'Copyright 2026 Somebody Else' -ModuleDir $cobraDir
    Check 'a stated line that is no source header: refused' ($r.Reason -like '*is not a Copyright line of the module*source headers*') "got '$($r.Reason)'"
    $r = Resolve-NugetgoCopyright -LicenseFile (Join-Path $copyRoot 'uuid/LICENSE') -Stated 'Copyright 2013-2023 The Cobra Authors' -ModuleDir $cobraDir
    Check 'a stated line for a license that carries one: refused' ($r.Reason -like '*carries 1 Copyright line*-Copyright applies only*') "got '$($r.Reason)'"
    $r = Resolve-NugetgoCopyright -LicenseFile (Join-Path $copyRoot 'jwt/LICENSE') -Stated '' -ModuleDir $cobraDir
    Check 'no stated line, a license with lines: the license''s, unchanged' ($null -eq $r.Reason -and $r.Source -ceq 'license' -and ($r.Lines -join '|') -ceq 'Copyright (c) 2012 Dave Grijalva|Copyright (c) 2021 golang-jwt maintainers') "got '$($r.Reason)' '$($r.Lines -join '|')'"
    $r = Get-NugetgoAssemblyCopyright -LicenseFile $cobraLicense -Lines @('Copyright 2013-2023 The Cobra Authors')
    Check 'the assembly copyright from resolved lines: the stated holder, then the scaffolding line' ($r.Copyright -ceq "Copyright 2013-2023 The Cobra Authors; $scaffold" -and -not $r.Skip) "got '$($r.Copyright)'"

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
    $pack.Contains('Get-NugetgoAssemblyCopyright -LicenseFile $LicenseFile -Lines $resolvedCopyright.Lines') -and $pack.Contains('$moduleTargets = Join-Path $moduleSrc ''Directory.Build.targets''') -and
    $pack.Contains('New-NugetgoAssemblyMetadataTargets -ModulePath $ModulePath') -and $pack.Contains('was not written by nugetgo-pack.ps1') -and
    $pack.Contains('$resolvedCopyright = Resolve-NugetgoCopyright -LicenseFile $LicenseFile -Stated $Copyright -ModuleDir $moduleCacheDir') -and
    $pack.Contains('$copyright = ($resolvedCopyright.Lines -join ''; '')') -and
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

Write-Host 'Apache-2.0 4(b) -- the per-file modification notice'
# A license whose terms ask each modified file to state that it was changed: the pack converts with -provenance and
# refuses a -RecurseRoot whose converted files carry no notice. The SPDX expression is stated (-LicenseSpdx), never
# inferred, and only a LICENSE identifier counts: the one after WITH names an exception.
foreach ($c in @(
        @('Apache-2.0', $true), @('apache-2.0', $true), @('MIT OR Apache-2.0', $true), @('Apache-2.0 AND MIT', $true),
        @('Apache-2.0 WITH LLVM-exception', $true), @('Apache-2.0+', $true),
        @('MIT', $false), @('BSD-3-Clause', $false), @('Apache-1.1', $false), @('MIT WITH Apache-2.0', $false))) {
    $got = Test-NugetgoProvenanceLicense $c[0]
    Check "'$($c[0])' $(if ($c[1]) { 'asks for' } else { 'does not ask for' }) the notice" ($got -eq $c[1]) "got $got"
}
# The notice the pack looks for IS the converter's (sourceProvenance in src/go2cs/licensing.go).
$noticeGo = [regex]::Match([System.IO.File]::ReadAllText($licensingGo), '// (Modified from the original by conversion to C# with go2cs) %s\.')
Check 'the notice the pack looks for is licensing.go''s' ($noticeGo.Success -and $noticeGo.Groups[1].Value -ceq (Get-NugetgoProvenanceNotice)) "licensing.go '$($noticeGo.Groups[1].Value)' vs pack '$(Get-NugetgoProvenanceNotice)'"
$provenanceRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('nugetgo-provenance-' + [guid]::NewGuid().ToString('N'))
try {
    $lib = New-Item -ItemType Directory -Force (Join-Path $provenanceRoot 'lib')
    $project = Join-Path $lib 'example.com.mod.lib.csproj'
    Set-Content -LiteralPath $project -Value '<Project />'
    $notice = "// Converted from Go source: ""a.go"" in module example.com/mod@v1.0.0.`n// $(Get-NugetgoProvenanceNotice) 1.26.10.3.`n"
    Set-Content -LiteralPath (Join-Path $lib 'a.cs') -Value "// Copyright 2020 Example`n`n$notice`nnamespace go.example.com.mod;"
    Set-Content -LiteralPath (Join-Path $lib 'package_info.cs') -Value 'namespace go.example.com.mod;'
    [void](New-Item -ItemType Directory -Force (Join-Path $lib 'obj'))
    Set-Content -LiteralPath (Join-Path $lib 'obj/AssemblyInfo.cs') -Value '// generated'
    $missing = @(Find-NugetgoFilesWithoutProvenance -ProjectFiles @(Get-Item -LiteralPath $project))
    Check 'every converted file carries the notice: nothing missing (package_info.cs and obj/ are not conversions)' ($missing.Count -eq 0) "missing $($missing -join ', ')"
    Set-Content -LiteralPath (Join-Path $lib 'b.cs') -Value 'namespace go.example.com.mod;'
    $missing = @(Find-NugetgoFilesWithoutProvenance -ProjectFiles @(Get-Item -LiteralPath $project))
    Check 'a converted file without the notice is named' ($missing.Count -eq 1 -and $missing[0] -eq (Join-Path $lib 'b.cs')) "missing $($missing -join ', ')"
}
finally { Remove-Item -Recurse -Force -LiteralPath $provenanceRoot -ErrorAction SilentlyContinue }
Check 'nugetgo-pack.ps1 converts each flavor with -provenance and refuses an unmarked -RecurseRoot when the license asks for the notice' (
    $pack.Contains('$provenance = Test-NugetgoProvenanceLicense $LicenseSpdx') -and
    $pack.Contains('$unmarked = @(Find-NugetgoFilesWithoutProvenance -ProjectFiles $libraries.ToArray())') -and
    $pack.Contains('convert it again with go2cs -recurse=nuget -provenance') -and
    $pack.Contains('& $converter -recurse=nuget @provenanceArgs -platforms')) 'the pack script does not wire the notice'
$repack = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Invoke-NugetgoRepack.ps1'))
Check 'Invoke-NugetgoRepack.ps1 converts PR with -provenance when the row''s license asks for the notice' (
    $repack.Contains('$provenanceArgs = @(if (Test-NugetgoProvenanceLicense $row.LicenseSpdx) { ''-provenance'' })') -and
    $repack.Contains('(@(''-recurse=nuget'') + $provenanceArgs + @(''-platforms''')) 'the repack driver does not pass -provenance to step 1.4'

# The driver passes -provenance only for a module it has a row for. A published module with no row is rebuilt by
# hand, and an Apache-2.0 one then reaches the pack without the notice: mousetrap, Apache-2.0 and without Go tests,
# had no row (P2, 2026-11-10). Each published module's sample program sits under src/tests/ModuleBehavior/<M@V>/.
$moduleRows = Import-PowerShellDataFile -LiteralPath (Join-Path $PSScriptRoot 'nugetgo-modules.psd1')
$behaviorRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../tests/ModuleBehavior'))
$sampled = @(Get-ChildItem -LiteralPath $behaviorRoot -Recurse -Filter 'mappings.txt' -File |
        ForEach-Object { [System.IO.Path]::GetRelativePath($behaviorRoot, $_.DirectoryName) -replace '\\', '/' } | Sort-Object)
$rowless = @($sampled | Where-Object { -not $moduleRows.ContainsKey($_) })
Check 'every module with a sample program under src/tests/ModuleBehavior has a row in nugetgo-modules.psd1' (
    $sampled.Count -gt 0 -and $rowless.Count -eq 0) "$($sampled.Count) sampled; no row: $($rowless -join ', ')"
# The census of the modules whose license asks for the notice, read from the rows, is the one the design note states.
$noticeRows = @($moduleRows.Keys | Where-Object { Test-NugetgoProvenanceLicense $moduleRows[$_].LicenseSpdx } | Sort-Object)
$design = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot '../../../docs/phase4/DESIGN-provenance-header.md'))
$censusLine = [regex]::Match($design, '(?m)^- \*\*The Apache-2\.0 census\*\*[^\n]*: (.+)$')
$stated = if ($censusLine.Success) { @([regex]::Matches($censusLine.Groups[1].Value, '`([^`]+)`') | ForEach-Object { $_.Groups[1].Value } | Sort-Object) } else { @() }
Check 'DESIGN-provenance-header.md names every module whose row asks for the notice, and only those' (
    $noticeRows.Count -gt 0 -and (($stated -join ' ') -ceq ($noticeRows -join ' '))) "rows: $($noticeRows -join ', '); design: $(if ($censusLine.Success) { $stated -join ', ' } else { 'no census line' })"
# A row with no Go tests carries no Expect, and the driver packs it on the runbook's no-tests route: no VR, the reason
# as the rehearsal's -UnvalidatedReason, the hosted rows as the release's -BehaviorProofDir.
$noTestRows = @($moduleRows.Keys | Where-Object { $moduleRows[$_].NoGoTests })
$tangled = @($noTestRows | Where-Object { $moduleRows[$_].Expect })
Check 'a row with NoGoTests has no Expect, and every other row has one' (
    $tangled.Count -eq 0 -and @($moduleRows.Keys | Where-Object { -not $moduleRows[$_].NoGoTests -and -not $moduleRows[$_].Expect }).Count -eq 0) "both or neither: $($tangled -join ', ')"
Check 'Invoke-NugetgoRepack.ps1 packs a row with no Go tests without VR: the reason for the rehearsal, the hosted rows for the release' (
    $repack.Contains('$noGoTests = [string]$row.NoGoTests') -and
    $repack.Contains('if ($noGoTests) {') -and $repack.Contains('Say "1.2-1.3  not run: $noGoTests"') -and
    $repack.Contains('if ($noGoTests) { Say "1.5      not run: no MODULE.md to bind" }') -and
    $repack.Contains("elseif (`$pack -eq 'int1') { @('-UnvalidatedReason', `$noGoTests) }") -and
    $repack.Contains("else { @('-BehaviorProofDir', `$BehaviorProofDir) }") -and
    $repack.Contains('its release pack needs -BehaviorProofDir')) 'the repack driver has no route for a module with no Go tests'

Write-Host "ran $ran, failed $failed"
exit $failed
