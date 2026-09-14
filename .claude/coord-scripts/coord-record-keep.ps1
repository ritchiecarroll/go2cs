# coord-record-keep.ps1 -- ONE definition of the coordinator's preserved-run-record naming and copy.
#
# Dot-sourced by coord-union-battery.ps1 (the sweeps leg) and coord-reflect-run.ps1 (the reflect RUN
# leg). CLAUDE.md rule 4: a gate PRESERVES a failed row's comparison record to a distinct path BEFORE
# any restore or cleanup -- deletion is for hygiene, never for evidence. Paid for on train 21, when
# the sweeps leg restored the tree after a FAILED net/http row and deleted the only record of which
# verdicts diverged; the row had to be re-measured standalone (268 s) before the train could land.
#
# The naming is SHARED on purpose. coord-reflect-run.ps1's SET DIFF finds a package's previous run by
# globbing '<scripts>/coord-pkg-run-record-<pkg.dots>-*' and taking the newest, so a record the sweeps
# leg preserved is found as "the previous run" with no hand-copying. Two independently maintained
# copies of that string would drift without a conflict (the silent-duplication rule), so there is one,
# here, and both callers ask this file for it.
#
# Layout of a preserved record (flat, so the glob's consumer finds go2cs_test_comparison.json at the
# same relative path whichever leg wrote it):
#   coord-pkg-run-record-<pkg with / as .>-<label>-<yyyyMMdd-HHmmss>\
#       go2cs_test_comparison.json     the verdict record (WHICH rows diverged)
#       go2cs_test_results.json/.xml   the host's own results (WHETHER the run was killed -- the tail)
#       go2cs_test_manifest.json       the digest, when present
#       go2cs_test_comparison\         the legacy directory form, when a run wrote one

$CoordRecordRoot = 'C:\Projects\go2cs\.claude\coord-scripts'

# The comparison record FIRST: it is the artifact rule 4 is about, and Save-CoordPkgRecord reports
# separately on whether it was among the files copied -- a preserved directory holding only a
# manifest is not evidence of anything, and must not read as a successful preservation.
$CoordRecordFiles = @(
    'go2cs_test_comparison.json',
    'go2cs_test_results.json',
    'go2cs_test_results.xml',
    'go2cs_test_manifest.json',
    'go2cs_test_comparison'
)

function Get-CoordRecordKeepPath {
    # The one spelling of the preserved-record directory name. $Stamp is accepted so a battery can
    # give every row of ONE run the same timestamp (they group in a directory listing); omitted, it
    # is taken now.
    param(
        [string] $Package,
        [string] $Label = 'run',
        [string] $Stamp = '',
        [string] $Root = ''
    )

    if ([string]::IsNullOrWhiteSpace($Package)) { throw 'Get-CoordRecordKeepPath: -Package is required' }
    if ([string]::IsNullOrWhiteSpace($Stamp)) { $Stamp = (Get-Date -Format 'yyyyMMdd-HHmmss') }
    if ([string]::IsNullOrWhiteSpace($Label)) { $Label = 'run' }
    if ([string]::IsNullOrWhiteSpace($Root))  { $Root  = $CoordRecordRoot }

    return (Join-Path $Root ('coord-pkg-run-record-' + ($Package -replace '/', '.') + '-' + $Label + '-' + $Stamp))
}

function Save-CoordPkgRecord {
    # Copy a package's pipeline record files out of the worktree to a distinct, dated path. Returns
    # $null when there was nothing to copy AND -AlwaysCreate was not asked for -- so a caller can
    # tell "preserved" from "the row produced no record", and an empty directory is never left
    # behind to read as evidence. Never throws on a missing file: a preserve step must not be the
    # thing that fails a row (the same discipline run-validated-sweep.ps1's Save-OracleEvidence keeps).
    param(
        [string] $Package,
        [string] $OutDir,          # the package's directory in the worktree, e.g. <wt>\src\core\net\http
        [string] $Label = 'run',
        [string] $Stamp = '',
        [string] $Root = '',
        [switch] $AlwaysCreate     # create (and return) the directory even when no record file exists
    )

    if ([string]::IsNullOrWhiteSpace($Package)) { throw 'Save-CoordPkgRecord: -Package is required' }
    if ([string]::IsNullOrWhiteSpace($OutDir))  { throw 'Save-CoordPkgRecord: -OutDir is required' }

    $present = @()
    foreach ($name in $CoordRecordFiles) {
        $p = Join-Path $OutDir $name
        if (Test-Path -LiteralPath $p) { $present += $name }
    }

    if ($present.Count -eq 0 -and -not $AlwaysCreate) { return $null }

    $keep = Get-CoordRecordKeepPath -Package $Package -Label $Label -Stamp $Stamp -Root $Root
    New-Item -ItemType Directory -Force -Path $keep | Out-Null

    $copied = @()
    foreach ($name in $present) {
        $p = Join-Path $OutDir $name
        Copy-Item -LiteralPath $p -Destination (Join-Path $keep $name) -Recurse -Force -ErrorAction SilentlyContinue
        if (Test-Path -LiteralPath (Join-Path $keep $name)) { $copied += $name }
    }

    return [pscustomobject] @{
        Path          = $keep
        Package       = $Package
        Files         = $copied.Count
        Copied        = $copied
        HasComparison = ($copied -contains 'go2cs_test_comparison.json') -or ($copied -contains 'go2cs_test_comparison')
    }
}

function Invoke-CoordRowRecordKeep {
    # THE DECISION, in one place so it can be controlled in both directions without re-implementing
    # it: a swept row whose verdict is not PASS has its record preserved BEFORE the caller's restore
    # and hygiene delete; a PASS row is left to the hygiene delete, unpreserved.
    #
    # The verdict is read from the sweep's EXIT CODE, not its printed word. With -Filter <pkg> -Exact
    # the row set is exactly one row, and run-validated-sweep.ps1 exits non-zero for every non-pass
    # shape it knows (FAIL, COUNT/DISC, oracle-unstable, comparison-validated-at-count) and for every
    # way the row can go NOT MEASURED (an unbanked filter throws, a toolchain refusal, a preflight
    # abort). That makes the predicate strictly wider than "the word said FAIL", which is what rule 4
    # wants: evidence is preserved whenever the row did not pass, including when it never ran.
    param(
        [string] $Package,
        [int] $ExitCode,
        [string] $SrcRoot,         # the worktree's src directory, e.g. <wt>\src
        [string] $Label = 'run',
        [string] $Stamp = '',
        [string] $Root = ''
    )

    if ($ExitCode -eq 0) { return $null }

    if ([string]::IsNullOrWhiteSpace($SrcRoot)) { throw 'Invoke-CoordRowRecordKeep: -SrcRoot is required' }
    $outDir = Join-Path $SrcRoot ('core\' + ($Package -replace '/', '\'))

    return (Save-CoordPkgRecord -Package $Package -OutDir $outDir -Label $Label -Stamp $Stamp -Root $Root)
}
