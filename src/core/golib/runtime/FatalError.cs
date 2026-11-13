// FatalError.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go.golib;

/// <summary>
/// Represents common fatal error messages reported in Go environment.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] public static class FatalError
{
    public static string DeadLock()
    {
        return "all goroutines are asleep - deadlock!";
    }
}
