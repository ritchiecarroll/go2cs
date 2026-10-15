# The five tested nugetgo modules the post-hop wave re-packs (WAVE-PLAN.md rows 1 to 5), with the pack arguments each
# was published with and the counts each kit recorded. Read by Invoke-NugetgoWaveRepack.ps1. A re-pack changes the
# package version and the digests (the converter and the closure move); it never changes these counts. A count that
# moves is a finding, posted before any owner sees a sheet.
#
# Upstream, LicenseSpdx and UpstreamPublishes are read from the published packages' nuspecs (2026-10-14).
# ConversionReadme names the README the kit packed with -ConversionReadme, as <owner/repo>@<tag>:<path> in its
# conversion repository (the kit refs of x/sync and pflag are pruned; the README at each tag is byte-identical to the
# kit's conversion/README.md, checked 2026-10-14); empty means the kit packed without one. Kit names the kit the
# package was published from, for provenance. Expect is per package: matched, disclosed.
@{
    'github.com/ritchiecarroll/hashset@v1.0.0' = @{
        RepositoryUrl     = 'https://github.com/ritchiecarroll/hashset-cs'
        Upstream          = 'The go2cs Authors'
        UpstreamPublishes = $true
        LicenseSpdx       = 'MIT'
        ExcludePackage    = @()
        ConversionReadme  = ''
        Kit               = 'claude/c2-nugetgo-hashset-rebuild-1'
        Expect            = @{ 'github.com/ritchiecarroll/hashset' = @(37, 0) }
    }
    'github.com/google/uuid@v1.6.0' = @{
        RepositoryUrl     = 'https://github.com/ritchiecarroll/uuid-cs'
        Upstream          = 'Google Inc.'
        UpstreamPublishes = $false
        LicenseSpdx       = 'BSD-3-Clause'
        ExcludePackage    = @()
        ConversionReadme  = ''
        Kit               = 'claude/c2-nugetgo-uuid-1.6.0'
        Expect            = @{ 'github.com/google/uuid' = @(54, 0) }
    }
    'github.com/golang-jwt/jwt/v5@v5.3.1' = @{
        RepositoryUrl     = 'https://github.com/ritchiecarroll/jwt-cs'
        Upstream          = 'the golang-jwt maintainers'
        UpstreamPublishes = $false
        LicenseSpdx       = 'MIT'
        ExcludePackage    = @('github.com/golang-jwt/jwt/v5/test')
        ConversionReadme  = ''
        Kit               = 'claude/c2-nugetgo-jwt-5.3.1'
        Expect            = @{ 'github.com/golang-jwt/jwt/v5' = @(186, 0); 'github.com/golang-jwt/jwt/v5/request' = @(3, 0) }
    }
    'golang.org/x/sync@v0.19.0' = @{
        RepositoryUrl     = 'https://github.com/ritchiecarroll/sync-cs'
        Upstream          = 'The Go Authors'
        UpstreamPublishes = $false
        LicenseSpdx       = 'BSD-3-Clause'
        ExcludePackage    = @()
        ConversionReadme  = 'ritchiecarroll/sync-cs@nuget-0.19.0:README.md'
        Kit               = 'claude/c2-nugetgo-xsync-0.19.0'
        Expect            = @{
            'golang.org/x/sync/errgroup' = @(5, 0); 'golang.org/x/sync/semaphore' = @(7, 1)
            'golang.org/x/sync/singleflight' = @(12, 0); 'golang.org/x/sync/syncmap' = @(3, 0)
        }
    }
    'github.com/spf13/pflag@v1.0.9' = @{
        RepositoryUrl     = 'https://github.com/ritchiecarroll/pflag-cs'
        Upstream          = 'the pflag maintainers'
        UpstreamPublishes = $false
        LicenseSpdx       = 'BSD-3-Clause'
        ExcludePackage    = @()
        ConversionReadme  = 'ritchiecarroll/pflag-cs@nuget-1.0.9:README.md'
        Kit               = 'claude/c2-nugetgo-pflag-1.0.9'
        Expect            = @{ 'github.com/spf13/pflag' = @(177, 0) }
    }
}
