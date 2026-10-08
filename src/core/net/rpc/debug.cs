// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

/*
	Some HTML presented at http://machine:port/debug/rpc
	Lists services, their methods, and some statistics, still rudimentary.
*/
using fmt = fmt_package;
using template = html.template_package;
using Δhttp = global::go.net.http_package;
using slices = slices_package;
using strings = strings_package;
using global::go.net;
using html;
using io = io_package;

partial class rpc_package {

internal static readonly @string debugText = """
<html>
	<body>
	<title>Services</title>
	{{range .}}
	<hr>
	Service {{.Name}}
	<hr>
		<table>
		<th align=center>Method</th><th align=center>Calls</th>
		{{range .Method}}
			<tr>
			<td align=left font=fixed>{{.Name}}({{.Type.ArgType}}, {{.Type.ReplyType}}) error</td>
			<td align=center>{{.Type.NumCalls}}</td>
			</tr>
		{{end}}
		</table>
	{{end}}
	</body>
	</html>
"""u8;

internal static (ж<template.Template>, error) tupleᴛ1ʗ = template.New("RPC debug"u8).Parse(debugText);
internal static ж<template.Template> debug = template.Must(tupleᴛ1ʗ.Item1, tupleᴛ1ʗ.Item2);

// If set, print log statements for internal and I/O errors.
internal static bool debugLog = false;

public partial struct debugMethod {
    public ж<methodType> Type;
    public @string Name;
}

partial struct methodArray /*[]debugMethod*/;

partial struct debugService {
    public ж<service> Service;
    public @string Name;
    public slice<debugMethod> Method;
}

partial struct serviceArray /*[]debugService*/;

internal static nint Len(this serviceArray s) {
    return len(s);
}

internal static bool Less(this serviceArray s, nint i, nint j) {
    return s[i].Name < s[j].Name;
}

internal static void Swap(this serviceArray s, nint i, nint j) {
    (s[i], s[j]) = (s[j], s[i]);
}

internal static nint Len(this methodArray m) {
    return len(m);
}

internal static bool Less(this methodArray m, nint i, nint j) {
    return m[i].Name < m[j].Name;
}

internal static void Swap(this methodArray m, nint i, nint j) {
    (m[i], m[j]) = (m[j], m[i]);
}

partial struct debugHTTP {
    public partial ref ж<Server> Server { get; }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object rpcErrorExecutingˢ = (@string)"rpc: error executing template:"u8;

// Runs at /debug/rpc
internal static void ServeHTTP(this debugHTTP server, Δhttp.ResponseWriter w, ж<Δhttp.Request> Ꮡreq) {
    // Build a sorted version of the data.
    ref var services = ref heap<serviceArray>(out var Ꮡservices);
    Ꮡ(server).of(debugHTTP.ᏑserviceMap).Range((any snamei, any svci) => {
        var svc = svci._<ж<service>>();
        var ds = new debugService(svc, snamei._<@string>(), new slice<debugMethod>(0, len((~svc).method)));
        foreach (var (mname, method) in (~svc).method) {
            ds.Method = append(ds.Method, new debugMethod(method, mname));
        }
        slices.SortFunc(ds.Method, (debugMethod a, debugMethod b) => strings.Compare(a.Name, b.Name));
        Ꮡservices.ValueSlot = append(Ꮡservices.ValueSlot, ds);
        return true;
    });
    slices.SortFunc(services, (debugService a, debugService b) => strings.Compare(a.Name, b.Name));
    var err = debug.Execute(new http_ResponseWriterᴠWriter(w), services);
    if (err != default!) {
        fmt.Fprintln(new http_ResponseWriterᴠWriter(w), rpcErrorExecutingˢ, err.Error());
    }
}

} // end rpc_package
