# Fixture: a sheet with one clean block and four bad ones (Test-SheetPaste.ps1)

## Block 1: clean

```powershell
Write-Host 'fine'
```

## Block 2: a top-level else on the line after its closing brace

```powershell
if ($true) {
    Write-Host 'yes'
}
else {
    Write-Host 'no'
}
```

## Block 3: the same inside a dot-sourced block: it pastes, and breaks the house rule

```powershell
. {
    if ($true) {
        Write-Host 'yes'
    }
    else {
        Write-Host 'no'
    }
}
```

## Block 4: a block that ends inside an open brace

```powershell
. {
    Write-Host 'never closed'
```

## Block 5: a statement that does not parse

```powershell
Write-Host (1 + )
```
