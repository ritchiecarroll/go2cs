// Copyright 2016 Google Inc.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.github.com.google;

using sync = sync_package;

partial class uuid_package {

internal static ж<sync.Mutex> ᏑnodeMu = new StandardBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex nodeMu => ref ᏑnodeMu.Value;
internal static @string ifname;
internal static array<byte> nodeID = new(6);
internal static array<byte> zeroID = new(6);

public static @string NodeInterface() {
    GoFrame ᒐ = default;
    try {
        defer(ᏑnodeMu.Unlock, ref ᒐ);
        ᏑnodeMu.Lock();
        return ifname;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

public static bool SetNodeInterface(@string name) {
    GoFrame ᒐ = default;
    try {
        defer(ᏑnodeMu.Unlock, ref ᒐ);
        ᏑnodeMu.Lock();
        return setNodeInterface(name);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string randomˢ = "random"u8;

internal static bool setNodeInterface(@string name) {
    var (iname, addr) = getHardwareInterface(name);
    if (iname != ""u8 && addr != default!) {
        ifname = iname;
        copy(nodeID[..], addr);
        return true;
    }
    if (name == ""u8) {
        ifname = randomˢ;
        randomBits(nodeID[..]);
        return true;
    }
    return false;
}

public static slice<byte> NodeID() {
    GoFrame ᒐ = default;
    try {
        defer(ᏑnodeMu.Unlock, ref ᒐ);
        ᏑnodeMu.Lock();
        if (nodeID == zeroID) {
            setNodeInterface(""u8);
        }
        var nid = nodeID.Clone();
        return nid[..];
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string userˢ = "user"u8;

public static bool SetNodeID(slice<byte> id) {
    GoFrame ᒐ = default;
    try {
        if (len(id) < 6) {
            return false;
        }
        defer(ᏑnodeMu.Unlock, ref ᒐ);
        ᏑnodeMu.Lock();
        copy(nodeID[..], id);
        ifname = userˢ;
        return true;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

public static slice<byte> NodeID(this UUID uuid) {
    uuid = uuid.Clone();

    array<byte> node = new(6);
    copy(node[..], uuid[10..]);
    return node[..];
}

} // end uuid_package
