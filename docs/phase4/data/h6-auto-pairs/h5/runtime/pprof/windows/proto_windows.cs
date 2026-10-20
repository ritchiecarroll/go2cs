// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using errors = errors_package;
using windows = @internal.syscall.windows_package;
using os = os_package;
using syscall = syscall_package;
using @internal.syscall;

partial class pprof_package {

// go2cs generated this placeholder — func readMapping is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func readMainModuleMapping is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static (syscallꓸHandle, error) createModuleSnapshot() {
    while (ᐧ) {
        var (snap, err) = syscall.CreateToolhelp32Snapshot((uint32)((uint32)windows.TH32CS_SNAPMODULE | (uint32)windows.TH32CS_SNAPMODULE32), (uint32)syscall.Getpid());
        {
            var (errno, ok) = errors.AsType<syscall.Errno>(err); if (ok && errno == windows.ERROR_BAD_LENGTH) {
                // When CreateToolhelp32Snapshot(SNAPMODULE|SNAPMODULE32, ...) fails
                // with ERROR_BAD_LENGTH then it should be retried until it succeeds.
                continue;
            }
        }
        return (snap, err);
    }
}

} // end pprof_package
