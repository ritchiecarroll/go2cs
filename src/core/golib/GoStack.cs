// GoStack.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go;

/// <summary>
/// The stack a goroutine's thread reserves.
/// </summary>
/// <remarks>
/// A Go goroutine's stack grows, while a .NET thread's is a fixed reservation whose overflow ends the process, so a
/// goroutine reserves a Go-scale stack by default (<see cref="go.golib.Goroutine.StackReserve"/>). That reservation is
/// what makes many goroutines parked at once slow to start. The converter marks the <c>go</c> statements whose
/// goroutine it proved cannot recurse -- every call it can make resolves statically, and none returns to a function
/// already on its stack -- with <see cref="Bounded"/>, and those start on a much smaller reservation
/// (<see cref="go.golib.Goroutine.BoundedStackReserve"/>). An unmarked <c>go</c> statement, and any code that starts a
/// goroutine without a mark, keeps the default.
/// </remarks>
public enum GoStack
{
    /// <summary>The Go-scale reservation: the goroutine may recurse as deeply as Go allows.</summary>
    Default,

    /// <summary>The converter proved the goroutine cannot recurse, so its depth is bounded by its call graph.</summary>
    Bounded
}
