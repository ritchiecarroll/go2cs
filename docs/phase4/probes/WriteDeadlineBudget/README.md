# WriteDeadlineBudget — does the h2 write-deadline row pass at ANY budget?

Mirrors `testWriteDeadlineEnforcedPerStream` (GOROOT `net/http/serve_test.go:1008`) in h2 mode —
`WriteTimeout = timeout/2`, first request must succeed, second must error — at budgets the real test
cannot reach. Go's `tryTimeouts` (`serve_test.go:980`) is hardcoded `{250ms, 500ms, 1s}`, so the
largest `WriteTimeout` it will ever set is **500 ms**.

The discrimination: **passes at some budget = performance gap; fails at every budget = the deadline
spans the handshake where Go's does not.**

> ⚠ **The argument is the BUDGET; `WriteTimeout` is budget/2.** The run that tests Go's real ceiling
> is `WriteDeadlineBudget 1000`, **not** `500`. Every result line prints both numbers so the mapping
> cannot be misread. This warning exists because a prose instruction of mine got it wrong and was
> caught only because the reader checked the source instead of trusting the prose.

## Measured

**G-LAPTOP** (Ryzen 7 PRO 6850U, 15–28 W mobile), three runs, identical:

| budget | `WriteTimeout` | Go | converted C# |
|---|---|---|---|
| 250 ms | 125 ms | PASS | FAIL |
| 1 s | **500 ms** | PASS | **FAIL** |
| 4 s | 2 s | PASS | PASS |
| 16 s | 8 s | PASS | PASS |

**i9 host** (desktop), three runs at the ceiling:

| budget | `WriteTimeout` | Go | converted C# |
|---|---|---|---|
| 500 ms | 250 ms | PASS | FAIL |
| 1 s | **500 ms** | PASS | **PASS** |

**Conclusion: the `/h2` divergence is a pure performance gap and is HOST-SPEED CONDITIONAL.** A
desktop clears Go's real ceiling; a low-power mobile part does not. Any claim of universal
impossibility from one host is too wide — a red generalizes no better than a green.

## Running

```
go run .                    # all four budgets
go run . 1000               # just the ceiling case
go2cs -go2cspath <repo>/src <this-dir>
dotnet build -c Debug -p:go2csPath=<repo>/src/ -p:UseSharedCompilation=false
./bin/Debug/net10.0/WriteDeadlineBudget.exe [budget-ms]
```

Three runs per side, agreeing, before any number leaves the host.
