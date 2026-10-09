# hashset v1.0.0: runbook section 3 for the owner (int.nugettest.org rehearsal)

Written by C2 on 2026-10-09 for the owner to paste on the i7. It covers `docs/NugetgoPublish.md` section 3 for
`github.com/ritchiecarroll/hashset` v1.0.0. Section 4 (sign and push to nuget.org) is not here: COORD brings it on the
consolidated owner sheet.

COORD may instead WAIVE this rehearsal. Both paths are below: **PATH A** runs it, **PATH B** records the waiver.

## What this branch holds

| Path | What it is |
|:--|:--|
| `nupkg/nugetgo.github.com.ritchiecarroll.hashset.1.0.0-int.1.nupkg` | the rehearsal package, unsigned, for int.nugettest.org only |
| `nupkg/nugetgo.github.com.ritchiecarroll.hashset.1.0.0.nupkg` | the release candidate, unsigned, for section 4 only; it never goes to int |
| `nupkg/SHA256SUMS` | both packages' SHA-256, as packed |
| `app/` | the sample program (`main.go`, `go.mod`, `go.sum`) and `go-run.out`, its `go run` output |
| `rehearsal/mappings.txt` | the one registry row, as a local mappings file (the registry has no row yet) |
| `rehearsal/nuget.config` | `nugetgo.*` from int.nugettest.org, everything else from nuget.org |
| `conversion/github.com/ritchiecarroll/hashset/` | the conversion source for the `hashset-cs` repository (runbook 4.5) |

Both packages were packed at `T` = `claude/c2-nugetgo-tools` @ `7888e4e72f`, against the published go.* 1.24.13.5,
with `-platforms linux/amd64`. Section 1: 37 matched, 0 disclosed, 5 Example declarations excluded; input digest
`sha256-7fb2058d54e18d61eaa288b0e79c847c953bdadf87cdd00fdb0d832750903114`, the same in the proof and the packed project.

For hashset the pack wrote no module `Directory.Build.targets`, because hashset's LICENSE already names The go2cs
Authors, so the csproj template's copyright applies. `conversion/` is therefore the whole of `PR/src/<module>`.

## PATH A: run the rehearsal

Paste each block in order into one pwsh 7 window, started at the root of your go2cs clone. Each block stops with a
`throw` if its step fails. Nothing here needs elevation.

### 0. Environment and the two trees

```powershell
$Repo = (git rev-parse --show-toplevel)
$Work = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-hashset-int1'
$T    = '7888e4e72f'
$Kit  = 'claude/c2-nugetgo-hashset-1.0.0'
$Id   = 'nugetgo.github.com.ritchiecarroll.hashset'
$PV   = '1.0.0-int.1'
$Int  = 'https://apiint.nugettest.org/v3/index.json'
$env:GOTOOLCHAIN = 'local'
if (Test-Path $Work) { throw "$Work already exists: remove it or choose another folder" }
New-Item -ItemType Directory $Work | Out-Null

git -C $Repo fetch origin claude/c2-nugetgo-tools $Kit
if ($LASTEXITCODE) { throw "git fetch exited $LASTEXITCODE" }
$tip = (git -C $Repo rev-parse origin/claude/c2-nugetgo-tools).Substring(0, 10)
if ($tip -ne $T) { throw "claude/c2-nugetgo-tools is at $tip, not ${T}: stop and tell COORD" }
git -C $Repo worktree add --detach "$Work\T" $T
if ($LASTEXITCODE) { throw "worktree T exited $LASTEXITCODE" }
git -C $Repo worktree add --detach "$Work\kit" "origin/$Kit"
if ($LASTEXITCODE) { throw "worktree kit exited $LASTEXITCODE" }

Get-Content "$Work\kit\nupkg\SHA256SUMS" | ForEach-Object {
    $want, $file = $_ -split '\s+', 2
    $got = (Get-FileHash -Algorithm SHA256 "$Work\kit\nupkg\$file").Hash.ToLowerInvariant()
    if ($got -ne $want) { throw "checksum mismatch: $file" }
    "checksum OK  $file"
}
```

### 3.1 Push the rehearsal package to int.nugettest.org

The key is read from a prompt. It is not typed on a command line, and PSReadLine does not record `Read-Host` input
in its history. While `dotnet nuget push` runs, the key is in that process's argument list, where a process listing on
the i7 can read it; `release-nuget.ps1` passes the nuget.org key the same way. The block removes the variable when the
push ends, whether or not it succeeded.

```powershell
$secure = Read-Host -AsSecureString 'int.nugettest.org API key'
$env:NUGETGO_INT_KEY = [Net.NetworkCredential]::new('', $secure).Password
try {
    dotnet nuget push "$Work\kit\nupkg\$Id.$PV.nupkg" --source $Int --api-key $env:NUGETGO_INT_KEY
    $rc = $LASTEXITCODE
}
finally {
    Remove-Item Env:NUGETGO_INT_KEY -ErrorAction SilentlyContinue
    $secure.Dispose()
}
if ($rc) { throw "dotnet nuget push exited $rc" }
```

### 3.2 Wait until int's flat container lists the version

Checks once a minute for up to an hour.

```powershell
$base = ((Invoke-RestMethod $Int).resources |
    Where-Object { $_.'@type' -eq 'PackageBaseAddress/3.0.0' } | Select-Object -First 1).'@id'.TrimEnd('/')
$listed = $false
foreach ($i in 1..60) {
    try { $listed = @((Invoke-RestMethod "$base/$Id/index.json").versions) -contains $PV } catch { $listed = $false }
    if ($listed) { break }
    Start-Sleep -Seconds 60
}
if (-not $listed) { throw "$Id $PV is not listed on int after 60 minutes" }
"listed on int: $Id $PV"
```

### 3.3 Convert the sample program against int

```powershell
Push-Location "$Work\T\src\go2cs"
go build -o "$Work\bin\go2cs.exe" .
$rc = $LASTEXITCODE
Pop-Location
if ($rc) { throw "go build exited $rc" }

Copy-Item -Recurse "$Work\kit\app" "$Work\app"
Copy-Item "$Work\kit\rehearsal\mappings.txt" "$Work\mappings.txt"
& "$Work\bin\go2cs.exe" -recurse=nuget -nuget-map "$Work\mappings.txt" -nuget-map-only -nuget-map-feed $Int `
    -go2cspath "$Work\T\src" "$Work\app" "$Work\AR" *> "$Work\AR.log"
$rc = $LASTEXITCODE
Get-Content "$Work\AR.log" | Select-Object -Last 12
if ($rc) { throw "go2cs exited $rc" }
```

### 3.4 Gate: the consumer half

```powershell
$log  = Get-Content -Raw "$Work\AR.log"
$lock = Get-Content -Raw "$Work\AR\go2cs.nuget.lock"
$proj = Get-Content -Raw "$Work\AR\src\example.com\hashsetdemo\example.com.hashsetdemo.csproj"
$gates = [ordered]@{
    'report reads 1 referenced as packages'      = $log -match '1 mapped, 1 referenced as packages'
    "PackageReference $Id [$PV]"                 = $proj -match [regex]::Escape("Include=""$Id"" Version=""[$PV]""")
    "lock pins $PV"                              = $lock -match "\t$([regex]::Escape($PV))\t"
    'lock records the mapping source relatively' = $lock -match '\t\.\.[\\/]mappings\.txt\t'
    'module not converted locally'               = -not (Test-Path "$Work\AR\src\github.com") -and -not (Test-Path "$Work\AR\pkg")
}
$gates.GetEnumerator() | ForEach-Object { '{0}  {1}' -f $(if ($_.Value) { 'PASS' } else { 'FAIL' }), $_.Key }
if ($gates.Values -contains $false) { throw 'a 3.4 gate failed' }
```

### 3.5 Gate: the program

`Start-Process` hands each program a file as its standard output, so both outputs are compared as the bytes each
program wrote.

```powershell
$appProject = "$Work\AR\src\example.com\hashsetdemo"
Copy-Item "$Work\kit\rehearsal\nuget.config" "$appProject\nuget.config"
$env:NUGET_PACKAGES = "$Work\packages"
$p = Start-Process dotnet -ArgumentList 'run', '-c', 'Release' -WorkingDirectory $appProject -NoNewWindow -Wait -PassThru `
    -RedirectStandardOutput "$Work\dotnet-run.out" -RedirectStandardError "$Work\dotnet-run.err"
if ($p.ExitCode) { throw "dotnet run exited $($p.ExitCode) (see $Work\dotnet-run.err)" }
$p = Start-Process go -ArgumentList 'run', '.' -WorkingDirectory "$Work\app" -NoNewWindow -Wait -PassThru `
    -RedirectStandardOutput "$Work\go-run.out" -RedirectStandardError "$Work\go-run.err"
if ($p.ExitCode) { throw "go run exited $($p.ExitCode) (see $Work\go-run.err)" }
Remove-Item Env:NUGET_PACKAGES

$hash = @{}
foreach ($f in "$Work\go-run.out", "$Work\dotnet-run.out", "$Work\kit\app\go-run.out") {
    $hash[$f] = (Get-FileHash -Algorithm SHA256 $f).Hash
    '{0}  {1}' -f $hash[$f], $f
}
if (@($hash.Values | Select-Object -Unique).Count -ne 1) { throw 'the three outputs differ' }
'3.5 PASS: dotnet run prints exactly what go run prints'
Get-ChildItem "$Work\packages" -Directory | Where-Object Name -match '^(go\.lib|nugetgo\.)' |
    ForEach-Object { '{0}  {1}' -f $_.Name, ((Get-ChildItem $_.FullName -Directory).Name -join ', ') }
```

### 3.6 What to send COORD

One reply with: the `checksum OK` lines; the push result line; the `listed on int` line; the 3.4 PASS lines; the three
hashes from 3.5 and the `3.5 PASS` line; and the last two lines of 3.5 (which versions restored). The content hash in
`go2cs.nuget.lock` is expected to differ from C2's local run, because a gallery can add its own repository signature
to the bytes it serves. Do not send `AR.log` itself: it contains local paths.

To clean up afterwards: `git -C $Repo worktree remove "$Work\T"`, the same for `"$Work\kit"`, then delete `$Work`.

## PATH B: waive the int rehearsal

If COORD and the owner waive section 3, nothing on this sheet runs, and no int.nugettest.org account or key is needed.
The record then cites C2's local equivalent, run on 2026-10-09 on a cloud lane box at `T`:

- The rehearsal package (`1.0.0-int.1`, SHA-256 `2302be7b…4ebae`) was placed in a local folder feed, and the sample
  program was converted with `-nuget-map-feed <folder>` and the same mappings row. The report read
  `1 mapped, 1 referenced as packages` and `PackageReference nugetgo.github.com.ritchiecarroll.hashset [1.0.0-int.1]`;
  the lock pinned `1.0.0-int.1` with the mapping source `../mappings.txt`; `AR/src` held only `example.com`, and
  there was no `AR/pkg`.
- Restore and run used a private `NUGET_PACKAGES` and a nuget.config mapping `nugetgo.*` to the folder and everything
  else to nuget.org. go.lib 1.24.13.5 came from nuget.org and the hashset package from the folder. `dotnet run`
  printed exactly what `go run` printed (6 lines, `app/go-run.out`).

What the waiver gives up, stated so that it is a decision and not an omission:

- The converter's package selection was not run against an HTTP V3 feed for a `nugetgo.*` package. A folder feed
  takes a different code path (`nugetFolderFeedVersions`) from the flat-container read a gallery uses.
- The run did not see the bytes a gallery serves, which can carry a repository signature the packed bytes do not.
- The first run of both is then section 6's demo, against nuget.org, after the package is published and cannot be
  deleted. Section 6 already reruns this same program, so a defect would be found there, but only after publication.
