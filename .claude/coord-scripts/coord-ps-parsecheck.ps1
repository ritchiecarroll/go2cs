param([string[]] $Files)
$bad = 0
foreach ($f in $Files) {
    $tokens = $null
    $errors = $null
    [void][System.Management.Automation.Language.Parser]::ParseFile($f, [ref] $tokens, [ref] $errors)
    if ($null -ne $errors -and $errors.Count -gt 0) {
        Write-Output ("PARSE ERRORS ({0}): {1}" -f $errors.Count, $f)
        $errors | Select-Object -First 6 | ForEach-Object { Write-Output ("   line " + $_.Extent.StartLineNumber + ": " + $_.Message) }
        $bad++
    } else {
        Write-Output ("parses clean (tokens=" + $tokens.Count + "): " + $f)
    }
}
Write-Output ("PARSECHECK edition=" + $PSVersionTable.PSEdition + " version=" + $PSVersionTable.PSVersion + " files=" + $Files.Count + " bad=" + $bad)
exit $bad
