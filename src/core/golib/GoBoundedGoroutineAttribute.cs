// GoBoundedGoroutineAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Records a <c>go</c> statement whose goroutine the converter proved cannot recurse, by the position of its
/// <c>goǃ</c> call, so go2cs-gen starts it on <see cref="GoStack.Bounded"/>.
/// </summary>
/// <param name="file">The converted source file's name, as the compiler sees it.</param>
/// <param name="line">The 1-based line the <c>goǃ</c> call starts on.</param>
/// <param name="column">The 1-based UTF-16 column the <c>goǃ</c> call starts at.</param>
/// <remarks>
/// <para>
/// The converter writes one assembly-level record per proved <c>go</c> statement in the package's
/// <c>package_info.cs</c>, and the statement itself reads as any other, <c>goǃ(f, x)</c>, as the Go <c>go f(x)</c>
/// does. go2cs-gen intercepts the <c>goǃ</c> call at each recorded position and forwards it to the
/// <see cref="GoStack"/>-marked rung (<c>BoundedGoroutineInterceptorGenerator</c>).
/// </para>
/// <para>
/// A record that does not land on a call of an unmarked <c>goǃ</c> rung is a build error (GO2CS0010), never a
/// goroutine on the wrong reservation. A <c>go</c> statement without a record, a launch golib's callers write by
/// hand, and a compilation go2cs-gen does not run on all keep the Go-scale default, <see cref="GoStack.Default"/>.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class GoBoundedGoroutineAttribute(string file, int line, int column) : Attribute
{
    /// <summary>
    /// Gets the converted source file's name.
    /// </summary>
    public string File => file;

    /// <summary>
    /// Gets the 1-based line the <c>goǃ</c> call starts on.
    /// </summary>
    public int Line => line;

    /// <summary>
    /// Gets the 1-based UTF-16 column the <c>goǃ</c> call starts at.
    /// </summary>
    public int Column => column;
}
