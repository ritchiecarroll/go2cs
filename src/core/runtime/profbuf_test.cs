// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static runtime_package;
using slices = slices_package;
using testing = testing_package;
using time = time_package;
using @unsafe = unsafe_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static global::go.runtime_internal_test_package;
using Δruntime = runtime_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string basicWriteReadˢ = "BasicWriteRead"u8;
internal static readonly @string readManyˢ = "ReadMany"u8;
internal static readonly @string readManyShortDataˢ = "ReadManyShortData"u8;
internal static readonly @string readManyShortTagsˢ = "ReadManyShortTags"u8;
internal static readonly @string readAfterOverflow1ˢ = "ReadAfterOverflow1"u8;
internal static readonly @string readAfterOverflow2ˢ = "ReadAfterOverflow2"u8;
internal static readonly @string readAtEndAfterOverflowˢ = "ReadAtEndAfterOverflow"u8;
internal static readonly @string blockingWriteReadˢ = "BlockingWriteRead"u8;
internal static readonly @string dataWraparoundˢ = "DataWraparound"u8;
internal static readonly @string tagWraparoundˢ = "TagWraparound"u8;
internal static readonly @string bothWraparoundˢ = "BothWraparound"u8;

public static void TestProfBuf(ж<testing.T> Ꮡt) {
    UntypedInt hdrSize = 2;
    void write(ж<testing.T> tΔ1, ж<global::go.runtime_internal_test_package.ProfBuf> b, @unsafe.Pointer tagʗp, int64 now, slice<uint64> hdr, slice<uintptr> stk) {
        ref var tag = ref heap(tagʗp, out var Ꮡtag);
        b.Write(Ꮡtag, now, hdr, stk);
    }
    void read(ж<testing.T> tΔ2, ж<global::go.runtime_internal_test_package.ProfBuf> b, slice<uint64> data, slice<@unsafe.Pointer> tags) {
        var (rdata, rtags, eof) = b.Read(runtime_internal_test_package.ProfBufNonBlocking);
        if (!slices.Equal<slice<uint64>, uint64>(rdata, data) || !slices.Equal<slice<@unsafe.Pointer>, @unsafe.Pointer>(rtags, tags)) {
            tΔ2.Fatalf("unexpected profile read:\nhave data %#x\nwant data %#x\nhave tags %#x\nwant tags %#x"u8, rdata, data, rtags, tags);
        }
        if (eof) {
            tΔ2.Fatalf("unexpected eof"u8);
        }
    }
    [MethodImpl(MethodImplOptions.NoInlining)] Action readBlock(ж<testing.T> tΔ3, ж<global::go.runtime_internal_test_package.ProfBuf> b, slice<uint64> data, slice<@unsafe.Pointer> tags) {
        var c = new channel<nint>(0);
        var cʗ1 = c;
        var dataʗ1 = data;
        var tagsʗ1 = tags;
        goǃ(() => {
            var eof = dataʗ1 == default!;
            var (rdata, rtags, reof) = b.Read(runtime_internal_test_package.ProfBufBlocking);
            if (!slices.Equal<slice<uint64>, uint64>(rdata, dataʗ1) || !slices.Equal<slice<@unsafe.Pointer>, @unsafe.Pointer>(rtags, tagsʗ1) || reof != eof) {
                // Errorf, not Fatalf, because called in goroutine.
                tΔ3.Errorf("unexpected profile read:\nhave data %#x\nwant data %#x\nhave tags %#x\nwant tags %#x\nhave eof=%v, want %v"u8, rdata, dataʗ1, rtags, tagsʗ1, reof, eof);
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
        if (rdata != default! || rtags != default! || !eof) {
            tΔ4.Errorf("unexpected profile read: %#x, %#x, eof=%v; want nil, nil, eof=true"u8, rdata, rtags, eof);
        }
        (rdata, rtags, eof) = b.Read(runtime_internal_test_package.ProfBufNonBlocking);
        if (rdata != default! || rtags != default! || !eof) {
            tΔ4.Errorf("unexpected profile read (non-blocking): %#x, %#x, eof=%v; want nil, nil, eof=true"u8, rdata, rtags, eof);
        }
    }
    var myTags = new slice<byte>(100);
    Ꮡt.Logf("myTags is %p"u8, Ꮡ(myTags, 0));
    var myTagsʗ1 = myTags;
    var readʗ1 = read;
    var writeʗ1 = write;
    Ꮡt.Run(basicWriteReadˢ, (ж<testing.T> tΔ5) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 11, 1);
        writeʗ1(tΔ5, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ1, 0)), 1, new uint64[]{2, 3}.slice(), new uintptr[]{4, 5, 6, 7, 8, 9}.slice());
        readʗ1(tΔ5, b, new uint64[]{10, 1, 2, 3, 4, 5, 6, 7, 8, 9}.slice(), new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ1, 0))}.slice());
        readʗ1(tΔ5, b, default!, default!); // release data returned by previous read
        writeʗ1(tΔ5, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ1, 2)), 99, new uint64[]{101, 102}.slice(), new uintptr[]{201, 202, 203, 204}.slice());
        readʗ1(tΔ5, b, new uint64[]{8, 99, 101, 102, 201, 202, 203, 204}.slice(), new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ1, 2))}.slice());
    });
    var myTagsʗ2 = myTags;
    var readʗ2 = read;
    var writeʗ2 = write;
    Ꮡt.Run(readManyˢ, (ж<testing.T> tΔ6) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 50, 50);
        writeʗ2(tΔ6, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ2, 0)), 1, new uint64[]{2, 3}.slice(), new uintptr[]{4, 5, 6, 7, 8, 9}.slice());
        writeʗ2(tΔ6, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ2, 2)), 99, new uint64[]{101, 102}.slice(), new uintptr[]{201, 202, 203, 204}.slice());
        writeʗ2(tΔ6, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ2, 1)), 500, new uint64[]{502, 504}.slice(), new uintptr[]{506}.slice());
        readʗ2(tΔ6, b, new uint64[]{10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 99, 101, 102, 201, 202, 203, 204, 5, 500, 502, 504, 506}.slice(), new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ2, 0)), @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ2, 2)), @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ2, 1))}.slice());
    });
    var myTagsʗ3 = myTags;
    var readʗ3 = read;
    var writeʗ3 = write;
    Ꮡt.Run(readManyShortDataˢ, (ж<testing.T> tΔ7) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 50, 50);
        writeʗ3(tΔ7, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ3, 0)), 1, new uint64[]{2, 3}.slice(), new uintptr[]{4, 5, 6, 7, 8, 9}.slice());
        writeʗ3(tΔ7, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ3, 2)), 99, new uint64[]{101, 102}.slice(), new uintptr[]{201, 202, 203, 204}.slice());
        readʗ3(tΔ7, b, new uint64[]{10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 99, 101, 102, 201, 202, 203, 204}.slice(), new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ3, 0)), @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ3, 2))}.slice());
    });
    var myTagsʗ4 = myTags;
    var readʗ4 = read;
    var writeʗ4 = write;
    Ꮡt.Run(readManyShortTagsˢ, (ж<testing.T> tΔ8) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 50, 50);
        writeʗ4(tΔ8, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ4, 0)), 1, new uint64[]{2, 3}.slice(), new uintptr[]{4, 5, 6, 7, 8, 9}.slice());
        writeʗ4(tΔ8, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ4, 2)), 99, new uint64[]{101, 102}.slice(), new uintptr[]{201, 202, 203, 204}.slice());
        readʗ4(tΔ8, b, new uint64[]{10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 99, 101, 102, 201, 202, 203, 204}.slice(), new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ4, 0)), @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ4, 2))}.slice());
    });
    var myTagsʗ5 = myTags;
    var readʗ5 = read;
    var writeʗ5 = write;
    Ꮡt.Run(readAfterOverflow1ˢ, (ж<testing.T> tΔ9) => {
        // overflow record synthesized by write
        var b = runtime_internal_test_package.NewProfBuf(2, 16, 5);
        writeʗ5(tΔ9, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ5, 0)), 1, new uint64[]{2, 3}.slice(), new uintptr[]{4, 5, 6, 7, 8, 9}.slice()); // uses 10
        readʗ5(tΔ9, b, new uint64[]{10, 1, 2, 3, 4, 5, 6, 7, 8, 9}.slice(), new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ5, 0))}.slice()); // reads 10 but still in use until next read
        writeʗ5(tΔ9, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ5, 0)), 1, new uint64[]{2, 3}.slice(), new uintptr[]{4, 5}.slice()); // uses 6
        readʗ5(tΔ9, b, new uint64[]{6, 1, 2, 3, 4, 5}.slice(), new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ5, 0))}.slice()); // reads 6 but still in use until next read
        // now 10 available
        writeʗ5(tΔ9, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ5, 2)), 99, new uint64[]{101, 102}.slice(), new uintptr[]{201, 202, 203, 204, 205, 206, 207, 208, 209}.slice()); // no room
        for (nint i = 0; i < 299; i++) {
            writeʗ5(tΔ9, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ5, 3)), (int64)(100 + i), new uint64[]{101, 102}.slice(), new uintptr[]{201, 202, 203, 204}.slice()); // no room for overflow+this record
        }
        writeʗ5(tΔ9, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ5, 1)), 500, new uint64[]{502, 504}.slice(), new uintptr[]{506}.slice()); // room for overflow+this record
        readʗ5(tΔ9, b, new uint64[]{5, 99, 0, 0, 300, 5, 500, 502, 504, 506}.slice(), new @unsafe.Pointer[]{default!, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ5, 1))}.slice());
    });
    var myTagsʗ6 = myTags;
    var readʗ6 = read;
    var writeʗ6 = write;
    Ꮡt.Run(readAfterOverflow2ˢ, (ж<testing.T> tΔ10) => {
        // overflow record synthesized by read
        var b = runtime_internal_test_package.NewProfBuf(2, 16, 5);
        writeʗ6(tΔ10, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ6, 0)), 1, new uint64[]{2, 3}.slice(), new uintptr[]{4, 5, 6, 7, 8, 9}.slice());
        writeʗ6(tΔ10, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ6, 2)), 99, new uint64[]{101, 102}.slice(), new uintptr[]{201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213}.slice());
        for (nint i = 0; i < 299; i++) {
            writeʗ6(tΔ10, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ6, 3)), 100, new uint64[]{101, 102}.slice(), new uintptr[]{201, 202, 203, 204}.slice());
        }
        readʗ6(tΔ10, b, new uint64[]{10, 1, 2, 3, 4, 5, 6, 7, 8, 9}.slice(), new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ6, 0))}.slice()); // reads 10 but still in use until next read
        writeʗ6(tΔ10, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ6, 1)), 500, new uint64[]{502, 504}.slice(), new uintptr[]{}.slice()); // still overflow
        readʗ6(tΔ10, b, new uint64[]{5, 99, 0, 0, 301}.slice(), new @unsafe.Pointer[]{default!}.slice()); // overflow synthesized by read
        writeʗ6(tΔ10, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ6, 1)), 500, new uint64[]{502, 505}.slice(), new uintptr[]{506}.slice()); // written
        readʗ6(tΔ10, b, new uint64[]{5, 500, 502, 505, 506}.slice(), new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ6, 1))}.slice());
    });
    var myTagsʗ7 = myTags;
    var readʗ7 = read;
    var writeʗ7 = write;
    Ꮡt.Run(readAtEndAfterOverflowˢ, (ж<testing.T> tΔ11) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 12, 5);
        writeʗ7(tΔ11, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ7, 0)), 1, new uint64[]{2, 3}.slice(), new uintptr[]{4, 5, 6, 7, 8, 9}.slice());
        writeʗ7(tΔ11, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ7, 2)), 99, new uint64[]{101, 102}.slice(), new uintptr[]{201, 202, 203, 204}.slice());
        for (nint i = 0; i < 299; i++) {
            writeʗ7(tΔ11, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ7, 3)), 100, new uint64[]{101, 102}.slice(), new uintptr[]{201, 202, 203, 204}.slice());
        }
        readʗ7(tΔ11, b, new uint64[]{10, 1, 2, 3, 4, 5, 6, 7, 8, 9}.slice(), new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ7, 0))}.slice());
        readʗ7(tΔ11, b, new uint64[]{5, 99, 0, 0, 300}.slice(), new @unsafe.Pointer[]{default!}.slice());
        writeʗ7(tΔ11, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ7, 1)), 500, new uint64[]{502, 504}.slice(), new uintptr[]{506}.slice());
        readʗ7(tΔ11, b, new uint64[]{5, 500, 502, 504, 506}.slice(), new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ7, 1))}.slice());
    });
    var myTagsʗ8 = myTags;
    var readBlockʗ1 = readBlock;
    var readEOFʗ1 = readEOF;
    var writeʗ8 = write;
    Ꮡt.Run(blockingWriteReadˢ, (ж<testing.T> tΔ12) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 11, 1);
        var wait = readBlockʗ1(tΔ12, b, new uint64[]{10, 1, 2, 3, 4, 5, 6, 7, 8, 9}.slice(), new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ8, 0))}.slice());
        writeʗ8(tΔ12, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ8, 0)), 1, new uint64[]{2, 3}.slice(), new uintptr[]{4, 5, 6, 7, 8, 9}.slice());
        wait();
        wait = readBlockʗ1(tΔ12, b, new uint64[]{8, 99, 101, 102, 201, 202, 203, 204}.slice(), new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ8, 2))}.slice());
        time.Sleep(10 * time.Millisecond);
        writeʗ8(tΔ12, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ8, 2)), 99, new uint64[]{101, 102}.slice(), new uintptr[]{201, 202, 203, 204}.slice());
        wait();
        wait = readBlockʗ1(tΔ12, b, default!, default!);
        b.Close();
        wait();
        wait = readBlockʗ1(tΔ12, b, default!, default!);
        wait();
        readEOFʗ1(tΔ12, b);
    });
    var myTagsʗ9 = myTags;
    var readʗ8 = read;
    var writeʗ9 = write;
    Ꮡt.Run(dataWraparoundˢ, (ж<testing.T> tΔ13) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 16, 1024);
        for (nint i = 0; i < 10; i++) {
            writeʗ9(tΔ13, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ9, 0)), 1, new uint64[]{2, 3}.slice(), new uintptr[]{4, 5, 6, 7, 8, 9}.slice());
            readʗ8(tΔ13, b, new uint64[]{10, 1, 2, 3, 4, 5, 6, 7, 8, 9}.slice(), new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ9, 0))}.slice());
            readʗ8(tΔ13, b, default!, default!); // release data returned by previous read
        }
    });
    var myTagsʗ10 = myTags;
    var readʗ9 = read;
    var writeʗ10 = write;
    Ꮡt.Run(tagWraparoundˢ, (ж<testing.T> tΔ14) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 1024, 2);
        for (nint i = 0; i < 10; i++) {
            writeʗ10(tΔ14, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ10, 0)), 1, new uint64[]{2, 3}.slice(), new uintptr[]{4, 5, 6, 7, 8, 9}.slice());
            readʗ9(tΔ14, b, new uint64[]{10, 1, 2, 3, 4, 5, 6, 7, 8, 9}.slice(), new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ10, 0))}.slice());
            readʗ9(tΔ14, b, default!, default!); // release data returned by previous read
        }
    });
    var myTagsʗ11 = myTags;
    var readʗ10 = read;
    var writeʗ11 = write;
    Ꮡt.Run(bothWraparoundˢ, (ж<testing.T> tΔ15) => {
        var b = runtime_internal_test_package.NewProfBuf(2, 16, 2);
        for (nint i = 0; i < 10; i++) {
            writeʗ11(tΔ15, b, @unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ11, 0)), 1, new uint64[]{2, 3}.slice(), new uintptr[]{4, 5, 6, 7, 8, 9}.slice());
            readʗ10(tΔ15, b, new uint64[]{10, 1, 2, 3, 4, 5, 6, 7, 8, 9}.slice(), new @unsafe.Pointer[]{@unsafe.Pointer.FromPinnedBox(Ꮡ(myTagsʗ11, 0))}.slice());
            readʗ10(tΔ15, b, default!, default!); // release data returned by previous read
        }
    });
}

} // end runtime_test_package
