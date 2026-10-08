// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bufio = bufio_package;
using fmt = fmt_package;
using net = net_package;
using time = time_package;

partial class http_package {

// A ResponseController is used by an HTTP handler to control the response.
//
// A ResponseController may not be used after the [Handler.ServeHTTP] method has returned.
partial struct ResponseController {
    internal ResponseWriter rw;
}

// NewResponseController creates a [ResponseController] for a request.
//
// The ResponseWriter should be the original value passed to the [Handler.ServeHTTP] method,
// or have an Unwrap method returning the original ResponseWriter.
//
// If the ResponseWriter implements any of the following methods, the ResponseController
// will call them as appropriate:
//
//	Flush()
//	FlushError() error // alternative Flush returning an error
//	Hijack() (net.Conn, *bufio.ReadWriter, error)
//	SetReadDeadline(deadline time.Time) error
//	SetWriteDeadline(deadline time.Time) error
//	EnableFullDuplex() error
//
// If the ResponseWriter does not support a method, ResponseController returns
// an error matching [ErrNotSupported].
public static ж<ResponseController> NewResponseController(ResponseWriter rw) {
    return Ꮡ(new ResponseController(rw));
}

partial interface rwUnwrapper {
    ResponseWriter Unwrap();
}

internal partial interface Flush_type /*dyn*/ {
    error FlushError();
}

// Flush flushes buffered data to the client.
public static error Flush(this ref ResponseController c) {
    var rw = c.rw;
    while (ᐧ) {
        switch (rw.type()) {
        case {} Δt when Δt._<Flush_type>(out var t): {
            return t.FlushError();
        }
        case {} Δt when Δt._<Flusher>(out var t): {
            t.Flush();
            return default!;
        }
        case {} Δt when Δt._<rwUnwrapper>(out var t): {
            rw = t.Unwrap();
            break;
        }
        default: {
            var t = rw;
            return errNotSupported();
        }}
    }
}

// Hijack lets the caller take over the connection.
// See the Hijacker interface for details.
public static (net.Conn, ж<bufio.ReadWriter>, error) Hijack(this ref ResponseController c) {
    var rw = c.rw;
    while (ᐧ) {
        switch (rw.type()) {
        case {} Δt when Δt._<Hijacker>(out var t): {
            return t.Hijack();
        }
        case {} Δt when Δt._<rwUnwrapper>(out var t): {
            rw = t.Unwrap();
            break;
        }
        default: {
            var t = rw;
            return (default!, default!, errNotSupported());
        }}
    }
}

internal partial interface SetReadDeadline_type /*dyn*/ {
    error SetReadDeadline(time.Time _);
}

// SetReadDeadline sets the deadline for reading the entire request, including the body.
// Reads from the request body after the deadline has been exceeded will return an error.
// A zero value means no deadline.
//
// Setting the read deadline after it has been exceeded will not extend it.
public static error SetReadDeadline(this ref ResponseController c, time.Time deadline) {
    var rw = c.rw;
    while (ᐧ) {
        switch (rw.type()) {
        case {} Δt when Δt._<SetReadDeadline_type>(out var t): {
            return t.SetReadDeadline(deadline);
        }
        case {} Δt when Δt._<rwUnwrapper>(out var t): {
            rw = t.Unwrap();
            break;
        }
        default: {
            var t = rw;
            return errNotSupported();
        }}
    }
}

internal partial interface SetWriteDeadline_type /*dyn*/ {
    error SetWriteDeadline(time.Time _);
}

// SetWriteDeadline sets the deadline for writing the response.
// Writes to the response body after the deadline has been exceeded will not block,
// but may succeed if the data has been buffered.
// A zero value means no deadline.
//
// Setting the write deadline after it has been exceeded will not extend it.
public static error SetWriteDeadline(this ref ResponseController c, time.Time deadline) {
    var rw = c.rw;
    while (ᐧ) {
        switch (rw.type()) {
        case {} Δt when Δt._<SetWriteDeadline_type>(out var t): {
            return t.SetWriteDeadline(deadline);
        }
        case {} Δt when Δt._<rwUnwrapper>(out var t): {
            rw = t.Unwrap();
            break;
        }
        default: {
            var t = rw;
            return errNotSupported();
        }}
    }
}

internal partial interface EnableFullDuplex_type /*dyn*/ {
    error EnableFullDuplex();
}

// EnableFullDuplex indicates that the request handler will interleave reads from [Request.Body]
// with writes to the [ResponseWriter].
//
// For HTTP/1 requests, the Go HTTP server by default consumes any unread portion of
// the request body before beginning to write the response, preventing handlers from
// concurrently reading from the request and writing the response.
// Calling EnableFullDuplex disables this behavior and permits handlers to continue to read
// from the request while concurrently writing the response.
//
// For HTTP/2 requests, the Go HTTP server always permits concurrent reads and responses.
public static error EnableFullDuplex(this ref ResponseController c) {
    var rw = c.rw;
    while (ᐧ) {
        switch (rw.type()) {
        case {} Δt when Δt._<EnableFullDuplex_type>(out var t): {
            return t.EnableFullDuplex();
        }
        case {} Δt when Δt._<rwUnwrapper>(out var t): {
            rw = t.Unwrap();
            break;
        }
        default: {
            var t = rw;
            return errNotSupported();
        }}
    }
}

// errNotSupported returns an error that Is ErrNotSupported,
// but is not == to it.
internal static error errNotSupported() {
    return fmt.Errorf("%w"u8, ErrNotSupported.OrTypedNil());
}

} // end http_package
