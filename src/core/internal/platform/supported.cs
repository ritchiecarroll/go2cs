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

internal static @string String⁔(/*this*/ OSArch p) {
    return p.GOOS + "/"u8 + p.GOARCH;
}

// RaceDetectorSupported reports whether goos/goarch supports the race
// detector. There is a copy of this function in cmd/dist/test.go.
// Race detector only supports 48-bit VMA on arm64. But it will always
// return true for arm64, because we don't have VMA size information during
// the compile time.
public static bool RaceDetectorSupported(@string goos, @string goarch) {
    switch (ᐧ) {
    case {} when goos == "linux"u8: {
        return goarch == "amd64"u8 || goarch == "arm64"u8 || goarch == "loong64"u8 || goarch == "ppc64le"u8 || goarch == "riscv64"u8 || goarch == "s390x"u8;
    }
    case {} when goos == "darwin"u8: {
        return goarch == "amd64"u8 || goarch == "arm64"u8;
    }
    case {} when goos == "freebsd"u8 || goos == "netbsd"u8 || goos == "windows"u8: {
        return goarch == "amd64"u8;
    }
    default: {
        return false;
    }}
}

// MSanSupported reports whether goos/goarch supports the memory
// sanitizer option.
public static bool MSanSupported(@string goos, @string goarch) {
    switch (ᐧ) {
    case {} when goos == "linux"u8: {
        return goarch == "amd64"u8 || goarch == "arm64"u8 || goarch == "loong64"u8;
    }
    case {} when goos == "freebsd"u8: {
        return goarch == "amd64"u8;
    }
    default: {
        return false;
    }}
}

// ASanSupported reports whether goos/goarch supports the address
// sanitizer option.
public static bool ASanSupported(@string goos, @string goarch) {
    switch (ᐧ) {
    case {} when goos == "linux"u8: {
        return goarch == "arm64"u8 || goarch == "amd64"u8 || goarch == "loong64"u8 || goarch == "riscv64"u8 || goarch == "ppc64le"u8;
    }
    default: {
        return false;
    }}
}

// FIPS140Supported reports whether goos/goarch supports FIPS 140-3 mode.
// It does not consider non-platform constraints such as the purego build tag,
// -asan, or GOEXPERIMENT=boringcrypto.
// There is a copy of this function in cmd/dist/test.go.
public static bool FIPS140Supported(@string goos, @string goarch) {
    // Keep this in sync with crypto/internal/fips140.Supported.
    switch (ᐧ) {
    case {} when (goarch == "wasm"u8) || (goos == "windows"u8 && goarch == "386"u8) || (goos == "openbsd"u8) || (goos == "aix"u8): {
        return false;
    }}
    return true;
}

// FuzzSupported reports whether goos/goarch supports fuzzing
// ('go test -fuzz=.').
public static bool FuzzSupported(@string goos, @string goarch) {
    switch (ᐧ) {
    case {} when goos == "darwin"u8 || goos == "freebsd"u8 || goos == "linux"u8 || goos == "openbsd"u8 || goos == "windows"u8: {
        return true;
    }
    default: {
        return false;
    }}
}

// FuzzInstrumented reports whether fuzzing on goos/goarch uses coverage
// instrumentation. (FuzzInstrumented implies FuzzSupported.)
public static bool FuzzInstrumented(@string goos, @string goarch) {
    switch (ᐧ) {
    case {} when goarch == "amd64"u8 || goarch == "arm64"u8 || goarch == "loong64"u8: {
        // TODO(#14565): support more architectures.
        return FuzzSupported(goos, goarch);
    }
    default: {
        return false;
    }}
}

// MustLinkExternal reports whether goos/goarch requires external linking
// with or without cgo dependencies.
public static bool MustLinkExternal(@string goos, @string goarch, bool withCgo) {
    if (withCgo) {
        switch (ᐧ) {
        case {} when goarch == "mips"u8 || goarch == "mipsle"u8 || goarch == "mips64"u8 || goarch == "mips64le"u8: {
            // Internally linking cgo is incomplete on some architectures.
            // https://go.dev/issue/14449
            return true;
        }
        case {} when goarch == "ppc64"u8: {
            // Big Endian PPC64 cgo internal linking is not implemented for aix or linux.
            // https://go.dev/issue/8912
            if (goos == "aix"u8 || goos == "linux"u8) {
                return true;
            }
            break;
        }}

        switch (ᐧ) {
        case {} when goos == "android"u8: {
            return true;
        }
        case {} when goos == "dragonfly"u8: {
            // It seems that on Dragonfly thread local storage is
            // set up by the dynamic linker, so internal cgo linking
            // doesn't work. Test case is "go test runtime/cgo".
            return true;
        }}
    }

    switch (ᐧ) {
    case {} when goos == "android"u8: {
        if (goarch != "arm64"u8) {
            return true;
        }
        break;
    }
    case {} when goos == "ios"u8: {
        if (goarch == "arm64"u8) {
            return true;
        }
        break;
    }}
    return false;
}

// BuildModeSupported reports whether goos/goarch supports the given build mode
// using the given compiler.
// There is a copy of this function in cmd/dist/test.go.
public static bool BuildModeSupported(@string compiler, @string buildmode, @string goos, @string goarch) {
    if (compiler == "gccgo"u8) {
        return true;
    }

    var (_, ok) = distInfo[new OSArch(goos, goarch), ꟷ]; if (!ok) {
        return false; // platform unrecognized
    }

    @string platform = goos + "/"u8 + goarch;
    switch (ᐧ) {
    case {} when buildmode == "archive"u8: {
        return true;
    }
    case {} when buildmode == "c-archive"u8: {
        switch (ᐧ) {
        case {} when goos == "aix"u8 || goos == "darwin"u8 || goos == "ios"u8 || goos == "windows"u8: {
            return true;
        }
        case {} when goos == "linux"u8: {
            switch (ᐧ) {
            case {} when goarch == "386"u8 || goarch == "amd64"u8 || goarch == "arm"u8 || goarch == "armbe"u8 || goarch == "arm64"u8 || goarch == "arm64be"u8 || goarch == "loong64"u8 || goarch == "ppc64le"u8 || goarch == "riscv64"u8 || goarch == "s390x"u8: {
                // linux/ppc64 not supported because it does
                // not support external linking mode yet.
                return true;
            }
            default: {
                // Other targets do not support -shared,
                // per ParseFlags in
                // cmd/compile/internal/base/flag.go.
                // For c-archive the Go tool passes -shared,
                // so that the result is suitable for inclusion
                // in a PIE or shared library.
                return false;
            }}
            break;
        }
        case {} when goos == "freebsd"u8: {
            return goarch == "amd64"u8;
        }}
        return false;
    }
    case {} when buildmode == "c-shared"u8: {
        switch (ᐧ) {
        case {} when platform == "linux/amd64"u8 || platform == "linux/arm"u8 || platform == "linux/arm64"u8 || platform == "linux/loong64"u8 || platform == "linux/386"u8 || platform == "linux/ppc64le"u8 || platform == "linux/riscv64"u8 || platform == "linux/s390x"u8 || platform == "android/amd64"u8 || platform == "android/arm"u8 || platform == "android/arm64"u8 || platform == "android/386"u8 || platform == "freebsd/amd64"u8 || platform == "darwin/amd64"u8 || platform == "darwin/arm64"u8 || platform == "windows/amd64"u8 || platform == "windows/386"u8 || platform == "windows/arm64"u8 || platform == "wasip1/wasm"u8: {
            return true;
        }}
        return false;
    }
    case {} when buildmode == "default"u8: {
        return true;
    }
    case {} when buildmode == "exe"u8: {
        return true;
    }
    case {} when buildmode == "pie"u8: {
        switch (ᐧ) {
        case {} when platform == "linux/386"u8 || platform == "linux/amd64"u8 || platform == "linux/arm"u8 || platform == "linux/arm64"u8 || platform == "linux/loong64"u8 || platform == "linux/ppc64le"u8 || platform == "linux/riscv64"u8 || platform == "linux/s390x"u8 || platform == "android/amd64"u8 || platform == "android/arm"u8 || platform == "android/arm64"u8 || platform == "android/386"u8 || platform == "freebsd/amd64"u8 || platform == "darwin/amd64"u8 || platform == "darwin/arm64"u8 || platform == "ios/amd64"u8 || platform == "ios/arm64"u8 || platform == "aix/ppc64"u8 || platform == "openbsd/arm64"u8 || platform == "windows/386"u8 || platform == "windows/amd64"u8 || platform == "windows/arm64"u8: {
            return true;
        }}
        return false;
    }
    case {} when buildmode == "shared"u8: {
        switch (ᐧ) {
        case {} when platform == "linux/386"u8 || platform == "linux/amd64"u8 || platform == "linux/arm"u8 || platform == "linux/arm64"u8 || platform == "linux/ppc64le"u8 || platform == "linux/s390x"u8: {
            return true;
        }}
        return false;
    }
    case {} when buildmode == "plugin"u8: {
        switch (ᐧ) {
        case {} when platform == "linux/amd64"u8 || platform == "linux/arm"u8 || platform == "linux/arm64"u8 || platform == "linux/386"u8 || platform == "linux/loong64"u8 || platform == "linux/riscv64"u8 || platform == "linux/s390x"u8 || platform == "linux/ppc64le"u8 || platform == "android/amd64"u8 || platform == "android/386"u8 || platform == "darwin/amd64"u8 || platform == "darwin/arm64"u8 || platform == "freebsd/amd64"u8: {
            return true;
        }}
        return false;
    }
    default: {
        return false;
    }}
}

public static bool InternalLinkPIESupported(@string goos, @string goarch) {
    var exprᴛ1 = goos + "/"u8 + goarch;
    switch (ᐧ) {
    case {} when exprᴛ1 == "android/arm64"u8 || exprᴛ1 == "darwin/amd64"u8 || exprᴛ1 == "darwin/arm64"u8 || exprᴛ1 == "linux/amd64"u8 || exprᴛ1 == "linux/arm64"u8 || exprᴛ1 == "linux/loong64"u8 || exprᴛ1 == "linux/ppc64le"u8 || exprᴛ1 == "windows/386"u8 || exprᴛ1 == "windows/amd64"u8 || exprᴛ1 == "windows/arm64"u8: {
        return true;
    }}
    return false;
}

// DefaultPIE reports whether goos/goarch produces a PIE binary when using the
// "default" buildmode. On Windows this is affected by -race,
// so force the caller to pass that in to centralize that choice.
public static bool DefaultPIE(@string goos, @string goarch, bool isRace) {
    switch (ᐧ) {
    case {} when goos == "android"u8 || goos == "ios"u8: {
        return true;
    }
    case {} when goos == "windows"u8: {
        if (isRace) {
            // PIE is not supported with -race on windows;
            // see https://go.dev/cl/416174.
            return false;
        }
        return true;
    }
    case {} when goos == "darwin"u8: {
        return true;
    }}
    return false;
}

// ExecutableHasDWARF reports whether the linked executable includes DWARF
// symbols on goos/goarch.
public static bool ExecutableHasDWARF(@string goos, @string goarch) {
    switch (ᐧ) {
    case {} when goos == "plan9"u8 || goos == "ios"u8: {
        return false;
    }}
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
