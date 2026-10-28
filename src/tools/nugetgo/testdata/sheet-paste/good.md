# Fixture: a sheet whose every PowerShell block pastes clean (Test-SheetPaste.ps1)

## Block 1: one dot-sourced statement, an else on its closing brace's line

```powershell
. {
    $x = 1
    if ($x -eq 1) {
        Write-Host 'one'
    } else {
        Write-Host 'other'
    }
}
```

A fence that is not PowerShell is not read, whatever it holds:

```text
}
else {
```

## Block 2: six statements, each continued only while it is unfinished

```pwsh
$items = @(1, 2, 3)
$items | ForEach-Object {
    $_ * 2
}
$items |
    Measure-Object
Write-Host 'continued' `
    'by a backtick'
try { Write-Host 'a' } catch { Write-Host 'b' } finally { Write-Host 'c' }
$text = @'
a here-string's lines are read to its close
'@
```
