// GoDeferredRegistrations.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace go;

/// <summary>
/// The registrations one golib registry is handed by the module initializers go2cs-gen emits, held until that
/// registry first MISSES: a lookup that finds nothing runs every registration deferred so far, then looks again.
/// </summary>
/// <remarks>
/// <para>
/// WHY (startup, COORD's ruling of 2026-10-21). Each registry below is read only through a lookup, and a lookup that
/// misses is the only place a registration can be observed. Run eagerly, the registrations JIT-compiled before
/// <c>main</c> a module initializer per needy struct and per pointer adapter, every opener's delegate, and the
/// operations every escaping container names. Deferred, a package's module initializer compiles one call that queues
/// its registration method, and a program that never misses compiles none of them. Measured on PerfStartup
/// (2026-10-22, win-x64 JIT, median of 9 interleaved runs): 4,402 -> 3,188 methods compiled at startup, 706 -> 627 ms
/// wall, against 597 ms on master before the Go 1.26 hop.
/// </para>
/// <para>
/// Native AOT is unchanged: the queued method is a delegate the module initializer creates, so it and every
/// instantiation it names are compiled exactly as before. A queue run at a miss holds every registration of every
/// assembly loaded so far; an assembly loaded later queues its own, and the next miss runs them.
/// </para>
/// <para>
/// The queues live on this class rather than on their registries so that queueing does not run a registry's static
/// constructor (<see cref="GoTypeOps"/> registers the builtins in its own).
/// </para>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class GoDeferredRegistrations
{
    /// <summary>The doors of <see cref="go.NativeArrayOpeners"/>, run on a native array no door is open for.</summary>
    public static readonly GoDeferredRegistrations NativeArrayOpeners = new();

    /// <summary>The factories of <see cref="GoZeroFactory{T}"/>, run on a constructed zero no factory is set for.</summary>
    public static readonly GoDeferredRegistrations ZeroFactories = new();

    /// <summary>The adapters of <see cref="AdapterRegistry"/>, run on a (box, interface) pair no adapter is found for.</summary>
    public static readonly GoDeferredRegistrations Adapters = new();

    /// <summary>The escaping containers' operations of <see cref="go.GoTypeOps"/>, run on a type no operations are found for.</summary>
    public static readonly GoDeferredRegistrations TypeOps = new();

    private readonly object m_lock = new();
    private readonly List<Action> m_pending = [];
    private int m_next;
    private volatile bool m_hasPending;

    private GoDeferredRegistrations()
    {
    }

    /// <summary>Queues <paramref name="register"/> to run on this registry's next miss.</summary>
    public void Defer(Action register)
    {
        lock (m_lock)
        {
            m_pending.Add(register);
            m_hasPending = true;
        }
    }

    /// <summary>
    /// Runs every registration queued so far, each once. Answers <c>true</c> when there were any, so the caller looks
    /// again; <c>false</c> means the miss stands.
    /// </summary>
    /// <remarks>
    /// A caller that finds another thread running the queue waits for it, so a <c>true</c> answer always follows every
    /// registration queued before the call. Re-entrant: a registration that itself queues or misses continues this
    /// same run.
    /// </remarks>
    internal bool Run()
    {
        if (!m_hasPending)
            return false;

        lock (m_lock)
        {
            while (m_next < m_pending.Count)
                m_pending[m_next++]();

            m_pending.Clear();
            m_next = 0;
            m_hasPending = false;
        }

        return true;
    }
}
