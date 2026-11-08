// GoNativeCall.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go.golib;

/// <summary>How a native call is expected to behave, for the scheduler's handoff (Go's entersyscall forms).</summary>
public enum NativeCallHint
{
    /// <summary>Go's <c>entersyscall</c>: the call may block; a long one has its P retaken.</summary>
    MayBlock,

    /// <summary>Go's <c>entersyscallblock</c>: the call is known to block; its P is handed off at once.</summary>
    Blocks
}

/// <summary>The one native-call transition seam (docs/phase4/DESIGN-native-call-seam.md). RED STUB.</summary>
public static class GoNativeCall
{
    public static void Enter(NativeCallHint hint = NativeCallHint.MayBlock) => throw new NotImplementedException();

    public static void Exit() => throw new NotImplementedException();

    public static void ReenterManaged() => throw new NotImplementedException();

    public static void ReturnToNative() => throw new NotImplementedException();

    public static int TransitionDepth => throw new NotImplementedException();
}
