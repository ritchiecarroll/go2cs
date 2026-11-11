<#
.SYNOPSIS
    Table-driven arms for GoApiCompatMoves.psm1, which writes the suppressions of one announced break: public methods
    that left a package class for a class marked [GoPromotedHost(typeof(S))]. A moved method must get its entry, a
    method removed any other way must get none, and the guard itself must agree with both. Exit code = the number of
    failed cases.

.DESCRIPTION
    The fixtures are two builds of one assembly (testdata/apicompat, moves-base.dll and moves-moved.dll, built by
    build-apicompat-fixtures.ps1 from src/Moves.cs). Four forwarders moved as the release announced; six left the
    package class another way, each a break nobody announced. The generated file is then handed to the guard
    (GoApiCompat.psm1) as push-nuget.ps1 hands it one: every entry must suppress exactly one diagnostic, none may be
    unnecessary, and the six other members must still read red.

    The generator reads assemblies with the Roslyn PowerShell 7 ships. Windows PowerShell 5.1 ships none, and this
    suite runs there too (repoguard's TestPushnugetSuitesPassUnderWindowsPowerShell): there it checks that the module
    loads and refuses by name, and skips the arms that need the reader.
#>
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'GoApiCompat.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'GoApiCompatMoves.psm1') -Force
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$fixtures = Join-Path $PSScriptRoot 'testdata/apicompat'
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("go2cs-apicompat-moves-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $work | Out-Null

$lib = 'lib/net10.0/Moves.dll'
$linux = 'runtimes/linux-x64/lib/net10.0/Moves.dll'

# A throwaway package: each asset path maps to a fixture file.
function New-Package([string]$Name, [hashtable]$Assets) {
    $path = Join-Path $work "$Name.nupkg"
    $zip = [System.IO.Compression.ZipFile]::Open($path, 'Create')
    try {
        foreach ($asset in $Assets.Keys) {
            $entry = $zip.CreateEntry($asset)
            $stream = $entry.Open()
            try {
                $bytes = [System.IO.File]::ReadAllBytes((Join-Path $fixtures $Assets[$asset]))
                $stream.Write($bytes, 0, $bytes.Length)
            }
            finally { $stream.Dispose() }
        }
    }
    finally { $zip.Dispose() }
    $path
}

try {
    $base = New-Package 'base' @{ $lib = 'moves-base.dll'; $linux = 'moves-base.dll' }
    $moved = New-Package 'moved' @{ $lib = 'moves-moved.dll'; $linux = 'moves-moved.dll' }
    $comment = "Test entries: forwarders moved into their struct's own class."

    Write-Host 'the module says whether this PowerShell can read an assembly'
    $available = Test-GoApiCompatMovesAvailable
    Check "the answer is a boolean (PowerShell $($PSVersionTable.PSVersion): $available)" ($available -is [bool]) "$available"

    Write-Host 'the file is the guard''s format whatever wrote the entries'
    $zh = [string][char]0x436
    $one = @([pscustomobject]@{ Asset = $linux; Target = "M:go.pkg_package.Val(go.$zh{go.pkg_package.Outer})"; Host = 'x' },
             [pscustomobject]@{ Asset = $lib; Target = 'M:go.pkg_package.Val(go.pkg_package.Outer)'; Host = 'x' })
    $written = Join-Path $work 'written.xml'
    Write-GoApiCompatMoveSuppressions -Entries $one -Path $written -Comment $comment
    $bytes = [System.IO.File]::ReadAllBytes($written)
    $text = [System.Text.Encoding]::UTF8.GetString($bytes)
    Check 'no byte-order mark, and LF line ends' ($bytes[0] -eq 0x3C -and -not $text.Contains("`r")) "first byte $($bytes[0])"
    Check 'the header says what announced the entries' ($text.Contains($comment)) 'no comment'
    Check 'no package-mode baseline marker' (-not $text.Contains('IsBaselineSuppression')) 'IsBaselineSuppression'
    $read = @(Read-GoApiCompatSuppressions $written)
    Check 'the guard''s reader reads both entries, sorted by asset' (
        $read.Count -eq 2 -and $read[0].Asset -ceq $lib -and $read[1].Asset -ceq $linux -and $read[0].DiagnosticId -ceq 'CP0002') "$($read.Count) entries"
    Check 'a non-ASCII target reads back as written' ($read.Count -eq 2 -and $read[1].Target -ceq $one[0].Target) "$(if ($read.Count -eq 2) { $read[1].Target })"
    $threw = $null
    try { Write-GoApiCompatMoveSuppressions -Entries $one -Path $written -Comment 'a -- b' } catch { $threw = $_.Exception.Message }
    Check 'a comment an XML comment cannot hold is refused' ([bool]$threw) 'no refusal'

    if (-not $available) {
        Write-Host 'Windows PowerShell 5.1'
        $threw = $null
        try { Get-GoApiCompatMoves -Baseline (Join-Path $fixtures 'moves-base.dll') -Current (Join-Path $fixtures 'moves-moved.dll') | Out-Null } catch { $threw = $_.Exception.Message }
        Check 'without Roslyn the generator refuses, naming PowerShell 7' ($threw -and $threw.Contains('PowerShell 7')) "threw [$threw]"
        Write-Host "  skip  the generator's arms: this PowerShell ($($PSVersionTable.PSVersion)) ships no Roslyn to read an assembly's surface"
    }
    else {
        Write-Host 'a moved forwarder gets one entry per asset, and nothing else gets any'
        $found = Get-GoApiCompatPackageMoves -Baseline $base -Current $moved -WorkDir $work
        $targets = @($found.Entries | Where-Object { $_.Asset -ceq $lib } | ForEach-Object { $_.Target } | Sort-Object -CaseSensitive -Culture '')
        $want = @('M:go.ann_package.Named(go.ann_package.Tree)', 'M:go.ann_package.Plain(go.ann_package.Tree)', "M:go.ann_package.Plain(go.$zh{go.ann_package.Tree})",
                  'M:go.internal.abi_package.Size(go.internal.abi_package.Named)', 'M:go.pkg_package.Get``1(go.pkg_package.Box{``0},``0)',
                  'M:go.pkg_package.Val(go.pkg_package.Outer)', "M:go.pkg_package.Val(go.$zh{go.pkg_package.Outer})")
        Check 'the seven moves: value and pointer forms, a generic struct''s, one below a keyword namespace, three whose nullable attributes sit elsewhere' (
            ($targets -join ' | ') -ceq ($want -join ' | ')) ($targets -join ' | ')
        Check 'each asset has the same seven' (@($found.Entries).Count -eq 14 -and @($found.Entries | Where-Object { $_.Asset -ceq $linux }).Count -eq 7) "$(@($found.Entries).Count) entries"
        $hosts = @($found.Entries | ForEach-Object { $_.Host } | Sort-Object -Unique)
        Check 'each move names the class that holds the method now' (
            ($hosts -join ',') -ceq 'go.forwardersOfBox,go.forwardersOfOuter,go.forwardersOfTree,go.internal.forwardersOfNamed') ($hosts -join ',')

        $refused = @($found.Refused | Where-Object { $_.Asset -ceq $lib })
        function Reason([string]$Member) {
            $needle = '.' + $Member + '('
            return [string](@($refused | Where-Object { $_.Target.Contains($needle) } | ForEach-Object { $_.Reason }) -join ' ; ')
        }
        Check 'seven other removed members are refused, per asset' ($refused.Count -eq 7 -and @($found.Refused).Count -eq 14) "$($refused.Count) of $(@($found.Refused).Count): $(@($refused | ForEach-Object { $_.Target }) -join ' | ')"
        Check 'a changed return type is not a move' ((Reason 'Changed').Contains('declares no Changed of the same signature')) (Reason 'Changed')
        Check 'a dropped attribute is not a move' ((Reason 'Unmarked').Contains('declares no Unmarked of the same signature')) (Reason 'Unmarked')
        Check 'a changed nullable annotation is not a move, and the reason shows both readings' (
            (Reason 'Kind').Contains('declares no Kind of the same signature') -and (Reason 'Kind').Contains('nullable 1 - <>') -and
            (Reason 'Kind').Contains('nullable 2 - <>')) (Reason 'Kind')
        Check 'a class without the attribute is not read' ((Reason 'Elsewhere').Contains('declares no Elsewhere')) (Reason 'Elsewhere')
        Check 'a class marked for another struct is not read' ((Reason 'Misfiled').Contains('go.forwardersOfOuter declares no Misfiled')) (Reason 'Misfiled')
        Check 'a removed method is not a move' ((Reason 'Gone').Contains('declares no Gone')) (Reason 'Gone')
        Check 'a struct with no marked class has no move' ((Reason 'Only').Contains('no class is marked GoPromotedHost(typeof(go.pkg_package+Solo))')) (Reason 'Only')

        # The three nullable moves test nothing unless the compiler really wrote their attributes in other places in
        # the two fixtures; a rebuilt fixture could lose that without failing any check above.
        Add-Type -AssemblyName System.Reflection.Metadata
        function OwnContext([string]$Dll, [string]$Type, [string]$Method) {
            $stream = [System.IO.File]::OpenRead((Join-Path $fixtures $Dll))
            $image = New-Object System.Reflection.PortableExecutable.PEReader($stream)
            try {
                $reader = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($image)
                foreach ($typeHandle in $reader.TypeDefinitions) {
                    $definition = $reader.GetTypeDefinition($typeHandle)
                    if ($reader.GetString($definition.Name) -cne $Type) { continue }
                    foreach ($methodHandle in $definition.GetMethods()) {
                        $m = $reader.GetMethodDefinition($methodHandle)
                        $first = @($m.GetParameters() | ForEach-Object { $reader.GetParameter($_) } | Where-Object { $_.SequenceNumber -eq 1 })
                        if ($reader.GetString($m.Name) -cne $Method -or $first.Count -ne 1 -or $reader.GetString($first[0].Name) -cne 'target') { continue }
                        $own = $false
                        foreach ($attributeHandle in $m.GetCustomAttributes()) {
                            $constructor = $reader.GetCustomAttribute($attributeHandle).Constructor
                            if ($constructor.Kind -ne 'MemberReference') { continue }
                            $parent = $reader.GetMemberReference([System.Reflection.Metadata.MemberReferenceHandle]$constructor).Parent
                            if ($parent.Kind -eq 'TypeReference' -and
                                $reader.GetString($reader.GetTypeReference([System.Reflection.Metadata.TypeReferenceHandle]$parent).Name) -ceq 'NullableContextAttribute') { $own = $true }
                        }
                        $own
                    }
                }
            }
            finally { $image.Dispose(); $stream.Dispose() }
        }
        $placed = "Named $(OwnContext 'moves-base.dll' 'ann_package' 'Named') -> $(OwnContext 'moves-moved.dll' 'forwardersOfTree' 'Named'); " +
                  "Plain $(OwnContext 'moves-base.dll' 'ann_package' 'Plain') -> $(OwnContext 'moves-moved.dll' 'forwardersOfTree' 'Plain')"
        Check 'the fixtures differ in where NullableContext sits: Named gains its own, both forms of Plain lose theirs' (
            $placed -ceq 'Named False -> True; Plain True True -> False False') $placed

        $same = Get-GoApiCompatPackageMoves -Baseline $base -Current $base -WorkDir $work
        Check 'the same surface has no move and no refusal' (@($same.Entries).Count -eq 0 -and @($same.Refused).Count -eq 0) "$(@($same.Entries).Count) entries, $(@($same.Refused).Count) refused"
        $back = Get-GoApiCompatPackageMoves -Baseline $moved -Current $base -WorkDir $work
        Check 'a method that left a marked class is not a move' (@($back.Entries).Count -eq 0 -and @($back.Refused).Count -gt 0) "$(@($back.Entries).Count) entries, $(@($back.Refused).Count) refused"

        Write-Host 'THE GUARD AGREES: the generated file suppresses the moves, every entry is needed, and the rest reads red'
        $pinned = $null
        try { $pinned = Initialize-GoApiCompatTool -RepoRoot $repoRoot } catch { $pinned = $null; $why = $_.Exception.Message }
        Check 'the pinned tool restores from the manifest' ([bool]$pinned) "$why"
        if ($pinned) {
            $r = Test-GoApiCompatPackage -Id 'fixture.moves' -Baseline $base -Current $moved -SuppressionFile $null -RepoRoot $repoRoot -WorkDir $work
            Check 'unsuppressed, the guard names all fourteen removed members on each asset' ($r.Failures.Count -eq 28 -and $r.Compared -eq 2) "$($r.Failures.Count) failures, compared $($r.Compared)"

            $generated = Join-Path $work 'generated.xml'
            Write-GoApiCompatMoveSuppressions -Entries $found.Entries -Path $generated -Comment $comment
            $r = Test-GoApiCompatPackage -Id 'fixture.moves' -Baseline $base -Current $moved -SuppressionFile $generated -RepoRoot $repoRoot -WorkDir $work
            Check 'no generated entry is unnecessary' (@($r.Failures | Where-Object { $_.Contains('unnecessary') }).Count -eq 0) "$(@($r.Failures | Where-Object { $_.Contains('unnecessary') }) -join '; ')"
            Check 'the fourteen refused removals still read red, and nothing else does' (
                $r.Failures.Count -eq 14 -and @($r.Failures | Where-Object { $_.Contains('CP0002') }).Count -eq 14 -and $r.Compared -eq 2) "$($r.Failures.Count) failures: $($r.Failures -join '; ')"
            foreach ($member in 'Changed', 'Unmarked', 'Kind', 'Elsewhere', 'Misfiled', 'Gone', 'Only') {
                $needle = '.' + $member + '('
                $named = @($r.Failures | Where-Object { $_.Contains($needle) }).Count
                Check "the guard still names $member" ($named -eq 2) "$named of 2"
            }
        }
    }

    Write-Host 'the release path has the generator'
    $driver = Join-Path $PSScriptRoot 'New-GoApiCompatMoveSuppressions.ps1'
    Check 'New-GoApiCompatMoveSuppressions.ps1 exists and reads moves through this module' (
        (Test-Path -LiteralPath $driver) -and [System.IO.File]::ReadAllText($driver).Contains('Get-GoApiCompatPackageMoves')) 'missing, or it does not call the module'
}
finally {
    Remove-Item -LiteralPath $work -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "ran $ran, failed $failed"
exit $failed
