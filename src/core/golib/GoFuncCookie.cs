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
//
// ONE WORD PER STATIC FUNCTION (sqlite-cs S1, docs/phase4/DESIGN-sqlite-cs-funcptr.md section 3.1)
//   A delegate with no target is a converted top-level Go func. Go gives such a func ONE static
//   funcval, never freed, so `__ccgo_fp(f)` answers the same number at every site, and C code compares
//   those numbers. So a static method's word is keyed on its METHOD, not on the delegate instance, and
//   its row is held strongly: every delegate over that method, under any delegate type, answers the
//   same word. A closure or a bound method value keeps the per-instance, weakly held word above, which
//   is Go's heap funcval. Both kinds come from the one band and the one counter, so the band proof
//   above covers both.
//
// RECOVERY, one resolve for every route
//   Func<F> (the converter's emission) and the Reinterpret route (runtime's callback) share Resolve's
//   rebind: the minted delegate when its type is the one asked for, otherwise a delegate of the asked
//   type over the same target and method -- two converted delegate types can spell one Go signature
//   (a named func type against Func<...>). A static function's rebinds are cached per delegate type on
//   its row; a closure's are not, because the cache would keep the closure alive.
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
    private static readonly Dictionary<RuntimeMethodHandle, StaticFuncRow> s_rowOfMethod = new();
    private static readonly Dictionary<nuint, StaticFuncRow> s_rowOfWord = new();
    private static ulong s_next;

    private sealed class CookieWord(nuint word)
    {
        public readonly nuint Word = word;
    }

    // A static function's row, for the process: its word, the delegate it was first minted from, and
    // the delegates of other types recovered over the same method.
    private sealed class StaticFuncRow(nuint word, Delegate plain)
    {
        public readonly nuint Word = word;
        public readonly Delegate Plain = plain;
        public Dictionary<Type, Delegate>? Rebinds;
    }

    /// <summary>
    /// Whether <paramref name="word"/> lies in the cookie band -- decided from its bits alone.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCookie(nuint word) => ((ulong)word & BandMask) == Base;

    /// <summary>
    /// The cookie for <paramref name="fn"/>: <c>0</c> for a nil func (Go's nil func word), otherwise a
    /// word in the cookie band. A static function answers one word for every delegate over it; any
    /// other delegate answers the same word for every read of the same instance.
    /// </summary>
    public static nuint Of(Delegate? fn)
    {
        if (fn is null)
            return 0;

        // A delegate instance read before answers without the lock and without reading its method.
        if (s_cookieOf.TryGetValue(fn, out CookieWord? known))
            return known.Word;

        RequireWordSize(IntPtr.Size);

        // A delegate built over another delegate (`new F(g)`, which is how a func value converts to
        // another delegate type) invokes g and stands for it: Go's conversion keeps the funcval.
        if (fn.Target is Delegate wrapped && fn.Method.Name == "Invoke" && fn.Method.DeclaringType == wrapped.GetType())
        {
            nuint innerWord = Of(wrapped);

            lock (s_gate)
                s_cookieOf.AddOrUpdate(fn, new CookieWord(innerWord));

            return innerWord;
        }

        lock (s_gate)
        {
            if (s_cookieOf.TryGetValue(fn, out CookieWord? existing))
                return existing.Word;

            if (fn.Target is null)
            {
                RuntimeMethodHandle method = fn.Method.MethodHandle;

                if (!s_rowOfMethod.TryGetValue(method, out StaticFuncRow? row))
                {
                    row = new StaticFuncRow(NextWord(), fn);
                    s_rowOfMethod.Add(method, row);
                    s_rowOfWord.Add(row.Word, row);
                }

                s_cookieOf.Add(fn, new CookieWord(row.Word));
                return row.Word;
            }

            nuint word = NextWord();
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
            return ResolveLocked(word);
    }

    /// <summary>
    /// The func <paramref name="word"/> names, as <typeparamref name="F"/>: Go's
    /// <c>*(*F)(unsafe.Pointer(&amp;w))</c> for a word <c>w</c>. Word 0 is Go's nil func. A word that
    /// is not a func word, names no live func, or names one that cannot be bound as
    /// <typeparamref name="F"/> panics by name.
    /// </summary>
    public static F Func<F>(nuint word) where F : Delegate => (F)ResolveAs(word, typeof(F))!;

    /// <summary>
    /// The one recovery behind <see cref="Func{F}"/> and golib's Reinterpret route: the minted delegate
    /// when it is already a <paramref name="type"/>, otherwise one rebound over the same target and
    /// method, cached on a static function's row.
    /// </summary>
    internal static Delegate? ResolveAs(nuint word, Type type)
    {
        if (word == 0)
            return null;

        if (!IsCookie(word))
            throw new PanicException("go2cs: 0x" + word.ToString("x") + " is not a func word -- no func value was read as this number");

        Delegate? fn;
        StaticFuncRow? row;

        lock (s_gate)
        {
            if (s_rowOfWord.TryGetValue(word, out row))
            {
                if (row.Rebinds is not null && row.Rebinds.TryGetValue(type, out Delegate? cached))
                    return cached;

                fn = row.Plain;
            }
            else
            {
                fn = ResolveLocked(word);
            }
        }

        if (fn is null)
            throw new PanicException("go2cs: func cookie 0x" + word.ToString("x") + " names no live func -- the func value it was read from is no longer referenced");

        if (type.IsInstanceOfType(fn))
            return fn;

        Delegate rebound = Delegate.CreateDelegate(type, fn.Target, fn.Method, throwOnBindFailure: false) ??
            throw new PanicException("go2cs: func cookie 0x" + word.ToString("x") + " holds a " + fn.GetType().Name + ", which cannot be read as " + type.Name);

        if (row is not null)
        {
            lock (s_gate)
            {
                row.Rebinds ??= new Dictionary<Type, Delegate>();

                if (row.Rebinds.TryGetValue(type, out Delegate? first))
                    return first;

                row.Rebinds.Add(type, rebound);
            }
        }

        return rebound;
    }

    /// <summary>
    /// Refuses a runtime whose word is not 64 bits: there the band truncates to a small number.
    /// </summary>
    internal static void RequireWordSize(int size)
    {
        if (size != 8)
        {
            throw new PlatformNotSupportedException(
                "go2cs: func words require a 64-bit runtime — on 32 bits the cookie band truncates to a " +
                "small number that could name memory");
        }
    }

    private static Delegate? ResolveLocked(nuint word)
    {
        if (s_rowOfWord.TryGetValue(word, out StaticFuncRow? row))
            return row.Plain;

        if (!s_delegateOf.TryGetValue(word, out WeakReference<Delegate>? entry))
            return null;

        if (entry.TryGetTarget(out Delegate? fn))
            return fn;

        s_delegateOf.Remove(word);
        return null;
    }

    private static nuint NextWord()
    {
        ulong index = ++s_next;

        // 2^43 cookies before the offset would carry into bit 47 and leave the band -- refused by
        // name rather than wrapped, as GoSyntheticPC refuses its own exhaustion.
        if (index >= 1UL << (47 - StrideShift))
            throw new InvalidOperationException("go2cs: func cookie band exhausted");

        return (nuint)(Base + (index << StrideShift));
    }
}
