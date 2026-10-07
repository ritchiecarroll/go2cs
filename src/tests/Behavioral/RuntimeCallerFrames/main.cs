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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sameLineAgreementˢ = (@string)"same-line agreement:"u8;
private static readonly object selfLineConstantˢ = (@string)"self line constant:"u8;
private static readonly object selfLineDiffersFromCallˢ = (@string)"self line differs from call site:"u8;
private static readonly object distinctCallSitesˢ = (@string)"distinct call sites:"u8;
private static readonly object sameFileˢ = (@string)"same file:"u8;
private static readonly object fileReportedˢ = (@string)"file reported:"u8;
private static readonly object callerFileTailˢ = (@string)"caller file tail:"u8;
private static readonly object callerFileRootedˢ = (@string)"caller file rooted:"u8;
private static readonly object callerLineˢ = (@string)"caller line:"u8;
private static readonly object callerLineTwoFramesUpˢ = (@string)"caller line two frames up:"u8;
private static readonly object tracebackNamesAGoFileˢ = (@string)"traceback names a go file:"u8;
private static readonly @string mainGoˢ = "/main.go:"u8;
private static readonly object okAt0ˢ = (@string)"ok at 0:"u8;
private static readonly object okAt1ˢ = (@string)"ok at 1:"u8;
private static readonly object okTwoLevelsUpˢ = (@string)"ok two levels up:"u8;
private static readonly object okPastTheStackˢ = (@string)"ok past the stack:"u8;
private static readonly object callersDepthDeltaˢ = (@string)"callers depth delta:"u8;
private static readonly object callerFileUsesForwardˢ = (@string)"caller file uses forward slash:"u8;
private static readonly object callerFileUsesHostˢ = (@string)"caller file uses host separator:"u8;
private static readonly object framesFilesUseForwardˢ = (@string)"frames files use forward slash:"u8;
private static readonly object framesFilesUseHostˢ = (@string)"frames files use host separator:"u8;
private static readonly object tracebackUsesHostˢ = (@string)"traceback uses host separator:"u8;
private static readonly object tracebackNamesPointerˢ = (@string)"traceback names pointer receiver:"u8;
private static readonly @string mainRecvTPtrFrameˢ = "main.(*recvT).ptrFrame"u8;
private static readonly object tracebackNamesValueˢ = (@string)"traceback names value receiver:"u8;
private static readonly @string mainRecvTValueFrameˢ = "main.recvT.valueFrame"u8;
private static readonly object tracebackDropsPointerˢ = (@string)"traceback drops pointer receiver:"u8;
private static readonly @string mainPtrFrameˢ = "main.ptrFrame"u8;
private static readonly object tracebackDropsValueˢ = (@string)"traceback drops value receiver:"u8;
private static readonly @string mainValueFrameˢ = "main.valueFrame"u8;
private static readonly object tracebackNamesGenericˢ = (@string)"traceback names generic receiver:"u8;
private static readonly @string mainGenRecvGenFrameˢ = "main.genRecv[...].genFrame"u8;
private static readonly object tracebackNamesPlainFuncˢ = (@string)"traceback names plain func:"u8;
private static readonly @string mainPlainFrameˢ = "main.plainFrame"u8;
private static readonly object tracebackParenthesizesˢ = (@string)"traceback parenthesizes plain func:"u8;

internal static partial void Main() {
    var (x, y) = sameSite();
    fmt.Println(sameLineAgreementˢ, x == y);
    fmt.Println(selfLineConstantˢ, selfLine() == selfLine());
    fmt.Println(selfLineDiffersFromCallˢ, selfLine() != callerLine());
    fmt.Println(distinctCallSitesˢ, siteA() != siteB());
    var (_, here, _, _) = runtime.Caller(0);
    fmt.Println(sameFileˢ, here == callerFile());
    fmt.Println(fileReportedˢ, len(here) > 0);
    fmt.Println(callerFileTailˢ, callerFileTail());
    fmt.Println(callerFileRootedˢ, callerFileRooted());
    fmt.Println(callerLineˢ, selfLine());
    fmt.Println(callerLineTwoFramesUpˢ, wrapGrand());
    fmt.Println(tracebackNamesAGoFileˢ, hasSub(stackText(), mainGoˢ));
    fmt.Println(okAt0ˢ, okAt(0));
    fmt.Println(okAt1ˢ, okAt(1));
    fmt.Println(okTwoLevelsUpˢ, deepOK());
    fmt.Println(okPastTheStackˢ, okAt(1000));
    fmt.Println(callersDepthDeltaˢ, depthPlus2() - depth());
    var (callerFwd, callerBack) = callerSeparators();
    fmt.Println(callerFileUsesForwardˢ, callerFwd);
    fmt.Println(callerFileUsesHostˢ, callerBack);
    var (framesFwd, framesBack) = framesSeparators();
    fmt.Println(framesFilesUseForwardˢ, framesFwd);
    fmt.Println(framesFilesUseHostˢ, framesBack);
    fmt.Println(tracebackUsesHostˢ, stackHasBackslash());
    @string methodTrace = ((recvT)0).valueFrame();
    fmt.Println(tracebackNamesPointerˢ, hasSub(methodTrace, mainRecvTPtrFrameˢ));
    fmt.Println(tracebackNamesValueˢ, hasSub(methodTrace, mainRecvTValueFrameˢ));
    fmt.Println(tracebackDropsPointerˢ, hasSub(methodTrace, mainPtrFrameˢ));
    fmt.Println(tracebackDropsValueˢ, hasSub(methodTrace, mainValueFrameˢ));
    @string genTrace = new genRecv<nint>(nil).genFrame();
    fmt.Println(tracebackNamesGenericˢ, hasSub(genTrace, mainGenRecvGenFrameˢ));
    @string plainTrace = plainFrame();
    fmt.Println(tracebackNamesPlainFuncˢ, hasSub(plainTrace, mainPlainFrameˢ));
    fmt.Println(tracebackParenthesizesˢ, hasSub(plainTrace, "(*"u8));
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
    bool fwd = default!;
    bool back = default!;

    var pc = new slice<uintptr>(64);
    nint n = runtime.Callers(0, pc);
    var frames = runtime.CallersFrames(pc.slice(0, n));
    while (ᐧ) {
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
    return hasByte(((@string)(buf.slice(0, n))), (rune)'\\');
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
    return ((@string)(buf.slice(0, n)));
}

[GoType("num:nint")] partial struct recvT;

[GoType] partial struct genRecv<X> {
    internal X v;
}

[GoRecv] internal static partial @string ptrFrame(this ref recvT t) {
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
