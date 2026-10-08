// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:generate go test . -run=^TestGenerated$ -fix
namespace go.@internal;

partial class platform_package {

// An OSArch is a pair of GOOS and GOARCH values indicating a platform.
partial struct OSArch {
    public @string GOOS, GOARCH;
}

public static @string String(this OSArch p) {
    return p.GOOS + "/"u8 + p.GOARCH;
}

// RaceDetectorSupported reports whether goos/goarch supports the race
// detector. There is a copy of this function in cmd/dist/test.go.
// Race detector only supports 48-bit VMA on arm64. But it will always
// return true for arm64, because we don't have VMA size information during
// the compile time.
public static bool RaceDetectorSupported(@string goos, @string goarch) {
    var exprᴛ1 = goos;
    if (exprᴛ1 == "linux"u8) {
        return goarch == "amd64"u8 || goarch == "ppc64le"u8 || goarch == "arm64"u8 || goarch == "s390x"u8;
    }
    if (exprᴛ1 == "darwin"u8) {
        return goarch == "amd64"u8 || goarch == "arm64"u8;
    }
    if (exprᴛ1 == "freebsd"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "windows"u8) {
        return goarch == "amd64"u8;
    }
    { /* default: */
        return false;
    }

}

// MSanSupported reports whether goos/goarch supports the memory
// sanitizer option.
public static bool MSanSupported(@string goos, @string goarch) {
    var exprᴛ1 = goos;
    if (exprᴛ1 == "linux"u8) {
        return goarch == "amd64"u8 || goarch == "arm64"u8 || goarch == "loong64"u8;
    }
    if (exprᴛ1 == "freebsd"u8) {
        return goarch == "amd64"u8;
    }
    { /* default: */
        return false;
    }

}

// ASanSupported reports whether goos/goarch supports the address
// sanitizer option.
public static bool ASanSupported(@string goos, @string goarch) {
    var exprᴛ1 = goos;
    if (exprᴛ1 == "linux"u8) {
        return goarch == "arm64"u8 || goarch == "amd64"u8 || goarch == "loong64"u8 || goarch == "riscv64"u8 || goarch == "ppc64le"u8;
    }
    { /* default: */
        return false;
    }

}

// FuzzSupported reports whether goos/goarch supports fuzzing
// ('go test -fuzz=.').
public static bool FuzzSupported(@string goos, @string goarch) {
    var exprᴛ1 = goos;
    if (exprᴛ1 == "darwin"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "linux"u8 || exprᴛ1 == "windows"u8) {
        return true;
    }
    { /* default: */
        return false;
    }

}

// FuzzInstrumented reports whether fuzzing on goos/goarch uses coverage
// instrumentation. (FuzzInstrumented implies FuzzSupported.)
public static bool FuzzInstrumented(@string goos, @string goarch) {
    var exprᴛ1 = goarch;
    if (exprᴛ1 == "amd64"u8 || exprᴛ1 == "arm64"u8) {
        return FuzzSupported(goos, // TODO(#14565): support more architectures.
 goarch);
    }
    { /* default: */
        return false;
    }

}

// MustLinkExternal reports whether goos/goarch requires external linking
// with or without cgo dependencies.
public static bool MustLinkExternal(@string goos, @string goarch, bool withCgo) {
    if (withCgo) {
        var exprᴛ1 = goarch;
        if (exprᴛ1 == "loong64"u8 || exprᴛ1 == "mips"u8 || exprᴛ1 == "mipsle"u8 || exprᴛ1 == "mips64"u8 || exprᴛ1 == "mips64le"u8) {
            return true;
        }
        if (exprᴛ1 == "arm64"u8) {
            if (goos == "windows"u8) {
                // Internally linking cgo is incomplete on some architectures.
                // https://go.dev/issue/14449
                // windows/arm64 internal linking is not implemented.
                return true;
            }
        }
        else if (exprᴛ1 == "ppc64"u8) {
            if (goos == "aix"u8 || goos == "linux"u8) {
                // Big Endian PPC64 cgo internal linking is not implemented for aix or linux.
                // https://go.dev/issue/8912
                return true;
            }
        }

        var exprᴛ2 = goos;
        if (exprᴛ2 == "android"u8) {
            return true;
        }
        if (exprᴛ2 == "dragonfly"u8) {
            return true;
        }

    }
    // It seems that on Dragonfly thread local storage is
    // set up by the dynamic linker, so internal cgo linking
    // doesn't work. Test case is "go test runtime/cgo".
    var exprᴛ3 = goos;
    if (exprᴛ3 == "android"u8) {
        if (goarch != "arm64"u8) {
            return true;
        }
    }
    else if (exprᴛ3 == "ios"u8) {
        if (goarch == "arm64"u8) {
            return true;
        }
    }

    return false;
}

// BuildModeSupported reports whether goos/goarch supports the given build mode
// using the given compiler.
// There is a copy of this function in cmd/dist/test.go.
public static bool BuildModeSupported(@string compiler, @string buildmode, @string goos, @string goarch) {
    if (compiler == "gccgo"u8) {
        return true;
    }
    {
        var (_, ok) = distInfo[new OSArch(goos, goarch), ꟷ]; if (!ok) {
            return false; // platform unrecognized
        }
    }
    @string platform = goos + "/"u8 + goarch;
    var exprᴛ1 = buildmode;
    if (exprᴛ1 == "archive"u8) {
        return true;
    }
    if (exprᴛ1 == "c-archive"u8) {
        var exprᴛ2 = goos;
        if (exprᴛ2 == "aix"u8 || exprᴛ2 == "darwin"u8 || exprᴛ2 == "ios"u8 || exprᴛ2 == "windows"u8) {
            return true;
        }
        if (exprᴛ2 == "linux"u8) {
            var exprᴛ3 = goarch;
            if (exprᴛ3 == "386"u8 || exprᴛ3 == "amd64"u8 || exprᴛ3 == "arm"u8 || exprᴛ3 == "armbe"u8 || exprᴛ3 == "arm64"u8 || exprᴛ3 == "arm64be"u8 || exprᴛ3 == "loong64"u8 || exprᴛ3 == "ppc64le"u8 || exprᴛ3 == "riscv64"u8 || exprᴛ3 == "s390x"u8) {
                return true;
            }
            { /* default: */
                return false;
            }

        }
        else if (exprᴛ2 == "freebsd"u8) {
            return goarch == "amd64"u8;
        }

        return false;
    }
    if (exprᴛ1 == "c-shared"u8) {
        var exprᴛ4 = platform;
        if (exprᴛ4 == "linux/amd64"u8 || exprᴛ4 == "linux/arm"u8 || exprᴛ4 == "linux/arm64"u8 || exprᴛ4 == "linux/loong64"u8 || exprᴛ4 == "linux/386"u8 || exprᴛ4 == "linux/ppc64le"u8 || exprᴛ4 == "linux/riscv64"u8 || exprᴛ4 == "linux/s390x"u8 || exprᴛ4 == "android/amd64"u8 || exprᴛ4 == "android/arm"u8 || exprᴛ4 == "android/arm64"u8 || exprᴛ4 == "android/386"u8 || exprᴛ4 == "freebsd/amd64"u8 || exprᴛ4 == "darwin/amd64"u8 || exprᴛ4 == "darwin/arm64"u8 || exprᴛ4 == "windows/amd64"u8 || exprᴛ4 == "windows/386"u8 || exprᴛ4 == "windows/arm64"u8 || exprᴛ4 == "wasip1/wasm"u8) {
            return true;
        }

        return false;
    }
    if (exprᴛ1 == "default"u8) {
        return true;
    }
    if (exprᴛ1 == "exe"u8) {
        return true;
    }
    if (exprᴛ1 == "pie"u8) {
        var exprᴛ5 = platform;
        if (exprᴛ5 == "linux/386"u8 || exprᴛ5 == "linux/amd64"u8 || exprᴛ5 == "linux/arm"u8 || exprᴛ5 == "linux/arm64"u8 || exprᴛ5 == "linux/loong64"u8 || exprᴛ5 == "linux/ppc64le"u8 || exprᴛ5 == "linux/riscv64"u8 || exprᴛ5 == "linux/s390x"u8 || exprᴛ5 == "android/amd64"u8 || exprᴛ5 == "android/arm"u8 || exprᴛ5 == "android/arm64"u8 || exprᴛ5 == "android/386"u8 || exprᴛ5 == "freebsd/amd64"u8 || exprᴛ5 == "darwin/amd64"u8 || exprᴛ5 == "darwin/arm64"u8 || exprᴛ5 == "ios/amd64"u8 || exprᴛ5 == "ios/arm64"u8 || exprᴛ5 == "aix/ppc64"u8 || exprᴛ5 == "openbsd/arm64"u8 || exprᴛ5 == "windows/386"u8 || exprᴛ5 == "windows/amd64"u8 || exprᴛ5 == "windows/arm"u8 || exprᴛ5 == "windows/arm64"u8) {
            return true;
        }

        return false;
    }
    if (exprᴛ1 == "shared"u8) {
        var exprᴛ6 = platform;
        if (exprᴛ6 == "linux/386"u8 || exprᴛ6 == "linux/amd64"u8 || exprᴛ6 == "linux/arm"u8 || exprᴛ6 == "linux/arm64"u8 || exprᴛ6 == "linux/ppc64le"u8 || exprᴛ6 == "linux/s390x"u8) {
            return true;
        }

        return false;
    }
    if (exprᴛ1 == "plugin"u8) {
        var exprᴛ7 = platform;
        if (exprᴛ7 == "linux/amd64"u8 || exprᴛ7 == "linux/arm"u8 || exprᴛ7 == "linux/arm64"u8 || exprᴛ7 == "linux/386"u8 || exprᴛ7 == "linux/loong64"u8 || exprᴛ7 == "linux/s390x"u8 || exprᴛ7 == "linux/ppc64le"u8 || exprᴛ7 == "android/amd64"u8 || exprᴛ7 == "android/386"u8 || exprᴛ7 == "darwin/amd64"u8 || exprᴛ7 == "darwin/arm64"u8 || exprᴛ7 == "freebsd/amd64"u8) {
            return true;
        }

        return false;
    }
    { /* default: */
        return false;
    }

}

// linux/ppc64 not supported because it does
// not support external linking mode yet.
// Other targets do not support -shared,
// per ParseFlags in
// cmd/compile/internal/base/flag.go.
// For c-archive the Go tool passes -shared,
// so that the result is suitable for inclusion
// in a PIE or shared library.
public static bool InternalLinkPIESupported(@string goos, @string goarch) {
    var exprᴛ1 = goos + "/"u8 + goarch;
    if (exprᴛ1 == "android/arm64"u8 || exprᴛ1 == "darwin/amd64"u8 || exprᴛ1 == "darwin/arm64"u8 || exprᴛ1 == "linux/amd64"u8 || exprᴛ1 == "linux/arm64"u8 || exprᴛ1 == "linux/ppc64le"u8 || exprᴛ1 == "windows/386"u8 || exprᴛ1 == "windows/amd64"u8 || exprᴛ1 == "windows/arm"u8 || exprᴛ1 == "windows/arm64"u8) {
        return true;
    }

    return false;
}

// DefaultPIE reports whether goos/goarch produces a PIE binary when using the
// "default" buildmode. On Windows this is affected by -race,
// so force the caller to pass that in to centralize that choice.
public static bool DefaultPIE(@string goos, @string goarch, bool isRace) {
    var exprᴛ1 = goos;
    if (exprᴛ1 == "android"u8 || exprᴛ1 == "ios"u8) {
        return true;
    }
    if (exprᴛ1 == "windows"u8) {
        if (isRace) {
            // PIE is not supported with -race on windows;
            // see https://go.dev/cl/416174.
            return false;
        }
        return true;
    }
    if (exprᴛ1 == "darwin"u8) {
        return true;
    }

    return false;
}

// ExecutableHasDWARF reports whether the linked executable includes DWARF
// symbols on goos/goarch.
public static bool ExecutableHasDWARF(@string goos, @string goarch) {
    var exprᴛ1 = goos;
    if (exprᴛ1 == "plan9"u8 || exprᴛ1 == "ios"u8) {
        return false;
    }

    return true;
}

// osArchInfo describes information about an OSArch extracted from cmd/dist and
// stored in the generated distInfo map.
partial struct osArchInfo {
    public bool CgoSupported;
    public bool FirstClass;
    public bool Broken;
}

// CgoSupported reports whether goos/goarch supports cgo.
public static bool CgoSupported(@string goos, @string goarch) {
    return distInfo[new OSArch(goos, goarch)].CgoSupported;
}

// FirstClass reports whether goos/goarch is considered a “first class” port.
// (See https://go.dev/wiki/PortingPolicy#first-class-ports.)
public static bool FirstClass(@string goos, @string goarch) {
    return distInfo[new OSArch(goos, goarch)].FirstClass;
}

// Broken reports whether goos/goarch is considered a broken port.
// (See https://go.dev/wiki/PortingPolicy#broken-ports.)
public static bool Broken(@string goos, @string goarch) {
    return distInfo[new OSArch(goos, goarch)].Broken;
}

} // end platform_package
