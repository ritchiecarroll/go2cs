// GoErrorException.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;

namespace go;

/// <summary>
/// The exception <see cref="MustExtensions"/> throws for a non-nil Go <c>error</c>: a Go panic whose
/// value is that error.
/// </summary>
/// <remarks>
/// <para>
/// Go's own <c>Must</c> helpers (<c>template.Must</c>, <c>regexp.MustCompile</c>) panic with the error
/// they were handed, so this IS a <see cref="PanicException"/> whose <see cref="PanicException.State"/>
/// is that error. Two things follow, both as in Go. A deferred <c>recover()</c> in converted Go code
/// between the throw and the catch (a C# callback that Go code calls) recovers the error value itself.
/// And a C# <c>catch (PanicException)</c> catches a failed <c>Must</c> as the panic it is.
/// </para>
/// <para>
/// <see cref="Err"/> is the original error, so <c>errors.Is</c> and <c>errors.As</c> work on it:
/// <c>catch (GoErrorException ex) when (errors.Is(ex.Err, strconv.ErrSyntax))</c>.
/// <see cref="Exception.Message"/> is the error's <c>Error()</c> text, the text Go prints for the panic.
/// </para>
/// </remarks>
[DebuggerNonUserCode]
public sealed class GoErrorException(error err) : PanicException(err ?? throw new ArgumentNullException(nameof(err)))
{
    /// <summary>
    /// Gets the Go error the failed call returned.
    /// </summary>
    public error Err { get; } = err;
}
