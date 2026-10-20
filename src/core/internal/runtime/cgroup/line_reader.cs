// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using bytealg = go.@internal.bytealg_package;
using go.@internal;

partial class cgroup_package {

// stringError is a trival implementation of error, equivalent to errors.New,
// which cannot be imported from a runtime package.
partial struct stringError /*@string*/;

internal static @string Error(this stringError e) {
    return (@string)e;
}

// All errors are explicit converted to type error in global initialization to
// ensure that the linker allocates a static interface value. This is necessary
// because these errors may be used before the allocator is available.

// The entire line did not fit into the scratch buffer.
internal static error errIncompleteLine = ((stringError)(@string)"incomplete line"u8);

// A system call failed.
internal static error errSyscallFailed = ((stringError)(@string)"syscall failed"u8);

// Reached EOF.
internal static error errEOF = ((stringError)(@string)"end of file"u8);

// lineReader reads line-by-line using only a single fixed scratch buffer.
//
// When a single line is too long for the scratch buffer, the remainder of the
// line will be skipped.
partial struct lineReader {
    internal Func<nint, slice<byte>, (nint, uintptr)> read;
    internal nint fd;
    internal slice<byte> scratch;
    internal nint n; // bytes of scratch in use.
    internal nint newline; // index of the first newline in scratch.
    internal bool eof; // read reached EOF.
}

// newLineReader returns a lineReader which reads lines from fd.
//
// fd is the file descriptor to read from.
//
// scratch is the scratch buffer to read into. Note that len(scratch) is the
// longest line that can be read. Lines longer than len(scratch) will have the
// remainder of the line skipped. See next for more details.
//
// read is the function used to read more bytes from fd. This is usually
// internal/runtime/syscall/linux.Read. Note that this follows syscall semantics (not
// io.Reader), so EOF is indicated with n=0, errno=0.
internal static ж<lineReader> newLineReader(nint fd, slice<byte> scratch, Func<nint, slice<byte>, (nint, uintptr)> read) {
    return Ꮡ(new lineReader(
        read: read,
        fd: fd,
        scratch: scratch,
        n: 0,
        newline: -1
    ));
}

// next advances to the next line.
//
// May return errIncompleteLine if the scratch buffer is too small to hold the
// entire line, in which case [r.line] will return the beginning of the line. A
// subsequent call to next will skip the remainder of the incomplete line.
//
// N.B. this behavior is important for /proc/self/mountinfo. Some lines
// (mounts), such as overlayfs, may be extremely long due to long super-block
// options, but we don't care about those. The mount type will appear early in
// the line.
//
// Returns errEOF when there are no more lines.
internal static error next(this ref lineReader r) {
    // Three cases:
    //
    // 1. First call, no data read.
    // 2. Previous call had a complete line. Drop it and look for the end
    //    of the next line.
    // 3. Previous call had an incomplete line. Find the end of that line
    //    (start of the next line), and the end of the next line.
    var prevComplete = r.newline >= 0;
    var firstCall = r.n == 0;

    while (ᐧ) {
        if (prevComplete) {
            // Drop the previous line.
            copy(r.scratch, r.scratch.slice(r.newline + 1, r.n));
            r.n -= r.newline + 1;

            r.newline = bytealg.IndexByte(r.scratch.slice(0, r.n), (rune)'\n');
            if (r.newline >= 0) {
                // We have another line already in scratch. Done.
                return default!;
            }
        }

        // No newline available.

        if (!prevComplete) {
            // If the previous line was incomplete, we are
            // searching for the end of that line and have no need
            // for any buffered data.
            r.n = 0;
        }

        var (n, errno) = r.read(r.fd, r.scratch.slice(r.n, len(r.scratch)));
        if (errno != 0) {
            return errSyscallFailed;
        }
        r.n += n;

        if (r.n == 0) {
            // Nothing left.
            //
            // N.B. we can't immediately return EOF when read
            // returns 0 as we may still need to return an
            // incomplete line.
            return errEOF;
        }

        r.newline = bytealg.IndexByte(r.scratch.slice(0, r.n), (rune)'\n');
        if (prevComplete || firstCall){
            // Already have the start of the line, just need to find the end.
            if (r.newline < 0) {
                // We filled the entire buffer or hit EOF, but
                // still no newline.
                return errIncompleteLine;
            }

            // Found the end of the line. Done.
            return default!;
        } else {
            // Don't have the start of the line. We are currently
            // looking for the end of the previous line.
            if (r.newline < 0) {
                // Not there yet.
                if (n == 0) {
                    // No more to read.
                    return errEOF;
                }
                continue;
            }

            // Found the end of the previous line. The next
            // iteration will drop the remainder of the previous
            // line and look for the next line.
            prevComplete = true;
        }
    }
}

// line returns a view of the current line, excluding the trailing newline.
//
// If [r.next] returned errIncompleteLine, then this returns only the beginning
// of the line.
//
// Preconditions: [r.next] is called prior to the first call to line.
//
// Postconditions: The caller must not keep a reference to the returned slice.
internal static slice<byte> line(this ref lineReader r) {
    if (r.newline < 0) {
        // Incomplete line
        return r.scratch.slice(0, r.n);
    }
    // Complete line.
    return r.scratch.slice(0, r.newline);
}

} // end cgroup_package
