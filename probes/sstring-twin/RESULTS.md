# sstring twin compile probe — results (i9, 2026-09-24)

Scratch probe on master `4c53b02a0abd07dae07758acb2b3bfde2b7cccd4`, dispatched by COORD. Only golib was
built, plus the probe projects here; builds were capped at `-m:2`. Nothing under `src/` is changed on
this ref. ARM 1's operator flip was made in the scratch worktree only and restored before ARM 2.

- **ARM 0** is golib as it is: `explicit operator sstring(ReadOnlySpan<byte>)`.
- **ARM 1** flips that operator (sstring.cs:185) to `implicit`. golib compiled clean with the flip, with
  no errors outside the probe (`results/arm1/summary.txt`).

The twin pair is `F.Sprintf(@string, params ꓸꓸꓸany)` and `[OverloadResolutionPriority(1)] F.Sprintf(sstring, params ꓸꓸꓸany)`.
`G` is the same pair without the attribute, and `H` is a single-parameter `object` / `any` sink.
A bound form's overload was printed at run time (pass 2). An error form's code comes from pass 1.

| form | call | ARM 0 (explicit) | ARM 1 (implicit) |
|---|---|---|---|
| a | `F.Sprintf("x %d"u8, 1)` | **@string** | **sstring** |
| b | `@string s; F.Sprintf(s, 1)` | **sstring** | **sstring** |
| c | `F.Sprintf("x %d", 1)` (C# string) | **sstring** | **sstring** |
| d | `sstring v; F.Sprintf(v, 1)` | sstring | sstring |
| e | `Funcꓸꓸꓸ<@string, any, @string> f = F.Sprintf;` | **CS0123** (no overload for `F.Sprintf(sstring, …)` matches the delegate) | **CS0123** |
| e2 | control for e: the same conversion from `G` (no priority) | @string | @string |
| e3 | control for e: a lambda `(@string f, Span<any> a) => F.Sprintf(f, a)` | sstring (the lambda compiles; its body binds sstring) | sstring |
| e4 | control for e: `(Funcꓸꓸꓸ<@string, any, @string>)F.Sprintf` | **CS0123** | **CS0123** |
| f | `var g = F.Sprintf;` | CS8917 (delegate type could not be inferred) | CS8917 |
| g | a lambda capturing an `sstring` parameter | CS9108 (ref-like parameter inside a lambda) | CS9108 |
| h | extra: `sstring v = "x %d"u8;` | CS0266 (explicit conversion exists) | compiles |
| n1 | `G.Sprintf("x %d"u8, 1)`: the pair with **no** priority, given a u8 literal | @string | **CS0121** (ambiguous) |
| n2 | `H.TakeObject("x"u8)` | CS1503 (ReadOnlySpan<byte> → object) | CS1503 |
| n3 | `H.TakeAny("x"u8)` | CS1503 | CS1503 |

## What the table says

1. **`OverloadResolutionPriority(1)` makes the priority member win wherever it is APPLICABLE.** A better
   conversion on the other member does not matter. Because `@string → sstring` and `string → sstring`
   are implicit, an `@string` argument (b) and a C# string (c) both bind the **sstring** member in both
   arms. Only the u8 literal (a) differs between arms: ARM 0 cannot reach sstring implicitly, so it binds
   @string; ARM 1 binds sstring.
2. **Form (e) is a real hazard.** A method-group conversion to a delegate typed on `@string` fails with
   **CS0123** in both arms. It does not bind the @string member. Priority filtering runs during the
   method group's overload resolution, before the delegate-compatibility check. The sstring member is
   applicable (via `@string → sstring`), so it is chosen, and then it fails the check that requires
   parameter identity. The controls isolate the attribute as the cause:
   - e2, the same conversion without the attribute, binds @string;
   - e4, the explicit cast, fails the same way;
   - e3, a lambda wrapper, compiles.

   So a converter that emits a twin pair must also emit a lambda (or a distinct name) wherever the Go
   code takes the function as a value, e.g. `fmt.Sprintf` passed as `func(string, ...any) string`.
3. **Without the attribute, the flip creates ambiguity.** In ARM 1 a u8 literal against an unattributed
   `(@string, sstring)` pair is CS0121 (n1), exactly as section 8.2R.2 predicts. In ARM 0 the same call
   binds @string, because only one conversion is implicit.
4. **Rules that did not move:** a u8 literal into `object` / `any` is CS1503 in both arms, since a ref
   struct cannot box. Capturing an sstring parameter in a lambda is CS9108 in both arms (the ref-struct
   rule). A method group's natural type is CS8917 whenever the twin pair exists.

## ARM 2 — does RecvGenerator accept an sstring parameter?

**Yes.** `recv/` holds a `[GoType] partial struct Recv` and a `[GoRecv] PutS(this ref Recv r, sstring s)`,
with an `@string` control `Put`. They were compiled with go2cs-gen referenced as the corpus references it
(`OutputItemType="Analyzer"`). The build is clean, and the generated ж overload forwards the parameter
unchanged:

```csharp
public static nint PutS(this ж<global::go.recvprobe_package.Recv> Ꮡr, global::go.sstring s)
{
    ref var r = ref Ꮡr.DerefOrNull();
    return r.PutS(s);
}
```

The run through `ж<Recv>` printed `PutS via ж: 3`, then `Put via ж: 5`: the receiver state accumulated
through the pointer. (The probe's first build hit CS0144 in the probe's own `Main`, `new ж<Recv>(…)` on
an abstract type. The corpus's `@new<Recv>()` replaced it. That was a harness error, not a generator
finding.)

## Reproduce

`bash run-probe.sh arm0` (golib as is); flip sstring.cs:185 to `implicit`, then `bash run-probe.sh arm1`;
restore; then `dotnet build recv/RecvProbe.csproj -m:2` and run `recv/bin/Debug/net10.0/RecvProbe.dll`.
Per-arm `summary.txt`, `errors.txt` and `run.txt` are under `results/`. The raw build logs are not
committed.
