// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using hex = encoding.hex_package;
using testing = testing_package;
using encoding;
using static go.crypto.tls_package;

partial class tls_internal_test_package {

internal partial struct TestDecodeECHConfigLists_type /*dyn*/ {
    internal @string list;
    internal nint numConfigs;
}

public static void TestDecodeECHConfigLists(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in new TestDecodeECHConfigLists_type[]{
        new("0045fe0d0041590020002092a01233db2218518ccbbbbc24df20686af417b37388de6460e94011974777090004000100010012636c6f7564666c6172652d6563682e636f6d0000"u8, 1),
        new("0105badd00050504030201fe0d0066000010004104e62b69e2bf659f97be2f1e0d948a4cd5976bb7a91e0d46fbdda9a91e9ddcba5a01e7d697a80a18f9c3c4a31e56e27c8348db161a1cf51d7ef1942d4bcf7222c1000c000100010001000200010003400e7075626c69632e6578616d706c650000fe0d003d00002000207d661615730214aeee70533366f36a609ead65c0c208e62322346ab5bcd8de1c000411112222400e7075626c69632e6578616d706c650000fe0d004d000020002085bd6a03277c25427b52e269e0c77a8eb524ba1eb3d2f132662d4b0ac6cb7357000c000100010001000200010003400e7075626c69632e6578616d706c650008aaaa000474657374"u8, 3)
    }.slice()) {
        var (b, err) = hex.DecodeString(tc.list);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var configs, err) = parseECHConfigList(b);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (len(configs) != tc.numConfigs) {
            Ꮡt.Fatalf("unexpected number of configs parsed: got %d want %d"u8, len(configs), tc.numConfigs);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object pickECHConfigPickedAnˢ = (@string)"pickECHConfig picked an invalid config"u8;

public static void TestSkipBadConfigs(ж<testing.T> Ꮡt) {
    var (b, err) = hex.DecodeString("00c8badd00050504030201fe0d0029006666000401020304000c000100010001000200010003400e7075626c69632e6578616d706c650000fe0d003d000020002072e8a23b7aef67832bcc89d652e3870a60f88ca684ec65d6eace6b61f136064c000411112222400e7075626c69632e6578616d706c650000fe0d004d00002000200ce95810a81d8023f41e83679bc92701b2acd46c75869f95c72bc61c6b12297c000c000100010001000200010003400e7075626c69632e6578616d706c650008aaaa000474657374"u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var configs, err) = parseECHConfigList(b);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var config = pickECHConfig(configs);
    if (config != nil) {
        Ꮡt.Fatal(pickECHConfigPickedAnˢ);
    }
}

} // end tls_internal_test_package
