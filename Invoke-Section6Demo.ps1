<#
.SYNOPSIS
    Runbook section 6, the sample-program demo, for the seven modules of the go.* 1.26.10.1 rebuild wave.

.DESCRIPTION
    For each module the script runs the sample under src/tests/ModuleBehavior/<module>@<version>/ (from the samples
    commit, pinned below) with `go run`, then converts it with a go2cs built at T and NO -nuget-map, so the default
    registry (nugetgo.net) answers. It runs with a fresh go2cs cache and a private NUGET_PACKAGES, then builds and runs
    the converted program with `dotnet run`. The gates are section 6's:

      1. The provenance report lists the module with its status and the registry URL as its source, and
         `PackageReference <ID> [<PV>]`. For cobra the same holds for pflag and mousetrap, which it requires.
      2. go2cs.nuget.lock records the registry URL, PV, and a content hash equal to the .nupkg.sha512 that the restore
         wrote for the package downloaded from nuget.org.
      3. Nothing of the module is converted locally (no src/<module> and no pkg/ under the output root).
      4. The converted program, built for this host's RID and run with dotnet, prints exactly what `go run` prints.

    It writes one record per module, <Work>/records/<name>.md (date, host, T, the converter, both output digests, the
    lock row, the provenance line), and prints one PASS or FAIL line per module. Exit code = the number of modules that
    failed. Before converting, it refuses a module that the registry does not serve yet.

    It needs a go2cs clone (-Repo), Go 1.26.10 with GOTOOLCHAIN=local, .NET 10 and PowerShell 7. It adds one detached
    worktree of the clone, <Work>/t, and removes it at the end. -Work must not exist.

    -RegistryUrl exists for testing this script on a host that cannot reach nugetgo.net: an https URL or a local
    mappings file, passed as -nuget-map and expected as the source (a file's lock row names it relative to the output
    root). The demo of record uses the default, without -nuget-map.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Repo,
    [Parameter(Mandatory)][string]$Work,
    [string[]]$Modules = @('hashset', 'uuid', 'jwt', 'xsync', 'pflag', 'mousetrap', 'cobra'),
    [string]$RegistryUrl = 'https://nugetgo.net/v1/mappings.txt',
    [string]$HostLabel = [Environment]::MachineName
)
$ErrorActionPreference = 'Stop'

$T = 'ee99292ba8d85b4dcda3580a4928a2cf81ab0448'        # cobra-pack before its batch 52 merge: go.* 1.26.10.1's dialect
$Samples = '0158158085342502cc3a4c7b9da1279e6cfd5ee2'  # claude/c2-nugetgo-samples
$DefaultRegistry = 'https://nugetgo.net/v1/mappings.txt'
$All = [ordered]@{
    hashset   = @{ Module = 'github.com/ritchiecarroll/hashset';    Version = 'v1.0.0';  Id = 'nugetgo.github.com.ritchiecarroll.hashset';    PV = '1.0.0.3' }
    uuid      = @{ Module = 'github.com/google/uuid';               Version = 'v1.6.0';  Id = 'nugetgo.github.com.google.uuid';               PV = '1.6.0.1' }
    jwt       = @{ Module = 'github.com/golang-jwt/jwt/v5';         Version = 'v5.3.1';  Id = 'nugetgo.github.com.golang-jwt.jwt.v5';         PV = '5.3.1.1' }
    xsync     = @{ Module = 'golang.org/x/sync';                    Version = 'v0.19.0'; Id = 'nugetgo.golang.org.x.sync';                    PV = '0.19.0.1' }
    pflag     = @{ Module = 'github.com/spf13/pflag';               Version = 'v1.0.9';  Id = 'nugetgo.github.com.spf13.pflag';               PV = '1.0.9.1' }
    mousetrap = @{ Module = 'github.com/inconshreveable/mousetrap'; Version = 'v1.1.0';  Id = 'nugetgo.github.com.inconshreveable.mousetrap'; PV = '1.1.0' }
    cobra     = @{ Module = 'github.com/spf13/cobra';               Version = 'v1.10.2'; Id = 'nugetgo.github.com.spf13.cobra';               PV = '1.10.2'; Requires = @('pflag', 'mousetrap') }
}
# cobra imports mousetrap only in command_win.go, so mousetrap is in its closure on Windows alone (measured on linux:
# the provenance report reads 2 referenced as packages, cobra and pflag).
if (-not $IsWindows) { $All.cobra.Requires = @('pflag') }

function Refuse([string]$why) { throw "REFUSED: $why" }
$Modules = @($Modules | ForEach-Object { $_ -split ',' } | Where-Object { $_ })   # pwsh -File passes a comma list as one string
foreach ($n in $Modules) { if (-not $All.Contains($n)) { Refuse "unknown module '$n'; known: $($All.Keys -join ', ')" } }
if (-not (Test-Path -LiteralPath (Join-Path $Repo 'src/go2cs/main.go'))) { Refuse "-Repo $Repo is not a go2cs clone (no src/go2cs/main.go)" }
if (Test-Path -LiteralPath $Work) { Refuse "-Work $Work exists" }
$exe = if ($IsWindows) { 'go2cs.exe' } else { 'go2cs' }
if ((go env GOVERSION) -ne 'go1.26.10') { Refuse "go is $(go env GOVERSION), not go1.26.10 (set GOTOOLCHAIN=local and put Go 1.26.10 first)" }
if ((dotnet --version) -notlike '10.*') { Refuse 'dotnet is not .NET 10' }
$os = if ($IsWindows) { 'win' } elseif ($IsMacOS) { 'osx' } else { 'linux' }
$Rid = "$os-$([Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLowerInvariant())"

# The registry must serve every requested module (and cobra's two requirements) before anything is converted.
$isFile = -not $RegistryUrl.StartsWith('https://')
if ($isFile) { $RegistryUrl = (Resolve-Path -LiteralPath $RegistryUrl).Path }
$text = if ($isFile) { [IO.File]::ReadAllText($RegistryUrl) } else { (Invoke-WebRequest -UseBasicParsing $RegistryUrl).Content }
$served = @{}
foreach ($line in ($text -split "`n")) {
    $f = $line.TrimEnd("`r") -split "`t"
    if ($f.Count -eq 6 -and -not $f[0].StartsWith('#') -and $f[2] -ne 'withdrawn') { $served[$f[0]] = $f }
}
foreach ($n in $Modules) {
    foreach ($m in @($n) + @($All[$n].Requires | Where-Object { $_ })) {
        $row = $served[$All[$m].Module]
        if (-not $row) { Refuse "$RegistryUrl does not serve $($All[$m].Module) yet: wait for its row" }
        if ($row[1] -ne $All[$m].Id) { Refuse "$RegistryUrl maps $($All[$m].Module) to $($row[1]), not $($All[$m].Id)" }
    }
}

New-Item -ItemType Directory $Work, "$Work/records", "$Work/bin" | Out-Null
$Work = (Resolve-Path -LiteralPath $Work).Path
git -C $Repo fetch --quiet origin $T $Samples
if ($LASTEXITCODE) { Refuse "git fetch of $T and $Samples exited $LASTEXITCODE" }
git -C $Repo worktree add --quiet --detach "$Work/t" $T
if ($LASTEXITCODE) { Refuse "git worktree add exited $LASTEXITCODE" }
$failed = 0
try {
    Push-Location "$Work/t/src/go2cs"
    try { go build -o "$Work/bin/$exe" . ; if ($LASTEXITCODE) { Refuse "go build of go2cs exited $LASTEXITCODE" } } finally { Pop-Location }
    $G = "$Work/bin/$exe"
    $version = (& $G -version) -join ' '
    "converter: $version"

    foreach ($n in $Modules) {
        $m = $All[$n]; $W = "$Work/$n"; $app = "$W/app"
        New-Item -ItemType Directory $app | Out-Null
        try {
            $dir = "src/tests/ModuleBehavior/$($m.Module)@$($m.Version)"
            git -C $Repo archive --format=zip -o "$W/sample.zip" $Samples -- $dir
            if ($LASTEXITCODE) { Refuse "git archive of $dir exited $LASTEXITCODE" }
            Expand-Archive -LiteralPath "$W/sample.zip" -DestinationPath "$W/x"
            Copy-Item "$W/x/$dir/main.go", "$W/x/$dir/go.mod", "$W/x/$dir/go.sum" $app

            # go run
            Push-Location $app
            try { & go run . > "$W/go.out" 2> "$W/go.err"; if ($LASTEXITCODE) { Refuse "go run exited $LASTEXITCODE (see $W/go.err)" } } finally { Pop-Location }

            # convert, with a fresh go2cs cache and the default registry
            $cacheVar = if ($IsWindows) { 'LOCALAPPDATA' } else { 'XDG_CACHE_HOME' }
            $saved = @{ cache = [Environment]::GetEnvironmentVariable($cacheVar); gocache = $env:GOCACHE }
            $env:GOCACHE = (go env GOCACHE)
            [Environment]::SetEnvironmentVariable($cacheVar, "$W/go2cs-cache")
            $mapArgs = if ($RegistryUrl -ne $DefaultRegistry) { @('-nuget-map', $RegistryUrl) } else { @() }
            try { & $G -recurse=nuget @mapArgs -go2cspath "$Work/t/src" $app "$W/DR" *> "$W/convert.log" } finally {
                [Environment]::SetEnvironmentVariable($cacheVar, $saved.cache); $env:GOCACHE = $saved.gocache
            }
            if ($LASTEXITCODE) { Refuse "go2cs exited $LASTEXITCODE (see $W/convert.log)" }

            # gate 1: the provenance report
            $report = Get-Content -LiteralPath "$W/convert.log"
            $want = @($n) + @($m.Requires | Where-Object { $_ })
            $lines = @()
            foreach ($k in $want) {
                $d = $All[$k]
                $pat = '^\s+' + [regex]::Escape("$($d.Module)@$($d.Version)") + '\s+->\s+' + [regex]::Escape($d.Id) + '\s+(canonical|community)\s+\[' +
                       [regex]::Escape($RegistryUrl) + '\].*PackageReference ' + [regex]::Escape("$($d.Id) [$($d.PV)]") + '\s*$'
                $hit = @($report | Where-Object { $_ -match $pat })
                if ($hit.Count -ne 1) { Refuse "the provenance report has no line for $($d.Module) -> $($d.Id) [$RegistryUrl] PackageReference [$($d.PV)] (see $W/convert.log)" }
                $lines += $hit[0].Trim()
            }
            if (-not ($report -match "\b$($want.Count) referenced as packages\b")) { Refuse "the provenance report does not read '$($want.Count) referenced as packages'" }

            # gate 3: nothing converted locally
            if ((Test-Path -LiteralPath "$W/DR/src/$($m.Module)") -or (Test-Path -LiteralPath "$W/DR/pkg")) { Refuse "$($m.Module) was converted locally" }

            # build and run, with a private NUGET_PACKAGES
            $proj = @(Get-ChildItem -LiteralPath "$W/DR/src" -Recurse -Filter '*.csproj' | Where-Object { (Get-Content -Raw $_.FullName) -match '<OutputType>Exe</OutputType>' })
            if ($proj.Count -ne 1) { Refuse "the output root holds $($proj.Count) program projects, not one" }
            # Built for this host's portable RID, then the built program run with dotnet (what `dotnet run` runs, without
            # its build messages in the output compared).
            $savedPk = $env:NUGET_PACKAGES; $env:NUGET_PACKAGES = "$W/packages"
            try {
                $p = $proj[0].FullName
                & dotnet build $p -c Release -r $Rid --no-self-contained -p:UseSharedCompilation=false --nologo -v q *> "$W/build.log"
                if ($LASTEXITCODE) { Refuse "dotnet build -r $Rid exited $LASTEXITCODE (see $W/build.log)" }
                $tp = (& dotnet msbuild $p -getProperty:TargetPath -p:Configuration=Release -p:RuntimeIdentifier=$Rid -p:SelfContained=false | Out-String).Trim()
                if (-not (Test-Path -LiteralPath $tp -PathType Leaf)) { Refuse "the build left no program at '$tp'" }
                Push-Location (Split-Path $p)
                try { & dotnet $tp > "$W/dotnet.out" 2> "$W/dotnet.err"; $runCode = $LASTEXITCODE } finally { Pop-Location }
            } finally { $env:NUGET_PACKAGES = $savedPk }
            if ($runCode) { Refuse "the converted program exited $runCode (see $W/dotnet.err)" }

            # gate 2: the lock file, against what the restore downloaded
            $lock = @(Get-Content -LiteralPath "$W/DR/go2cs.nuget.lock" | Where-Object { $_ -and -not $_.StartsWith('#') } | ForEach-Object { ,($_ -split "`t") })
            $lockLines = @()
            foreach ($k in $want) {
                $d = $All[$k]
                $row = @($lock | Where-Object { $_[0] -eq $d.Module })
                if ($row.Count -ne 1) { Refuse "go2cs.nuget.lock has $($row.Count) rows for $($d.Module)" }
                $r = $row[0]
                $layer = if ($isFile) { [IO.Path]::GetRelativePath("$W/DR", $RegistryUrl).Replace('\', '/') } else { $RegistryUrl }
                if ($r[1] -ne $d.Version -or $r[2] -ne $d.Id -or $r[4] -ne $layer -or $r[5] -ne $d.PV) { Refuse "go2cs.nuget.lock's row for $($d.Module) is '$($r -join ' ')'" }
                $sha = "$W/packages/$($d.Id.ToLowerInvariant())/$($d.PV.ToLowerInvariant())/$($d.Id.ToLowerInvariant()).$($d.PV.ToLowerInvariant()).nupkg.sha512"
                if (-not (Test-Path -LiteralPath $sha)) { Refuse "the restore left no $sha" }
                if ($r[6] -ne ('sha512-' + (Get-Content -Raw -LiteralPath $sha).Trim())) { Refuse "go2cs.nuget.lock's content hash for $($d.Id) is not the downloaded package's" }
                $lockLines += ($r -join ' ')
            }

            # gate 4: the outputs
            $goHash = (Get-FileHash -Algorithm SHA256 "$W/go.out").Hash.ToLowerInvariant()
            $netHash = (Get-FileHash -Algorithm SHA256 "$W/dotnet.out").Hash.ToLowerInvariant()
            $equal = $goHash -eq $netHash
            @(
                "# Section 6 demo: $($m.Module)@$($m.Version) -> $($m.Id) $($m.PV)", '',
                "- date: $([DateTime]::UtcNow.ToString('yyyy-MM-dd HH:mm:ss')) UTC; host: $HostLabel; built for $Rid",
                "- T: $T; converter: $version", "- registry: $RegistryUrl$(if ($mapArgs) { ' (via -nuget-map: a TEST run, not the demo of record)' })",
                "- go run: $((Get-Item "$W/go.out").Length) bytes, sha256 $goHash", "- dotnet run: $((Get-Item "$W/dotnet.out").Length) bytes, sha256 $netHash",
                "- outputs-equal: $($equal.ToString().ToLowerInvariant())", '', 'Provenance:', '', ($lines | ForEach-Object { "    $_" }), '', 'Lock:', '', ($lockLines | ForEach-Object { "    $_" })
            ) | ForEach-Object { $_ } | Set-Content -LiteralPath "$Work/records/$n.md" -Encoding utf8
            if (-not $equal) { Refuse "dotnet run's output differs from go run's (see $W/go.out and $W/dotnet.out)" }
            "PASS  $n  $($m.Id) $($m.PV)  outputs equal ($((Get-Item "$W/go.out").Length) bytes)"
        } catch {
            $failed++
            "FAIL  $n  $($_.Exception.Message)"
        }
    }
} finally {
    git -C $Repo worktree remove --force "$Work/t"
    git -C $Repo worktree prune
}
"modules: $($Modules.Count), failed: $failed; records in $Work/records"
exit $failed
