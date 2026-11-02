// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using Δregexp = regexp_package;
using Δruntime = runtime_package;
using static runtime_package;
using slices = slices_package;
using Δsync = sync_package;
using testing = testing_package;
using time = time_package;
using @unsafe = unsafe_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string basicWriteReadˢ => ref basicWriteReadˢᵇ;
private static readonly @string basicWriteReadˢᵇ = "BasicWriteRead"u8;
internal static ref readonly @string readManyˢ => ref readManyˢᵇ;
private static readonly @string readManyˢᵇ = "ReadMany"u8;
internal static ref readonly @string readManyShortDataˢ => ref readManyShortDataˢᵇ;
private static readonly @string readManyShortDataˢᵇ = "ReadManyShortData"u8;
internal static ref readonly @string readManyShortTagsˢ => ref readManyShortTagsˢᵇ;
private static readonly @string readManyShortTagsˢᵇ = "ReadManyShortTags"u8;
internal static ref readonly @string readAfterOverflow1ˢ => ref readAfterOverflow1ˢᵇ;
private static readonly @string readAfterOverflow1ˢᵇ = "ReadAfterOverflow1"u8;
internal static ref readonly @string readAfterOverflow2ˢ => ref readAfterOverflow2ˢᵇ;
private static readonly @string readAfterOverflow2ˢᵇ = "ReadAfterOverflow2"u8;
internal static ref readonly @string readAtEndAfterOverflowˢ => ref readAtEndAfterOverflowˢᵇ;
private static readonly @string readAtEndAfterOverflowˢᵇ = "ReadAtEndAfterOverflow"u8;
internal static ref readonly @string blockingWriteReadˢ => ref blockingWriteReadˢᵇ;
private static readonly @string blockingWriteReadˢᵇ = "BlockingWriteRead"u8;
internal static ref readonly @string dataWraparoundˢ => ref dataWraparoundˢᵇ;
private static readonly @string dataWraparoundˢᵇ = "DataWraparound"u8;
internal static ref readonly @string tagWraparoundˢ => ref tagWraparoundˢᵇ;
private static readonly @string tagWraparoundˢᵇ = "TagWraparound"u8;
internal static ref readonly @string bothWraparoundˢ => ref bothWraparoundˢᵇ;
private static readonly @string bothWraparoundˢᵇ = "BothWraparound"u8;

public static void TestProfBuf(ж<testing.T> t) {
    UntypedInt hdrSize = 2;

    void write(ж<testing.T> tΔ1, ж<global::go.runtime_internal_test_package.ProfBuf> b, @unsafe.Pointer tagʗp, int64 now, slice<uint64> hdr, slice<uintptr> stk) {
        ref var tag = ref heap(tagʗp, out var Ꮡtag);
        b.Write(Ꮡtag, now, hdr, stk);
    }
    void read(ж<testing.T> tΔ2, ж<global::go.runtime_internal_test_package.ProfBuf> b, slice<uint64> data, slice<@unsafe.Pointer> tags) {
        var (rdata, rtags, eof) = b.Read(runtime_internal_test_package.ProfBufNonBlocking);
        if (!slices.Equal<slice<uint64>, uint64>(rdata, data) || !slices.Equal<slice<@unsafe.Pointer>, @unsafe.Pointer>(rtags, tags)) {
            tΔ2.Fatalf("unexpected profile read:\nhave data %#x\nwant data %#x\nhave tags %#x\nwant tags %#x", rdata, data, rtags, tags);
        }
        if (eof) {
            tΔ2.Fatalf("unexpected eof");
        }
    }
    [MethodImpl(MethodImplOptions.NoInlining)] Action readBlock(ж<testing.T> tΔ3, ж<global::go.runtime_internal_test_package.ProfBuf> b, slice<uint64> data, slice<@unsafe.Pointer> tags) {
        var c = new channel<nint>(0);
        var cʗ1 = c;
        var dataʗ1 = data;
        var tagsʗ1 = tags;
        goǃ(() => {
            var eof = dataʗ1 == nil;
            var (rdata, rtags, reof) = b.Read(runtime_internal_test_package.ProfBufBlocking);
            if (!slices.Equal<slice<uint64>, uint64>(rdata, dataʗ1) || !slices.Equal<slice<@unsafe.Pointer>, @unsafe.Pointer>(rtags, tagsʗ1) || reof != eof) {
                // Errorf, not Fatalf, because called in goroutine.
                tΔ3.Errorf("unexpected profile read:\nhave data %#x\nwant data %#x\nhave tags %#x\nwant tags %#x\nhave eof=%v, want %v", rdata, dataʗ1, rtags, tagsʗ1, reof, eof);
            }
            cʗ1.ᐸꟷ(1);
        });
        time.Sleep(10 * time.Millisecond); // let goroutine run and block
        var cʗ2 = c;
        return () => {
            ᐸꟷ(cʗ2);
        };
    }
    void readEOF(ж<testing.T> tΔ4, ж<global::go.runtime_internal_test_package.ProfBuf> b) {
        var (rdata, rtags, eof) = b.Read(runtime_internal_test_package.ProfBufBlocking);
        if (rdata != nil || rtags != nil || !eof) {
            tΔ4.Errorf("unexpected profile read: %#x, %#x, eof=%v; want nil, nil, eof=true", rdata, rtags, eof);
        }
        (rdata, rtags, eof) = b.Read(runtime_internal_test_package.ProfBufNonBlocking);
        if (rdata != nil || rtags != nil || !eof) {
            tΔ4.Errorf("unexpected profile read (non-blocking): %#x, %#x, eof=%v; want nil, nil, eof=true", rdata, rtags, eof);
        }
    }

    var myTags = new slice<byte>(100);
    t.Logf("myTags is %p", Ꮡ(myTags, 0));

    var myTagsʗ1 = myTags;
    var readʗ1 = read;
    var writeʗ1 = write;
    t.Run(basicWriteReadˢ, (ж<testing.T> tΔ5) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 11, 1);
        writeʗ1(tΔ5, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ1, 0)), 1, slice<uint64>([2, 3]), slice<uintptr>([4, 5, 6, 7, 8, 9]));
        readʗ1(tΔ5, b, slice<uint64>([10, 1, 2, 3, 4, 5, 6, 7, 8, 9]), slice<@unsafe.Pointer>([@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ1, 0))]));
        readʗ1(tΔ5, b, default, default); // release data returned by previous read
        writeʗ1(tΔ5, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ1, 2)), 99, slice<uint64>([101, 102]), slice<uintptr>([201, 202, 203, 204]));
        readʗ1(tΔ5, b, slice<uint64>([8, 99, 101, 102, 201, 202, 203, 204]), slice<@unsafe.Pointer>([@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ1, 2))]));
    });

    var myTagsʗ2 = myTags;
    var readʗ2 = read;
    var writeʗ2 = write;
    t.Run(readManyˢ, (ж<testing.T> tΔ6) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 50, 50);
        writeʗ2(tΔ6, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ2, 0)), 1, slice<uint64>([2, 3]), slice<uintptr>([4, 5, 6, 7, 8, 9]));
        writeʗ2(tΔ6, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ2, 2)), 99, slice<uint64>([101, 102]), slice<uintptr>([201, 202, 203, 204]));
        writeʗ2(tΔ6, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ2, 1)), 500, slice<uint64>([502, 504]), slice<uintptr>([506]));
        readʗ2(tΔ6, b, slice<uint64>([10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 99, 101, 102, 201, 202, 203, 204, 5, 500, 502, 504, 506]), slice<@unsafe.Pointer>([@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ2, 0)), @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ2, 2)), @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ2, 1))]));
    });

    var myTagsʗ3 = myTags;
    var readʗ3 = read;
    var writeʗ3 = write;
    t.Run(readManyShortDataˢ, (ж<testing.T> tΔ7) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 50, 50);
        writeʗ3(tΔ7, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ3, 0)), 1, slice<uint64>([2, 3]), slice<uintptr>([4, 5, 6, 7, 8, 9]));
        writeʗ3(tΔ7, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ3, 2)), 99, slice<uint64>([101, 102]), slice<uintptr>([201, 202, 203, 204]));
        readʗ3(tΔ7, b, slice<uint64>([10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 99, 101, 102, 201, 202, 203, 204]), slice<@unsafe.Pointer>([@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ3, 0)), @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ3, 2))]));
    });

    var myTagsʗ4 = myTags;
    var readʗ4 = read;
    var writeʗ4 = write;
    t.Run(readManyShortTagsˢ, (ж<testing.T> tΔ8) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 50, 50);
        writeʗ4(tΔ8, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ4, 0)), 1, slice<uint64>([2, 3]), slice<uintptr>([4, 5, 6, 7, 8, 9]));
        writeʗ4(tΔ8, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ4, 2)), 99, slice<uint64>([101, 102]), slice<uintptr>([201, 202, 203, 204]));
        readʗ4(tΔ8, b, slice<uint64>([10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 99, 101, 102, 201, 202, 203, 204]), slice<@unsafe.Pointer>([@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ4, 0)), @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ4, 2))]));
    });

    var myTagsʗ5 = myTags;
    var readʗ5 = read;
    var writeʗ5 = write;
    t.Run(readAfterOverflow1ˢ, (ж<testing.T> tΔ9) => {
        // overflow record synthesized by write
        var b = runtime_internal_test_package.NewProfBuf(2, 16, 5);
        writeʗ5(tΔ9, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ5, 0)), 1, slice<uint64>([2, 3]), slice<uintptr>([4, 5, 6, 7, 8, 9]));            // uses 10
        readʗ5(tΔ9, b, slice<uint64>([10, 1, 2, 3, 4, 5, 6, 7, 8, 9]), slice<@unsafe.Pointer>([@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ5, 0))])); // reads 10 but still in use until next read
        writeʗ5(tΔ9, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ5, 0)), 1, slice<uint64>([2, 3]), slice<uintptr>([4, 5]));                        // uses 6
        readʗ5(tΔ9, b, slice<uint64>([6, 1, 2, 3, 4, 5]), slice<@unsafe.Pointer>([@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ5, 0))]));              // reads 6 but still in use until next read
        // now 10 available
        writeʗ5(tΔ9, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ5, 2)), 99, slice<uint64>([101, 102]), slice<uintptr>([201, 202, 203, 204, 205, 206, 207, 208, 209])); // no room
        for (nint i = 0; i < 299; i++) {
            writeʗ5(tΔ9, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ5, 3)), (int64)(100 + i), slice<uint64>([101, 102]), slice<uintptr>([201, 202, 203, 204])); // no room for overflow+this record
        }
        writeʗ5(tΔ9, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ5, 1)), 500, slice<uint64>([502, 504]), slice<uintptr>([506])); // room for overflow+this record
        readʗ5(tΔ9, b, slice<uint64>([5, 99, 0, 0, 300, 5, 500, 502, 504, 506]), slice<@unsafe.Pointer>([default, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ5, 1))]));
    });

    var myTagsʗ6 = myTags;
    var readʗ6 = read;
    var writeʗ6 = write;
    t.Run(readAfterOverflow2ˢ, (ж<testing.T> tΔ10) => {
        // overflow record synthesized by read
        var b = runtime_internal_test_package.NewProfBuf(2, 16, 5);
        writeʗ6(tΔ10, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ6, 0)), 1, slice<uint64>([2, 3]), slice<uintptr>([4, 5, 6, 7, 8, 9]));
        writeʗ6(tΔ10, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ6, 2)), 99, slice<uint64>([101, 102]), slice<uintptr>([201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213]));
        for (nint i = 0; i < 299; i++) {
            writeʗ6(tΔ10, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ6, 3)), 100, slice<uint64>([101, 102]), slice<uintptr>([201, 202, 203, 204]));
        }
        readʗ6(tΔ10, b, slice<uint64>([10, 1, 2, 3, 4, 5, 6, 7, 8, 9]), slice<@unsafe.Pointer>([@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ6, 0))])); // reads 10 but still in use until next read
        writeʗ6(tΔ10, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ6, 1)), 500, slice<uint64>([502, 504]), slice<uintptr>([]));                      // still overflow
        readʗ6(tΔ10, b, slice<uint64>([5, 99, 0, 0, 301]), slice<@unsafe.Pointer>([default]));                                                    // overflow synthesized by read
        writeʗ6(tΔ10, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ6, 1)), 500, slice<uint64>([502, 505]), slice<uintptr>([506]));                   // written
        readʗ6(tΔ10, b, slice<uint64>([5, 500, 502, 505, 506]), slice<@unsafe.Pointer>([@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ6, 1))]));
    });

    var myTagsʗ7 = myTags;
    var readʗ7 = read;
    var writeʗ7 = write;
    t.Run(readAtEndAfterOverflowˢ, (ж<testing.T> tΔ11) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 12, 5);
        writeʗ7(tΔ11, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ7, 0)), 1, slice<uint64>([2, 3]), slice<uintptr>([4, 5, 6, 7, 8, 9]));
        writeʗ7(tΔ11, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ7, 2)), 99, slice<uint64>([101, 102]), slice<uintptr>([201, 202, 203, 204]));
        for (nint i = 0; i < 299; i++) {
            writeʗ7(tΔ11, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ7, 3)), 100, slice<uint64>([101, 102]), slice<uintptr>([201, 202, 203, 204]));
        }
        readʗ7(tΔ11, b, slice<uint64>([10, 1, 2, 3, 4, 5, 6, 7, 8, 9]), slice<@unsafe.Pointer>([@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ7, 0))]));
        readʗ7(tΔ11, b, slice<uint64>([5, 99, 0, 0, 300]), slice<@unsafe.Pointer>([default]));
        writeʗ7(tΔ11, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ7, 1)), 500, slice<uint64>([502, 504]), slice<uintptr>([506]));
        readʗ7(tΔ11, b, slice<uint64>([5, 500, 502, 504, 506]), slice<@unsafe.Pointer>([@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ7, 1))]));
    });

    var myTagsʗ8 = myTags;
    var readBlockʗ1 = readBlock;
    var readEOFʗ1 = readEOF;
    var writeʗ8 = write;
    t.Run(blockingWriteReadˢ, (ж<testing.T> tΔ12) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 11, 1);
        var wait = readBlockʗ1(tΔ12, b, slice<uint64>([10, 1, 2, 3, 4, 5, 6, 7, 8, 9]), slice<@unsafe.Pointer>([@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ8, 0))]));
        writeʗ8(tΔ12, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ8, 0)), 1, slice<uint64>([2, 3]), slice<uintptr>([4, 5, 6, 7, 8, 9]));
        wait();
        wait = readBlockʗ1(tΔ12, b, slice<uint64>([8, 99, 101, 102, 201, 202, 203, 204]), slice<@unsafe.Pointer>([@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ8, 2))]));
        time.Sleep(10 * time.Millisecond);
        writeʗ8(tΔ12, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ8, 2)), 99, slice<uint64>([101, 102]), slice<uintptr>([201, 202, 203, 204]));
        wait();
        wait = readBlockʗ1(tΔ12, b, default, default);
        b.Close();
        wait();
        wait = readBlockʗ1(tΔ12, b, default, default);
        wait();
        readEOFʗ1(tΔ12, b);
    });

    var myTagsʗ9 = myTags;
    var readʗ8 = read;
    var writeʗ9 = write;
    t.Run(dataWraparoundˢ, (ж<testing.T> tΔ13) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 16, 1024);
        for (nint i = 0; i < 10; i++) {
            writeʗ9(tΔ13, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ9, 0)), 1, slice<uint64>([2, 3]), slice<uintptr>([4, 5, 6, 7, 8, 9]));
            readʗ8(tΔ13, b, slice<uint64>([10, 1, 2, 3, 4, 5, 6, 7, 8, 9]), slice<@unsafe.Pointer>([@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ9, 0))]));
            readʗ8(tΔ13, b, default, default); // release data returned by previous read
        }
    });

    var myTagsʗ10 = myTags;
    var readʗ9 = read;
    var writeʗ10 = write;
    t.Run(tagWraparoundˢ, (ж<testing.T> tΔ14) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 1024, 2);
        for (nint i = 0; i < 10; i++) {
            writeʗ10(tΔ14, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ10, 0)), 1, slice<uint64>([2, 3]), slice<uintptr>([4, 5, 6, 7, 8, 9]));
            readʗ9(tΔ14, b, slice<uint64>([10, 1, 2, 3, 4, 5, 6, 7, 8, 9]), slice<@unsafe.Pointer>([@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ10, 0))]));
            readʗ9(tΔ14, b, default, default); // release data returned by previous read
        }
    });

    var myTagsʗ11 = myTags;
    var readʗ10 = read;
    var writeʗ11 = write;
    t.Run(bothWraparoundˢ, (ж<testing.T> tΔ15) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 16, 2);
        for (nint i = 0; i < 10; i++) {
            writeʗ11(tΔ15, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ11, 0)), 1, slice<uint64>([2, 3]), slice<uintptr>([4, 5, 6, 7, 8, 9]));
            readʗ10(tΔ15, b, slice<uint64>([10, 1, 2, 3, 4, 5, 6, 7, 8, 9]), slice<@unsafe.Pointer>([@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ11, 0))]));
            readʗ10(tΔ15, b, default, default); // release data returned by previous read
        }
    });
}

public static partial void TestProfBufDoubleWakeup(ж<testing.T> t) {
    var b = runtime_internal_test_package.NewProfBuf(2, 16, 2);
    var bʗ1 = b;
    goǃ(() => {
        foreach (var _ᴛ1 in range(1000)) {
            bʗ1.Write(nil, 1, slice<uint64>([5, 6]), slice<uintptr>([7, 8]));
        }
        bʗ1.Close();
    });
    for (;;) {
        var (_, _, eof) = b.Read(runtime_internal_test_package.ProfBufBlocking);
        if (eof) {
            return;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string waitingˢ => ref waitingˢᵇ;
private static readonly @string waitingˢᵇ = "waiting"u8;
internal static ref readonly @string runnableˢ => ref runnableˢᵇ;
private static readonly @string runnableˢᵇ = "runnable"u8;
internal static ref readonly @string syscallˢ => ref syscallˢᵇ;
private static readonly @string syscallˢᵇ = "syscall"u8;
internal static ref readonly @string testProfBufWakeupFunc1ˢ => ref testProfBufWakeupFunc1ˢᵇ;
private static readonly @string testProfBufWakeupFunc1ˢᵇ = "TestProfBufWakeup.func1"u8;

public static void TestProfBufWakeup(ж<testing.T> t) {
    var b = runtime_internal_test_package.NewProfBuf(2, 16, 2);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    var bʗ1 = b;
    Ꮡwg.Go(() => {
        nint read = 0;
        for (;;) {
            var (rdata, _, eof) = bʗ1.Read(runtime_internal_test_package.ProfBufBlocking);
            if (read == 0 && len(rdata) < 8) {
                t.Errorf("first wake up at less than half full, got %x", rdata);
            }
            read += len(rdata);
            if (eof) {
                return;
            }
        }
    });

    // Under the hood profBuf uses notetsleepg when the reader blocks.
    // Different platforms have different implementations, leading to
    // different statuses we need to look for to determine whether the
    // reader is blocked.
    @string waitStatus = default;
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "js"u8: {
        waitStatus = waitingˢ;
        break;
    }
    case {} when exprᴛ1 == "wasip1"u8: {
        waitStatus = runnableˢ;
        break;
    }
    default: {
        waitStatus = syscallˢ;
        break;
    }}

    // Ensure that the reader is blocked
    awaitBlockedGoroutine(waitStatus, testProfBufWakeupFunc1ˢ);
    // NB: this writes 6 words not 4. Fine for the test.
    // The reader shouldn't wake up for this
    b.Write(nil, 1, slice<uint64>([1, 2]), slice<uintptr>([3, 4]));

    // The reader should still be blocked. The awaitBlockedGoroutine here
    // checks that and also gives a buggy implementation a chance to
    // actually wake up (it calls Gosched) before the next write. There is a
    // small chance that a buggy implementation would have woken up but
    // doesn't get scheduled by the time we do the next write. In that case
    // the reader will see a more-than-half-full buffer and the test will
    // pass. But if the implementation is broken, this test should fail
    // regularly, even if not 100% of the time.
    awaitBlockedGoroutine(waitStatus, testProfBufWakeupFunc1ˢ);
    b.Write(nil, 1, slice<uint64>([5, 6]), slice<uintptr>([7, 8]));
    b.Close();

    // Wait here so we can be sure the reader got the data
    Ꮡwg.Wait();
}

// see also runtime/pprof tests
internal static partial void awaitBlockedGoroutine(@string state, @string fName) {
    // NB: this matches [state] as well as [state, n minutes]
    @string re = fmt.Sprintf(@"(?m)^goroutine \d+ \[%s.*\]:\n(?:.+\n\t.+\n)*runtime_test\.%s"u8, Δregexp.QuoteMeta(state), fName);
    var r = Δregexp.MustCompile(re);

    var buf = new slice<byte>((64 << 10));
    for (;;) {
        Gosched();
        nint n = Stack(buf, true);
        if (n == len(buf)) {
            // Buffer wasn't large enough for a full goroutine dump.
            // Resize it and try again.
            buf = new slice<byte>(2 * len(buf));
            continue;
        }
        const nint count = 1;
        if (len(r.FindAll(buf.slice(0, n), -1)) >= count) {
            return;
        }
    }
}

} // end runtime_test_package
