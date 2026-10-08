// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build go1.10
namespace go.vendor.golang.org.x.text.secure;

partial class bidirule_package {

internal static bool isFinal(this ref Transformer t) {
    return t.state == ruleLTRFinal || t.state == ruleRTLFinal || t.state == ruleInitial;
}

} // end bidirule_package
