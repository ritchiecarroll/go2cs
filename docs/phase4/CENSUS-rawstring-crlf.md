# CENSUS: multi-line raw strings and the CRLF checkout (scout for the fix form)

Dated 2026-10-12, lane P2, read at master 226afb810a. linux, uid 0, Microsoft .NET 10.0.12, SDK 10.0.112,
go1.24.13. A SCOUT, not a seat: no emission is changed on this branch. The fix form is the owner's call.

## The defect, as measured

A Go raw string never holds a CR. The converter (`getStringLiteral`, `src/go2cs/typePredicates.go`) emits a raw
string that contains a newline as a C# multi-line raw literal (`"""..."""u8`). It writes LF inside the
literal and CRLF for its own line ends, so the EMITTED file is correct. But `.gitattributes` pins `*.cs` to
`text eol=crlf`, so every CHECKOUT, on every OS, materialises CRLF inside the literal. C# takes a raw literal's
line breaks from the source file, so a build of the checkout holds `\r\n` where Go holds `\n`.

A probe (len, %q, ==, CR count and an FNV hash of a PEM-shaped const) matched Go as emitted and in an all-LF form,
and DIFFERED in the checkout form: 18 "line one\r\nline two" false 1 against Go's 17 "line one\nline two" true 0.

A `-tests` run re-emits the package's production and test `.cs` before building it (measured on net/rpc), so the
banked roster verdicts were measured on the correct form. Exposed is anything built from the checkout without a
reconvert: the published packages (8 production files), a test host built from committed `_test.cs`, and a
behavioral project compiled without its transpile phase. `.claude/rules/harness-gates.md` already records the
behavioral half; the `-text` marks on Solitaire, SortArrayType and StdLibInternalAbi are its workaround.

## 1. Census by C# context: how much shape (a) costs

564 literals in 121 files: 558 in `src/core` (111 `_test.cs`, 8 production files) and 6 in Behavioral
(Solitaire, SortArrayType).

| C# context | literals |
|:--|--:|
| `static readonly` field initialiser | 298 |
| call argument | 105 |
| other expression position (concatenation operand, named argument, return) | 111 |
| non-readonly field or local initialiser | 50 |
| **constant-only position** (const declaration, case label, attribute argument, default parameter value, constant pattern) | **0** |

The zero is compile-proven, not only pattern-matched. 557 of the 564 carry the `u8` suffix, which types them
`ReadOnlySpan<byte>`, and that is never a C# constant. The other 7 are C# `string` operands of a `+` chain with a
non-constant variable: go/build `read_test.cs` x3, internal/trace/traceviewer `http.cs` x2, and Behavioral
SortArrayType and Solitaire. A Go string constant is emitted `static readonly @string` (the probe's `const pem` was), never a C# `const`. So (b)
covers every site, and no site needs (a) for constant reasons.

## 2. The (b) form: strip CR at run time

Go's own rule is that a raw literal holds no CR, so stripping every CR from a raw literal's bytes is exact on
every checkout, CRLF or LF. The strip must apply to RAW literals only, never inside the shared
`@string(ReadOnlySpan<byte>)` conversion, because an interpreted `"a\r\n"u8` legitimately holds a CR.

**Where it lives:** golib, beside `@string(in ReadOnlySpan<byte>)` in `golib/string.cs`.

**Cost:** zero extra allocations. Today's implicit `u8 -> @string` conversion already copies the span into a new
array on every evaluation (`AllocationCounter.CopyOf`). The strip makes that same one array, skipping CRs, after
a vectorised `Count` of CR (no CR on an LF checkout, so the copy is unchanged). 298 literals are already
`static readonly`, so they run once per process. The rest run where the literal is evaluated, exactly as the copy
does today.

Two spellings, both prototyped, built and run on a CRLF checkout, and both matching Go byte for byte:

- **(b1) a prefix call:** `@string.Raw("""..."""u8)`. This is a static method on the golib struct. A bare
  `raw(...)` builtin was rejected because 284 corpus files declare an identifier `raw`, which would shadow it.
- **(b2) a postfix extension (recommended):** `"""..."""u8.Raw()`. The opening line and every content line
  are byte-identical to today, and the only change is `.Raw()` on the closing line, the least added text of the
  candidates. It is an extension on `ReadOnlySpan<byte>` in namespace `go`, so every converted namespace sees it.
  It has a `string` overload for the 7 concatenation sites (`"""...""".Raw() + quote + ...`).

The prototype helper (a stand-in the probe compiled; a fix would add it to golib):

```csharp
// Stand-in for the golib helper a (b) fix would add. Go's rule: a raw string literal never holds a
// carriage return (the compiler drops them), so stripping every CR from a raw literal's bytes is
// exactly Go's semantics. One allocation: the same one @string(ReadOnlySpan<byte>) already makes.
namespace go;

using System;

public static class RawString {
    public static @string Raw(this ReadOnlySpan<byte> literal) {
        int cr = literal.Count((byte)'\r');
        if (cr == 0) return new @string(literal);
        byte[] bytes = new byte[literal.Length - cr];
        int n = 0;
        foreach (byte b in literal) if (b != (byte)'\r') bytes[n++] = b;
        return new @string(bytes);
    }

    public static string Raw(this string literal) => literal.Replace("\r", "");
}
```

## 3. The (a) form: no raw literal

Shape (a) is exact without a helper, but it gives up the multi-line raw literal. Two spellings, both prototyped
and matching Go: (a1) one escaped `u8` literal on one line, and (a2) one `u8` literal per Go line, each ending
`\n`, joined by `+`. C# folds a `u8 + u8` chain at compile time. (a2) keeps the line structure, but every line
gains quotes, `\n` and `u8 +`.

## Before and after on two real sites

The real statements at 226afb810a, transformed by the same generator the probe compiled. PEM body lines are elided
for length; nothing else is.

#### net/http pattern.cs:458

BEFORE (master):
```csharp
return fmt.Sprintf("""
%[1]s and %[2]s both match some paths, like %[3]q.
But neither is more specific than the other.
%[1]s matches %[4]q, but %[2]s doesn't.
%[2]s matches %[5]q, but %[1]s doesn't.
"""u8,
            Ꮡp1.OrTypedNil(), Ꮡp2.OrTypedNil(), commonPath(ref (Ꮡp1).DerefOrNull(), ref (Ꮡp2).DerefOrNull()), differencePath(ref (Ꮡp1).DerefOrNull(), ref (Ꮡp2).DerefOrNull()), differencePath(ref (Ꮡp2).DerefOrNull(), ref (Ꮡp1).DerefOrNull()));
```

b1-prefix:
```csharp
return fmt.Sprintf(@string.Raw("""
%[1]s and %[2]s both match some paths, like %[3]q.
But neither is more specific than the other.
%[1]s matches %[4]q, but %[2]s doesn't.
%[2]s matches %[5]q, but %[1]s doesn't.
"""u8),
            Ꮡp1.OrTypedNil(), Ꮡp2.OrTypedNil(), commonPath(ref (Ꮡp1).DerefOrNull(), ref (Ꮡp2).DerefOrNull()), differencePath(ref (Ꮡp1).DerefOrNull(), ref (Ꮡp2).DerefOrNull()), differencePath(ref (Ꮡp2).DerefOrNull(), ref (Ꮡp1).DerefOrNull()));
```

b2-postfix:
```csharp
return fmt.Sprintf("""
%[1]s and %[2]s both match some paths, like %[3]q.
But neither is more specific than the other.
%[1]s matches %[4]q, but %[2]s doesn't.
%[2]s matches %[5]q, but %[1]s doesn't.
"""u8.Raw(),
            Ꮡp1.OrTypedNil(), Ꮡp2.OrTypedNil(), commonPath(ref (Ꮡp1).DerefOrNull(), ref (Ꮡp2).DerefOrNull()), differencePath(ref (Ꮡp1).DerefOrNull(), ref (Ꮡp2).DerefOrNull()), differencePath(ref (Ꮡp2).DerefOrNull(), ref (Ꮡp1).DerefOrNull()));
```

a1-escaped:
```csharp
return fmt.Sprintf("%[1]s and %[2]s both match some paths, like %[3]q.\nBut neither is more specific than the other.\n%[1]s matches %[4]q, but %[2]s doesn't.\n%[2]s matches %[5]q, but %[1]s doesn't."u8,
            Ꮡp1.OrTypedNil(), Ꮡp2.OrTypedNil(), commonPath(ref (Ꮡp1).DerefOrNull(), ref (Ꮡp2).Der [... single line continues ...]
```

a2-perline:
```csharp
return fmt.Sprintf("%[1]s and %[2]s both match some paths, like %[3]q.\n"u8 +
"But neither is more specific than the other.\n"u8 +
"%[1]s matches %[4]q, but %[2]s doesn't.\n"u8 +
"%[2]s matches %[5]q, but %[1]s doesn't."u8,
            Ꮡp1.OrTypedNil(), Ꮡp2.OrTypedNil(), commonPath(ref (Ꮡp1).DerefOrNull(), ref (Ꮡp2).DerefOrNull()), differencePath(ref (Ꮡp1).DerefOrNull(), ref (Ꮡp2).DerefOrNull()), differencePath(ref (Ꮡp2).DerefOrNull(), ref (Ꮡp1).DerefOrNull()));
```

#### net/http/internal/testcert testcert.cs:16

BEFORE (master):
```csharp
public static slice<byte> LocalhostCert = slice<byte>("""
-----BEGIN CERTIFICATE-----
MIIDSDCCAjCgAwIBAgIQEP/md970HysdBTpuzDOf0DANBgkqhkiG9w0BAQsFADAS
MRAwDgYDVQQKEwdBY21lIENvMCAXDTcwMDEwMTAwMDAwMFoYDzIwODQwMTI5MTYw
MDAwWjASMRAwDgYDVQQKEwdBY21lIENvMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A
        [... 12 PEM lines elided ...]
2GFBoapol9L5FxCc77bztzK8CpLujkBi25Vk6GAFbl27opLfpyxkM+rX/T6MXCPO
6/YBacNZ7ff1/57Etg4i5mNA6ubCpuc4Gi9oYqCNNohftr2lkJr7REdDR6OW0lsL
rF7r4gUnKeC7mYIH1zypY7laskopiLFAfe96Kg==
-----END CERTIFICATE-----
"""u8);
```

b1-prefix:
```csharp
public static slice<byte> LocalhostCert = slice<byte>(@string.Raw("""
-----BEGIN CERTIFICATE-----
MIIDSDCCAjCgAwIBAgIQEP/md970HysdBTpuzDOf0DANBgkqhkiG9w0BAQsFADAS
MRAwDgYDVQQKEwdBY21lIENvMCAXDTcwMDEwMTAwMDAwMFoYDzIwODQwMTI5MTYw
MDAwWjASMRAwDgYDVQQKEwdBY21lIENvMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A
        [... 12 PEM lines elided ...]
2GFBoapol9L5FxCc77bztzK8CpLujkBi25Vk6GAFbl27opLfpyxkM+rX/T6MXCPO
6/YBacNZ7ff1/57Etg4i5mNA6ubCpuc4Gi9oYqCNNohftr2lkJr7REdDR6OW0lsL
rF7r4gUnKeC7mYIH1zypY7laskopiLFAfe96Kg==
-----END CERTIFICATE-----
"""u8));
```

b2-postfix:
```csharp
public static slice<byte> LocalhostCert = slice<byte>("""
-----BEGIN CERTIFICATE-----
MIIDSDCCAjCgAwIBAgIQEP/md970HysdBTpuzDOf0DANBgkqhkiG9w0BAQsFADAS
MRAwDgYDVQQKEwdBY21lIENvMCAXDTcwMDEwMTAwMDAwMFoYDzIwODQwMTI5MTYw
MDAwWjASMRAwDgYDVQQKEwdBY21lIENvMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A
        [... 12 PEM lines elided ...]
2GFBoapol9L5FxCc77bztzK8CpLujkBi25Vk6GAFbl27opLfpyxkM+rX/T6MXCPO
6/YBacNZ7ff1/57Etg4i5mNA6ubCpuc4Gi9oYqCNNohftr2lkJr7REdDR6OW0lsL
rF7r4gUnKeC7mYIH1zypY7laskopiLFAfe96Kg==
-----END CERTIFICATE-----
"""u8.Raw());
```

a1-escaped:
```csharp
public static slice<byte> LocalhostCert = slice<byte>("-----BEGIN CERTIFICATE-----\nMIIDSDCCAjCgAwIBAgIQEP/md970HysdBTpuzDOf0DANBgkqhkiG9w0BAQsFADAS\nMRAwDgYDVQQKEwdBY21lIENvMCAXDTcwMDEwMTAwMDAwMFoYDzIwODQwMTI5MTYw\nMDAwWjASMRAwDgYDVQQKEwdBY21lIENvMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A\nMIIBCgKCAQEAxcl69R [... single line continues ...]
```

a2-perline:
```csharp
public static slice<byte> LocalhostCert = slice<byte>("-----BEGIN CERTIFICATE-----\n"u8 +
"MIIDSDCCAjCgAwIBAgIQEP/md970HysdBTpuzDOf0DANBgkqhkiG9w0BAQsFADAS\n"u8 +
"MRAwDgYDVQQKEwdBY21lIENvMCAXDTcwMDEwMTAwMDAwMFoYDzIwODQwMTI5MTYw\n"u8 +
"MDAwWjASMRAwDgYDVQQKEwdBY21lIENvMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A\n"u8 +
        [... 12 PEM lines elided ...]
"2GFBoapol9L5FxCc77bztzK8CpLujkBi25Vk6GAFbl27opLfpyxkM+rX/T6MXCPO\n"u8 +
"6/YBacNZ7ff1/57Etg4i5mNA6ubCpuc4Gi9oYqCNNohftr2lkJr7REdDR6OW0lsL\n"u8 +
"rF7r4gUnKeC7mYIH1zypY7laskopiLFAfe96Kg==\n"u8 +
"-----END CERTIFICATE-----"u8);
```

## 4. The gate a real fix needs

A new behavioral project, for example `RawStringLineEndings`, marked `[GoTestMatchingConsoleOutput]` and
deliberately NOT `-text`. It prints `len` and `%q` of a multi-line raw string, of a raw string with a trailing
newline, of a `[]byte(...)` raw string, and of one inside a `+` chain, plus an FNV hash of a PEM-shaped constant.
Its Go output is the golden.

- **RED first:** run it with the compile and output phases only (`run-behavioral.ps1 --filter RawStringLineEndings
  --phase compile,output`). That builds the committed CRLF `main.cs` with no transpile, which is the form a
  published package or a plain `dotnet build` sees. At master it must FAIL on the length and `%q` lines. The probe
  above shows exactly that.
- **GREEN after the fix:** the same two phases pass, and so does the full four-phase run. The golden `.cs.target`
  carries `.Raw()`.
- The `-text` marks on Solitaire, SortArrayType and StdLibInternalAbi then become removable, as a follow-up with a
  rebank. They are not part of the fix.

Evidence (lane-local, not pushed): the probes, the four variant builds and their outputs.
