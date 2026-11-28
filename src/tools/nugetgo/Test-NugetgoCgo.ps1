<#
.SYNOPSIS
    Arms for NugetgoCgo.psm1, the -Cgo pack of a module with a cgo package (the cgo-against-go.* item, INC 2b; COORD's
    ruling (a) and its condition 2 as reworded). Exit code = the number of failed cases.

.DESCRIPTION
    A cgo package's C binding is per target, so a -Cgo pack carries a true flavor for each RID it ships and nothing for
    a RID it does not. Every RID the release ships is packed: osx-arm64 joined with its own darwin/arm64 cgo flavor (zig's
    aarch64-macos target) once the go.* assets for it were darwin/arm64 (D6). Its lib/<tfm>/ is reference
    assemblies only, and its build/<id>.targets fails a consumer build by name for a RID the pack does not carry, read
    from the pack's own runtimes/ folders.

      rids       the -Cgo RID map is the release's whole map, osx-arm64 included, and no RID is excluded
      compilers  each flavor's C compiler is the pinned zig with that flavor's target, and the pin is .github's
      statement  the pack's statement names the packed RIDs and the remedy (-r); with a non-empty exclusion map (a
                 self-test one) the map, the statement and the guard's message all leave that RID out, with its reason
      guard      a consumer of a probe pack: -r <unpacked RID> and a portable build on an unpacked host both fail with
                 NUGETGOCGO001 by name; -r <packed RID> builds; a pack that carries the host's RID builds portably
                 with the SAME guard (it reads the set from the pack). Needs dotnet.
      surface    R7: each RID's assembly against lib/'s reference assembly, strict, both ways: an agreeing control is
                 clean, a RID with an extra or a missing public member is named. Needs dotnet and the apicompat pin.
#>
#Requires -Version 7
[CmdletBinding()]
param([switch]$SkipDotnet)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NugetgoFlavors.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'NugetgoCgo.psm1') -Force
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))

Write-Host 'rids: the release''s whole map, osx-arm64 included'
$all = Get-NugetgoRidFlavors
$cgo = Get-NugetgoCgoRidFlavors
$excluded = Get-NugetgoCgoExcludedRids
Check 'the -Cgo map ships win-x64, linux-x64, osx-x64, linux-arm64 and osx-arm64, in the release''s order' ((@($cgo.Keys) -join ',') -ceq 'win-x64,linux-x64,osx-x64,linux-arm64,osx-arm64') "got $(@($cgo.Keys) -join ',')"
Check 'each packed RID ships the release''s own flavor for it' (@($cgo.Keys | Where-Object { $cgo[$_] -cne $all[$_] }).Count -eq 0) "got $(@($cgo.Keys | ForEach-Object { "$_=$($cgo[$_])" }) -join ',')"
Check 'no RID is excluded (osx-arm64 packs its own darwin/arm64 flavor since the cgo arm)' ($excluded.Count -eq 0) "got $(@($excluded.Keys | ForEach-Object { "$_=$($excluded[$_])" }) -join '; ')"
Check 'packed and excluded together are the release''s RIDs, once each' ((@(@($cgo.Keys) + @($excluded.Keys) | Sort-Object) -join ',') -ceq (@($all.Keys | Sort-Object) -join ',')) "packed $(@($cgo.Keys) -join ',') + excluded $(@($excluded.Keys) -join ',') vs $(@($all.Keys) -join ',')"
Check 'the reference flavor is packed (lib/ is its reference assemblies)' ($cgo.Values -contains (Get-NugetgoReferenceFlavor)) "reference $(Get-NugetgoReferenceFlavor)"

Write-Host 'compilers: the pinned zig, one target per flavor'
$want = [ordered]@{ 'windows/amd64' = 'x86_64-windows-gnu'; 'linux/amd64' = 'x86_64-linux-gnu'; 'darwin/amd64' = 'x86_64-macos'; 'linux/arm64' = 'aarch64-linux-gnu'; 'darwin/arm64' = 'aarch64-macos' }
foreach ($flavor in $want.Keys) {
    $triple = $null; try { $triple = Get-NugetgoCgoTriple -Flavor $flavor } catch { $triple = "threw: $($_.Exception.Message)" }
    Check "$flavor compiles for $($want[$flavor])" ($triple -ceq $want[$flavor]) "got $triple"
}
Check 'every packed flavor has a target' (@(Get-NugetgoFlavors -RidFlavors $cgo | Where-Object { -not $want.Contains($_) }).Count -eq 0) "flavors $((Get-NugetgoFlavors -RidFlavors $cgo) -join ',')"
Check 'the -Cgo flavors are the release''s five, darwin/arm64 its own (not darwin/amd64''s)' ((@(Get-NugetgoFlavors -RidFlavors $cgo) -join ',') -ceq 'windows/amd64,linux/amd64,darwin/amd64,linux/arm64,darwin/arm64') "flavors $((Get-NugetgoFlavors -RidFlavors $cgo) -join ',')"
# nugetgo-pack.ps1 measures the flavors of the map it packs. Until the cgo arm a -Cgo pack's map left osx-arm64 out, so a
# flavor loop over the release's whole map would have converted a flavor with no C target; the maps are equal now, and
# every flavor call in the pack still names the pack's map, so a RID excluded again is excluded everywhere.
$packText = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot 'nugetgo-pack.ps1'))
$flavorCalls = @([regex]::Matches($packText, '(Get-NugetgoFlavors|Get-NugetgoFlavorDecision|Merge-NugetgoFlavorRequires|Get-NugetgoFlavorDisclosure)\b[^\r\n]*') | ForEach-Object { $_.Value })
$unscoped = @($flavorCalls | Where-Object { $_ -notmatch '-RidFlavors \$ridFlavors' })
Check 'nugetgo-pack.ps1 passes its own map to every flavor call' ($flavorCalls.Count -ge 6 -and $unscoped.Count -eq 0) "$($flavorCalls.Count) calls; without -RidFlavors `$ridFlavors: $($unscoped -join ' | ')"
$unknown = $null; try { Get-NugetgoCgoTriple -Flavor 'freebsd/amd64' | Out-Null } catch { $unknown = $_.Exception.Message }
Check 'a flavor with no target is refused by name' ($unknown -and $unknown.Contains('freebsd/amd64')) "got '$unknown'"
Check 'CC is the zig path, cc and -target' ((Get-NugetgoCgoCompiler -Zig '/opt/zig/zig' -Flavor 'linux/arm64') -ceq '/opt/zig/zig cc -target aarch64-linux-gnu') "got $(Get-NugetgoCgoCompiler -Zig '/opt/zig/zig' -Flavor 'linux/arm64')"
Check 'a zig path with a space is quoted, as go splits CC' ((Get-NugetgoCgoCompiler -Zig 'C:/Program Files/zig/zig.exe' -Flavor 'windows/amd64') -ceq '"C:/Program Files/zig/zig.exe" cc -target x86_64-windows-gnu') "got $(Get-NugetgoCgoCompiler -Zig 'C:/Program Files/zig/zig.exe' -Flavor 'windows/amd64')"
$pinText = [System.IO.File]::ReadAllText((Join-Path $repoRoot '.github/cgo-toolchain.ps1'))
$pin = [regex]::Match($pinText, "ziglang\.org/download/(?<v>[0-9.]+)'").Groups['v'].Value
Check 'the pinned zig version is .github/cgo-toolchain.ps1''s' ($pin -and (Get-NugetgoCgoZigVersion) -ceq $pin) "module $(Get-NugetgoCgoZigVersion), cgo-toolchain.ps1 '$pin'"

Write-Host 'statement: what the pack says about its RIDs'
$statement = Get-NugetgoCgoStatement -PackageId 'nugetgo.example.cgo'
foreach ($rid in @($cgo.Keys) + @($excluded.Keys) + @('NUGETGOCGO001', '-r ', 'reference assemblies', 'linux-musl', 'run-time target check')) {
    Check "the statement names $rid" ($statement.Contains($rid)) $statement
}

Check 'with no exclusion the statement says no RID is not packed' (-not $statement.Contains('is not packed')) $statement
# The exclusion paths, read with a self-test map since the module's own is empty (P3's read of the cgo arm, NIT 3). Not
# reached by any suite: nugetgo-pack.ps1's refusal of a -RecurseRoot converted for an excluded RID.
$fake = [ordered]@{ 'osx-arm64' = 'a self-test reason' }
$fakeMap = Get-NugetgoCgoRidFlavors -Excluded $fake
Check 'an excluded RID leaves the -Cgo map, and only it' ((@($fakeMap.Keys) -join ',') -ceq 'win-x64,linux-x64,osx-x64,linux-arm64') "got $(@($fakeMap.Keys) -join ',')"
$fakeStatement = Get-NugetgoCgoStatement -PackageId 'nugetgo.example.cgo' -Excluded $fake
Check 'the statement names the excluded RID with its reason, and packs only the rest' ($fakeStatement.Contains('osx-arm64 is not packed: a self-test reason.') -and $fakeStatement.Contains('for win-x64, linux-x64, osx-x64 and linux-arm64 only')) $fakeStatement
$fakeGuard = New-NugetgoCgoRidGuard -PackageId 'nugetgo.example.cgo' -Excluded $fake
Check 'the guard''s message names the excluded RID with its reason' ($fakeGuard.Contains('osx-arm64 is not packed: a self-test reason.')) $fakeGuard
Check 'and with no exclusion it names none' (-not (New-NugetgoCgoRidGuard -PackageId 'nugetgo.example.cgo').Contains('is not packed')) 'the guard names an exclusion'

Write-Host 'A6''s floor: the push lift (COORD''s ruling (a))'
$treeTargets = [System.IO.File]::ReadAllText((Join-Path $repoRoot 'src/core/golib/buildTransitive/go.lib.targets'))
Check 'this tree''s go.lib.targets declares GoCgoRunTimeTargetCheck' ($null -eq (Test-NugetgoCgoGoLibFloor -GoLibTargets $treeTargets -Version 'tree')) (Test-NugetgoCgoGoLibFloor -GoLibTargets $treeTargets -Version 'tree')
$golibSource = [System.IO.File]::ReadAllText((Join-Path $repoRoot 'src/core/golib/runtime/GoCgoLibraries.cs'))
Check 'and its golib''s Register refuses at registration (the property is never declared without the check)' ($golibSource -match '(?s)public static void Register\(Assembly assembly, string target, params GoCgoLibrary\[\] libraries\)\s*\{.*?if \(Refusal\(target\) is \{ \} refusal\)\s*throw new GoCgoTargetMismatchException') 'Register no longer begins with the A6 refusal'
$published = "$(& git -C $repoRoot show 'nuget-1.26.11.1:src/core/golib/buildTransitive/go.lib.targets' 2>$null)"
if ($published) {
    $why = Test-NugetgoCgoGoLibFloor -GoLibTargets $published -Version '1.26.11.1'
    Check 'the published go.lib 1.26.11.1, which predates A6, is refused by name (the natural control)' ($why -and $why.Contains('1.26.11.1') -and $why.Contains('GoCgoRunTimeTargetCheck')) "got '$why'"
}
else { Write-Host '  NOT MEASURED: the nuget-1.26.11.1 tag is not in this clone' }
$commented = $treeTargets -replace '(<GoCgoRunTimeTargetCheck>true</GoCgoRunTimeTargetCheck>)', '<!-- $1 -->'
Check 'a go.lib.targets whose property is commented out is refused (it is parsed, not matched as text)' ($commented -ne $treeTargets -and $null -ne (Test-NugetgoCgoGoLibFloor -GoLibTargets $commented -Version 'commented')) 'accepted, or the property was not found to comment out'
Check 'an empty go.lib.targets (none restored) is refused' ($null -ne (Test-NugetgoCgoGoLibFloor -GoLibTargets '' -Version 'none')) 'accepted'
Check 'the a6 note starts with the "a6: " the push reads' ((Get-NugetgoCgoA6Note -Version '1.26.12.1').StartsWith('a6: ', [StringComparison]::Ordinal)) (Get-NugetgoCgoA6Note -Version '1.26.12.1')

if ($SkipDotnet) { Write-Host "SKIPPED guard and surface (-SkipDotnet): NOT MEASURED"; Write-Host "ran $ran, failed $failed"; exit $failed }

$work = Join-Path ([System.IO.Path]::GetTempPath()) ("nugetgo-cgo-" + [Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Force $work | Out-Null
# The work directory by its REAL path. macOS's temp directory is /var/folders/..., a symlink to /private/var/folders/...,
# and there a consumer's restore read its project reference under the other spelling and left the referenced project
# out of the consumer's package graph (hosted converter-cgo, both mac legs, 2026-11-21: App's assets listed no Lib and
# no package, so the transitive arm measured nothing; linux, with no symlink, read it green).
if (-not $IsWindows) {
    $real = "$(& realpath $work 2>$null)".Trim()
    if ($LASTEXITCODE -eq 0 -and $real) { $work = $real }
}
$savedNp = $env:NUGET_PACKAGES
$env:NUGET_PACKAGES = Join-Path $work 'gpf'
try {
    # Two classlibs from one source: Same.dll, and Wider.dll with one more public member. Each gives an implementation
    # and its reference assembly.
    function New-Lib([string]$name, [string]$source) {
        $dir = Join-Path $work "src-$name"
        New-Item -ItemType Directory -Force $dir | Out-Null
        Set-Content -LiteralPath (Join-Path $dir 'Probe.csproj') -Value '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><AssemblyName>Probe</AssemblyName><Deterministic>true</Deterministic></PropertyGroup></Project>'
        Set-Content -LiteralPath (Join-Path $dir 'Probe.cs') -Value $source
        $log = & dotnet build (Join-Path $dir 'Probe.csproj') -c Release --nologo -v q 2>&1
        if ($LASTEXITCODE -ne 0) { throw "the $name probe did not build: $($log -join ' | ')" }
        [pscustomobject]@{ Impl = Join-Path $dir 'bin/Release/net10.0/Probe.dll'; Ref = Join-Path $dir 'obj/Release/net10.0/ref/Probe.dll' }
    }
    $same = New-Lib 'same' 'namespace Probe; public static class P { public static string Where() => "impl"; }'
    $wider = New-Lib 'wider' 'namespace Probe; public static class P { public static string Where() => "impl"; public static int Extra() => 1; }'
    $narrower = New-Lib 'narrower' 'namespace Probe; public static class P { public static string Other() => "impl"; }'

    Write-Host 'layout: lib/ is reference assemblies, runtimes/ the packed RIDs only'
    Check 'a reference assembly reads as one' (Test-NugetgoReferenceAssembly ([System.IO.File]::ReadAllBytes($same.Ref))) $same.Ref
    Check 'an implementation does not' (-not (Test-NugetgoReferenceAssembly ([System.IO.File]::ReadAllBytes($same.Impl)))) $same.Impl
    $layoutPack = Join-Path $work 'layout.nupkg'
    $zip = [System.IO.Compression.ZipFile]::Open($layoutPack, 'Create')
    try { [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $same.Impl, 'lib/net10.0/Probe.dll') } finally { $zip.Dispose() }
    $implBytes = [System.IO.File]::ReadAllBytes($same.Impl)
    $flavorSets = @{}
    foreach ($flavor in Get-NugetgoFlavors) { $flavorSets[$flavor] = @{ 'Probe.dll' = $implBytes } }
    $libSet = @{ 'Probe.dll' = [System.IO.File]::ReadAllBytes($same.Ref) }
    Set-NugetgoRidAssets -Package $layoutPack -Assemblies $flavorSets -TargetFramework 'net10.0' -RidFlavors $cgo -LibAssemblies $libSet
    $layout = @(Test-NugetgoRidLayout -Package $layoutPack -Assemblies $flavorSets -TargetFramework 'net10.0' -RidFlavors $cgo -LibAssemblies $libSet)
    Check 'the -Cgo layout reads back clean' ($layout.Count -eq 0) "reasons: $($layout -join ' | ')"
    $zip = [System.IO.Compression.ZipFile]::OpenRead($layoutPack)
    try { $names = @($zip.Entries | ForEach-Object { $_.FullName }); $libEntry = $zip.GetEntry('lib/net10.0/Probe.dll'); $ms = New-Object System.IO.MemoryStream; $st = $libEntry.Open(); try { $st.CopyTo($ms) } finally { $st.Dispose() } }
    finally { $zip.Dispose() }
    Check 'runtimes/osx-arm64 carries its own entry' (@($names | Where-Object { $_ -ceq 'runtimes/osx-arm64/lib/net10.0/Probe.dll' }).Count -eq 1) "entries: $($names -join ', ')"
    Check 'lib/ is the reference assembly' (Test-NugetgoReferenceAssembly $ms.ToArray()) 'lib/net10.0/Probe.dll is an implementation'
    $default = @(Test-NugetgoRidLayout -Package $layoutPack -Assemblies $flavorSets -TargetFramework 'net10.0')
    Check 'the release''s own layout check refuses it (control: lib/ is not the implementation)' (@($default | Where-Object { $_ -like 'lib/*' }).Count -ge 1) "reasons: $($default -join ' | ')"
    # The red the cgo arm turns on: a -Cgo pack that leaves runtimes/osx-arm64 out (the layout before it) is refused by
    # the -Cgo map's own check, naming osx-arm64.
    $noArm = [ordered]@{}; foreach ($rid in $cgo.Keys) { if ($rid -cne 'osx-arm64') { $noArm[$rid] = $cgo[$rid] } }
    $noArmPack = Join-Path $work 'layout-no-osx-arm64.nupkg'
    $zip = [System.IO.Compression.ZipFile]::Open($noArmPack, 'Create')
    try { [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $same.Impl, 'lib/net10.0/Probe.dll') } finally { $zip.Dispose() }
    Set-NugetgoRidAssets -Package $noArmPack -Assemblies $flavorSets -TargetFramework 'net10.0' -RidFlavors $noArm -LibAssemblies $libSet
    $noArmWhy = @(Test-NugetgoRidLayout -Package $noArmPack -Assemblies $flavorSets -TargetFramework 'net10.0' -RidFlavors $cgo -LibAssemblies $libSet)
    Check 'a -Cgo layout without runtimes/osx-arm64 is refused, naming it, and nothing else' ($noArmWhy.Count -ge 1 -and @($noArmWhy | Where-Object { $_ -notlike 'runtimes/osx-arm64/*' }).Count -eq 0) "reasons: $($noArmWhy -join ' | ')"

    Write-Host 'guard: a consumer build of a pack outside its RIDs fails by name'
    # The probe pack never carries this host's RID, so every host reads condition 2's arms with the host standing in for
    # an unpacked RID (until the cgo arm, osx-arm64 read them as the RID a -Cgo pack left out by name). The second pack
    # adds the host's RID with the same guard: the RID joining later.
    $hostRid = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier
    Write-Host "  host $hostRid$(if ($excluded.Contains($hostRid)) { ', a RID a -Cgo pack leaves out by name' } else { ', standing in for an unpacked RID' })"
    $feed = Join-Path $work 'feed'
    New-Item -ItemType Directory -Force $feed | Out-Null
    function New-ProbePack([string]$version, [string[]]$rids, [switch]$BuildOnly) {
        $nupkg = Join-Path $feed "nugetgo.cgo.probe.$version.nupkg"
        $zip = [System.IO.Compression.ZipFile]::Open($nupkg, 'Create')
        try {
            $w = New-Object System.IO.StreamWriter($zip.CreateEntry('nugetgo.cgo.probe.nuspec').Open())
            try { $w.Write("<?xml version=`"1.0`"?><package><metadata><id>nugetgo.cgo.probe</id><version>$version</version><authors>selftest</authors><description>selftest</description></metadata></package>") } finally { $w.Dispose() }
            [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $same.Ref, 'lib/net10.0/Probe.dll')
            foreach ($rid in $rids) { [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $same.Impl, "runtimes/$rid/lib/net10.0/Probe.dll") }
            # The guard at build/ and buildTransitive/, as a -Cgo pack carries it, beside a build/<id>.props as every
            # nugetgo pack carries its usings: a direct consumer must still import that props (Program.cs needs its constant).
            foreach ($path in @('build/nugetgo.cgo.probe.targets') + @(if (-not $BuildOnly) { 'buildTransitive/nugetgo.cgo.probe.targets' })) {
                $w = New-Object System.IO.StreamWriter($zip.CreateEntry($path).Open())
                try { $w.Write((New-NugetgoCgoRidGuard -PackageId 'nugetgo.cgo.probe')) } finally { $w.Dispose() }
            }
            $w = New-Object System.IO.StreamWriter($zip.CreateEntry('build/nugetgo.cgo.probe.props').Open())
            try { $w.Write('<Project><PropertyGroup><DefineConstants>$(DefineConstants);NUGETGO_PROBE_PROPS</DefineConstants></PropertyGroup></Project>') } finally { $w.Dispose() }
        }
        finally { $zip.Dispose() }
    }
    $others = @($cgo.Keys | Where-Object { $_ -cne $hostRid })
    New-ProbePack '1.0.0' $others
    New-ProbePack '1.0.1' (@($others) + $hostRid)
    function Invoke-Consumer([string]$name, [string]$version, [string[]]$buildArgs) {
        $dir = Join-Path $work "consumer-$name"
        New-Item -ItemType Directory -Force $dir | Out-Null
        Set-Content -LiteralPath (Join-Path $dir 'nuget.config') -Value "<?xml version=`"1.0`" encoding=`"utf-8`"?><configuration><packageSources><clear /><add key=`"probe`" value=`"$([System.Security.SecurityElement]::Escape($feed))`" /><add key=`"nuget`" value=`"https://api.nuget.org/v3/index.json`" /></packageSources></configuration>"
        Set-Content -LiteralPath (Join-Path $dir 'App.csproj') -Value "<Project Sdk=`"Microsoft.NET.Sdk`"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup><ItemGroup><PackageReference Include=`"nugetgo.cgo.probe`" Version=`"$version`" /></ItemGroup></Project>"
        Set-Content -LiteralPath (Join-Path $dir 'Program.cs') -Value "#if !NUGETGO_PROBE_PROPS`n#error the package's build/ props was not imported`n#endif`nSystem.Console.WriteLine(Probe.P.Where());"
        $out = @(& dotnet build (Join-Path $dir 'App.csproj') -c Release --nologo -o (Join-Path $dir 'out') @buildArgs 2>&1 | ForEach-Object { "$_" })
        [pscustomobject]@{ Exit = $LASTEXITCODE; Text = ($out -join "`n"); Out = (Join-Path $dir 'out') }
    }
    $named = Invoke-Consumer 'rid' '1.0.0' @('-r', $hostRid)
    Check "-r $hostRid against a pack without it fails with NUGETGOCGO001 naming $hostRid" ($named.Exit -ne 0 -and $named.Text -match "NUGETGOCGO001.*$([regex]::Escape($hostRid))") "exit $($named.Exit): $(@($named.Text -split "`n" | Select-String 'error' | Select-Object -First 2) -join ' | ')"
    Check 'the refusal lists the RIDs the pack carries' ($named.Text -match [regex]::Escape($others[0])) "want $($others[0]) in: $(@($named.Text -split "`n" | Select-String 'NUGETGOCGO001' | Select-Object -First 1))"
    $portable = Invoke-Consumer 'portable' '1.0.0' @()
    Check "a portable build on a $hostRid host against a pack without it fails with NUGETGOCGO001" ($portable.Exit -ne 0 -and $portable.Text -match 'NUGETGOCGO001') "exit $($portable.Exit)"
    $packed = Invoke-Consumer 'packed' '1.0.0' @('-r', $others[0])
    Check "-r $($others[0]), a packed RID, builds" ($packed.Exit -eq 0) "exit $($packed.Exit): $(@($packed.Text -split "`n" | Select-String 'error' | Select-Object -First 2) -join ' | ')"
    # The backstop: a portable build made on a host the pack carries (its SDK RID set to $($others[0]), no apphost),
    # deployed here. The guard passes it there; here the loader finds no runtimes/ asset for this RID, takes lib/'s
    # reference assembly, and refuses to load it -- never a bind of another target's code.
    $deployed = Invoke-Consumer 'deployed' '1.0.0' @("-p:NETCoreSdkRuntimeIdentifier=$($others[0])", '-p:UseAppHost=false')
    Check "a portable build made as on $($others[0]) builds (the guard passes it there)" ($deployed.Exit -eq 0) "exit $($deployed.Exit): $(@($deployed.Text -split "`n" | Select-String 'error' | Select-Object -First 2) -join ' | ')"
    if ($deployed.Exit -eq 0) {
        $runText = @(& dotnet (Join-Path $deployed.Out 'App.dll') 2>&1 | ForEach-Object { "$_" }); $runExit = $LASTEXITCODE
        Check "run on $hostRid, it fails at load on the reference assembly, never printing the implementation's answer" ($runExit -ne 0 -and ($runText -join ' ') -match 'BadImageFormatException' -and ($runText -join ' ') -match 'Reference assemblies' -and ($runText -join ' ') -notmatch '\bimpl\b') "exit ${runExit}: $(@($runText | Select-Object -First 2) -join ' | ')"
    }
    # Through a project reference (P3's INC 2b read, SHOULD 2): App -> Lib -> the pack. Lib is built for a packed RID of its
    # own (its project's RuntimeIdentifier), so its own guard passes, and App names the unpacked RID in its project: only
    # buildTransitive/ reaches App, which references the package transitively. The control is a pack with the guard at
    # build/ only, through which App builds.
    New-ProbePack '1.0.2' $others -BuildOnly
    function Invoke-Transitive([string]$name, [string]$version) {
        $tdir = Join-Path $work "consumer-transitive-$name"
        New-Item -ItemType Directory -Force (Join-Path $tdir 'Lib'), (Join-Path $tdir 'App') | Out-Null
        Set-Content -LiteralPath (Join-Path $tdir 'nuget.config') -Value "<?xml version=`"1.0`" encoding=`"utf-8`"?><configuration><packageSources><clear /><add key=`"probe`" value=`"$([System.Security.SecurityElement]::Escape($feed))`" /><add key=`"nuget`" value=`"https://api.nuget.org/v3/index.json`" /></packageSources></configuration>"
        Set-Content -LiteralPath (Join-Path $tdir 'Lib/Lib.csproj') -Value "<Project Sdk=`"Microsoft.NET.Sdk`"><PropertyGroup><TargetFramework>net10.0</TargetFramework><RuntimeIdentifier>$($others[0])</RuntimeIdentifier></PropertyGroup><ItemGroup><PackageReference Include=`"nugetgo.cgo.probe`" Version=`"$version`" /></ItemGroup></Project>"
        Set-Content -LiteralPath (Join-Path $tdir 'Lib/Lib.cs') -Value 'namespace Lib; public static class L { public static string Where() => Probe.P.Where(); }'
        Set-Content -LiteralPath (Join-Path $tdir 'App/App.csproj') -Value "<Project Sdk=`"Microsoft.NET.Sdk`"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><RuntimeIdentifier>$hostRid</RuntimeIdentifier><UseAppHost>false</UseAppHost></PropertyGroup><ItemGroup><ProjectReference Include=`"../Lib/Lib.csproj`" /></ItemGroup></Project>"
        Set-Content -LiteralPath (Join-Path $tdir 'App/Program.cs') -Value 'System.Console.WriteLine(Lib.L.Where());'
        $tout = @(& dotnet build (Join-Path $tdir 'App/App.csproj') -c Release --nologo 2>&1 | ForEach-Object { "$_" })
        [pscustomobject]@{ Exit = $LASTEXITCODE; Text = $tout; App = @($tout | Where-Object { $_ -match 'NUGETGOCGO001' -and $_ -match '[\\/]App\.csproj\]' }).Count; Lib = @($tout | Where-Object { $_ -match 'NUGETGOCGO001' -and $_ -match '[\\/]Lib\.csproj\]' }).Count }
    }
    $both = Invoke-Transitive 'both' '1.0.0'
    Check "App -> Lib ($($others[0])) -> the pack, App for ${hostRid}: App's own build fails with NUGETGOCGO001 (buildTransitive/), Lib's passes" ($both.Exit -ne 0 -and $both.App -ge 1 -and $both.Lib -eq 0) "exit $($both.Exit), App errors $($both.App), Lib errors $($both.Lib): $(@($both.Text | Where-Object { $_ -match 'error' } | Select-Object -First 2) -join ' | ')"
    $buildOnly = Invoke-Transitive 'buildonly' '1.0.2'
    Check 'control: with the guard at build/ only, the same App builds (the gap buildTransitive/ closes)' ($buildOnly.Exit -eq 0 -and $buildOnly.App -eq 0) "exit $($buildOnly.Exit): $(@($buildOnly.Text | Where-Object { $_ -match 'error' } | Select-Object -First 2) -join ' | ')"
    $musl = Invoke-Consumer 'musl' '1.0.1' @('-r', 'linux-musl-x64')
    Check '-r linux-musl-x64 fails with NUGETGOCGO001 (the pack has no runtimes/linux-musl-x64; only a portable build made elsewhere reaches musl, A6''s)' ($musl.Exit -ne 0 -and $musl.Text -match 'NUGETGOCGO001.*linux-musl-x64') "exit $($musl.Exit): $(@($musl.Text -split "`n" | Select-String 'error' | Select-Object -First 2) -join ' | ')"
    $joined = Invoke-Consumer 'joined' '1.0.1' @()
    Check "the same guard, in a pack that carries $hostRid, builds portably (the set is the pack's)" ($joined.Exit -eq 0) "exit $($joined.Exit): $(@($joined.Text -split "`n" | Select-String 'error' | Select-Object -First 2) -join ' | ')"

    Write-Host 'surface: R7, each RID against lib/''s reference assembly'
    $agree = Test-NugetgoCgoSurface -RepoRoot $repoRoot -WorkDir (Join-Path $work 'r7-agree') -Reference @{ 'Probe.dll' = $same.Ref } `
        -Runtimes ([ordered]@{ 'win-x64' = @{ 'Probe.dll' = $same.Impl }; 'linux-x64' = @{ 'Probe.dll' = $same.Impl } })
    Check 'an agreeing control is clean, and every comparison ran' ($agree.Reasons.Count -eq 0 -and $agree.Compared -eq 2) "compared $($agree.Compared): $($agree.Reasons -join ' | ')"
    $extra = Test-NugetgoCgoSurface -RepoRoot $repoRoot -WorkDir (Join-Path $work 'r7-extra') -Reference @{ 'Probe.dll' = $same.Ref } `
        -Runtimes ([ordered]@{ 'win-x64' = @{ 'Probe.dll' = $same.Impl }; 'linux-x64' = @{ 'Probe.dll' = $wider.Impl } })
    Check 'a RID with an extra public member is named, and only it' ($extra.Reasons.Count -ge 1 -and @($extra.Reasons | Where-Object { $_ -notmatch 'linux-x64' }).Count -eq 0 -and ($extra.Reasons -join ' ') -match 'Extra') "reasons: $($extra.Reasons -join ' | ')"
    $missing = Test-NugetgoCgoSurface -RepoRoot $repoRoot -WorkDir (Join-Path $work 'r7-missing') -Reference @{ 'Probe.dll' = $same.Ref } `
        -Runtimes ([ordered]@{ 'win-x64' = @{ 'Probe.dll' = $narrower.Impl }; 'linux-x64' = @{ 'Probe.dll' = $same.Impl } })
    Check 'a RID missing a public member is named, and only it' ($missing.Reasons.Count -ge 1 -and @($missing.Reasons | Where-Object { $_ -notmatch 'win-x64' }).Count -eq 0 -and ($missing.Reasons -join ' ') -match 'Where') "reasons: $($missing.Reasons -join ' | ')"
    $absent = Test-NugetgoCgoSurface -RepoRoot $repoRoot -WorkDir (Join-Path $work 'r7-absent') -Reference @{ 'Probe.dll' = $same.Ref } `
        -Runtimes ([ordered]@{ 'win-x64' = @{} })
    Check 'a RID without the assembly is named' (($absent.Reasons -join ' ') -match 'win-x64.*Probe\.dll') "reasons: $($absent.Reasons -join ' | ')"
}
finally {
    $env:NUGET_PACKAGES = $savedNp
    Remove-Item -Recurse -Force -LiteralPath $work -ErrorAction SilentlyContinue
}
Write-Host "ran $ran, failed $failed"
exit $failed
