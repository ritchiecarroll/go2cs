// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using rand = global::go.math.rand_package;
using static runtime_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go.math;
using static global::go.runtime_internal_test_package;
using Δruntime = runtime_package;

partial class runtime_test_package {

partial struct MyNode {
    /*embed*/ public global::go.runtime_internal_test_package.LFNode LFNode;
    internal nint data;
}

// allocMyNode allocates nodes that are stored in an lfstack
// outside the Go heap.
// We require lfstack objects to live outside the heap so that
// checkptr passes on the unsafe shenanigans used.
internal static ж<MyNode> allocMyNode(nint data) {
    var n = (ж<MyNode>)(uintptr)(runtime_internal_test_package.PersistentAlloc(/* unsafe.Sizeof(MyNode{}) */ (uintptr)24, runtime_internal_test_package.TagAlign));
    runtime_internal_test_package.LFNodeValidate(n.ᏑLFNode);
    n.data = data;
    return n;
}

internal static ж<global::go.runtime_internal_test_package.LFNode> fromMyNode(ж<MyNode> node) {
    return node.Reinterpret<MyNode, global::go.runtime_internal_test_package.LFNode>();
}

internal static ж<MyNode> toMyNode(ж<global::go.runtime_internal_test_package.LFNode> node) {
    return node.Reinterpret<global::go.runtime_internal_test_package.LFNode, MyNode>();
}

internal static any global;

public static void TestLFStack(ж<testing.T> t) {
    var stack = @new<uint64>();
    global = stack.OrTypedNil(); // force heap allocation

    // Check the stack is initially empty.
    if (runtime_internal_test_package.LFStackPop(stack) != nil) {
        t.Fatalf("stack is not empty");
    }

    // Push one element.
    var node = allocMyNode(42);
    runtime_internal_test_package.LFStackPush(stack, fromMyNode(node));

    // Push another.
    node = allocMyNode(43);
    runtime_internal_test_package.LFStackPush(stack, fromMyNode(node));

    // Pop one element.
    node = toMyNode(runtime_internal_test_package.LFStackPop(stack));
    if (node == nil) {
        t.Fatalf("stack is empty");
    }
    if (node.data != 43) {
        t.Fatalf("no lifo");
    }

    // Pop another.
    node = toMyNode(runtime_internal_test_package.LFStackPop(stack));
    if (node == nil) {
        t.Fatalf("stack is empty");
    }
    if (node.data != 42) {
        t.Fatalf("no lifo");
    }

    // Check the stack is empty again.
    if (runtime_internal_test_package.LFStackPop(stack) != nil) {
        t.Fatalf("stack is not empty");
    }
    if (stack.Value != 0) {
        t.Fatalf("stack is not empty");
    }
}

public static partial void TestLFStackStress(ж<testing.T> t) {
    const nint K = 100;
    nint P = 4 * GOMAXPROCS(-1);
    nint N = 100000;
    if (testing.Short()) {
        N /= 10;
    }
    // Create 2 stacks.
    ref var stacks = ref heap<array<ж<uint64>>>(new ж<uint64>[]{@new<uint64>(), @new<uint64>()}.array(), out var Ꮡstacks);
    // Push K elements randomly onto the stacks.
    nint sum = 0;
    for (nint i = 0; i < K; i++) {
        sum += i;
        var node = allocMyNode(i);
        runtime_internal_test_package.LFStackPush(stacks[i % 2], fromMyNode(node));
    }
    var c = new channel<bool>(P);
    for (nint p = 0; p < P; p++) {
        var cʗ1 = c;
        var stacksʗ1 = stacks;
        goǃ(() => {
            var r = rand.New(rand.NewSource(rand.Int63()));
            // Pop a node from a random stack, then push it onto a random stack.
            for (nint i = 0; i < N; i++) {
                var node = toMyNode(runtime_internal_test_package.LFStackPop(stacksʗ1[r.Intn(2)]));
                if (node != nil) {
                    runtime_internal_test_package.LFStackPush(stacksʗ1[r.Intn(2)], fromMyNode(node));
                }
            }
            cʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < P; i++) {
        ᐸꟷ(c);
    }
    // Pop all elements from both stacks, and verify that nothing lost.
    nint sum2 = 0;
    nint cnt = 0;
    for (nint i = 0; i < 2; i++) {
        for (;;) {
            var node = toMyNode(runtime_internal_test_package.LFStackPop(stacks[i]));
            if (node == nil) {
                break;
            }
            cnt++;
            sum2 += node.data;
            node.Next = 0;
        }
    }
    if (cnt != K) {
        t.Fatalf("Wrong number of nodes %d/%d", cnt, (nint)(K));
    }
    if (sum2 != sum) {
        t.Fatalf("Wrong sum %d/%d", sum2, sum);
    }
}

} // end runtime_test_package
