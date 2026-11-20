// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using static textproto_package;

partial class textproto_package {

// A MIMEHeader represents a MIME-style header mapping
// keys to sets of values.
partial struct MIMEHeader /*map[@string, slice<@string>]*/;

} partial class textproto_package {

// Add adds the key, value pair to the header.
// It appends to any existing values associated with key.
internal static void Addᴍ(MIMEHeader h, @string key, @string value) {
    key = CanonicalMIMEHeaderKey(key);
    h[key] = append(h[key], value);
}

// Set sets the header entries associated with key to
// the single element value. It replaces any existing
// values associated with key.
internal static void Setᴍ(MIMEHeader h, @string key, @string value) {
    h[CanonicalMIMEHeaderKey(key)] = [value];
}

// Get gets the first value associated with the given key.
// It is case insensitive; [CanonicalMIMEHeaderKey] is used
// to canonicalize the provided key.
// If there are no values associated with the key, Get returns "".
// To use non-canonical keys, access the map directly.
internal static @string Getᴍ(MIMEHeader h, @string key) {
    if (h == nil) {
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
internal static slice<@string> Valuesᴍ(MIMEHeader h, @string key) {
    if (h == nil) {
        return default;
    }
    return h[CanonicalMIMEHeaderKey(key)];
}

// Del deletes the values associated with key.
internal static void Delᴍ(MIMEHeader h, @string key) {
    delete(h, CanonicalMIMEHeaderKey(key));
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class textprotoᴛMIMEHeader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Add(this MIMEHeader h, @string key, @string value) => textproto_package.Addᴍ(h, key, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Set(this MIMEHeader h, @string key, @string value) => textproto_package.Setᴍ(h, key, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Get(this MIMEHeader h, @string key) => textproto_package.Getᴍ(h, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<@string> Values(this MIMEHeader h, @string key) => textproto_package.Valuesᴍ(h, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Del(this MIMEHeader h, @string key) => textproto_package.Delᴍ(h, key);
}
// </GlyphForwarders>
