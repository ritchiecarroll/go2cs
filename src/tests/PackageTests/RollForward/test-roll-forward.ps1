#Requires -Version 7
<#
.SYNOPSIS
    Gate for go.lib's roll-forward default (src/core/golib/buildTransitive/go.lib.targets, THE ROLL-FORWARD).

.DESCRIPTION
    Restores the RollForward fixture from ONE feed into a FRESH package cache, builds it per arm, and reads the
    runtimeconfig.json each build writes:

      GREEN         an executable gets "rollForward": "Major".
      WINEXE        so does a Windows GUI executable (-p:OutputType=WinExe).
      TEST PROJECT  so does a test project shaped like a converted test host (IsTestProject, OutputType Exe).
      USER WINS     a RollForward the PROJECT sets (LatestPatch, assigned in the project body through the fixture-only
                    FixtureRollForward; a -p:RollForward would be global and could not test the clause) is kept.
      TEMPLATE      with a runtimeconfig.template.json that sets rollForward, the template's value is the only one.
      LIBRARY       with -p:OutputType=Library the property stays unset (a library chooses no runtime).
      CONTROL       the arm can fail: a project-set Disable is written and, under -DotnetRoot, cannot start.
      LOCAL REFS    a program in the shape of a `go2cs -recurse` conversion in local-reference mode: it references
                    golib's PROJECT (so go.lib.targets never reaches it) and its output root's Directory.Build.targets
                    imports go.lib.rollforward.targets with the converter's own line. It gets Major and, under
                    -DotnetRoot, runs; without that import it gets no rollForward (the red the import turns green).

    With -DotnetRoot pointing at a .NET that has NO 10.0 runtime (an isolated .NET 11 install, say), the GREEN
    build is also RUN under that root and must start and print its runtime major; the CONTROL build must fail to
    start with the missing-framework error. With -TargetsFile (pre-pack), a PUBLISHED arm also builds the fixture
    from the package as published, without the working-tree targets, and expects no rollForward key, and, under
    -DotnetRoot, the same failure to start: that is the red this change turns green. A PUBLISH arm then publishes
    self-contained with and without the working-tree targets and expects the same warnings from both.

    Exit 0 only when every arm reads as expected. Nothing outside a temporary work directory (and its -local sibling,
    the LOCAL REFS arm's) is written.

.PARAMETER Version
    The go.lib package version to restore (the fixture's GoPackageVersion).

.PARAMETER Source
    The feed: a local folder of .nupkg files (a pack rehearsal's output) or a URL. Default nuget.org.

.PARAMETER TargetsFile
    PRE-PACK ONLY: import this go.lib.targets explicitly, to test the working-tree file against an
    already-published go.lib that does not carry the roll-forward default yet.

.PARAMETER Go2csPath
    The go2cs runtime root the LOCAL REFS arm references golib from. Default: this clone's src/.

.PARAMETER DotnetRoot
    Optional: a dotnet root with no Microsoft.NETCore.App 10.0 runtime, used to RUN the built program. The build
    itself always uses the dotnet on PATH.

.EXAMPLE
    pwsh ./test-roll-forward.ps1 -Version 1.26.11.1 -TargetsFile ../../../core/golib/buildTransitive/go.lib.targets -DotnetRoot D:\dotnet11
#>
param(
    [Parameter(Mandatory)] [string] $Version,
    [string] $Source = 'https://api.nuget.org/v3/index.json',
    [string] $TargetsFile,
    [string] $DotnetRoot,
    [string] $Go2csPath = (Join-Path $PSScriptRoot '../../..'),
    [switch] $KeepWork
)

$ErrorActionPreference = 'Stop'

$fixture = $PSScriptRoot
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("roll-forward-" + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $work | Out-Null
Copy-Item (Join-Path $fixture 'RollForward.csproj'), (Join-Path $fixture 'Program.cs') $work

if (Test-Path $Source) { $Source = (Resolve-Path $Source).Path }

if ($DotnetRoot) {
    $DotnetRoot = (Resolve-Path $DotnetRoot).Path
    $tenRuntime = Get-ChildItem (Join-Path $DotnetRoot 'shared/Microsoft.NETCore.App') -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like '10.*' }
    if ($tenRuntime) { throw "-DotnetRoot $DotnetRoot carries a 10.0 runtime ($($tenRuntime.Name -join ', ')); the run arms need a root without one" }
}

# A feed of ONE source and a cache nothing else has touched: a green read must come from the package
# under test, never from a machine-wide cache or a second feed.
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="under-test" value="$Source" />
  </packageSources>
</configuration>
"@ | Set-Content -Path (Join-Path $work 'nuget.config') -Encoding utf8

$env:NUGET_PACKAGES = Join-Path $work 'packages'
$env:NUGET_HTTP_CACHE_PATH = Join-Path $work 'http-cache'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
Remove-Item Env:DOTNET_ROLL_FORWARD -ErrorAction SilentlyContinue

$project = Join-Path $work 'RollForward.csproj'
$withTargets = @()
if ($TargetsFile) { $withTargets = @("-p:GoLibTargets=$((Resolve-Path $TargetsFile).Path)") }

function Build-Arm([string[]] $Extra) {
    Remove-Item -Recurse -Force (Join-Path $work 'bin'), (Join-Path $work 'obj') -ErrorAction SilentlyContinue
    $log = & dotnet build $project -c Debug "-p:GoPackageVersion=$Version" -nologo -v:m @Extra 2>&1 | ForEach-Object { "$_" }
    if ($LASTEXITCODE -ne 0) { $log | Select-String ' error ' | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" }; return $null }
    return Join-Path $work 'bin/Debug/net10.0'
}

function Read-RollForward([string] $OutDir) {
    $config = Join-Path $OutDir 'RollForward.runtimeconfig.json'
    if (-not (Test-Path $config)) { return '<no runtimeconfig>' }
    # Read the KEYS, not a parsed object: a parser keeps one of two duplicate keys and hides the other.
    $values = @([regex]::Matches((Get-Content -Raw $config), '"rollForward"\s*:\s*"([^"]*)"') | ForEach-Object { $_.Groups[1].Value })
    if ($values.Count -eq 0) { return '<unset>' }
    if ($values.Count -gt 1) { return "<$($values.Count) keys: $($values -join ', ')>" }
    return $values[0]
}

# A project property as MSBuild evaluates it, read as JSON (two properties asked) so a warning line cannot pass for it.
function Read-Property([string] $Name, [string[]] $Extra) {
    $json = (& dotnet msbuild $project -restore "-p:GoPackageVersion=$Version" -nologo "-getProperty:$Name" -getProperty:OutputType @Extra 2>$null) -join "`n"
    $start = $json.IndexOf('{')
    if ($LASTEXITCODE -ne 0 -or $start -lt 0) { return $null }
    return ($json.Substring($start) | ConvertFrom-Json).Properties
}

function Invoke-Program([string] $OutDir) {
    $exe = Join-Path $OutDir ('RollForward' + $(if ($IsWindows) { '.exe' } else { '' }))
    $saved = $env:DOTNET_ROOT, $env:DOTNET_ROOT_X64
    try {
        $env:DOTNET_ROOT = $DotnetRoot; $env:DOTNET_ROOT_X64 = $DotnetRoot
        $out = & $exe 2>&1 | ForEach-Object { "$_" }
        return [pscustomobject]@{ Code = $LASTEXITCODE; Text = ($out -join "`n") }
    } finally {
        $env:DOTNET_ROOT, $env:DOTNET_ROOT_X64 = $saved
    }
}

$verdicts = @()
Write-Host "Roll-forward gate: go.lib $Version from $Source$(if ($TargetsFile) { ", working-tree targets" })$(if ($DotnetRoot) { ", run under $DotnetRoot" })"

# GREEN: an executable rolls forward by major.
$out = Build-Arm $withTargets
if (-not $out) { $verdicts += 'FAIL (GREEN): the fixture did not build' }
else {
    $value = Read-RollForward $out
    if ($value -ne 'Major') { $verdicts += "FAIL (GREEN): runtimeconfig rollForward is $value, want Major" }
    elseif ($DotnetRoot) {
        $run = Invoke-Program $out
        Write-Host "    $($run.Text)"
        if ($run.Code -ne 0 -or $run.Text -notmatch 'ROLL-FORWARD: ok runtime=(\d+)' -or [int]$Matches[1] -lt 11) {
            $verdicts += "FAIL (GREEN): under $DotnetRoot the program did not run on a newer major (exit $($run.Code))"
        } else { $verdicts += "PASS (GREEN): rollForward Major, ran on runtime $($Matches[1])" }
    } else { $verdicts += 'PASS (GREEN): rollForward Major' }
}

# WINEXE: a Windows GUI executable rolls forward the same way.
$out = Build-Arm ($withTargets + '-p:OutputType=WinExe')
$value = if ($out) { Read-RollForward $out } else { '<build failed>' }
$verdicts += $(if ($value -eq 'Major') { 'PASS (WINEXE): rollForward Major' } else { "FAIL (WINEXE): rollForward is $value, want Major" })

# TEST PROJECT: a converted test host is an executable test project (its template sets OutputType Exe).
$out = Build-Arm ($withTargets + '-p:FixtureTestProject=true')
$value = if ($out) { Read-RollForward $out } else { '<build failed>' }
$testProps = Read-Property 'IsTestProject' ($withTargets + '-p:FixtureTestProject=true')
$testType = if ($testProps -and $testProps.IsTestProject -eq 'true') { $testProps.OutputType } else { '<not a test project>' }
$verdicts += $(if ($value -eq 'Major' -and $testType -eq 'Exe') { 'PASS (TEST PROJECT): IsTestProject with OutputType Exe, rollForward Major' } else { "FAIL (TEST PROJECT): OutputType '$testType', rollForward $value, want Exe and Major" })

# USER WINS: a value the PROJECT sets is kept. Assigned in the project body, never with -p:RollForward, which is global.
$out = Build-Arm ($withTargets + '-p:FixtureRollForward=LatestPatch')
$value = if ($out) { Read-RollForward $out } else { '<build failed>' }
$verdicts += $(if ($value -eq 'LatestPatch') { 'PASS (USER WINS): rollForward LatestPatch' } else { "FAIL (USER WINS): rollForward is $value, want LatestPatch" })

# TEMPLATE: a runtimeconfig.template.json that sets rollForward owns the value; it must be the only key written.
$template = Join-Path $work 'runtimeconfig.template.json'
'{ "rollForward": "LatestPatch" }' | Set-Content -Path $template -Encoding utf8
$out = Build-Arm $withTargets
$value = if ($out) { Read-RollForward $out } else { '<build failed>' }
Remove-Item $template
$verdicts += $(if ($value -eq 'LatestPatch') { 'PASS (TEMPLATE): one rollForward key, the template''s LatestPatch' } else { "FAIL (TEMPLATE): rollForward is $value, want the template's LatestPatch alone" })

# LIBRARY: no runtime of its own, so the property stays unset.
$library = Read-Property 'RollForward' ($withTargets + '-p:OutputType=Library')
$verdicts += $(if ($null -ne $library -and $library.OutputType -eq 'Library' -and $library.RollForward -eq '') { 'PASS (LIBRARY): RollForward unset' } else { "FAIL (LIBRARY): RollForward reads '$($library.RollForward)' for OutputType '$($library.OutputType)', want unset" })

# CONTROL: the arm can fail. A project-set Disable is written as such and, on a root without 10.0, cannot start.
$out = Build-Arm ($withTargets + '-p:FixtureRollForward=Disable')
$value = if ($out) { Read-RollForward $out } else { '<build failed>' }
if ($value -ne 'Disable') { $verdicts += "FAIL (CONTROL): rollForward is $value, want Disable" }
elseif ($DotnetRoot) {
    $run = Invoke-Program $out
    if ($run.Code -eq 0 -or $run.Text -notmatch "Microsoft\.NETCore\.App', version '10\.") {
        $verdicts += "FAIL (CONTROL): with rollForward Disable the program started under $DotnetRoot (exit $($run.Code))"
    } else { $verdicts += "PASS (CONTROL): rollForward Disable, failed to start as required (exit $($run.Code))" }
} else { $verdicts += 'PASS (CONTROL): rollForward Disable' }

# LOCAL REFS: a -recurse conversion in local-reference mode. Its program references golib's PROJECT, so go.lib.targets
# (and the default it imports) never reaches it; the output root's Directory.Build.targets imports
# go.lib.rollforward.targets itself. The Import below is the converter's own line, verbatim (moduleConverter.go,
# generateRecurseBuildFiles; TestRecurseLocalRefsImportTheRollForward holds the two equal). Every build output, golib's
# included, goes to a sibling of the work directory through the artifacts layout: a folder INSIDE the work directory
# would join the package fixture's default compile glob.
$go2csRoot = ([System.IO.Path]::GetFullPath($Go2csPath)).TrimEnd('/', '\') + '/'
$localRoot = "$work-local"
$localApp = Join-Path $localRoot 'app'
New-Item -ItemType Directory -Path $localApp | Out-Null
Copy-Item (Join-Path $fixture 'Program.cs') $localApp
@"
<Project>
  <PropertyGroup Condition="'`$(go2csPath)' == ''">
    <go2csPath>$go2csRoot</go2csPath>
  </PropertyGroup>
</Project>
"@ | Set-Content -Path (Join-Path $localRoot 'Directory.Build.props') -Encoding utf8
$localImport = @'
  <Import Project="$(go2csPath)core/golib/buildTransitive/go.lib.rollforward.targets"
          Condition="'$(GoLibRollForwardTargetsImported)' != 'true' and '$(go2csPath)' != '' and Exists('$(go2csPath)core/golib/buildTransitive/go.lib.rollforward.targets')" />
'@
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <AssemblyName>RollForward</AssemblyName>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>disable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="`$(go2csPath)core/golib/golib.csproj" />
    <Using Include="go" />
    <Using Include="go.builtin" Static="true" />
  </ItemGroup>
</Project>
"@ | Set-Content -Path (Join-Path $localApp 'RollForward.csproj') -Encoding utf8

function Build-LocalArm([bool] $WithImport) {
    $targets = Join-Path $localRoot 'Directory.Build.targets'
    "<Project>`n$(if ($WithImport) { $localImport })</Project>`n" | Set-Content -Path $targets -Encoding utf8
    $artifacts = Join-Path $localRoot 'artifacts'
    Remove-Item -Recurse -Force $artifacts -ErrorAction SilentlyContinue
    $log = & dotnet build (Join-Path $localApp 'RollForward.csproj') -c Debug -nologo -v:m -p:UseArtifactsOutput=true "-p:ArtifactsPath=$artifacts" 2>&1 | ForEach-Object { "$_" }
    if ($LASTEXITCODE -ne 0) { $log | Select-String ' error ' | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" }; return $null }
    return Join-Path $artifacts 'bin/RollForward/debug'
}

if (-not (Test-Path (Join-Path $go2csRoot 'core/golib/buildTransitive/go.lib.rollforward.targets'))) {
    $verdicts += "FAIL (LOCAL REFS): $go2csRoot carries no core/golib/buildTransitive/go.lib.rollforward.targets"
} else {
    $out = Build-LocalArm $true
    $value = if ($out) { Read-RollForward $out } else { '<build failed>' }
    if ($value -ne 'Major') { $verdicts += "FAIL (LOCAL REFS): rollForward is $value, want Major" }
    elseif ($DotnetRoot) {
        $run = Invoke-Program $out
        Write-Host "    $($run.Text)"
        if ($run.Code -ne 0 -or $run.Text -notmatch 'ROLL-FORWARD: ok runtime=(\d+)' -or [int]$Matches[1] -lt 11) {
            $verdicts += "FAIL (LOCAL REFS): under $DotnetRoot the program did not run on a newer major (exit $($run.Code))"
        } else { $verdicts += "PASS (LOCAL REFS): rollForward Major, ran on runtime $($Matches[1])" }
    } else { $verdicts += 'PASS (LOCAL REFS): rollForward Major' }

    # The same program without the output root's import: no rollForward, and under -DotnetRoot no start. This is the red.
    $out = Build-LocalArm $false
    $value = if ($out) { Read-RollForward $out } else { '<build failed>' }
    if ($value -ne '<unset>') { $verdicts += "FAIL (LOCAL REFS, NO IMPORT): rollForward is $value, want unset" }
    elseif ($DotnetRoot) {
        $run = Invoke-Program $out
        if ($run.Code -eq 0) { $verdicts += "FAIL (LOCAL REFS, NO IMPORT): started under $DotnetRoot; the red did not reproduce" }
        else { $verdicts += "PASS (LOCAL REFS, NO IMPORT): no rollForward, failed to start under $DotnetRoot (exit $($run.Code))" }
    } else { $verdicts += 'PASS (LOCAL REFS, NO IMPORT): no rollForward key' }
}

# PUBLISHED (pre-pack only): the package as published, without the working-tree targets -- the red.
if ($TargetsFile) {
    $out = Build-Arm @()
    $value = if ($out) { Read-RollForward $out } else { '<build failed>' }
    if ($value -ne '<unset>') { $verdicts += "NOTE (PUBLISHED): go.lib $Version already writes rollForward $value" }
    elseif ($DotnetRoot) {
        $run = Invoke-Program $out
        if ($run.Code -eq 0) { $verdicts += "FAIL (PUBLISHED): the published package's build started under $DotnetRoot; the red did not reproduce" }
        else { $verdicts += "PASS (PUBLISHED): no rollForward, failed to start under $DotnetRoot (exit $($run.Code)), the red this change turns green" }
    } else { $verdicts += 'PASS (PUBLISHED): no rollForward key' }

    # PUBLISH: a self-contained publish carries its own runtime; the working-tree file must add no warning to it. The
    # warnings travel as ONE string (a sorted, newline-joined set): a function returning an empty array returns $null,
    # which would read a clean publish as a failed one.
    function Publish-Warnings([string[]] $Extra) {
        Remove-Item -Recurse -Force (Join-Path $work 'bin'), (Join-Path $work 'obj') -ErrorAction SilentlyContinue
        $log = & dotnet publish $project -c Release -r win-x64 --self-contained "-p:GoPackageVersion=$Version" -nologo -v:m @Extra 2>&1 | ForEach-Object { "$_" }
        if ($LASTEXITCODE -ne 0) { return '<failed>' }
        $warnings = @($log | Where-Object { $_ -match ': warning ' } | ForEach-Object { ($_ -replace '\s*\[[^\]]*\]\s*$', '').Trim() } | Sort-Object -Unique)
        return 'warnings=' + $warnings.Count + "`n" + ($warnings -join "`n")
    }
    $before = Publish-Warnings @()
    $after = Publish-Warnings $withTargets
    if ($before -eq '<failed>' -or $after -eq '<failed>') { $verdicts += 'FAIL (PUBLISH): a self-contained publish did not complete' }
    elseif ($before -ne $after) { $verdicts += "FAIL (PUBLISH): the working-tree file changes the publish warnings ($(($before -split "`n")[0]) before, $(($after -split "`n")[0]) after)" }
    else { $verdicts += "PASS (PUBLISH): self-contained publish, the same $(($after -split "`n")[0]) with and without the file" }
}

$verdicts | ForEach-Object { Write-Host $_ }
if (-not $KeepWork) { Remove-Item -Recurse -Force $work, "$work-local" -ErrorAction SilentlyContinue }
if (@($verdicts | Where-Object { $_ -like 'FAIL*' }).Count -gt 0) { exit 1 }
exit 0
