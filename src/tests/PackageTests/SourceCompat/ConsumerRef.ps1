# ConsumerRef.ps1 - the ref logic of release-smoke.ps1's arm H (source compatibility), dot-sourced by it and by
# test-consumer-ref.ps1, which exercises it against a local origin.

# The newest nuget-<a.b.c.d> release tag on origin below the version under test. A prerelease (1.26.10.4-ci.7) is below
# its own release, so it compares by its numeric part; a published version's own tag is not below it and is skipped.
# $null when origin cannot be listed or holds no such tag.
function Resolve-ConsumerRef([string]$RepoRoot, [string]$UnderTest) {
    $base = [version](($UnderTest -split '-', 2)[0])
    $ErrorActionPreference = 'Continue'
    $lines = @(& git -C $RepoRoot ls-remote --tags origin 'refs/tags/nuget-*' 2>$null)
    $code = $LASTEXITCODE
    $ErrorActionPreference = 'Stop'
    if ($code -ne 0) { return $null }
    $best = $null
    $bestVersion = $null
    foreach ($line in $lines) {
        if ($line -notmatch 'refs/tags/nuget-(\d+\.\d+\.\d+\.\d+)$') { continue }
        $tagVersion = [version]$Matches[1]
        if ($tagVersion -lt $base -and (-not $bestVersion -or $tagVersion -gt $bestVersion)) {
            $best = "nuget-$($Matches[1])"
            $bestVersion = $tagVersion
        }
    }
    return $best
}

# The commit $Ref names, without changing the repository's depth. A tag already in the repository is immutable and
# resolves with no fetch. Anything else is fetched from origin, with --depth 1 ONLY in a repository that is already
# shallow: on a full clone, --depth writes .git/shallow at the fetched commit and truncates the history of every
# worktree that shares the common directory (the fleet's gate trees share one; COORD review, 2026-11-12). A branch is
# always fetched, since a local remote-tracking ref may be stale. Returns the commit, or $null with the fetch's output
# in $Log.
function Get-ConsumerCommit([string]$RepoRoot, [string]$Ref, [string]$Log) {
    $ErrorActionPreference = 'Continue'
    try {
        & git -C $RepoRoot cat-file -e "refs/tags/$Ref^{commit}" 2>$null
        if ($LASTEXITCODE -eq 0) {
            "the tag $Ref is already in the repository: no fetch" | Set-Content -LiteralPath $Log -Encoding utf8
            return (& git -C $RepoRoot rev-parse "refs/tags/$Ref^{commit}").Trim()
        }

        $shallow = (& git -C $RepoRoot rev-parse --is-shallow-repository).Trim() -eq 'true'
        $fetch = @('-C', $RepoRoot, 'fetch', '--no-tags')
        if ($shallow) { $fetch += @('--depth', '1') }
        $fetch += @('origin', $Ref)
        & git @fetch *>&1 | ForEach-Object { "$_" } | Set-Content -LiteralPath $Log -Encoding utf8
        if ($LASTEXITCODE -ne 0) { return $null }
        return (& git -C $RepoRoot rev-parse 'FETCH_HEAD^{commit}').Trim()
    }
    finally { $ErrorActionPreference = 'Stop' }
}
