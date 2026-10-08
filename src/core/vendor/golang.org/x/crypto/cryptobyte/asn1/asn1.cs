// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package asn1 contains supporting types for parsing and building ASN.1
// messages with the cryptobyte package.
namespace go.vendor.golang.org.x.crypto.cryptobyte;

partial class asn1_package {

partial struct Tag /*num:uint8*/;

internal static UntypedInt classConstructed => 0x20;
internal static UntypedInt classContextSpecific => 0x80;

// Constructed returns t with the constructed class bit set.
public static Tag Constructed(this Tag t) {
    return (Tag)(t | (uint8)classConstructed);
}

// ContextSpecific returns t with the context-specific class bit set.
public static Tag ContextSpecific(this Tag t) {
    return (Tag)(t | (uint8)classContextSpecific);
}

// The following is a list of standard tag and class combinations.
public static Tag BOOLEAN => /* Tag(1) */ 1;

public static Tag INTEGER => /* Tag(2) */ 2;

public static Tag BIT_STRING => /* Tag(3) */ 3;

public static Tag OCTET_STRING => /* Tag(4) */ 4;

public static Tag NULL => /* Tag(5) */ 5;

public static Tag OBJECT_IDENTIFIER => /* Tag(6) */ 6;

public static Tag ENUM => /* Tag(10) */ 10;

public static Tag UTF8String => /* Tag(12) */ 12;

public static Tag SEQUENCE => /* Tag(16 | classConstructed) */ 48;

public static Tag SET => /* Tag(17 | classConstructed) */ 49;

public static Tag PrintableString => /* Tag(19) */ 19;

public static Tag T61String => /* Tag(20) */ 20;

public static Tag IA5String => /* Tag(22) */ 22;

public static Tag UTCTime => /* Tag(23) */ 23;

public static Tag GeneralizedTime => /* Tag(24) */ 24;

public static Tag GeneralString => /* Tag(27) */ 27;

} // end asn1_package
