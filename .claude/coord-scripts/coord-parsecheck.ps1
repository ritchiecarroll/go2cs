param([string] $Path)
$e = $null
$src = Get-Content -Raw $Path
$null = [System.Management.Automation.PSParser]::Tokenize($src, [ref]$e)
"real: errors=" + @($e).Count
$e2 = $null
$null = [System.Management.Automation.PSParser]::Tokenize(($src + "`nif (`$x { }"), [ref]$e2)
"control (broken copy): errors=" + @($e2).Count
