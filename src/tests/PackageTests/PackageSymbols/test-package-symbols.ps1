#Requires -Version 7
<#
.SYNOPSIS
    Gate for the go.* packages' symbol files: a NuGet consumer's STD frames resolve to their Go file:line.

.DESCRIPTION
    Restores the PackageSymbols fixture from ONE feed into a FRESH package cache. The program prints the first go.sort
    frame above a sort.Slice comparator as the converted runtime resolves it (runtime.Caller), which reads the frame's
    file:line from go.sort's .pdb beside the application: "slice.go:<n>" or "zsortfunc.go:<n>" with the symbols, "none"
    without. Arms:

      RUN        `dotnet run`: the frame resolves, and every go.* assembly in the build output has its .pdb beside it with
                 the matching debug id (the RID-specific packages' assemblies included).
      PUBLISH    a single-file, self-contained publish for the host's RID into ONE folder, twice with nothing changed
                 between: the frame resolves after both (the second leans on go.lib.symbols.targets).
      FDD        a framework-dependent publish that is not single-file: the frame resolves, the .pdb match as in RUN.
      OFF        `dotnet run` with -p:GoCopyPackageSymbols=false: today's output, the frame "none" and no go.* .pdb
                 beside the application -- the off switch, and the proof the arms above can fail.
      AOT        (-Aot only; needs the ILCompiler packages) a Native AOT publish succeeds; its .pdb list is printed.

    Exit 0 only when every arm reads as expected. Against a feed whose packages ship no .pdb (the published 1.24.13.4
    shape) RUN, PUBLISH and FDD fail: that is the gate's red. Nothing outside a temporary work directory is written.

.PARAMETER Version
    The go.* package version to restore (the fixture's GoPackageVersion).

.PARAMETER Source
    The feed: a local folder of .nupkg files (a pack rehearsal's output) or a URL. Default nuget.org.

.PARAMETER FallbackFolder
    Optional read-only package folder for the SDK's own packs (the self-contained runtime pack), so a box with no
    network can publish. The go.* packages must still come from -Source: the gate refuses one resolved anywhere else.

.EXAMPLE
    pwsh ./test-package-symbols.ps1 -Version 1.24.13.5 -Source <feed> -FallbackFolder <package folder>
#>
param(
    [Parameter(Mandatory)] [string] $Version,
    [string] $Source = 'https://api.nuget.org/v3/index.json',
    [string] $FallbackFolder,
    [switch] $Aot,
    [switch] $TrimReadings,
    [switch] $KeepWork
)

$ErrorActionPreference = 'Stop'

$fixture = $PSScriptRoot
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("package-symbols-" + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $work | Out-Null
Copy-Item (Join-Path $fixture 'PackageSymbols.csproj'), (Join-Path $fixture 'Program.cs') $work

if (Test-Path $Source) { $Source = (Resolve-Path $Source).Path }

$fallback = ''
if ($FallbackFolder) {
    $fallback = @"
  <fallbackPackageFolders>
    <add key="sdk-packs" value="$((Resolve-Path $FallbackFolder).Path)" />
  </fallbackPackageFolders>
"@
}

# go.* from the source under test ONLY, and a cache nothing else has touched: a green read must come from the packages
# under test. The self-contained and Native AOT publishes also need the SDK's runtime and ILCompiler packs, which a pack
# rehearsal's local feed does not carry, so everything else maps to nuget.org (release-smoke's arm F runs this against
# such a feed). When the source IS nuget.org it is listed once: NuGet drops a URL listed under two keys.
$nugetOrg = 'https://api.nuget.org/v3/index.json'
$sources = if ($Source.TrimEnd('/') -eq $nugetOrg) { "    <add key=`"under-test`" value=`"$nugetOrg`" />" }
           else { "    <add key=`"under-test`" value=`"$Source`" />`n    <add key=`"nuget.org`" value=`"$nugetOrg`" />" }
$mapping = if ($Source.TrimEnd('/') -eq $nugetOrg) { '    <packageSource key="under-test"><package pattern="*" /></packageSource>' }
           else { "    <packageSource key=`"under-test`"><package pattern=`"go.*`" /></packageSource>`n    <packageSource key=`"nuget.org`"><package pattern=`"*`" /></packageSource>" }
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
$sources
  </packageSources>
  <packageSourceMapping>
$mapping
  </packageSourceMapping>
$fallback
</configuration>
"@ | Set-Content -Path (Join-Path $work 'nuget.config') -Encoding utf8

$env:NUGET_PACKAGES = Join-Path $work 'packages'
$env:NUGET_HTTP_CACHE_PATH = Join-Path $work 'http-cache'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'

$rid = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier
$exe = if ($IsWindows) { 'PackageSymbols.exe' } else { 'PackageSymbols' }
$project = Join-Path $work 'PackageSymbols.csproj'
$common = @("-p:GoPackageVersion=$Version", '-nologo', '-v:m')
$frame = '^PACKAGE-SYMBOLS: (slice|zsortfunc)\.go:[1-9][0-9]*$'

function Clear-Build { Remove-Item -Recurse -Force (Join-Path $work 'bin'), (Join-Path $work 'obj') -ErrorAction SilentlyContinue }

function Get-FrameLine([string[]] $Output) {
    return "$(@($Output | ForEach-Object { "$_" } | Where-Object { $_ -like 'PACKAGE-SYMBOLS:*' }) | Select-Object -First 1)"
}

# Every assembly beside the application other than its own, with its .pdb's verdict: 'match' when the .pdb's id equals
# the assembly's CodeView record, else 'missing' or 'mismatch'.
function Get-SymbolPairs([string] $Folder) {
    foreach ($dll in @(Get-ChildItem $Folder -Filter '*.dll' -File | Where-Object { $_.BaseName -ne 'PackageSymbols' })) {
        $pdb = [System.IO.Path]::ChangeExtension($dll.FullName, '.pdb')
        $verdict = 'missing'

        if (Test-Path $pdb) {
            $pe = [System.Reflection.PortableExecutable.PEReader]::new([System.IO.File]::OpenRead($dll.FullName))
            try {
                $codeView = @($pe.ReadDebugDirectory() | Where-Object { $_.Type -eq 'CodeView' }) | Select-Object -First 1
                $guid = if ($codeView) { $pe.ReadCodeViewDebugDirectoryData($codeView).Guid } else { [guid]::Empty }
            }
            finally { $pe.Dispose() }

            $provider = [System.Reflection.Metadata.MetadataReaderProvider]::FromPortablePdbStream([System.IO.File]::OpenRead($pdb))
            try { $id = [System.Reflection.Metadata.BlobContentId]::new($provider.GetMetadataReader().DebugMetadataHeader.Id) }
            finally { $provider.Dispose() }

            $verdict = if ($id.Guid -eq $guid) { 'match' } else { 'mismatch' }
        }

        [pscustomobject]@{ Name = $dll.Name; Verdict = $verdict }
    }
}

function Read-Pairs([string] $Name, [string] $Folder) {
    $pairs = @(Get-SymbolPairs $Folder)
    $matched = @($pairs | Where-Object Verdict -eq 'match').Count
    $bad = @($pairs | Where-Object Verdict -ne 'match')
    Write-Host "    ${Name}: $($pairs.Count) dependency assemblies, $matched with a matching .pdb$(if ($bad.Count) { "; not: $(($bad | Select-Object -First 5 | ForEach-Object { "$($_.Name) ($($_.Verdict))" }) -join ', ')" })"
    return [pscustomobject]@{ Count = $pairs.Count; Matched = $matched }
}

function Invoke-Dotnet([string] $Name, [string[]] $Arguments) {
    $log = & dotnet @Arguments 2>&1 | ForEach-Object { "$_" }
    if ($LASTEXITCODE -ne 0) {
        $log | Select-String ' error ' | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" }
        throw "FAIL ($Name): dotnet $($Arguments[0]) did not succeed"
    }
    return $log
}

$verdicts = @()

function Add-Verdict([string] $Name, [scriptblock] $Arm) {
    try { $script:verdicts += (& $Arm) }
    catch { $script:verdicts += "$_" }
}

Write-Host "Package symbols gate: go.* $Version from $Source ($rid)"

Add-Verdict 'RUN' {
    Clear-Build
    $line = Get-FrameLine (Invoke-Dotnet 'RUN' (@('run', '--project', $project, '-c', 'Release') + $common))
    Write-Host "    RUN: $line"
    if (-not (Test-Path (Join-Path $work 'packages/go.sort'))) { return "FAIL (RUN): go.sort did not resolve from the feed under test" }
    $pairs = Read-Pairs 'RUN' (Join-Path $work 'bin/Release/net10.0')
    if ($line -notmatch $frame) { return "FAIL (RUN): the go.sort frame has no file:line ($line)" }
    if ($pairs.Count -eq 0 -or $pairs.Matched -ne $pairs.Count) { return "FAIL (RUN): $($pairs.Count - $pairs.Matched) of $($pairs.Count) dependency assemblies lack a matching .pdb" }
    return 'PASS (RUN)'
}

Add-Verdict 'PUBLISH' {
    Clear-Build
    $out = Join-Path $work 'out-single'
    $lines = foreach ($pass in 1, 2) {
        [void](Invoke-Dotnet "PUBLISH $pass" (@('publish', $project, '-c', 'Release', '-r', $rid, '-p:PublishSingleFile=true', '--self-contained', '-o', $out) + $common))
        $line = Get-FrameLine (& (Join-Path $out $exe) 2>&1)
        Write-Host "    PUBLISH ${pass}: $(@(Get-ChildItem $out -Filter '*.pdb' -File).Count) .pdb beside the host :: $line"
        $line
    }
    if (@($lines | Where-Object { $_ -notmatch $frame }).Count) { return "FAIL (PUBLISH): a single-file publish printed no file:line ($($lines -join ' / '))" }
    return 'PASS (PUBLISH, twice)'
}

Add-Verdict 'FDD' {
    Clear-Build
    $out = Join-Path $work 'out-fdd'
    [void](Invoke-Dotnet 'FDD' (@('publish', $project, '-c', 'Release', '-r', $rid, '--self-contained', 'false', '-o', $out) + $common))
    $line = Get-FrameLine (& (Join-Path $out $exe) 2>&1)
    Write-Host "    FDD: $line"
    $pairs = Read-Pairs 'FDD' $out
    if ($line -notmatch $frame) { return "FAIL (FDD): the go.sort frame has no file:line ($line)" }
    if ($pairs.Count -eq 0 -or $pairs.Matched -ne $pairs.Count) { return "FAIL (FDD): $($pairs.Count - $pairs.Matched) of $($pairs.Count) dependency assemblies lack a matching .pdb" }
    return 'PASS (FDD)'
}

Add-Verdict 'OFF' {
    Clear-Build
    $line = Get-FrameLine (Invoke-Dotnet 'OFF' (@('run', '--project', $project, '-c', 'Release', '-p:GoCopyPackageSymbols=false') + $common))
    $pdbs = @(Get-ChildItem (Join-Path $work 'bin/Release/net10.0') -Filter '*.pdb' -File | Where-Object { $_.BaseName -ne 'PackageSymbols' })
    Write-Host "    OFF: $line :: $($pdbs.Count) dependency .pdb beside the application"
    if ($line -ne 'PACKAGE-SYMBOLS: none' -or $pdbs.Count -ne 0) { return "FAIL (OFF): with the switch off the output still carries package symbols" }
    return 'PASS (OFF)'
}

if ($Aot) {
    Add-Verdict 'AOT' {
        Clear-Build
        $out = Join-Path $work 'out-aot'
        [void](Invoke-Dotnet 'AOT' (@('publish', $project, '-c', 'Release', '-r', $rid, '-p:PublishAot=true', '-o', $out) + $common))
        $run = @(& (Join-Path $out $exe) 2>&1 | ForEach-Object { "$_" })
        $code = $LASTEXITCODE
        $line = Get-FrameLine $run
        Write-Host "    AOT: exit $code :: $line :: .pdb in the output: $((@(Get-ChildItem $out -Filter '*.pdb' -File | ForEach-Object Name) | Sort-Object) -join ', ')"
        # The executable must RUN and print its frame line. A file:line is not required: what Native AOT resolves for a
        # frame is printed, not judged. Before go.lib set TrimMode=partial, a consumer's AOT executable died at type
        # initialization (exit 2) and printed no line at all.
        if ($code -ne 0 -or $line -notlike 'PACKAGE-SYMBOLS:*') {
            $why = @($run | Where-Object { $_ -match 'Exception' } | Select-Object -First 2) -join ' / '
            return "FAIL (AOT): the Native AOT executable did not run to its frame line (exit $code$(if ($why) { "; $why" }))"
        }
        return "PASS (AOT): $line"
    }
}

# PROBE-ONLY READINGS (the trim-default seat's one-off measurements; never PASS or FAIL, only READ).
if ($TrimReadings) {
    Add-Verdict 'TRIMMODE' {
        Clear-Build
        [void](Invoke-Dotnet 'TRIMMODE restore' (@('restore', $project, '-r', $rid) + $common))
        foreach ($arm in @(@{ L = 'aot'; A = @('-p:PublishAot=true') }, @{ L = 'aot+full'; A = @('-p:PublishAot=true', '-p:TrimMode=full') },
                           @{ L = 'trimmed'; A = @('-p:PublishTrimmed=true') })) {
            $value = (& dotnet msbuild $project "-p:RuntimeIdentifier=$rid" @($arm.A) "-p:GoPackageVersion=$Version" -getProperty:TrimMode -nologo 2>&1 | Select-Object -Last 1)
            Write-Host "    TRIMMODE: $($arm.L) evaluates TrimMode='$value'"
        }
        return 'READ (TRIMMODE)'
    }
    foreach ($reading in @(@{ L = 'TRIM-FULL'; A = @('-p:PublishAot=true', '-p:TrimMode=full') },
                           @{ L = 'TRIMMED'; A = @('-p:PublishTrimmed=true', '--self-contained') })) {
        Add-Verdict $reading.L {
            Clear-Build
            $out = Join-Path $work ('out-' + $reading.L.ToLowerInvariant())
            [void](Invoke-Dotnet $reading.L (@('publish', $project, '-c', 'Release', '-r', $rid, '-o', $out) + $reading.A + $common))
            $run = @(& (Join-Path $out $exe) 2>&1 | ForEach-Object { "$_" })
            $code = $LASTEXITCODE
            $line = Get-FrameLine $run
            $why = @($run | Where-Object { $_ -match 'InvalidOperationException|TypeInitializationException' } | Select-Object -First 1) -join ''
            Write-Host "    $($reading.L): exit $code :: $(if ($line) { $line } else { $why.Substring(0, [Math]::Min(300, $why.Length)) })"
            return "READ ($($reading.L))"
        }
    }
}

$verdicts | ForEach-Object { Write-Host $_ }
if (-not $KeepWork) { Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue }
if (@($verdicts | Where-Object { $_ -like 'FAIL*' }).Count -gt 0 -or $verdicts.Count -lt 4) { exit 1 }
exit 0
