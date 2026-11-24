<#
.SYNOPSIS
    Finds a host root -- the pack's build root, its scratch, the module cache, the user profile -- inside packed bytes, so a
    package never publishes the path of the machine that built it (the nugetgo rehearsal, 2026-10-09, gap 3).
.DESCRIPTION
    An assembly carries a path as a UTF-8 string (an attribute argument, the pdb path in its debug directory) or a UTF-16LE
    one (a user string); a pdb and a text file carry either. A root is looked for in each encoding (UTF-16LE at both byte
    alignments), with either separator, ignoring letter case, and only at a path boundary: the character after it is a
    separator or not a path character, so /x/pkg/mod does not match /x/pkg/modules. A portable pdb keeps each document
    name as separator-joined segments in its blob heap, where no byte run spells the path, so its Document table is read
    as well.
#>

# ISO-8859-1 by code page: the Encoding.Latin1 property is .NET 5 and later, $null on the .NET Framework that Windows
# PowerShell 5.1 runs on (the T3 battery's 5.1 leg, 2026-10-11).
$script:latin1 = [System.Text.Encoding]::GetEncoding(28591)

# A portable pdb's Document table is read with System.Reflection.Metadata. PowerShell 7 carries it; .NET Framework does
# not, so Windows PowerShell 5.1 has it only where the GAC does. Without it a pdb is REFUSED by name, never passed unread.
if (-not ('System.Reflection.Metadata.MetadataReaderProvider' -as [type])) {
    try { Add-Type -AssemblyName System.Reflection.Metadata -ErrorAction Stop } catch { }
}
$script:PdbReaderMissing = -not ('System.Reflection.Metadata.MetadataReaderProvider' -as [type])

function Get-NugetgoRootSpellings([string]$root) {
    $trimmed = $root.Trim().TrimEnd('/', '\')
    if ($trimmed.Length -eq 0) { return @() }
    @($trimmed.Replace('\', '/'), $trimmed.Replace('/', '\')) | Select-Object -Unique
}

function Test-NugetgoPathBoundary([string]$text, [int]$end) {
    if ($end -ge $text.Length) { return $true }
    $next = $text[$end]
    return ($next -eq '/' -or $next -eq '\' -or -not ([char]::IsLetterOrDigit($next) -or $next -eq '.' -or $next -eq '_' -or $next -eq '-'))
}

function Test-NugetgoContainsRoot([string]$text, [string]$spelling) {
    $at = 0
    while (($at = $text.IndexOf($spelling, $at, [StringComparison]::OrdinalIgnoreCase)) -ge 0) {
        if (Test-NugetgoPathBoundary $text ($at + $spelling.Length)) { return $true }
        $at++
    }
    return $false
}

# The document names of a portable pdb (metadata signature BSJB at offset 0), joined by newlines; empty for anything else.
function Get-NugetgoPdbDocumentNames([byte[]]$bytes) {
    if ($bytes.Length -lt 4 -or $bytes[0] -ne 0x42 -or $bytes[1] -ne 0x53 -or $bytes[2] -ne 0x4A -or $bytes[3] -ne 0x42) { return '' }
    if ($script:PdbReaderMissing) {
        throw "a portable pdb's document names are read with System.Reflection.Metadata, which this PowerShell ($($PSVersionTable.PSVersion)) cannot load: run the pack under PowerShell 7"
    }
    try {
        $provider = [System.Reflection.Metadata.MetadataReaderProvider]::FromPortablePdbStream([System.IO.MemoryStream]::new($bytes))
        try {
            $reader = $provider.GetMetadataReader()
            return (@(foreach ($handle in $reader.Documents) { $reader.GetString($reader.GetDocument($handle).Name) }) -join "`n")
        }
        finally { $provider.Dispose() }
    }
    catch { return '' }
}

<#
.SYNOPSIS
    Returns each of -Roots that -Bytes names, in the form it was given; empty when none.
#>
function Find-NugetgoHostPaths([byte[]]$Bytes, [string[]]$Roots) {
    if ($null -eq $Bytes -or $Bytes.Length -eq 0) { return @() }
    $views = @(
        @{ Text = $script:latin1.GetString($Bytes); Encode = { param($s) $script:latin1.GetString([System.Text.Encoding]::UTF8.GetBytes($s)) } },
        @{ Text = [System.Text.Encoding]::Unicode.GetString($Bytes); Encode = { param($s) $s } },
        @{ Text = $(if ($Bytes.Length -gt 1) { [System.Text.Encoding]::Unicode.GetString($Bytes, 1, $Bytes.Length - 1) } else { '' }); Encode = { param($s) $s } },
        @{ Text = (Get-NugetgoPdbDocumentNames $Bytes); Encode = { param($s) $s } }
    )
    $found = New-Object System.Collections.Generic.List[string]
    foreach ($root in @($Roots | Where-Object { $_ -and $_.Trim() })) {
        $hit = $false
        foreach ($spelling in (Get-NugetgoRootSpellings $root)) {
            foreach ($view in $views) {
                if (Test-NugetgoContainsRoot $view.Text (& $view.Encode $spelling)) { $hit = $true; break }
            }
            if ($hit) { break }
        }
        if ($hit -and -not $found.Contains($root)) { $found.Add($root) }
    }
    return $found.ToArray()
}

# ---- the converter's own temp markers (COORD's INC 4 review, fold 5) -----------------------------------------------------
# A go.mod-less module-cache release converts from a copy under the temp directory (go2cs-modcopy-*), and the -tests driver
# writes a go.mod overlay (go2cs-gomod-overlay*). Neither may reach a packed byte. They are found as plain text, in UTF-8
# and in UTF-16LE at both alignments: the root scan above needs a path boundary after a root, and a marker is followed by
# the temp name's random digits. The temp directory itself is no root, since a Go string such as "/tmp" is legitimate
# packed content.
$script:TempMarkers = @('go2cs-modcopy-', 'go2cs-gomod-overlay')

<#
.SYNOPSIS
    Returns each converter temp marker that -Bytes names; empty when none.
#>
function Find-NugetgoTempMarkers([byte[]]$Bytes) {
    if ($null -eq $Bytes -or $Bytes.Length -eq 0) { return @() }
    $views = @($script:latin1.GetString($Bytes), [System.Text.Encoding]::Unicode.GetString($Bytes),
        $(if ($Bytes.Length -gt 1) { [System.Text.Encoding]::Unicode.GetString($Bytes, 1, $Bytes.Length - 1) } else { '' }))
    @($script:TempMarkers | Where-Object { $marker = $_; @($views | Where-Object { $_.Contains($marker) }).Count })
}

# ---- GoPositionMap identities (cgo against go.* INC 4, increment 1c; COORD's ruling 2026-11-22) -------------------------
# A GoPositionMap record's first argument names the Go file a frame was converted from, in one of goSourceIdentity's forms:
# GOROOT-relative (`fmt/print.go`), `<module>@<version>/<path>`, `<module>@/<path>` (the -trimpath forms) or a bare
# `<file>.go`. None names a directory of the converting machine. An ABSOLUTE identity does (an untrimmed source path, or
# the hashed Go build-cache file the -cgo route once recorded for a tree cmd/cgo rewrote), so it never ships: the host-path
# scan above only finds the roots it is told, and a build cache is not one of them.

<#
.SYNOPSIS
    Why -Identity may not ship, or $null when it may: it is rooted in either OS's spelling, or it names a Go build-cache
    entry (`<2 hex>/<64 hex>-d`, the file cmd/cgo writes there).
#>
function Test-NugetgoPositionMapIdentity([string]$Identity) {
    if ($Identity -match '(^|[\\/])[0-9a-f]{2}[\\/][0-9a-f]{64}-[a-z]$') { return "a Go build-cache path ($Identity)" }
    if ($Identity -match '^([\\/]|[A-Za-z]:)') { return "an absolute path ($Identity)" }
    return $null
}

<#
.SYNOPSIS
    The GoPositionMap identities a package_info.cs declares, in order.
#>
function Get-NugetgoSourcePositionMapIdentities([string]$Text) {
    @([regex]::Matches($Text, 'GoPositionMap\("((?:[^"\\]|\\.)*)"') | ForEach-Object { [regex]::Unescape($_.Groups[1].Value) })
}

# A custom attribute's constructor names its type through a MemberReference (the attribute lives in go.lib) or a
# MethodDefinition (it is declared in the assembly itself).
function Get-NugetgoAttributeTypeName($reader, $ctorHandle) {
    $kind = $ctorHandle.Kind.ToString()
    if ($kind -eq 'MemberReference') {
        $parent = $reader.GetMemberReference([System.Reflection.Metadata.MemberReferenceHandle]$ctorHandle).Parent
        if ($parent.Kind.ToString() -ne 'TypeReference') { return '' }
        $type = $reader.GetTypeReference([System.Reflection.Metadata.TypeReferenceHandle]$parent)
    }
    elseif ($kind -eq 'MethodDefinition') {
        $declaring = $reader.GetMethodDefinition([System.Reflection.Metadata.MethodDefinitionHandle]$ctorHandle).GetDeclaringType()
        $type = $reader.GetTypeDefinition($declaring)
    }
    else { return '' }
    "$($reader.GetString($type.Namespace)).$($reader.GetString($type.Name))"
}

<#
.SYNOPSIS
    The GoPositionMap identities an assembly carries: the first argument of each assembly-level go.GoPositionMapAttribute,
    read from its metadata. Empty for bytes that are not a managed assembly.
#>
function Get-NugetgoAssemblyPositionMapIdentities([byte[]]$Bytes) {
    if ($script:PdbReaderMissing) {
        throw "an assembly's GoPositionMap records are read with System.Reflection.Metadata, which this PowerShell ($($PSVersionTable.PSVersion)) cannot load: run the pack under PowerShell 7"
    }
    $pe = [System.Reflection.PortableExecutable.PEReader]::new([System.IO.MemoryStream]::new($Bytes))
    try {
        if (-not $pe.HasMetadata) { return @() }
        $reader = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
        if (-not $reader.IsAssembly) { return @() }
        $identities = New-Object System.Collections.Generic.List[string]
        foreach ($handle in $reader.GetAssemblyDefinition().GetCustomAttributes()) {
            $attribute = $reader.GetCustomAttribute($handle)
            if ((Get-NugetgoAttributeTypeName $reader $attribute.Constructor) -cne 'go.GoPositionMapAttribute') { continue }
            # ECMA-335 II.23.3: the prolog 0x0001, then the first fixed argument, a SerString -- a compressed length
            # (0xFF for null) and that many UTF-8 bytes.
            $blob = $reader.GetBlobBytes($attribute.Value)
            $at = 2
            $first = $blob[$at]
            if ($first -eq 0xFF) { $identities.Add(''); continue }
            if (($first -band 0x80) -eq 0) { $length = $first; $at += 1 }
            elseif (($first -band 0xC0) -eq 0x80) { $length = (($first -band 0x3F) -shl 8) -bor $blob[$at + 1]; $at += 2 }
            else { $length = (($first -band 0x1F) -shl 24) -bor ($blob[$at + 1] -shl 16) -bor ($blob[$at + 2] -shl 8) -bor $blob[$at + 3]; $at += 4 }
            $identities.Add([System.Text.Encoding]::UTF8.GetString($blob, $at, $length))
        }
        return $identities.ToArray()
    }
    catch [System.BadImageFormatException] { return @() }
    finally { $pe.Dispose() }
}

<#
.SYNOPSIS
    The reasons the GoPositionMap identities of a package_info.cs text (-Text) or an assembly (-Bytes) may not ship; empty
    when every one may.
#>
function Find-NugetgoPositionMapIdentityHits([string]$Text, [byte[]]$Bytes) {
    $identities = if ($PSBoundParameters.ContainsKey('Bytes')) { Get-NugetgoAssemblyPositionMapIdentities $Bytes } else { Get-NugetgoSourcePositionMapIdentities $Text }
    @($identities | ForEach-Object { Test-NugetgoPositionMapIdentity $_ } | Where-Object { $_ })
}

Export-ModuleMember -Function Find-NugetgoHostPaths, Find-NugetgoTempMarkers, Test-NugetgoPositionMapIdentity, Get-NugetgoSourcePositionMapIdentities, Get-NugetgoAssemblyPositionMapIdentities, Find-NugetgoPositionMapIdentityHits
