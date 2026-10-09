# Registry row pull request: github.com/golang-jwt/jwt/v5 (draft text)

Draft by C2, 2026-10-09, for COORD to adapt when the row is opened (runbook section 5). The row goes in after
`nugetgo.github.com.golang-jwt.jwt.v5` 5.3.1 is listed on nuget.org.

## The row

```
github.com/golang-jwt/jwt/v5	nugetgo.github.com.golang-jwt.jwt.v5	community	https://github.com/ritchiecarroll/jwt-cs	<date>	<contact>
```

`community`: the conversion repository is not under the module's own org. The package's description says the
conversion is unofficial and not affiliated with or endorsed by the golang-jwt maintainers or the Go project.

## What the package is

| | |
|:--|:--|
| Module | `github.com/golang-jwt/jwt/v5` v5.3.1 |
| Package | `nugetgo.github.com.golang-jwt.jwt.v5` 5.3.1, signed by the go2cs publisher |
| Assemblies | the root package and `request`. `test` (jwt's own test fixtures) is excluded from the pack by COORD ruling; `cmd/jwt` is a command, which the pack never includes |
| Built against | go.* 1.24.13.5 (22 `go.*` dependencies, each at `[1.24.13.5, 1.25.0)`) |
| Converted with | go2cs `7888e4e72f` (the 1.24.13.5 record commit with the nugetgo pack tools merged), `-platforms linux/amd64` |
| Validation | 189 matched, 0 disclosed (root 186, `request` 3); input digests `sha256-9647af3692e22d534f884f8a59f731746d35bc4c31966ad54c405af23b24eb78` (root) and `sha256-4a1b7eb540934d7a448b0026aa284ce46c76fb8391dfd946258d9647b2c0f8cb` (`request`), shipped as `VALIDATION.md` |
| Conversion source | `https://github.com/ritchiecarroll/jwt-cs` (nested layout; upstream LICENSE verbatim) |

Check 4(c) converts for the same target: `-platforms linux/amd64`. The validation run was green on its first
attempt.
