// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements the host side of CGI (being the webserver
// parent process).

// Package cgi implements CGI (Common Gateway Interface) as specified
// in RFC 3875.
//
// Note that using CGI means starting a new process to handle each
// request, which is typically less efficient than using a
// long-running server. This package is intended primarily for
// compatibility with existing systems.
namespace go.net.http;

using bufio = bufio_package;
using fmt = fmt_package;
using io = io_package;
using log = log_package;
using net = net_package;
using http = go.net.http_package;
using textproto = go.net.textproto_package;
using os = os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using regexp = regexp_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using httpguts = vendor.golang.org.x.net.http.httpguts_package;
using go.net;
using go.os;
using go.path;
using url = go.net.url_package;
using vendor.golang.org.x.net.http;
using ꓸꓸꓸany = Span<any>;

partial class cgi_package {

internal static ж<regexp.Regexp> trailingPort = regexp.MustCompile(@":([0-9]+)$"u8);

internal static slice<@string> osDefaultInheritEnv = ((Func<slice<@string>>)(() => {
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8) {
        return new @string[]{"DYLD_LIBRARY_PATH"u8}.slice();
    }
    if (exprᴛ1 == "android"u8 || exprᴛ1 == "linux"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8) {
        return new @string[]{"LD_LIBRARY_PATH"u8}.slice();
    }
    if (exprᴛ1 == "hpux"u8) {
        return new @string[]{"LD_LIBRARY_PATH"u8, "SHLIB_PATH"u8}.slice();
    }
    if (exprᴛ1 == "irix"u8) {
        return new @string[]{"LD_LIBRARY_PATH"u8, "LD_LIBRARYN32_PATH"u8, "LD_LIBRARY64_PATH"u8}.slice();
    }
    if (exprᴛ1 == "illumos"u8 || exprᴛ1 == "solaris"u8) {
        return new @string[]{"LD_LIBRARY_PATH"u8, "LD_LIBRARY_PATH_32"u8, "LD_LIBRARY_PATH_64"u8}.slice();
    }
    if (exprᴛ1 == "windows"u8) {
        return new @string[]{"SystemRoot"u8, "COMSPEC"u8, "PATHEXT"u8, "WINDIR"u8}.slice();
    }

    return default!;
}))();

// Handler runs an executable in a subprocess with a CGI environment.
[GoType] partial struct Handler {
    public @string Path; // path to the CGI executable
    public @string Root; // root URI prefix of handler or empty for "/"
    // Dir specifies the CGI executable's working directory.
    // If Dir is empty, the base directory of Path is used.
    // If Path has no base directory, the current working
    // directory is used.
    public @string Dir;
    public slice<@string> Env; // extra environment variables to set, if any, as "key=value"
    public slice<@string> InheritEnv; // environment variables to inherit from host, as "key"
    public ж<log.Logger> Logger; // optional log for errors or nil to use log.Print
    public slice<@string> Args; // optional arguments to pass to child process
    public io.Writer Stderr;   // optional stderr for the child process; nil means os.Stderr
    // PathLocationHandler specifies the root http Handler that
    // should handle internal redirects when the CGI process
    // returns a Location header value starting with a "/", as
    // specified in RFC 3875 § 6.3.2. This will likely be
    // http.DefaultServeMux.
    //
    // If nil, a CGI response with a local URI path is instead sent
    // back to the client and not redirected internally.
    public httpꓸHandler PathLocationHandler;
}

[GoRecv] internal static io.Writer stderr(this ref Handler h) {
    if (h.Stderr != default!) {
        return h.Stderr;
    }
    return new os.FileжWriter(os.Stderr);
}

// removeLeadingDuplicates remove leading duplicate in environments.
// It's possible to override environment like following.
//
//	cgi.Handler{
//	  ...
//	  Env: []string{"SCRIPT_FILENAME=foo.php"},
//	}
internal static slice<@string> /*ret*/ removeLeadingDuplicates(slice<@string> env) {
    slice<@string> ret = default!;

    foreach (var (i, e) in env) {
        var found = false;
        {
            nint eq = strings.IndexByte(e, (rune)'='); if (eq != -1) {
                @string keq = e.slice(0, eq + 1); // "key="
                foreach (var (_, e2) in env.slice(i + 1)) {
                    if (strings.HasPrefix(e2, keq)) {
                        found = true;
                        break;
                    }
                }
            }
        }
        if (!found) {
            ret = append(ret, e);
        }
    }
    return ret;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pathˢ = "PATH"u8;
internal static readonly @string binUsrBinUsrUcbUsrBsdUsrˢ = "/bin:/usr/bin:/usr/ucb:/usr/bsd:/usr/local/bin"u8;
internal static readonly @string locationˢ = "Location"u8;

public static void ServeHTTP(this ж<Handler> Ꮡh, http.ResponseWriter rw, ж<http.Request> Ꮡreq) {
    GoFrame ᒐ = default;
    try {
        ref var h = ref Ꮡh.DerefOrNull();
        ref var req = ref Ꮡreq.DerefOrNull();

        if (len(req.TransferEncoding) > 0 && req.TransferEncoding[0] == "chunked") {
            rw.WriteHeader(http.StatusBadRequest);
            rw.Write(slice<byte>("Chunked request bodies are not supported by CGI."u8));
            return;
        }
        @string root = strings.TrimRight(h.Root, "/"u8);
        @string pathInfo = strings.TrimPrefix((~req.URL).Path, root);
        @string port = "80"u8;
        if (req.TLS != nil) {
            port = "443"u8;
        }
        {
            var matches = trailingPort.FindStringSubmatch(req.Host); if (len(matches) != 0) {
                port = matches[1];
            }
        }
        var env = new @string[]{
            "SERVER_SOFTWARE=go"u8,
            "SERVER_PROTOCOL=HTTP/1.1"u8,
            "HTTP_HOST="u8 + req.Host,
            "GATEWAY_INTERFACE=CGI/1.1"u8,
            "REQUEST_METHOD="u8 + req.Method,
            "QUERY_STRING="u8 + (~req.URL).RawQuery,
            "REQUEST_URI="u8 + req.URL.RequestURI(),
            "PATH_INFO="u8 + pathInfo,
            "SCRIPT_NAME="u8 + root,
            "SCRIPT_FILENAME="u8 + h.Path,
            "SERVER_PORT="u8 + port
        }.slice();
        {
            var (remoteIP, remotePort, errΔ1) = net.SplitHostPort(req.RemoteAddr); if (errΔ1 == default!){
                env = append(env, "REMOTE_ADDR="u8 + remoteIP, "REMOTE_HOST=" + remoteIP, "REMOTE_PORT=" + remotePort);
            } else {
                // could not parse ip:port, let's use whole RemoteAddr and leave REMOTE_PORT undefined
                env = append(env, "REMOTE_ADDR="u8 + req.RemoteAddr, "REMOTE_HOST=" + req.RemoteAddr);
            }
        }
        {
            var (hostDomain, _, errΔ2) = net.SplitHostPort(req.Host); if (errΔ2 == default!){
                env = append(env, "SERVER_NAME="u8 + hostDomain);
            } else {
                env = append(env, "SERVER_NAME="u8 + req.Host);
            }
        }
        if (req.TLS != nil) {
            env = append(env, "HTTPS=on"u8);
        }
        foreach (var (kᴛ1, v) in req.Header) {
            var k = kᴛ1;

            k = strings.Map(upperCaseAndUnderscore, k);
            if (k == "PROXY"u8) {
                // See Issue 16405
                continue;
            }
            @string joinStr = ", "u8;
            if (k == "COOKIE"u8) {
                joinStr = "; "u8;
            }
            env = append(env, "HTTP_"u8 + k + "="u8 + strings.Join(v, joinStr));
        }
        if (req.ContentLength > 0) {
            env = append(env, fmt.Sprintf("CONTENT_LENGTH=%d"u8, req.ContentLength));
        }
        {
            @string ctype = req.Header.Get(contentTypeˢ2); if (ctype != ""u8) {
                env = append(env, "CONTENT_TYPE="u8 + ctype);
            }
        }
        @string envPath = os.Getenv(pathˢ);
        if (envPath == ""u8) {
            envPath = binUsrBinUsrUcbUsrBsdUsrˢ;
        }
        env = append(env, "PATH="u8 + envPath);
        foreach (var (_, e) in h.InheritEnv) {
            {
                @string v = os.Getenv(e); if (v != ""u8) {
                    env = append(env, e + "="u8 + v);
                }
            }
        }
        foreach (var (_, e) in osDefaultInheritEnv) {
            {
                @string v = os.Getenv(e); if (v != ""u8) {
                    env = append(env, e + "="u8 + v);
                }
            }
        }
        if (h.Env != default!) {
            env = appendꓸꓸꓸ(env, h.Env);
        }
        env = removeLeadingDuplicates(env);
        ref var cwd = ref heap(new @string(), out var Ꮡcwd);
        ref var path = ref heap(new @string(), out var Ꮡpath);
        if (h.Dir != ""u8){
            path = h.Path;
            cwd = h.Dir;
        } else {
            (cwd, path) = filepath.Split(h.Path);
        }
        if (cwd == ""u8) {
            cwd = "."u8;
        }
        void internalError(error errΔ3) {
            rw.WriteHeader(http.StatusInternalServerError);
            Ꮡh.Value.printf("CGI error: %v"u8, errΔ3);
        }
        var cmd = Ꮡ(new exec.Cmd(
            Path: path,
            Args: appendꓸꓸꓸ(new @string[]{h.Path}.slice(), h.Args),
            Dir: cwd,
            Env: env,
            Stderr: h.stderr()
        ));
        if (req.ContentLength != 0) {
            cmd.Value.Stdin = req.Body;
        }
        var (stdoutRead, err) = cmd.StdoutPipe();
        if (err != default!) {
            internalError(err);
            return;
        }
        err = cmd.Start();
        if (err != default!) {
            internalError(err);
            return;
        }
        {
            var hook = testHookStartProcess; if (hook != default!) {
                hook((~cmd).Process);
            }
        }
        var cmdʗ1 = cmd;
        defer(() => cmdʗ1.Wait(), ref ᒐ);
        var stdoutReadʗ1 = stdoutRead;
        defer(() => stdoutReadʗ1.Close(), ref ᒐ);
        var linebody = bufio.NewReaderSize(stdoutRead, 1024);
        var headers = new httpꓸHeader(0);
        nint statusCode = 0;
        nint headerLines = 0;
        var sawBlankLine = false;
        while (ᐧ) {
            var (line, isPrefix, errΔ4) = linebody.ReadLine();
            if (isPrefix) {
                rw.WriteHeader(http.StatusInternalServerError);
                h.printf("cgi: long header line from subprocess."u8);
                return;
            }
            if (AreEqual(errΔ4, io.EOF)) {
                break;
            }
            if (errΔ4 != default!) {
                rw.WriteHeader(http.StatusInternalServerError);
                h.printf("cgi: error reading headers: %v"u8, errΔ4);
                return;
            }
            if (len(line) == 0) {
                sawBlankLine = true;
                break;
            }
            headerLines++;
            var (header, val, ok) = strings.Cut(((@string)line), ":"u8);
            if (!ok) {
                h.printf("cgi: bogus header line: %s"u8, line);
                continue;
            }
            if (!httpguts.ValidHeaderFieldName(header)) {
                h.printf("cgi: invalid header name: %q"u8, header);
                continue;
            }
            val = textproto.TrimString(val);
            switch (ᐧ) {
            case {} when header == "Status"u8: {
                if (len(val) < 3) {
                    h.printf("cgi: bogus status (short): %q"u8, val);
                    return;
                }
                var (code, errΔ6) = strconv.Atoi(val[0..3]);
                if (errΔ6 != default!) {
                    h.printf("cgi: bogus status: %q"u8, val);
                    h.printf("cgi: line was %q"u8, line);
                    return;
                }
                statusCode = code;
                break;
            }
            default: {
                headers.Add(header, val);
                break;
            }}

        }
        if (headerLines == 0 || !sawBlankLine) {
            rw.WriteHeader(http.StatusInternalServerError);
            h.printf("cgi: no headers"u8);
            return;
        }
        {
            @string loc = headers.Get(locationˢ); if (loc != ""u8) {
                if (strings.HasPrefix(loc, "/"u8) && h.PathLocationHandler != default!) {
                    h.handleInternalRedirect(rw, Ꮡreq, loc);
                    return;
                }
                if (statusCode == 0) {
                    statusCode = http.StatusFound;
                }
            }
        }
        if (statusCode == 0 && headers.Get(contentTypeˢ2) == ""u8) {
            rw.WriteHeader(http.StatusInternalServerError);
            h.printf("cgi: missing required Content-Type in headers"u8);
            return;
        }
        if (statusCode == 0) {
            statusCode = http.StatusOK;
        }
        // Copy headers to rw's headers, after we've decided not to
        // go into handleInternalRedirect, which won't want its rw
        // headers to have been touched.
        foreach (var (k, vv) in headers) {
            foreach (var (_, v) in vv) {
                rw.Header().Add(k, v);
            }
        }
        rw.WriteHeader(statusCode);
        (_, err) = io.Copy(new http_ResponseWriterᴠWriter(rw), new bufio_ReaderжReader(linebody));
        if (err != default!) {
            h.printf("cgi: copy error: %v"u8, err);
            // And kill the child CGI process so we don't hang on
            // the deferred cmd.Wait above if the error was just
            // the client (rw) going away. If it was a read error
            // (because the child died itself), then the extra
            // kill of an already-dead process is harmless (the PID
            // won't be reused until the Wait above).
            (~cmd).Process.Kill();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoRecv] internal static void printf(this ref Handler h, @string format, params ꓸꓸꓸany vʗp) {
    var v = vʗp.sslice();

    if (h.Logger != nil){
        h.Logger.Printf(format, v.ꓸꓸꓸ);
    } else {
        log.Printf(format, v.ꓸꓸꓸ);
    }
}

[GoRecv] internal static void handleInternalRedirect(this ref Handler h, http.ResponseWriter rw, ж<http.Request> Ꮡreq, @string path) {
    ref var req = ref Ꮡreq.DerefOrNull();

    var (url, err) = req.URL.Parse(path);
    if (err != default!) {
        rw.WriteHeader(http.StatusInternalServerError);
        h.printf("cgi: error resolving local URI path %q: %v"u8, path, err);
        return;
    }
    // TODO: RFC 3875 isn't clear if only GET is supported, but it
    // suggests so: "Note that any message-body attached to the
    // request (such as for a POST request) may not be available
    // to the resource that is the target of the redirect."  We
    // should do some tests against Apache to see how it handles
    // POST, HEAD, etc. Does the internal redirect get the same
    // method or just GET? What about incoming headers?
    // (e.g. Cookies) Which headers, if any, are copied into the
    // second request?
    var newReq = Ꮡ(new http.Request(
        Method: "GET"u8,
        URL: url,
        Proto: "HTTP/1.1"u8,
        ProtoMajor: 1,
        ProtoMinor: 1,
        Header: new httpꓸHeader(0),
        Host: (~url).Host,
        RemoteAddr: req.RemoteAddr,
        TLS: req.TLS
    ));
    h.PathLocationHandler.ServeHTTP(rw, newReq);
}

internal static rune upperCaseAndUnderscore(rune r) {
    switch (ᐧ) {
    case {} when r >= (rune)'a' && r <= (rune)'z': {
        return r - ((rune)'a' - (rune)'A');
    }
    case {} when r is (rune)'-': {
        return (rune)'_';
    }
    case {} when r is (rune)'=': {
        return (rune)'_';
    }}

    // Maybe not part of the CGI 'spec' but would mess up
    // the environment in any case, as Go represents the
    // environment as a slice of "key=value" strings.
    // TODO: other transformations in spec or practice?
    return r;
}

internal static Action<ж<os.Process>> testHookStartProcess;  // nil except for some tests

} // end cgi_package
