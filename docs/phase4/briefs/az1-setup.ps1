#Requires -RunAsAdministrator
<# setup-az1.ps1 -- one-time provisioning for the temporary go2cs lane AZ1 (an Azure Windows Server 2025 VM).
   Run ONCE, in an ELEVATED Windows PowerShell (5.1 is fine), from C:\az1, so that no error record can
   carry a profile path:
       New-Item -ItemType Directory -Force C:\az1 | Out-Null      # then save this file as C:\az1\setup-az1.ps1
       Set-ExecutionPolicy -Scope Process Bypass -Force; C:\az1\setup-az1.ps1
   IDEMPOTENT: every step checks before it acts, so a re-run only fills gaps and reprints the readback.
   NEVER re-run it while a lane tree exists. If it finds one, it SKIPS the fetch and ownership steps and
   says so: a `fetch --prune` on the PARENT repo moves the refs the linked worktrees read, and a recursive
   owner change would reach the live worktree.
   Stores NO credential. GitHub push auth is a separate owner step (AZ1-brief.md, section A.2).
   Everything installs under fixed roots OUTSIDE any user profile, so no username can reach a path in a
   run log or a committed evidence file.
   PASTE-SAFE: ONLY the READBACK block, which prints no host identifier. An error record prints full
   paths (`At <script path>:NN`, download and move targets). Never paste error text to COORD as it is:
   describe it, or have AZ1 run it through the census first.
   DOWNLOAD TRUST: every installer is downloaded fresh (a file from an earlier run is never reused), and
   it must carry a Valid Authenticode signature from a PINNED signer: the Git for Windows maintainer for
   Git, and Microsoft Corporation for the PowerShell MSI and dotnet-install.ps1. Otherwise the file is
   deleted and the script stops, naming the signer it saw. The Go zip is checked against go.dev's
   published SHA-256. A GitHub release digest is checked too, but it comes from the same API as the file,
   so it proves transport integrity only; the pinned signer is the trust.
   NOT installed on purpose: a machine-scope Go (the MSI puts one on PATH), go1.23.12, Visual Studio/C++,
   Python, gh. NOT set on purpose: GOROOT/GOFLAGS/GODEBUG/DOTNET_ROOT at any persistent scope (per shell only). #>
[CmdletBinding()]
param(
    [string] $GoRoot     = 'C:\go124',      # go1.24.13; the exact spelling `go env GOROOT` prints (floor 6)
    [string] $DotnetRoot = 'C:\dotnet10',   # .NET SDK root; exported per shell as DOTNET_ROOT, never machine-wide
    [string] $SrcRoot    = 'C:\src',        # the clone lands at $SrcRoot\go2cs; run worktrees become siblings
    [string] $AzRoot     = 'C:\az1',        # the brief's <AZ>: scratch root outside every git tree; downloads go to $AzRoot\dl
    [string] $GoVersion  = 'go1.24.13',     # the corpus pin (claude/version-go1.24.13's src/version.props GoStdLibVersion; master still reads 1.23.12)
    [string] $DotnetSdk  = '10.0.400',      # the fleet's patch (i7, R-LAPTOP; R's validated crypto/tls reading)
    [string] $TimeZoneId = 'Central Standard Time', # ledger stamps are the owner's local time; '' keeps UTC
    [switch] $DeveloperMode                 # OFF by default -- see step 6
)
$ErrorActionPreference = 'Stop'
$ProgressPreference    = 'SilentlyContinue'  # 5.1's progress bar makes Invoke-WebRequest ~10x slower
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$env:DOTNET_NOLOGO = '1'; $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$dl = Join-Path $AzRoot 'dl'                 # NOT under $env:TEMP: that is inside the admin profile
$SignerGit       = '^CN=Johannes Schindelin,'   # Git for Windows' maintainer (confirm once on a trusted box; a change throws, naming the subject seen)
$SignerMicrosoft = '^CN=Microsoft Corporation,'
function Step($m) { Write-Host "`n== $m" -ForegroundColor Cyan }
# Native tools write progress to stderr; 5.1 under Stop would turn that into a throw. Judge by exit code only.
# LASTEXITCODE is RESET first: an executable that fails to launch (CommandNotFound is non-terminating under
# 'Continue') leaves no exit code, and a stale 0 from the previous native call must not read as success.
function Native([string] $what, [scriptblock] $b) {
    $old = $ErrorActionPreference; $ErrorActionPreference = 'Continue'
    $global:LASTEXITCODE = $null
    try { & $b } finally { $ErrorActionPreference = $old }
    if ($null -eq $LASTEXITCODE) { throw "$what did not run: no exit code (was the executable found?)" }
    if ($LASTEXITCODE -ne 0) { throw "$what failed: exit $LASTEXITCODE" }
}
# Always a FRESH download: a partial or replaced file from an earlier run is never reused or executed.
function Get-File($url, $out) {
    if (Test-Path -LiteralPath $out) { Remove-Item -LiteralPath $out -Force }
    Invoke-WebRequest -UseBasicParsing -Uri $url -OutFile $out
    $out
}
function Assert-Sha256($file, $want) {
    $got = (Get-FileHash -Algorithm SHA256 $file).Hash.ToLowerInvariant()
    if ($got -ne $want.ToLowerInvariant()) { Remove-Item $file -Force; throw "SHA-256 MISMATCH on $(Split-Path $file -Leaf): got $got, want $want" }
    Write-Host "  sha256 OK  $(Split-Path $file -Leaf)"
}
# Valid AND the pinned signer, or the file is deleted before the throw (so a re-run starts clean).
function Assert-Signer($file, $subjectRe) {
    $sig = Get-AuthenticodeSignature $file
    $who = if ($sig.SignerCertificate) { $sig.SignerCertificate.Subject } else { '(no signer)' }
    if ($sig.Status -ne 'Valid' -or $who -notmatch $subjectRe) {
        Remove-Item $file -Force
        throw "$(Split-Path $file -Leaf): Authenticode status $($sig.Status), signer '$who' -- want Valid and a subject matching '$subjectRe'"
    }
    Write-Host "  signer OK  $(Split-Path $file -Leaf)  ($($sig.Status); $($who.Split(',')[0]))"
}
# The latest (non-prerelease) GitHub release asset matching $pattern: the published digest is checked
# when the API carries one, and the pinned Authenticode signer is checked always.
function Get-GhAsset($repo, $pattern, $signerRe) {
    $rel = Invoke-RestMethod -UseBasicParsing "https://api.github.com/repos/$repo/releases/latest" -Headers @{ 'User-Agent' = 'az1-setup' }
    $a = @($rel.assets | Where-Object { $_.name -match $pattern })[0]
    if (-not $a) { throw "no asset matching '$pattern' in $repo's latest release" }
    $f = Get-File $a.browser_download_url (Join-Path $dl $a.name)
    if ($a.PSObject.Properties['digest'] -and "$($a.digest)" -like 'sha256:*') { Assert-Sha256 $f $a.digest.Substring(7) }
    Assert-Signer $f $signerRe
    $f
}

Step "0. Scratch root $AzRoot (the brief's <AZ>) and its download folder"
New-Item -ItemType Directory -Force $AzRoot, $dl | Out-Null

Step '1. Git for Windows (the converter spawns git; Claude Code on Windows needs Git Bash)'
$git = 'C:\Program Files\Git\cmd\git.exe'
if (-not (Test-Path $git)) {
    $exe = Get-GhAsset 'git-for-windows/git' '^Git-[\d.]+-64-bit\.exe$' $SignerGit
    # Installer defaults are the fleet's: core.autocrlf=true (system), Git Credential Manager, Git Bash.
    $p = Start-Process $exe -ArgumentList '/VERYSILENT', '/NORESTART', '/NOCANCEL', '/SP-', '/SUPPRESSMSGBOXES' -Wait -PassThru
    if ($p.ExitCode -ne 0) { throw "Git installer exit $($p.ExitCode)" }
    if (-not (Test-Path $git)) { throw "the Git installer exited 0 but placed no git.exe at the expected path" }
} else { Write-Host '  present' }
$env:PATH = 'C:\Program Files\Git\cmd;' + $env:PATH

Step '2. PowerShell 7 from the MSI (self-contained -- NOT the dotnet-tool pwsh, whose net8 apphost dies under a .NET 10 DOTNET_ROOT)'
$pwsh = 'C:\Program Files\PowerShell\7\pwsh.exe'
if (-not (Test-Path $pwsh)) {
    $msi = Get-GhAsset 'PowerShell/PowerShell' '^PowerShell-7\.[\d.]+-win-x64\.msi$' $SignerMicrosoft
    $p = Start-Process msiexec.exe -ArgumentList '/i', "`"$msi`"", '/quiet', '/norestart', 'ADD_PATH=1', 'USE_MU=0', 'ENABLE_MU=0' -Wait -PassThru
    if ($p.ExitCode -notin 0, 3010) { throw "PowerShell MSI exit $($p.ExitCode)" }
} else { Write-Host '  present' }

Step "3. Go $GoVersion -> $GoRoot (official zip, SHA-256 checked against go.dev's manifest; System32 tar; no MSI, no GOTOOLCHAIN fetch)"
$have = ''; if (Test-Path "$GoRoot\VERSION") { $have = Get-Content "$GoRoot\VERSION" -TotalCount 1 }
if ($have -ne $GoVersion) {
    if (Test-Path $GoRoot) { throw "$GoRoot exists but its VERSION reads '$have', not $GoVersion -- remove it by hand, then re-run" }
    $name = "$GoVersion.windows-amd64.zip"
    # Read the manifest as text: the include=all document is large, and one object is all we need.
    $json = (Invoke-WebRequest -UseBasicParsing 'https://go.dev/dl/?mode=json&include=all').Content
    $obj  = [regex]::Match($json, '\{[^{}]*"filename"\s*:\s*"' + [regex]::Escape($name) + '"[^{}]*\}').Value
    $sha  = [regex]::Match($obj, '"sha256"\s*:\s*"([0-9a-f]{64})"').Groups[1].Value
    if (-not $sha) { throw "go.dev's manifest publishes no sha256 for $name" }
    $zip = Get-File "https://go.dev/dl/$name" (Join-Path $dl $name)
    Assert-Sha256 $zip $sha
    $stage = "$GoRoot.staging"; if (Test-Path $stage) { Remove-Item -Recurse -Force $stage }
    New-Item -ItemType Directory $stage | Out-Null
    Native 'tar' { & "$env:SystemRoot\System32\tar.exe" -xf $zip -C $stage }  # writable files, unlike a toolchain fetch
    Move-Item (Join-Path $stage 'go') $GoRoot; Remove-Item $stage -Force
} else { Write-Host '  present' }

Step "4. .NET SDK $DotnetSdk -> $DotnetRoot (dotnet-install.ps1 -Version; exactly one SDK, since the repo has no global.json)"
$dotnet = Join-Path $DotnetRoot 'dotnet.exe'
$sdks = @(); if (Test-Path $dotnet) { $sdks = @(& $dotnet --list-sdks) }
if (-not ($sdks -match ('^' + [regex]::Escape($DotnetSdk) + ' '))) {
    $inst = Get-File 'https://dot.net/v1/dotnet-install.ps1' (Join-Path $dl 'dotnet-install.ps1')
    Assert-Signer $inst $SignerMicrosoft          # BEFORE it runs under -ExecutionPolicy Bypass
    Native 'dotnet-install' { & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $inst -Version $DotnetSdk -InstallDir $DotnetRoot -NoPath }
} else { Write-Host '  present' }

Step '5. LongPathsEnabled=1 (golib sets its own long-path bit, so the row does not need it; git, tar and Explorer on deep temp paths do)'
$fs = 'HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem'
if ((Get-ItemProperty $fs).LongPathsEnabled -ne 1) { Set-ItemProperty $fs -Name LongPathsEnabled -Value 1 -Type DWord }

# 6. Developer Mode lets a non-elevated process create symlinks. crypto/tls does NOT need it: R-LAPTOP
#    validated this row without the symlink privilege, and the tree's winsymlink host seat covers the
#    junction fallback. Left OFF so AZ1 matches the validated host state; pass -DeveloperMode only if COORD asks.
Step "6. Developer Mode: $(if ($DeveloperMode) { 'ON (asked)' } else { 'left as is (off by default)' })"
if ($DeveloperMode) {
    $k = 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock'
    if (-not (Test-Path $k)) { New-Item $k | Out-Null }
    Set-ItemProperty $k -Name AllowDevelopmentWithoutDevLicense -Value 1 -Type DWord
}

Step '7. Time zone and OS partition'
if ($TimeZoneId -and (Get-TimeZone).Id -ne $TimeZoneId) { Set-TimeZone -Id $TimeZoneId }
$max = (Get-PartitionSupportedSize -DriveLetter C).SizeMax        # grow C: to the whole 256 GiB disk if the image left it short
if ($max - (Get-Partition -DriveLetter C).Size -gt 1GB) { Resize-Partition -DriveLetter C -Size $max }

Step "8. Clone the public repo -> $SrcRoot\go2cs (full clone; ~0.5 GiB checkout plus a pack of roughly 0.75 GiB or more)"
$repo = Join-Path $SrcRoot 'go2cs'
New-Item -ItemType Directory -Force $SrcRoot | Out-Null
# RE-RUN GUARD: count linked worktrees. -1 = unreadable, treated as present (the safe direction).
$laneTrees = 0
if (Test-Path (Join-Path $repo '.git')) {
    $old = $ErrorActionPreference; $ErrorActionPreference = 'Continue'; $global:LASTEXITCODE = $null
    try { $wt = @(& $git -C $repo worktree list) } finally { $ErrorActionPreference = $old }
    $laneTrees = if ($LASTEXITCODE -eq 0) { $wt.Count - 1 } else { -1 }
}
if ($laneTrees -ne 0) {
    Write-Host "  a lane tree exists (linked worktrees: $laneTrees) -- fetch and ownership steps SKIPPED. Do not re-run setup during a row." -ForegroundColor Yellow
} else {
    if (-not (Test-Path (Join-Path $repo '.git'))) { Native 'git clone' { & $git clone https://github.com/ritchiecarroll/go2cs $repo } }
    else { Native 'git fetch' { & $git -C $repo fetch origin --prune --quiet } }
    # An elevated shell makes BUILTIN\Administrators the owner, and git (and `go build`'s VCS stamping) then
    # refuses the repo as "dubious ownership" in the non-elevated AZ1 session. Hand it to the invoking user.
    Native 'icacls /setowner' { & icacls.exe $SrcRoot /setowner "$env:USERDOMAIN\$env:USERNAME" /T /C /Q | Out-Null }
}
# A commit must never fall back to an identity git synthesizes from the account and the host's DNS name.
# The owner sets user.name / user.email (AZ1-brief.md A.2); the readback compares them with the fleet's.
Native 'git config useConfigOnly' { & $git config --global user.useConfigOnly true }

Step 'READBACK (no host identifiers; the ONLY paste-safe block)'
foreach ($v in 'GOROOT', 'GOFLAGS', 'GODEBUG', 'GOTOOLCHAIN', 'GOPATH', 'DOTNET_ROOT') {   # must all be empty
    foreach ($s in 'Machine', 'User') { $x = [Environment]::GetEnvironmentVariable($v, $s)
        if ($x) { Write-Host "  WARNING: $v is set at $s scope -- remove it; the brief sets it per shell" -ForegroundColor Yellow } } }
Remove-Item Env:GOROOT, Env:GOFLAGS, Env:GODEBUG -ErrorAction SilentlyContinue; $env:GOTOOLCHAIN = 'local'
$go = Join-Path $GoRoot 'bin\go.exe'; $goRootOut = & $go env GOROOT
$c  = Get-PSDrive C; $cs = Get-CimInstance Win32_ComputerSystem
# Commit identity: STATUS only, never the value. The reference is R-LAPTOP's evidence commit 7462befde0.
$old = $ErrorActionPreference; $ErrorActionPreference = 'Continue'
try {
    $fleetId = @(& $git -C $repo log -1 '--format=%an%n%ae' 7462befde0)
    $myId    = @((& $git config --global user.name), (& $git config --global user.email))
    $useOnly = & $git config --global user.useConfigOnly
} finally { $ErrorActionPreference = $old }
$identity = if ($fleetId.Count -ne 2)                { 'UNKNOWN: 7462befde0 is not in the clone -- tell COORD' }
            elseif (-not $myId[0] -or -not $myId[1]) { 'NOT SET -- owner act, AZ1-brief.md A.2' }
            elseif (($myId -join "`n") -ceq ($fleetId -join "`n")) { 'matches the fleet identity (7462befde0)' }
            else                                     { 'DIFFERS from the fleet identity -- fix before AZ1 starts' }
# Defender: an ELEVATED read, because a non-elevated session cannot see exclusions. COUNTS only, never paths.
$rtp = try { (Get-MpComputerStatus -ErrorAction Stop).RealTimeProtectionEnabled } catch { 'unreadable' }
$exc = try { $mp = Get-MpPreference -ErrorAction Stop
             'paths {0}, processes {1}, extensions {2} (counts only)' -f @($mp.ExclusionPath | Where-Object { $_ }).Count,
                 @($mp.ExclusionProcess | Where-Object { $_ }).Count, @($mp.ExclusionExtension | Where-Object { $_ }).Count } catch { 'unreadable' }
$rows = [ordered]@{
    'go version'            = (& $go version)
    'go env GOROOT'         = "$goRootOut   $(if ($goRootOut -ceq $GoRoot) { '(OK: -GoRoot spelling)' } else { '(MISMATCH)' })"
    'go env GOFLAGS'        = "'$(& $go env GOFLAGS)'   (must be empty)"
    'dotnet --list-sdks'    = ((& $dotnet --list-sdks) -join '; ')
    'dotnet runtimes'       = ((& $dotnet --list-runtimes | ForEach-Object { ($_ -split ' \[')[0] }) -join '; ')
    'pwsh --version'        = (& $pwsh --version)
    'git --version'         = (& $git --version)
    'core.autocrlf(system)' = "$(& $git config --system core.autocrlf)   (fleet = true)"
    'clone HEAD'            = "$(& $git -C $repo rev-parse --abbrev-ref HEAD) $(& $git -C $repo rev-parse --short=10 HEAD)"
    'lane trees'            = "$laneTrees   (0 on a first run; setup skipped fetch/ownership otherwise)"
    'user.useConfigOnly'    = "$useOnly   (must be true)"
    'commit identity'       = $identity
    'Defender real-time'    = $rtp
    'Defender exclusions'   = $exc
    'free disk C:'          = '{0:N1} GB free of {1:N1} GB' -f ($c.Free / 1GB), (($c.Used + $c.Free) / 1GB)
    'logical CPUs'          = [Environment]::ProcessorCount
    'CPU model'             = (Get-CimInstance Win32_Processor | Select-Object -First 1).Name.Trim()
    'RAM'                   = '{0:N1} GiB' -f ($cs.TotalPhysicalMemory / 1GB)
    'LongPathsEnabled'      = (Get-ItemProperty $fs).LongPathsEnabled
    'time zone'             = (Get-TimeZone).Id
}
$rows.GetEnumerator() | ForEach-Object { Write-Host ('  {0,-22}: {1}' -f $_.Key, $_.Value) }
