// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

partial class textproto_package {

partial struct MIMEHeader /*map[@string, slice<@string>]*/;

// Add adds the key, value pair to the header.
// It appends to any existing values associated with key.
public static void Add(this MIMEHeader h, @string key, @string value) {
    key = CanonicalMIMEHeaderKey(key);
    h[key] = append(h[key], value);
}

// Set sets the header entries associated with key to
// the single element value. It replaces any existing
// values associated with key.
public static void Set(this MIMEHeader h, @string key, @string value) {
    h[CanonicalMIMEHeaderKey(key)] = new @string[]{value}.slice();
}

// Get gets the first value associated with the given key.
// It is case insensitive; [CanonicalMIMEHeaderKey] is used
// to canonicalize the provided key.
// If there are no values associated with the key, Get returns "".
// To use non-canonical keys, access the map directly.
public static @string Get(this MIMEHeader h, @string key) {
    if (h == default!) {
        return ""u8;
    }
    var v = h[CanonicalMIMEHeaderKey(key)];
    if (len(v) == 0) {
        return ""u8;
    }
    return v[0];
}

// Values returns all values associated with the given key.
// It is case insensitive; [CanonicalMIMEHeaderKey] is
// used to canonicalize the provided key. To use non-canonical
// keys, access the map directly.
// The returned slice is not a copy.
public static slice<@string> Values(this MIMEHeader h, @string key) {
    if (h == default!) {
        return default!;
    }
    return h[CanonicalMIMEHeaderKey(key)];
}

// Del deletes the values associated with key.
public static void Del(this MIMEHeader h, @string key) {
    delete(h, CanonicalMIMEHeaderKey(key));
}

} // end textproto_package
