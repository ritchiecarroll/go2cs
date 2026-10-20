// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using runtime = runtime_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using @unsafe = unsafe_package;
using weak = weak_package;
using @internal;
using go.sync;

partial class unique_package {

// canonMap is a map of T -> *T. The map controls the creation
// of a canonical *T, and elements of the map are automatically
// deleted when the canonical *T is no longer referenced.
partial struct canonMap<T> {
    internal atomic.Pointer<Δindirect<T>> root;
    internal Func<@unsafe.Pointer, uintptr, uintptr> hash;
    internal uintptr seed;
}

internal static ж<canonMap<T>> newCanonMap<T>() {
    var cm = @new<canonMap<T>>();
    cm.of(canonMap<T>.Ꮡroot).Store(newIndirectNode<T>(nil));

    map<T, EmptyStruct> m = default!;
    var mapType = abi.TypeOf(m).MapType();
    cm.Value.hash = mapType.Value.Hasher;
    cm.Value.seed = (uintptr)runtime_rand();
    return cm;
}

internal static ж<T> Load<T>(this ж<canonMap<T>> Ꮡm, T keyʗp) {
    ref var m = ref Ꮡm.DerefOrNull();
    ref var key = ref heap(keyʗp, out var Ꮡkey);

    var hash = m.hash((uintptr)abi.NoEscape(@unsafe.Pointer.FromPinnedBox(Ꮡkey)), m.seed);
    System.GC.KeepAlive(Ꮡkey);

    var i = Ꮡm.of(canonMap<T>.Ꮡroot).Load();
    nint hashShift = 8 * goarch.PtrSize;
    while (hashShift != 0) {
        hashShift -= nChildrenLog2;

        var n = i.at(unique_package.Δindirect<T>.Ꮡchildren, (ulong)((uintptr)((hash.Rsh((int64)(hashShift))) & (uintptr)nChildrenMask))).Load();
        if (n == nil) {
            return default!;
        }
        if ((~n).isEntry) {
            var (v, _) = n.entry().lookup(key);
            return v;
        }
        i = n.indirect();
    }
    throw panic("unique.canonMap: ran out of hash bits while iterating");
}

internal static ж<T> LoadOrStore<T>(this ж<canonMap<T>> Ꮡm, T keyʗp) {
    GoFrame ᒐ = default;
    try {
        ref var m = ref Ꮡm.DerefOrNull();
        ref var key = ref heap(keyʗp, out var Ꮡkey);

        var hash = m.hash((uintptr)abi.NoEscape(@unsafe.Pointer.FromPinnedBox(Ꮡkey)), m.seed);
        System.GC.KeepAlive(Ꮡkey);

        ж<Δindirect<T>> i = default!;
        nuint hashShift = default!;
        ж<atomic.Pointer<node<T>>> slot = default!;
        ж<node<T>> n = default!;
        while (ᐧ) {
            // Find the key or a candidate location for insertion.
            i = Ꮡm.of(canonMap<T>.Ꮡroot).Load();
            hashShift = 8 * goarch.PtrSize;
            var haveInsertPoint = false;
            while (hashShift != 0) {
                hashShift -= nChildrenLog2;

                slot = i.at(unique_package.Δindirect<T>.Ꮡchildren, (ulong)((uintptr)((hash.Rsh(hashShift)) & (uintptr)nChildrenMask)));
                n = slot.Load();
                if (n == nil) {
                    // We found a nil slot which is a candidate for insertion.
                    haveInsertPoint = true;
                    break;
                }
                if ((~n).isEntry) {
                    // We found an existing entry, which is as far as we can go.
                    // If it stays this way, we'll have to replace it with an
                    // indirect node.
                    {
                        var (v, _) = n.entry().lookup(key); if (v != nil) {
                            return v;
                        }
                    }
                    haveInsertPoint = true;
                    break;
                }
                i = n.indirect();
            }
            if (!haveInsertPoint) {
                throw panic("unique.canonMap: ran out of hash bits while iterating");
            }

            // Grab the lock and double-check what we saw.
            i.of(unique_package.Δindirect<T>.Ꮡmu).Lock();
            n = slot.Load();
            if ((n == nil || (~n).isEntry) && !i.of(unique_package.Δindirect<T>.Ꮡdead).Load()) {
                // What we saw is still true, so we can continue with the insert.
                break;
            }
            // We have to start over.
            i.of(unique_package.Δindirect<T>.Ꮡmu).Unlock();
        }
        // N.B. This lock is held from when we broke out of the outer loop above.
        // We specifically break this out so that we can use defer here safely.
        // One option is to break this out into a new function instead, but
        // there's so much local iteration state used below that this turns out
        // to be cleaner.
        var iʗ1 = i;
        defer(iʗ1.of(unique_package.Δindirect<T>.Ꮡmu).Unlock, ref ᒐ);

        ж<Δentry<T>> oldEntry = default!;
        if (n != nil) {
            oldEntry = n.entry();
            {
                var (v, _) = oldEntry.lookup(key); if (v != nil) {
                    // Easy case: by loading again, it turns out exactly what we wanted is here!
                    return v;
                }
            }
        }
        ref var wp = ref heap<weak.Pointer<T>>(out var Ꮡwp);
        (var newEntry, var canon, wp) = newEntryNode(key, hash);
        // Prune dead pointers. This is to avoid O(n) lookups when we store the exact same
        // value in the set but the cleanup hasn't run yet because it got delayed for some
        // reason.
        oldEntry = oldEntry.prune();
        if (oldEntry == nil){
            // Easy case: create a new entry and store it.
            slot.Store(newEntry.of(unique_package.Δentry<T>.Ꮡnode));
        } else {
            // We possibly need to expand the entry already there into one or more new nodes.
            //
            // Publish the node last, which will make both oldEntry and newEntry visible. We
            // don't want readers to be able to observe that oldEntry isn't in the tree.
            slot.Store(m.expand(oldEntry, newEntry, hash, hashShift, i));
        }
        var wpʗ1 = wp;
        runtime.AddCleanup(canon, (EmptyStruct _Δp0) => {
            Ꮡm.cleanup(hash, wpʗ1);
        }, new EmptyStruct());
        return canon;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// expand takes oldEntry and newEntry whose hashes conflict from bit 64 down to hashShift and
// produces a subtree of indirect nodes to hold the two new entries. newHash is the hash of
// the value in the new entry.
internal static ж<node<T>> expand<T>(this ref canonMap<T> m, ж<Δentry<T>> ᏑoldEntry, ж<Δentry<T>> ᏑnewEntry, uintptr newHash, nuint hashShift, ж<Δindirect<T>> Ꮡparent) {
    ref var oldEntry = ref ᏑoldEntry.DerefOrNull();

    // Check for a hash collision.
    var oldHash = oldEntry.hash;
    if (oldHash == newHash) {
        // Store the old entry in the new entry's overflow list, then store
        // the new entry.
        ᏑnewEntry.of(unique_package.Δentry<T>.Ꮡoverflow).Store(ᏑoldEntry);
        return ᏑnewEntry.of(unique_package.Δentry<T>.Ꮡnode);
    }
    // We have to add an indirect node. Worse still, we may need to add more than one.
    var newIndirect = newIndirectNode(Ꮡparent);
    var top = newIndirect;
    while (ᐧ) {
        if (hashShift == 0) {
            throw panic("unique.canonMap: ran out of hash bits while inserting");
        }
        hashShift -= nChildrenLog2; // hashShift is for the level parent is at. We need to go deeper.
        var oi = (uintptr)((oldHash.Rsh(hashShift)) & (uintptr)nChildrenMask);
        var ni = (uintptr)((newHash.Rsh(hashShift)) & (uintptr)nChildrenMask);
        if (oi != ni) {
            newIndirect.at(unique_package.Δindirect<T>.Ꮡchildren, (ulong)(oi)).Store(ᏑoldEntry.of(unique_package.Δentry<T>.Ꮡnode));
            newIndirect.at(unique_package.Δindirect<T>.Ꮡchildren, (ulong)(ni)).Store(ᏑnewEntry.of(unique_package.Δentry<T>.Ꮡnode));
            break;
        }
        var nextIndirect = newIndirectNode(newIndirect);
        newIndirect.at(unique_package.Δindirect<T>.Ꮡchildren, (ulong)(oi)).Store(nextIndirect.of(unique_package.Δindirect<T>.Ꮡnode));
        newIndirect = nextIndirect;
    }
    return top.of(unique_package.Δindirect<T>.Ꮡnode);
}

// cleanup deletes the entry corresponding to wp in the canon map, if it's
// still in the map. wp must have a Value method that returns nil by the
// time this function is called. hash must be the hash of the value that
// wp once pointed to (that is, the hash of *wp.Value()).
internal static void cleanup<T>(this ж<canonMap<T>> Ꮡm, uintptr hash, weak.Pointer<T> wp) {
    ж<Δindirect<T>> i = default!;
    nuint hashShift = default!;
    ж<atomic.Pointer<node<T>>> slot = default!;
    ж<node<T>> n = default!;
    while (ᐧ) {
        // Find wp in the map by following hash.
        i = Ꮡm.of(canonMap<T>.Ꮡroot).Load();
        hashShift = 8 * goarch.PtrSize;
        var haveEntry = false;
        while (hashShift != 0) {
            hashShift -= nChildrenLog2;

            slot = i.at(unique_package.Δindirect<T>.Ꮡchildren, (ulong)((uintptr)((hash.Rsh(hashShift)) & (uintptr)nChildrenMask)));
            n = slot.Load();
            if (n == nil) {
                // We found a nil slot, already deleted.
                return;
            }
            if ((~n).isEntry) {
                if (!n.entry().hasWeakPointer(wp)) {
                    // The weak pointer was already pruned.
                    return;
                }
                haveEntry = true;
                break;
            }
            i = n.indirect();
        }
        if (!haveEntry) {
            throw panic("unique.canonMap: ran out of hash bits while iterating");
        }

        // Grab the lock and double-check what we saw.
        i.of(unique_package.Δindirect<T>.Ꮡmu).Lock();
        n = slot.Load();
        if (n != nil && (~n).isEntry) {
            // Prune the entry node without thinking too hard. If we do
            // somebody else's work, such as someone trying to insert an
            // entry with the same hash (probably the same value) then
            // great, they'll back out without taking the lock.
            var newEntry = n.entry().prune();
            if (newEntry == nil){
                slot.Store(nil);
            } else {
                slot.Store(newEntry.of(unique_package.Δentry<T>.Ꮡnode));
            }

            // Delete interior nodes that are empty, up the tree.
            //
            // We'll hand-over-hand lock our way up the tree as we do this,
            // since we need to delete each empty node's link in its parent,
            // which requires the parents' lock.
            while ((~i).parent != nil && i.empty()) {
                if (hashShift == (nuint)(8 * goarch.PtrSize)) {
                    throw panic("unique.canonMap: ran out of hash bits while iterating");
                }
                hashShift += nChildrenLog2;

                // Delete the current node in the parent.
                var parent = i.Value.parent;
                parent.of(unique_package.Δindirect<T>.Ꮡmu).Lock();
                i.of(unique_package.Δindirect<T>.Ꮡdead).Store(true); // Could be done outside of parent's lock.
                parent.at(unique_package.Δindirect<T>.Ꮡchildren, (ulong)((uintptr)((hash.Rsh(hashShift)) & (uintptr)nChildrenMask))).Store(nil);
                i.of(unique_package.Δindirect<T>.Ꮡmu).Unlock();
                i = parent;
            }
            i.of(unique_package.Δindirect<T>.Ꮡmu).Unlock();
            return;
        }
        // We have to start over.
        i.of(unique_package.Δindirect<T>.Ꮡmu).Unlock();
    }
}

// node is the header for a node. It's polymorphic and
// is actually either an entry or an indirect.
partial struct node<T> {
    internal bool isEntry;
}

internal static ж<Δentry<T>> entry<T>(this ж<node<T>> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (!n.isEntry) {
        throw panic("called entry on non-entry node");
    }
    return Ꮡn.Reinterpret<node<T>, Δentry<T>>();
}

internal static ж<Δindirect<T>> indirect<T>(this ж<node<T>> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (n.isEntry) {
        throw panic("called indirect on entry node");
    }
    return Ꮡn.Reinterpret<node<T>, Δindirect<T>>();
}

// 16 children. This seems to be the sweet spot for
// load performance: any smaller and we lose out on
// 50% or more in CPU performance. Any larger and the
// returns are minuscule (~1% improvement for 32 children).
internal static UntypedInt nChildrenLog2 => 4;
internal static UntypedInt nChildren => /* 1 << nChildrenLog2 */ 16;
internal static UntypedInt nChildrenMask => /* nChildren - 1 */ 15;

// indirect is an internal node in the hash-trie.
partial struct Δindirect<T> {
    /*embed*/ internal node<T> node;
    internal atomic.Bool dead;
    internal ж<Δindirect<T>> parent;
    internal Δsync.Mutex mu; // Protects mutation to children and any children that are entry nodes.
    internal array<atomic.Pointer<node<T>>> children = new(nChildren);
}

internal static ж<Δindirect<T>> newIndirectNode<T>(ж<Δindirect<T>> Ꮡparent) {
    return Ꮡ(new Δindirect<T>(node: new node<T>(isEntry: false), parent: Ꮡparent));
}

internal static bool empty<T>(this ref Δindirect<T> i) {
    foreach (var (j, _) in i.children) {
        if (Ꮡ(i.children, j).Load() != nil) {
            return false;
        }
    }
    return true;
}

// entry is a leaf node in the hash-trie.
partial struct Δentry<T> {
    /*embed*/ internal node<T> node;
    internal atomic.Pointer<Δentry<T>> overflow; // Overflow for hash collisions.
    internal weak.Pointer<T> key;
    internal uintptr hash;
}

internal static (ж<Δentry<T>>, ж<T>, weak.Pointer<T>) newEntryNode<T>(T key, uintptr hash) {
    var k = @new<T>();
    k.ValueSlot = key;
    ref var wp = ref heap<weak.Pointer<T>>(out var Ꮡwp);
    wp = weak.Make<T>(k);
    return (Ꮡ(new Δentry<T>(
        node: new node<T>(isEntry: true),
        key: wp,
        hash: hash
    )), k, wp);
}

// lookup finds the entry in the overflow chain that has the provided key.
//
// Returns the key's canonical pointer and the weak pointer for that canonical pointer.
internal static (ж<T>, weak.Pointer<T>) lookup<T>(this ж<Δentry<T>> Ꮡe, T key) {
    ref var e = ref Ꮡe.DerefOrNull();

    while (Ꮡe != nil) {
        var s = e.key.Value();
        if (s != nil && AreEqual(s.ValueSlot, key)) {
            return (s, e.key);
        }
        Ꮡe = Ꮡe.of(unique_package.Δentry<T>.Ꮡoverflow).Load(); e = ref Ꮡe.DerefOrNull();
    }
    return (default!, new weak.Pointer<T>(nil));
}

// hasWeakPointer returns true if the provided weak pointer can be found in the overflow chain.
internal static bool hasWeakPointer<T>(this ж<Δentry<T>> Ꮡe, weak.Pointer<T> wp) {
    ref var e = ref Ꮡe.DerefOrNull();

    while (Ꮡe != nil) {
        if (e.key == wp) {
            return true;
        }
        Ꮡe = Ꮡe.of(unique_package.Δentry<T>.Ꮡoverflow).Load(); e = ref Ꮡe.DerefOrNull();
    }
    return false;
}

// prune removes all entries in the overflow chain whose keys are nil.
//
// The caller must hold the lock on e's parent node.
internal static ж<Δentry<T>> prune<T>(this ж<Δentry<T>> Ꮡe) {
    ref var e = ref Ꮡe.DerefOrNull();

    // Prune the head of the list.
    while (Ꮡe != nil) {
        if (e.key.Value() != nil) {
            break;
        }
        Ꮡe = Ꮡe.of(unique_package.Δentry<T>.Ꮡoverflow).Load(); e = ref Ꮡe.DerefOrNull();
    }
    if (Ꮡe == nil) {
        return default!;
    }

    // Prune individual nodes in the list.
    var newHead = Ꮡe;
    var i = Ꮡe.of(unique_package.Δentry<T>.Ꮡoverflow);
    Ꮡe = i.Load(); e = ref Ꮡe.DerefOrNull();
    while (Ꮡe != nil) {
        if (e.key.Value() != nil){
            i = Ꮡe.of(unique_package.Δentry<T>.Ꮡoverflow);
        } else {
            i.Store(Ꮡe.of(unique_package.Δentry<T>.Ꮡoverflow).Load());
        }
        Ꮡe = Ꮡe.of(unique_package.Δentry<T>.Ꮡoverflow).Load(); e = ref Ꮡe.DerefOrNull();
    }
    return newHead;
}

// Pull in runtime.rand so that we don't need to take a dependency
// on math/rand/v2.
//
//go:linkname runtime_rand runtime.rand
internal static partial uint64 runtime_rand();

} // end unique_package
