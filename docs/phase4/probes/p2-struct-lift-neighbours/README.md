# p2-struct-lift-neighbours -- three shapes where one unnamed struct type becomes two C# types (point-in-time record)

The sqlite-cs W12 seat records a conversion wherever a value moves between two lifts of one unnamed struct type
in one file (BOARD row of 2026-11-15, "a struct value moves between a named type, an unnamed type and a second
lift of the unnamed type"). These six one-site packages are the shapes that seat leaves open. Each is legal Go,
prints 3, converts with exit 0, and does not compile as C#. P2, 2026-11-15, Go 1.26.11, converter at the W12
seat's first commit on master `b2214e00e4`. Not a gate.

## The shapes and what the C# compiler says

| Package | Shape | Error |
|---|---|---|
| `xvar` | a function's variable in `aa.go`, its value a result of a function in `zz.go` | CS0029, `Src_r` to `Run_x` |
| `xpkgvar` | an exported package variable in `aa.go` initialized from one in `zz.go` | CS0029, `V0ᴛ1` to `Xᴛ1` |
| `aptr` | `*struct{ Hz int }` from a function declared below | CS0029, `ж<psrc_R0>` to `ж<Run_p>` |
| `aslice` | `[]struct{ Hz int }` from a function declared below | CS0029, `slice<all_R0>` to `slice<Run_s>` |
| `gpub` | an exported package variable written below an unexported one of the same type | CS0052, CS0053 |
| `gbox` | an exported type's field written below an unexported variable of the same type | CS0050, CS0051, CS0052 |

- **Cross-file (`xvar`, `xpkgvar`).** A lift's name is known per file, so the pass that records a move between
  two lifts sees only one of the two names. Nothing is recorded and the assignment has no conversion.
- **Behind a pointer or a slice (`aptr`, `aslice`).** The two element types are two lifts, and no operator can
  convert `ж<A>` to `ж<B>` or `slice<A>` to `slice<B>`. A map and a channel of the type are the same case. The
  cure is one lift, in the lifter.
- **Exported reuse (`gpub`, `gbox`).** There is ONE lift here: the exported declaration adopts the lift the
  unexported one registered above it, `v0ᴛ1`, which is `internal`, and a public member cannot name it.

## To reproduce

`go run .` prints six lines, each ending in 3. Convert each package and then the root with a second positional
(the output directory), for example into a copy of this directory, and build the root project:

```
go2cs -go2cspath <repo>/src -platforms linux/amd64 <copy>/xvar <copy>/xvar      (and the other five, then <copy>)
dotnet build -c Release -p:go2csPath=<repo>/src/ -p:GoTargetOS=linux
```

Read with the seat's converter inside a 39-package probe, the build reported the errors in the table and no
other; with these six taken out, the other 33 packages (forms the seat covers, and one-lift controls) built and
printed Go's output.
