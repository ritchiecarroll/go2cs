# PIN session S3: sign and push timefmt-go, a new nugetgo package built on go.* 1.26.10.3 (runbook section 4)

> **TEMPLATE (C2, 2026-11-08), for the nugetgo wave against go.* 1.26.10.3
> ([`PLAN-rebank-1-26-10-3.md`](../../PLAN-rebank-1-26-10-3.md), section 8).** It is not pasted as it stands. On
> release day C2 fills its one placeholder, `<<SIGNER_SHA>>` in block 1: the full SHA of the master commit whose
> `src/sign-nupkgs.ps1` signs and whose `src/_nugetpush.ps1` pushes. C2 then re-runs `Test-SheetPaste.ps1` on the
> filled copy and posts it to COORD. Block 1 refuses while any `<<...>>` placeholder is left.
> Paste it only when COORD says so (below).

It is one owner sheet for one `dotnet nuget sign` process (one PIN) over one package. Nothing is edited at the table.
It follows the S1 sheet block for block, with one package. What differs: the package is a NEW ID on nuget.org, so
the key needs the new-packages scope (S1's key has it), and there is nothing to deprecate.

| # | Package | Version | Kit ref |
|--:|:--|:--|:--|
| 1 | nugetgo.github.com.itchyny.timefmt-go | 0.1.8 | claude/c2-nugetgo-1261003-timefmt |

timefmt-go depends on no other nugetgo package. gojq (session S4) requires it at 0.1.8, so S4 starts only after this
sheet's block 5 lists it, and after its registry row is served or COORD supplies the local `-nuget-map` source
(PLAN-rebank-1-26-10-3.md section 8, step 5).

**Do not start it until COORD says so.** COORD says so only after: COORD has ruled sections 1-3 green for the kit;
and the first commit of `timefmt-go-cs` (the converted source, the LICENSE verbatim, the README, `.gitignore`,
VALIDATION.md and its proof pages) and its `nuget-0.1.8` tag are pushed (runbook 4.5).

**A version pushed to nuget.org is permanent.** It can be unlisted, but never deleted or replaced. Every block below
stops before the push if a gate fails.

## How the blocks work

Each block is ONE statement (a dot-sourced `{ ... }`). A `throw` stops that whole block, so no line after a red line
runs. Read each block's output before you paste the next. Block 0 refuses the two missteps of 2026-10-14 with the
fix printed: a Windows PowerShell 5.1 window, and a window outside the go2cs clone.

## First: create the nugetgo NuGet API key for this wave

**Create one new nugetgo key before S1's block 0; it serves all four sheets of this wave (S1 to S4).** The
1.26.10.2 wave's key was scoped to *Push only new package versions*, which cannot push a package ID that
nuget.org does not have yet, and S3 and S4 push two new IDs. That key may also lapse on or about 2026-11-09. On
nuget.org: your account, **API Keys**, then **Create**:

- **Glob pattern:** ONE pattern per entry. Type `nugetgo.*` (or `*`). A comma-separated list in that field fails.
- **Scope:** **Push**, with *Push new packages and package versions*. This package ID is NEW on nuget.org, so this scope is required; it is the key S1 created.
- **Expires in:** the longest offered.

Copy the new key once, into `$env:NUGETGO_API_KEY` in the window you will use, or keep it ready for block 4's prompt.

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

    $Release = '1.26.10.3'
    $Signer  = '<<SIGNER_SHA>>'
    if ($Signer -like '*<<*' -or $Signer -notmatch '^[0-9a-f]{40}$') { throw 'this sheet is the TEMPLATE: the signer SHA is not filled in. Ask COORD for the filled copy.' }
    $Org     = 'https://api.nuget.org/v3/index.json'
    $Pub     = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-1261003-s3'
    # In push order. PV is Base, plus ".Rev" when Rev is not 0 (B3). Requires: the kits this one depends on.
    $Kits = @(
        [ordered]@{ Name = 'timefmt'; Base = '0.1.8'; Rev = '0'; Id = 'nugetgo.github.com.itchyny.timefmt-go'; CsRepo = 'ritchiecarroll/timefmt-go-cs'; Requires = @() }
    )
    foreach ($k in $Kits) {
        $k.PV  = if ([int]$k.Rev) { "$($k.Base).$($k.Rev)" } else { $k.Base }
        $k.Ref = "claude/c2-nugetgo-1261003-$($k.Name)"
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

For each kit: its release candidate matches its SHA256SUMS line; its nuspec references go.* at `[1.26.10.3, ...)`;
its version is the table's; its README names release 1.26.10.3 in its release line; and a package that requires
another kit names that kit's version. All are then copied into one folder, and the signer runs once without
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
        foreach ($need in $k.Requires) {
            $dep = $Kits | Where-Object { $_.Name -eq $need }
            if ($nuspec -notmatch "id=`"$([regex]::Escape($dep.Id))`" version=`"\[?$([regex]::Escape($dep.PV))[,\])`"]") {
                throw "$($k.Name): its nuspec does not require $($dep.Id) $($dep.PV)"
            }
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

## Block 6: nothing to deprecate

This is the first revision of a new package ID, so no earlier revision exists. Block 7 reads
`deprecation: none (new ID)`.

## Block 7: report to COORD

Send COORD one reply with: the `block 0 OK` line; the `tag OK`, `checksum OK` and `signed and verified` lines; the
push helper's summary; and the `listed on nuget.org` line. Do not send the signer's or the build's own output: it
contains local paths.

State `deprecation: none (new ID)`.

Keep `$Pub\sign` until COORD confirms the registry row. To clean up after that:
`git -C $Repo worktree remove $Kits[0].Dir`; then `git -C $Repo worktree remove "$Pub\signer"`; then delete `$Pub`.
