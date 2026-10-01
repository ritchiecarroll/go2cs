// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;
using Δsync = sync_package;
using testing = testing_package;
using System.Runtime.CompilerServices;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

[GoType] partial struct response {
}

[GoType] partial struct myError {
}

internal static @string Error(this myError _) {
    return ""u8;
}

[GoType("dyn")] internal partial struct doRequest_async {
    internal ж<response> resp;
    internal error err;
}

internal static (ж<response>, error) doRequest(bool useSelect) {
    var ch = new channel<ж<doRequest_async>>(0);
    var done = new channel<EmptyStruct>(0);
    if (useSelect){
        var chʗ1 = ch;
        var doneʗ1 = done;
        goǃ(() => {
            var selᴛ66 = chʗ1.ᐸꟷ(Ꮡ(new doRequest_async(resp: nil, err: new myError(nil))), ꓸꓸꓸ);
            var selᴛ67 = doneʗ1;
            switch (select(selᴛ66, ᐸꟷ(selᴛ67, ꓸꓸꓸ))) {
            case 0: {
                break;
            }
            case 1 when selᴛ67.ꟷᐳ(out _): {
                break;
            }}
        });
    } else {
        var chʗ2 = ch;
        goǃ(() => {
            chʗ2.ᐸꟷ(Ꮡ(new doRequest_async(resp: nil, err: new myError(nil))));
        });
    }
    var r = ᐸꟷ(ch);
    Δruntime.Gosched();
    return ((~r).resp, (~r).err);
}

public static void TestChanSendSelectBarrier(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    testChanSendBarrier(true);
}

public static void TestChanSendBarrier(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    testChanSendBarrier(false);
}

internal static void testChanSendBarrier(bool useSelect) {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    nint outer = 100;
    nint inner = 100000;
    if (testing.Short() || Δruntime.GOARCH == "wasm"u8) {
        outer = 10;
        inner = 1000;
    }
    for (nint i = 0; i < outer; i++) {
        Ꮡwg.Add(1);
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                slice<byte> garbage = default!;
                for (nint j = 0; j < inner; j++) {
                    var (_, err) = doRequest(useSelect);
                    var (_, ok) = err._<myError>(ᐧ);
                    if (!ok) {
                        throw panic((nint)(1));
                    }
                    garbage = makeByte();
                }
                _ = garbage;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static slice<byte> makeByte() {
    return new slice<byte>((1 << (int)(10)));
}

} // end runtime_test_package
