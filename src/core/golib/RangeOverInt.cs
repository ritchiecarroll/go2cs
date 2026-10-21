// RangeOverInt.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace go;

/// <summary>
/// The integers 0..n-1 of Go's range-over-int, <c>for i := range n</c>, as <see cref="builtin.range{T}(T)"/>
/// returns them.
/// </summary>
/// <remarks>
/// A struct with a struct <see cref="Enumerator"/>: C#'s <c>foreach</c> binds <c>GetEnumerator</c> by pattern before
/// it considers any interface, so the converted loop allocates nothing, as Go's does. While <c>range</c> was an
/// iterator method every loop allocated its iterator (56 B on x64), which golib's AllocationCounter never charged
/// and <c>testing.AllocsPerRun</c> then reported in bytes (GolibTests RangeOverIntAllocationTests). It is still an
/// <see cref="IEnumerable{T}"/> for any other consumer, which boxes it.
/// </remarks>
/// <typeparam name="T">Integer type of the range operand and of the loop variable.</typeparam>
public readonly struct RangeOverInt<T> : IEnumerable<T> where T : struct, IComparisonOperators<T, T, bool>, IIncrementOperators<T>
{
    private readonly T m_n;

    internal RangeOverInt(T n) => m_n = n;

    /// <summary>
    /// Returns the allocation-free enumerator <c>foreach</c> binds.
    /// </summary>
    public Enumerator GetEnumerator() => new(m_n);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Enumerates 0..n-1, and nothing when n &lt;= 0.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private readonly T m_n;
        private T m_current;
        private bool m_started;

        internal Enumerator(T n)
        {
            m_n = n;
            m_current = default;
            m_started = false;
        }

        /// <inheritdoc />
        public readonly T Current => m_current;

        readonly object IEnumerator.Current => m_current;

        /// <inheritdoc />
        public bool MoveNext()
        {
            // default(T) is the zero of every Go integer type, so no INumberBase is needed (see range<T>).
            if (m_started)
                m_current++;
            else
                m_started = true;

            return m_current < m_n;
        }

        /// <inheritdoc />
        public void Reset()
        {
            m_current = default;
            m_started = false;
        }

        /// <inheritdoc />
        public readonly void Dispose()
        {
        }
    }
}
