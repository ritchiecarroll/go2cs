// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using flag = flag_package;
using fmt = fmt_package;
using url = go.net.url_package;
using go.net;
using static go.flag_internal_test_package;

partial class flag_test_package {

partial struct URLValue {
    public ж<url.URL> URL;
}

public static @string ΔString(this URLValue v) {
    if (v.URL != nil) {
        return v.URL.String();
    }
    return ""u8;
}

public static error ΔSet(this URLValue v, @string s) {
    {
        var (u, err) = url.Parse(s); if (err != default!){
            return err;
        } else {
            v.URL.Value = u.Value;
        }
    }
    return default!;
}

internal static ж<url.URL> u = Ꮡ(new url.URL(nil));

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exampleValueˢ = "ExampleValue"u8;
internal static readonly @string urlˢ = "url"u8;
internal static readonly @string urlToParseˢ = "URL to parse"u8;

public static void ExampleValue() {
    var fs = flag.NewFlagSet(exampleValueˢ, flag.ExitOnError);
    fs.Var(new flag_test_package.URLValueжValue(Ꮡ(new URLValue(u))), urlˢ, urlToParseˢ);
    fs.Parse(new @string[]{"-url"u8, "https://golang.org/pkg/flag/"u8}.slice());
    fmt.Printf(@"{scheme: %q, host: %q, path: %q}"u8, (~u).Scheme, (~u).Host, (~u).Path);
}

// Output:
// {scheme: "https", host: "golang.org", path: "/pkg/flag/"}

} // end flag_test_package
