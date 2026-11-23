// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;

partial class net_package {

// if non-nil, overrides dialTCP.
internal static Func<context.Context, @string, ж<TCPAddr>, ж<TCPAddr>, (ж<TCPConn>, error)> testHookDialTCP;

[GoLazy] partial class lazyᴛtestHookLookupIP { internal static Func<context.Context, Func<context.Context, @string, @string, (slice<IPAddr>, error)>, @string, @string, (slice<IPAddr>, error)> testHookLookupIP = [GoFunc("init.func1")] (context.Context ctx, Func<context.Context, @string, @string, (slice<IPAddr>, error)> fn, @string network, @string host) => fn(ctx, network, host); }
internal static Action<ж<netFD>> testPreHookSetKeepAlive = [GoFunc("init.func2")] (ж<netFD> _) => {
};
internal static Action<KeepAliveConfig> testHookSetKeepAlive = [GoFunc("init.func3")] (KeepAliveConfig _) => {
};

// testHookStepTime sleeps until time has moved forward by a nonzero amount.
// This helps to avoid flakes in timeout tests by ensuring that an implausibly
// short deadline (such as 1ns in the future) is always expired by the time
// a relevant system call occurs.
internal static Action testHookStepTime = [GoFunc("init.func4")] () => {
};

} // end net_package
