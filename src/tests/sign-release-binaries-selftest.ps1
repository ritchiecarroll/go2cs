#Requires -Version 5.1
<#
.SYNOPSIS
    Guards the release-signing step (src/sign-release-binaries.ps1) against a MOCK draft Release, with a self-signed
    test certificate and a throwaway GPG key, so it never touches this repository's real releases or the release card.

.DESCRIPTION
    Builds the six archives the way release-binaries.yml does (two real Windows executables cross-compiled with Go,
    zipped with `zip -9`; four tarballs), their SHA256SUMS and a stand-in attestation bundle, and serves them as a draft
    Release through src/tests/gh-release-mock.ps1, put first on PATH as `gh`. A scratch git repository holds the release tag, signed by a
    throwaway GPG key that is NOT the keyring's default, so the SHA256SUMS signature is shown to come from the tag's
    own key. Then:

      census      no -Apply -> the signed archives, SHA256SUMS and SHA256SUMS.asc are made locally; nothing is
                  uploaded or deleted
      apply       -Apply -> both Windows archives are replaced on the draft; each go2cs.exe carries an Authenticode
                  signature by the test certificate; every zip record is byte-identical except go2cs.exe's CRC-32,
                  sizes and body and the offsets after it; the four tarballs are untouched; SHA256SUMS matches every archive on
                  the draft; SHA256SUMS.asc verifies and was made by the tag's key; the stale attestation bundle is
                  gone; the Release is still a draft
      published   a Release that is not a draft -> refused, nothing uploaded
      checksum    an archive that does not match SHA256SUMS -> refused by name, nothing uploaded
      unattested  an archive with no attestation in the repository -> refused by name, nothing uploaded
      signed      a draft that already carries SHA256SUMS.asc -> refused, nothing uploaded

    Needs go, zip, unzip, openssl, osslsigncode, gpg and git on PATH. Exit 0 clean, 1 on any violation. Writes only
    under the host's temp directory.
#>
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'

$srcRoot = Split-Path $PSScriptRoot -Parent
$script = Join-Path $srcRoot 'sign-release-binaries.ps1'
$mock = Join-Path $PSScriptRoot 'gh-release-mock.ps1'
$pwshPath = (Get-Process -Id $PID).Path

Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem

$failures = New-Object System.Collections.Generic.List[string]
function Check([bool] $Condition, [string] $Message) { if (-not $Condition) { $failures.Add($Message) } }

foreach ($tool in 'go', 'zip', 'unzip', 'openssl', 'osslsigncode', 'gpg', 'git') {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) { Write-Host "sign-release-binaries selftest: $tool is not on PATH"; exit 1 }
}

# Native commands write progress to stderr; only their exit codes decide.
function Invoke-Native([string] $Exe, [string[]] $Arguments, [string] $Dir) {
    $ErrorActionPreference = 'Continue'
    if ($Dir) { Push-Location $Dir }
    try { $out = @(& $Exe @Arguments 2>&1 | ForEach-Object { "$_" }) } finally { if ($Dir) { Pop-Location } }
    if ($LASTEXITCODE -ne 0) { throw "$Exe $($Arguments -join ' ') exited $($LASTEXITCODE):`n$($out -join "`n")" }
    $out
}

function Get-Sha256([string] $Path) { (Get-FileHash $Path -Algorithm SHA256).Hash.ToLowerInvariant() }

# gpg-agent's socket path is length-limited, so the work root is kept short.
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("srb-" + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $work | Out-Null
$savedGnupgHome = $env:GNUPGHOME
$savedPath = $env:PATH

try {
    # ---- the six archives, as release-binaries.yml builds them ------------------------------------------------------
    $fixture = Join-Path $work 'fixture'
    $prog = Join-Path $work 'prog'
    New-Item -ItemType Directory -Path $fixture, $prog | Out-Null
    Set-Content -Path (Join-Path $prog 'go.mod') -Value "module selftest`n`ngo 1.21`n"
    Set-Content -Path (Join-Path $prog 'main.go') -Value "package main`n`nimport `"fmt`"`n`nfunc main() { fmt.Println(`"go2cs selftest`") }`n"

    $savedGoos = $env:GOOS; $savedGoarch = $env:GOARCH; $savedCgo = $env:CGO_ENABLED
    try {
        foreach ($t in @(@('windows', 'amd64', 'win-x64'), @('windows', 'arm64', 'win-arm64'))) {
            $stage = Join-Path $work "stage-$($t[2])"
            New-Item -ItemType Directory -Path $stage | Out-Null
            $env:GOOS = $t[0]; $env:GOARCH = $t[1]; $env:CGO_ENABLED = '0'
            [void](Invoke-Native 'go' @('build', '-trimpath', '-o', (Join-Path $stage 'go2cs.exe'), '.') $prog)
            Set-Content -Path (Join-Path $stage 'LICENSE') -Value 'selftest license'
            Set-Content -Path (Join-Path $stage 'LICENSE-EXCEPTION') -Value 'selftest license exception'
            [void](Invoke-Native 'zip' @('-q', '-9', (Join-Path $fixture "go2cs-$($t[2]).zip"), 'go2cs.exe', 'LICENSE', 'LICENSE-EXCEPTION') $stage)
        }
    }
    finally { $env:GOOS = $savedGoos; $env:GOARCH = $savedGoarch; $env:CGO_ENABLED = $savedCgo }

    foreach ($rid in 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64') {
        $stage = Join-Path $work "stage-$rid"
        New-Item -ItemType Directory -Path $stage | Out-Null
        Set-Content -Path (Join-Path $stage 'go2cs') -Value "not a real $rid binary"
        Set-Content -Path (Join-Path $stage 'LICENSE') -Value 'selftest license'
        [void](Invoke-Native 'tar' @('-C', $stage, '-czf', (Join-Path $fixture "go2cs-$rid.tar.gz"), 'go2cs', 'LICENSE') $null)
    }

    # sha256sum's format, in the byte order a C-locale glob gives.
    $names = [string[]]@(Get-ChildItem $fixture -File | ForEach-Object { $_.Name })
    [System.Array]::Sort($names, [System.StringComparer]::Ordinal)
    $sums = ($names | ForEach-Object { "$(Get-Sha256 (Join-Path $fixture $_))  $_" }) -join "`n"
    [System.IO.File]::WriteAllText((Join-Path $fixture 'SHA256SUMS'), "$sums`n")
    [System.IO.File]::WriteAllText((Join-Path $fixture 'go2cs-attestation.sigstore.jsonl'), "{`"selftest`":`"stand-in bundle`"}`n")
    $attestedDigests = @($names | ForEach-Object { Get-Sha256 (Join-Path $fixture $_) })

    # ---- the self-signed test certificate ---------------------------------------------------------------------------
    $certFile = Join-Path $work 'test-cert.pem'
    $keyFile = Join-Path $work 'test-key.pem'
    [void](Invoke-Native 'openssl' @('req', '-x509', '-newkey', 'rsa:2048', '-nodes', '-keyout', $keyFile, '-out', $certFile, '-days', '2',
        '-subj', '/CN=go2cs selftest code signing', '-addext', 'extendedKeyUsage=codeSigning') $null)

    # ---- two throwaway GPG keys; the tag is signed by the SECOND, so the keyring's default is the wrong one ----------
    $env:GNUPGHOME = Join-Path $work 'gnupg'
    New-Item -ItemType Directory -Path $env:GNUPGHOME | Out-Null
    foreach ($who in 'default', 'tag') {
        [void](Invoke-Native 'gpg' @('--batch', '--pinentry-mode', 'loopback', '--passphrase', '', '--quick-gen-key',
            "go2cs selftest $who <$who@selftest.invalid>", 'ed25519', 'sign', '1d') $null)
    }
    $tagKey = ((Invoke-Native 'gpg' @('--batch', '--with-colons', '--list-secret-keys', 'tag@selftest.invalid') $null) |
        Where-Object { $_ -like 'fpr:*' } | Select-Object -First 1).Split(':')[9]

    $tagRepo = Join-Path $work 'repo'
    New-Item -ItemType Directory -Path $tagRepo | Out-Null
    $tag = 'nuget-9.9.9.9'
    $git = @('-c', 'user.name=selftest', '-c', 'user.email=tag@selftest.invalid', '-c', 'gpg.format=openpgp', '-c', "user.signingkey=$tagKey", '-c', 'commit.gpgsign=false')
    [void](Invoke-Native 'git' @('init', '-q') $tagRepo)
    [void](Invoke-Native 'git' ($git + @('commit', '-q', '--allow-empty', '-m', 'selftest')) $tagRepo)
    [void](Invoke-Native 'git' ($git + @('tag', '-s', $tag, '-m', 'selftest release')) $tagRepo)

    # ---- the mock Release, answering as `gh` from the front of PATH ---------------------------------------------------
    $mockRoot = Join-Path $work 'mock'
    $bin = Join-Path $work 'bin'
    New-Item -ItemType Directory -Path $bin | Out-Null
    if ([System.IO.Path]::DirectorySeparatorChar -eq '\') {
        [System.IO.File]::WriteAllText((Join-Path $bin 'gh.cmd'), "@`"$pwshPath`" -NoProfile -File `"$mock`" %*`r`n")
    }
    else {
        [System.IO.File]::WriteAllText((Join-Path $bin 'gh'), "#!/bin/sh`nexec `"$pwshPath`" -NoProfile -File `"$mock`" `"`$@`"`n")
        [void](Invoke-Native 'chmod' @('+x', (Join-Path $bin 'gh')) $null)
    }
    $env:PATH = $bin + [System.IO.Path]::PathSeparator + $env:PATH

    function Reset-Release([bool] $Draft = $true) {
        if (Test-Path $mockRoot) { Remove-Item -Recurse -Force $mockRoot }
        New-Item -ItemType Directory -Path (Join-Path $mockRoot 'assets') | Out-Null
        Get-ChildItem $fixture -File | ForEach-Object { Copy-Item $_.FullName (Join-Path $mockRoot "assets/$($_.Name)") }
        [System.IO.File]::WriteAllText((Join-Path $mockRoot 'release.json'), (@{ tag = $tag; isDraft = $Draft } | ConvertTo-Json))
        [System.IO.File]::WriteAllText((Join-Path $mockRoot 'attested.txt'), (($attestedDigests -join "`n") + "`n"))
        [System.IO.File]::WriteAllText((Join-Path $mockRoot 'calls.log'), '')
    }

    function Invoke-Signing([string] $Case, [switch] $Apply) {
        $ErrorActionPreference = 'Continue'
        $caseWork = Join-Path $work "out-$Case"
        $env:GH_MOCK_ROOT = $mockRoot
        $arguments = @('-NoProfile', '-File', $script, '-Tag', $tag, '-Repo', 'selftest/mock', '-WorkDir', $caseWork,
            '-TagRepository', $tagRepo, '-CertificateFile', $certFile, '-KeyFile', $keyFile, '-NoTimestamp')
        if ($Apply) { $arguments += '-Apply' }
        $out = @(& $pwshPath @arguments 2>&1 | ForEach-Object { "$_" })
        [pscustomobject]@{ Code = $LASTEXITCODE; Text = ($out -join "`n"); Work = $caseWork }
    }

    function Get-Mutations {
        @(Get-Content (Join-Path $mockRoot 'calls.log') | Where-Object { $_ -match '^release (upload|delete-asset)' })
    }

    # Entry name, order, time, attributes and content, for every entry but the one named.
    function Get-ZipShape([string] $Path, [string] $Except) {
        $zip = [System.IO.Compression.ZipFile]::OpenRead($Path)
        try {
            @($zip.Entries | Where-Object { $_.FullName -ne $Except } | ForEach-Object {
                $s = $_.Open(); $sha = [System.Security.Cryptography.SHA256]::Create()
                try { $h = [System.BitConverter]::ToString($sha.ComputeHash($s)) } finally { $s.Dispose(); $sha.Dispose() }
                "$($_.FullName)|$($_.LastWriteTime.ToString('o'))|$($_.ExternalAttributes)|$($_.CompressedLength)|$h"
            }) -join "`n"
        }
        finally { $zip.Dispose() }
    }

    # Every zip record, raw: each entry's local header and central record with the fields a content change must move
    # (CRC-32, sizes, local offset) blanked, plus a digest of each compressed body except the one named. Equal
    # results mean nothing but those fields and that body changed.
    function Get-ZipRecords([string] $Path, [string] $Except) {
        $b = [System.IO.File]::ReadAllBytes($Path)
        $u16 = { param($o) [int]$b[$o] -bor ([int]$b[$o + 1] -shl 8) }
        $u32 = { param($o) [uint32]$b[$o] -bor ([uint32]$b[$o + 1] -shl 8) -bor ([uint32]$b[$o + 2] -shl 16) -bor ([uint32]$b[$o + 3] -shl 24) }
        $hex = { param($from, $length, [int[]] $blank) $copy = New-Object byte[] $length; [System.Array]::Copy($b, $from, $copy, 0, $length); foreach ($o in $blank) { $copy[$o] = 0 }; [System.BitConverter]::ToString($copy) }
        $eocd = $b.Length - 22
        while ((& $u32 $eocd) -ne 0x06054b50) { $eocd-- }
        $count = & $u16 ($eocd + 10)
        $p = [int](& $u32 ($eocd + 16))
        $out = @()
        for ($i = 0; $i -lt $count; $i++) {
            $n = & $u16 ($p + 28); $e = & $u16 ($p + 30); $c = & $u16 ($p + 32)
            $name = [System.Text.Encoding]::UTF8.GetString($b, $p + 46, $n)
            $out += "central $(& $hex $p (46 + $n + $e + $c) @(16..27 + 42..45))"
            $at = [int](& $u32 ($p + 42))
            $ln = & $u16 ($at + 26); $le = & $u16 ($at + 28)
            $out += "local $(& $hex $at (30 + $ln + $le) @(14..25))"
            if ($name -ne $Except) {
                $sha = [System.Security.Cryptography.SHA256]::Create()
                try { $out += "body $name $([System.BitConverter]::ToString($sha.ComputeHash($b, $at + 30 + $ln + $le, [int](& $u32 ($at + 18)))))" } finally { $sha.Dispose() }
            }
            $p += 46 + $n + $e + $c
        }
        $out += "end $(& $hex $eocd ($b.Length - $eocd) @(16..19))"
        $out -join "`n"
    }

    function Expand-Exe([string] $Zip, [string] $To) {
        $archive = [System.IO.Compression.ZipFile]::OpenRead($Zip)
        try { [System.IO.Compression.ZipFileExtensions]::ExtractToFile($archive.GetEntry('go2cs.exe'), $To, $true) } finally { $archive.Dispose() }
    }

    # ---- census -------------------------------------------------------------------------------------------------------
    Reset-Release
    $run = Invoke-Signing 'census'
    Check ($run.Code -eq 0) "census: exit $($run.Code); want 0:`n$($run.Text)"
    Check ((Get-Mutations).Count -eq 0) "census: the draft was changed: $((Get-Mutations) -join '; ')"
    foreach ($name in 'go2cs-win-x64.zip', 'go2cs-win-arm64.zip', 'SHA256SUMS', 'SHA256SUMS.asc') {
        Check (Test-Path (Join-Path $run.Work "signed/$name")) "census: $name was not prepared locally"
    }

    # ---- apply --------------------------------------------------------------------------------------------------------
    Reset-Release
    $run = Invoke-Signing 'apply' -Apply
    Check ($run.Code -eq 0) "apply: exit $($run.Code); want 0:`n$($run.Text)"
    $assets = Join-Path $mockRoot 'assets'

    foreach ($rid in 'win-x64', 'win-arm64') {
        $name = "go2cs-$rid.zip"
        $original = Join-Path $fixture $name
        $shipped = Join-Path $assets $name
        Check ((Get-Sha256 $original) -ne (Get-Sha256 $shipped)) "apply: $name on the draft is still the unsigned archive"
        [void](Invoke-Native 'unzip' @('-tq', $shipped) $null)
        Check ((Get-ZipShape $original 'go2cs.exe') -eq (Get-ZipShape $shipped 'go2cs.exe')) "apply: $name changed beyond go2cs.exe:`n$(Get-ZipShape $original 'go2cs.exe')`n--- vs ---`n$(Get-ZipShape $shipped 'go2cs.exe')"
        Check ((Get-ZipRecords $original 'go2cs.exe') -eq (Get-ZipRecords $shipped 'go2cs.exe')) "apply: $name records changed beyond go2cs.exe's CRC, sizes, body and the offsets"
        $listing = { param($z) @(Invoke-Native 'unzip' @('-Z1', $z) $null) -join ',' }
        Check ((& $listing $original) -eq (& $listing $shipped)) "apply: $name entry order changed"

        $exe = Join-Path $work "$rid-shipped.exe"
        Expand-Exe $shipped $exe
        $verify = $null
        try { $verify = Invoke-Native 'osslsigncode' @('verify', '-CAfile', $certFile, '-in', $exe) $null } catch { $failures.Add("apply: go2cs.exe in $name does not verify against the test certificate: $_") }
        if ($verify) { Check (($verify -join "`n") -match 'go2cs selftest code signing') "apply: go2cs.exe in $name is not signed by the test certificate:`n$($verify -join "`n")" }
    }

    foreach ($rid in 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64') {
        $name = "go2cs-$rid.tar.gz"
        Check ((Get-Sha256 (Join-Path $fixture $name)) -eq (Get-Sha256 (Join-Path $assets $name))) "apply: $name was changed"
    }

    $sumLines = @(Get-Content (Join-Path $assets 'SHA256SUMS') | Where-Object { $_ })
    Check ($sumLines.Count -eq 6) "apply: SHA256SUMS has $($sumLines.Count) lines; want 6"
    Check ((@($sumLines | ForEach-Object { ($_ -split '\s+')[1] }) -join ',') -eq ($names -join ',')) "apply: SHA256SUMS lists $(@($sumLines | ForEach-Object { ($_ -split '\s+')[1] }) -join ',') ; want the original order $($names -join ',')"
    foreach ($line in $sumLines) {
        $want, $name = $line -split '\s+', 2
        $path = Join-Path $assets $name
        Check ((Test-Path $path) -and (Get-Sha256 $path) -eq $want) "apply: SHA256SUMS line does not match the draft's asset: $line"
    }

    $gpgCheck = $null
    try { $gpgCheck = Invoke-Native 'gpg' @('--batch', '--status-fd', '1', '--verify', (Join-Path $assets 'SHA256SUMS.asc'), (Join-Path $assets 'SHA256SUMS')) $null } catch { $failures.Add("apply: SHA256SUMS.asc does not verify: $_") }
    if ($gpgCheck) { Check (($gpgCheck -join "`n") -match "VALIDSIG $tagKey") "apply: SHA256SUMS.asc was not made by the tag's key $($tagKey):`n$($gpgCheck -join "`n")" }

    Check (-not (Test-Path (Join-Path $assets 'go2cs-attestation.sigstore.jsonl'))) 'apply: the attestation bundle for the unsigned archives is still on the draft'
    Check ([bool]((Get-Content -Raw (Join-Path $mockRoot 'release.json') | ConvertFrom-Json).isDraft)) 'apply: the Release is no longer a draft'
    Check ($run.Text -match 'attest-release\.yml') "apply: the run does not name the attestation workflow to run next:`n$($run.Text)"

    # ---- refusals -----------------------------------------------------------------------------------------------------
    Reset-Release -Draft $false
    $run = Invoke-Signing 'published' -Apply
    Check ($run.Code -ne 0 -and $run.Text -match 'not a draft') "published: exit $($run.Code), want a refusal naming the published Release:`n$($run.Text)"
    Check ((Get-Mutations).Count -eq 0) "published: the Release was changed: $((Get-Mutations) -join '; ')"

    Reset-Release
    Add-Content -Path (Join-Path $mockRoot 'assets/go2cs-osx-arm64.tar.gz') -Value 'tampered'
    $run = Invoke-Signing 'checksum' -Apply
    Check ($run.Code -ne 0 -and $run.Text -match 'go2cs-osx-arm64\.tar\.gz' -and $run.Text -match 'SHA256SUMS') "checksum: exit $($run.Code), want a refusal naming go2cs-osx-arm64.tar.gz:`n$($run.Text)"
    Check ((Get-Mutations).Count -eq 0) "checksum: the draft was changed: $((Get-Mutations) -join '; ')"

    Reset-Release
    $unattested = Get-Sha256 (Join-Path $fixture 'go2cs-win-arm64.zip')
    [System.IO.File]::WriteAllText((Join-Path $mockRoot 'attested.txt'), ((@($attestedDigests | Where-Object { $_ -ne $unattested }) -join "`n") + "`n"))
    $run = Invoke-Signing 'unattested' -Apply
    Check ($run.Code -ne 0 -and $run.Text -match 'go2cs-win-arm64\.zip' -and $run.Text -match 'attestation') "unattested: exit $($run.Code), want a refusal naming go2cs-win-arm64.zip:`n$($run.Text)"
    Check ((Get-Mutations).Count -eq 0) "unattested: the draft was changed: $((Get-Mutations) -join '; ')"

    Reset-Release
    [System.IO.File]::WriteAllText((Join-Path $mockRoot 'assets/SHA256SUMS.asc'), "signed already`n")
    $run = Invoke-Signing 'signed' -Apply
    Check ($run.Code -ne 0 -and $run.Text -match 'SHA256SUMS\.asc') "signed: exit $($run.Code), want a refusal naming SHA256SUMS.asc:`n$($run.Text)"
    Check ((Get-Mutations).Count -eq 0) "signed: the draft was changed: $((Get-Mutations) -join '; ')"
}
catch {
    $failures.Add("selftest setup or case threw: $_")
}
finally {
    try { & gpgconf --kill gpg-agent 2>$null } catch { }
    $env:GNUPGHOME = $savedGnupgHome
    $env:PATH = $savedPath
    Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue
}

if ($failures.Count -gt 0) { $failures | ForEach-Object { Write-Host "FAIL: $_" }; exit 1 }
Write-Host 'sign-release-binaries selftest: PASS (census changed nothing; apply signed both Windows exes, kept every other zip entry, left the tarballs, matched SHA256SUMS, signed it with the tag''s key, dropped the stale bundle, stayed a draft; refused a published Release, a bad checksum, an unattested archive and a signed draft)'
exit 0
