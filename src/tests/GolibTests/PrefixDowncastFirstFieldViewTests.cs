// PrefixDowncastFirstFieldViewTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Go's PREFIX-DOWNCAST over a FIRST-FIELD VIEW: <c>(*entry[T])(unsafe.Pointer(n))</c> where <c>n</c> is a
/// <c>*node[T]</c> that was made as <c>&amp;e.node</c>. unique's canonMap is the shape (canonmap.go's
/// <c>node.entry()</c> and <c>node.indirect()</c>): every node pointer it stores is the first field of an
/// entry or an indirect, and it casts back to the larger type to read the rest.
/// </summary>
/// <remarks>
/// The pair can never alias (the destination does not fit in the header), so the reinterpret fell to the
/// address route and handed out a box over the header's address, whose reads past the header are not the
/// entry's fields: canonMap's Load and LoadOrStore crashed with nil dereferences. In Go the larger object
/// really is behind the pointer, and here it is too: the view's SOURCE is the parent box. Criteria: the
/// downcast answers that parent box itself, so writes through either pointer are seen through the other;
/// a view nested at offset 0 inside an outer allocation answers the inner parent view; a view whose
/// offset-0 chain reaches no box of the destination type panics, naming both types, rather than reading
/// past the header; and a box that is not a view keeps the address route exactly as before.
/// </remarks>
[TestClass]
public class PrefixDowncastFirstFieldViewTests
{
    // The shapes go2cs-gen emits for canonmap.go's generic node, entry and indirect: each of entry and
    // indirect EMBEDS node as its first field (a `ʗ` backing field, its ref property and the generated
    // accessor), and Pair nests an entry at a non-zero offset of a larger allocation.
    public struct Node<T>
    {
        internal bool isEntry;
    }

    public struct Entry<T>
    {
        private Node<T> ʗNode;
        internal long key;
        internal @string name;

        [System.Diagnostics.CodeAnalysis.UnscopedRef] internal ref Node<T> Node => ref ʗNode;

        internal static ref Node<T> ᏑNode(ref Entry<T> instance) => ref instance.Node;
    }

    public struct Indirect<T>
    {
        private Node<T> ʗNode;
        internal long depth;

        [System.Diagnostics.CodeAnalysis.UnscopedRef] internal ref Node<T> Node => ref ʗNode;

        internal static ref Node<T> ᏑNode(ref Indirect<T> instance) => ref instance.Node;
    }

    public struct Pair
    {
        internal Entry<long> first;
        internal Entry<long> second;

        internal static ref Entry<long> Ꮡsecond(ref Pair instance) => ref instance.second;
    }

    [TestMethod]
    public void AFirstFieldViewDowncastsToItsParent_AndWritesAreSharedBothWays()
    {
        builtin.heap(new Entry<long> { key = 1, name = "a" }, out ж<Entry<long>> entry);
        ж<Node<long>> node = entry.of(Entry<long>.ᏑNode);

        ж<Entry<long>> back = node.Reinterpret<Node<long>, Entry<long>>();

        Assert.AreSame(entry, back, "the downcast answers the parent the view was made from");

        back.Value.key = 7;
        Assert.AreEqual(7L, entry.Value.key, "a write through the downcast is the parent's");

        node.Value.isEntry = true;
        Assert.IsTrue(back.Value.Node.isEntry, "and a write through the view is seen through the downcast");
    }

    [TestMethod]
    public void AViewAtOffsetZeroOfANestedParentAnswersThatParentView()
    {
        builtin.heap(new Pair { second = new Entry<long> { key = 2 } }, out ж<Pair> pair);
        ж<Entry<long>> second = pair.of(Pair.Ꮡsecond);
        ж<Node<long>> node = second.of(Entry<long>.ᏑNode);

        ж<Entry<long>> back = node.Reinterpret<Node<long>, Entry<long>>();

        Assert.AreSame(second, back, "the parent is the inner view, not the outer allocation");

        back.Value.key = 9;
        Assert.AreEqual(9L, pair.Value.second.key, "and its storage is the outer allocation's second entry");
    }

    [TestMethod]
    public void AViewOfAnotherParentPanics_NamingBothTypes()
    {
        builtin.heap(new Indirect<long> { depth = 3 }, out ж<Indirect<long>> indirect);
        ж<Node<long>> node = indirect.of(Indirect<long>.ᏑNode);

        PanicException panic = Assert.ThrowsException<PanicException>(() => node.Reinterpret<Node<long>, Entry<long>>(),
            "a view whose parent is not the destination type has nothing behind it to downcast to");

        StringAssert.Contains(panic.Message, nameof(Node<long>));
        StringAssert.Contains(panic.Message, nameof(Entry<long>));
        Assert.IsFalse(panic.Message.Contains('`'), "Go names, without the CLR generic arity suffix");
    }

    [TestMethod]
    public void ABoxThatIsNotAViewKeepsTheAddressRoute()
    {
        ж<Node<long>> node = new StandardBox<Node<long>>(new Node<long> { isEntry = true });

        ж<Entry<long>> derived = node.Reinterpret<Node<long>, Entry<long>>();

        Assert.IsTrue(derived.IsNative, "a standalone header keeps the address route, as before this seat");
    }
}
