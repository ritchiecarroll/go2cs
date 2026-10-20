<#
.SYNOPSIS
    Write ONE behavioral proof row, <rid>.md, for a nugetgo package of a module with no Go tests, on this host.

.DESCRIPTION
    COORD ruling 2026-10-12 (the cobra increment, part 3): a module with no Go tests is proven by behavior, one row per
    shipped RID, and every row is run on a host of its own RID: no row is inferred from another. The os-matrix
    workflow's nugetgo-behavior stage runs this script on hosted runners (windows-latest, ubuntu-latest, macos-15 and
    macos-15-intel); any other host may run it too, naming itself with -HostLabel.

    The steps are the runbook's rehearsal (docs/NugetgoPublish.md, section 3, steps 3 to 5) on this host:

      1. The sample program under src/tests/ModuleBehavior/<module>@<version>/ is run with `go run`.
      2. It is converted with -Converter against the package on -Feed (`-recurse=nuget -nuget-map-only
         -nuget-map-feed`, the mappings row beside the sample), and the consumer half is checked: the app references
         the package at exactly -PackageVersion, and the module was not converted locally.
      3. It is built for this RID (`dotnet build -r <rid>`) with a private NUGET_PACKAGES, so each module assembly in
         the output folder is the one the host loads: the package's runtimes/<rid>/ asset when it has one, else lib/.
         The script refuses a build whose copy of any of them is not that asset.
      4. The built program is run with dotnet, and the row records both outputs' hashes, whether they are equal, every
         module assembly's file name and hash (one per Go package the package carries), this host's RID (as its
         dotnet reports it) and GOOS (as go reports it), the host and the toolchains.

    The row is written even when the outputs differ, as evidence; the script then exits 1. NugetgoBehaviorProof.psm1
    reads the rows, and nugetgo-pack.ps1 -BehaviorProofDir binds each one to the bytes it packs for that RID.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ModulePath,
    [Parameter(Mandatory)][string]$GoVersion,
    # The package version the row is run against: the rehearsal (int.1) package, whose assemblies are the release's.
    [Parameter(Mandatory)][string]$PackageVersion,
    # A NuGet v3 feed URL (int.nugettest.org's, for a rehearsal on the test gallery) or a local folder of nupkgs.
    [Parameter(Mandatory)][string]$Feed,
    [Parameter(Mandatory)][string]$Rid,
    # The go2cs binary, built from this tree.
    [Parameter(Mandatory)][string]$Converter,
    # Where <rid>.md is written.
    [Parameter(Mandatory)][string]$OutDir,
    # A new or empty working directory.
    [Parameter(Mandatory)][string]$Work,
    [string]$SampleDir,
    # The host's name when it is not a hosted runner (Get-NugetgoRowHost).
    [string]$HostLabel
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NugetgoBehaviorProof.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'NugetgoFlavors.psm1') -Force

function Refuse([string]$why) { throw "REFUSED: $why" }
function Sha256File([string]$path) { 'sha256-' + (Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash.ToLowerInvariant() }
function Invoke-Captured([string]$file, [string[]]$arguments, [string]$dir, [string]$stdout, [string]$stderr) {
    # Start-Process hands the program a FILE as its standard output, so the output is compared as the bytes it wrote.
    $p = Start-Process $file -ArgumentList $arguments -WorkingDirectory $dir -NoNewWindow -Wait -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
    $p.ExitCode
}

$src = Split-Path (Split-Path $PSScriptRoot)
if (-not $SampleDir) { $SampleDir = Join-Path $src "tests/ModuleBehavior/$ModulePath@$GoVersion" }

# ---- this host is the RID's own -------------------------------------------------------------------------------------
$hostName = Get-NugetgoRowHost -Environment @{ ImageOS = $env:ImageOS; ImageVersion = $env:ImageVersion; NUGETGO_RUNNER = $env:NUGETGO_RUNNER } -HostLabel $HostLabel
$ridGoos = Get-NugetgoRidFlavors
if (-not $ridGoos.Contains($Rid)) { Refuse "$Rid is not a RID the go.* release ships ($(@($ridGoos.Keys) -join ', '))" }
$dotnetInfo = @(& dotnet --info)
$runtimeRid = @($dotnetInfo | ForEach-Object { if ($_ -match '^\s*RID:\s*(\S+)\s*$') { $Matches[1] } })[0]
if ($runtimeRid -cne $Rid) { Refuse "this host's dotnet runs $runtimeRid, not ${Rid}: a row is written on a host of its own RID" }
$goos = (& go env GOOS).Trim()
$goHostOs = (& go env GOHOSTOS).Trim()
if ($goos -cne $ridGoos[$Rid] -or $goHostOs -cne $ridGoos[$Rid]) { Refuse "go reports GOOS $goos on GOHOSTOS $goHostOs, not ${Rid}'s $($ridGoos[$Rid])" }
$osName = if ($env:RUNNER_OS) { "$($env:RUNNER_OS) $($env:RUNNER_ARCH)" } else { "$([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture)" }
$os = "${osName}: $([System.Runtime.InteropServices.RuntimeInformation]::OSDescription.Trim())"

# ---- the sample and its mappings row --------------------------------------------------------------------------------
foreach ($f in 'main.go', 'go.mod', 'go.sum', 'mappings.txt') {
    if (-not (Test-Path -LiteralPath (Join-Path $SampleDir $f) -PathType Leaf)) { Refuse "the sample $SampleDir has no $f" }
}
$mapping = @([System.IO.File]::ReadAllLines((Join-Path $SampleDir 'mappings.txt')) | Where-Object { $_ -and -not $_.StartsWith('#') } |
    ForEach-Object { , ($_ -split "`t") } | Where-Object { $_[0] -ceq $ModulePath })
if ($mapping.Count -ne 1) { Refuse "$SampleDir/mappings.txt has $($mapping.Count) row(s) for $ModulePath; one is needed" }
$id = $mapping[0][1]

if ((Test-Path -LiteralPath $Work) -and @(Get-ChildItem -LiteralPath $Work -Force).Count) { Refuse "-Work $Work is not empty" }
$app = Join-Path $Work 'app'
$converted = Join-Path $Work 'AR'
New-Item -ItemType Directory -Force $app, $OutDir | Out-Null
Copy-Item -Path (Join-Path $SampleDir '*') -Destination $app -Recurse
Move-Item -LiteralPath (Join-Path $app 'mappings.txt') -Destination (Join-Path $Work 'mappings.txt')

# ---- 1. go run -------------------------------------------------------------------------------------------------------
$goOut = Join-Path $Work 'go-run.out'
$code = Invoke-Captured 'go' @('run', '.') $app $goOut (Join-Path $Work 'go-run.err')
if ($code -ne 0) { Refuse "go run exited $code (see $Work/go-run.err)" }
Write-Host "go run: $((Get-Item -LiteralPath $goOut).Length) bytes, $(Sha256File $goOut)"

# ---- 2. convert against the package, and the consumer half ------------------------------------------------------------
$convertLog = Join-Path $Work 'convert.log'
& $Converter -recurse=nuget -nuget-map (Join-Path $Work 'mappings.txt') -nuget-map-only -nuget-map-feed $Feed -go2cspath $src $app $converted *> $convertLog
if ($LASTEXITCODE -ne 0) { Refuse "go2cs exited $LASTEXITCODE (see $convertLog)" }
$exe = @(Get-ChildItem -LiteralPath (Join-Path $converted 'src') -Recurse -Filter '*.csproj' | Where-Object { [System.IO.File]::ReadAllText($_.FullName) -match '<OutputType>Exe</OutputType>' })
if ($exe.Count -ne 1) { Refuse "the conversion holds $($exe.Count) program project(s), not one (see $convertLog)" }
$project = $exe[0].FullName
if ([System.IO.File]::ReadAllText($project) -notmatch [regex]::Escape("Include=""$id"" Version=""[$PackageVersion]""")) { Refuse "$project does not reference $id at exactly [$PackageVersion]" }
if ((Test-Path -LiteralPath (Join-Path (Join-Path $converted 'src') $ModulePath)) -or (Test-Path -LiteralPath (Join-Path $converted 'pkg'))) { Refuse "$ModulePath was converted locally: the program does not run the package" }
Write-Host "converted: $(Split-Path -Leaf $project) references $id [$PackageVersion]"

# ---- 3. build for this RID, against the package -------------------------------------------------------------------------
$feedValue = if (Test-Path -LiteralPath $Feed -PathType Container) { [System.IO.Path]::GetFullPath($Feed) } else { $Feed }
[System.IO.File]::WriteAllText((Join-Path (Split-Path $project) 'nuget.config'), @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="nugetgo" value="$([System.Security.SecurityElement]::Escape($feedValue))" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="nugetgo"><package pattern="nugetgo.*" /></packageSource>
    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
  </packageSourceMapping>
</configuration>
"@, (New-Object System.Text.UTF8Encoding($false)))
$packages = Join-Path $Work 'packages'
$saved = @{ NUGET_PACKAGES = $env:NUGET_PACKAGES; GoTargetOS = $env:GoTargetOS }
try {
    $env:NUGET_PACKAGES = $packages
    # The program is not a corpus project: a leg-wide GoTargetOS must not reach its build as a global property.
    Remove-Item Env:GoTargetOS -ErrorAction SilentlyContinue
    $ridArgs = @('-c', 'Release', '-r', $Rid, '--no-self-contained', '-p:UseSharedCompilation=false', '--nologo')
    & dotnet build $project @ridArgs -v q *> (Join-Path $Work 'build.log')
    if ($LASTEXITCODE -ne 0) { Refuse "dotnet build -r $Rid exited $LASTEXITCODE (see $Work/build.log)" }
    $targetPath = (& dotnet msbuild $project -getProperty:TargetPath -p:Configuration=Release -p:RuntimeIdentifier=$Rid -p:SelfContained=false | Out-String).Trim()
    if (-not (Test-Path -LiteralPath $targetPath -PathType Leaf)) { Refuse "the build left no program at '$targetPath'" }

    # ---- 4. dotnet run ------------------------------------------------------------------------------------------------
    $dotnetOut = Join-Path $Work 'dotnet-run.out'
    # Quoted: Start-Process joins its arguments into one command line, so a path with a space would split.
    $code = Invoke-Captured 'dotnet' @("`"$targetPath`"") (Split-Path $project) $dotnetOut (Join-Path $Work 'dotnet-run.err')
    if ($code -ne 0) { Refuse "the converted program exited $code (see $Work/dotnet-run.err)" }
}
finally { foreach ($k in $saved.Keys) { if ($null -eq $saved[$k]) { Remove-Item "env:$k" -ErrorAction SilentlyContinue } else { Set-Item "env:$k" $saved[$k] } } }
Write-Host "dotnet: $((Get-Item -LiteralPath $dotnetOut).Length) bytes, $(Sha256File $dotnetOut)"

# ---- the module assemblies that loaded ---------------------------------------------------------------------------------
# Every assembly the package carries (one per Go package), each as the program's folder holds it, which must be the
# package's asset for this RID: runtimes/<rid>/ when the package has one, else lib/.
$restored = Join-Path (Join-Path $packages $id.ToLowerInvariant()) $PackageVersion.ToLowerInvariant()
$libNames = @(Get-ChildItem -LiteralPath (Join-Path $restored 'lib/net10.0') -Filter '*.dll' -File -ErrorAction SilentlyContinue | ForEach-Object { $_.Name } | Sort-Object)
if ($libNames.Count -eq 0) { Refuse "$id $PackageVersion carries no assembly in lib/net10.0 (restored at $restored)" }
$assemblies = foreach ($name in $libNames) {
    $asset = Join-Path $restored "runtimes/$Rid/lib/net10.0/$name"
    if (-not (Test-Path -LiteralPath $asset -PathType Leaf)) { $asset = Join-Path $restored "lib/net10.0/$name" }
    $loaded = Join-Path (Split-Path $targetPath) $name
    if (-not (Test-Path -LiteralPath $loaded -PathType Leaf)) { Refuse "the program's folder holds no $name" }
    if ((Sha256File $loaded) -cne (Sha256File $asset)) { Refuse "the program's $name is not the package's $Rid asset ($asset)" }
    "$name $(Sha256File $loaded)"
}

# ---- the row ------------------------------------------------------------------------------------------------------------
$goHash = Sha256File $goOut
$dotnetHash = Sha256File $dotnetOut
$fields = [ordered]@{
    'module'        = $ModulePath
    'version'       = $GoVersion
    'package'       = "$id $PackageVersion"
    'rid'           = $Rid
    'runtime-rid'   = $runtimeRid
    'goos'          = $goos
    'host'          = $hostName
    'os'            = $os
    'go'            = (& go env GOVERSION).Trim()
    'dotnet'        = (& dotnet --version).Trim()
    'go-output'     = $goHash
    'dotnet-output' = $dotnetHash
    'outputs-equal' = $(if ($goHash -ceq $dotnetHash) { 'true' } else { 'false' })
    'assembly'      = (@($assemblies) -join '; ')
    'date'          = [DateTime]::UtcNow.ToString('yyyy-MM-dd')
}
$row = New-NugetgoBehaviorRowText -Fields $fields
$rowPath = Join-Path $OutDir "$Rid.md"
[System.IO.File]::WriteAllText($rowPath, $row, (New-Object System.Text.UTF8Encoding($false)))
Write-Host "row: $rowPath"
Write-Host $row
if ($fields['outputs-equal'] -cne 'true') { Write-Host "the converted program did not print what go run printed: go $goHash, dotnet $dotnetHash"; exit 1 }
exit 0
