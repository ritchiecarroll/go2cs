"echo executable on PATH=" + [bool](Get-Command echo -CommandType Application -ErrorAction SilentlyContinue)
$prev = [Console]::OutputEncoding; [Console]::OutputEncoding = [Text.Encoding]::Unicode
$running = @(& wsl.exe --list --running --quiet 2>$null | ForEach-Object { $_.Trim([char]0, ' ') } | Where-Object { $_ })
[Console]::OutputEncoding = $prev
"WSL running distros=$($running.Count) [" + ($running -join ',') + "]"
