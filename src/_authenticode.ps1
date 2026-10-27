# _authenticode.ps1 - Authenticode-signs Windows executables with the release certificate.
#
# Dot-sourced by sign-release-binaries.ps1 (go2cs.exe in the GitHub release archives) and by the go2cs dotnet tool's
# packing step (the executables inside the tool packages), so every Windows executable the project ships is signed
# the same way. Dot-source this file, then:
#
#   Invoke-GoAuthenticodeSign -Files <string[]> [-Fingerprint <SHA-256>] [-Timestamper <RFC 3161 URL>]
#
# Each file is signed in place and verified; the call returns one [pscustomobject]@{ File; Signer; Timestamped } per
# file and throws, naming the file, on the first one that does not verify.
#
# ON WINDOWS (the release machine). The certificate is the release certificate, found in the CurrentUser or
# LocalMachine store by its SHA-256 fingerprint, the same value as $env:NuGetCertFingerprint that signs the packages.
# Every file is signed by ONE signtool process, because the smart card's KSP caches the PIN for the life of the
# process that unlocked it: one call is one PIN prompt however many files it signs. A separate process, such as the
# `dotnet nuget sign` that signs the packages, asks again. The signature is SHA-256 with an RFC 3161 timestamp, which
# keeps it valid after the certificate expires. signtool is taken from PATH, or else the newest Windows 10 SDK.
#
# WITH -CertificateFile AND -KeyFile (PEM) it signs with osslsigncode instead, on any platform. That is the path the
# selftest takes with a self-signed test certificate; the release certificate is on a card only the release machine
# reaches. -NoTimestamp is for that path's tests, which have no timestamp server.

function Find-GoSigningCertificate([string] $Fingerprint) {
    $wanted = $Fingerprint.Replace(':', '').Replace(' ', '').ToUpperInvariant()

    if ($wanted.Length -eq 40) {
        throw "The fingerprint $Fingerprint is a SHA-1 thumbprint; pass the certificate's SHA-256 fingerprint, the value `$env:NuGetCertFingerprint holds"
    }

    foreach ($store in 'Cert:\CurrentUser\My', 'Cert:\LocalMachine\My') {
        $found = @(Get-ChildItem $store -ErrorAction SilentlyContinue | Where-Object { $_.GetCertHashString('SHA256') -eq $wanted }) | Select-Object -First 1
        if ($found) { return $found }
    }

    throw "No certificate with SHA-256 fingerprint $wanted in Cert:\CurrentUser\My or Cert:\LocalMachine\My. Is the card inserted?"
}

function Find-GoSignTool {
    $onPath = Get-Command signtool.exe -ErrorAction SilentlyContinue
    if ($onPath) { return $onPath.Source }

    $kits = Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\bin'
    $newest = @(Get-ChildItem $kits -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^\d+\.' } |
        Sort-Object { [version]$_.Name } -Descending | ForEach-Object { Join-Path $_.FullName 'x64\signtool.exe' } |
        Where-Object { Test-Path $_ }) | Select-Object -First 1
    if ($newest) { return $newest }

    throw 'signtool.exe is not on PATH and no Windows 10 SDK holds one; install the Windows SDK signing tools'
}

# Runs a native tool and returns its output lines; a non-zero exit throws with that output. 'Continue' keeps a
# stderr line from becoming a terminating error under Windows PowerShell.
function Invoke-GoSigner([string] $Exe, [string[]] $Arguments) {
    $ErrorActionPreference = 'Continue'
    $out = @(& $Exe @Arguments 2>&1 | ForEach-Object { "$_" })
    if ($LASTEXITCODE -ne 0) { throw "$(Split-Path $Exe -Leaf) exited $($LASTEXITCODE):`n$($out -join "`n")" }
    $out
}

function Invoke-GoAuthenticodeSign {
    param(
        [Parameter(Mandatory)] [string[]] $Files,
        [string] $Fingerprint = $env:NuGetCertFingerprint,
        [string] $Timestamper = 'http://timestamp.digicert.com',
        [string] $CertificateFile,
        [string] $KeyFile,
        [switch] $NoTimestamp
    )

    $paths = @($Files | ForEach-Object { (Resolve-Path $_).Path })

    if ($CertificateFile -or $KeyFile) {
        if (-not ($CertificateFile -and $KeyFile)) { throw '-CertificateFile and -KeyFile go together' }
        $tool = (Get-Command osslsigncode -ErrorAction Stop).Source
        $subject = ((Invoke-GoSigner 'openssl' @('x509', '-in', $CertificateFile, '-noout', '-subject')) -join '') -replace '^subject=\s*', ''

        foreach ($path in $paths) {
            $signed = "$path.signed"
            $arguments = @('sign', '-certs', $CertificateFile, '-key', $KeyFile, '-h', 'sha256', '-in', $path, '-out', $signed)
            if (-not $NoTimestamp) { $arguments += @('-ts', $Timestamper) }
            [void](Invoke-GoSigner $tool $arguments)
            Move-Item -Force $signed $path
        }

        # With the certificate as the only trusted root, a verify that passes is a signature by that certificate.
        foreach ($path in $paths) {
            try { [void](Invoke-GoSigner $tool @('verify', '-CAfile', $CertificateFile, '-in', $path)) }
            catch { throw "$path does not verify as signed by $($subject): $_" }
            [pscustomobject]@{ File = $path; Signer = $subject; Timestamped = (-not $NoTimestamp) }
        }

        return
    }

    if (-not $Fingerprint) { throw 'No signing certificate: pass -Fingerprint or set $env:NuGetCertFingerprint (the SHA-256 fingerprint)' }
    $certificate = Find-GoSigningCertificate $Fingerprint
    $signtool = Find-GoSignTool

    $arguments = @('sign', '/sha1', $certificate.Thumbprint, '/fd', 'sha256')
    if (-not $NoTimestamp) { $arguments += @('/tr', $Timestamper, '/td', 'sha256') }
    [void](Invoke-GoSigner $signtool ($arguments + $paths))

    foreach ($path in $paths) {
        $signature = Get-AuthenticodeSignature -FilePath $path
        if (-not $signature.SignerCertificate -or $signature.SignerCertificate.Thumbprint -ne $certificate.Thumbprint) {
            throw "$path is not signed by $($certificate.Subject) after signtool: $($signature.Status) $($signature.StatusMessage)"
        }
        if (-not $NoTimestamp -and -not $signature.TimeStamperCertificate) { throw "$path carries no timestamp after signtool" }
        [pscustomobject]@{ File = $path; Signer = $certificate.Subject; Timestamped = [bool]$signature.TimeStamperCertificate }
    }
}
