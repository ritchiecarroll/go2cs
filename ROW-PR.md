# Registry row pull request: github.com/google/uuid (draft text)

Draft by C2, 2026-10-09, for COORD to adapt when the row is opened (runbook section 5). The row goes in after
`nugetgo.github.com.google.uuid` 1.6.0 is listed on nuget.org.

## The row

```
github.com/google/uuid	nugetgo.github.com.google.uuid	community	https://github.com/ritchiecarroll/uuid-cs	<date>	<contact>
```

`community`: the conversion repository is not under the module's own org. The package's description says the
conversion is unofficial and not affiliated with or endorsed by Google Inc. or the Go project.

## What the package is

| | |
|:--|:--|
| Module | `github.com/google/uuid` v1.6.0 |
| Package | `nugetgo.github.com.google.uuid` 1.6.0, signed by the go2cs publisher |
| Built against | go.* 1.24.13.5 (18 `go.*` dependencies, each at `[1.24.13.5, 1.25.0)`) |
| Converted with | go2cs `7888e4e72f` (the 1.24.13.5 record commit with the nugetgo pack tools merged), `-platforms linux/amd64` |
| Validation | 54 matched, 0 disclosed (1 package); input digest `sha256-1d40aaa871f4e430ac651e70e05593f11e9ed13539146378156e81f13af51b28`, shipped as `VALIDATION.md` |
| Conversion source | `https://github.com/ritchiecarroll/uuid-cs` (nested layout; upstream LICENSE verbatim) |

Check 4(c) converts for the same target: `-platforms linux/amd64`.

## An upstream test race, cited and not disclosed

`TestVersion6` can fail on either runtime with "time reversed". The cause is upstream, in google/uuid v1.6.0 itself:
`NewV6` writes the version nibble over bits 12-15 of the 60-bit timestamp, and `Time()` reads those bits back
unmasked. Whenever the timestamp crosses a multiple of 0x1000 (4096 x 100 ns = 409.6 microseconds) between the
test's two `NewV6` calls, the second UUID's decoded time is lower than the first while the clock sequence is
unchanged.

- Deterministic: a Go probe that encodes two timestamps the way `NewV6` does reads `...6fff` then `...6000` for the
  pair `...5fff`/`...6000`, and no reversal for `...5ffe`/`...5fff`.
- Native Go: `go test -run 'TestVersion6$' -count=20000` failed 3 times, each "time reversed".
- go2cs: the first `-tests` run read Go=pass, C#=fail on this test only. The converted test then passed 200 of 200
  fresh-process runs on its own, and the proof is a later run in which both sides passed.

COORD ruled it cited, not disclosed (2026-10-09): a disclosure states a stable Go-versus-C# divergence, and this is an
upstream race that Go reproduces. The same note is printed on the package's proof page.
