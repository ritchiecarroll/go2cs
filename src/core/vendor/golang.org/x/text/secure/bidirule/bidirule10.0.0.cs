// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build go1.10
namespace go.vendor.golang.org.x.text.secure;

using static bidirule_package;

partial class bidirule_package {

} partial class bidiruleᴛTransformer {

internal static bool isFinal(this ref Transformer t) {
    return t.state == ruleLTRFinal || t.state == ruleRTLFinal || t.state == ruleInitial;
}

} // end bidiruleᴛTransformer
