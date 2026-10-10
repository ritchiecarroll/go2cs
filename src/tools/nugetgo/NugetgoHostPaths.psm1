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

$script:latin1 = [System.Text.Encoding]::Latin1

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

Export-ModuleMember -Function Find-NugetgoHostPaths
