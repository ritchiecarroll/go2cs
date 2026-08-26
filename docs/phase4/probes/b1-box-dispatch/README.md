# b1-box-dispatch — the P-F2 precondition microbench (point-in-time record)

The five-variant box layout/dispatch bench behind `docs/phase4/DESIGN-zh-box-b1.md` §1.
**A record, not a gate**: no solution registers it, no harness builds it, and its numbers are
frozen in the design note — re-run it only to re-derive them, and expect machine-dependent
absolutes (the decision is the relative table).

Run:  `dotnet run -c Release` (JIT arm), then
`dotnet publish -c Release -r win-x64 -p:PublishAot=true` and run the published exe (AOT arm;
needs MSVC link.exe — prepend the VS Installer dir to PATH for the SDK's vswhere probe).

`output-jit.txt` / `output-aot.txt` are the runs of record (GRETCHEN-LAPTOP, Ryzen 5 PRO 6650U,
CoreCLR/NativeAOT 10.0.11, 2026-08-26).
