<#
.SYNOPSIS
    push-nuget.ps1's platform-neutral check: does a package's lib/ entry differ MATERIALLY between two GoTargetOS
    flavors, or only in the fields every rebuild moves?

.DESCRIPTION
    Get-GoLibEntryKey returns a key per lib/ entry; two flavors' entries differ materially exactly when their keys
    differ. Get-GoLibEntryDifference says WHY, by name, for the warning push-nuget prints.

    THE KEY OF AN ASSEMBLY is the SHA-256 of its bytes with Roslyn's deterministic identity fields zeroed, and nothing
    else: the PE header's TimeDateStamp and CheckSum, the whole #GUID heap (the MVID; the EnC ids are empty), each debug
    directory entry's TimeDateStamp, and the data of the CodeView (the PDB id and path), PdbChecksum and embedded
    portable PDB entries, plus a strong-name signature if one were present. Those fields are hashes OF THE
    COMPILATION, and they cascade: a package whose dependency was rebuilt for another GoTargetOS moves all of them
    while every other byte stays put (measured, increment 4: ~72 bytes of a ~340 KB assembly). Every other byte is
    the meaning -- type and member references, signatures, user strings, IL, the module initializer -- so any other
    difference is material.

    WHY NOT THE LENGTH. A length test cannot see a difference that keeps the length, and PE sections pad to 512
    bytes. The 1.24.13.6 release rehearsal shipped go.internal.coverage.decodemeta with only its Windows assembly in
    lib/: its flavors reference different per-OS types (go.slice<go.syscall_package.WSABuf> on Windows) and are all
    345,600 bytes long.

    WHY NOT System.Reflection.Metadata. push-nuget.ps1 requires only Windows PowerShell 5.1, which does not ship it, so
    this module reads the PE and metadata headers itself. It reads only what it zeroes, and an image it cannot parse is
    keyed by its raw bytes: a parse failure can only promote a package, never hide a difference.

    Any other lib/ entry (an XML doc, say) is keyed by its raw bytes.
#>
#Requires -Version 5.1
Set-StrictMode -Version 2

# Debug directory entry types whose data names the compilation rather than its meaning.
$script:IdentityDebugTypes = @(
    2,   # CodeView: the PDB id (a GUID), its age and the PDB path
    17,  # EmbeddedPortablePdb: the whole PDB, which records its flavor's documents
    19   # PdbChecksum: a hash of the PDB
)

function Get-GoSha256Hex([byte[]]$Bytes) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try { return [BitConverter]::ToString($sha.ComputeHash($Bytes)).Replace('-', '') } finally { $sha.Dispose() }
}

function Clear-GoBytes([byte[]]$Bytes, [long]$Offset, [long]$Count) {
    if ($Offset -lt 0 -or $Count -le 0 -or $Offset + $Count -gt $Bytes.Length) { throw "a field at $Offset+$Count lies outside the image" }
    [Array]::Clear($Bytes, [int]$Offset, [int]$Count)
}

# Reads the PE and metadata headers. Returns $null for anything that is not a managed PE image; otherwise the image
# with its identity fields zeroed (Normalized) and the file offsets of its #Strings and #US heaps.
function Read-GoManagedImage([byte[]]$Bytes) {
    try {
        if ($Bytes.Length -lt 0x40 -or $Bytes[0] -ne 0x4D -or $Bytes[1] -ne 0x5A) { return $null }
        $pe = [BitConverter]::ToInt32($Bytes, 0x3C)
        if ($pe -lt 0 -or $pe + 24 -gt $Bytes.Length -or [BitConverter]::ToUInt32($Bytes, $pe) -ne 0x00004550) { return $null }

        $coff = $pe + 4
        $sectionCount = [BitConverter]::ToUInt16($Bytes, $coff + 2)
        $optionalSize = [BitConverter]::ToUInt16($Bytes, $coff + 16)
        $opt = $coff + 20
        $magic = [BitConverter]::ToUInt16($Bytes, $opt)
        if ($magic -eq 0x10B) { $dirs = $opt + 96 } elseif ($magic -eq 0x20B) { $dirs = $opt + 112 } else { return $null }
        $dirCount = [BitConverter]::ToUInt32($Bytes, $dirs - 4)
        if ($dirCount -le 14) { return $null }   # no CLI header directory: not a managed image

        $sections = @()
        for ($i = 0; $i -lt $sectionCount; $i++) {
            $s = $opt + $optionalSize + 40 * $i
            $sections += [pscustomobject]@{
                Va      = [long][BitConverter]::ToUInt32($Bytes, $s + 12)
                Span    = [long][Math]::Max([BitConverter]::ToUInt32($Bytes, $s + 8), [BitConverter]::ToUInt32($Bytes, $s + 16))
                RawPtr  = [long][BitConverter]::ToUInt32($Bytes, $s + 20)
            }
        }
        $toOffset = {
            param([long]$Rva)
            foreach ($s in $sections) { if ($Rva -ge $s.Va -and $Rva -lt $s.Va + $s.Span) { return $Rva - $s.Va + $s.RawPtr } }
            throw "RVA $Rva lies in no section"
        }

        $image = [byte[]]$Bytes.Clone()
        Clear-GoBytes $image ($coff + 4) 4    # COFF TimeDateStamp
        Clear-GoBytes $image ($opt + 64) 4    # optional header CheckSum

        $debugRva = [BitConverter]::ToUInt32($Bytes, $dirs + 6 * 8)
        $debugSize = [BitConverter]::ToUInt32($Bytes, $dirs + 6 * 8 + 4)
        if ($debugRva -ne 0 -and $debugSize -ne 0) {
            $debug = & $toOffset $debugRva
            for ($e = $debug; $e + 28 -le $debug + $debugSize; $e += 28) {
                Clear-GoBytes $image ($e + 4) 4    # the entry's TimeDateStamp
                $type = [BitConverter]::ToUInt32($Bytes, $e + 12)
                $dataSize = [BitConverter]::ToUInt32($Bytes, $e + 16)
                $dataPtr = [BitConverter]::ToUInt32($Bytes, $e + 24)
                if ($script:IdentityDebugTypes -contains $type -and $dataSize -gt 0) { Clear-GoBytes $image $dataPtr $dataSize }
            }
        }

        $cliRva = [BitConverter]::ToUInt32($Bytes, $dirs + 14 * 8)
        if ($cliRva -eq 0) { return $null }
        $cli = & $toOffset $cliRva
        $metadata = & $toOffset ([BitConverter]::ToUInt32($Bytes, $cli + 8))
        $snRva = [BitConverter]::ToUInt32($Bytes, $cli + 32)
        $snSize = [BitConverter]::ToUInt32($Bytes, $cli + 36)
        if ($snRva -ne 0 -and $snSize -ne 0) { Clear-GoBytes $image (& $toOffset $snRva) $snSize }

        if ([BitConverter]::ToUInt32($Bytes, $metadata) -ne 0x424A5342) { return $null }   # 'BSJB'
        $p = $metadata + 16 + [BitConverter]::ToInt32($Bytes, $metadata + 12)
        $streamCount = [BitConverter]::ToUInt16($Bytes, $p + 2)
        $p += 4
        $streams = @{}
        for ($i = 0; $i -lt $streamCount; $i++) {
            $offset = [BitConverter]::ToUInt32($Bytes, $p)
            $size = [BitConverter]::ToUInt32($Bytes, $p + 4)
            $n = $p + 8
            while ($Bytes[$n] -ne 0) { $n++ }
            $name = [System.Text.Encoding]::ASCII.GetString($Bytes, $p + 8, $n - $p - 8)
            $streams[$name] = [pscustomobject]@{ Offset = [long]($metadata + $offset); Size = [long]$size }
            $p = ($n + 4) -band -4    # the name is null-terminated and padded to a 4-byte boundary
        }
        if ($streams.ContainsKey('#GUID')) { Clear-GoBytes $image $streams['#GUID'].Offset $streams['#GUID'].Size }

        return [pscustomobject]@{
            Normalized  = $image
            Strings     = $streams['#Strings']
            UserStrings = $streams['#US']
        }
    }
    catch { return $null }
}

function Get-GoLibEntryKey {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][AllowEmptyCollection()][byte[]]$Bytes
    )

    if ($Name -like '*.dll') {
        $image = Read-GoManagedImage $Bytes
        if ($image) { return 'meaning:' + (Get-GoSha256Hex $image.Normalized) }
    }
    return 'bytes:' + (Get-GoSha256Hex $Bytes)
}

# The #Strings heap (type, member and namespace names) and the #US heap (string literals) as sets.
function Get-GoImageNames([byte[]]$Bytes, $Image) {
    $names = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::Ordinal)
    if ($Image.Strings) {
        foreach ($s in [System.Text.Encoding]::UTF8.GetString($Bytes, $Image.Strings.Offset, $Image.Strings.Size).Split([char]0)) {
            if ($s) { [void]$names.Add($s) }
        }
    }

    $literals = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::Ordinal)
    if ($Image.UserStrings) {
        $p = $Image.UserStrings.Offset + 1
        $end = $Image.UserStrings.Offset + $Image.UserStrings.Size
        while ($p -lt $end) {
            $b = $Bytes[$p]
            if (($b -band 0x80) -eq 0) { $len = $b; $p += 1 }
            elseif (($b -band 0xC0) -eq 0x80) { $len = (($b -band 0x3F) -shl 8) -bor $Bytes[$p + 1]; $p += 2 }
            else { $len = (($b -band 0x1F) -shl 24) -bor ($Bytes[$p + 1] -shl 16) -bor ($Bytes[$p + 2] -shl 8) -bor $Bytes[$p + 3]; $p += 4 }
            if ($len -eq 0) { continue }
            if ($len -gt 1) { [void]$literals.Add([System.Text.Encoding]::Unicode.GetString($Bytes, $p, $len - 1)) }
            $p += $len
        }
    }

    return [pscustomobject]@{ Names = $names; Literals = $literals }
}

function Format-GoOnlyIn([System.Collections.Generic.HashSet[string]]$Left, [System.Collections.Generic.HashSet[string]]$Right) {
    $only = @($Left | Where-Object { -not $Right.Contains($_) } | Sort-Object)
    if ($only.Count -eq 0) { return '' }
    $shown = @($only | Select-Object -First 8)
    $text = ($shown -join ', ')
    if ($only.Count -gt $shown.Count) { $text += ", and $($only.Count - $shown.Count) more" }
    return $text
}

function Get-GoLibEntryDifference {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][AllowEmptyCollection()][byte[]]$A,
        [Parameter(Mandatory)][AllowEmptyCollection()][byte[]]$B
    )

    if ((Get-GoLibEntryKey -Name $Name -Bytes $A) -eq (Get-GoLibEntryKey -Name $Name -Bytes $B)) { return $null }

    $imageA = $null
    $imageB = $null
    if ($Name -like '*.dll') { $imageA = Read-GoManagedImage $A; $imageB = Read-GoManagedImage $B }
    if (-not $imageA -or -not $imageB) { return "$Name differs in content ($($A.Length) vs $($B.Length) bytes)" }

    $first = Get-GoImageNames $A $imageA
    $second = Get-GoImageNames $B $imageB
    $parts = @()
    $x = Format-GoOnlyIn $first.Names $second.Names;       if ($x) { $parts += "names only in the first: $x" }
    $x = Format-GoOnlyIn $second.Names $first.Names;       if ($x) { $parts += "names only in the second: $x" }
    $x = Format-GoOnlyIn $first.Literals $second.Literals; if ($x) { $parts += "string literals only in the first: $x" }
    $x = Format-GoOnlyIn $second.Literals $first.Literals; if ($x) { $parts += "string literals only in the second: $x" }
    if ($parts.Count -eq 0) { $parts += "the same names and literals, but other metadata or IL differs ($($A.Length) vs $($B.Length) bytes)" }

    return "$Name -- " + ($parts -join '; ')
}

Export-ModuleMember -Function Get-GoLibEntryKey, Get-GoLibEntryDifference
