// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
namespace go.@internal;

partial class routebsd_package {

// A Message represents a routing message.
partial interface Message {
    void message();
}

// parseRIB parses b as a routing information base and returns a list
// of routing messages.
internal static (slice<Message>, error) parseRIB(slice<byte> b) {
    var msgsᴷ = new stackAppend<Message>(moves: true);

    slice<Message> msgs = default;
    nint nmsgs = 0;
    nint nskips = 0;
    while (len(b) > 4) {
        nmsgs++;
        nint l = (nint)nativeEndian.Uint16(b[..2]);
        if (l == 0) {
            return (default, errInvalidMessage);
        }
        if (len(b) < l) {
            return (default, errMessageTooShort);
        }
        if (b[2] != rtmVersion) {
            b = b.slice(l);
            continue;
        }
        {
            var (w, ok) = wireFormats[(nint)b[3], ꟷ]; if (!ok) {
                nskips++;
            } else {
                var (m, err) = w.parse(b.slice(0, l));
                if (err != nil) {
                    return (default, err);
                }
                if (m == nil) {
                    nskips++;
                } else {
                    msgs = append(msgsᴷ, msgs, m);
                }
            }
        }
        b = b.slice(l);
    }

    // We failed to parse any of the messages - version mismatch?
    if (nmsgs != len(msgs) + nskips) {
        return (default, errMessageMismatch);
    }
    msgs = move2heap(msgsᴷ, msgs);
    return (msgs, default);
}

} // end routebsd_package
