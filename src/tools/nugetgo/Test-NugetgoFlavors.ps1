<#
.SYNOPSIS
    Table-driven arms for NugetgoFlavors.psm1: a nugetgo package carries RID-specific assemblies exactly when its
    flavors differ in MEANING, decided by the same material check the go.* release uses (GoAssemblyMaterial.psm1), and
    in the same layout the go.* release ships (push-nuget.ps1). COORD ruling 2026-10-12, for mousetrap and cobra:
    the decision comes from a measurement, never from the module author or a list. Exit code = the number of failed
    cases.
#>
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NugetgoFlavors.psm1') -Force
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}
# The material check's own fixtures (src/tools/pushnuget/testdata/build-fixtures.ps1): alpha and bravo are one source
# with an element type of Dep.Alpha or Dep.Bravo, equal in length and different in meaning; cascade-a and cascade-b are
# one source built against two Deps, different only in Roslyn's deterministic identity fields.
function Fixture([string]$name) { , [System.IO.File]::ReadAllBytes((Join-Path $PSScriptRoot "../pushnuget/testdata/$name")) }
$alpha = Fixture 'alpha.dll'
$bravo = Fixture 'bravo.dll'
$cascadeA = Fixture 'cascade-a.dll'
$cascadeB = Fixture 'cascade-b.dll'
# One assembly set per flavor; the linux/arm64 flavor agrees with linux/amd64's unless a case gives it its own.
function Flavors($windows, $linux, $darwin, $linuxArm = $linux) { [ordered]@{ 'windows/amd64' = $windows; 'linux/amd64' = $linux; 'darwin/amd64' = $darwin; 'linux/arm64' = $linuxArm } }

Write-Host 'the RID map is the go.* release''s'
$rids = Get-NugetgoRidFlavors
$release = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot '../../push-nuget.ps1'))
$block = [regex]::Match($release, '(?s)\$ridFlavors = \[ordered\]@\{(?<body>.*?)\}')
$releaseMap = [ordered]@{}
foreach ($m in [regex]::Matches($block.Groups['body'].Value, "'(?<rid>[^']+)'\s*=\s*'(?<flavor>[^']+)'")) { $releaseMap[$m.Groups['rid'].Value] = $m.Groups['flavor'].Value }
Check 'push-nuget.ps1 has a $ridFlavors map to mirror' ($releaseMap.Count -gt 0) 'no map found: the release script moved'
Check 'the same RIDs, the same flavors, the same order (the first is the reference flavor)' (
    (@($rids.Keys) -join ',') -ceq (@($releaseMap.Keys) -join ',') -and (@($rids.Values) -join ',') -ceq (@($releaseMap.Values) -join ',')) `
    "nugetgo $(@($rids.Keys | ForEach-Object { "$_=$($rids[$_])" }) -join ',') vs release $(@($releaseMap.Keys | ForEach-Object { "$_=$($releaseMap[$_])" }) -join ',')"
Check 'the reference flavor is windows/amd64' ((Get-NugetgoReferenceFlavor) -ceq 'windows/amd64') "got $(Get-NugetgoReferenceFlavor)"
Check 'linux-arm64 ships linux/arm64, a flavor of its own and not linux-x64''s twin (S6)' (
    $rids.Contains('linux-arm64') -and $rids['linux-arm64'] -ceq 'linux/arm64' -and $rids['linux-x64'] -ceq 'linux/amd64') "got $(@($rids.Keys | ForEach-Object { "$_=$($rids[$_])" }) -join ',')"
Check 'both Mac RIDs ship the one darwin/amd64 flavor' ($rids['osx-x64'] -ceq 'darwin/amd64' -and $rids['osx-arm64'] -ceq 'darwin/amd64') "got osx-x64=$($rids['osx-x64']), osx-arm64=$($rids['osx-arm64'])"

Write-Host 'the decision is a measurement'
$split = Get-NugetgoFlavorDecision -Assemblies (Flavors @{ 'App.dll' = $alpha } @{ 'App.dll' = $bravo } @{ 'App.dll' = $bravo }) -ProofFlavor 'linux/amd64'
Check 'a GOOS-split pair (meaning differs) is RID-specific' ($split.RidSpecific) 'packed neutral'
Check 'its reason names the assembly and the flavor' (@($split.Reasons | Where-Object { $_ -like '*windows*App.dll*' }).Count -eq 1) "reasons: $($split.Reasons -join ' | ')"
$identity = Get-NugetgoFlavorDecision -Assemblies (Flavors @{ 'App.dll' = $cascadeA } @{ 'App.dll' = $cascadeB } @{ 'App.dll' = $cascadeA }) -ProofFlavor 'linux/amd64'
Check 'an identity-only pair is neutral (the cascade is not material)' (-not $identity.RidSpecific) "reasons: $($identity.Reasons -join ' | ')"
$planted = Get-NugetgoFlavorDecision -Assemblies (Flavors @{ 'App.dll' = $alpha; 'Dep.dll' = $bravo } @{ 'App.dll' = $alpha; 'Dep.dll' = $bravo } @{ 'App.dll' = $alpha; 'Dep.dll' = $bravo }) -ProofFlavor 'linux/amd64'
Check 'a planted identical pair is neutral' (-not $planted.RidSpecific) "reasons: $($planted.Reasons -join ' | ')"
$armOnly = Get-NugetgoFlavorDecision -Assemblies (Flavors @{ 'App.dll' = $alpha } @{ 'App.dll' = $alpha } @{ 'App.dll' = $alpha } @{ 'App.dll' = $bravo }) -ProofFlavor 'linux/amd64'
Check 'a difference at linux/arm64 alone is RID-specific (it is not linux/amd64''s twin)' ($armOnly.RidSpecific -and @($armOnly.Reasons | Where-Object { $_ -like '*linux/arm64*' }).Count -eq 1) "reasons: $($armOnly.Reasons -join ' | ')"
$darwinOnly = Get-NugetgoFlavorDecision -Assemblies (Flavors @{ 'App.dll' = $alpha } @{ 'App.dll' = $alpha } @{ 'App.dll' = $bravo }) -ProofFlavor 'linux/amd64'
Check 'a difference in one flavor alone is RID-specific' ($darwinOnly.RidSpecific -and @($darwinOnly.Reasons | Where-Object { $_ -like '*darwin*' }).Count -eq 1) "reasons: $($darwinOnly.Reasons -join ' | ')"
$missing = Get-NugetgoFlavorDecision -Assemblies (Flavors @{ 'App.dll' = $alpha } @{ 'App.dll' = $alpha; 'Extra.dll' = $alpha } @{ 'App.dll' = $alpha; 'Extra.dll' = $alpha }) -ProofFlavor 'linux/amd64'
Check 'a different assembly set is RID-specific' ($missing.RidSpecific -and @($missing.Reasons | Where-Object { $_ -like '*Extra.dll*' }).Count -ge 1) "reasons: $($missing.Reasons -join ' | ')"
$noFlavor = $null
try { Get-NugetgoFlavorDecision -Assemblies ([ordered]@{ 'windows/amd64' = @{ 'App.dll' = $alpha }; 'linux/amd64' = @{ 'App.dll' = $alpha }; 'linux/arm64' = @{ 'App.dll' = $alpha } }) -ProofFlavor 'linux/amd64' | Out-Null }
catch { $noFlavor = $_.Exception.Message }
Check 'a missing flavor is refused, never read as neutral' ($noFlavor -like '*darwin*') "got: $noFlavor"

Write-Host 'the RID-specific layout is the go.* release''s'
$scratch = Join-Path ([System.IO.Path]::GetTempPath()) "nugetgo-flavors-$PID"
New-Item -ItemType Directory -Force $scratch | Out-Null
function New-Package([string]$path, [hashtable]$lib) {
    if (Test-Path $path) { Remove-Item $path }
    $zip = [System.IO.Compression.ZipFile]::Open($path, 'Create')
    try {
        foreach ($name in @('nugetgo.example.nuspec', 'README.md') + @($lib.Keys | ForEach-Object { "lib/net10.0/$_" })) {
            $bytes = if ($name -like 'lib/*') { $lib[(Split-Path -Leaf $name)] } else { [System.Text.Encoding]::UTF8.GetBytes($name) }
            $s = $zip.CreateEntry($name).Open(); try { $s.Write($bytes, 0, $bytes.Length) } finally { $s.Dispose() }
        }
    }
    finally { $zip.Dispose() }
}
function Entries([string]$path) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($path)
    try {
        $map = @{}
        foreach ($e in $zip.Entries) { $ms = New-Object System.IO.MemoryStream; $s = $e.Open(); try { $s.CopyTo($ms) } finally { $s.Dispose() }; $map[$e.FullName] = $ms.ToArray() }
        $map
    }
    finally { $zip.Dispose() }
}
function Same($a, $b) { $null -ne $a -and $null -ne $b -and [System.Linq.Enumerable]::SequenceEqual([byte[]]$a, [byte[]]$b) }

$pkg = Join-Path $scratch 'split.nupkg'
New-Package $pkg @{ 'App.dll' = $bravo }   # packed from the proof flavor (linux)
$assemblies = Flavors @{ 'App.dll' = $alpha } @{ 'App.dll' = $bravo } @{ 'App.dll' = $cascadeB } @{ 'App.dll' = $cascadeA }
Set-NugetgoRidAssets -Package $pkg -Assemblies $assemblies -TargetFramework net10.0
$e = Entries $pkg
Check 'lib/ carries the reference (windows) flavor' (Same $e['lib/net10.0/App.dll'] $alpha) 'lib/ is not the windows assembly'
Check 'runtimes/win-x64 carries windows' (Same $e['runtimes/win-x64/lib/net10.0/App.dll'] $alpha) 'missing or wrong'
Check 'runtimes/linux-x64 carries linux' (Same $e['runtimes/linux-x64/lib/net10.0/App.dll'] $bravo) 'missing or wrong'
Check 'runtimes/osx-x64 carries darwin' (Same $e['runtimes/osx-x64/lib/net10.0/App.dll'] $cascadeB) 'missing or wrong'
Check 'runtimes/osx-arm64 carries the same darwin bytes' (Same $e['runtimes/osx-arm64/lib/net10.0/App.dll'] $cascadeB) 'missing or wrong'
Check 'runtimes/linux-arm64 carries linux/arm64, not linux-x64''s bytes' (Same $e['runtimes/linux-arm64/lib/net10.0/App.dll'] $cascadeA) 'missing or wrong'
Check 'nothing else moved (the nuspec and README are untouched)' (
    (Same $e['README.md'] ([System.Text.Encoding]::UTF8.GetBytes('README.md'))) -and @($e.Keys | Where-Object { $_ -like 'runtimes/*' }).Count -eq 5) "entries: $(@($e.Keys | Sort-Object) -join ', ')"
Check 'the read-back accepts it' (@(Test-NugetgoRidLayout -Package $pkg -Assemblies $assemblies -TargetFramework net10.0).Count -eq 0) "$(@(Test-NugetgoRidLayout -Package $pkg -Assemblies $assemblies -TargetFramework net10.0) -join ' | ')"

$bad = Join-Path $scratch 'twins.nupkg'
New-Package $bad @{ 'App.dll' = $bravo }
Set-NugetgoRidAssets -Package $bad -Assemblies $assemblies -TargetFramework net10.0
$zip = [System.IO.Compression.ZipFile]::Open($bad, 'Update')
try { $old = $zip.GetEntry('runtimes/osx-arm64/lib/net10.0/App.dll'); if ($old) { $old.Delete() }; $s = $zip.CreateEntry('runtimes/osx-arm64/lib/net10.0/App.dll').Open(); try { $s.Write($alpha, 0, $alpha.Length) } finally { $s.Dispose() } }
finally { $zip.Dispose() }
$why = @(Test-NugetgoRidLayout -Package $bad -Assemblies $assemblies -TargetFramework net10.0)
Check 'the read-back refuses Mac twins that differ' (@($why | Where-Object { $_ -like '*osx-arm64*' }).Count -ge 1) "reasons: $($why -join ' | ')"
$short = Join-Path $scratch 'short.nupkg'
New-Package $short @{ 'App.dll' = $bravo }
$why = @(Test-NugetgoRidLayout -Package $short -Assemblies $assemblies -TargetFramework net10.0)
Check 'the read-back refuses a package with no runtimes/ folder' (@($why | Where-Object { $_ -like '*win-x64*' }).Count -ge 1) "reasons: $($why -join ' | ')"
$wrongSet = $null
try { New-Package $short @{ 'Other.dll' = $bravo }; Set-NugetgoRidAssets -Package $short -Assemblies $assemblies -TargetFramework net10.0 }
catch { $wrongSet = $_.Exception.Message }
Check 'a package whose lib/ is not the flavors'' assembly set is refused' ($wrongSet -like '*Other.dll*') "got: $wrongSet"
Remove-Item -Recurse -Force $scratch

Write-Host 'requires are the union of the flavors'' (COORD ruling 2026-10-12, for cobra)'
function Req($module, $version, $id, $pv) { [pscustomobject]@{ Module = $module; Version = $version; NuGetId = $id; PackageVersion = $pv } }
$trap = Req 'github.com/inconshreveable/mousetrap' 'v1.1.0' 'nugetgo.github.com.inconshreveable.mousetrap' '1.1.0'
$pflag = Req 'github.com/spf13/pflag' 'v1.0.9' 'nugetgo.github.com.spf13.pflag' '1.0.9'
$named = @('github.com/inconshreveable/mousetrap=nugetgo.github.com.inconshreveable.mousetrap@1.1.0', 'github.com/spf13/pflag=nugetgo.github.com.spf13.pflag@1.0.9')
$union = @(try { Merge-NugetgoFlavorRequires -Requires (Flavors @($pflag, $trap) @($pflag) @($pflag)) -ThirdPartyPackage $named } catch { "threw: $($_.Exception.Message)" })
Check 'a require only windows has is in the union, once each, sorted by module' (
    (@($union | ForEach-Object { $_.Module }) -join ',') -ceq 'github.com/inconshreveable/mousetrap,github.com/spf13/pflag') "got $(@($union | ForEach-Object { if ($_.Module) { $_.Module } else { $_ } }) -join ', ')"
$same = @(Merge-NugetgoFlavorRequires -Requires (Flavors @($pflag) @($pflag) @($pflag)) -ThirdPartyPackage @($named[1]))
Check 'flavors that agree give their own requires' ((@($same | ForEach-Object { $_.Module }) -join ',') -ceq 'github.com/spf13/pflag') "got $(@($same | ForEach-Object { $_.Module }) -join ', ')"
$why = $null
try { Merge-NugetgoFlavorRequires -Requires (Flavors @($trap) @((Req 'github.com/inconshreveable/mousetrap' 'v1.0.0' 'nugetgo.github.com.inconshreveable.mousetrap' '1.0.0')) @($trap)) -ThirdPartyPackage @($named[0]) | Out-Null } catch { $why = $_.Exception.Message }
Check 'one module at two versions across flavors is refused' ($why -like '*mousetrap*v1.0.0*v1.1.0*' -or $why -like '*mousetrap*v1.1.0*v1.0.0*') "got: $why"
$why = $null
try { Merge-NugetgoFlavorRequires -Requires (Flavors @($pflag) @($pflag) @($pflag)) -ThirdPartyPackage $named | Out-Null } catch { $why = $_.Exception.Message }
Check 'a -ThirdPartyPackage no flavor references is refused' ($why -like '*mousetrap*no flavor*') "got: $why"

Write-Host 'a RID-specific package discloses the flavors its self-description does not describe (COORD ruling 2026-10-12)'
$line = Get-NugetgoFlavorDisclosure -ProofFlavor 'linux/amd64' -RidSpecific
Check 'it names the described flavor and the three others' ("$line" -like '*describes the linux/amd64 flavor*' -and "$line" -like '*windows/amd64, darwin/amd64 and linux/arm64 flavors*' -and "$line" -like '*source-metadata.txt*') "got '$line'"
Check 'it is one line' ("$line" -notmatch "`n") "got '$line'"
Check 'a neutral package carries no such line' ($null -eq (Get-NugetgoFlavorDisclosure -ProofFlavor 'linux/amd64')) 'a neutral package got a line'

Write-Host "ran $ran, failed $failed"
exit $failed
