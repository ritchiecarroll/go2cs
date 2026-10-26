#Requires -Version 5.1
<#
.SYNOPSIS
    Sign the go2cs release binaries on the release machine: Authenticode for the Windows executables, and a detached
    GPG signature over SHA256SUMS for every archive. Then replace them on the DRAFT GitHub Release.

.DESCRIPTION
    release-binaries.yml builds the six go2cs archives in CI from the release record commit and drafts a GitHub Release
    holding them, SHA256SUMS and a build-provenance attestation bundle. The Windows executables are not code-signed
    there: the release certificate is on a smart card in the release machine. This script is the release-ritual step
    that closes that gap, in the release's PIN session:

      1. Reads the release tag's GPG signature and finds the key that made it (the SHA256SUMS signature is made with
         that key, nothing else), and verifies the tag.
      2. Refuses a Release that is not a draft, or that already carries SHA256SUMS.asc (an earlier run signed it).
      3. Downloads the draft's assets and verifies each archive against SHA256SUMS AND `gh attestation verify`, so what
         is signed is exactly what CI built.
      4. Authenticode-signs go2cs.exe from the win-x64 and win-arm64 archives, both in ONE signtool call, so the card
         asks for its PIN once (_authenticode.ps1, the step the tool packages' executables use too).
      5. Re-packs those two archives with only go2cs.exe changed: every other entry's record (header, extra fields,
         compressed bytes) is copied byte for byte, in the same order, and go2cs.exe keeps its name, time and
         attributes. The four tarballs are not touched: Authenticode signs Windows executables only.
      6. Regenerates SHA256SUMS (same lines, same order, the two new digests) and signs it: SHA256SUMS.asc, armored,
         detached, by the tag's key.
      7. With -Apply: uploads the two archives, SHA256SUMS and SHA256SUMS.asc to the draft; deletes the CI
         attestation bundle, which no longer matches the Windows archives; and re-downloads to verify. Without
         -Apply it stops after step 6 and changes nothing on GitHub (a census, like the repository's other tools).

    WHAT IT LEAVES TO DO. The draft then has no attestation bundle. Run the attest-release workflow
    (`gh workflow run attest-release.yml -f tag=<tag>`): it attests the six archives as uploaded and puts the bundle
    back. Publish the Release only after it succeeds, so a published Release never carries an attestation that does
    not match its assets. macOS and Linux binaries are covered by the signed SHA256SUMS only; macOS notarization
    needs an Apple Developer ID and is not done here.

    THE PIN. One signtool process signs both executables, so this script asks once. `dotnet nuget sign` (the
    packages) is a separate process and asks again: the card's KSP caches the PIN per process.

.PARAMETER Tag
    The release tag, nuget-N.N.N.N. Its Release must exist and be a draft.

.PARAMETER Repo
    owner/name of the GitHub repository. Defaults to the one origin points at.

.PARAMETER WorkDir
    Where the downloads, the signed files and the re-download go. Defaults to src\artifacts\release-binaries\<tag>,
    emptied first.

.PARAMETER Fingerprint
    SHA-256 fingerprint of the release certificate. Defaults to $env:NuGetCertFingerprint, the value that signs the
    packages.

.PARAMETER Timestamper
    RFC 3161 timestamping URL for the Authenticode signatures.

.PARAMETER CertificateFile
    With -KeyFile: sign with this PEM certificate through osslsigncode instead of the store. For the selftest and a
    rehearsal with a self-signed test certificate; never the release path.

.PARAMETER KeyFile
    The PEM private key that goes with -CertificateFile.

.PARAMETER NoTimestamp
    Sign without a timestamp. For the selftest, which has no timestamp server; a release signature is timestamped.

.PARAMETER TagRepository
    The git repository that holds the tag. Defaults to this one.

.PARAMETER Apply
    Upload to the draft. Without it nothing on GitHub changes.

.EXAMPLE
    .\sign-release-binaries.ps1 -Tag nuget-1.26.10.2
    Download, verify, sign and re-pack locally; print what would be uploaded.

.EXAMPLE
    .\sign-release-binaries.ps1 -Tag nuget-1.26.10.2 -Apply
    The release step: the same, then replace the assets on the draft. Then run the attest-release workflow.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Tag,
    [string] $Repo,
    [string] $WorkDir,
    [string] $Fingerprint = $env:NuGetCertFingerprint,
    [string] $Timestamper = 'http://timestamp.digicert.com',
    [string] $CertificateFile,
    [string] $KeyFile,
    [switch] $NoTimestamp,
    [string] $TagRepository,
    [switch] $Apply
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot '_paths.ps1')
. (Join-Path $PSScriptRoot '_authenticode.ps1')

Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem

function Write-Step([string] $Message) { Write-Host "==> $Message" -ForegroundColor Cyan }
function Die([string] $Message) { Write-Host $Message -ForegroundColor Red; exit 1 }

# A native command's output and exit code; 'Continue' so a stderr line is a record, not a terminating error.
function Invoke-Tool([string] $Exe, [string[]] $Arguments) {
    $ErrorActionPreference = 'Continue'
    $out = @(& $Exe @Arguments 2>&1 | ForEach-Object { "$_" })
    [pscustomobject]@{ Code = $LASTEXITCODE; Output = $out; Text = ($out -join "`n") }
}

function Get-Sha256([string] $Path) { (Get-FileHash $Path -Algorithm SHA256).Hash.ToLowerInvariant() }

$archives = @('go2cs-linux-arm64.tar.gz', 'go2cs-linux-x64.tar.gz', 'go2cs-osx-arm64.tar.gz', 'go2cs-osx-x64.tar.gz',
    'go2cs-win-arm64.zip', 'go2cs-win-x64.zip')
$windows = @('go2cs-win-arm64.zip', 'go2cs-win-x64.zip')
$bundle = 'go2cs-attestation.sigstore.jsonl'

if ($Tag -notmatch '^nuget-\d+\.\d+\.\d+\.\d+$') { Die "-Tag must be nuget-N.N.N.N; got '$Tag'" }
if (-not $TagRepository) { $TagRepository = $RepoRoot }

if (-not $Repo) {
    $origin = Invoke-Tool 'git' @('-C', $RepoRoot, 'remote', 'get-url', 'origin')
    if ($origin.Code -ne 0 -or $origin.Text -notmatch 'github\.com[:/]([^/]+/[^/]+?)(\.git)?$') { Die "Cannot tell the GitHub repository from origin ($($origin.Text)); pass -Repo owner/name" }
    $Repo = $Matches[1]
}

if (-not $WorkDir) { $WorkDir = Join-Path $SrcRoot "artifacts/release-binaries/$Tag" }
if (Test-Path $WorkDir) { Remove-Item -Recurse -Force $WorkDir }
New-Item -ItemType Directory -Path $WorkDir | Out-Null
$WorkDir = (Resolve-Path $WorkDir).Path

$gpg = (Invoke-Tool 'git' @('-C', $TagRepository, 'config', 'gpg.program')).Text.Trim()
if (-not $gpg) { $gpg = 'gpg' }

Write-Step "Release $Tag on $Repo (work folder $WorkDir)"

# ---- 1. the tag, and the key that signed it --------------------------------------------------------------------------
$tagObject = Invoke-Tool 'git' @('-C', $TagRepository, 'cat-file', '-p', "refs/tags/$Tag")
if ($tagObject.Code -ne 0) { Die "The tag $Tag is not in $TagRepository (fetch the tags first): $($tagObject.Text)" }
if ($tagObject.Text -match 'BEGIN SSH SIGNATURE') { Die "$Tag is signed with an SSH key. SHA256SUMS.asc is a GPG signature by the key that signs the release tag, so the tag must be GPG-signed." }
if ($tagObject.Text -notmatch '(?s)(-----BEGIN PGP SIGNATURE-----.*?-----END PGP SIGNATURE-----)') { Die "$Tag carries no GPG signature; a release tag is signed." }
$tagSignature = Join-Path $WorkDir 'tag.sig'
[System.IO.File]::WriteAllText($tagSignature, $Matches[1] + "`n")

$packets = Invoke-Tool $gpg @('--batch', '--list-packets', $tagSignature)
if ($packets.Text -match 'issuer fpr v\d+ ([0-9A-Fa-f]{40,64})') { $signingKey = $Matches[1].ToUpperInvariant() }
elseif ($packets.Text -match 'keyid ([0-9A-Fa-f]{16})') { $signingKey = $Matches[1].ToUpperInvariant() }
else { Die "Cannot read the key that signed $($Tag): $($packets.Text)" }

$verifyTag = Invoke-Tool 'git' @('-C', $TagRepository, 'verify-tag', $Tag)
if ($verifyTag.Code -ne 0) { Die "git verify-tag $Tag failed: $($verifyTag.Text)" }

$secret = Invoke-Tool $gpg @('--batch', '--list-secret-keys', $signingKey)
if ($secret.Code -ne 0) { Die "The key that signed $Tag ($signingKey) has no secret key in this keyring, so it cannot sign SHA256SUMS. Run this on the machine that signs the release tags." }
Write-Host "  tag key          : $signingKey (verified $Tag)"

# ---- 2. the draft -----------------------------------------------------------------------------------------------------
function Read-Release {
    $view = Invoke-Tool 'gh' @('release', 'view', $Tag, '--repo', $Repo, '--json', 'isDraft,assets')
    if ($view.Code -ne 0) { Die "gh release view $Tag failed: $($view.Text)" }
    $view.Text | ConvertFrom-Json
}

$release = Read-Release
$assetNames = @($release.assets | ForEach-Object { "$($_.name)" })
if (-not $release.isDraft) { Die "The Release for $Tag is not a draft. Its assets are public; this script replaces assets only before publication." }
if ($assetNames -contains 'SHA256SUMS.asc') { Die "The draft for $Tag already carries SHA256SUMS.asc: an earlier run signed it. Delete the draft's signed assets and re-run release-binaries.yml for the tag to start again." }
$missing = @(@($archives + 'SHA256SUMS') | Where-Object { $assetNames -notcontains $_ })
if ($missing.Count -gt 0) { Die "The draft for $Tag lacks $($missing -join ', ')" }
Write-Host "  draft            : yes, $($assetNames.Count) assets"

# ---- 3. download, and verify against SHA256SUMS and the build attestation -----------------------------------------
$original = Join-Path $WorkDir 'original'
$download = Invoke-Tool 'gh' @('release', 'download', $Tag, '--repo', $Repo, '--dir', $original, '--clobber')
if ($download.Code -ne 0) { Die "gh release download $Tag failed: $($download.Text)" }

$sumsText = [System.IO.File]::ReadAllText((Join-Path $original 'SHA256SUMS'))
$sums = @{}
foreach ($line in ($sumsText -split "`n" | Where-Object { $_.Trim() })) {
    $digest, $name = $line.Trim() -split '\s+\*?', 2
    $sums[$name] = $digest.ToLowerInvariant()
}

foreach ($name in $archives) {
    $path = Join-Path $original $name
    if (-not $sums.ContainsKey($name)) { Die "SHA256SUMS on the draft has no line for $name" }
    if ((Get-Sha256 $path) -ne $sums[$name]) { Die "$name does not match SHA256SUMS on the draft: $(Get-Sha256 $path) against $($sums[$name])" }

    $attest = Invoke-Tool 'gh' @('attestation', 'verify', $path, '--repo', $Repo)
    if ($attest.Code -ne 0) { Die "$name has no build attestation in $Repo that matches it, so it is not known to be the CI build: $($attest.Text)" }
}
Write-Host "  verified         : $($archives.Count) archives against SHA256SUMS and their build attestations"

# ---- 4. Authenticode-sign the two Windows executables, in one signer call --------------------------------------------
$signDir = Join-Path $WorkDir 'sign'
$exes = @()

foreach ($name in $windows) {
    $dir = Join-Path $signDir ([System.IO.Path]::GetFileNameWithoutExtension($name))
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    $exe = Join-Path $dir 'go2cs.exe'
    $zip = [System.IO.Compression.ZipFile]::OpenRead((Join-Path $original $name))
    try {
        $entry = $zip.GetEntry('go2cs.exe')
        if (-not $entry) { Die "$name holds no go2cs.exe" }
        [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $exe, $true)
    }
    finally { $zip.Dispose() }
    $exes += $exe
}

Write-Step "Authenticode-signing $($exes.Count) executables (one signer call: one PIN prompt)"
$signArguments = @{ Files = $exes; Fingerprint = $Fingerprint; Timestamper = $Timestamper; NoTimestamp = $NoTimestamp }
if ($CertificateFile) { $signArguments.CertificateFile = $CertificateFile; $signArguments.KeyFile = $KeyFile }
try { $signatures = @(Invoke-GoAuthenticodeSign @signArguments) }
catch { Die "Signing FAILED; nothing was uploaded: $_" }
$signatures | ForEach-Object { Write-Host "  signed           : $($_.File) by $($_.Signer)$(if ($_.Timestamped) { ', timestamped' })" }

# ---- 5. re-pack: only go2cs.exe changes -------------------------------------------------------------------------------
Add-Type -TypeDefinition @'
using System;
using System.IO;
using System.IO.Compression;

// Rewrites a zip with one entry's content replaced. Every other entry's local record (header, name, extra field,
// compressed data) and central record is copied byte for byte, in the same order. The replaced entry keeps its
// header, name, extra fields, time and attributes; only its CRC-32, sizes and data change, and the offsets that
// follow it. Refuses ZIP64, data descriptors, spanned archives and any layout it does not fully account for.
public static class GoZipSplice
{
    static readonly uint[] Table = MakeTable();

    static uint[] MakeTable()
    {
        uint[] t = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            uint c = i;
            for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            t[i] = c;
        }
        return t;
    }

    static uint Crc32(byte[] data)
    {
        uint c = 0xFFFFFFFFu;
        for (int i = 0; i < data.Length; i++) c = Table[(c ^ data[i]) & 0xFF] ^ (c >> 8);
        return c ^ 0xFFFFFFFFu;
    }

    static int U16(byte[] b, int o) { return b[o] | (b[o + 1] << 8); }
    static uint U32(byte[] b, int o) { return (uint)(b[o] | (b[o + 1] << 8) | (b[o + 2] << 16) | (b[o + 3] << 24)); }
    static void Put32(byte[] b, int o, uint v) { b[o] = (byte)v; b[o + 1] = (byte)(v >> 8); b[o + 2] = (byte)(v >> 16); b[o + 3] = (byte)(v >> 24); }

    static bool HasExtra(byte[] b, int start, int length, int id)
    {
        for (int p = start; p + 4 <= start + length; p += 4 + U16(b, p + 2))
            if (U16(b, p) == id) return true;
        return false;
    }

    static Exception Bad(string zip, string why) { return new InvalidDataException(zip + ": " + why); }

    public static void ReplaceEntry(string zipIn, string entryName, string contentPath, string zipOut)
    {
        byte[] zip = File.ReadAllBytes(zipIn);
        int eocd = -1;
        for (int i = zip.Length - 22; i >= 0 && i >= zip.Length - 22 - 65535; i--)
            if (U32(zip, i) == 0x06054b50u) { eocd = i; break; }
        if (eocd < 0) throw Bad(zipIn, "no end-of-central-directory record");

        int count = U16(zip, eocd + 10);
        uint cdSize = U32(zip, eocd + 12);
        uint cdOffset = U32(zip, eocd + 16);
        if (count == 0xFFFF || cdOffset == 0xFFFFFFFFu) throw Bad(zipIn, "ZIP64 is not supported");
        if (U16(zip, eocd + 4) != 0 || U16(zip, eocd + 6) != 0 || U16(zip, eocd + 8) != count) throw Bad(zipIn, "a spanned archive is not supported");
        if ((long)cdOffset + cdSize != eocd) throw Bad(zipIn, "the central directory does not end where the end record starts");

        int[] cdAt = new int[count];
        int[] cdLength = new int[count];
        uint[] local = new uint[count];
        int target = -1;
        int p = (int)cdOffset;
        for (int i = 0; i < count; i++)
        {
            if (U32(zip, p) != 0x02014b50u) throw Bad(zipIn, "a central record is malformed");
            int n = U16(zip, p + 28), e = U16(zip, p + 30), c = U16(zip, p + 32);
            cdAt[i] = p;
            cdLength[i] = 46 + n + e + c;
            local[i] = U32(zip, p + 42);
            if (System.Text.Encoding.UTF8.GetString(zip, p + 46, n) == entryName)
            {
                if (target >= 0) throw Bad(zipIn, entryName + " appears twice");
                if (HasExtra(zip, p + 46 + n, e, 1)) throw Bad(zipIn, entryName + " carries a ZIP64 extra field");
                target = i;
            }
            p += cdLength[i];
        }
        if (target < 0) throw Bad(zipIn, "no entry named " + entryName);

        int[] order = new int[count];
        uint[] keys = new uint[count];
        for (int i = 0; i < count; i++) { order[i] = i; keys[i] = local[i]; }
        Array.Sort(keys, order);

        int[] localLength = new int[count];
        long expect = 0;
        foreach (int i in order)
        {
            int at = (int)local[i];
            if (at != expect) throw Bad(zipIn, "the local records do not tile the archive");
            if (U32(zip, at) != 0x04034b50u) throw Bad(zipIn, "a local record is malformed");
            if ((U16(zip, at + 6) & 8) != 0) throw Bad(zipIn, "data descriptors are not supported");
            if (U32(zip, at + 18) != U32(zip, cdAt[i] + 20)) throw Bad(zipIn, "a local and a central compressed size differ");
            int n = U16(zip, at + 26), e = U16(zip, at + 28);
            if (i == target && HasExtra(zip, at + 30 + n, e, 1)) throw Bad(zipIn, entryName + " carries a ZIP64 extra field");
            localLength[i] = 30 + n + e + (int)U32(zip, at + 18);
            expect += localLength[i];
        }
        if (expect != cdOffset) throw Bad(zipIn, "bytes lie between the last local record and the central directory");

        byte[] content = File.ReadAllBytes(contentPath);
        uint crc = Crc32(content);
        int method = U16(zip, (int)local[target] + 8);
        byte[] data;
        if (method == 8)
        {
            MemoryStream deflated = new MemoryStream();
            using (DeflateStream stream = new DeflateStream(deflated, CompressionLevel.Optimal, true)) stream.Write(content, 0, content.Length);
            data = deflated.ToArray();
        }
        else if (method == 0) data = content;
        else throw Bad(zipIn, entryName + " uses compression method " + method + "; only stored (0) and deflated (8) are supported");

        uint[] newLocal = new uint[count];
        using (FileStream output = new FileStream(zipOut, FileMode.CreateNew))
        {
            long position = 0;
            foreach (int i in order)
            {
                int at = (int)local[i];
                newLocal[i] = (uint)position;
                if (i == target)
                {
                    int headLength = localLength[i] - (int)U32(zip, at + 18);
                    byte[] head = new byte[headLength];
                    Array.Copy(zip, at, head, 0, headLength);
                    Put32(head, 14, crc);
                    Put32(head, 18, (uint)data.Length);
                    Put32(head, 22, (uint)content.Length);
                    output.Write(head, 0, head.Length);
                    output.Write(data, 0, data.Length);
                    position += head.Length + data.Length;
                }
                else
                {
                    output.Write(zip, at, localLength[i]);
                    position += localLength[i];
                }
            }

            uint newCdOffset = (uint)position;
            for (int i = 0; i < count; i++)
            {
                byte[] record = new byte[cdLength[i]];
                Array.Copy(zip, cdAt[i], record, 0, record.Length);
                if (i == target)
                {
                    Put32(record, 16, crc);
                    Put32(record, 20, (uint)data.Length);
                    Put32(record, 24, (uint)content.Length);
                }
                Put32(record, 42, newLocal[i]);
                output.Write(record, 0, record.Length);
            }

            byte[] end = new byte[zip.Length - eocd];
            Array.Copy(zip, eocd, end, 0, end.Length);
            Put32(end, 16, newCdOffset);
            output.Write(end, 0, end.Length);
        }
    }
}
'@

$signedDir = Join-Path $WorkDir 'signed'
New-Item -ItemType Directory -Path $signedDir | Out-Null

foreach ($name in $windows) {
    $exe = Join-Path (Join-Path $signDir ([System.IO.Path]::GetFileNameWithoutExtension($name))) 'go2cs.exe'
    $out = Join-Path $signedDir $name
    try { [GoZipSplice]::ReplaceEntry((Join-Path $original $name), 'go2cs.exe', $exe, $out) }
    catch { Die "Re-packing $name FAILED; nothing was uploaded: $($_.Exception.InnerException.Message)$($_.Exception.Message)" }

    # Read it back: the entry list is unchanged and go2cs.exe is the signed executable.
    $before = [System.IO.Compression.ZipFile]::OpenRead((Join-Path $original $name))
    $after = [System.IO.Compression.ZipFile]::OpenRead($out)
    try {
        $was = @($before.Entries | ForEach-Object { $_.FullName }) -join ','
        $now = @($after.Entries | ForEach-Object { $_.FullName }) -join ','
        if ($was -ne $now) { Die "Re-packed $name lists $now; the original lists $was" }
        $read = Join-Path $signDir "$name.readback.exe"
        [System.IO.Compression.ZipFileExtensions]::ExtractToFile($after.GetEntry('go2cs.exe'), $read, $true)
        if ((Get-Sha256 $read) -ne (Get-Sha256 $exe)) { Die "Re-packed $name does not hold the signed go2cs.exe" }
    }
    finally { $before.Dispose(); $after.Dispose() }
    Write-Host "  re-packed        : $name ($((Get-Item $out).Length) bytes; only go2cs.exe changed)"
}

# ---- 6. SHA256SUMS, regenerated and signed by the tag's key -------------------------------------------------------------
$newSums = ($sumsText -split "`n" | ForEach-Object {
    if ($_ -match '^([0-9a-fA-F]{64})(\s+\*?)(\S.*)$' -and $windows -contains $Matches[3]) { "$(Get-Sha256 (Join-Path $signedDir $Matches[3]))$($Matches[2])$($Matches[3])" }
    else { $_ }
}) -join "`n"
$sumsPath = Join-Path $signedDir 'SHA256SUMS'
[System.IO.File]::WriteAllText($sumsPath, $newSums)

$ascPath = Join-Path $signedDir 'SHA256SUMS.asc'
$sign = Invoke-Tool $gpg @('--batch', '--yes', '--armor', '--local-user', "$signingKey!", '--output', $ascPath, '--detach-sign', $sumsPath)
if ($sign.Code -ne 0) { Die "gpg could not sign SHA256SUMS with $($signingKey): $($sign.Text)" }
$check = Invoke-Tool $gpg @('--batch', '--status-fd', '1', '--verify', $ascPath, $sumsPath)
if ($check.Code -ne 0 -or $check.Text -notmatch 'VALIDSIG') { Die "SHA256SUMS.asc does not verify: $($check.Text)" }
Write-Host "  SHA256SUMS       : regenerated and signed (SHA256SUMS.asc, key $signingKey)"

$upload = @($windows | ForEach-Object { Join-Path $signedDir $_ }) + @($sumsPath, $ascPath)

if (-not $Apply) {
    Write-Host ''
    Write-Host "CENSUS (no -Apply): nothing on GitHub changed. With -Apply this uploads to the draft for $($Tag):" -ForegroundColor Yellow
    $upload | ForEach-Object { Write-Host "    $_" }
    if ($assetNames -contains $bundle) { Write-Host "  and deletes $bundle (it attests the unsigned Windows archives)." }
    exit 0
}

# ---- 7. replace the assets on the draft, then read them back --------------------------------------------------------------
if (-not (Read-Release).isDraft) { Die "The Release for $Tag was published while this ran; nothing was uploaded." }

Write-Step "Uploading $($upload.Count) assets to the draft for $Tag"
$put = Invoke-Tool 'gh' (@('release', 'upload', $Tag) + $upload + @('--repo', $Repo, '--clobber'))
if ($put.Code -ne 0) {
    Die ("Upload FAILED: $($put.Text)`nThe signed files are in $signedDir. Finish by hand:`n" +
        "    gh release upload $Tag $($upload -join ' ') --repo $Repo --clobber`n    gh release delete-asset $Tag $bundle --repo $Repo --yes")
}

if ($assetNames -contains $bundle) {
    $drop = Invoke-Tool 'gh' @('release', 'delete-asset', $Tag, $bundle, '--repo', $Repo, '--yes')
    if ($drop.Code -ne 0) { Die "The signed assets are uploaded, but deleting $bundle FAILED: $($drop.Text)`nDelete it by hand; it attests the unsigned Windows archives." }
}

$verifyDir = Join-Path $WorkDir 'verify'
$again = Invoke-Tool 'gh' @('release', 'download', $Tag, '--repo', $Repo, '--dir', $verifyDir, '--clobber')
if ($again.Code -ne 0) { Die "Re-download for verification FAILED: $($again.Text)" }

foreach ($path in $upload) {
    $name = Split-Path $path -Leaf
    $back = Join-Path $verifyDir $name
    if (-not (Test-Path $back) -or (Get-Sha256 $back) -ne (Get-Sha256 $path)) { Die "$name on the draft is not the file uploaded" }
}
foreach ($name in $archives) {
    if ((Get-Sha256 (Join-Path $verifyDir $name)) -ne ((Get-Content (Join-Path $verifyDir 'SHA256SUMS') | Where-Object { $_ -match [regex]::Escape($name) + '$' }) -split '\s+')[0]) {
        Die "$name on the draft does not match the uploaded SHA256SUMS"
    }
}
if (Test-Path (Join-Path $verifyDir $bundle)) { Die "$bundle is still on the draft" }
if (-not (Read-Release).isDraft) { Die "The Release for $Tag is no longer a draft" }

Write-Host ''
Write-Host "  The draft for $Tag now holds the signed Windows archives, SHA256SUMS and SHA256SUMS.asc, verified by re-download." -ForegroundColor Green
Write-Host '  It holds NO attestation bundle yet. Next, attest the archives as uploaded:'
Write-Host "    gh workflow run attest-release.yml --repo $Repo -f tag=$Tag" -ForegroundColor DarkGray
Write-Host '  and publish the Release only after that workflow succeeds.'
exit 0
