// GoFuncCookie.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace go;

// ---------------------------------------------------------------------------------------------
// FUNC COOKIES — the machine word a Go func value answers when it is read AS a word.
//
// WHY THERE IS A FILE HERE AT ALL
//   Go's func value is one pointer word (to a funcval), so `uintptr(*(*unsafe.Pointer)(unsafe.Pointer(&f)))`
//   is a number an OS can carry and hand back: runtime's `nestedCall` smuggles a closure through
//   EnumTimeFormatsEx's lParam that way, and its `callback` reads it back with
//   `*(*func())(unsafe.Pointer(&lparam))` and calls it. A managed func value is a DELEGATE, a
//   reference with no address to give, so the word used to be the order token of the box HOLDING the
//   func -- which the syscall door rightly refuses as a managed-pointer token at argument 3 (runtime's
//   TestCallback family; ruling 2026-09-28 15:03 (2), approved as designed).
//
// WHAT A COOKIE PROMISES
//   Unique per delegate INSTANCE, stable while that delegate is alive, and resolvable back to it.
//   It is never an address and addresses nothing: native code only CARRIES it back, which is the
//   whole contract of an lParam. Its stated falsifier stands with the ruling -- a native callee that
//   STORES and DEREFERENCES the cookie would read nothing -- and none is known.
//
// THE BAND, and why it is disjoint from every other minted number
//   Base 0xC000_8000_0000_0000, 16 bytes per cookie: bits 63, 62 and 47 set, bits 61..48 clear.
//     - order tokens (ManagedPointerTokens.IsTaggedToken) are bit 63 set, bit 47 CLEAR: disjoint on
//       bit 47, which is exactly why the syscall door PASSES a cookie;
//     - caller spans (runtime, from 0x8000_8000_0000_0000) have bit 62 clear;
//     - synthetic PCs (GoSyntheticPC, from 0xFFFF_8000_0000_0000) have bits 61..48 set;
//     - real addresses are canonical (bits 63..47 all equal); a cookie is not.
//   Bit 63 is set, so ManagedPointerTokens.NamesNoUserMemory refuses a raw dereference of one: a
//   misuse fails as a caught nil-dereference panic, never as a read of a stranger's memory.
//
// LIFETIME, which is Go's
//   A uintptr does not keep its referent alive in Go, so the registry holds delegates WEAKLY: a
//   cookie resolves exactly while something else holds the func. nestedCall's own frame holds `f`
//   across the call, which is the liveness the ruling records.
// ---------------------------------------------------------------------------------------------

/// <summary>
/// Mints and resolves the word a Go func value answers when it is read as a machine word.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] public static class GoFuncCookie
{
    private const ulong Base = 0xC000_8000_0000_0000UL;
    private const ulong BandMask = 0xFFFF_8000_0000_0000UL;
    private const int StrideShift = 4;

    private static readonly object s_gate = new();
    private static readonly ConditionalWeakTable<Delegate, CookieWord> s_cookieOf = new();
    private static readonly Dictionary<nuint, WeakReference<Delegate>> s_delegateOf = new();
    private static ulong s_next;

    private sealed class CookieWord(nuint word)
    {
        public readonly nuint Word = word;
    }

    /// <summary>
    /// Whether <paramref name="word"/> lies in the cookie band -- decided from its bits alone.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCookie(nuint word) => ((ulong)word & BandMask) == Base;

    /// <summary>
    /// The cookie for <paramref name="fn"/>: <c>0</c> for a nil func (Go's nil func word), otherwise a
    /// word in the cookie band that is the same for every read of the same delegate instance.
    /// </summary>
    public static nuint Of(Delegate? fn)
    {
        if (fn is null)
            return 0;

        lock (s_gate)
        {
            if (s_cookieOf.TryGetValue(fn, out CookieWord? existing))
                return existing.Word;

            ulong index = ++s_next;

            // 2^43 cookies before the offset would carry into bit 47 and leave the band -- refused by
            // name rather than wrapped, as GoSyntheticPC refuses its own exhaustion.
            if (index >= 1UL << (47 - StrideShift))
                throw new InvalidOperationException("go2cs: func cookie band exhausted");

            nuint word = (nuint)(Base + (index << StrideShift));
            s_cookieOf.Add(fn, new CookieWord(word));
            s_delegateOf[word] = new WeakReference<Delegate>(fn);
            return word;
        }
    }

    /// <summary>
    /// The delegate <paramref name="word"/> was minted for, or <c>null</c> when the word is not a
    /// cookie or its delegate is no longer alive.
    /// </summary>
    public static Delegate? Resolve(nuint word)
    {
        if (!IsCookie(word))
            return null;

        lock (s_gate)
        {
            if (!s_delegateOf.TryGetValue(word, out WeakReference<Delegate>? entry))
                return null;

            if (entry.TryGetTarget(out Delegate? fn))
                return fn;

            s_delegateOf.Remove(word);
            return null;
        }
    }
}
