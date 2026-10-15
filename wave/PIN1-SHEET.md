# PIN session 1: sign and push six nugetgo packages on one PIN (runbook section 4) -- DRAFT

**DRAFT, prepared by C2 on 2026-10-14 (COORD ruling: S1 GO, on this ref, never master).** It is one owner sheet for
WAVE-PLAN.md's PIN session 1: hashset, uuid, jwt/v5, x/sync, pflag and mousetrap, signed in ONE `dotnet nuget sign`
process (one PIN) and pushed in the wave's order. It replaces six per-kit section-4 sheets, so nothing is edited at
the table. Values marked `<...>` are filled when the six wave kits exist: `<R>` (the go.* release they were packed
against), `<SIGNER>` (the master commit whose `sign-nupkgs.ps1` signs) and the six revisions.

**Do not start it until COORD says so.** COORD says so only after: go.* `<R>` is published and its record is on master;
COORD has ruled sections 1-3 green for all six kits (for mousetrap, its four hosted rows are in its release
candidate); and the six conversion commits and their `nuget-<PV>` tags are pushed (COORD's step, runbook 4.5).

**A version pushed to nuget.org is permanent.** It can be unlisted, but never deleted or replaced. Every block below
stops before the push if a gate fails.

## Two lessons from 2026-10-14, built in

Both of the owner's missteps that day are now refused by block 0, with the fix printed:
- the window was Windows PowerShell 5.1, not pwsh 7;
- the window was not inside the go2cs clone.

And each block is ONE statement (a dot-sourced `{ ... }`). A `throw` stops that whole block, so no line after a red
line runs. That replaces the old rule "stop at the first red line" with a guarantee. Still read each block's output
before you paste the next.

## Before the first block

You need: the signing card inserted; `NuGetCertFingerprint` set as it is for `release-nuget.ps1`; and a nuget.org API
key whose glob covers `nugetgo.*`, either in `$env:NUGETGO_API_KEY` or typed at the prompt in block 4. A key scoped to
`go.*` does not cover `nugetgo.*`; a 403 in block 4 means the key's scope.

## Block 0: the window

This block uses only Windows PowerShell 5.1 syntax, so it runs (and refuses) in either shell.

```powershell
. {
    $GuardOK = $false
    if ($PSVersionTable.PSVersion.Major -lt 7) {
        Write-Host "STOP: this window is PowerShell $($PSVersionTable.PSVersion), not 7.x." -ForegroundColor Red
        Write-Host '  FIX: in THIS window run:  & "$env:USERPROFILE\.dotnet\tools\pwsh.exe"' -ForegroundColor Yellow
        Write-Host '       then Set-Location to your go2cs clone and paste block 0 again.' -ForegroundColor Yellow
        throw 'block 0: not PowerShell 7'
    }
    $Repo = git rev-parse --show-toplevel 2>$null
    if ($LASTEXITCODE -or -not $Repo) {
        Write-Host "STOP: $((Get-Location).Path) is not inside your go2cs clone." -ForegroundColor Red
        Write-Host '  FIX: Set-Location <the root of your go2cs clone>, then paste block 0 again.' -ForegroundColor Yellow
        throw 'block 0: not inside the go2cs clone'
    }
    $GuardOK = $true
    "block 0 OK: pwsh $($PSVersionTable.PSVersion), clone $Repo"
}
```

## Block 1: the environment, the six kits and the signer

```powershell
. {
    if (-not $GuardOK) { throw 'block 0 has not passed in this window' }
    $env:DOTNET_ROOT = "$env:USERPROFILE\dotnet10"
    $env:MSBUILDDISABLENODEREUSE = '1'
    $env:PATH = "$env:DOTNET_ROOT;$env:PATH"
    if ((dotnet --version) -notlike '10.*') { throw 'dotnet is not .NET 10: check DOTNET_ROOT' }

    $Release = '<R>'
    $Signer  = '<SIGNER>'
    $Org     = 'https://api.nuget.org/v3/index.json'
    $Pub     = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-pin1'
    # In WAVE-PLAN order. PV is Base, plus ".Rev" when Rev is not 0 (B3).
    $Kits = @(
        [ordered]@{ Name = 'hashset';   Base = '1.0.0';  Rev = '<rev>'; Id = 'nugetgo.github.com.ritchiecarroll.hashset';      CsRepo = 'ritchiecarroll/hashset-cs' }
        [ordered]@{ Name = 'uuid';      Base = '1.6.0';  Rev = '<rev>'; Id = 'nugetgo.github.com.google.uuid';                 CsRepo = 'ritchiecarroll/uuid-cs' }
        [ordered]@{ Name = 'jwt';       Base = '5.3.1';  Rev = '<rev>'; Id = 'nugetgo.github.com.golang-jwt.jwt.v5';           CsRepo = 'ritchiecarroll/jwt-cs' }
        [ordered]@{ Name = 'xsync';     Base = '0.19.0'; Rev = '<rev>'; Id = 'nugetgo.golang.org.x.sync';                      CsRepo = 'ritchiecarroll/sync-cs' }
        [ordered]@{ Name = 'pflag';     Base = '1.0.9';  Rev = '<rev>'; Id = 'nugetgo.github.com.spf13.pflag';                 CsRepo = 'ritchiecarroll/pflag-cs' }
        [ordered]@{ Name = 'mousetrap'; Base = '1.1.0';  Rev = '0';     Id = 'nugetgo.github.com.inconshreveable.mousetrap';   CsRepo = 'ritchiecarroll/mousetrap-cs' }
    )
    foreach ($k in $Kits) {
        $k.PV  = if ([int]$k.Rev) { "$($k.Base).$($k.Rev)" } else { $k.Base }
        $k.Ref = "claude/c2-nugetgo-wave2-$($k.Name)"
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

## Block 2: the six conversion repositories' tags

For each package: the tag `nuget-<PV>` exists on its conversion repository, read anonymously; VALIDATION.md and LICENSE
resolve at it; and VALIDATION.md is byte for byte the one the kit carries. It changes nothing.

```powershell
. {
    if (-not $GuardOK) { throw 'block 0 has not passed in this window' }
    $env:GIT_TERMINAL_PROMPT = '0'
    foreach ($k in $Kits) {
        $tag = "nuget-$($k.PV)"
        $refs = @(git -c credential.helper= ls-remote "https://github.com/$($k.CsRepo).git" "refs/tags/$tag" "refs/tags/$tag^{}")
        if ($LASTEXITCODE) { throw "git ls-remote exited ${LASTEXITCODE}: $($k.CsRepo) cannot be read anonymously" }
        if (-not $refs) { throw "$($k.CsRepo) has no tag ${tag}: stop and tell COORD" }
        foreach ($f in 'VALIDATION.md', 'LICENSE') {
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

## Block 3: the checksum and release gates, then ONE signing process, then verify

For each kit: its release candidate matches its SHA256SUMS line, and its nuspec references go.* at `[<R>, ...)`, so
no package built against another release can be signed. All six are then copied into one folder, and the signer runs
once without `-Apply` (the certificate check) and once with it: ONE process, so ONE PIN prompt (`sign-nupkgs.ps1`:
the card caches the PIN for that process only). Each signed package is then verified against the fingerprint.

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
            $entry = $zip.Entries | Where-Object { $_.FullName -like '*.nuspec' } | Select-Object -First 1
            $reader = [IO.StreamReader]::new($entry.Open())
            try { $nuspec = $reader.ReadToEnd() } finally { $reader.Dispose() }
        } finally { $zip.Dispose() }
        $ranges = @([regex]::Matches($nuspec, 'id="go\.[^"]+" version="([^"]+)"') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique)
        if (-not $ranges -or @($ranges | Where-Object { $_ -notlike "``[$Release,*" }).Count) {
            throw "$($k.Name): its go.* ranges are $($ranges -join ' '), not [$Release, ...): packed against another release"
        }
        if (([xml]$nuspec).package.metadata.version -ne $k.PV) { throw "$($k.Name): the package version is not $($k.PV)" }
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

First, for each package, nuget.org must not already hold its version. A new ID (mousetrap) reads 404; a rebuild's
ID exists, and its list must not contain the version. Then you type `publish` once for the six. The packages are
pushed in WAVE-PLAN order, and the first failure stops the block: the ones before it are pushed, the ones after
are not, and the block says which. `--skip-duplicate` is left out on purpose, as in every sheet.

```powershell
. {
    if (-not $GuardOK) { throw 'block 0 has not passed in this window' }
    foreach ($k in $Kits) {
        $probe = Invoke-WebRequest "https://api.nuget.org/v3-flatcontainer/$($k.Id.ToLowerInvariant())/index.json" -SkipHttpErrorCheck
        if ($probe.StatusCode -eq 200) {
            if (@(($probe.Content | ConvertFrom-Json).versions) -contains $k.PV.ToLowerInvariant()) { throw "$($k.Id) $($k.PV) is already on nuget.org: stop and tell COORD" }
        } elseif ($probe.StatusCode -ne 404) { throw "$($k.Id): nuget.org answered HTTP $($probe.StatusCode): stop and tell COORD" }
        Write-Host ('  {0,-46} {1,-10} sha256 {2}' -f $k.Id, $k.PV, $k.SignedHash)
    }
    Write-Host "About to push these $($Kits.Count) packages to nuget.org, in this order." -ForegroundColor Yellow
    Write-Host 'This is PERMANENT: a pushed version can be unlisted, never deleted or replaced.' -ForegroundColor Yellow
    if ((Read-Host "Type 'publish' to push all $($Kits.Count)") -cne 'publish') { throw 'Not published. Nothing was sent.' }

    if ($env:NUGETGO_API_KEY) { $key = $env:NUGETGO_API_KEY; 'key: from $env:NUGETGO_API_KEY' } else { $secure = Read-Host -AsSecureString 'nuget.org API key (nugetgo.*)'; $key = [Net.NetworkCredential]::new('', $secure).Password; $secure.Dispose(); 'key: from the prompt' }
    $pushed = @()
    try {
        foreach ($k in $Kits) {
            dotnet nuget push $k.Signed --source $Org --api-key $key
            if ($LASTEXITCODE) {
                throw "push of $($k.Name) exited $LASTEXITCODE (a 403 means the key's glob). Pushed before it: $(if ($pushed) { $pushed -join ', ' } else { 'none' }). Do not re-sign or re-pack; tell COORD."
            }
            $pushed += $k.Name
            "pushed: $($k.Id) $($k.PV)"
        }
    } finally { Remove-Variable key -ErrorAction SilentlyContinue }
}
```

## Block 5: wait until nuget.org lists all six

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

Send COORD one reply with: the `block 0 OK` line; the six `tag OK` lines; the six `checksum OK` and six `signed and
verified` lines; the six `pushed:` lines; and the six `listed on nuget.org` lines. Do not send the signer's or the
build's own output: it contains local paths.

Keep `$Pub\sign` until COORD confirms the six registry rows. To clean up after that: for each kit,
`git -C $Repo worktree remove $k.Dir`; then `git -C $Repo worktree remove "$Pub\signer"`; then delete `$Pub`.
