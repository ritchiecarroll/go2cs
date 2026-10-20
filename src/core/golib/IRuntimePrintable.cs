// IRuntimePrintable.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

namespace go;

/// <summary>
/// A print/println argument that prints itself through the runtime's printer, as the Go compiler
/// lowers a print of a type it knows (<c>runtime.hex</c> prints through printhex, <c>runtime.quoted</c>
/// through printquoted) instead of the decimal or raw text the argument's ToString would give.
/// </summary>
/// <remarks>
/// golib sits below the runtime package and cannot name those types, so they implement this interface
/// (runtime/print_impl.cs) and <c>builtin.print</c> calls <see cref="RuntimePrint"/> in place of
/// formatting the argument. The call happens with the runtime's printlock already held, which is
/// recursive, so the implementation writes through the runtime's own print primitives.
/// </remarks>
public interface IRuntimePrintable
{
    /// <summary>Writes this value as the runtime's printer would, under the caller's printlock.</summary>
    void RuntimePrint();
}
