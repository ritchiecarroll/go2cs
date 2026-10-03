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

Write-Host "ran $ran, failed $failed"
exit $failed
