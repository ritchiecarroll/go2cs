// GoStackRootAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go.golib;

/// <summary>
/// Marks a host method that stands where a Go runtime or library function sits on a real Go stack,
/// so runtime.Callers reports that Go frame in the host method's place.
/// </summary>
/// <remarks>
/// <para>
/// The managed walk counts only converted Go frames, so a host that runs Go code from its own C#
/// leaves no frame where Go has one. The test host is the case that matters: Go runs every test body
/// from <c>testing.tRunner</c>, and Go's own tests read the bottom of the stack (runtime's
/// <c>testCallersEqual</c> drops the last frame it is given). The marked method must not be inlined,
/// or the CLR's stack trace loses it.
/// </para>
/// <para>
/// A walk that reaches a marked frame, or that runs on a goroutine a <c>go</c> statement started,
/// also ends at <c>runtime.goexit</c>, the frame Go's unwinder reports at the bottom of every
/// goroutine (runtime/managed_impl.cs, captureCallers).
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false)]
public sealed class GoStackRootAttribute(string function, string file, int line) : Attribute
{
    /// <summary>The Go function the frame reports, in Go's spelling (<c>testing.tRunner</c>).</summary>
    public string Function { get; } = function;

    /// <summary>The Go source file the frame reports, GOROOT-relative for a standard library frame.</summary>
    public string File { get; } = file;

    /// <summary>The Go line the frame reports: the line of the call the Go function makes.</summary>
    public int Line { get; } = line;
}
