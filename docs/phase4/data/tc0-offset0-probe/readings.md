# TC0 offset-0 probe: readings

The reduced probe behind the BOARD row of 2026-11-25 (i9), "a TC0 frame reported at IL offset 0 settles at the
method's first line". `main.go` and `lg/lg.go` are the Go source as measured; `go.out` is `go run .` (go1.26.11),
with each line's trailing `runtime.main` and `runtime.goexit` frames removed for reading. The C# side was converted
in place as a behavioral-shaped project (lg, then main), built `-c Release`, and its exe run three ways. The
converted program prints only the three Go frames shown.

## Go (`go.out`)

```
method variadic  ZzCallDepthProbe/lg.(*Logger).record:11 ZzCallDepthProbe/lg.(*Logger).Log:23 main.main:11
method plain  ZzCallDepthProbe/lg.(*Logger).record:11 ZzCallDepthProbe/lg.(*Logger).Info:25 main.main:13
func variadic  ZzCallDepthProbe/lg.(*Logger).record:11 ZzCallDepthProbe/lg.Top:27 main.main:15
```

## C#, windows/amd64, converter and corpus at master 1c556c35a5

`DOTNET_TieredCompilation=0`:

```
method variadic  ZzCallDepthProbe/lg.(*Logger).record:10 ZzCallDepthProbe/lg.(*Logger).Log:23 main.main:10
method plain  ZzCallDepthProbe/lg.(*Logger).record:10 ZzCallDepthProbe/lg.(*Logger).Info:25 main.main:10
func variadic  ZzCallDepthProbe/lg.(*Logger).record:10 ZzCallDepthProbe/lg.Top:27 main.main:15
```

Tiered (the default), and tiered with `DOTNET_TieredPGO=0`: every line equals Go's.

The same three readings, byte for byte, on 1f2a9afdfc (train 4 before the glyph-methods merge), so this is older
than the forwarder chain. Hand-flipping the probe's forwarders from `AggressiveInlining` to `NoInlining` (one axis,
no reconvert) left the TC0 reading unchanged.

Two frames are wrong at TC0: `main` on both method calls reads line 10 (the `&lg.Logger{}` statement) for 11 and 13,
and the leaf `record`, suspended in `runtime.Callers`, reads 10 for 11. The package-level call's frame is right.

## The resolver's view (temporary instrumentation of internCallerFrame, removed; master tree)

TC0, `main`'s frames:

```
main il=0 callee=lgᴛLogger.Log  atCall=False settling=True targetAtIl=.ctor rs=0 targetAtRs=.ctor settled=-1 posIl=10
main il=0 callee=lgᴛLogger.Info atCall=False settling=True targetAtIl=.ctor rs=0 targetAtRs=.ctor settled=-1 posIl=10
```

Tiered, for contrast (lines right):

```
main il=5  callee=lgᴛLogger.Log  atCall=False settling=True targetAtIl=Ꮡ rs=16 settled=35  posRs=11
main il=40 callee=lgᴛLogger.Info atCall=False settling=True targetAtIl=  rs=51 settled=115 posIl=12
```

At TC0 the JIT reports `main`'s frame at IL offset 0, the method's first instruction. returnSiteILOffset reads the
first call at or after it, the Logger constructor in the line-10 statement, and calleeSettledCallSite searches only
the statement holding the reported offset, which holds no call of the callee. So nothing settles, and the line is
the method's first. The ж-overload callee arm (claude/i9-slog-calldepth-recv, the TestCallDepth cure) does not reach
this case: that cure needs the reported offset to be the call itself, as it is in log/slog's frame.
