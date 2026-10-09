# hashset v1.0.0: the owner sheet (runbook sections 3 and 4)

Written by C2 on 2026-10-09 for the owner to paste on the i7. It is the one owner sheet for publishing
`github.com/ritchiecarroll/hashset` v1.0.0: `docs/NugetgoPublish.md` section 3 (the int.nugettest.org rehearsal) and
section 4 (the conversion repository, signing, and the nuget.org push). The registry row and the section 6 demo
follow on COORD's side after the owner reports.

Section 3 has two paths: **PATH A** runs the rehearsal, **PATH B** records COORD's waiver of it. Section 4 runs only
after COORD has ruled sections 1-3 green, by either path.

## What this branch holds

| Path | What it is |
|:--|:--|
| `nupkg/nugetgo.github.com.ritchiecarroll.hashset.1.0.0-int.1.nupkg` | the rehearsal package, unsigned, for int.nugettest.org only |
| `nupkg/nugetgo.github.com.ritchiecarroll.hashset.1.0.0.nupkg` | the release candidate, unsigned, for section 4 only; it never goes to int |
| `nupkg/SHA256SUMS` | both packages' SHA-256, as packed |
| `app/` | the sample program (`main.go`, `go.mod`, `go.sum`) and `go-run.out`, its `go run` output |
| `rehearsal/mappings.txt` | the one registry row, as a local mappings file (the registry has no row yet) |
| `rehearsal/nuget.config` | `nugetgo.*` from int.nugettest.org, everything else from nuget.org |
| `conversion/github.com/ritchiecarroll/hashset/` | the first commit of the `hashset-cs` repository (section 4.0) |

Both packages were packed at `T` = `claude/c2-nugetgo-tools` @ `7888e4e72f`, against the published go.* 1.24.13.5,
with `-platforms linux/amd64`. Section 1: 37 matched, 0 disclosed, 5 Example declarations excluded; input digest
`sha256-7fb2058d54e18d61eaa288b0e79c847c953bdadf87cdd00fdb0d832750903114`, the same in the proof and the packed project.

`conversion/` holds the whole of `PR/src/<module>` (for hashset the pack wrote no module `Directory.Build.targets`,
because hashset's LICENSE already names The go2cs Authors), plus five files: `PR`'s root `Directory.Build.props` and
`Directory.Build.targets`, without which the project does not restore (`GoStdLibVersion` is set there; NU1015 without
it), hashset's LICENSE verbatim, a README, and a `.gitignore` for the build output. Measured on 2026-10-09: a fresh copy
of `conversion/` builds with `dotnet build -c Release -p:GoStdLibVersion=1.24.13.5`, and the dll it builds carries the
same version and copyright attributes as the packed one.

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
$Work = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-hashset-int1'
$T    = '7888e4e72f'
$Kit  = 'claude/c2-nugetgo-hashset-1.0.0'
$Id   = 'nugetgo.github.com.ritchiecarroll.hashset'
$PV   = '1.0.0-int.1'
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
$proj = Get-Content -Raw "$Work\AR\src\example.com\hashsetdemo\example.com.hashsetdemo.csproj"
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

```powershell
$appProject = "$Work\AR\src\example.com\hashsetdemo"
Copy-Item "$Work\kit\rehearsal\nuget.config" "$appProject\nuget.config"
$env:NUGET_PACKAGES = "$Work\packages"
$p = Start-Process dotnet -ArgumentList 'run', '-c', 'Release' -WorkingDirectory $appProject -NoNewWindow -Wait -PassThru `
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

- The rehearsal package (`1.0.0-int.1`, SHA-256 `2302be7b…4ebae`) was placed in a local folder feed, and the sample
  program was converted with `-nuget-map-feed <folder>` and the same mappings row. The report read
  `1 mapped, 1 referenced as packages` and `PackageReference nugetgo.github.com.ritchiecarroll.hashset [1.0.0-int.1]`;
  the lock pinned `1.0.0-int.1` with the mapping source `../mappings.txt`; `AR/src` held only `example.com`, and
  there was no `AR/pkg`.
- Restore and run used a private `NUGET_PACKAGES` and a nuget.config mapping `nugetgo.*` to the folder and everything
  else to nuget.org. go.lib 1.24.13.5 came from nuget.org and the hashset package from the folder. `dotnet run`
  printed exactly what `go run` printed (6 lines, `app/go-run.out`).

What the waiver gives up, stated so that it is a decision and not an omission:

- The converter's package selection was not run against an HTTP V3 feed for a `nugetgo.*` package. A folder feed
  takes a different code path (`nugetFolderFeedVersions`) from the flat-container read a gallery uses.
- The run did not see the bytes a gallery serves, which can carry a repository signature the packed bytes do not.
- The first run of both is then section 6's demo, against nuget.org, after the package is published and cannot be
  deleted. Section 6 already reruns this same program, so a defect would be found there, but only after publication.

## Section 4: publish to nuget.org (PERMANENT)

Run this only after COORD has ruled sections 1-3 green, by PATH A or PATH B. **A version pushed to nuget.org is
permanent.** It can be unlisted, but it can never be deleted or replaced, and `nugetgo.github.com.ritchiecarroll.hashset`
1.0.0 can never be pushed again with other bytes. Every block below stops with a `throw` before the push if a gate
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
$Pub    = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-hashset-publish'
$Kit    = 'claude/c2-nugetgo-hashset-1.0.0'
$Master = '18f9c58186'
$Id     = 'nugetgo.github.com.ritchiecarroll.hashset'
$PV     = '1.0.0'
$Org    = 'https://api.nuget.org/v3/index.json'
$CsRepo = 'ritchiecarroll/hashset-cs'
if (Test-Path $Pub) { throw "$Pub already exists: remove it or choose another folder" }
New-Item -ItemType Directory $Pub | Out-Null

git -C $Repo fetch origin master $Kit
if ($LASTEXITCODE) { throw "git fetch exited $LASTEXITCODE" }
git -C $Repo worktree add --detach "$Pub\kit" "origin/$Kit"
if ($LASTEXITCODE) { throw "worktree kit exited $LASTEXITCODE" }
git -C $Repo worktree add --detach "$Pub\signer" $Master
if ($LASTEXITCODE) { throw "worktree signer exited $LASTEXITCODE" }
```

### 4.0 Create the conversion repository and push its first commit

First create `github.com/ritchiecarroll/hashset-cs` as an EMPTY public repository: in the GitHub UI (owner
ritchiecarroll, name hashset-cs, Public, and no README, license or .gitignore, because the first commit brings them),
or with `gh repo create ritchiecarroll/hashset-cs --public`. The package's `RepositoryUrl` already names it.

The block checks that the files build on their own, commits them, pushes, and reads the repository back anonymously
to confirm it is public. git signs the commit if your configuration sets `commit.gpgsign`; add `-S` to the commit
line to sign it regardless.

```powershell
$Cs = "$Pub\hashset-cs"
Copy-Item -Recurse "$Pub\kit\conversion\github.com\ritchiecarroll\hashset" $Cs
if (-not (Test-Path "$Cs\.gitignore")) { throw '.gitignore was not copied' }
Push-Location $Cs
dotnet build -c Release -p:GoStdLibVersion=1.24.13.5
$rc = $LASTEXITCODE
Pop-Location
if ($rc) { throw "the conversion source does not build (dotnet build exited $rc)" }

git -C $Cs init -b main
git -C $Cs add .
git -C $Cs status --short
git -C $Cs commit -m 'hashset v1.0.0, converted by go2cs against go.* 1.24.13.5'
if ($LASTEXITCODE) { throw "git commit exited $LASTEXITCODE" }
git -C $Cs remote add origin "https://github.com/$CsRepo.git"
git -C $Cs push -u origin main
if ($LASTEXITCODE) { throw "git push exited $LASTEXITCODE" }
try { $visible = Invoke-RestMethod "https://api.github.com/repos/$CsRepo" }
catch { throw "$CsRepo cannot be read anonymously: make it public (runbook 0.6)" }
if ($visible.private) { throw "$CsRepo is private: make it public (runbook 0.6)" }
"public: $($visible.html_url) @ $(git -C $Cs rev-parse --short=10 HEAD)"
```

`git status --short` lists 11 files: the two generated `.cs` files, the csproj and slnx, the two icons, the two
`Directory.Build` files, LICENSE, README.md and .gitignore. The build output is under `.artifacts\`, which `.gitignore`
excludes.

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

The block first checks that nuget.org holds no package with this ID (runbook 0.5), then asks you to type `publish`,
then reads the key from a prompt. The key handling is the same as 3.1: the key is never typed on a command line and
does not enter PSReadLine history; while `dotnet nuget push` runs, it is in that process's argument list, as with
`release-nuget.ps1`; the variable is removed when the push ends.

`release-nuget.ps1` adds `--skip-duplicate` because a re-run of a 300-package release has to pass over the packages
already sent. It is left out here on purpose: the ID was just shown to be free, so a duplicate would mean something is
wrong, and the push should fail on it rather than skip it.

```powershell
$probe = Invoke-WebRequest "https://api.nuget.org/v3-flatcontainer/$($Id.ToLowerInvariant())/index.json" -SkipHttpErrorCheck
if ($probe.StatusCode -ne 404) { throw "$Id is not free on nuget.org (HTTP $($probe.StatusCode)): stop and tell COORD" }
Write-Host "About to push $file (sha256 $signedHash) to nuget.org." -ForegroundColor Yellow
Write-Host 'This is PERMANENT: a pushed version can be unlisted, never deleted or replaced.' -ForegroundColor Yellow
if ((Read-Host "Type 'publish' to push") -cne 'publish') { throw 'Not published. Nothing was sent.' }

$secure = Read-Host -AsSecureString 'nuget.org API key'
$env:NUGETGO_ORG_KEY = [Net.NetworkCredential]::new('', $secure).Password
try {
    dotnet nuget push "$SignDir\$file" --source $Org --api-key $env:NUGETGO_ORG_KEY
    $rc = $LASTEXITCODE
}
finally {
    Remove-Item Env:NUGETGO_ORG_KEY -ErrorAction SilentlyContinue
    $secure.Dispose()
}
if ($rc) { throw "dotnet nuget push exited $rc. Do not re-sign or re-pack; tell COORD." }
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

Send COORD one reply with: the `public:` line from 4.0; the `checksum OK` line and the `signed and verified` line from
4.1; the `pushed:` line from 4.2; and the `listed on nuget.org` line from 4.3. Do not send the build or signer output
itself: it contains local paths.

Then stop. The registry row #1 pull request on the nugetgo repository and the section 6 demo are COORD's.

Keep `$Pub\sign` (the signed package as pushed) until COORD confirms the row. To clean up after that:
`git -C $Repo worktree remove "$Pub\kit"`, the same for `"$Pub\signer"`, then delete `$Pub`.
