# The tested nugetgo modules, with the pack arguments each is published with and the counts its proof reads. Read by
# Invoke-NugetgoRepack.ps1. A rebuild changes the package version and the digests (the converter and the closure move);
# it never changes these counts. A count that moves is a finding, posted before any owner sees a sheet.
#
# Upstream, LicenseSpdx and UpstreamPublishes are read from the published packages' nuspecs (2026-10-14); cobra's, which
# has none yet, from its source (the holder its files name, and LICENSE.txt). ConversionReadme names the root README a
# pack checks with -ConversionReadme, as <owner/repo>@<tag>:<path> in its conversion repository; empty means the module
# was packed without one. Kit names the kit the published package came from, for provenance. Expect is per package:
# matched, disclosed; re-read at go1.26.10 by P1 and P2 at 9dcf0bafe1 (2026-10-24), equal to the published counts.
# cobra's are its 2026-10-14 non-root reading; cobra/doc is validated and excluded from the package (its imports,
# go-md2man and yaml.v3, are not nugetgo packages). mousetrap has no Go tests and no row here: its proof is behavioral.
# Copyright states the holder for a license that carries no Copyright line (cobra's LICENSE.txt): one of the module's own
# source-header lines, which the pack checks (COORD ruling 2026-10-26).
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
    'github.com/spf13/cobra@v1.10.2' = @{
        RepositoryUrl     = 'https://github.com/ritchiecarroll/cobra-cs'
        Upstream          = 'The Cobra Authors'
        UpstreamPublishes = $false
        LicenseSpdx       = 'Apache-2.0'
        Copyright         = 'Copyright 2013-2023 The Cobra Authors'
        ExcludePackage    = @('github.com/spf13/cobra/doc')
        ConversionReadme  = ''
        Kit               = ''
        Expect            = @{ 'github.com/spf13/cobra' = @(405, 0); 'github.com/spf13/cobra/doc' = @(19, 0) }
    }
}
