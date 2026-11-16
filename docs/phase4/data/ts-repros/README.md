# Repros from the typescript-go and sqlite `-tests` probe

The i7 TS session's `-tests -recurse` probe of 2026-11-15/16 hit three converter classes. Each folder here is a
small Go module that reproduces one of them. Native Go builds and runs every module (go1.26.11, windows/amd64).
The emissions below were read at go2cs tree `4a38d1a9bb`, which is master `0c1d295ea8` plus `26c2938653`,
`7dfbf9595e` and `fdec56d8d3` (all three are in master `2c1ce10b25`).

| Folder | Class | Go prints | Converted |
|---|---|---|---|
| `t1-genericfunc-slice-ptr-nonself` | B | `a,b 2` | builds red: CS1503 |
| `t1ctl-genericfunc-slice-iface` | B's control | `a,b 2` | builds, prints `a,b 2` |
| `t2b-testref-ext-overload` | A | `ok` (go test) | the test project builds red: CS0012 |
| `t3-promoted-iface-method-collision` | C | `true 1` | builds red: CS0102 |

## Class A: a test project misses an assembly that an extension method's signature names

`t2b/lib` declares `func (b *Backup) Commit() (driver.Conn, error)`, as modernc.org/sqlite's `backup.go` does. The
test imports `database/sql` and `t2b/lib`, but not `database/sql/driver`, and calls `(*sql.Tx).Commit`.

Every converted package is in namespace `go`, so C# overload resolution for `tx.Commit()` also considers lib's
public `Commit` extension. To rule it out, the compiler loads the assembly its result type lives in.
`t2b.tests.csproj` sets `DisableTransitiveProjectReferences` and does not reference `go.database.sql.driver`.

```
lib.cs:11        public static (driver.Conn, error) Commit(this ref Backup b) { return (default, default); }
t2b_test.cs:18   return tx.Commit();
t2b_test.cs(18,12): error CS0012: The type 'driver_package.Conn' is defined in an assembly that is not referenced.
```

The same mechanism explains these probe readings, though only pcache's was traced to its competing extension:

- sqlite `pcache`: `e2e_test.cs(108,21)` reads `tx.Commit()`, and sqlite's `backup.cs:80` is the competing extension.
- sqlite root: 73 errors naming `big_package.ΔInt`.
- typescript-go `scanner` and `ast`: errors naming `assert_package.LogT`.

An earlier repro without the second `Commit` (a test that only imports `database/sql`) validates 1/1, so it
does not reproduce the class.

## Class B: a generic function's []T argument is widened when T is inferred as the pointer

`join[T Named](xs []T, cmp func(a, b T) int)` is called with a `[]*item`, where only `*item` has `Name()`. This is
the shape of typescript-go's `tsbaseline.GetErrorBaseline(..., WrapASTDiagnostics(errors), CompareASTDiagnostics, ...)`.

```
main.cs:51  fmt.Println(join<ж<item>>(widen<ж<item>, Named>(items, elemᴛ0 => new itemжNamed(elemᴛ0)), compareItems), len(items));
main.cs(51,31): error CS1503: Argument 1: cannot convert from 'go.slice<go.main_package.Named>' to 'go.slice<go.ж<go.main_package.item>>'
```

The type argument is the pointer, but the argument is widened to the constraint's slice. The control, where
`T` is `Named` itself, emits `join<Named>(items, compareItems)`.

`join` clones into a new local before it sorts, as `iterateErrorBaseline` does. A first version reassigned the
parameter (`xs = slices.Clone(xs)`) and was refused with GO2CS2006 (a callee may write it): the may-write analysis is
flow-insensitive, and the caller's slice is never written after that reassignment.

## Class C: a promoted interface method's forwarder takes the TYPE's collision spelling

The package declares a type `Driver` and a method `Driver`, so "Driver" is in its type-vs-method collision set.
`counting` embeds `driver.Connector` and overrides `Connect` with a pointer receiver, so go2cs emits a forwarder for
the promoted `Driver()`.

```
main.cs:28  internal static driver.Driver ΔDriver(this counting recvᴛ) => recvᴛ.Connector.ΔDriver();
main.cs:19  internal static driver.Driver Driver(this @base _) { ... }      (the package's own method)
main.cs(28,31): error CS0102: The type 'main_package' already contains a definition for 'ΔDriver'
```

The `Δ` prefix is the collision set's spelling for the type; the method keeps its core-sanitized name, as
`base.Driver` shows. The forwarder in `visitStructType.go` takes `getSanitizedIdentifier`. In sqlite's
`connector_test.cs`, the test class is a different class from the one holding the type, so the same name surfaced
as CS1929 (`Connector` has no `ΔDriver`).
