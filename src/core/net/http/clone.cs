// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using multipart = go.mime.multipart_package;
using textproto = go.net.textproto_package;
using url = go.net.url_package;
/* blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) */ // for linkname
using go.mime;
using go.net;

partial class http_package {

// cloneURLValues should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/searKing/golang
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname cloneURLValues
internal static url.Values cloneURLValues(url.Values v) {
    if (v == nil) {
        return default;
    }
    // http.Header and url.Values have the same representation, so temporarily
    // treat it like http.Header, which does have a clone:
    return (url.Values)(map<@string, slice<@string>>)((ΔHeader)(map<@string, slice<@string>>)v).Clone();
}

// cloneURL should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/searKing/golang
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname cloneURL
internal static ж<url.URL> cloneURL(ж<url.URL> Ꮡu) {
    ref var u = ref Ꮡu.DerefOrNull();

    if (Ꮡu == nil) {
        return default;
    }
    var u2 = @new<url.URL>();
    u2.Value = u;
    if (u.User != nil) {
        u2.User = @new<url.Userinfo>();
        u2.User.Value = u.User.Value;
    }
    return u2;
}

// cloneMultipartForm should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/searKing/golang
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname cloneMultipartForm
internal static ж<multipart.Form> cloneMultipartForm(ж<multipart.Form> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (Ꮡf == nil) {
        return default;
    }
    var f2 = Ꮡ(new multipart.Form(
        Value: ((map<@string, slice<@string>>)((ΔHeader)f.Value).Clone())
    ));
    if (f.File != nil) {
        var m = new map<@string, slice<ж<multipart.FileHeader>>>(builtin.len(f.File));
        foreach (var (k, vv) in f.File) {
            var vv2 = new slice<ж<multipart.FileHeader>>(builtin.len(vv));
            foreach (var (i, v) in vv) {
                vv2[i] = cloneMultipartFileHeader(v);
            }
            m[k] = vv2;
        }
        f2.File = m;
    }
    return f2;
}

// cloneMultipartFileHeader should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/searKing/golang
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname cloneMultipartFileHeader
internal static ж<multipart.FileHeader> cloneMultipartFileHeader(ж<multipart.FileHeader> Ꮡfh) {
    ref var fh = ref Ꮡfh.DerefOrNull();

    if (Ꮡfh == nil) {
        return default;
    }
    var fh2 = @new<multipart.FileHeader>();
    fh2.Value = fh;
    fh2.Header = (textproto.MIMEHeader)(map<@string, slice<@string>>)((ΔHeader)(map<@string, slice<@string>>)fh.Header).Clone();
    return fh2;
}

// cloneOrMakeHeader invokes Header.Clone but if the
// result is nil, it'll instead make and return a non-nil Header.
//
// cloneOrMakeHeader should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/searKing/golang
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname cloneOrMakeHeader
internal static ΔHeader cloneOrMakeHeader(ΔHeader hdr) {
    var clone = hdr.Clone();
    if (clone == nil) {
        clone = new ΔHeader(0);
    }
    return clone;
}

} // end http_package
