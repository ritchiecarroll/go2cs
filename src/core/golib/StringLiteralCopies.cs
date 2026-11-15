// StringLiteralCopies.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace go.golib;

/// <summary>
/// The immortal native copies behind a number-shaped string header (<c>reflect.StringHeader</c>) over PACKAGE-LEVEL
/// storage: route (L) of docs/phase4/DESIGN-sqlite-ccgo-ts.md, section 10.
/// </summary>
/// <remarks>
/// <para>
/// Go keeps a string literal's bytes in read-only data: immutable, never freed, and not a Go pointer to
/// <c>cgoIsGoPointer</c>. ccgo leans on that: it takes the data address of a package's literal table once, as a number,
/// and reads <c>__ccgo_ts + N</c> for the program's whole life. A managed backing can move, so the number names a native
/// copy of it instead: made once per backing, never freed, never pinned, never in any registry.
/// </para>
/// <para>
/// KEYS ARE STRONG. Every key is the backing of a package-level variable's string, and its copy is never freed, so a
/// weak key would buy nothing. The table grows by one entry per distinct backing ever read this way: for ccgo, one per
/// literal table.
/// </para>
/// <para>
/// VALIDATE ON READ (R-a). A package-level variable can hold a string that SHARES a live <c>[]byte</c> (built by
/// <c>unsafe.String</c>, or assigned at run time). So every read compares the WHOLE backing with the whole copy, and a
/// mismatch is refused by name: a stale copy is never handed out again. It costs O(length) per read, once per ccgo
/// declaration.
/// </para>
/// <para>
/// THE FIRST-READ RACE. The table is read lock-free first. On a miss a block is built and OFFERED. A thread whose offer
/// loses frees its own block at once, then reads the winner's. No number is formed from a block before the table has
/// accepted it, so no caller ever sees a loser's block.
/// </para>
/// </remarks>
internal static unsafe class StringLiteralCopies
{
    private sealed class Block
    {
        public readonly nuint Base;
        public readonly int Length;

        public Block(nuint @base, int length)
        {
            Base = @base;
            Length = length;
        }
    }

    private static readonly ConcurrentDictionary<byte[], Block> s_table = new(ReferenceEqualityComparer.Instance);
    private static int s_count;

    // Test-readable counters (W9, W10): blocks accepted into the table, their bytes, blocks freed by a losing offer, and
    // whole-backing compares.
    internal static long Copies;
    internal static long CopiedBytes;
    internal static long LosersFreed;
    internal static long Compares;

    /// <summary>Whether any backing has a copy. QW-9's lookup is made only when this is false.</summary>
    internal static bool IsEmpty => Volatile.Read(ref s_count) == 0;

    /// <summary>
    /// The base of <paramref name="backing"/>'s immortal copy, made on its first read. Refused by name when the backing's
    /// bytes no longer match the copy.
    /// </summary>
    internal static nuint BaseOf(byte[] backing)
    {
        if (s_table.TryGetValue(backing, out Block? found))
            return Validated(backing, found);

        Block mine = Copy(backing);

        if (s_table.TryAdd(backing, mine))
        {
            Interlocked.Increment(ref s_count);
            Interlocked.Increment(ref Copies);
            Interlocked.Add(ref CopiedBytes, mine.Length);
            return mine.Base;
        }

        // Lost the first-read race: the block was never offered to a caller, so it is freed now, and the winner's is read.
        NativeMemory.Free((void*)mine.Base);
        Interlocked.Increment(ref LosersFreed);

        return Validated(backing, s_table[backing]);
    }

    /// <summary>
    /// QW-9: the base of <paramref name="backing"/>'s copy when one already exists (validated), so a header over any
    /// string sharing a package-level literal's backing reads the literal's one number.
    /// </summary>
    internal static bool TryExisting(byte[] backing, out nuint @base)
    {
        @base = 0;

        if (IsEmpty || !s_table.TryGetValue(backing, out Block? found))
            return false;

        @base = Validated(backing, found);
        return true;
    }

    private static Block Copy(byte[] backing)
    {
        byte* block = (byte*)NativeMemory.Alloc((nuint)backing.Length);
        backing.AsSpan().CopyTo(new Span<byte>(block, backing.Length));
        return new Block((nuint)block, backing.Length);
    }

    private static nuint Validated(byte[] backing, Block block)
    {
        Interlocked.Increment(ref Compares);

        if (!new ReadOnlySpan<byte>(backing).SequenceEqual(new ReadOnlySpan<byte>((void*)block.Base, block.Length)))
            throw new PanicException("string header refused: the bytes behind a package-level string changed after go2cs copied them for its uintptr Data (the string shares a []byte that was written; docs/phase4/DESIGN-sqlite-ccgo-ts.md, R-a)");

        return block.Base;
    }
}
