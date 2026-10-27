# PIN session S2: sign and push cobra, built on go.* 1.26.10.1 (runbook section 4)

**Prepared by C2 on 2026-10-27, for COORD's nugetgo rebuild wave, session S2 (COORD ruling 2026-10-26: cobra follows
S1, after its pack changes land).** It is one owner sheet for one `dotnet nuget sign` process (one PIN) over one
package. Nothing is edited at the table. The signer is master `d9b8e5ac0e` (batch 50), as in S1: its
`sign-nupkgs.ps1` signs and its `_nugetpush.ps1` pushes.

| # | Package | Version | Kit ref |
|--:|:--|:--|:--|
| 1 | nugetgo.github.com.spf13.cobra | 1.10.2 | claude/c2-nugetgo-wave3-cobra |

cobra requires `nugetgo.github.com.spf13.pflag` 1.0.9.1 and `nugetgo.github.com.inconshreveable.mousetrap` 1.1.0, which
S1 published. Block 3 checks that its nuspec names exactly those versions and that nuget.org lists both.

**Do not start it until COORD says so.** COORD says so only after: COORD has ruled sections 1-3 green for the kit;
and the conversion commit and its `nuget-1.10.2` tag are pushed to cobra-cs (runbook 4.5).

**A version pushed to nuget.org is permanent.** It can be unlisted, but never deleted or replaced. Every block below
stops before the push if a gate fails.

## How the blocks work

Each block is ONE statement (a dot-sourced `{ ... }`). A `throw` stops that whole block, so no line after a red line
runs. Read each block's output before you paste the next. Block 0 refuses the two missteps of 2026-10-14 with the
fix printed: a Windows PowerShell 5.1 window, and a window outside the go2cs clone (another repository, such as the
registry clone, counts as outside: it checks for `src/go2cs/main.go`).

## Before the first block

You need: the signing card inserted; `NuGetCertFingerprint` set as it is for `release-nuget.ps1`; and a nuget.org API
key whose glob covers `nugetgo.*`, either in `$env:NUGETGO_API_KEY` or typed at the prompt in block 4. A key scoped to
`go.*` does not cover `nugetgo.*`.

## Block 0: the window

This block uses only Windows PowerShell 5.1 syntax, so it runs (and refuses) in either shell.

```powershell
. {
    $GuardOK = $false
    if ($PSVersionTable.PSVersion.Major -lt 7) {
        Write-Host "STOP: this window is PowerShell $($PSVersionTable.PSVersion), not 7.x." -ForegroundColor Red
        Write-Host '  FIX: open a NEW PowerShell window, then in it run:  & "$env:USERPROFILE\.dotnet\tools\pwsh.exe"' -ForegroundColor Yellow
        Write-Host '       then Set-Location to your go2cs clone and paste block 0 again.' -ForegroundColor Yellow
        Write-Host '       (A window that ran the release block or another sheet points DOTNET_ROOT and PATH at .NET 10,' -ForegroundColor Yellow
        Write-Host '       and this pwsh needs .NET 8, so it does not start there.)' -ForegroundColor Yellow
        throw 'block 0: not PowerShell 7'
    }
    $Repo = git rev-parse --show-toplevel 2>$null
    if ($LASTEXITCODE -or -not $Repo -or -not (Test-Path -LiteralPath (Join-Path $Repo 'src/go2cs/main.go'))) {
        Write-Host "STOP: $((Get-Location).Path) is not inside your go2cs clone." -ForegroundColor Red
        Write-Host '  FIX: Set-Location <the root of your go2cs clone>, then paste block 0 again.' -ForegroundColor Yellow
        throw 'block 0: not inside the go2cs clone'
    }
    $GuardOK = $true
    "block 0 OK: pwsh $($PSVersionTable.PSVersion), clone $Repo"
}
```

## Block 1: the environment, the kits and the signer

```powershell
. {
    if (-not $GuardOK) { throw 'block 0 has not passed in this window' }
    $env:DOTNET_ROOT = "$env:USERPROFILE\dotnet10"
    $env:MSBUILDDISABLENODEREUSE = '1'
    $env:PATH = "$env:DOTNET_ROOT;$env:PATH"
    if ((dotnet --version) -notlike '10.*') { throw 'dotnet is not .NET 10: check DOTNET_ROOT' }

    $Release = '1.26.10.1'
    $Signer  = 'd9b8e5ac0efe6508a285a3084612e152d5f943a0'
    $Org     = 'https://api.nuget.org/v3/index.json'
    $Pub     = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-wave3-s2'
    # PV is Base, plus ".Rev" when Rev is not 0 (B3). Requires: the published packages this one depends on.
    $Kits = @(
        [ordered]@{ Name = 'cobra'; Base = '1.10.2'; Rev = '0'; Id = 'nugetgo.github.com.spf13.cobra'; CsRepo = 'ritchiecarroll/cobra-cs'
            Requires = @(@{ Id = 'nugetgo.github.com.spf13.pflag'; PV = '1.0.9.1' }, @{ Id = 'nugetgo.github.com.inconshreveable.mousetrap'; PV = '1.1.0' }) }
    )
    foreach ($k in $Kits) {
        $k.PV  = if ([int]$k.Rev) { "$($k.Base).$($k.Rev)" } else { $k.Base }
        $k.Ref = "claude/c2-nugetgo-wave3-$($k.Name)"
        $k.Dir = Join-Path $Pub "kit-$($k.Name)"
    }
    if (Test-Path $Pub) { throw "$Pub already exists: remove it or choose another folder" }
    New-Item -ItemType Directory $Pub | Out-Null

    git -C $Repo fetch origin master @($Kits | ForEach-Object { $_.Ref })
    if ($LASTEXITCODE) { throw "git fetch exited $LASTEXITCODE" }
    foreach ($k in $Kits) {
        git -C $Repo worktree add --detach $k.Dir "origin/$($k.Ref)"
        if ($LASTEXITCODE) { throw "worktree for $($k.Name) exited $LASTEXITCODE" }
    }
    git -C $Repo worktree add --detach "$Pub\signer" $Signer
    if ($LASTEXITCODE) { throw "worktree signer exited $LASTEXITCODE" }
    $Kits | ForEach-Object { '{0,-10} {1} {2}' -f $_.Name, $_.Id, $_.PV }
}
```

## Block 2: the conversion repositories' tags

For each package: the tag `nuget-<PV>` exists on its conversion repository, read anonymously; VALIDATION.md and the
license file resolve at it; and VALIDATION.md is byte for byte the one the kit carries. It changes nothing.

```powershell
. {
    if (-not $GuardOK) { throw 'block 0 has not passed in this window' }
    $env:GIT_TERMINAL_PROMPT = '0'
    foreach ($k in $Kits) {
        $tag = "nuget-$($k.PV)"
        $refs = @(git -c credential.helper= ls-remote "https://github.com/$($k.CsRepo).git" "refs/tags/$tag" "refs/tags/$tag^{}")
        if ($LASTEXITCODE) { throw "git ls-remote exited ${LASTEXITCODE}: $($k.CsRepo) cannot be read anonymously" }
        if (-not $refs) { throw "$($k.CsRepo) has no tag ${tag}: stop and tell COORD" }
        $license = @(Get-ChildItem "$($k.Dir)\conversion" -File | Where-Object { $_.Name -like 'LICENSE*' })[0].Name
        foreach ($f in 'VALIDATION.md', $license) {
            try { Invoke-WebRequest "https://raw.githubusercontent.com/$($k.CsRepo)/$tag/$f" -OutFile "$Pub\tag-$($k.Name)-$f" } catch {
                throw "$f does not resolve anonymously at $($k.CsRepo) ${tag}: stop and tell COORD"
            }
        }
        if ((Get-FileHash "$Pub\tag-$($k.Name)-VALIDATION.md").Hash -ne (Get-FileHash "$($k.Dir)\conversion\VALIDATION.md").Hash) {
            throw "VALIDATION.md at $($k.CsRepo) $tag is not the one the $($k.Name) package carries: stop and tell COORD"
        }
        "tag OK  $($k.CsRepo) $tag"
    }
}
```

## Block 3: the gates, then ONE signing process, then verify

For each kit: its release candidate matches its SHA256SUMS line; its nuspec references go.* at `[1.26.10.1, ...)`;
its version is the table's; its README names release 1.26.10.1 in its release line; and its nuspec names each
required package at its version, which nuget.org lists. All are then copied into one folder, and the signer runs once without
`-Apply` (the certificate check) and once with it: ONE process, so ONE PIN prompt. Each signed package is then
verified against the fingerprint.

```powershell
. {
    if (-not $GuardOK) { throw 'block 0 has not passed in this window' }
    $SignDir = "$Pub\sign"
    New-Item -ItemType Directory $SignDir | Out-Null
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    foreach ($k in $Kits) {
        $file = "$($k.Id).nupkg"
        $sums = @(Get-Content "$($k.Dir)\nupkg\SHA256SUMS") -match "\srelease/$([regex]::Escape($file))$"
        $want = if ($sums) { ($sums[0] -split '\s+')[0] } else { $null }
        if (-not $want) { throw "$($k.Name): SHA256SUMS has no line for release/$file" }
        $got = (Get-FileHash -Algorithm SHA256 "$($k.Dir)\nupkg\release\$file").Hash.ToLowerInvariant()
        if ($got -ne $want) { throw "$($k.Name): checksum mismatch: $file is not the package C2 packed" }
        $zip = [IO.Compression.ZipFile]::OpenRead("$($k.Dir)\nupkg\release\$file")
        try {
            $read = { param($pattern) $e = $zip.Entries | Where-Object { $_.FullName -like $pattern } | Select-Object -First 1
                      $r = [IO.StreamReader]::new($e.Open()); try { $r.ReadToEnd() } finally { $r.Dispose() } }
            $nuspec = & $read '*.nuspec'
            $readme = & $read 'README.md'
        } finally { $zip.Dispose() }
        $ranges = @([regex]::Matches($nuspec, 'id="go\.[^"]+" version="([^"]+)"') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique)
        if (-not $ranges -or @($ranges | Where-Object { $_ -notlike "``[$Release,*" }).Count) {
            throw "$($k.Name): its go.* ranges are $($ranges -join ' '), not [$Release, ...): packed against another release"
        }
        if (([xml]$nuspec).package.metadata.version -ne $k.PV) { throw "$($k.Name): the package version is not $($k.PV)" }
        if (-not $readme.Contains("compiled against the go2cs standard-library packages at release ${Release}:")) {
            throw "$($k.Name): its README does not name release $Release in its release line"
        }
        foreach ($dep in $k.Requires) {
            if ($nuspec -notmatch "id=`"$([regex]::Escape($dep.Id))`" version=`"\[?$([regex]::Escape($dep.PV))[,\])`"]") {
                throw "$($k.Name): its nuspec does not require $($dep.Id) $($dep.PV)"
            }
            try { $listed = @((Invoke-RestMethod "https://api.nuget.org/v3-flatcontainer/$($dep.Id.ToLowerInvariant())/index.json").versions) } catch { $listed = @() }
            if ($listed -notcontains $dep.PV.ToLowerInvariant()) { throw "$($k.Name): nuget.org does not list $($dep.Id) $($dep.PV), which it requires" }
        }
        Copy-Item "$($k.Dir)\nupkg\release\$file" "$SignDir\$($k.Id).$($k.PV).nupkg"
        "checksum OK  $($k.Name)  $file  (go.* $($ranges -join ' '))"
    }
    if (@(Get-ChildItem "$SignDir\*.nupkg").Count -ne $Kits.Count) { throw "$SignDir must hold exactly the $($Kits.Count) release candidates" }

    if (-not $env:NuGetCertFingerprint) { throw 'NuGetCertFingerprint is not set: it is the variable release-nuget.ps1 and sign-nupkgs.ps1 read' }
    $fingerprint = $env:NuGetCertFingerprint.Trim().Replace(' ', '').ToUpperInvariant()
    $signer = "$Pub\signer\src\sign-nupkgs.ps1"
    & powershell -NoProfile -ExecutionPolicy Bypass -File $signer -PackageDir $SignDir
    if ($LASTEXITCODE) { throw "the signer's certificate check exited ${LASTEXITCODE}: is the card inserted?" }
    & powershell -NoProfile -ExecutionPolicy Bypass -File $signer -PackageDir $SignDir -Apply
    if ($LASTEXITCODE) { throw "signing exited $LASTEXITCODE; nothing was pushed" }
    foreach ($k in $Kits) {
        $k.Signed = "$SignDir\$($k.Id).$($k.PV).nupkg"
        dotnet nuget verify --all $k.Signed --certificate-fingerprint $fingerprint
        if ($LASTEXITCODE) { throw "dotnet nuget verify exited $LASTEXITCODE for $($k.Name); do not push" }
        $k.SignedHash = (Get-FileHash -Algorithm SHA256 $k.Signed).Hash.ToLowerInvariant()
        "signed and verified  $($k.Name)  sha256 $($k.SignedHash)"
    }
}
```

## Block 4: push to nuget.org (the permanent step)

The push is P1's helper on master (`src/_nugetpush.ps1` at the signer commit), the one implementation every push
calls. In table order it skips a version nuget.org already lists, pushes the rest, counts a 409 as published, and
with `-StopOnRateLimit` stops at once on a refusal that carries a Retry-After, naming the package, the counts so far
and the UTC time to paste this block again. A re-paste is the resume: it skips everything already listed and goes on
from the package that stopped it. You type `publish` once per paste.

```powershell
. {
    if (-not $GuardOK) { throw 'block 0 has not passed in this window' }
    $Kits | ForEach-Object { Write-Host ('  {0,-46} {1,-10} sha256 {2}' -f $_.Id, $_.PV, $_.SignedHash) }
    Write-Host "About to push these $($Kits.Count) packages to nuget.org, in this order, skipping any already listed." -ForegroundColor Yellow
    Write-Host 'This is PERMANENT: a pushed version can be unlisted, never deleted or replaced.' -ForegroundColor Yellow
    if ((Read-Host "Type 'publish' to push") -cne 'publish') { throw 'Not published. Nothing was sent.' }
    if ($env:NUGETGO_API_KEY) { $key = $env:NUGETGO_API_KEY; 'key: from $env:NUGETGO_API_KEY' } else { $secure = Read-Host -AsSecureString 'nuget.org API key (nugetgo.*)'; $key = [Net.NetworkCredential]::new('', $secure).Password; $secure.Dispose(); 'key: from the prompt' }
    try {
        . "$Pub\signer\src\_nugetpush.ps1"
        $result = Invoke-GoNuGetPush -Packages @($Kits | ForEach-Object { Get-Item -LiteralPath $_.Signed }) -Source $Org -ApiKey $key -StopOnRateLimit
        "push done: $($result.Pushed) pushed, $($result.AlreadyPublished) already published"
    } finally { Remove-Variable key -ErrorAction SilentlyContinue }
}
```

## Block 5: wait until nuget.org lists every package

Checks the flat container and the registration for every package once a minute, for up to 90 minutes in all.

```powershell
. {
    if (-not $GuardOK) { throw 'block 0 has not passed in this window' }
    $index = Invoke-RestMethod $Org
    $flat = ($index.resources | Where-Object { $_.'@type' -eq 'PackageBaseAddress/3.0.0' } | Select-Object -First 1).'@id'.TrimEnd('/')
    $reg  = ($index.resources | Where-Object { $_.'@type' -eq 'RegistrationsBaseUrl/3.6.0' } | Select-Object -First 1).'@id'.TrimEnd('/')
    $waiting = [Collections.Generic.List[object]]::new($Kits)
    foreach ($i in 1..90) {
        foreach ($k in @($waiting)) {
            $lower = $k.Id.ToLowerInvariant()
            try { $inFlat = @((Invoke-RestMethod "$flat/$lower/index.json").versions) -contains $k.PV.ToLowerInvariant() } catch { $inFlat = $false }
            $inReg = $false
            try {
                foreach ($page in @((Invoke-RestMethod "$reg/$lower/index.json").items)) {
                    $leaves = if ($page.items) { $page.items } else { (Invoke-RestMethod $page.'@id').items }
                    if (@($leaves.catalogEntry.version) -contains $k.PV) { $inReg = $true }
                }
            } catch { $inReg = $false }
            if ($inFlat -and $inReg) { [void]$waiting.Remove($k); "listed on nuget.org: $($k.Id) $($k.PV)" }
        }
        if (-not $waiting.Count) { break }
        Start-Sleep -Seconds 60
    }
    if ($waiting.Count) { throw "not listed after 90 minutes: $(($waiting | ForEach-Object { $_.Name }) -join ', '); tell COORD; do not push again" }
}
```

## Block 6: report to COORD

Send COORD one reply with: the `block 0 OK` line; the `tag OK`, `checksum OK` and `signed and verified` lines; the
push helper's summary; and the `listed on nuget.org` line. Do not send the signer's or the build's own output: it
contains local paths.

Keep `$Pub\sign` until COORD confirms the registry row. To clean up after that:
`git -C $Repo worktree remove $Kits[0].Dir`; then `git -C $Repo worktree remove "$Pub\signer"`; then delete `$Pub`.
