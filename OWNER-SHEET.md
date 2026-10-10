# hashset v1.0.0, rebuild 1.0.0.2: the owner sheet (sign, push, poll)

Written by C2 on 2026-10-10 for the owner to paste on the i7. It publishes `nugetgo.github.com.ritchiecarroll.hashset`
**1.0.0.2**: the same Go module, `github.com/ritchiecarroll/hashset` v1.0.0, rebuilt (B3: a release's rebuild adds a
fourth part, `X.Y.Z.N`). The rebuild exists to fix the package page on nuget.org: 1.0.0 showed VALIDATION.md as its
README and had no icon. 1.0.0.2 carries a generated README in the style of the `go.*` packages and the go2cs icon.
**Revision 1 (1.0.0.1) is skipped forever** (COORD ruling C, 2026-10-10): it was packed, and its tag `nuget-1.0.0.1` was
pushed to `hashset-cs`, but it was never pushed to nuget.org. The tag stays where it is, unconsumed, because a posted
ref is never replaced. Your review of its preview changed the package's text, so the change ships as revision 2.

This is `docs/NugetgoPublish.md` section 4, steps 1 to 4 only: sign, verify, push, wait. There is no int rehearsal,
no registry change (the row maps the module to the ID, not to a version) and no new conversion: the C# source is the
one `hashset-cs` already holds. **Do not start it until COORD says so.**

## Before the first block, in every new window

1. The i7's default shell is Windows PowerShell 5.1. This sheet needs PowerShell 7, which is installed as a .NET
   global tool. Start it FIRST, from the 5.1 window and before the environment block:
   `& "$env:USERPROFILE\.dotnet\tools\pwsh.exe"`. Once the environment block has put .NET 10 first on `PATH`, that
   pwsh no longer starts.
2. Confirm that `$PSVersionTable.PSVersion` reads 7.x.
3. `Set-Location` to the root of your go2cs clone.
4. **Stop at the first red line.** A `throw` stops only the statement it is in: the lines pasted after it keep
   running. Read each block's output before you paste the next, and send COORD the first red line instead.

You need: the signing card inserted, `NuGetCertFingerprint` set as it is for `release-nuget.ps1`, and a nuget.org API
key whose glob covers `nugetgo.*` (in `$env:NUGETGO_API_KEY`, or typed at the prompt in block 2).

## What this branch holds

| Path | What it is |
|:--|:--|
| `nupkg/nugetgo.github.com.ritchiecarroll.hashset.1.0.0.2.nupkg` | the rebuild, unsigned, as packed |
| `nupkg/SHA256SUMS` | its SHA-256, as packed |
| `preview/README.md` | the package README it carries, as text |
| `preview/README.png` | that README rendered on a dark page (the badges drawn locally; see below) |
| `preview/VALIDATION-as-1.0.0-readme.png` | 1.0.0's README (VALIDATION.md), rendered the same way, for comparison |
| `conversion-add/VALIDATION.md`, `conversion-add/index.md` | the two files the `hashset-cs` repository adds at its root (COORD's commit) |
| `evidence/SELECTION.md` | the measurement that a consumer selects 1.0.0.2 over 1.0.0 |

Packed at `T` = `claude/c2-nugetgo-tools` @ `3d0d70ba53` (the 1.0.0 tools, the README and icon cut, and your review
of the revision-1 preview), from the same
two roots as 1.0.0, against the published go.* 1.24.13.5, with `-Revision 2 -Release -LicenseSpdx MIT`. The proof is
unchanged: 37 matched, 0 disclosed, input digest
`sha256-7fb2058d54e18d61eaa288b0e79c847c953bdadf87cdd00fdb0d832750903114`, bound to the packed project. The read-back
was clean: README.md generated, the go2cs icon packed byte for byte, no host path, and the self-description still
reads module version `v1.0.0` (B3: the rebuild changes the package version only).

**Your review of the revision-1 preview (2026-10-10).** The description and the README no longer say "PROOF:", and
the README's first link is the Go source: the module path in the top callout, and in the "Converted from ..." line,
links `github.com/ritchiecarroll/hashset` at its tag `v1.0.0` on GitHub. The package tags are `go2cs golang go`, with
PROOF dropped (COORD ruling C). The pack refuses a "PROOF:" in the description or README, a PROOF tag, and a README
whose first link is anything else. Against revision 1's package, the assembly, the pdb, the icon, LICENSE, `index.md`,
VALIDATION.md and the self-description are byte for byte the same; the version, the tags and the README's tag links
differ. Against 1.0.0's text, VALIDATION.md's first line lost "PROOF: " too, because it is the description.
`conversion-add/VALIDATION.md` is that file, byte for byte as packed (mixed line endings: the header line LF, the page
CRLF; git blob `501e16c9d9`). COORD commits it to `hashset-cs` and mints the signed tag `nuget-1.0.0.2` on that commit
before you start; block 1b checks it.

The preview badges are local stand-ins: this lane box cannot reach img.shields.io, so each badge is drawn from its
own URL (label, message, color). nuget.org loads the real images from shields.io.

**The README links `hashset-cs` at the tag `nuget-1.0.0.2`:** the C# Source badge links the tree, the Tests badge
`VALIDATION.md` and the license line `LICENSE`, both at the repository root (COORD ruling 2026-10-10: a tag, never
`HEAD`, so 1.0.0's page and this one each keep their own). LICENSE is already there. VALIDATION.md is not, so COORD's
commit adds `conversion-add/VALIDATION.md` and `conversion-add/index.md` at the root, and COORD mints the signed tag
on that commit, before you start. Block 1b checks both. The C# source
does not change: measured against `hashset-cs` @ `e886982b0b`, every source file differs from the kit's only in line
endings (git stored LF), and LICENSE, README.md, .gitignore and both icons are byte-identical.

### 1. Environment, the kit and the signer

The first seven lines are the environment block every sheet uses: they put the .NET 10 SDK and go1.24.13 first on
`PATH` for this window only.

```powershell
$env:DOTNET_ROOT = "$env:USERPROFILE\dotnet10"
$env:GOROOT = "$env:USERPROFILE\sdk\go1.24.13"
$env:GOTOOLCHAIN = 'local'
$env:MSBUILDDISABLENODEREUSE = '1'
$env:PATH = "$env:DOTNET_ROOT;$env:GOROOT\bin;$env:PATH"
if ((dotnet --version) -notlike '10.*') { throw 'dotnet is not .NET 10: check the env block' }
if ((go version) -notmatch 'go1\.24\.13 ') { throw 'go is not go1.24.13: check the env block' }

$Repo   = (git rev-parse --show-toplevel)
$Pub    = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-hashset-rebuild-1'
$Kit    = 'claude/c2-nugetgo-hashset-rebuild-1'
$Master = '18f9c58186'
$Id     = 'nugetgo.github.com.ritchiecarroll.hashset'
$PV     = '1.0.0.2'
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

### 1b. Gate: the conversion repository's tag

The tag `nuget-$PV` on `ritchiecarroll/hashset-cs` names the conversion commit the package README links (COORD ruling 2026-10-10:
a tag, never `HEAD`). COORD pushes that commit and mints the signed tag before you start; this block checks, as an
anonymous reader (no credentials), that the tag exists, that VALIDATION.md and the license file resolve at it, and
that VALIDATION.md is byte for byte the one the package carries. It changes nothing.

```powershell
$CsRepo = 'ritchiecarroll/hashset-cs'
$Tag = "nuget-$PV"
$env:GIT_TERMINAL_PROMPT = '0'
$refs = @(git -c credential.helper= ls-remote "https://github.com/$CsRepo.git" "refs/tags/$Tag" "refs/tags/$Tag^{}")
if ($LASTEXITCODE) { throw "git ls-remote exited ${LASTEXITCODE}: $CsRepo cannot be read anonymously" }
if (-not $refs) { throw "$CsRepo has no tag ${Tag}: COORD pushes it before this sheet; stop and tell COORD" }
$commit = (@($refs | Where-Object { $_ -like '*^{}' }) + $refs)[0].Split("`t")[0]
foreach ($f in 'VALIDATION.md', 'LICENSE') {
    try { Invoke-WebRequest "https://raw.githubusercontent.com/$CsRepo/$Tag/$f" -OutFile "$Pub\tag-$f" }
    catch { throw "$f does not resolve anonymously at $CsRepo ${Tag}: stop and tell COORD" }
}
if ((Get-FileHash "$Pub\tag-VALIDATION.md").Hash -ne (Get-FileHash "$Pub\kit\conversion-add\VALIDATION.md").Hash) { throw "VALIDATION.md at $Tag is not the one the package carries: stop and tell COORD" }
"tag OK  $CsRepo $Tag -> commit $($commit.Substring(0, 10)); VALIDATION.md and LICENSE resolve anonymously"
```

### 2. The checksum gate, then sign and verify

The same block as hashset 1.0.0's 4.1. `sign-nupkgs.ps1` signs every `.nupkg` in the folder it is given, so the
package is copied into a folder of its own. The two signer calls are `release-nuget.ps1`'s own, read at master
`18f9c58186` (`src/sign-nupkgs.ps1` and `src/release-nuget.ps1` are unchanged from there to master on 2026-10-10). The
PIN prompt comes once, at `-Apply`. The verify fails unless the signer is the certificate `NuGetCertFingerprint` names.

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
if (@(Get-ChildItem "$SignDir\*.nupkg").Count -ne 1) { throw "$SignDir must hold the package alone" }
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

### 3. Push to nuget.org (the permanent step)

The block first reads nuget.org: the ID must already list 1.0.0 (it is yours, from the first publish) and must list
neither 1.0.0.1 (revision 1, skipped: it must never appear) nor 1.0.0.2. It then asks you to type `publish`. **A pushed version is permanent:** it can be unlisted, never deleted
or replaced.

The key comes from `$env:NUGETGO_API_KEY` when that is set, and from a prompt when it is empty. Either way it is
never printed, it never appears on a command line you type or in PSReadLine history, and the block's copy of it is
removed when the push ends. Your `$env:NUGETGO_API_KEY` is read, not changed. While `dotnet nuget push` runs, the key
is in that process's argument list, as with `release-nuget.ps1`. `--skip-duplicate` is left out on purpose: the block
has just shown that 1.0.0.2 is not on nuget.org, so a duplicate means something is wrong.

```powershell
$lower = $Id.ToLowerInvariant()
$listedNow = @((Invoke-RestMethod "https://api.nuget.org/v3-flatcontainer/$lower/index.json").versions)
if ($listedNow -notcontains '1.0.0') { throw "nuget.org does not list $Id 1.0.0: stop and tell COORD" }
foreach ($v in '1.0.0.1', $PV) { if ($listedNow -contains $v) { throw "nuget.org already lists $Id ${v}: stop and tell COORD; do not push" } }
"nuget.org lists $Id $($listedNow -join ', '); $PV is not there yet"
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

### 4. Wait until nuget.org lists the version

nuget.org validates a package before listing it. The block checks the flat container and the registration (runbook
4.4) once a minute for up to 90 minutes.

```powershell
$index = Invoke-RestMethod $Org
$flat = ($index.resources | Where-Object { $_.'@type' -eq 'PackageBaseAddress/3.0.0' } | Select-Object -First 1).'@id'.TrimEnd('/')
$reg  = ($index.resources | Where-Object { $_.'@type' -eq 'RegistrationsBaseUrl/3.6.0' } | Select-Object -First 1).'@id'.TrimEnd('/')
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

### 5. Stop and report to COORD

Send COORD one reply with: the `tag OK` line from block 1b, the `checksum OK` and `signed and verified` lines from block 2, the `nuget.org lists` and
`pushed:` lines from block 3, and the `listed on nuget.org` line from block 4. Do not send the signer output itself:
it contains local paths.

Then stop. Look at the package page on nuget.org once it shows 1.0.0.2, and tell COORD whether it reads as you want.
Keep `$Pub\sign` (the signed package as pushed) until COORD confirms. To clean up after that:
`git -C $Repo worktree remove "$Pub\kit"`, the same for `"$Pub\signer"`, then delete `$Pub`.
