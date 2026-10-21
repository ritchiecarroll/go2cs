# g-perf-assert-callers: G's two perf-review items, as one Go program

The adversarial perf review's items in G's area, measured the same way in Go and in the converted C#:
`runtime.Callers` capturing 8 frames, and a type assertion from `any` holding a pointer (`*T`, a raw `ж<T>` box in
C#) or a value `V`, to its concrete type and to an interface (`lener`). Each assertion sits in a `//go:noinline`
function over a package variable, so neither compiler can fold it.

## Run

Go: `go run . [iterations]` (default 2,000,000). C#: convert as a single program (`-platforms windows/amd64`, the
output directory as the second positional), build Release, then run with `DOTNET_TC_CallCountingDelayMs=0` and
20,000,000 iterations. Without that variable a short loop can finish inside the runtime's 100 ms call-counting
delay and be measured at tier 0, which reads a value assert at 2-7x its steady cost.

## Readings, G's windows box (Release, steady state)

| row | Go 1.26.10 | C# at 9924b9621c | C# with claude/g-assert-inline-head |
|---|---|---|---|
| `any(*T).(*T)` | 1.0 ns | 14.3-19.5 ns | 4.4-4.5 ns |
| `any(V).(V)` | 1.5 ns | 5.5-5.6 ns | 2.8 ns |
| `any(*T).(lener)` | 3.0 ns | 33.4-33.8 ns | 25.6-25.7 ns |
| `any(V).(lener)` | 3.0 ns | 50.7-52.1 ns | 40.9-42.1 ns |
| `runtime.Callers`, 8 frames | 263 ns | 4.23-4.27 us | (unchanged; the control row) |

Three alternating runs per arm. The base column is a fresh tree at 9924b9621c built from the same converted source.

The assertion root: `TryTypeAssert<T>` runs as one shared `__Canon` body for every reference T, and the inline head
removes that cost (see the head's comment in builtin.cs). The interface residue and the `Callers` floor are BOARD
rows of 2026-10-21.
