# github.com/inconshreveable/mousetrap v1.1.0: the owner sheet (runbook sections 3 and 4) -- DRAFT

**DRAFT, prepared by C2 on 2026-10-14 for the post-hop wave; not runnable yet.** It was drafted at master
`7ca22d951f` against go.* 1.24.13.7 so that the post-hop pack is a re-pack, not new work. Every value marked
`<...>` is filled when the kit is re-packed against the go.* 1.26.x record: `<T>` (that record's commit), `<R>` (the
release), `<GO>` and `<GO-REGEX>` (the Go toolchain the i7 runs then), `<SIGNER>` (the master commit whose
`sign-nupkgs.ps1` signs), and the two packages' hashes. No package in this draft is a release candidate.

It is the owner sheet for publishing `github.com/inconshreveable/mousetrap` v1.1.0 as
`nugetgo.github.com.inconshreveable.mousetrap`: `docs/NugetgoPublish.md` section 3 and section 4. mousetrap is the
second rung of cobra's ladder (pflag, then mousetrap, then cobra). Its conversion repository is
`github.com/ritchiecarroll/mousetrap-cs` (to be created by COORD, in the form of `pflag-cs`).

**How it differs from the pflag sheet.** mousetrap has no Go tests upstream, so its proof is behavioral (COORD
ruling 2026-10-12): one row per shipped RID, each written on a hosted runner of that OS by the OS matrix's
`nugetgo-behavior` stage, against the REHEARSAL package on int.nugettest.org. So:
- Section 3 is yours only for the int push (3.1) and the listing wait (3.2). The sample conversion and the program
  gates (pflag's 3.3 to 3.5) are the lane's hosted dispatch (3.3 below), four times, once per RID.
- **There is no PATH B.** The hosted legs restore the rehearsal package from int.nugettest.org, so a waiver of int
  leaves the release candidate with no rows, and the pack refuses a release without them.
- The release candidate is packed AFTER the rows exist (`-BehaviorProofDir`), so it reaches you in a second kit push,
  after 3.3, and section 4 waits for COORD's word that it is there.
- The package is RID-specific: `lib/net10.0` carries the windows assembly, and `runtimes/<rid>/lib/net10.0` carries
  win-x64, linux-x64, osx-x64 and osx-arm64. trap_windows.go walks the process snapshot; trap_others.go returns false.

## Before the first block, in every new window

As the pflag sheet: start pwsh 7 from the 5.1 window first, confirm 7.x, `Set-Location` to your go2cs clone, and
stop at the first red line.

## What the kit branch will hold

| Path | What it is |
|:--|:--|
| `nupkg/int.1/` | the rehearsal package (1.1.0-int.1), unsigned, for int.nugettest.org only |
| `nupkg/release/` | the release candidate (1.1.0), unsigned, packed after the rows; it never goes to int |
| `nupkg/SHA256SUMS` | the packages' SHA-256, as packed |
| `rows/` | the four behavioral rows (`win-x64.md`, `linux-x64.md`, `osx-x64.md`, `osx-arm64.md`) fetched from the dispatch |
| `preview/README.md`, `preview/README.png` | the package README, as text and rendered |
| `evidence/` | `Test-ChecksumGate.ps1` and `Test-SheetPaste.ps1`, as in the pflag kit |
| `conversion/` | for COORD: the conversion commit's files, nested layout, LICENSE and VALIDATION.md at the root |
| `ROW-PR.md` | draft text for the registry row's pull request |

## Section 3: the rehearsal (int.nugettest.org)

### 0. Environment and the kit

```powershell
$Repo = (git rev-parse --show-toplevel)
$Work = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-mousetrap-int1'
$Kit  = 'claude/c2-nugetgo-mousetrap-1.1.0'
$Id   = 'nugetgo.github.com.inconshreveable.mousetrap'
$PV   = '1.1.0-int.1'
$Int  = 'https://apiint.nugettest.org/v3/index.json'
if (Test-Path $Work) { throw "$Work already exists: remove it or choose another folder" }
New-Item -ItemType Directory $Work | Out-Null

git -C $Repo fetch origin $Kit
if ($LASTEXITCODE) { throw "git fetch exited $LASTEXITCODE" }
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
    dotnet nuget push "$Work\kit\nupkg\int.1\$Id.nupkg" --source $Int --api-key $env:NUGETGO_INT_KEY
    $rc = $LASTEXITCODE
} finally {
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

### 3.3 The hosted rows (the lane's step, not yours)

Send COORD the `listed on int` line. The lane then dispatches the OS matrix's `nugetgo-behavior` stage on `<T>`'s
branch with `nugetgo_module=github.com/inconshreveable/mousetrap@v1.1.0` and `nugetgo_package=1.1.0-int.1`, fetches
the four rows (`Get-NugetgoBehaviorRows.ps1`), gates them (runbook 3, *The hosted rows*), packs the release candidate
with `-BehaviorProofDir`, and pushes it to the kit branch. Nothing in section 4 runs before COORD says the release
candidate is there.

To clean up afterwards: `git -C $Repo worktree remove "$Work\kit"`, then delete `$Work`.

## Section 4: publish to nuget.org (PERMANENT)

Run this only after COORD has ruled sections 1-3 green and the release candidate (packed with the four rows) is on the kit branch. **A version pushed to nuget.org is
permanent.** It can be unlisted, but it can never be deleted or replaced, and `nugetgo.github.com.inconshreveable.mousetrap`
1.1.0 can never be pushed again with other bytes. Every block below stops with a `throw` before the push if a gate
fails.

Paste the blocks in order into one pwsh 7 window, started at the root of your go2cs clone. Section 4 does not need
section 3's folder. You need: the signing card inserted, `NuGetCertFingerprint` set as it is for `release-nuget.ps1`, and
a nuget.org API key whose glob covers this ID. A key scoped to `go.*` does not cover `nugetgo.*`; a 403 at 4.2 means
the key's scope, and nuget.org's API keys page sets it.

### 4.0a Environment and the two trees

The first seven lines are the same environment block as section 3's block 0, for the same reason.

```powershell
$env:DOTNET_ROOT = "$env:USERPROFILE\dotnet10"
$env:GOROOT = "$env:USERPROFILE\sdk\<GO>"
$env:GOTOOLCHAIN = 'local'
$env:MSBUILDDISABLENODEREUSE = '1'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
if ((dotnet --version) -notlike '10.*') { throw 'dotnet is not .NET 10: check the env block' }
if ((go version) -notmatch '<GO-REGEX> ') { throw 'go is not <GO>: check the env block' }

$Repo   = (git rev-parse --show-toplevel)
$Pub    = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-mousetrap-publish'
$Kit    = 'claude/c2-nugetgo-mousetrap-1.1.0'
$Master = '<SIGNER>'
$Id     = 'nugetgo.github.com.inconshreveable.mousetrap'
$PV     = '1.1.0'
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

COORD pushes the conversion commit to `github.com/ritchiecarroll/mousetrap-cs`, the repository COORD creates for it
(runbook 4.5, nested layout). The package's `RepositoryUrl` names it. Nothing in this section touches it.

The commit carries the upstream LICENSE, VALIDATION.md and the proof page(s) at the repository root (`conversion/` has
them there), because the package README links `VALIDATION.md` and `LICENSE` at that root, at the tag `nuget-$PV`.
COORD pushes the commit and mints the signed tag on it before you start section 4; 4.0b checks both.

### 4.0b Gate: the conversion repository's tag

The tag `nuget-$PV` on `ritchiecarroll/mousetrap-cs` names the conversion commit the package README links (COORD ruling 2026-10-10:
a tag, never `HEAD`). COORD pushes that commit and mints the signed tag before you start; this block checks, as an
anonymous reader (no credentials), that the tag exists, that VALIDATION.md and the license file resolve at it, and
that VALIDATION.md is byte for byte the one the package carries. It changes nothing.

```powershell
$CsRepo = 'ritchiecarroll/mousetrap-cs'
$Tag = "nuget-$PV"
$env:GIT_TERMINAL_PROMPT = '0'
$refs = @(git -c credential.helper= ls-remote "https://github.com/$CsRepo.git" "refs/tags/$Tag" "refs/tags/$Tag^{}")
if ($LASTEXITCODE) { throw "git ls-remote exited ${LASTEXITCODE}: $CsRepo cannot be read anonymously" }
if (-not $refs) { throw "$CsRepo has no tag ${Tag}: COORD pushes it before this sheet; stop and tell COORD" }
$commit = (@($refs | Where-Object { $_ -like '*^{}' }) + $refs)[0].Split("`t")[0]
foreach ($f in 'VALIDATION.md', 'LICENSE') {
    try { Invoke-WebRequest "https://raw.githubusercontent.com/$CsRepo/$Tag/$f" -OutFile "$Pub\tag-$f" } catch { throw "$f does not resolve anonymously at $CsRepo ${Tag}: stop and tell COORD" }
}
if ((Get-FileHash "$Pub\tag-VALIDATION.md").Hash -ne (Get-FileHash "$Pub\kit\conversion\VALIDATION.md").Hash) { throw "VALIDATION.md at $Tag is not the one the package carries: stop and tell COORD" }
"tag OK  $CsRepo $Tag -> commit $($commit.Substring(0, 10)); VALIDATION.md and LICENSE resolve anonymously"
```

### 4.1 The checksum gate, then sign and verify

`sign-nupkgs.ps1` signs every `.nupkg` in the folder it is given, so the release candidate is copied into a folder of
its own. The rehearsal package is never signed and never goes to nuget.org.

The two signer calls are `release-nuget.ps1`'s own, read from `src/release-nuget.ps1` and `src/sign-nupkgs.ps1` at
master `<SIGNER>`. Its Phase 0 runs the signer without `-Apply` to prove the certificate is reachable, and its Phase 2
runs it with `-Apply`. Both run under Windows PowerShell exactly as `release-nuget.ps1` launches them. The PIN prompt
comes once, at `-Apply`. The verify is the step `sign-nupkgs.ps1` recommends after signing, with two of the SDK's own
options: `--all`, and `--certificate-fingerprint`, which fails unless the signer is the certificate
`NuGetCertFingerprint` names.

```powershell
$file = "$Id.nupkg"
$sums = @(Get-Content "$Pub\kit\nupkg\SHA256SUMS") -match "\srelease/$([regex]::Escape($file))$"
$want = if ($sums) { ($sums[0] -split '\s+')[0] } else { $null }
if (-not $want) { throw "SHA256SUMS has no line for $file" }
$SignDir = "$Pub\sign"
New-Item -ItemType Directory $SignDir | Out-Null
Copy-Item "$Pub\kit\nupkg\release\$file" $SignDir
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

if ($env:NUGETGO_API_KEY) { $key = $env:NUGETGO_API_KEY; 'key: from $env:NUGETGO_API_KEY' } else { $secure = Read-Host -AsSecureString 'nuget.org API key (nugetgo.*)'; $key = [Net.NetworkCredential]::new('', $secure).Password; $secure.Dispose(); 'key: from the prompt' }
try {
    dotnet nuget push "$SignDir\$file" --source $Org --api-key $key
    $rc = $LASTEXITCODE
} finally { Remove-Variable key -ErrorAction SilentlyContinue }
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
    } catch { $inReg = $false }
    if ($inFlat -and $inReg) { $listed = $true; break }
    Start-Sleep -Seconds 60
}
if (-not $listed) { throw "$Id $PV is not listed after 90 minutes (flat container $inFlat, registration $inReg): tell COORD; do not push again" }
"listed on nuget.org: $Id $PV (flat container and registration)"
```

### 4.4 Stop and report to COORD

Send COORD one reply with: the `tag OK` line from 4.0b; the `checksum OK` line and the `signed and verified` line from
4.1; the `pushed:` line from 4.2; and the `listed on nuget.org` line from 4.3. Do not send the build or signer output
itself: it contains local paths.

Then stop. The registry row's pull request on the nugetgo repository and the section 6 demo are COORD's.

Keep `$Pub\sign` (the signed package as pushed) until COORD confirms the row. To clean up after that:
`git -C $Repo worktree remove "$Pub\kit"`, the same for `"$Pub\signer"`, then delete `$Pub`.
