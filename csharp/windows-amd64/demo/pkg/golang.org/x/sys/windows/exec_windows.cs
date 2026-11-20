// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using errorspkg = errors_package;
using @unsafe = unsafe_package;
using static windows_package;

partial class windows_package {

public static @string EscapeArg(@string s) {
    if (len(s) == 0) {
        return @"""""";
    }
    nint n = len(s);
    var hasSpace = false;
    for (nint i = 0; i < len(s); i++) {
        switch (s[i]) {
        case (byte)'"' or (byte)'\\': {
            n++;
            break;
        }
        case (byte)' ' or (byte)'\t': {
            hasSpace = true;
            break;
        }}

    }
    if (hasSpace) {
        n += 2;
    }
    if (n == len(s)) {
        return s;
    }

    var qs = new slice<byte>(n);
    nint j = 0;
    if (hasSpace) {
        qs[j] = (byte)'"';
        j++;
    }
    nint slashes = 0;
    for (nint i = 0; i < len(s); i++) {
        switch (s[i]) {
        default: {
            slashes = 0;
            qs[j] = s[i];
            break;
        }
        case (byte)'\\': {
            slashes++;
            qs[j] = s[i];
            break;
        }
        case (byte)'"': {
            for (; slashes > 0; slashes--) {
                qs[j] = (byte)'\\';
                j++;
            }
            qs[j] = (byte)'\\';
            j++;
            qs[j] = s[i];
            break;
        }}

        j++;
    }
    if (hasSpace) {
        for (; slashes > 0; slashes--) {
            qs[j] = (byte)'\\';
            j++;
        }
        qs[j] = (byte)'"';
        j++;
    }
    return (@string)(qs.slice(0, j));
}

public static @string ComposeCommandLine(slice<@string> args) {
    var commandLineᴷ = new stackAppend<byte>();

    if (len(args) == 0) {
        return "";
    }

    @string prog = args[0];
    var mustQuote = len(prog) == 0;
    for (nint i = 0; i < len(prog); i++) {
        var c = prog[i];
        if (c <= ' ' || (c == '"' && i == 0)) {
            mustQuote = true;
            break;
        }
    }
    slice<byte> commandLine = default;
    if (mustQuote) {
        commandLine = new slice<byte>(0, len(prog) + 2);
        commandLine = append(commandLineᴷ, commandLine, (byte)'"');
        for (nint i = 0; i < len(prog); i++) {
            var c = prog[i];
            if (c == '"') {
                continue;
            }
            commandLine = append(commandLine, c);
        }
        commandLine = append(commandLine, (byte)'"');
    } else {
        if (len(args) == 1) {
            return prog;
        }
        commandLine = slice<byte>(prog);
    }

    foreach (var (_, arg) in range(args[1..])) {
        commandLine = append(commandLine, (byte)' ');
        commandLine = append(commandLine, EscapeArg(arg).ꓸꓸꓸ);
    }
    return (@string)commandLine;
}

public static (slice<@string>, error) DecomposeCommandLine(@string commandLine) {
    GoFrame ᒐ = default;
    try {
        var argsᴷ = new stackAppend<@string>(moves: true);

        if (len(commandLine) == 0) {
            return (slice<@string>([]), default);
        }
        var (utf16CommandLine, err) = UTF16FromString(commandLine);
        if (err != nil) {
            return (default, errorspkg.New("string with NUL passed to DecomposeCommandLine"));
        }
        ref var argc = ref heap(new int32(), out var Ꮡargc);
        (var argv, err) = commandLineToArgv(Ꮡ(utf16CommandLine, 0), Ꮡargc);
        if (err != nil) {
            return (default, err);
        }
        defer(LocalFree, (Handle)(uintptr)@unsafe.Pointer.FromBox(argv), ref ᒐ);

        slice<@string> args = default;
        foreach (var (_, p) in range(@unsafe.Slice(argv, argc))) {
            args = append(argsᴷ, args, UTF16PtrToString(p));
        }
        args = move2heap(argsᴷ, args);
        return (args, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static (ж<array<ж<array<uint16>>>> argv, error err) CommandLineToArgv(ж<uint16> cmd, ж<int32> argc) {
    ж<array<ж<array<uint16>>>> argv = default;
    error err = default;

    (var argp, err) = commandLineToArgv(cmd, argc);
    argv = @unsafe.Pointer.To<array<ж<array<uint16>>>>(@unsafe.Pointer.FromBox(argp));
    return (argv, err);
}

public static void CloseOnExec(Handle fd) {
    SetHandleInformation(fd, HANDLE_FLAG_INHERIT, 0);
}

public static (@string path, error err) FullPath(@string name) {
    error err = default;

    (var p, err) = UTF16PtrFromString(name);
    if (err != nil) {
        return ("", err);
    }
    var n = (uint32)100;
    for (;;) {
        var buf = new slice<uint16>((nint)(n));
        (n, err) = GetFullPathName(p, (uint32)len(buf), Ꮡ(buf, 0), nil);
        if (err != nil) {
            return ("", err);
        }
        if (n <= (uint32)len(buf)) {
            return (UTF16ToString(buf.slice(0, (nint)(n))), default);
        }
    }
}

public static (ж<ProcThreadAttributeListContainer>, error) NewProcThreadAttributeList(uint32 maxAttrCount) {
    ref var size = ref heap(new uintptr(), out var Ꮡsize);
    var err = initializeProcThreadAttributeList(nil, maxAttrCount, 0, Ꮡsize);
    if (!AreEqual(err, ERROR_INSUFFICIENT_BUFFER)) {
        if (err == nil) {
            return (default, errorspkg.New("unable to query buffer size from InitializeProcThreadAttributeList"));
        }
        return (default, err);
    }
    (var alloc, err) = LocalAlloc(LMEM_FIXED, (uint32)size);
    if (err != nil) {
        return (default, err);
    }
    var al = Ꮡ(new ProcThreadAttributeListContainer(data: @unsafe.Pointer.To<ProcThreadAttributeList>((@unsafe.Pointer)alloc)));
    err = initializeProcThreadAttributeList(al.data, maxAttrCount, 0, Ꮡsize);
    if (err != nil) {
        return (default, err);
    }
    return (al, err);
}

} partial class windowsᴛProcThreadAttributeListContainer {

public static error Update(this ref ProcThreadAttributeListContainer al, uintptr attribute, @unsafe.Pointer value, uintptr size) {
    al.pointers = append(al.pointers, value);
    return updateProcThreadAttribute(al.data, 0, attribute, value, size, nil, nil);
}

public static void Delete(this ref ProcThreadAttributeListContainer al) {
    deleteProcThreadAttributeList(al.data);
    LocalFree((Handle)(uintptr)al.data);
    al.data = default;
    al.pointers = default;
}

public static ж<ProcThreadAttributeList> List(this ref ProcThreadAttributeListContainer al) {
    return al.data;
}

} // end windowsᴛProcThreadAttributeListContainer
