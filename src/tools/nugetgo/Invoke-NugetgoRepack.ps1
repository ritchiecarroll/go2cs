<#
.SYNOPSIS
    Rebuilds one tested nugetgo module against a go.* release: docs/NugetgoPublish.md sections 1 and 2, end to end.

.DESCRIPTION
    One module per run, from nugetgo-modules.psd1 (the pack arguments each module was published with, and the counts
    its kit recorded). The steps are the runbook's, in its order:

      1.1  build G once in T (or take -Converter), and refuse unless G's corpus release is -Release
      1.2  G -recurse -tests -test-action all -platforms P -module-disclosures ... SRC VR      (watched for free disk)
      1.3  gate: exit 0, MODULE.md present; every row compared with the kit's counts (the prediction)
      1.4  G -recurse=nuget -platforms P SRC PR                                                (watched for free disk)
      1.5  gate: every PR project's GoInputDigest equals its MODULE.md row (the binding)
      2    nugetgo-pack.ps1 for each requested pack (int.1 and/or release), -Converter G, -ClosureVersion -Release;
           with -PackageReadme, the author's package README is put in place first (PR/src/M/nuget/README.md), so
           the pack rewrites only its fence and the author's part ships as written

    SRC is the module cache directory, never a copy: a copy has no module version, so -module-disclosures would not
    be consulted for it. The packages land in <Work>/kit/nupkg/<role>/<ID>.nupkg with a SHA256SUMS in the form the
    kits and their checksum gate use, and the package README the release pack left in <Work>/kit/nuget/README.md.
    Build output under VR, PR and T/src/core is purged when the run ends, whatever its outcome.

    It refuses to start over an existing -Work, with a conversion of G already running, or below the disk floor,
    and it stops a conversion that takes free disk below -FloorGB. Exit 0 only when every gate passed and every
    count matched the kit's.

.PARAMETER Module
    module@version, a key of nugetgo-modules.psd1 (e.g. golang.org/x/sync@v0.19.0).
.PARAMETER Revision
    B3's fourth part for this package version: 0 for a first publish, then one more for each rebuild of the same
    module version.
.PARAMETER T
    The go2cs worktree at the release record the module is packed against (runbook 0.3). Must be clean.
.PARAMETER Release
    The go.* release (R): the closure the packages reference, and the corpus G must report.
.PARAMETER Work
    A directory that must not exist yet. Everything the run writes goes under it.
.PARAMETER Converter
    G, when already built from T. Without it, G is built into <Work>/bin from T/src/go2cs.
.PARAMETER Platform
    P, the conversion target. Defaults to this host's GOOS/GOARCH.
.PARAMETER Packs
    Which packs to make: int1, release, or both (the default).
.PARAMETER TestTimeout
    Passed to -tests as -test-timeout when given; state it beside the proof.
.PARAMETER FloorGB
    Free-disk floor in GB for the watched conversions. Default 10.
.PARAMETER PackageReadme
    The author's package README (docs/NugetgoPublish.md, "Customizing the package page"): a fenced generated block,
    which the pack fills, and the author's text outside it. Without it, the pack seeds one.
.PARAMETER ThirdPartyPackage
    For a module that requires other nugetgo modules (cobra): '<module>=<nuget-id>@<package-version>', one per
    module, passed to the pack, which makes each a NuGet dependency.
.PARAMETER Feed
    The pack's single restore source. nuget.org by default; a local folder when it must hold release candidates that
    are not published yet, beside the go.* release they were packed against.
.PARAMETER ConversionReadme
    The conversion repository's root README as it will be committed at nuget-<PV>, a local file, checked by the pack
    in place of the module table's ConversionReadme (the README at the repository's last tag).
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Module,
    [Parameter(Mandatory)][int]$Revision,
    [Parameter(Mandatory)][string]$T,
    [Parameter(Mandatory)][string]$Release,
    [Parameter(Mandatory)][string]$Work,
    [string]$Converter,
    [string]$Platform,
    [ValidateSet('int1', 'release')][string[]]$Packs = @('int1', 'release'),
    [string]$TestTimeout,
    [int]$FloorGB = 10,
    [string]$PackageReadme,
    [string[]]$ThirdPartyPackage = @(),
    [string]$Feed = 'https://api.nuget.org/v3/index.json',
    [string]$ConversionReadme,
    [string]$ModulesFile = (Join-Path $PSScriptRoot 'nugetgo-modules.psd1')
)

$ErrorActionPreference = 'Stop'
function Refuse([string]$why) { throw "REFUSED: $why" }
function Say([string]$line) { Write-Host $line }

if ($PSVersionTable.PSVersion.Major -lt 7) { Refuse "PowerShell $($PSVersionTable.PSVersion) is not 7.x: start pwsh 7 and run again" }
if ($Release -notmatch '^(\d+)\.(\d+)\.(\d+)\.(\d+)$') { Refuse "-Release '$Release' is not a four-part go.* release" }
$goWant = "go$($Matches[1]).$($Matches[2]).$($Matches[3])"

if ($PackageReadme -and -not (Test-Path -LiteralPath $PackageReadme -PathType Leaf)) { Refuse "-PackageReadme $PackageReadme is not a file" }
if ($PackageReadme) { $PackageReadme = (Resolve-Path -LiteralPath $PackageReadme).Path }
$table = Import-PowerShellDataFile -LiteralPath $ModulesFile
if (-not $table.ContainsKey($Module)) { Refuse "$Module is not in $ModulesFile ($($table.Keys -join ', '))" }
$row = $table[$Module]
$modPath, $goVersion = $Module -split '@', 2

$T = (Resolve-Path -LiteralPath $T).Path
$top = git -C $T rev-parse --show-toplevel 2>$null
if ($LASTEXITCODE -or -not $top) { Refuse "$T is not a git worktree" }
$dirty = @(git -C $T status --porcelain)
if ($dirty.Count) { Refuse "$T has $($dirty.Count) uncommitted change(s): the input digests would not name a commit" }
$tHead = git -C $T rev-parse HEAD

if (Test-Path -LiteralPath $Work) { Refuse "$Work exists: every run starts from a fresh root" }

$goVer = (go version) -split ' ' | Select-Object -Index 2
if ($goVer -ne $goWant) { Refuse "go is $goVer; release $Release needs $goWant first on PATH (GOTOOLCHAIN=local)" }
$dotnetVer = dotnet --version
if ($dotnetVer -notlike '10.*') { Refuse "dotnet is $dotnetVer; the packs need the .NET 10 SDK first on PATH" }

if (-not $Platform) {
    $os = if ($IsWindows) { 'windows' } elseif ($IsMacOS) { 'darwin' } else { 'linux' }
    $arch = if ([Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq 'Arm64') { 'arm64' } else { 'amd64' }
    $Platform = "$os/$arch"
}

function Get-FreeBytes { [IO.DriveInfo]::new([IO.Path]::GetPathRoot((Resolve-Path -LiteralPath $Work).Path)).AvailableFreeSpace }
New-Item -ItemType Directory -Path $Work | Out-Null
$Work = (Resolve-Path -LiteralPath $Work).Path
$floor = [int64]$FloorGB * 1GB
$free0 = Get-FreeBytes
if ($free0 -lt $floor) { Refuse ("free disk {0:N1} GB is below the {1} GB floor" -f ($free0 / 1GB), $FloorGB) }
$script:minFree = $free0

# 1.1 G, built once, and the corpus it was built for.
$exe = if ($IsWindows) { '.exe' } else { '' }
if ($Converter) {
    $G = (Resolve-Path -LiteralPath $Converter).Path
} else {
    $G = Join-Path $Work "bin/go2cs$exe"
    Push-Location (Join-Path $T 'src/go2cs')
    try { go build -o $G . ; if ($LASTEXITCODE) { Refuse "go build of G exited $LASTEXITCODE" } } finally { Pop-Location }
}
$gVersion = @(& $G -version)
$gRelease = ($gVersion | Where-Object { $_ -like 'go2cs.release=*' }) -replace '^go2cs\.release=', ''
if ($gRelease -ne $Release) { Refuse "G reports go2cs.release=$gRelease, not ${Release}: T is not the $Release record" }
$alive = @(Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $G })
if ($alive.Count) { Refuse "a conversion by $G is already running (pid $($alive[0].Id))" }

Say "module   $Module  rev $Revision  release $Release  platform $Platform"
Say "T        $tHead (clean)"
Say "G        $($gVersion -join ' ')"
Say "tools    go $goVer, dotnet $dotnetVer, pwsh $($PSVersionTable.PSVersion)"

$SRC = (go mod download -json $Module | ConvertFrom-Json).Dir
if (-not $SRC -or -not (Test-Path -LiteralPath $SRC)) { Refuse "go mod download $Module gave no module directory" }
Say "SRC      the module cache directory of $Module"

# Start-Process does not quote the elements of an -ArgumentList array, so an argument holding a space (an upstream
# holder, a Windows profile path) would split. Every argument is passed double-quoted in one string instead, which
# .NET parses back into the same argv on every OS.
function Join-Argv([string[]]$argv) { ($argv | ForEach-Object { '"' + (($_ -replace '(\\*)"', '$1$1\"') -replace '(\\+)$', '$1$1') + '"' }) -join ' ' }

function Read-NuspecMetadata([string]$nupkg) {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead($nupkg)
    try {
        $entry = $zip.Entries | Where-Object { $_.FullName -like '*.nuspec' } | Select-Object -First 1
        $reader = [IO.StreamReader]::new($entry.Open())
        try { ([xml]$reader.ReadToEnd()).package.metadata } finally { $reader.Dispose() }
    } finally { $zip.Dispose() }
}

# A conversion run as a child, watched for free disk. Output goes to <log>.out / <log>.err.
function Invoke-Watched([string]$label, [string[]]$argv, [string]$log) {
    $start = Get-Date
    $p = Start-Process -FilePath $G -ArgumentList (Join-Argv $argv) -NoNewWindow -PassThru `
        -RedirectStandardOutput "$log.out" -RedirectStandardError "$log.err"
    while (-not $p.HasExited) {
        $f = Get-FreeBytes
        if ($f -lt $script:minFree) { $script:minFree = $f }
        if ($f -lt $floor) {
            Stop-Process -Id $p.Id -Force
            $p.WaitForExit()
            Refuse ("{0}: free disk {1:N1} GB fell below the {2} GB floor; pid {3} stopped" -f $label, ($f / 1GB), $FloorGB, $p.Id)
        }
        Start-Sleep -Seconds 10
    }
    $p.WaitForExit()
    Say ("{0,-8} rc {1}  {2:N0} s" -f $label, $p.ExitCode, ((Get-Date) - $start).TotalSeconds)
    return $p.ExitCode
}

$failures = [Collections.Generic.List[string]]::new()
$VR = Join-Path $Work 'VR'
$PR = Join-Path $Work 'PR'
try {
    # 1.2 and 1.3: the proof, and the prediction.
    $vrArgs = @('-recurse', '-tests', '-test-action', 'all', '-platforms', $Platform, '-go2cspath', (Join-Path $T 'src'),
        '-module-disclosures', (Join-Path $T 'src/tests/ModuleDisclosures'))
    if ($TestTimeout) { $vrArgs += @('-test-timeout', $TestTimeout) }
    $rc = Invoke-Watched 'VR' ($vrArgs + @($SRC, $VR)) (Join-Path $Work 'VR')
    if ($rc) { $failures.Add("1.2: -tests exited $rc (a tested package that did not validate is named in MODULE.md)") }
    $moduleMd = Join-Path $VR "validation/$modPath/MODULE.md"
    if (-not (Test-Path -LiteralPath $moduleMd)) { Refuse "1.3: $moduleMd was not written" }
    $rows = @{}
    foreach ($line in Get-Content -LiteralPath $moduleMd) {
        if ($line -match '^\| `([^`]+)` \| (\d+) \| (\d+) \| `(sha256-[0-9a-f]+)`') {
            $rows[$Matches[1]] = @{ Matched = [int]$Matches[2]; Disclosed = [int]$Matches[3]; Digest = $Matches[4] }
        }
    }
    Say 'PREDICTION (the kit''s counts; a re-pack moves the digests, never the counts)'
    foreach ($pkg in ($row.Expect.Keys + $rows.Keys | Sort-Object -Unique)) {
        $want = $row.Expect[$pkg]
        $got = $rows[$pkg]
        $wantText = if ($want) { "$($want[0]) + $($want[1])" } else { 'no row' }
        $gotText = if ($got) { "$($got.Matched) + $($got.Disclosed)" } else { 'no row' }
        $met = $want -and $got -and $got.Matched -eq $want[0] -and $got.Disclosed -eq $want[1]
        Say ("  {0,-6} {1}: predicted {2}, read {3}" -f $(if ($met) { 'MET' } else { 'MISSED' }), $pkg, $wantText, $gotText)
        if (-not $met) { $failures.Add("1.3: $pkg predicted $wantText, read $gotText") }
    }

    # 1.4 and 1.5: the pack's input, and the binding.
    $rc = Invoke-Watched 'PR' @('-recurse=nuget', '-platforms', $Platform, '-go2cspath', (Join-Path $T 'src'), $SRC, $PR) (Join-Path $Work 'PR')
    if ($rc) { Refuse "1.4: -recurse=nuget exited $rc" }
    $prModule = Join-Path $PR "src/$modPath"
    $excluded = @($row.ExcludePackage)
    foreach ($proj in Get-ChildItem -LiteralPath $prModule -Recurse -Filter '*.csproj' | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }) {
        $rel = [IO.Path]::GetRelativePath((Join-Path $PR 'src'), $proj.DirectoryName) -replace '\\', '/'
        $digest = ([regex]::Match((Get-Content -Raw -LiteralPath $proj.FullName), '<GoInputDigest>([^<]+)</GoInputDigest>')).Groups[1].Value
        if (-not $rows.ContainsKey($rel)) {
            $why = if ($excluded -contains $rel) { 'excluded by the kit' } else { 'no MODULE.md row (untested, or the pack drops it)' }
            Say "  binding  $rel  $why"
            continue
        }
        $ok = $digest -eq $rows[$rel].Digest
        Say ("  binding  {0}  {1}" -f $rel, $(if ($ok) { 'equal' } else { "DIFFERS: PR $digest, MODULE.md $($rows[$rel].Digest)" }))
        if (-not $ok) { $failures.Add("1.5: $rel input digest differs from MODULE.md") }
    }
    if ($failures.Count) { Refuse "section 1 failed; no pack made:`n  $($failures -join "`n  ")" }

    # 2: the packs. The author's package README goes in place first: the pack rewrites only its fence.
    $packageReadmePath = Join-Path $prModule 'nuget/README.md'
    if ($PackageReadme) {
        New-Item -ItemType Directory -Force -Path (Split-Path $packageReadmePath) | Out-Null
        Copy-Item -LiteralPath $PackageReadme -Destination $packageReadmePath -Force
        Say "package README  the author's, put in place"
    }
    $readme = $null
    if ($ConversionReadme) {
        $readme = (Resolve-Path -LiteralPath $ConversionReadme).Path
        Say "readme   $readme"
    } elseif ($row.ConversionReadme) {
        if ($row.ConversionReadme -notmatch '^([^@]+)@([^:]+):(.+)$') { Refuse "ConversionReadme '$($row.ConversionReadme)' is not <owner/repo>@<tag>:<path>" }
        $repo, $tag, $path = $Matches[1], $Matches[2], $Matches[3]
        $clone = Join-Path $Work 'conversion-repo'
        git clone --quiet --depth 1 --branch $tag "https://github.com/$repo.git" $clone
        if ($LASTEXITCODE) { Refuse "git clone of $repo at $tag exited $LASTEXITCODE" }
        $readme = Join-Path $Work 'readme/README.md'
        New-Item -ItemType Directory -Path (Split-Path $readme) | Out-Null
        Copy-Item -LiteralPath (Join-Path $clone $path) -Destination $readme
        Say "readme   $repo $tag $path"
    }
    # The pack runs in a child pwsh 7: the one on PATH. Not this process's own path, which is `dotnet` when pwsh is
    # installed as a .NET global tool (the i7's and the cloud lanes' layout).
    $pwsh = (Get-Command pwsh -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1).Source
    if (-not $pwsh) { Refuse 'no pwsh on PATH for the pack child' }
    $kitDir = Join-Path $Work 'kit/nupkg'
    $sums = [Collections.Generic.List[string]]::new()
    foreach ($pack in $Packs) {
        $packArgs = @('-NoProfile', '-NonInteractive', '-File', (Join-Path $T 'src/tools/nugetgo/nugetgo-pack.ps1'),
            '-ModulePath', $modPath, '-GoVersion', $goVersion, '-Revision', $Revision, '-RecurseRoot', $PR,
            '-ValidationDir', (Join-Path $VR "validation/$modPath"), '-ClosureVersion', $Release,
            '-Feed', $Feed, '-OutDir', (Join-Path $Work "out-$pack"),
            '-Scratch', (Join-Path $Work "scratch-$pack"), '-RepositoryUrl', $row.RepositoryUrl, '-Upstream', $row.Upstream,
            '-LicenseSpdx', $row.LicenseSpdx, '-Converter', $G)
        if ($row.UpstreamPublishes) { $packArgs += '-UpstreamPublishes' }
        if ($readme) { $packArgs += @('-ConversionReadme', $readme) }
        foreach ($x in $excluded) { $packArgs += @('-ExcludePackage', $x) }
        foreach ($x in $ThirdPartyPackage) { $packArgs += @('-ThirdPartyPackage', $x) }
        $packArgs += if ($pack -eq 'int1') { @('-RehearsalSuffix', 'int.1') } else { @('-Release') }
        $start = Get-Date
        $p = Start-Process -FilePath $pwsh -ArgumentList (Join-Argv $packArgs) -NoNewWindow -PassThru -Wait `
            -RedirectStandardOutput (Join-Path $Work "pack-$pack.out") -RedirectStandardError (Join-Path $Work "pack-$pack.err")
        Say ("pack {0,-7} rc {1}  {2:N0} s" -f $pack, $p.ExitCode, ((Get-Date) - $start).TotalSeconds)
        if ($p.ExitCode) { Refuse "the $pack pack exited $($p.ExitCode): see pack-$pack.out" }
        $nupkg = @(Get-ChildItem -LiteralPath (Join-Path $Work "out-$pack") -Filter '*.nupkg')
        if ($nupkg.Count -ne 1) { Refuse "the $pack pack wrote $($nupkg.Count) nupkg files, not 1" }
        $role = if ($pack -eq 'int1') { 'int.1' } else { 'release' }
        $id = Read-NuspecMetadata $nupkg[0].FullName
        New-Item -ItemType Directory -Force -Path (Join-Path $kitDir $role) | Out-Null
        $dest = Join-Path $kitDir "$role/$($id.id).nupkg"
        Copy-Item -LiteralPath $nupkg[0].FullName -Destination $dest
        $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $dest).Hash.ToLowerInvariant()
        $sums.Add("$hash  $role/$($id.id).nupkg")
        Say "  $role  $($id.id) $($id.version)  sha256 $hash"
    }
    Set-Content -LiteralPath (Join-Path $kitDir 'SHA256SUMS') -Value $sums
    # The package README as the last pack left it: the file the conversion commit carries (runbook 4.5).
    if (Test-Path -LiteralPath $packageReadmePath) {
        $kitReadme = Join-Path $Work 'kit/nuget/README.md'
        New-Item -ItemType Directory -Force -Path (Split-Path $kitReadme) | Out-Null
        Copy-Item -LiteralPath $packageReadmePath -Destination $kitReadme
        Say "package README  kit/nuget/README.md"
    }
    Say ("min free during run: {0:N1} GB" -f ($script:minFree / 1GB))
    Say 'DONE: every gate passed and every count matched the kit''s.'
} finally {
    foreach ($root in @($VR, $PR, (Join-Path $T 'src/core'))) {
        if (Test-Path -LiteralPath $root) {
            Get-ChildItem -LiteralPath $root -Recurse -Directory -Force -ErrorAction SilentlyContinue |
                Where-Object { $_.Name -in 'bin', 'obj' } | Sort-Object { $_.FullName.Length } -Descending |
                ForEach-Object { if (Test-Path -LiteralPath $_.FullName) { Remove-Item -LiteralPath $_.FullName -Recurse -Force } }
        }
    }
    $after = @(git -C $T status --porcelain)
    if ($after.Count) { Write-Warning "T has $($after.Count) change(s) after the run's purge: read them before the next run" }
}
