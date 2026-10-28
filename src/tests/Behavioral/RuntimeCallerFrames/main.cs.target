namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial nint selfLine() {
    var (_, _, line, _) = runtime.Caller(0);
    return line;
}

internal static partial nint callerLine() {
    var (_, _, line, _) = runtime.Caller(1);
    return line;
}

internal static partial @string callerFile() {
    var (_, @file, _, _) = runtime.Caller(1);
    return @file;
}

internal static partial nint grandLine() {
    var (_, _, line, _) = runtime.Caller(2);
    return line;
}

internal static partial nint wrapGrand() {
    return grandLine();
}

internal static partial (nint, nint) sameSite() {
    return (callerLine(), wrapGrand());
}

internal static partial nint siteA() {
    return callerLine();
}

internal static partial nint siteB() {
    return callerLine();
}

internal static partial bool okAt(nint skip) {
    var (_, _, _, ok) = runtime.Caller(skip);
    return ok;
}

internal static partial bool deepOK() {
    return okAt(2);
}

internal static partial nint depth() {
    var pc = new slice<uintptr>(256);
    return runtime.Callers(0, pc);
}

internal static partial nint depthPlus1() {
    return depth();
}

internal static partial nint depthPlus2() {
    return depthPlus1();
}

internal static partial void Main() {
    var (x, y) = sameSite();
    fmt.Println("same-line agreement:", x == y);

    fmt.Println("self line constant:", selfLine() == selfLine());
    fmt.Println("self line differs from call site:", selfLine() != callerLine());

    fmt.Println("distinct call sites:", siteA() != siteB());

    var (_, here, _, _) = runtime.Caller(0);
    fmt.Println("same file:", here == callerFile());
    fmt.Println("file reported:", len(here) > 0);

    fmt.Println("caller file tail:", callerFileTail());
    fmt.Println("caller file rooted:", callerFileRooted());
    fmt.Println("caller line:", selfLine());
    fmt.Println("caller line two frames up:", wrapGrand());
    fmt.Println("traceback names a go file:", hasSub(stackText(), "/main.go:"));

    fmt.Println("ok at 0:", okAt(0));
    fmt.Println("ok at 1:", okAt(1));
    fmt.Println("ok two levels up:", deepOK());
    fmt.Println("ok past the stack:", okAt(1000));

    fmt.Println("callers depth delta:", depthPlus2() - depth());

    var (callerFwd, callerBack) = callerSeparators();
    fmt.Println("caller file uses forward slash:", callerFwd);
    fmt.Println("caller file uses host separator:", callerBack);

    var (framesFwd, framesBack) = framesSeparators();
    fmt.Println("frames files use forward slash:", framesFwd);
    fmt.Println("frames files use host separator:", framesBack);

    fmt.Println("traceback uses host separator:", stackHasBackslash());

    @string methodTrace = ((recvT)0).valueFrame();
    fmt.Println("traceback names pointer receiver:", hasSub(methodTrace, "main.(*recvT).ptrFrame"));
    fmt.Println("traceback names value receiver:", hasSub(methodTrace, "main.recvT.valueFrame"));
    fmt.Println("traceback drops pointer receiver:", hasSub(methodTrace, "main.ptrFrame"));
    fmt.Println("traceback drops value receiver:", hasSub(methodTrace, "main.valueFrame"));

    @string genTrace = new genRecv<nint>().genFrame();
    fmt.Println("traceback names generic receiver:", hasSub(genTrace, "main.genRecv[...].genFrame"));

    @string plainTrace = plainFrame();
    fmt.Println("traceback names plain func:", hasSub(plainTrace, "main.plainFrame"));
    fmt.Println("traceback parenthesizes plain func:", hasSub(plainTrace, "(*"));
}

internal static bool hasByte(@string s, byte b) {
    for (nint i = 0; i < len(s); i++) {
        if (s[i] == b) {
            return true;
        }
    }
    return false;
}

internal static partial (bool fwd, bool back) callerSeparators() {
    var (_, @file, _, _) = runtime.Caller(0);
    return (hasByte(@file, (rune)'/'), hasByte(@file, (rune)'\\'));
}

internal static partial (bool fwd, bool back) framesSeparators() {
    bool fwd = default;
    bool back = default;

    var pc = new slice<uintptr>(64);
    nint n = runtime.Callers(0, pc);
    var frames = runtime.CallersFrames(pc.slice(0, n));
    for (;;) {
        var (frame, more) = frames.Next();
        if (len(frame.File) > 0) {
            fwd = fwd || hasByte(frame.File, (rune)'/');
            back = back || hasByte(frame.File, (rune)'\\');
        }
        if (!more) {
            break;
        }
    }
    return (fwd, back);
}

internal static partial @string callerFileTail() {
    var (_, @file, _, _) = runtime.Caller(0);
    nint cut = 0;
    nint seen = 0;
    for (nint i = len(@file) - 1; i >= 0; i--) {
        if (@file[i] == (rune)'/') {
            seen++;
            if (seen == 2) {
                cut = i + 1;
                break;
            }
        }
    }
    return @file.slice(cut);
}

internal static partial bool callerFileRooted() {
    var (_, @file, _, _) = runtime.Caller(0);
    if (len(@file) > 0 && @file[0] == (rune)'/') {
        return true;
    }
    return len(@file) > 1 && @file[1] == (rune)':';
}

internal static partial bool stackHasBackslash() {
    var buf = new slice<byte>(8192);
    nint n = runtime.Stack(buf, false);
    return hasByte((@string)(buf.slice(0, n)), (rune)'\\');
}

internal static bool hasSub(@string s, @string sub) {
    if (len(sub) > len(s)) {
        return false;
    }
    for (nint i = 0; i + len(sub) <= len(s); i++) {
        nint j = 0;
        while (j < len(sub) && s[i + j] == sub[j]) {
            j++;
        }
        if (j == len(sub)) {
            return true;
        }
    }
    return false;
}

internal static partial @string stackText() {
    var buf = new slice<byte>(8192);
    nint n = runtime.Stack(buf, false);
    return (@string)(buf.slice(0, n));
}

partial struct recvT /*num:nint*/;

partial struct genRecv<X> {
    internal X v;
}

internal static partial @string ptrFrame(this ref recvT t) {
    return stackText();
}

internal static partial @string valueFrame(this recvT t) {
    return t.ptrFrame();
}

internal static partial @string genFrame<X>(this genRecv<X> g) {
    return stackText();
}

internal static partial @string plainFrame() {
    return stackText();
}

} // end main_package
