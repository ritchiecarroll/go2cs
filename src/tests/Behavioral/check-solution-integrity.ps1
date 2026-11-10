<#
.SYNOPSIS
    Project-graph integrity guard, in two parts: (1) the set of behavioral test .csproj files on disk
    is exactly the set registered in go2cs.slnx, and (2) the emitted src\core project-reference graph
    is ACYCLIC for every target. Catches a test (or sibling library sub-project) that was
    committed but never added to the solution, and a converter emission that closes a reference cycle.

.DESCRIPTION
    A behavioral test can pull in a sibling library project via <ProjectReference> -- e.g.
    GoNamespaceShadow references nsshadowlib\go.nsshadow.csproj. The BehavioralRunner / MSTest harness
    builds each test .csproj *by path* with $(SolutionDir) set, so a transitive ProjectReference
    resolves and the suite stays green EVEN WHEN the referenced project is missing from go2cs.slnx.
    The gap only bites when go2cs.slnx is opened/built in Visual Studio: the unregistered project is
    not a solution member, so VS builds it without the Debug+SolutionDir context and its $(go2csPath)
    references (core\golib, core\math, gen\go2cs-gen) fail to resolve (CS0246/CS0234). That is exactly
    how nsshadow slipped through (added in 96eff53cd, never registered until 53dd2497e).

    The invariant that prevents this: every .csproj physically under tests\Behavioral is registered as
    a <Project Path="..."> in go2cs.slnx, and vice-versa. This script checks that equality both ways:
      - on disk but NOT in the solution  =>  forgot to register it (the build-breaking gap)
      - in the solution but NOT on disk  =>  dangling registration (renamed / deleted project)

    A second, related invariant guards the PATH CASING of the tracked tree: every tracked path under the
    behavioral tree must be spelled exactly src/tests/Behavioral/... On Windows a `git add .` / `git add -A`
    records the ON-DISK directory casing, so a drifted src\Tests silently banks a test at
    src/Tests/Behavioral/... -- the same directory here, a SECOND one on a case-sensitive filesystem, where
    the solution's lowercase registration stops resolving. Checks 1-3 cannot see it (they compare
    case-insensitively on a case-insensitive filesystem), so this one reads the git index directly.

    The FIFTH invariant is about a different graph and a different failure: the emitted corpus's
    PROJECT-REFERENCE graph must be acyclic. C# project references are compile-time edges, so a cycle
    is not a slow build -- it is MSB4006 ("there is a circular dependency"), and every project in the
    cycle stops building. Go's own import graph is acyclic by construction, so an emission derived
    only from imports cannot produce one; what can is a reference the converter introduces that Go's
    graph does not contain -- a `//go:linkname` forwarding property, whose ProjectReference points
    wherever the directive names, in either direction. That is W1
    (docs\phase4\DESIGN-linkname-push-cycles.md): a `-tests` conversion of `runtime` emitted
    `runtime -> internal/syscall/windows`, and because Go's own imports contain
    `internal/syscall/windows -> syscall -> runtime`, the result is a cycle NO conversion order can
    undo. It was found by hand, and nothing in the tree would have caught it.

    The invariant this makes mechanical is narrower than "-tests must not rewrite the production
    emission" (which contradicts the standing restore doctrine -- four documented closure families
    legitimately differ) and sharper than "the push must not add a reference":

        A -tests conversion's production emission may differ from -stdlib's only in ways that do
        not change the project GRAPH.

    So this check reads the emitted .csproj files themselves -- the artifact, not the converter's
    intent -- and DFSes them once per target (GOOS and GOARCH), because the <ItemGroup> blocks gated
    on $(GoTargetOS), $(GoTargetArch) or both make each target a DIFFERENT graph (layout L3,
    docs\phase4\DESIGN-multiplatform-corpus.md). Every target of the universe is checked: a cycle on
    the linux/arm64 graph is just as fatal to that build as a windows one is to the corpus reference
    target.

    POSITIVE CONTROL (a green that cannot go red is not a measurement). -InjectReference adds an edge
    to the parsed graph in memory, so the assertion can be made to fail on demand without touching a
    tracked file:

        ./check-solution-integrity.ps1 -Target windows/amd64 -InjectReference 'runtime=internal/syscall/windows'

    must report EXACTLY these six cycles and exit 1 -- W1's own set, independently reproduced in
    DESIGN-linkname-push-cycles.md section 1.3:

        errors -> internal/reflectlite -> runtime -> internal/syscall/windows -> errors
        runtime -> internal/syscall/windows -> runtime
        runtime -> internal/syscall/windows -> sync -> runtime
        errors -> internal/reflectlite -> runtime -> internal/syscall/windows -> syscall -> errors
        errors -> internal/reflectlite -> runtime -> internal/syscall/windows -> syscall -> internal/oserror -> errors
        runtime -> internal/syscall/windows -> syscall -> runtime

    Pure static analysis over the file tree, the .slnx text, the git index and the corpus .csproj XML:
    no build, no transpile, no run (~6s for all three target flavors on the i7-5820K, of which the
    cycle DFS is ~4s). Runs automatically as the preflight step of check-no-regression.ps1; also
    runnable standalone.

.EXAMPLE
    ./check-solution-integrity.ps1

.EXAMPLE
    # Positive control for the cycle assertion -- must print the six W1 cycles and exit 1.
    ./check-solution-integrity.ps1 -Target windows/amd64 -InjectReference 'runtime=internal/syscall/windows'
#>
[CmdletBinding()]
param(
    # The targets to DFS, <goos>/<arch>: by default the corpus's universe, the converter's platformUniverse
    # (src\go2cs\platformLattice.go), which the repoguard test TestLayoutKeyListsAreTheUniverses holds this
    # line to. Each is a different graph: an <ItemGroup> gated on $(GoTargetOS), $(GoTargetArch) or both
    # contributes references only to the targets it names. A bare GOOS (the -TargetOS spelling this
    # parameter had) means every default target of that GOOS.
    [Alias('TargetOS')]
    [string[]] $Target = @('windows/amd64', 'linux/amd64', 'darwin/amd64', 'linux/arm64'),

    # Positive-control edges, '<fromPackage>=<toPackage>' with package paths relative to src\core
    # (e.g. 'runtime=internal/syscall/windows'). Added to the parsed graph in memory only.
    [string[]] $InjectReference = @()
)

$ErrorActionPreference = "Stop"

# State the exit code rather than inheriting it. $ErrorActionPreference='Stop' makes an unhandled
# error terminate the SCRIPT, but a script-terminating error leaves $LASTEXITCODE untouched -- so a
# caller that reads $LASTEXITCODE (which is exactly how check-no-regression.ps1 preflights this file)
# would see the 0 from whatever ran before. That is false-green route #6 in miniature: an instrument
# that could not finish reporting success. Any error reaching here is a failed check.
trap {
    Write-Host "==> check-solution-integrity ABORTED on an unhandled error:" -ForegroundColor Red
    Write-Host "    $_" -ForegroundColor Red
    exit 1
}

. (Join-Path $PSScriptRoot '_paths.ps1')

$behavioral = $BehavioralRoot
$srcRoot    = $SrcRoot
$slnxPath   = (Resolve-Path (Join-Path $srcRoot "go2cs.slnx")).Path

# 1. Every .csproj physically under tests\Behavioral. -Recurse descends into sibling library folders
#    (nsshadowlib\, netlike\, inner\, FsLike\, ...). Rendered as a src-relative forward-slash path so
#    it lines up with the go2cs.slnx <Project Path="..."> form. Includes the two tooling projects
#    (BehavioralTests, BehavioralRunner) -- they are legitimate solution members, so they appear on
#    both sides and cancel.
$onDisk = Get-ChildItem -Path $behavioral -Recurse -Filter *.csproj -File |
    ForEach-Object { Get-RelativeDisplayPath $_.FullName $srcRoot } |
    Sort-Object

# 1a. F8 (2026-09-02): a behavioral package marked [GoPlatformExclusive("...")] in its package_info.cs whose
#     native platforms do NOT include windows is EXEMPT from go2cs.slnx -- the solution builds the windows
#     flavour of the corpus on every host, so such a package (ScmRightsSeam: unix-only syscall API) cannot
#     compile there on ANY host; it is built only by the native-host runner, by path. The marker regex is
#     line-anchored (the attribute name appears in prose) and mirrors src/tests/PlatformExclusive.cs.
#     Windows-native exclusives (FindFirstFileData and the other five) stay registered: they compile in the
#     windows flavour everywhere and only their Output phase is host-bound. A registered non-windows-native
#     package is reported as a violation (it turns the slnx gate red by construction -- measured at the
#     train-11 union battery, 23 errors, all ScmRightsSeam).
$markerRegex = [regex]'(?m)^\s*\[(?:go\.)?GoPlatformExclusive\s*\(([^)]*)\)\]'
$exempt = @()
foreach ($rel in $onDisk) {
    $dir  = Split-Path -Parent (Join-Path $srcRoot $rel)
    $info = Join-Path $dir 'package_info.cs'
    if (-not (Test-Path -LiteralPath $info)) { continue }
    $m = $markerRegex.Match([System.IO.File]::ReadAllText($info))
    if (-not $m.Success) { continue }
    $platforms = @([regex]::Matches($m.Groups[1].Value, '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value.ToLowerInvariant() })
    if ($platforms.Count -gt 0 -and ($platforms -notcontains 'windows')) { $exempt += $rel }
}
if ($exempt.Count -gt 0) {
    Write-Host "==> EXEMPT from go2cs.slnx (platform-exclusive, not windows-native; built only by the native-host runner):" -ForegroundColor DarkYellow
    $exempt | ForEach-Object { Write-Host "    $_" }
    $onDisk = @($onDisk | Where-Object { $_ -notin $exempt })
}

# 2. Every tests/Behavioral/*.csproj registered in go2cs.slnx. Folder elements use Name="..." (not
#    Path=), so this Path-anchored pattern matches only <Project> entries.
$slnxText   = Get-Content -Raw -LiteralPath $slnxPath
$registered = [regex]::Matches($slnxText, 'Path="(tests/Behavioral/[^"]+\.csproj)"') |
    ForEach-Object { $_.Groups[1].Value } |
    Sort-Object -Unique

# 3. Compare both directions.
$missing  = @($onDisk     | Where-Object { $_ -notin $registered })  # on disk, not in the solution
$dangling = @($registered | Where-Object { $_ -notin $onDisk })      # in the solution, not on disk

$ok = $true

$wrongRegistered = @($exempt | Where-Object { $_ -in $registered })
if ($wrongRegistered.Count -gt 0) {
    Write-Host "==> REGISTERED but not windows-native -- cannot compile in the solution's windows flavour on any host; remove from go2cs.slnx:" -ForegroundColor Red
    $wrongRegistered | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
    $ok = $false
}

if ($missing.Count -gt 0) {
    $ok = $false
    Write-Host "==> NOT REGISTERED in go2cs.slnx -- add a <Project Path=`"...`" /> line under" -ForegroundColor Red
    Write-Host "    the /tests/behavioral/target-projects/ folder for each:" -ForegroundColor Red
    $missing | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
}

if ($dangling.Count -gt 0) {
    $ok = $false
    Write-Host "==> DANGLING go2cs.slnx registration -- no such .csproj on disk (renamed/deleted?):" -ForegroundColor Red
    $dangling | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
}

# 4. Case-integrity guard. On Windows (core.ignorecase=true) `git add .` / `git add -A` records the path it
#    gets from readdir -- the ON-DISK casing -- so a clone whose src\tests had drifted to a capital src\Tests
#    banks the new test under src/Tests/Behavioral/..., which is indistinguishable from src/tests/... locally
#    but a SECOND directory on any case-sensitive filesystem (Linux clone, container CI, case-sensitive macOS
#    volume), where go2cs.slnx's lowercase tests/Behavioral/... registration no longer resolves. That is how
#    DeferFrameScopes was committed (repaired 2026-08-07). Checks 1-3 above cannot see it: they run on a
#    case-insensitive filesystem and match case-insensitively. So assert against the INDEX, case-sensitively.
$tracked = $null
try {
    $tracked = & git -C $srcRoot ls-files --full-name
    if ($LASTEXITCODE -ne 0) { $tracked = $null }
}
catch { $tracked = $null }

if ($null -eq $tracked) {
    Write-Host "==> NOTE: git unavailable (or not a repository) -- skipped the path-casing check." -ForegroundColor Yellow
}
else {
    # Every tracked path in the behavioral tree must be spelled exactly "src/tests/Behavioral/".
    $miscased = @($tracked | Where-Object { $_ -imatch '^src/tests/behavioral/' -and $_ -cnotmatch '^src/tests/Behavioral/' })

    if ($miscased.Count -gt 0) {
        $ok = $false
        Write-Host "==> MIS-CASED TRACKED PATHS -- these are a separate directory on a case-sensitive" -ForegroundColor Red
        Write-Host "    filesystem and will not resolve from go2cs.slnx there:" -ForegroundColor Red
        $miscased | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
        Write-Host "    Repair (index-only; `git mv` cannot do a case-only rename on Windows):" -ForegroundColor Red
        Write-Host "      git update-index --force-remove <wrong-cased-path>" -ForegroundColor Red
        Write-Host "      git update-index --add --cacheinfo 100644,<sha>,<correct-path>   # sha from git ls-tree -r HEAD" -ForegroundColor Red
        Write-Host "    Then fix the ON-DISK directory casing too (rename via a temp name), or the next" -ForegroundColor Red
        Write-Host "    'git add -A' re-creates it. See CLAUDE.md 'Adding a regression test' step 3." -ForegroundColor Red
    }
}

# 5. Corpus project-reference CYCLE assertion (see .DESCRIPTION). Everything below reads emitted
#    .csproj XML and answers one question: is the reference graph acyclic for each target?

# A bare GOOS names every default target of that GOOS; anything else must be one default target. The
# defaults are read back from this script's own param block, so the universe is written once.
$defaultTargets = @(([System.Management.Automation.Language.Parser]::ParseFile($PSCommandPath, [ref]$null, [ref]$null)).ParamBlock.Parameters |
    Where-Object { $_.Name.VariablePath.UserPath -eq 'Target' }).DefaultValue.SafeGetValue()

$Target = @(foreach ($requested in $Target) {
    $matched = @($defaultTargets | Where-Object { $_ -eq $requested -or ($_ -split '/')[0] -eq $requested })
    if ($matched.Count -eq 0) { throw "-Target '$requested' is neither a target of the universe ($($defaultTargets -join ', ')) nor a GOOS of one" }
    $matched
})

# The MSBuild condition forms that gate a reference group by target: '$(GoTargetOS)'=='<goos>',
# '$(GoTargetArch)'=='<arch>', or the two joined by And. Anything else naming either property is
# REJECTED rather than guessed at: a condition this parser half-understands would silently model the
# wrong edge set, and a cycle assertion over the wrong graph is worse than none.
$goosConditionPattern = "^'\`$\(GoTargetOS\)'\s*==\s*'([A-Za-z0-9_]+)'$"
$archConditionPattern = "^'\`$\(GoTargetArch\)'\s*==\s*'([A-Za-z0-9_]+)'$"

# A project's Go-import-path-shaped name: the directory it lives in, relative to src\core (so
# core/internal/syscall/windows -> internal/syscall/windows, core/golib -> golib). Projects outside
# core keep their src-relative path (gen/go2cs-gen), which is what makes them visible in a cycle.
# Returns $null when the referenced project does not exist -- a dangling reference, which the caller
# reports. Both answers are cached: the same ~5,000 edges are resolved once per target OS.
$projectNodeCache = @{}

function Get-ProjectGraphNodeName {
    param(
        [Parameter(Mandatory)][string] $ProjectPath,
        [Parameter(Mandatory)][string] $SrcRoot
    )

    if ($projectNodeCache.ContainsKey($ProjectPath)) { return $projectNodeCache[$ProjectPath] }

    $name = $null

    if (Test-Path -LiteralPath $ProjectPath -PathType Leaf) {
        $relative = Get-RelativeDisplayPath ([System.IO.Path]::GetDirectoryName($ProjectPath)) $SrcRoot
        $name = if ($relative -like 'core/*') { $relative.Substring('core/'.Length) } else { $relative }
    }

    $projectNodeCache[$ProjectPath] = $name
    return $name
}

# Every <ProjectReference Include="..."> in one .csproj, resolved to a full path and TAGGED with the
# $(GoTargetOS) and $(GoTargetArch) its <ItemGroup> is gated on ($null = not gated on it, i.e. every
# target). Parsed once per file and cached, because one graph per target is built from the same files
# and re-reading them per target is most of the runtime. $(go2csPath) is the src root in a Debug build (each csproj sets it
# from $(SolutionDir)), which is how the corpus spells every cross-package reference.
$projectReferenceCache = @{}

function Get-ProjectReferenceEdges {
    param(
        [Parameter(Mandatory)][string] $ProjectPath,
        [Parameter(Mandatory)][string] $SrcRoot
    )

    if ($projectReferenceCache.ContainsKey($ProjectPath)) { return $projectReferenceCache[$ProjectPath] }

    $xml = New-Object System.Xml.XmlDocument
    $xml.LoadXml([System.IO.File]::ReadAllText($ProjectPath))

    $projectDir = [System.IO.Path]::GetDirectoryName($ProjectPath)
    $edges      = New-Object System.Collections.Generic.List[psobject]

    foreach ($group in $xml.Project.ChildNodes) {
        if ($group.NodeType -ne [System.Xml.XmlNodeType]::Element -or $group.Name -ne 'ItemGroup') { continue }

        $condition = $group.GetAttribute('Condition')
        $goos      = $null
        $arch      = $null

        if ($condition -match 'GoTargetOS|GoTargetArch') {
            foreach ($term in ($condition.Trim() -split '\s+[Aa][Nn][Dd]\s+')) {
                if ($null -eq $goos -and $term -match $goosConditionPattern) { $goos = $Matches[1] }
                elseif ($null -eq $arch -and $term -match $archConditionPattern) { $arch = $Matches[1] }
                else {
                    $form = "'`$(GoTargetOS)'=='<goos>', '`$(GoTargetArch)'=='<arch>' or the two joined by And"
                    throw "Unrecognized target condition in $ProjectPath -- this parser models only $form, and got: $condition"
                }
            }
        }
        # Any other condition ('$(OutputType)'=='Library', Exists(...)) holds for every corpus
        # package, so its items are unconditional here. Erring toward MORE edges is the safe
        # direction: it can only over-report a cycle, never hide one.

        foreach ($item in $group.ChildNodes) {
            if ($item.NodeType -ne [System.Xml.XmlNodeType]::Element -or $item.Name -ne 'ProjectReference') { continue }

            $include = $item.GetAttribute('Include')
            if ([string]::IsNullOrWhiteSpace($include)) { continue }

            $include = $include -replace '\$\(go2csPath\)', ($SrcRoot.TrimEnd('\', '/') + '/')

            $resolved = if ([System.IO.Path]::IsPathRooted($include)) { $include } else { Join-Path $projectDir $include }
            $edges.Add([pscustomobject]@{ TargetOS = $goos; TargetArch = $arch; Path = [System.IO.Path]::GetFullPath($resolved) })
        }
    }

    $projectReferenceCache[$ProjectPath] = $edges
    return $edges
}

# The whole reachable project graph for one target, keyed by node name. Seeded from every
# non-test .csproj under src\core -- a <pkg>.tests.csproj is a graph SINK (nothing references one),
# so excluding them changes no cycle while keeping the node set exactly the production corpus -- then
# expanded transitively so a reference OUT of core (gen/go2cs-gen) is followed too.
function Get-CorpusProjectGraph {
    param(
        [Parameter(Mandatory)][string] $SrcRoot,
        [Parameter(Mandatory)][string] $CoreRoot,
        [Parameter(Mandatory)][string] $GraphTarget
    )

    $graphOS, $graphArch = $GraphTarget -split '/'

    $adjacency = @{}   # node name -> sorted node names
    $nodePath  = @{}   # node name -> .csproj full path
    $unresolved = New-Object System.Collections.Generic.List[string]
    $pending   = New-Object System.Collections.Generic.Queue[string]

    Get-ChildItem -Path $CoreRoot -Recurse -Filter *.csproj -File |
        Where-Object { $_.Name -notlike '*.tests.csproj' } |
        ForEach-Object { $pending.Enqueue($_.FullName) }

    while ($pending.Count -gt 0) {
        $projectPath = $pending.Dequeue()
        $node = Get-ProjectGraphNodeName $projectPath $SrcRoot

        if ($adjacency.ContainsKey($node)) {
            if ($nodePath[$node] -ne $projectPath) {
                throw "Two project files claim the graph node '$node': $($nodePath[$node]) and $projectPath"
            }
            continue
        }

        $adjacency[$node] = @()
        $nodePath[$node]  = $projectPath

        $edges = @()
        foreach ($edge in (Get-ProjectReferenceEdges $projectPath $SrcRoot)) {
            if ($null -ne $edge.TargetOS -and $edge.TargetOS -ne $graphOS) { continue }
            if ($null -ne $edge.TargetArch -and $edge.TargetArch -ne $graphArch) { continue }

            $target = Get-ProjectGraphNodeName $edge.Path $SrcRoot
            if ($null -eq $target) {
                $unresolved.Add("$node -> $(Get-RelativeDisplayPath $edge.Path $SrcRoot)")
                continue
            }

            $edges += $target
            $pending.Enqueue($edge.Path)
        }

        $adjacency[$node] = @($edges | Sort-Object -Unique)
    }

    return [pscustomobject]@{
        Adjacency  = $adjacency
        Unresolved = $unresolved
    }
}

# Depth-first search, coloring white/gray/black. An edge into a GRAY node is a back edge, i.e. a
# cycle; it is reported as the current stack from that node onward, which names every project the
# cycle actually runs through rather than just the two ends.
function Find-ProjectGraphCycles {
    param(
        [Parameter(Mandatory)][string] $Node,
        [Parameter(Mandatory)][hashtable] $Adjacency,
        [Parameter(Mandatory)][hashtable] $Color,
        # AllowEmptyCollection, and NOT Mandatory: a Mandatory collection parameter refuses an EMPTY
        # one, and both of these start empty on the first call of every DFS root.
        [AllowEmptyCollection()][System.Collections.Generic.List[string]] $Stack,
        [AllowEmptyCollection()][System.Collections.Generic.List[string]] $Cycles
    )

    $Color[$Node] = 'gray'
    $Stack.Add($Node)

    foreach ($next in $Adjacency[$Node]) {
        $state = $Color[$next]

        if ($state -eq 'gray') {
            $from = $Stack.IndexOf($next)
            $path = @()
            for ($i = $from; $i -lt $Stack.Count; $i++) { $path += $Stack[$i] }
            $path += $next
            $Cycles.Add($path -join ' -> ')
        }
        elseif ($null -eq $state) {
            Find-ProjectGraphCycles -Node $next -Adjacency $Adjacency -Color $Color -Stack $Stack -Cycles $Cycles
        }
    }

    $Stack.RemoveAt($Stack.Count - 1)
    $Color[$Node] = 'black'
}

$coreRoot = Join-Path $srcRoot 'core'

foreach ($graphTarget in $Target) {
    $graph      = Get-CorpusProjectGraph $srcRoot $coreRoot $graphTarget
    $adjacency  = $graph.Adjacency

    foreach ($injection in $InjectReference) {
        $parts = $injection -split '=', 2
        if ($parts.Count -ne 2) { throw "-InjectReference expects '<fromPackage>=<toPackage>', got: $injection" }
        if (-not $adjacency.ContainsKey($parts[0])) { throw "-InjectReference source is not a corpus project: $($parts[0])" }
        if (-not $adjacency.ContainsKey($parts[1])) { throw "-InjectReference target is not a corpus project: $($parts[1])" }
        $adjacency[$parts[0]] = @($adjacency[$parts[0]] + $parts[1] | Sort-Object -Unique)
    }

    if ($graph.Unresolved.Count -gt 0) {
        # A reference to a project file that does not exist is broken on its own terms, and it also
        # makes the cycle answer below unsound: the missing project's own edges are invisible, so a
        # cycle running through it cannot be seen. Fail rather than report a hollow zero.
        $ok = $false
        Write-Host "==> UNRESOLVED ProjectReference ($graphTarget) -- the graph is incomplete, so the" -ForegroundColor Red
        Write-Host "    cycle assertion below cannot be trusted until these resolve:" -ForegroundColor Red
        $graph.Unresolved | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
    }

    $color  = @{}
    $stack  = New-Object System.Collections.Generic.List[string]
    $cycles = New-Object System.Collections.Generic.List[string]

    foreach ($node in ($adjacency.Keys | Sort-Object)) {
        if (-not $color.ContainsKey($node)) {
            Find-ProjectGraphCycles -Node $node -Adjacency $adjacency -Color $color -Stack $stack -Cycles $cycles
        }
    }

    if ($cycles.Count -gt 0) {
        $ok = $false
        Write-Host "==> PROJECT-REFERENCE CYCLE ($graphTarget) -- $($cycles.Count) cycle(s); every project" -ForegroundColor Red
        Write-Host "    on one of these paths fails to build with MSB4006:" -ForegroundColor Red
        $cycles | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
        if ($InjectReference.Count -eq 0) {
            Write-Host "    A C# project reference is a compile-time edge, so no conversion ORDER can undo a" -ForegroundColor Red
            Write-Host "    cycle -- the emitted edge itself has to go. See docs/phase4/DESIGN-linkname-push-cycles.md." -ForegroundColor Red
        }
    }
    elseif ($graph.Unresolved.Count -eq 0) {
        Write-Host "==> PROJECT GRAPH OK ($graphTarget): 0 cycles across $($adjacency.Count) projects." -ForegroundColor Green
    }
}

if ($ok) {
    Write-Host "==> SOLUTION INTEGRITY OK: all $(@($onDisk).Count) behavioral projects are registered in go2cs.slnx." -ForegroundColor Green
    if ($null -ne $tracked) {
        Write-Host "==> PATH CASING OK: all $(@($tracked | Where-Object { $_ -imatch '^src/tests/behavioral/' }).Count) tracked behavioral paths are spelled src/tests/Behavioral/." -ForegroundColor Green
    }
    exit 0
}

Write-Host ""
Write-Host "Solution integrity check FAILED (see CLAUDE.md 'Adding a regression test' step 3)." -ForegroundColor Yellow
exit 1
