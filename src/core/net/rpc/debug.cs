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
using static rpc_package;

partial class rpc_package {

internal static ref readonly @string debugText => ref debugTextᵇ;
private static readonly @string debugTextᵇ = """
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
"""u8.Raw();

internal static (ж<template.Template>, error) tupleᴛ1ʗ = template.New("RPC debug").Parse(debugText);
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

} partial class rpc_package {

internal static nint Lenᴍ(serviceArray s) { return len(s); }
internal static bool Lessᴍ(serviceArray s, nint i, nint j) { return s[i].Name < s[j].Name; }
internal static void Swapᴍ(serviceArray s, nint i, nint j) { (s[i], s[j]) = (s[j], s[i]); }

} partial class rpc_package {

internal static nint Lenᴍ(methodArray m) { return len(m); }
internal static bool Lessᴍ(methodArray m, nint i, nint j) { return m[i].Name < m[j].Name; }
internal static void Swapᴍ(methodArray m, nint i, nint j) { (m[i], m[j]) = (m[j], m[i]); }

} partial class rpc_package {

partial struct debugHTTP {
    /*embed*/ public ж<Server> Server;
}

} partial class rpc_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object rpcErrorExecutingˢᴍdebugHTTP = (@string)"rpc: error executing template:"u8;

// Runs at /debug/rpc
internal static void ServeHTTPᴍ(debugHTTP server, Δhttp.ResponseWriter w, ж<Δhttp.Request> req) {
    // Build a sorted version of the data.
    ref var services = ref heap<serviceArray>(out var Ꮡservices);
    Ꮡ(server).of(debugHTTP.ᏑserviceMap).Range((any snamei, any svci) => {
        var svc = svci._<ж<service>>();
        var ds = new debugService(svc, snamei._<@string>(), new slice<debugMethod>(0, len(svc.method)));
        foreach (var (mname, method) in svc.method) {
            ds.Method = append(ds.Method, new debugMethod(method, mname));
        }
        slices.SortFunc(ds.Method, (debugMethod a, debugMethod b) => strings.Compare(a.Name, b.Name));
        Ꮡservices.ValueSlot = append(Ꮡservices.ValueSlot, ds);
        return true;
    });
    slices.SortFunc(services, (debugService a, debugService b) => strings.Compare(a.Name, b.Name));
    var err = debug.Execute(new http_ResponseWriterᴠWriter(w), services);
    if (err != nil) {
        fmt.Fprintln(new http_ResponseWriterᴠWriter(w), rpcErrorExecutingˢᴍdebugHTTP, err.Error());
    }
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class rpcᴛserviceArray {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this serviceArray s) => rpc_package.Lenᴍ(s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Less(this serviceArray s, nint i, nint j) => rpc_package.Lessᴍ(s, i, j);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Swap(this serviceArray s, nint i, nint j) => rpc_package.Swapᴍ(s, i, j);
}
partial class rpcᴛmethodArray {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this methodArray m) => rpc_package.Lenᴍ(m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Less(this methodArray m, nint i, nint j) => rpc_package.Lessᴍ(m, i, j);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Swap(this methodArray m, nint i, nint j) => rpc_package.Swapᴍ(m, i, j);
}
partial class rpcᴛdebugHTTP {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ServeHTTP(this debugHTTP server, Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => rpc_package.ServeHTTPᴍ(server, w, req);
}
// </GlyphForwarders>
