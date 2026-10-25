// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !nethttpomithttp2
namespace go.net;

using errors = errors_package;
using fmt = fmt_package;
using testing = testing_package;
using static global::go.net.http_package;

partial class http_internal_test_package {

public partial struct externalStreamErrorCode /*num:uint32*/;

internal partial struct externalStreamError {
    public uint32 StreamID;
    public externalStreamErrorCode Code;
    public error Cause;
}

internal static @string Error(this externalStreamError e) {
    return fmt.Sprintf("ID %v, code %v"u8, e.StreamID, e.Code);
}

public static void TestStreamError(ж<testing.T> Ꮡt) {
    var streamErr = http2streamError(42, http2ErrCodeProtocol);
    var (extStreamErr, ok) = errors.AsType<externalStreamError>(streamErr);
    if (!ok) {
        Ꮡt.Fatalf("errors.AsType failed"u8);
    }
    if (extStreamErr.StreamID != streamErr.StreamID) {
        Ꮡt.Errorf("got StreamID %v, expected %v"u8, extStreamErr.StreamID, streamErr.StreamID);
    }
    if (!AreEqual(extStreamErr.Cause, streamErr.Cause)) {
        Ꮡt.Errorf("got Cause %v, expected %v"u8, extStreamErr.Cause, streamErr.Cause);
    }
    if ((uint32)extStreamErr.Code != (uint32)streamErr.Code) {
        Ꮡt.Errorf("got Code %v, expected %v"u8, extStreamErr.Code, streamErr.Code);
    }
}

} // end http_internal_test_package
