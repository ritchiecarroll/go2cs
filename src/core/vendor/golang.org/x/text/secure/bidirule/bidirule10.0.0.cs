// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build go1.10
namespace go.vendor.golang.org.x.text.secure;

using static bidirule_package;

partial class bidirule_package {

} partial class bidirule_package {

internal static bool isFinalᴍ(ref Transformer t) {
    return t.state == ruleLTRFinal || t.state == ruleRTLFinal || t.state == ruleInitial;
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class bidiruleᴛTransformer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isFinal(this ref Transformer t) => bidirule_package.isFinalᴍ(ref t);
}
// </GlyphForwarders>
