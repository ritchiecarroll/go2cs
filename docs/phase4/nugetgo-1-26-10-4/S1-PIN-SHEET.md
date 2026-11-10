# PIN session S1: sign and push six nugetgo packages built on go.* 1.26.10.4 (runbook section 4)

> **TEMPLATE (C2, 2026-11-10), for the nugetgo wave against go.* 1.26.10.4 (`docs/PLAN-nugetgo-wave-1-26-10-4.md`).** It is not pasted as
> it stands. On release day C2 fills its placeholders, all in block 1: `<<SIGNER_SHA>>`, the full SHA of the master
> commit whose `src/sign-nupkgs.ps1` signs and whose `src/_nugetpush.ps1` pushes; and each kit's full commit on its
> `claude/c2-nugetgo-1261004-<name>` ref, the one COORD ruled green: hashset `<<KIT_SHA_HASHSET>>`, uuid `<<KIT_SHA_UUID>>`, jwt `<<KIT_SHA_JWT>>`, xsync `<<KIT_SHA_XSYNC>>`, pflag `<<KIT_SHA_PFLAG>>`, mousetrap `<<KIT_SHA_MOUSETRAP>>`. C2 then re-runs
> `Test-SheetPaste.ps1` on the filled copy and posts it to COORD. Block 1 refuses while any placeholder is left, and
> refuses a kit ref that has moved since the ruling.

It is one owner sheet for one `dotnet nuget sign` process (one PIN) over six packages, pushed in the order below.
Nothing is edited at the table. It follows the 1.26.10.3 wave's S1 sheet block for block. What changes: the release, the revisions, the kit refs and their pinned
commits, the work folder, block 6's lists, and block 7, which reports exceptions only.

| # | Package | Version | Kit ref |
|--:|:--|:--|:--|
| 1 | nugetgo.github.com.ritchiecarroll.hashset | 1.0.0.6 | claude/c2-nugetgo-1261004-hashset |
| 2 | nugetgo.github.com.google.uuid | 1.6.0.4 | claude/c2-nugetgo-1261004-uuid |
| 3 | nugetgo.github.com.golang-jwt.jwt.v5 | 5.3.1.4 | claude/c2-nugetgo-1261004-jwt |
| 4 | nugetgo.golang.org.x.sync | 0.19.0.4 | claude/c2-nugetgo-1261004-xsync |
| 5 | nugetgo.github.com.spf13.pflag | 1.0.9.4 | claude/c2-nugetgo-1261004-pflag |
| 6 | nugetgo.github.com.inconshreveable.mousetrap | 1.1.0.3 | claude/c2-nugetgo-1261004-mousetrap |

None of the six depends on another. cobra (session S2) depends on pflag 1.0.9.4 and mousetrap 1.1.0.3, which S1
publishes, so S2 starts only after this sheet's block 5 lists both.

**Do not start it until COORD says so.** COORD says so only after: COORD has ruled sections 1-3 green for every
kit (for mousetrap, its four hosted rows are in its release candidate); and the six conversion commits and their
`nuget-<PV>` tags are pushed (runbook 4.5), mousetrap-cs's included.

**A version pushed to nuget.org is permanent.** It can be unlisted, but never deleted or replaced. Every block below
stops before the push if a gate fails.

## How the blocks work

Each block is ONE statement (a dot-sourced `{ ... }`). A `throw` stops that whole block, so no line after a red line
runs. Read each block's output before you paste the next. Block 0 refuses the two missteps of 2026-10-14 with the
fix printed: a Windows PowerShell 5.1 window, and a window outside the go2cs clone.

## First: the nugetgo NuGet API key

**One nugetgo key serves all four sheets of this wave (S1 to S4).** The 1.26.10.3 wave's key serves if it has not
expired: its glob is `nugetgo.*` and its scope is *Push*. This wave pushes no new package ID, so a new key needs
only *Push only new package versions*. To create one: on nuget.org, your account, **API Keys**, then **Create**:

- **Glob pattern:** ONE pattern per entry. Type `nugetgo.*` (or `*`). A comma-separated list in that field fails.
- **Scope:** **Push**, with *Push only new package versions* (or *Push new packages and package versions*).
- **Expires in:** the longest offered.

Copy the key once, into `$env:NUGETGO_API_KEY` in the window you will use, or keep it ready for block 4's prompt.

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

    $Release = '1.26.10.4'
    $Signer  = '<<SIGNER_SHA>>'
    if ($Signer -like '*<<*' -or $Signer -notmatch '^[0-9a-f]{40}$') { throw 'this sheet is the TEMPLATE: the signer SHA is not filled in. Ask COORD for the filled copy.' }
    $Org     = 'https://api.nuget.org/v3/index.json'
    $Pub     = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-1261004-s1'
    # In push order. PV is Base, plus ".Rev" when Rev is not 0 (B3). Requires: the kits this one depends on.
    $Kits = @(
        [ordered]@{ Name = 'hashset';   Base = '1.0.0';  Rev = '6'; Id = 'nugetgo.github.com.ritchiecarroll.hashset';    CsRepo = 'ritchiecarroll/hashset-cs'; Sha = '<<KIT_SHA_HASHSET>>';   Requires = @() }
        [ordered]@{ Name = 'uuid';      Base = '1.6.0';  Rev = '4'; Id = 'nugetgo.github.com.google.uuid';               CsRepo = 'ritchiecarroll/uuid-cs'; Sha = '<<KIT_SHA_UUID>>';      Requires = @() }
        [ordered]@{ Name = 'jwt';       Base = '5.3.1';  Rev = '4'; Id = 'nugetgo.github.com.golang-jwt.jwt.v5';         CsRepo = 'ritchiecarroll/jwt-cs'; Sha = '<<KIT_SHA_JWT>>';       Requires = @() }
        [ordered]@{ Name = 'xsync';     Base = '0.19.0'; Rev = '4'; Id = 'nugetgo.golang.org.x.sync';                    CsRepo = 'ritchiecarroll/sync-cs'; Sha = '<<KIT_SHA_XSYNC>>';      Requires = @() }
        [ordered]@{ Name = 'pflag';     Base = '1.0.9';  Rev = '4'; Id = 'nugetgo.github.com.spf13.pflag';               CsRepo = 'ritchiecarroll/pflag-cs'; Sha = '<<KIT_SHA_PFLAG>>';     Requires = @() }
        [ordered]@{ Name = 'mousetrap'; Base = '1.1.0';  Rev = '3'; Id = 'nugetgo.github.com.inconshreveable.mousetrap'; CsRepo = 'ritchiecarroll/mousetrap-cs'; Sha = '<<KIT_SHA_MOUSETRAP>>'; Requires = @() }
    )
    foreach ($k in $Kits) {
        $k.PV  = if ([int]$k.Rev) { "$($k.Base).$($k.Rev)" } else { $k.Base }
        $k.Ref = "claude/c2-nugetgo-1261004-$($k.Name)"
        $k.Dir = Join-Path $Pub "kit-$($k.Name)"
    }
    if (Test-Path $Pub) { throw "$Pub already exists: remove it or choose another folder" }
    New-Item -ItemType Directory $Pub | Out-Null

    git -C $Repo fetch origin master @($Kits | ForEach-Object { $_.Ref })
    if ($LASTEXITCODE) { throw "git fetch exited $LASTEXITCODE" }
    foreach ($k in $Kits) {
        if ($k.Sha -like '*<<*' -or $k.Sha -notmatch '^[0-9a-f]{40}$') { throw "this sheet is the TEMPLATE: the $($k.Name) kit SHA is not filled in. Ask COORD for the filled copy." }
        $tip = git -C $Repo rev-parse "origin/$($k.Ref)"
        if ($LASTEXITCODE -or $tip -ne $k.Sha) { throw "$($k.Ref) is at $tip, not $($k.Sha), the commit COORD ruled green: stop and tell COORD" }
        git -C $Repo worktree add --detach $k.Dir $k.Sha
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

For each kit: its release candidate matches its SHA256SUMS line; its nuspec references go.* at `[1.26.10.4, ...)`;
its version is the table's; its README names release 1.26.10.4 in its release line; and a package that requires
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

## Block 6: deprecate the eighteen revisions built on go.* 1.26.10.1 to 1.26.10.3

**Why.** go.* 1.26.10.4 changes members that compiled code binds, and go.lib's break floor (`GoBreakFloor`) is
1.26.10.4. Every revision of these packages built on an earlier go.* 1.26.10 release has a go.* range that admits
1.26.10.4, so NuGet restores it beside go.* 1.26.10.4 without an error, and the build then stops with `GO2CS3002`,
naming the package and the release it was built on (`GoCheckBreakFloor` runs before `CoreCompile`). A restore prints
no deprecation line (measured, `docs/phase4/RECORD-nuget-sibling-pins.md` section 5); `dotnet list package
--deprecated`, an IDE's package manager and the package page show one. A deprecation unlists and deletes nothing,
and can be undone from the same page. The revisions built against go.* 1.24.13 cap their range below 1.25, cannot
resolve under 1.26, and are left as they are.

The 1.26.10.3 wave deprecated the 1.26.10.1 and 1.26.10.2 revisions with the 1.26.10.3 ones as alternates.
Deprecating them again moves their alternate to this wave's revision, so no package page points at a revision
the floor refuses.

**Ruled IN** (owner, 2026-11-07: "Can deprecate existing published modules, no prob"; COORD ruling 2026-11-08: every
listed revision that admits the new release and breaks on it; the plan, section 6). Run it only after block 5 has
listed the new six revisions: the block checks that again before it prints anything, and it changes nothing itself.
nuget.org deprecates from the package's Manage page, signed in, and one save covers every version ticked, so the
block prints, for each package, the page to open, the versions to tick, the reason, the alternate and the message to
paste.

```powershell
. {
    if (-not $GuardOK) { throw 'block 0 has not passed in this window' }
    # The revisions each new one replaces, read on nuget.org on 2026-11-10 (the plan, section 6), oldest first.
    $Stale = @{ hashset = @('1.0.0.3', '1.0.0.4', '1.0.0.5'); uuid = @('1.6.0.1', '1.6.0.2', '1.6.0.3'); jwt = @('5.3.1.1', '5.3.1.2', '5.3.1.3'); xsync = @('0.19.0.1', '0.19.0.2', '0.19.0.3'); pflag = @('1.0.9.1', '1.0.9.2', '1.0.9.3'); mousetrap = @('1.1.0', '1.1.0.1', '1.1.0.2') }
    foreach ($k in $Kits) {
        try { $listed = @((Invoke-RestMethod "https://api.nuget.org/v3-flatcontainer/$($k.Id.ToLowerInvariant())/index.json").versions) } catch { $listed = @() }
        if ($listed -notcontains $k.PV.ToLowerInvariant()) { throw "nuget.org does not list $($k.Id) $($k.PV) yet: run block 5 first; deprecate nothing" }
        foreach ($v in $Stale[$k.Name]) { if ($listed -notcontains $v.ToLowerInvariant()) { throw "nuget.org does not list $($k.Id) ${v}: stop and tell COORD" } }
    }
    foreach ($k in $Kits) {
        $old = @($Stale[$k.Name])
        ''
        "PACKAGE  $($k.Id)"
        "  page:      https://www.nuget.org/packages/$($k.Id)/$($old[-1])/Manage   (section: Deprecation)"
        "  tick:      $($old -join ' and ')   (these versions only)"
        '  reason:    Other'
        "  alternate: $($k.Id), version $($k.PV)"
        "  message:   Built against an earlier go.* 1.26.10 release. go.* 1.26.10.4 refuses it at build time with GO2CS3002: its break floor is 1.26.10.4. Use $($k.PV), built against go.* 1.26.10.4."
    }
}
```

## Block 7: report to COORD

Reply with one line. When nothing is red, that line is all:

- `S1 done`: every block printed its OK lines, and block 6's eighteen deprecations are saved.
- `S1 red at block <n>`, then that block's red line (the `STOP` line, or the error text) and nothing after it.

Do not paste the signer's or the build's own output: it contains local paths.

Keep `$Pub\sign` until COORD confirms the registry rows. To clean up after that: for each kit,
`git -C $Repo worktree remove $k.Dir`; then `git -C $Repo worktree remove "$Pub\signer"`; then delete `$Pub`.
