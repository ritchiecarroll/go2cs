# typecount.ps1 DIR : for every go.*.dll in DIR, count TypeDefs, closure classes (<>c__DisplayClass*), and the
# methods of IGoMethodSet-implementing parts' ᴛMethod; print per-assembly rows for the ones with closures and totals.
param([string]$Dir)
$tot = [ordered]@{ asm = 0; types = 0; display = 0; methods = 0; lambdas = 0; bytes = 0 }
$rows = @()
foreach ($f in Get-ChildItem -Path $Dir -Filter 'go.*.dll') {
  $fs = [System.IO.File]::OpenRead($f.FullName)
  try {
    $pe = [System.Reflection.PortableExecutable.PEReader]::new($fs)
    if (-not $pe.HasMetadata) { continue }
    $md = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
    $types = 0; $display = 0; $lambdas = 0
    foreach ($h in $md.TypeDefinitions) {
      $types++
      $n = $md.GetString($md.GetTypeDefinition($h).Name)
      if ($n.StartsWith('<>c__DisplayClass')) { $display++ }
    }
    foreach ($h in $md.MethodDefinitions) {
      $n = $md.GetString($md.GetMethodDefinition($h).Name)
      if ($n.Contains('>b__')) { $lambdas++ }
    }
    $tot.asm++; $tot.types += $types; $tot.display += $display; $tot.methods += $md.MethodDefinitions.Count; $tot.lambdas += $lambdas; $tot.bytes += $f.Length
    if ($display -gt 0) { $rows += "{0,-34} types={1,6} display={2,6} lambdas={3,6} bytes={4,9}" -f $f.Name, $types, $display, $lambdas, $f.Length }
  } finally { $fs.Dispose() }
}
$rows
"TOTAL asm={0} types={1} display={2} methods={3} lambdas={4} bytes={5}" -f $tot.asm, $tot.types, $tot.display, $tot.methods, $tot.lambdas, $tot.bytes
