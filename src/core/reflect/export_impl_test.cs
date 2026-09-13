// export_impl_test.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The hand-owned companion of reflect/export_test.go's conversion — the same shape as
// internal/reflectlite's export_impl_test.cs, which established the pattern (the `_test.cs`
// suffix keeps it under the production csproj's test-artifact exclusion; testConversion globs
// `*_impl_test.cs` into the tests project's compile items and the conversion digest).

namespace go;

using static global::go.reflect_package;

partial class reflect_internal_test_package
{
    // IsExported reports whether the type's NAME is exported. Go's export_test.go answers it by
    // resolving the descriptor's name offset into the linker's name blob and reading the blob's
    // flag byte (`typ.nameOff(typ.t.Str)` → `n.IsExported()`); a synthesized descriptor has no
    // name blob and Str 0, so the resolved Name's Bytes pointer is nil and the flag read was a
    // nil dereference (TestExported died on row 0).
    //
    // The PROPERTY is answerable without the blob, from the same authority the bridge's own
    // rtype.Name stands on: a NAMED type answers its short name's first rune (Go's rule is
    // unicode upper — `ΦExported` is exported, `φUnexported` is not, which is why this is a rune
    // test and not an ASCII one); an UNNAMED POINTER answers its pointee (the blob spells such a
    // name "*T" with the extra-star flag, so its exported bit IS the pointee's — TestExported's
    // `{(*D1)(nil), true}` / `{(*big)(nil), false}` rows); anything else unnamed answers false
    // (the blob would spell "[]int" or "struct {...}", whose first rune is never upper).
    // KNOWN RESIDUAL, measured by direct probe (2026-09-01): TestExported's rows 9/11 stay red
    // because a FUNCTION-LOCAL named type lifts to `<Func>_<name>` and GoTypeName reports that
    // LIFTED identifier — `reflect_test.TestExported_p` where Go's Name() is `p` — so the first
    // rune is the function prefix's 'T' and every lifted local type reads exported. The rows that
    // pass here pass by ACCIDENT of case; TestSliceOf fails on the same root by asserting the name
    // itself. The fix is converter-side ([GoLocalName] stamped at visitTypeSpec's lift branches,
    // exactly as the dyn-lift sites already do) plus GoTypeName consulting goLocalNameOf — not a
    // guess this companion should paper over.
    public static bool IsExported(global::go.reflect_package.ΔType t)
    {
        var (rt, ok) = t._<ж<global::go.reflect_package.rtype>>(ᐧ);

        if (!ok || rt == nil)
            return false;

        return isExportedManagedType(rt.Value.t.sysType);
    }

    private static bool isExportedManagedType(System.Type? st)
    {
        if (st is null)
            return false;

        if (GoReflect.HasGoName(st))
        {
            // The short name, exactly as rtype.Name derives it from the same pair of calls.
            string full = GoReflect.GoTypeName(st);
            int dot = full.LastIndexOf('.');
            string name = dot >= 0 ? full[(dot + 1)..] : full;

            // Go's exported rule — first RUNE upper (reflect_package.isExportedGoName's twin;
            // that one is private to the production assembly, and three lines do not justify
            // widening its accessibility for a test companion).
            return System.Text.Rune.DecodeFromUtf16(name, out System.Text.Rune first, out _) == System.Buffers.OperationStatus.Done &&
                   System.Text.Rune.IsUpper(first);
        }

        if (GoReflect.KindOf(st) == GoReflect.Pointer)
            return isExportedManagedType(GoReflect.ElementType(st));

        return false;
    }

    // Go satisfies export_test.go's bodyless `func gcbits(any) []byte // provided by runtime`
    // with `//go:linkname reflect_gcbits reflect.gcbits` from runtime/mbitmap.go — so in Go this
    // IS runtime.getgcmask, under a second name. The corpus cannot cross that seam and the reason
    // is structural rather than incidental: the push's DESTINATION is declared in a _test.go, and
    // the -tests pipeline emits it into `reflect_internal_test_package`, a different class from
    // `reflect_package` where a production-side push would land; runtime exposes reflect_gcbits as
    // `internal` with no InternalsVisibleTo for reflect. Without a body here go2cs-gen's
    // PartialStubGenerator mints a throwing stub and TestGCBits reports infrastructure-error
    // whatever runtime answers — measured 2026-09-02, and it was the ONLY such stub in any
    // *_internal_test_package across the generated output on that box.
    //
    // So this restates runtime.getgcmask's two golib calls rather than inventing a channel: ONE
    // authority (GoReflect's layout walk, which is where PtrBytes comes from), two Go-named entry
    // points, exactly as Go has two names for one function. The bad-argument PANIC is deliberately
    // not restated — that text is runtime's, verifyGCBits only ever calls this as
    // GCBits(New(typ).Interface()), and a nil answer is what Go reports for a noscan span.
    internal static slice<byte> gcbits(any ep)
    {
        System.Type? elem = GoReflect.PointeeTypeOfValue(ep);

        if (elem is null)
            return default!;

        byte[]? mask = GoReflect.GoGCMaskOf(elem, GoReflect.PointeeArrayDims(ep));

        return mask is null ? default! : new slice<byte>(mask);
    }
}
