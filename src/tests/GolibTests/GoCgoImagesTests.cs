using System;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using @unsafe = go.unsafe_package;

namespace GolibTests;

// cgo Phase 3, increment 3.4, the struct image door (docs/phase4/DESIGN-cgo-phase3-marshaling.md 19.7 and 19.9, with P3's
// B4 to B8 folded). net's getnameinfo takes (*C.struct_sockaddr)(unsafe.Pointer(&sa)), sa a syscall.RawSockaddrInet4.
// Its inline arrays are managed references here, so the view is a native box over an ORDER TOKEN with no storage C can
// read, and the plain struct door refused it (TestCgoLookupPTR's red). The door encodes the view's ROOT at Go's own
// offsets into a buffer the wrapper pins, and decodes it back after the call, only when C changed it.
[TestClass]
public unsafe class GoCgoImagesTests
{
    private struct sockIn4
    {
        public uint16 Family;
        public uint16 Port;
        public array<byte> Addr = new(4);
        public array<byte> Zero = new(8);

        public sockIn4() { }
    }

    private struct sockAny
    {
        public uint16 Family;
        public array<sbyte> Data = new(14);

        public sockAny() { }
    }

    private struct small
    {
        public uint16 Family;
        public array<byte> Pad = new(2);

        public small() { }
    }

    private struct withPointer
    {
        public uint16 Family;
        public ж<int32>? P;
        public array<byte> Pad = new(14);

        public withPointer() { }
    }

    private static string RefusalOf(Action call)
    {
        try
        {
            call();
        }
        catch (PanicException panic)
        {
            return panic.Message;
        }

        Assert.Fail("the call was not refused");
        return "";
    }

    private static ж<sockIn4> Root(uint16 port)
    {
        sockIn4 value = new() { Family = 2, Port = port };
        value.Addr[0] = 127;
        value.Addr[3] = 1;
        return new StandardBox<sockIn4>(value);
    }

    // The view crosses as an image: Pinned, the buffer holding the root at Go's offsets (family at 0, port at 2, the
    // address at 4). A write C makes lands in the root at copy-back.
    [TestMethod]
    public void AViewOfAGoStructNoDoorCanPinCrossesAsAnImageAndCWritesComeBack()
    {
        ж<sockIn4> root = Root(0x1234);
        ж<sockAny> view = root.Reinterpret<sockIn4, sockAny>();
        GoCgoImages images = default;

        Assert.AreEqual(GoCgoStructPointer.Pinned, GoCgoArgs.StructPointer(view, "getnameinfo", ref images, out _, out GoCgoPin pin));

        fixed (byte* pinned = pin)
        {
            byte* at = (byte*)pin.Address(pinned);
            Assert.AreEqual((ushort)2, *(ushort*)at, "family at offset 0");
            Assert.AreEqual((ushort)0x1234, *(ushort*)(at + 2), "port at offset 2");
            Assert.AreEqual((byte)127, at[4], "address at offset 4");
            Assert.AreEqual((byte)1, at[7]);

            *(ushort*)(at + 2) = 0x5678; // C writes the port
        }

        images.CopyBack();
        Assert.AreEqual((uint16)0x5678, root.Value.Port, "C's write lands in the Go struct at copy-back");
        Assert.AreEqual((byte)127, root.Value.Addr[0], "the rest is unchanged");
    }

    // B6 (and B7): an image C did not change is not decoded, so a write a goroutine makes to the Go value during the call
    // survives, and nothing is re-encoded.
    [TestMethod]
    public void AnImageCLeftAloneIsNotDecodedSoAConcurrentGoWriteSurvives()
    {
        ж<sockIn4> root = Root(80);
        GoCgoImages images = default;

        Assert.AreEqual(GoCgoStructPointer.Pinned, GoCgoArgs.StructPointer(root.Reinterpret<sockIn4, sockAny>(), "getnameinfo", ref images, out _, out _));

        root.Value.Port = 443; // a goroutine's write while C blocks
        images.CopyBack();

        Assert.AreEqual((uint16)443, root.Value.Port, "the Go write is not erased by a buffer C never wrote");
    }

    // B5: two views of one root share ONE image, so what C writes through either is what the other sees, and the root
    // is decoded once (add_into(view(&g), view(&g), view(&g))'s shape).
    [TestMethod]
    public void TwoViewsOfOneRootShareOneImage()
    {
        ж<sockIn4> root = Root(1);
        GoCgoImages images = default;

        GoCgoArgs.StructPointer(root.Reinterpret<sockIn4, sockAny>(), "add_into", ref images, out _, out GoCgoPin first);
        GoCgoArgs.StructPointer(root.Reinterpret<sockIn4, sockAny>(), "add_into", ref images, out _, out GoCgoPin second);

        Assert.AreEqual(1, images.ImageCount, "one image for one root");

        fixed (byte* a = first)
        fixed (byte* b = second)
        {
            Assert.AreEqual(first.Address(a), second.Address(b), "both views hand C the same buffer");
            *(ushort*)((byte*)second.Address(b) + 2) = 9;
        }

        images.CopyBack();
        Assert.AreEqual((uint16)9, root.Value.Port);
    }

    // B5's mixed case: a root one parameter hands C as an image and another through the scalar door is refused by name, in
    // either order, never handed over as two copies.
    [TestMethod]
    public void ARootImagedAndHandedThroughAnotherDoorIsRefusedEitherWay()
    {
        ж<sockIn4> root = Root(1);
        ж<uint16> port = new FieldRefBox<uint16>(root, static p => ref ((ж<sockIn4>)p).Value.Port);

        GoCgoImages imageFirst = default;
        GoCgoArgs.StructPointer(root.Reinterpret<sockIn4, sockAny>(), "f", ref imageFirst, out _, out _);
        StringAssert.Contains(RefusalOf(() => GoCgoArgs.ScalarPointer(port, "f", ref imageFirst, out _, out _)), "name the same Go value");

        GoCgoImages scalarFirst = default;
        GoCgoArgs.ScalarPointer(port, "f", ref scalarFirst, out _, out _);
        StringAssert.Contains(RefusalOf(() => GoCgoArgs.StructPointer(root.Reinterpret<sockIn4, sockAny>(), "f", ref scalarFirst, out _, out _)), "name the same Go value");
    }

    // B5 across allocations: an inline Go array is its own allocation here, so a pointer into sa.Addr pins Addr's backing
    // and names neither sa's box nor its view. Handed beside an image of sa it would give C two copies of Addr's bytes,
    // and the image's copy-back would overwrite what C wrote through the pin. The image claims every backing inside its
    // root and the pin claims what it pins, so the refusal holds both ways, for the e2e shape
    // (*C.int)(unsafe.Pointer(&sa.Addr)) and for the element &sa.Addr[0].
    [TestMethod]
    public void APointerIntoAnArrayInsideAnImagedRootIsRefusedEitherWay()
    {
        ж<sockIn4> root = Root(1);
        ж<sockAny> View() => root.Reinterpret<sockIn4, sockAny>();
        ж<int> AddrAsInt() => @unsafe.Pointer.To<int>(@unsafe.Pointer.FromBox(new FieldRefBox<array<byte>>(root, static p => ref ((ж<sockIn4>)p).Value.Addr)));
        ж<byte> AddrFirst() => new ElemRefBox<byte>(root.Value.Addr, 0);

        GoCgoImages a = default;
        GoCgoArgs.StructPointer(View(), "f", ref a, out _, out _);
        StringAssert.Contains(RefusalOf(() => GoCgoArgs.ScalarPointer(AddrAsInt(), "f", ref a, out _, out _)), "name the same Go value", "image, then (*C.int)(unsafe.Pointer(&sa.Addr))");

        GoCgoImages b = default;
        Assert.AreEqual(GoCgoStructPointer.Pinned, GoCgoArgs.ScalarPointer(AddrAsInt(), "f", ref b, out _, out _), "the field view pins Addr's backing");
        StringAssert.Contains(RefusalOf(() => GoCgoArgs.StructPointer(View(), "f", ref b, out _, out _)), "name the same Go value", "(*C.int)(unsafe.Pointer(&sa.Addr)), then image");

        GoCgoImages c = default;
        GoCgoArgs.StructPointer(View(), "f", ref c, out _, out _);
        StringAssert.Contains(RefusalOf(() => GoCgoArgs.ScalarPointer(AddrFirst(), "f", ref c, out _, out _)), "name the same Go value", "image, then &sa.Addr[0]");

        GoCgoImages d = default;
        GoCgoArgs.ScalarPointer(AddrFirst(), "f", ref d, out _, out _);
        StringAssert.Contains(RefusalOf(() => GoCgoArgs.StructPointer(View(), "f", ref d, out _, out _)), "name the same Go value", "&sa.Addr[0], then image");
    }

    private struct inAddr
    {
        public array<byte> B = new(4);

        public inAddr() { }
    }

    // B4: a view that starts inside its root never hands C the root's first bytes. P3's shape,
    // (*C.struct_in_addr)(unsafe.Pointer(&sa.Addr)), is a FIELD view that aliases the field's own storage, so the plain
    // door takes the mirror of exactly the field's bytes (127.0.0.1 here) and never reaches the image. The other route to
    // an offset, unsafe.Add over the whole view's order token, is refused by golib's pointer arithmetic before any door.
    // The image arm's own offset refusal (ImageRoot) stays as the guard behind both.
    [TestMethod]
    public void AViewIntoTheMiddleOfAGoValueNeverHandsCTheRootsFirstBytes()
    {
        ж<sockIn4> root = Root(1);
        ж<inAddr> field = new FieldRefBox<array<byte>>(root, static p => ref ((ж<sockIn4>)p).Value.Addr).Reinterpret<array<byte>, inAddr>();
        GoCgoImages images = default;

        Assert.AreEqual(GoCgoStructPointer.Mirror, GoCgoArgs.StructPointer(field, "f", ref images, out _, out _), "the field view takes the mirror");
        Assert.AreEqual((byte)127, field.Value.B[0], "the mirror's source is the field, not the root's first bytes");
        Assert.AreEqual((byte)1, field.Value.B[3]);
        Assert.AreEqual(0, images.ImageCount);

        StringAssert.Contains(RefusalOf(() => @unsafe.Pointer.To<sockAny>(@unsafe.Add(@unsafe.Pointer.FromBox(root.Reinterpret<sockIn4, sockAny>()), 4))), "pointer arithmetic");
    }

    // A root holding a Go pointer has no image C may see, and stays refused by name.
    [TestMethod]
    public void ARootHoldingAGoPointerIsRefusedByName()
    {
        ж<withPointer> root = new StandardBox<withPointer>(new withPointer { Family = 2, P = new StandardBox<int32>(7) });
        GoCgoImages images = default;

        StringAssert.Contains(RefusalOf(() => GoCgoArgs.StructPointer(root.Reinterpret<withPointer, sockAny>(), "f", ref images, out _, out _)), "holds Go pointers");
    }

    // B8: a root smaller than the C struct gets a buffer of the C struct's size, its tail zeroed, so C never reads past
    // the image.
    [TestMethod]
    public void ARootSmallerThanTheCStructGetsAZeroedTail()
    {
        ж<small> root = new StandardBox<small>(new small { Family = 0xAAAA });
        GoCgoImages images = default;

        Assert.AreEqual(GoCgoStructPointer.Pinned, GoCgoArgs.StructPointer(root.Reinterpret<small, sockAny>(), "f", ref images, out _, out GoCgoPin pin));
        Assert.AreEqual(16, pin.StorageLength, "the buffer is the C struct's 16 bytes, not the root's 4");

        fixed (byte* pinned = pin)
        {
            byte* at = (byte*)pin.Address(pinned);
            Assert.AreEqual((ushort)0xAAAA, *(ushort*)at);

            for (int i = 4; i < 16; i++)
                Assert.AreEqual((byte)0, at[i], $"tail byte {i}");
        }
    }

    private struct four
    {
        public uint16 A;
        public uint16 B;
    }

    // P3's MUST on 7adc8e619c: the first view's image is sized max(root, its C struct) and pinned, so it cannot grow. A
    // second, LARGER view of the same root would get that short buffer and C would read and write past it. It is refused
    // by name. The other order shares one buffer of the larger size, as Go's one memory does.
    [TestMethod]
    public void ASecondLargerViewOfAnImagedRootIsRefusedByName()
    {
        ж<small> root = new StandardBox<small>(new small { Family = 2 });
        GoCgoImages images = default;

        GoCgoArgs.StructPointer(root.Reinterpret<small, four>(), "f", ref images, out _, out GoCgoPin first);
        Assert.AreEqual(4, first.StorageLength, "the first view's buffer is the 4-byte root");

        StringAssert.Contains(RefusalOf(() => GoCgoArgs.StructPointer(root.Reinterpret<small, sockAny>(), "f", ref images, out _, out _)), "C structs of different sizes");
    }

    [TestMethod]
    public void ALargerViewFirstThenASmallerOneShareOneImage()
    {
        ж<small> root = new StandardBox<small>(new small { Family = 2 });
        GoCgoImages images = default;

        GoCgoArgs.StructPointer(root.Reinterpret<small, sockAny>(), "f", ref images, out _, out GoCgoPin large);
        GoCgoArgs.StructPointer(root.Reinterpret<small, four>(), "f", ref images, out _, out GoCgoPin smaller);

        Assert.AreEqual(1, images.ImageCount, "one image for one root");
        Assert.AreEqual(16, large.StorageLength);
        Assert.AreEqual(16, smaller.StorageLength, "the smaller view shares the 16-byte image");

        fixed (byte* a = large)
        fixed (byte* b = smaller)
            Assert.AreEqual(large.Address(a), smaller.Address(b), "both views hand C the same buffer");
    }

    // P3's NIT 2: an image is shared only with the SAME root's image. Two distinct roots whose values share a backing (a
    // struct copy that did not clone its arrays) are refused by name, not handed one buffer at offset 0.
    [TestMethod]
    public void TwoRootsSharingABackingAreNotHandedOneImage()
    {
        sockIn4 value = new() { Family = 2 };
        ж<sockIn4> one = new StandardBox<sockIn4>(value);
        ж<sockIn4> two = new StandardBox<sockIn4>(value);
        Assert.IsTrue(ReferenceEquals(GoCgoImages.ImageKeys(one, one.Value)[1], GoCgoImages.ImageKeys(two, two.Value)[1]), "the premise: the two roots share Addr's backing");

        GoCgoImages images = default;
        GoCgoArgs.StructPointer(one.Reinterpret<sockIn4, sockAny>(), "f", ref images, out _, out _);

        StringAssert.Contains(RefusalOf(() => GoCgoArgs.StructPointer(two.Reinterpret<sockIn4, sockAny>(), "f", ref images, out _, out _)), "share an allocation");
    }

    // P3's NIT 3: copy-back decodes a FRESH value (new backing arrays) and WriteBoxSlot stores it IN PLACE for a value
    // that owns array storage (IGoValueAssign, which go2cs-gen gives every [GoValueClone] struct), so a slice of an
    // inline array taken BEFORE the call still aliases the root's array after C changed the image. The root is net's own
    // syscall.RawSockaddrInet4: a hand-written test struct has no IGoValueAssign and would be stored by replacement.
    [TestMethod]
    public void ASliceTakenBeforeTheCallStillAliasesTheRootAfterCopyBack()
    {
        Assert.IsTrue(typeof(IGoValueAssign<syscall_package.RawSockaddrInet4>).IsAssignableFrom(typeof(syscall_package.RawSockaddrInet4)), "the premise: the converted root assigns in place");

        syscall_package.RawSockaddrInet4 value = new() { Family = 2, Port = 1 };
        value.Addr[0] = 127;
        ж<syscall_package.RawSockaddrInet4> root = new StandardBox<syscall_package.RawSockaddrInet4>(value);
        slice<byte> addr = root.Value.Addr[..];
        GoCgoImages images = default;

        GoCgoArgs.StructPointer(root.Reinterpret<syscall_package.RawSockaddrInet4, sockAny>(), "f", ref images, out _, out GoCgoPin pin);

        fixed (byte* pinned = pin)
            ((byte*)pin.Address(pinned))[4] = 10; // C writes Addr[0]

        images.CopyBack();
        Assert.AreEqual((byte)10, root.Value.Addr[0], "C's write landed");

        addr[0] = 42;
        Assert.AreEqual((byte)42, root.Value.Addr[0], "the slice taken before the call still aliases the root's array");
    }

    // The plain door keeps its answers: a struct in its own box still takes the mirror through the overload.
    [TestMethod]
    public void AStructInItsOwnBoxStillTakesTheMirror()
    {
        ж<sockAny> own = new StandardBox<sockAny>(new sockAny { Family = 2 });
        GoCgoImages images = default;

        Assert.AreEqual(GoCgoStructPointer.Mirror, GoCgoArgs.StructPointer(own, "f", ref images, out _, out _));
        Assert.AreEqual(0, images.ImageCount);
    }
}
