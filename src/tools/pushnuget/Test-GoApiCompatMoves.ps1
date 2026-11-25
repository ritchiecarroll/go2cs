<#
.SYNOPSIS
    Table-driven arms for GoApiCompatMoves.psm1, which writes the suppressions of four announced breaks: public methods
    that left a package class for a class marked [GoPromotedHost(typeof(S))] or [GoMethodHost(typeof(S))], the Delta
    drop, a struct that no longer needs a value clone, and a variadic tail that became go.variadic<T>. A moved
    method must get its entry, a
    method removed any other way must get none, and the guard itself must agree with both. Exit code = the number of
    failed cases.

.DESCRIPTION
    The fixtures are two builds of one assembly (testdata/apicompat, moves-base.dll and moves-moved.dll, built by
    build-apicompat-fixtures.ps1 from src/Moves.cs). Seven forwarders and two methods of a method holder moved as the
    release announced; seven left the package class another way, each a break nobody announced. Nine variadic-tail
    entries and ten members changed some other way test the fourth kind. The generated file is then handed to the guard
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
        $moves = @($found.Entries | Where-Object { $_.Kind -ceq 'move' })
        $targets = @($moves | Where-Object { $_.Asset -ceq $lib } | ForEach-Object { $_.Target } | Sort-Object -CaseSensitive -Culture '')
        $want = @('M:go.ann_package.Named(go.ann_package.Tree)', 'M:go.ann_package.Plain(go.ann_package.Tree)', "M:go.ann_package.Plain(go.$zh{go.ann_package.Tree})",
                  'M:go.hold_package.Read(go.hold_package.Clock)', "M:go.hold_package.Reset(go.$zh{go.hold_package.Clock})",
                  'M:go.internal.abi_package.Size(go.internal.abi_package.Named)', 'M:go.pkg_package.Get``1(go.pkg_package.Box{``0},``0)',
                  'M:go.pkg_package.Val(go.pkg_package.Outer)', "M:go.pkg_package.Val(go.$zh{go.pkg_package.Outer})")
        Check 'the nine moves: value and pointer forms, a generic struct''s, one below a keyword namespace, three whose nullable attributes sit elsewhere, and a method holder''s two' (
            ($targets -join ' | ') -ceq ($want -join ' | ')) ($targets -join ' | ')
        Check 'each asset has the same nine, each a CP0002' (
            $moves.Count -eq 18 -and @($moves | Where-Object { $_.Asset -ceq $linux }).Count -eq 9 -and @($moves | Where-Object { $_.DiagnosticId -cne 'CP0002' }).Count -eq 0) "$($moves.Count) moves"
        $hosts = @($moves | ForEach-Object { $_.Host } | Sort-Object -Unique)
        $holder = 'go.hold' + [string][char]0x1D1B + 'Clock'
        Check 'each move names the class that holds the method now' (
            ($hosts -join ',') -ceq "go.forwardersOfBox,go.forwardersOfOuter,go.forwardersOfTree,$holder,go.internal.forwardersOfNamed") ($hosts -join ',')

        $refused = @($found.Refused | Where-Object { $_.Asset -ceq $lib })
        function Reason([string]$Member) {
            $needle = '.' + $Member + '('
            return [string](@($refused | Where-Object { $_.Target.Contains($needle) } | ForEach-Object { $_.Reason }) -join ' ; ')
        }
        Check 'twenty-four other removed or changed members are refused, per asset: seven moves gone wrong, five Delta-drop negative controls, three clone-drop ones and nine variadic-tail ones' ($refused.Count -eq 24 -and @($found.Refused).Count -eq 48) "$($refused.Count) of $(@($found.Refused).Count): $(@($refused | ForEach-Object { $_.Target }) -join ' | ')"
        Check 'a changed return type is not a move' ((Reason 'Changed').Contains('declares no Changed of the same signature')) (Reason 'Changed')
        Check 'a dropped attribute is not a move' ((Reason 'Unmarked').Contains('declares no Unmarked of the same signature')) (Reason 'Unmarked')
        Check 'a changed nullable annotation is not a move, and the reason shows both readings' (
            (Reason 'Kind').Contains('declares no Kind of the same signature') -and (Reason 'Kind').Contains('nullable 1 - <>') -and
            (Reason 'Kind').Contains('nullable 2 - <>')) (Reason 'Kind')
        Check 'a class without the attribute is not read' ((Reason 'Elsewhere').Contains('declares no Elsewhere')) (Reason 'Elsewhere')
        Check 'a class marked for another struct is not read' ((Reason 'Misfiled').Contains('go.forwardersOfOuter declares no Misfiled')) (Reason 'Misfiled')
        Check 'a removed method is not a move' ((Reason 'Gone').Contains('declares no Gone')) (Reason 'Gone')
        Check 'a struct with no marked class has no move' ((Reason 'Only').Contains('no class is marked GoPromotedHost(typeof(go.pkg_package+Solo)) or GoMethodHost(typeof(go.pkg_package+Solo))')) (Reason 'Only')

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

        Write-Host 'THE DELTA DROP: a rename gets the entry the engine reports, and only an exact one does'
        $d = [string][char]0x394
        $renames = @($found.Entries | Where-Object { $_.Kind -ceq 'rename' })
        $renameLib = @($renames | Where-Object { $_.Asset -ceq $lib } | ForEach-Object { "$($_.DiagnosticId) $($_.Target)" } | Sort-Object -CaseSensitive -Culture '')
        $wantRenames = @("CP0001 T:go.ren_package.${d}Stamp", "CP0001 T:go.ren_package.${d}Text", 'CP0002 F:go.ren_package.Holder.When',
                         "CP0002 M:go.ren_package.get_${d}Limit", 'CP0002 M:go.ren_package.IClock.Now', 'CP0002 M:go.ren_package.Spin(go.ren_package.Dial)', "CP0002 M:go.ren_package.Tick(go.ren_package.${d}Stamp)",
                         "CP0002 M:go.ren_package.Use(go.ren_package.${d}Stamp)", 'CP0006 M:go.ren_package.IClock.Now', 'CP0008 T:go.ren_package.Italic') |
            Sort-Object -CaseSensitive -Culture ''
        Check 'ten renames: the type, the interface, the constant by its getter, a function over the type, a method of the type now in its holder, a method whose receiver parameter dropped its prefix, a field and an interface member that name the type (CP0002 and CP0006), and a type that implements the renamed interface (CP0008)' (
            ($renameLib -join ' | ') -ceq ($wantRenames -join ' | ')) ($renameLib -join ' | ')
        Check 'each asset has the same ten renames' ($renames.Count -eq 20) "$($renames.Count) renames"
        $tick = @($renames | Where-Object { $_.Asset -ceq $lib -and $_.Target.Contains('.Tick(') })
        Check 'the receiver''s method is a rename into its holder, by the Go-named signature' (
            $tick.Count -eq 1 -and $tick[0].Host -ceq ('M:go.ren' + [string][char]0x1D1B + 'Stamp.Tick(go.ren_package.Stamp)')) "$(@($tick | ForEach-Object { $_.Host }) -join ', ')"
        $negatives = @("T:go.ren_package.${d}Orphan", "M:go.ren_package.Orphan(go.ren_package.${d}Orphan)", "M:go.ren_package.Shifted(go.ren_package.${d}Stamp)", 'M:go.ren_package.Retyped', 'M:go.ren_package.Spun(go.ren_package.Dial)')
        $refusedTargets = @($refused | ForEach-Object { $_.Target })
        Check 'NEGATIVE CONTROL: a Delta type with no Go-named successor, a parameter added, a return type changed, and a receiver parameter renamed to anything but its Go name are refused' (
            @($negatives | Where-Object { $refusedTargets -ccontains $_ }).Count -eq 5) "refused: $(@($negatives | Where-Object { $refusedTargets -ccontains $_ }) -join ' | ')"
        Check 'NEGATIVE CONTROL: no entry names any of them' (@($found.Entries | Where-Object { $negatives -ccontains $_.Target }).Count -eq 0) "$(@($found.Entries | Where-Object { $negatives -ccontains $_.Target } | ForEach-Object { $_.Target }) -join ' | ')"
        Check 'the changed return type says both types' ((@($refused | Where-Object { $_.Target -ceq 'M:go.ren_package.Retyped' })[0].Reason).Contains('changed its type from')) 'no reason'

        Write-Host 'THE CLONE DROP: a struct that lost go.IGoValueClone and its Clone(), and nothing else, gets both entries'
        $clones = @($found.Entries | Where-Object { $_.Kind -ceq 'clone' })
        $cloneLib = @($clones | Where-Object { $_.Asset -ceq $lib } | ForEach-Object { "$($_.DiagnosticId) $($_.Target)" } | Sort-Object -CaseSensitive -Culture '')
        Check 'Event: a CP0002 for its Clone() and a CP0008 for the interface' (
            ($cloneLib -join ' | ') -ceq "CP0002 M:go.cln_package.Event.${d}Clone | CP0008 T:go.cln_package.Event") ($cloneLib -join ' | ')
        Check 'each asset has the same two' ($clones.Count -eq 4) "$($clones.Count) clone entries"
        $cloneNegatives = @("M:go.cln_package.Kept.${d}Clone", "M:go.cln_package.Boxed.${d}Clone", 'T:go.cln_package.Boxed')
        Check 'NEGATIVE CONTROL: a struct that keeps the interface and loses the method, and a class that loses both, are refused' (
            @($cloneNegatives | Where-Object { $refusedTargets -ccontains $_ }).Count -eq 3) "refused: $(@($cloneNegatives | Where-Object { $refusedTargets -ccontains $_ }) -join ' | ')"
        Check 'NEGATIVE CONTROL: no entry names Kept or Boxed' (@($found.Entries | Where-Object { $_.Target.Contains('.Kept') -or $_.Target.Contains('.Boxed') }).Count -eq 0) 'an entry'

        Write-Host 'THE VARIADIC TAIL: a params System.Span<X> that became go.variadic<X>, and nothing else, gets the entries the engine reports'
        $tails = @($found.Entries | Where-Object { $_.Kind -ceq 'variadic' })
        $tailLib = @($tails | Where-Object { $_.Asset -ceq $lib } | ForEach-Object { "$($_.DiagnosticId) $($_.Target)" } | Sort-Object -CaseSensitive -Culture '')
        $wantTails = @('CP0002 M:go.vat_package.ILogger.Log(System.Span{System.Object})', 'CP0006 M:go.vat_package.ILogger.Log(go.variadic{System.Object})',
                       'CP0002 M:go.vat_package.Insert``1(System.Int32,System.Span{``0})', 'CP0002 M:go.vat_package.Ints.get_Spread',
                       'CP0006 P:go.vat_package.IHides.Elems', 'CP0006 P:go.vat_package.IHidesOf`1.Elems',
                       'CP0002 M:go.vat_package.Logf.BeginInvoke(System.String,System.Span{System.Object},System.AsyncCallback,System.Object)',
                       'CP0002 M:go.vat_package.Logf.Invoke(System.String,System.Span{System.Object})', 'CP0002 M:go.vat_package.Print(System.Span{System.Object})') |
            Sort-Object -CaseSensitive -Culture ''
        Check 'nine: a function, a generic one, a delegate''s Invoke and BeginInvoke, an interface method (CP0002 and CP0006), a spread property by its getter, and a spread property an interface adds over its base''s, plain and generic (CP0006)' (
            ($tailLib -join ' | ') -ceq ($wantTails -join ' | ')) ($tailLib -join ' | ')
        Check 'each asset has the same nine' ($tails.Count -eq 18) "$($tails.Count) variadic-tail entries"
        $print = @($tails | Where-Object { $_.Asset -ceq $lib -and $_.Target.Contains('.Print(') })
        Check 'each names the member that replaced it' ($print.Count -eq 1 -and $print[0].Host -ceq 'M:go.vat_package.Print(go.variadic{System.Object})') "$(@($print | ForEach-Object { $_.Host }) -join ', ')"
        $tailNegatives = @('M:go.vat_package.Narrowed(System.Span{System.Object})', 'M:go.vat_package.Recast(System.Span{System.Object})',
                           'M:go.vat_package.Widened(System.Int32,System.Span{System.Object})', 'M:go.vat_package.Fixed(System.Span{System.Object},System.Int32)', 'P:go.vat_package.Ints.Window',
                           'M:go.vat_package.Fixedf.Invoke(System.Span{System.Object},System.Int32)',
                           'M:go.vat_package.Fixedf.BeginInvoke(System.Span{System.Object},System.Int32,System.AsyncCallback,System.Object)', 'M:go.vat_package.Packed', 'M:go.vat_package.Overloaded(System.Span{System.Object})')
        Check 'NEGATIVE CONTROL: another span type, another element type, a second parameter changed, a span that is not params (in a method and in a delegate''s Invoke and BeginInvoke), a property that became another span type, a method whose span return became a pack, and a span overload removed beside a variadic one the baseline already had are refused' (
            @($tailNegatives | Where-Object { $refusedTargets -ccontains $_ }).Count -eq 9) "refused: $(@($tailNegatives | Where-Object { $refusedTargets -ccontains $_ }) -join ' | ')"
        Check 'NEGATIVE CONTROL: no entry names any of them, nor IWidens, whose added property hides the base spread with another span type' (@($found.Entries | Where-Object { $_.Target -match '\.(Narrowed|Recast|Widened|Fixed|Packed)\(|Window|Fixedf|\.Packed$|Overloaded|IWidens' }).Count -eq 0) 'an entry'

        $same = Get-GoApiCompatPackageMoves -Baseline $base -Current $base -WorkDir $work
        Check 'the same surface has no move and no refusal' (@($same.Entries).Count -eq 0 -and @($same.Refused).Count -eq 0) "$(@($same.Entries).Count) entries, $(@($same.Refused).Count) refused"
        $back = Get-GoApiCompatPackageMoves -Baseline $moved -Current $base -WorkDir $work
        Check 'a method that left a marked class is not a move, and a Delta GAINED is no rename' (@($back.Entries).Count -eq 0 -and @($back.Refused).Count -gt 0) "$(@($back.Entries).Count) entries ($(@($back.Entries | ForEach-Object { "$($_.DiagnosticId) $($_.Target)" }) -join ' | ')), $(@($back.Refused).Count) refused"

        Write-Host 'a renamed assembly is read against its baseline under the declared rename, as the guard compares it'
        $renamedMoved = New-Package 'renamed-moved' @{ 'lib/net10.0/go.Moves.dll' = 'moves-moved.dll'; 'runtimes/linux-x64/lib/net10.0/go.Moves.dll' = 'moves-moved.dll' }
        $rename = Get-GoApiCompatRenameSuccessors -Id 'go.Moves' -Assets @(Get-GoApiCompatAssets $base) -CurrentAssets @(Get-GoApiCompatAssets $renamedMoved) -ReleaseVersion '1.26.11.1-ci.1'
        Check 'the declaration pairs each baseline asset with go.Moves.dll beside it' (
            $rename.Declared -and $rename.Successors.Count -eq 2 -and $rename.Successors[$lib] -ceq 'lib/net10.0/go.Moves.dll' -and
            $rename.Successors[$linux] -ceq 'runtimes/linux-x64/lib/net10.0/go.Moves.dll') "declared $($rename.Declared), $($rename.Successors.Count) successors"
        $unpaired = Get-GoApiCompatPackageMoves -Baseline $base -Current $renamedMoved -WorkDir $work
        Check 'unpaired, a renamed assembly is not read at all' (@($unpaired.Entries).Count -eq 0 -and @($unpaired.Refused).Count -eq 0) "$(@($unpaired.Entries).Count) entries"
        $paired = Get-GoApiCompatPackageMoves -Baseline $base -Current $renamedMoved -WorkDir $work -Successors $rename.Successors
        $pairedMoves = @($paired.Entries | Where-Object { $_.Kind -ceq 'move' })
        $pairedTargets = @($pairedMoves | Where-Object { $_.Asset -ceq $lib } | ForEach-Object { $_.Target } | Sort-Object -CaseSensitive -Culture '')
        Check 'paired, the same nine moves, ten renames, two clone entries and nine variadic-tail entries on each asset, each entry naming the baseline asset' (
            @($paired.Entries).Count -eq 60 -and $pairedMoves.Count -eq 18 -and @($paired.Entries | Where-Object { $_.Asset -ceq $linux }).Count -eq 30 -and
            ($pairedTargets -join ' | ') -ceq ($want -join ' | ')) "$(@($paired.Entries).Count) entries: $(@($paired.Entries | ForEach-Object { $_.Asset } | Sort-Object -Unique) -join ', ')"
        Check 'another release declares no rename' (-not (Get-GoApiCompatRenameSuccessors -Id 'go.Moves' -Assets @($lib) -CurrentAssets @('lib/net10.0/go.Moves.dll') -ReleaseVersion '1.26.12.1').Declared) 'declared'
        Check 'go.lib keeps its assembly name: no successor' ((Get-GoApiCompatRenameSuccessors -Id 'go.lib' -Assets @('lib/net10.0/golib.dll') -CurrentAssets @('lib/net10.0/go.lib.dll') -ReleaseVersion '1.26.11.1').Successors.Count -eq 0) 'a successor'

        Write-Host 'THE GUARD AGREES: the generated file suppresses the moves, every entry is needed, and the rest reads red'
        $pinned = $null
        try { $pinned = Initialize-GoApiCompatTool -RepoRoot $repoRoot } catch { $pinned = $null; $why = $_.Exception.Message }
        Check 'the pinned tool restores from the manifest' ([bool]$pinned) "$why"
        if ($pinned) {
            $r = Test-GoApiCompatPackage -Id 'fixture.moves' -Baseline $base -Current $moved -SuppressionFile $null -RepoRoot $repoRoot -WorkDir $work
            Check 'unsuppressed, the guard names all fifty-five breaks on each asset' ($r.Failures.Count -eq 110 -and $r.Compared -eq 2) "$($r.Failures.Count) failures, compared $($r.Compared)"

            $generated = Join-Path $work 'generated.xml'
            Write-GoApiCompatMoveSuppressions -Entries $found.Entries -Path $generated -Comment $comment
            $r = Test-GoApiCompatPackage -Id 'fixture.moves' -Baseline $base -Current $moved -SuppressionFile $generated -RepoRoot $repoRoot -WorkDir $work
            Check 'no generated entry is unnecessary' (@($r.Failures | Where-Object { $_.Contains('unnecessary') }).Count -eq 0) "$(@($r.Failures | Where-Object { $_.Contains('unnecessary') }) -join '; ')"
            Check 'the fifty unsuppressed breaks still read red (the negative controls among them), and nothing else does' (
                $r.Failures.Count -eq 50 -and @($r.Failures | Where-Object { $_.Contains('CP0002') }).Count -eq 44 -and
                @($r.Failures | Where-Object { $_.Contains('CP0001') }).Count -eq 2 -and @($r.Failures | Where-Object { $_.Contains('CP0008') }).Count -eq 2 -and
                @($r.Failures | Where-Object { $_.Contains('CP0006') }).Count -eq 2 -and $r.Compared -eq 2) "$($r.Failures.Count) failures: $($r.Failures -join '; ')"
            foreach ($member in 'Changed', 'Unmarked', 'Kind', 'Elsewhere', 'Misfiled', 'Gone', 'Only', 'Orphan', 'Shifted', 'Retyped', 'Spun', 'Narrowed', 'Recast', 'Widened', 'Fixed', 'Overloaded') {
                $needle = '.' + $member + '('
                $named = @($r.Failures | Where-Object { $_.Contains($needle) }).Count
                Check "the guard still names $member" ($named -eq 2) "$named of 2"
            }
            foreach ($member in 'Kept', 'Boxed') {
                $named = @($r.Failures | Where-Object { $_.Contains("cln_package.$member") }).Count
                $want = if ($member -ceq 'Kept') { 2 } else { 4 }
                Check "the guard still names $member" ($named -eq $want) "$named of $want"
            }
            Check 'and no longer names Event' (@($r.Failures | Where-Object { $_.Contains('cln_package.Event') }).Count -eq 0) 'named'
            Check 'the guard still names IWidens''s added property' (@($r.Failures | Where-Object { $_.Contains('CP0006') -and $_.Contains('IWidens.Elems') }).Count -eq 2) "$(@($r.Failures | Where-Object { $_.Contains('IWidens') }) -join '; ')"
            Check 'the guard still names Window by its getter' (@($r.Failures | Where-Object { $_.Contains('Ints.Window.get') }).Count -eq 2) 'not named twice'
            Check 'the guard still names Fixedf''s Invoke and BeginInvoke, and Packed' (
                @($r.Failures | Where-Object { $_.Contains('Fixedf.Invoke(') -or $_.Contains('Fixedf.BeginInvoke(') -or $_.Contains('vat_package.Packed(') }).Count -eq 6) "$(@($r.Failures | Where-Object { $_.Contains('Fixedf') -or $_.Contains('Packed') }) -join '; ')"
            $suppressedTails = @($r.Failures | Where-Object { $_ -match 'vat_package\.(Print|Insert|Logf|ILogger|Ints\.Spread|IHides)' })
            Check 'and names no variadic tail it was given an entry for' ($suppressedTails.Count -eq 0) ($suppressedTails -join '; ')
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
