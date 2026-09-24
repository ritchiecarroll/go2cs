# nuget.org deprecations for the 14 package IDs Go 1.24 removed: the owner's POST-PUBLISH hand

Derived at H11 (B4; ledger 2026-09-24, CP2 `620ba7a2b8`) by name from `go2cs-stdlib.slnx` at the version tip against
the `nuget-1.23.12.3` tree, and read back from the nuget.org flat container: each removed ID has exactly these 10 versions.
**Deprecate, never unlist.** Run these AFTER 1.24.13.1 is on the feed; the alternate package must already exist, so
confirm it on the first one.

nuget.org → the package → **Manage** → **Deprecation**. The same for every row:
- **Select version(s):** all 10: 1.23.1.1, 1.23.1.2, 1.23.1.3, 1.23.1.4, 1.23.1.5, 1.23.1.6, 1.23.1.7, 1.23.12.1, 1.23.12.2, 1.23.12.3.
- **Select reason(s):** *This package is legacy and is no longer maintained*.
- **Alternate package version:** 1.24.13.1, or "any version" if the form offers it.

| # | package ID | alternate package ID | custom message |
|:-:|:--|:--|:--|
| 1 | go.crypto.internal.alias | go.crypto.internal.fips140.alias | Go 1.24 moved this package to crypto/internal/fips140/alias. Use go.crypto.internal.fips140.alias 1.24.13.1 or later; 1.23.12.3 is the last release of this ID. |
| 2 | go.crypto.internal.bigmod | go.crypto.internal.fips140.bigmod | Go 1.24 moved this package to crypto/internal/fips140/bigmod. Use go.crypto.internal.fips140.bigmod 1.24.13.1 or later; 1.23.12.3 is the last release of this ID. |
| 3 | go.crypto.internal.edwards25519 | go.crypto.internal.fips140.edwards25519 | Go 1.24 moved this package to crypto/internal/fips140/edwards25519. Use go.crypto.internal.fips140.edwards25519 1.24.13.1 or later; 1.23.12.3 is the last release of this ID. |
| 4 | go.crypto.internal.edwards25519.field | go.crypto.internal.fips140.edwards25519.field | Go 1.24 moved this package to crypto/internal/fips140/edwards25519/field. Use go.crypto.internal.fips140.edwards25519.field 1.24.13.1 or later; 1.23.12.3 is the last release of this ID. |
| 5 | go.crypto.internal.mlkem768 | go.crypto.mlkem | Go 1.24 split this package into crypto/mlkem and crypto/internal/fips140/mlkem. Use go.crypto.mlkem 1.24.13.1 or later; 1.23.12.3 is the last release of this ID. |
| 6 | go.crypto.internal.nistec | go.crypto.internal.fips140.nistec | Go 1.24 moved this package to crypto/internal/fips140/nistec. Use go.crypto.internal.fips140.nistec 1.24.13.1 or later; 1.23.12.3 is the last release of this ID. |
| 7 | go.crypto.internal.nistec.fiat | go.crypto.internal.fips140.nistec.fiat | Go 1.24 moved this package to crypto/internal/fips140/nistec/fiat. Use go.crypto.internal.fips140.nistec.fiat 1.24.13.1 or later; 1.23.12.3 is the last release of this ID. |
| 8 | go.go.internal.typeparams | none (no alternate) | Go 1.24 deleted go/internal/typeparams; it has no successor. 1.23.12.3 is the last release of this ID. |
| 9 | go.internal.concurrent | go.internal.sync | Go 1.24 moved this package to internal/sync. Use go.internal.sync 1.24.13.1 or later; 1.23.12.3 is the last release of this ID. |
| 10 | go.internal.weak | go.weak | Go 1.24 promoted this package to the public weak package. Use go.weak 1.24.13.1 or later; 1.23.12.3 is the last release of this ID. |
| 11 | go.runtime.internal.math | go.internal.runtime.math | Go 1.24 moved this package to internal/runtime/math. Use go.internal.runtime.math 1.24.13.1 or later; 1.23.12.3 is the last release of this ID. |
| 12 | go.runtime.internal.sys | go.internal.runtime.sys | Go 1.24 moved this package to internal/runtime/sys. Use go.internal.runtime.sys 1.24.13.1 or later; 1.23.12.3 is the last release of this ID. |
| 13 | go.vendor.golang.org.x.crypto.hkdf | go.crypto.hkdf | Go 1.24 moved this package into the standard library as crypto/hkdf. Use go.crypto.hkdf 1.24.13.1 or later; 1.23.12.3 is the last release of this ID. |
| 14 | go.vendor.golang.org.x.crypto.sha3 | go.crypto.sha3 | Go 1.24 split this package into crypto/sha3 and crypto/internal/fips140/sha3. Use go.crypto.sha3 1.24.13.1 or later; 1.23.12.3 is the last release of this ID. |

Every message is at most 200 characters, and every alternate is in the 51-ID NEW set (all 51 were 404, i.e. unclaimed, on the
feed at H11). After the deprecations run, the announcement's drafted follow-up sentence (docs/phase4/briefs/announcement-1.24.13.1-draft.md,
"Post-publish follow-up") may land.
