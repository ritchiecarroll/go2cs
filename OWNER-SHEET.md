# uuid v1.6.0: the owner sheet (runbook sections 3 and 4)

Written by C2 on 2026-10-09 for the owner to paste on the i7. It is the owner sheet for publishing
`github.com/google/uuid` v1.6.0 as `nugetgo.github.com.google.uuid`: `docs/NugetgoPublish.md` section 3 (the
int.nugettest.org rehearsal) and section 4 (signing and the nuget.org push).

**Do not start it until COORD says so.** uuid goes to nuget.org only after hashset is listed there and hashset's
registry row has landed (the first-wave plan, section 1). The conversion commit on `github.com/ritchiecarroll/uuid-cs`
is COORD's (runbook 4.5): that repository already exists, and COORD holds its clone. The registry row and the section
6 demo also follow on COORD's side.

Section 3 has two paths: **PATH A** runs the rehearsal, **PATH B** records COORD's waiver of it. Section 4 runs only
after COORD has ruled sections 1-3 green, by either path.

## Before the first block, in every new window

1. The i7's default shell is Windows PowerShell 5.1. This sheet needs PowerShell 7, which is installed as a .NET
   global tool. Start it FIRST, from the 5.1 window and before the environment block:
   `& "$env:USERPROFILE\.dotnet\tools\pwsh.exe"`. Once the environment block has put .NET 10 first on `PATH`, that
   pwsh no longer starts.
2. Confirm that `$PSVersionTable.PSVersion` reads 7.x.
3. `Set-Location` to the root of your go2cs clone.
4. **Stop at the first red line.** A `throw` stops only the statement it is in: the lines pasted after it keep
   running. Read each block's output before you paste the next, and send COORD the first red line instead.

## What this branch holds

| Path | What it is |
|:--|:--|
| `nupkg/nugetgo.github.com.google.uuid.1.6.0-int.1.nupkg` | the rehearsal package, unsigned, for int.nugettest.org only |
| `nupkg/nugetgo.github.com.google.uuid.1.6.0.nupkg` | the release candidate, unsigned, for section 4 only; it never goes to int |
| `nupkg/SHA256SUMS` | both packages' SHA-256, as packed |
| `preview/README.md` | the package README both packages carry (their README on nuget.org), as text |
| `preview/README.png` | that README rendered on a dark page (the badges drawn locally: this box cannot reach img.shields.io) |
| `app/` | the sample program (`main.go`, `go.mod`, `go.sum`) and `go-run.out`, its `go run` output |
| `rehearsal/mappings.txt` | the one registry row (`community`), as a local mappings file |
| `rehearsal/nuget.config` | `nugetgo.*` from int.nugettest.org, everything else from nuget.org |
| `conversion/` | for COORD: the files of the conversion commit on `uuid-cs`, in the nested layout; the upstream LICENSE, VALIDATION.md and the proof page(s) sit at its root |
| `disclosures/` | the notes-only module disclosure manifest that put the `TestVersion6` note on the proof page |
| `evidence/v6probe/` | the Go probe that shows the upstream `TestVersion6` race deterministically |
| `ROW-PR.md` | draft text for the registry row's pull request |

Both packages were packed at `T` = `claude/c2-nugetgo-tools` @ `c8ef680e14`, against the published go.* 1.24.13.5,
with `-platforms linux/amd64`. Section 1: 54 matched, 0 disclosed, 18 Benchmark and Fuzz declarations excluded; input
digest `sha256-1d40aaa871f4e430ac651e70e05593f11e9ed13539146378156e81f13af51b28`, the same in the proof and the packed
project. The first `-tests` run read one red, `TestVersion6`, an upstream test race that native Go reproduces; COORD
ruled it cited, not disclosed (`ROW-PR.md` and the first-wave plan give the evidence).

`T` is the tools that packed the first packages here plus the README and icon cut (owner, 2026-10-10), and the packs
add `-LicenseSpdx BSD-3-Clause`. Each package now carries a generated README.md as its nuget.org README, the go2cs icon,
and VALIDATION.md as a file the README links. Everything else is unchanged and was measured so: the packed assemblies
are byte-identical to the ones packed at `7888e4e72f`, and so are the description and VALIDATION.md.

## PATH A: run the rehearsal

Paste each block in order into one pwsh 7 window, started at the root of your go2cs clone. Each block stops with a
`throw` if its step fails. Nothing here needs elevation.

### 0. Environment and the two trees

The first seven lines put the .NET 10 SDK and go1.24.13 first on `PATH` for this window and read both back. The
machine-wide `dotnet` on the i7 does not have the .NET 10 SDK, so without them `dotnet build`, go2cs, `dotnet run` and
`dotnet nuget verify` would fail or use the wrong SDK. They change only this window.

```powershell
$env:DOTNET_ROOT = "$env:USERPROFILE\dotnet10"
$env:GOROOT = "$env:USERPROFILE\sdk\go1.24.13"
$env:GOTOOLCHAIN = 'local'
$env:MSBUILDDISABLENODEREUSE = '1'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
if ((dotnet --version) -notlike '10.*') { throw 'dotnet is not .NET 10: check the env block' }
if ((go version) -notmatch 'go1\.24\.13 ') { throw 'go is not go1.24.13: check the env block' }

$Repo = (git rev-parse --show-toplevel)
$Work = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-uuid-int1'
$T    = 'c8ef680e14'
$Kit  = 'claude/c2-nugetgo-uuid-1.6.0'
$Id   = 'nugetgo.github.com.google.uuid'
$PV   = '1.6.0-int.1'
$Int  = 'https://apiint.nugettest.org/v3/index.json'
if (Test-Path $Work) { throw "$Work already exists: remove it or choose another folder" }
New-Item -ItemType Directory $Work | Out-Null

git -C $Repo fetch origin claude/c2-nugetgo-tools $Kit
if ($LASTEXITCODE) { throw "git fetch exited $LASTEXITCODE" }
$tip = (git -C $Repo rev-parse origin/claude/c2-nugetgo-tools).Substring(0, 10)
if ($tip -ne $T) { throw "claude/c2-nugetgo-tools is at $tip, not ${T}: stop and tell COORD" }
git -C $Repo worktree add --detach "$Work\T" $T
if ($LASTEXITCODE) { throw "worktree T exited $LASTEXITCODE" }
git -C $Repo worktree add --detach "$Work\kit" "origin/$Kit"
if ($LASTEXITCODE) { throw "worktree kit exited $LASTEXITCODE" }

Get-Content "$Work\kit\nupkg\SHA256SUMS" | ForEach-Object {
    $want, $file = $_ -split '\s+', 2
    $got = (Get-FileHash -Algorithm SHA256 "$Work\kit\nupkg\$file").Hash.ToLowerInvariant()
    if ($got -ne $want) { throw "checksum mismatch: $file" }
    "checksum OK  $file"
}
```

### 3.1 Push the rehearsal package to int.nugettest.org

The key is read from a prompt. It is not typed on a command line, and PSReadLine does not record `Read-Host` input
in its history. While `dotnet nuget push` runs, the key is in that process's argument list, where a process listing on
the i7 can read it; `release-nuget.ps1` passes the nuget.org key the same way. The block removes the variable when the
push ends, whether or not it succeeded.

```powershell
$secure = Read-Host -AsSecureString 'int.nugettest.org API key'
$env:NUGETGO_INT_KEY = [Net.NetworkCredential]::new('', $secure).Password
try {
    dotnet nuget push "$Work\kit\nupkg\$Id.$PV.nupkg" --source $Int --api-key $env:NUGETGO_INT_KEY
    $rc = $LASTEXITCODE
}
finally {
    Remove-Item Env:NUGETGO_INT_KEY -ErrorAction SilentlyContinue
    $secure.Dispose()
}
if ($rc) { throw "dotnet nuget push exited $rc" }
```

### 3.2 Wait until int's flat container lists the version

Checks once a minute for up to an hour.

```powershell
$base = ((Invoke-RestMethod $Int).resources |
    Where-Object { $_.'@type' -eq 'PackageBaseAddress/3.0.0' } | Select-Object -First 1).'@id'.TrimEnd('/')
$listed = $false
foreach ($i in 1..60) {
    try { $listed = @((Invoke-RestMethod "$base/$Id/index.json").versions) -contains $PV } catch { $listed = $false }
    if ($listed) { break }
    Start-Sleep -Seconds 60
}
if (-not $listed) { throw "$Id $PV is not listed on int after 60 minutes" }
"listed on int: $Id $PV"
```

### 3.3 Convert the sample program against int

```powershell
Push-Location "$Work\T\src\go2cs"
go build -o "$Work\bin\go2cs.exe" .
$rc = $LASTEXITCODE
Pop-Location
if ($rc) { throw "go build exited $rc" }

Copy-Item -Recurse "$Work\kit\app" "$Work\app"
Copy-Item "$Work\kit\rehearsal\mappings.txt" "$Work\mappings.txt"
& "$Work\bin\go2cs.exe" -recurse=nuget -nuget-map "$Work\mappings.txt" -nuget-map-only -nuget-map-feed $Int `
    -go2cspath "$Work\T\src" "$Work\app" "$Work\AR" *> "$Work\AR.log"
$rc = $LASTEXITCODE
Get-Content "$Work\AR.log" | Select-Object -Last 12
if ($rc) { throw "go2cs exited $rc" }
```

### 3.4 Gate: the consumer half

```powershell
$log  = Get-Content -Raw "$Work\AR.log"
$lock = Get-Content -Raw "$Work\AR\go2cs.nuget.lock"
$proj = Get-Content -Raw "$Work\AR\src\example.com\uuiddemo\example.com.uuiddemo.csproj"
$gates = [ordered]@{
    'report reads 1 referenced as packages'      = $log -match '1 mapped, 1 referenced as packages'
    "PackageReference $Id [$PV]"                 = $proj -match [regex]::Escape("Include=""$Id"" Version=""[$PV]""")
    "lock pins $PV"                              = $lock -match "\t$([regex]::Escape($PV))\t"
    'lock records the mapping source relatively' = $lock -match '\t\.\.[\\/]mappings\.txt\t'
    'module not converted locally'               = -not (Test-Path "$Work\AR\src\github.com") -and -not (Test-Path "$Work\AR\pkg")
}
$gates.GetEnumerator() | ForEach-Object { '{0}  {1}' -f $(if ($_.Value) { 'PASS' } else { 'FAIL' }), $_.Key }
if ($gates.Values -contains $false) { throw 'a 3.4 gate failed' }
```

### 3.5 Gate: the program

`Start-Process` hands each program a file as its standard output, so both outputs are compared as the bytes each
program wrote.

`-p:UseSharedCompilation=false` keeps the build from starting the Roslyn compiler server. On Windows,
`Start-Process -Wait` waits for every process the command starts, and that server stays alive for minutes after a
build: the hashset run on the i7 waited about 10 minutes without it.

```powershell
$appProject = "$Work\AR\src\example.com\uuiddemo"
Copy-Item "$Work\kit\rehearsal\nuget.config" "$appProject\nuget.config"
$env:NUGET_PACKAGES = "$Work\packages"
$p = Start-Process dotnet -ArgumentList 'run', '-c', 'Release', '-p:UseSharedCompilation=false' -WorkingDirectory $appProject -NoNewWindow -Wait -PassThru `
    -RedirectStandardOutput "$Work\dotnet-run.out" -RedirectStandardError "$Work\dotnet-run.err"
if ($p.ExitCode) { throw "dotnet run exited $($p.ExitCode) (see $Work\dotnet-run.err)" }
$p = Start-Process go -ArgumentList 'run', '.' -WorkingDirectory "$Work\app" -NoNewWindow -Wait -PassThru `
    -RedirectStandardOutput "$Work\go-run.out" -RedirectStandardError "$Work\go-run.err"
if ($p.ExitCode) { throw "go run exited $($p.ExitCode) (see $Work\go-run.err)" }
Remove-Item Env:NUGET_PACKAGES

$hash = @{}
foreach ($f in "$Work\go-run.out", "$Work\dotnet-run.out", "$Work\kit\app\go-run.out") {
    $hash[$f] = (Get-FileHash -Algorithm SHA256 $f).Hash
    '{0}  {1}' -f $hash[$f], $f
}
if (@($hash.Values | Select-Object -Unique).Count -ne 1) { throw 'the three outputs differ' }
'3.5 PASS: dotnet run prints exactly what go run prints'
Get-ChildItem "$Work\packages" -Directory | Where-Object Name -match '^(go\.lib|nugetgo\.)' |
    ForEach-Object { '{0}  {1}' -f $_.Name, ((Get-ChildItem $_.FullName -Directory).Name -join ', ') }
```

### 3.6 What to send COORD

One reply with: the `checksum OK` lines; the push result line; the `listed on int` line; the 3.4 PASS lines; the three
hashes from 3.5 and the `3.5 PASS` line; and the last two lines of 3.5 (which versions restored). The content hash in
`go2cs.nuget.lock` is expected to differ from C2's local run, because a gallery can add its own repository signature
to the bytes it serves. Do not send `AR.log` itself: it contains local paths.

To clean up afterwards: `git -C $Repo worktree remove "$Work\T"`, the same for `"$Work\kit"`, then delete `$Work`.

## PATH B: waive the int rehearsal

If COORD and the owner waive section 3, nothing on this sheet runs, and no int.nugettest.org account or key is needed.
The record then cites C2's local equivalent, run on 2026-10-09 on a cloud lane box at `T`:

- The rehearsal package (`1.6.0-int.1`, SHA-256 `99529e48…25e1`) was placed in a local folder feed, and the sample
  program was converted with `-nuget-map-feed <folder>` and the same mappings row. The report read
  `1 mapped, 1 referenced as packages`, the `community` TRUST line, and
  `PackageReference nugetgo.github.com.google.uuid [1.6.0-int.1]`; the lock pinned `1.6.0-int.1` with the mapping
  source `../mappings.txt`; `AR/src` held only `example.com`, and there was no `AR/pkg`.
- Restore and run used a private `NUGET_PACKAGES` and a nuget.config mapping `nugetgo.*` to the folder and everything
  else to nuget.org. go.lib 1.24.13.5 came from nuget.org and the uuid package from the folder. `dotnet run`
  printed exactly what `go run` printed (3 lines, `app/go-run.out`).

What the waiver gives up, stated so that it is a decision and not an omission:

- The converter's package selection was not run against an HTTP V3 feed for a `nugetgo.*` package. A folder feed
  takes a different code path (`nugetFolderFeedVersions`) from the flat-container read a gallery uses.
- The run did not see the bytes a gallery serves, which can carry a repository signature the packed bytes do not.
- The first run of both is then section 6's demo, against nuget.org, after the package is published and cannot be
  deleted. Section 6 already reruns this same program, so a defect would be found there, but only after publication.

## Section 4: publish to nuget.org (PERMANENT)

Run this only after COORD has ruled sections 1-3 green, by PATH A or PATH B. **A version pushed to nuget.org is
permanent.** It can be unlisted, but it can never be deleted or replaced, and `nugetgo.github.com.google.uuid`
1.6.0 can never be pushed again with other bytes. Every block below stops with a `throw` before the push if a gate
fails.

Paste the blocks in order into one pwsh 7 window, started at the root of your go2cs clone. Section 4 does not need
PATH A's folder. You need: the signing card inserted, `NuGetCertFingerprint` set as it is for `release-nuget.ps1`, and
a nuget.org API key whose glob covers this ID. A key scoped to `go.*` does not cover `nugetgo.*`; a 403 at 4.2 means
the key's scope, and nuget.org's API keys page sets it.

### 4.0a Environment and the two trees

The first seven lines are the same environment block as section 3's block 0, for the same reason.

```powershell
$env:DOTNET_ROOT = "$env:USERPROFILE\dotnet10"
$env:GOROOT = "$env:USERPROFILE\sdk\go1.24.13"
$env:GOTOOLCHAIN = 'local'
$env:MSBUILDDISABLENODEREUSE = '1'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
if ((dotnet --version) -notlike '10.*') { throw 'dotnet is not .NET 10: check the env block' }
if ((go version) -notmatch 'go1\.24\.13 ') { throw 'go is not go1.24.13: check the env block' }

$Repo   = (git rev-parse --show-toplevel)
$Pub    = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-uuid-publish'
$Kit    = 'claude/c2-nugetgo-uuid-1.6.0'
$Master = '18f9c58186'
$Id     = 'nugetgo.github.com.google.uuid'
$PV     = '1.6.0'
$Org    = 'https://api.nuget.org/v3/index.json'
if (Test-Path $Pub) { throw "$Pub already exists: remove it or choose another folder" }
New-Item -ItemType Directory $Pub | Out-Null

git -C $Repo fetch origin master $Kit
if ($LASTEXITCODE) { throw "git fetch exited $LASTEXITCODE" }
git -C $Repo worktree add --detach "$Pub\kit" "origin/$Kit"
if ($LASTEXITCODE) { throw "worktree kit exited $LASTEXITCODE" }
git -C $Repo worktree add --detach "$Pub\signer" $Master
if ($LASTEXITCODE) { throw "worktree signer exited $LASTEXITCODE" }
```

### 4.0 The conversion repository (COORD's step, not yours)

COORD pushes the conversion commit to `github.com/ritchiecarroll/uuid-cs` (the existing repository; runbook 4.5,
nested layout). The package's `RepositoryUrl` names it. Nothing in this section touches it.

The commit carries the upstream LICENSE, VALIDATION.md and the proof page(s) at the repository root (`conversion/` has
them there), because the package README links `VALIDATION.md` and `LICENSE` at that root. It should land before
4.2, so those links never fail.

### 4.1 The checksum gate, then sign and verify

`sign-nupkgs.ps1` signs every `.nupkg` in the folder it is given, so the release candidate is copied into a folder of
its own. The rehearsal package is never signed and never goes to nuget.org.

The two signer calls are `release-nuget.ps1`'s own, read from `src/release-nuget.ps1` and `src/sign-nupkgs.ps1` at
master `18f9c58186`. Its Phase 0 runs the signer without `-Apply` to prove the certificate is reachable, and its Phase 2
runs it with `-Apply`. Both run under Windows PowerShell exactly as `release-nuget.ps1` launches them. The PIN prompt
comes once, at `-Apply`. The verify is the step `sign-nupkgs.ps1` recommends after signing, with two of the SDK's own
options: `--all`, and `--certificate-fingerprint`, which fails unless the signer is the certificate
`NuGetCertFingerprint` names.

```powershell
$file = "$Id.$PV.nupkg"
$sums = (Get-Content "$Pub\kit\nupkg\SHA256SUMS") -match "\s$([regex]::Escape($file))$"
$want = if ($sums) { ($sums[0] -split '\s+')[0] } else { $null }
if (-not $want) { throw "SHA256SUMS has no line for $file" }
$SignDir = "$Pub\sign"
New-Item -ItemType Directory $SignDir | Out-Null
Copy-Item "$Pub\kit\nupkg\$file" $SignDir
$got = (Get-FileHash -Algorithm SHA256 "$SignDir\$file").Hash.ToLowerInvariant()
if ($got -ne $want) { throw "checksum mismatch: $file is not the package C2 packed" }
if (@(Get-ChildItem "$SignDir\*.nupkg").Count -ne 1) { throw "$SignDir must hold the release candidate alone" }
"checksum OK  $file (unsigned, as packed)"

if (-not $env:NuGetCertFingerprint) { throw 'NuGetCertFingerprint is not set: it is the variable release-nuget.ps1 and sign-nupkgs.ps1 read' }
$fingerprint = $env:NuGetCertFingerprint.Trim().Replace(' ', '').ToUpperInvariant()
$signer = "$Pub\signer\src\sign-nupkgs.ps1"
& powershell -NoProfile -ExecutionPolicy Bypass -File $signer -PackageDir $SignDir
if ($LASTEXITCODE) { throw "the signer's certificate check exited ${LASTEXITCODE}: is the card inserted?" }
& powershell -NoProfile -ExecutionPolicy Bypass -File $signer -PackageDir $SignDir -Apply
if ($LASTEXITCODE) { throw "signing exited $LASTEXITCODE; nothing was pushed" }
dotnet nuget verify --all "$SignDir\$file" --certificate-fingerprint $fingerprint
if ($LASTEXITCODE) { throw "dotnet nuget verify exited $LASTEXITCODE; do not push" }
$signedHash = (Get-FileHash -Algorithm SHA256 "$SignDir\$file").Hash.ToLowerInvariant()
"signed and verified  $file  sha256 $signedHash"
```

### 4.2 Push to nuget.org (the permanent step)

The block first checks that nuget.org holds no package with this ID (runbook 0.5), then asks you to type `publish`.
The key comes from `$env:NUGETGO_API_KEY` when that is set, and from a prompt when it is empty. Either way it is never
printed, never typed on a command line and never in PSReadLine history; while `dotnet nuget push` runs, it is in that
process's argument list, as with `release-nuget.ps1`; the block's copy is removed when the push ends. Your
`$env:NUGETGO_API_KEY` is read, not changed. The key's glob must cover `nugetgo.*`.

`release-nuget.ps1` adds `--skip-duplicate` because a re-run of a 300-package release has to pass over the packages
already sent. It is left out here on purpose: the ID was just shown to be free, so a duplicate would mean something is
wrong, and the push should fail on it rather than skip it.

```powershell
$probe = Invoke-WebRequest "https://api.nuget.org/v3-flatcontainer/$($Id.ToLowerInvariant())/index.json" -SkipHttpErrorCheck
if ($probe.StatusCode -ne 404) { throw "$Id is not free on nuget.org (HTTP $($probe.StatusCode)): stop and tell COORD" }
Write-Host "About to push $file (sha256 $signedHash) to nuget.org." -ForegroundColor Yellow
Write-Host 'This is PERMANENT: a pushed version can be unlisted, never deleted or replaced.' -ForegroundColor Yellow
if ((Read-Host "Type 'publish' to push") -cne 'publish') { throw 'Not published. Nothing was sent.' }

if ($env:NUGETGO_API_KEY) { $key = $env:NUGETGO_API_KEY; 'key: from $env:NUGETGO_API_KEY' }
else { $secure = Read-Host -AsSecureString 'nuget.org API key (nugetgo.*)'; $key = [Net.NetworkCredential]::new('', $secure).Password; $secure.Dispose(); 'key: from the prompt' }
try {
    dotnet nuget push "$SignDir\$file" --source $Org --api-key $key
    $rc = $LASTEXITCODE
}
finally { Remove-Variable key -ErrorAction SilentlyContinue }
if ($rc) { throw "dotnet nuget push exited $rc (a 403 in its output means the key's glob does not cover nugetgo.*). Do not re-sign or re-pack; tell COORD." }
"pushed: $Id $PV"
```

### 4.3 Wait until nuget.org lists the version

nuget.org validates a package before listing it. The block checks the flat container and the registration (runbook
4.4) once a minute for up to 90 minutes.

```powershell
$index = Invoke-RestMethod $Org
$flat = ($index.resources | Where-Object { $_.'@type' -eq 'PackageBaseAddress/3.0.0' } | Select-Object -First 1).'@id'.TrimEnd('/')
$reg  = ($index.resources | Where-Object { $_.'@type' -eq 'RegistrationsBaseUrl/3.6.0' } | Select-Object -First 1).'@id'.TrimEnd('/')
$lower = $Id.ToLowerInvariant()
$listed = $false
foreach ($i in 1..90) {
    try { $inFlat = @((Invoke-RestMethod "$flat/$lower/index.json").versions) -contains $PV } catch { $inFlat = $false }
    $inReg = $false
    try {
        foreach ($page in @((Invoke-RestMethod "$reg/$lower/index.json").items)) {
            $leaves = if ($page.items) { $page.items } else { (Invoke-RestMethod $page.'@id').items }
            if (@($leaves.catalogEntry.version) -contains $PV) { $inReg = $true }
        }
    }
    catch { $inReg = $false }
    if ($inFlat -and $inReg) { $listed = $true; break }
    Start-Sleep -Seconds 60
}
if (-not $listed) { throw "$Id $PV is not listed after 90 minutes (flat container $inFlat, registration $inReg): tell COORD; do not push again" }
"listed on nuget.org: $Id $PV (flat container and registration)"
```

### 4.4 Stop and report to COORD

Send COORD one reply with: the `checksum OK` line and the `signed and verified` line from
4.1; the `pushed:` line from 4.2; and the `listed on nuget.org` line from 4.3. Do not send the build or signer output
itself: it contains local paths.

Then stop. The registry row #1 pull request on the nugetgo repository and the section 6 demo are COORD's.

Keep `$Pub\sign` (the signed package as pushed) until COORD confirms the row. To clean up after that:
`git -C $Repo worktree remove "$Pub\kit"`, the same for `"$Pub\signer"`, then delete `$Pub`.
