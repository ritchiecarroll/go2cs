# IDE-mode spike inputs

These are the inputs behind [`SPIKE-ide-mode.md`](../SPIKE-ide-mode.md), kept beside the record so that the desktop
half (E2 with vsdbg, E3, E4 and the IDE part of E8) can rerun the same samples. `samples/ticker` is the E5 Hot Reload
program. `samples/stepping` is the E6/E6b stepping sample. `samples/caller` is the E7 `runtime.Caller` sample.
`samples/multi` is the 10-package module of E1 and of E8's Go-`main` shape, and `samples/shared` is the Go library of
E8's C#-consumer shape. `hosts/gomain/GoMain.csproj` and `hosts/consumer/Consumer.csproj` (with its `Program.cs`) are
the two E8 host projects. They convert `samples/multi` and `samples/shared` and expect `go2cs` on `PATH`.
`tools/linedirect.py` is the option-(b) prototype of E6, E7 and E9 (use `--mode hidden --skip-comment-records`, plus
`--goroot` for a standard-library package). `tools/swap.targets` compiles its output in place of the committed files.
`tools/stmtlines` prints the Go statement lines that E6 and E9 count coverage against. These are spike inputs, not
product code; nothing builds or tests them.
