# ildiff

Compares the **method bodies** of two builds of the same assemblies and names every method whose IL differs.
It is the claim instrument for an emission change declared *IL-identical* (the parity stage, ruled 2026-10-19).
Such a change must read `differing=0`. A change that is not IL-identical must read differences only at the
methods it names.

```
dotnet run --project src/tools/ildiff -c Release -- <baseDir> <seatDir> [--list N]
```

Every `*.dll` under `baseDir` is paired with the same relative path under `seatDir`. The last line is the
reading:

```
ILDIFF assemblies=67 methods=42315 differing=0 only-in-base=0 only-in-seat=0 missing-in-seat=0
```

The exit code is 0 only when every count after `methods` is 0.

## What it compares

For each method with a body, keyed by declaring type, name and decoded signature:

- the instructions, with every token operand resolved to the type, member, signature or string it names;
- the local variable types;
- the exception regions;
- the max stack.

Metadata row numbers are never compared. A change that only reorders metadata rows reads identical.

## Building the two sides

Build both sides **optimized**, so a debug build's `nop`s, which follow the source's line structure, do not
count. Send each project's output to its own directory with `-p:OutDir`.

`-o` is not enough. A converted project's csproj sets `OutDir` itself, so `-o` receives only the copied
references and the project's own assembly stays in its `bin`.

```
dotnet build <project>.csproj -c Debug -p:Optimize=true -p:SolutionDir=<tree>\src\ -p:OutDir=<dir>\
```

## Controls (2026-10-19, HexByteStringLiteral, 67 assemblies, 42,315 methods)

- The same source built twice reads `differing=0`.
- Dropping redundant parentheses (`a | (b << 8)` written as `a | b << 8`) reads `differing=0`.
- Regrouping (`a | (b << 8)` written as `(a | b) << 8`) reads `differing=1` and names
  `go.main_package::get4(go.string,IntPtr):IntPtr`.
