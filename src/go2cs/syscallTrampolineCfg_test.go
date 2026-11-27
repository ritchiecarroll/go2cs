// syscallTrampolineCfg_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"errors"
	"os/exec"
	"path/filepath"
	"runtime"
	"slices"
	"strings"
	"testing"
)

// syscall's windows/amd64 trampoline (docs/phase4/DESIGN-syscall-windows-amd64-trampoline.md, section 6) is one page
// made executable by VirtualProtect, and under Control Flow Guard only its entry, offset 0, may be an indirect-call
// target. Windows marks every location of a page valid when VirtualProtect makes it executable, unless the protection
// carries PAGE_TARGETS_NO_UPDATE; Microsoft documents that a PAGE_TARGETS_INVALID allocation starts with every location
// invalid, and does not document what a PAGE_READWRITE allocation starts with. This row measures it, in a process built
// with CFG, for three ways to map the page:
//
//   - default:    VirtualAlloc(PAGE_READWRITE), write, VirtualProtect(PAGE_EXECUTE_READ). The control: page+16 is
//     callable, so a termination in the other arms is the narrowing's doing, not the instrument's.
//   - rw-narrow:  VirtualAlloc(PAGE_READWRITE), write, VirtualProtect(PAGE_EXECUTE_READ | PAGE_TARGETS_NO_UPDATE), then
//     SetProcessValidCallTargets for offset 0 only.
//   - invalid:    VirtualAlloc(PAGE_EXECUTE_READ | PAGE_TARGETS_INVALID), VirtualProtect(PAGE_READWRITE), write,
//     VirtualProtect(PAGE_EXECUTE_READ | PAGE_TARGETS_NO_UPDATE), then SetProcessValidCallTargets for offset 0 only.
//   - both:       rw-narrow with offsets 0 and 16 marked. Recorded, not asserted: whether SetProcessValidCallTargets
//     takes effect at all.
//
// In every arm page+0 must be callable. In a narrowed arm a call to page+16 must end the process with the CFG fast
// fail (STATUS_STACK_BUFFER_OVERRUN). The row reads NOT MEASURED where the C compiler cannot build with CFG
// (-mguard=cf: clang, llvm-mingw's on the converter-cgo leg) or the process does not run with CFG on.
const cfgProbeSource = `#include <windows.h>
#include <stdio.h>
#include <string.h>

typedef struct { ULONG_PTR Offset; ULONG_PTR Flags; } go2cs_cfg_target;
typedef BOOL (WINAPI *go2cs_set_targets)(HANDLE, PVOID, SIZE_T, ULONG, go2cs_cfg_target *);
typedef BOOL (WINAPI *go2cs_get_policy)(HANDLE, int, PVOID, SIZE_T);
typedef int (*go2cs_entry)(void);

#define GO2CS_TARGETS 0x40000000 /* PAGE_TARGETS_INVALID for VirtualAlloc, PAGE_TARGETS_NO_UPDATE for VirtualProtect */

static void fail(const char *step) { printf("step-failed: %s %lu\n", step, GetLastError()); fflush(stdout); ExitProcess(3); }

int main(int argc, char **argv) {
    const char *arm = argc > 1 ? argv[1] : "";
    HMODULE kernelbase = LoadLibraryW(L"kernelbase.dll");
    go2cs_set_targets set_targets = (go2cs_set_targets)GetProcAddress(kernelbase, "SetProcessValidCallTargets");
    go2cs_get_policy get_policy = (go2cs_get_policy)GetProcAddress(GetModuleHandleW(L"kernel32.dll"), "GetProcessMitigationPolicy");
    DWORD policy = 0, old;
    SYSTEM_INFO si;
    unsigned char *page;
    volatile go2cs_entry entry;
    BOOL narrow = strcmp(arm, "default") != 0;

    if (get_policy == NULL || !get_policy(GetCurrentProcess(), 7 /* ProcessControlFlowGuardPolicy */, &policy, sizeof policy)) fail("GetProcessMitigationPolicy");
    printf("cfg: %lu\n", policy & 1);
    fflush(stdout);
    if ((policy & 1) == 0) return 2;

    GetSystemInfo(&si);

    if (strcmp(arm, "invalid") == 0) {
        page = VirtualAlloc(NULL, si.dwPageSize, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READ | GO2CS_TARGETS);
        if (page == NULL) fail("VirtualAlloc");
        if (!VirtualProtect(page, si.dwPageSize, PAGE_READWRITE, &old)) fail("VirtualProtect-rw");
    } else {
        page = VirtualAlloc(NULL, si.dwPageSize, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        if (page == NULL) fail("VirtualAlloc");
    }

    /* mov eax, 7; ret at offset 0, and mov eax, 16; ret at offset 16 */
    memcpy(page, "\xB8\x07\x00\x00\x00\xC3", 6);
    memcpy(page + 16, "\xB8\x10\x00\x00\x00\xC3", 6);

    if (!VirtualProtect(page, si.dwPageSize, PAGE_EXECUTE_READ | (narrow ? GO2CS_TARGETS : 0), &old)) fail("VirtualProtect-rx");
    FlushInstructionCache(GetCurrentProcess(), page, si.dwPageSize);
    printf("protected: %p\n", (void *)page);
    fflush(stdout);

    if (narrow) {
        /* "both" marks offset 16 too: the instrument's check that SetProcessValidCallTargets takes effect */
        go2cs_cfg_target targets[2] = { { 0, 1 /* CFG_CALL_TARGET_VALID */ }, { 16, 1 } };
        ULONG count = strcmp(arm, "both") == 0 ? 2 : 1;
        if (set_targets == NULL || !set_targets(GetCurrentProcess(), page, si.dwPageSize, count, targets)) fail("SetProcessValidCallTargets");
        printf("targets: %lu, flags %llu\n", count, (unsigned long long)targets[0].Flags);
        fflush(stdout);
    }

    entry = (go2cs_entry)page;
    printf("page+0: %d\n", entry());
    fflush(stdout);
    entry = (go2cs_entry)(page + 16);
    printf("page+16: %d\n", entry());
    fflush(stdout);
    return 0;
}
`

// statusStackBufferOverrun is the exit status of a CFG fast fail (__fastfail(FAST_FAIL_GUARD_ICALL_CHECK_FAILURE)).
const statusStackBufferOverrun = 0xC0000409

func TestCgoSyscallTrampolinePageCallTargets(t *testing.T) {
	if runtime.GOOS != "windows" || runtime.GOARCH != "amd64" {
		t.Skipf("the trampoline is windows/amd64 only: NOT MEASURED on %s/%s", runtime.GOOS, runtime.GOARCH)
	}

	toolchain := cgoCaptureToolchain(t)
	dir := t.TempDir()
	source := writeCgoFixture(t, dir, "cfgprobe.c", cfgProbeSource)
	probe := filepath.Join(dir, "cfgprobe.exe")

	args := append(slices.Clone(toolchain.cc[1:]), "-O1", "-mguard=cf", "-o", probe, source)

	if out, err := exec.Command(toolchain.cc[0], args...).CombinedOutput(); err != nil {
		t.Skipf("NOT MEASURED: %v cannot build with Control Flow Guard (-mguard=cf): %v\n%s", toolchain.cc, err, out)
	}

	run := func(arm string) (string, uint32) {
		out, err := exec.Command(probe, arm).CombinedOutput()
		text := strings.ReplaceAll(string(out), "\r\n", "\n")
		t.Logf("READING %s: %q (%v)", arm, text, err)

		var exit *exec.ExitError

		switch {
		case err == nil:
			return text, 0
		case errors.As(err, &exit):
			return text, uint32(exit.ExitCode())
		default:
			t.Fatalf("the probe did not run (%s): %v", arm, err)
			return "", 0
		}
	}

	text, code := run("default")

	if code == 2 && strings.Contains(text, "cfg: 0") {
		t.Skip("NOT MEASURED: the probe was built with -mguard=cf but does not run with Control Flow Guard on")
	}

	if code != 0 || !strings.Contains(text, "page+0: 7\n") || !strings.Contains(text, "page+16: 16\n") {
		t.Fatalf("control (default protection): page+0 and page+16 should both be callable; read exit %#x, %q", code, text)
	}

	for _, arm := range []string{"rw-narrow", "invalid"} {
		text, code := run(arm)

		if !strings.Contains(text, "page+0: 7\n") {
			t.Errorf("%s: page+0 should be callable; read exit %#x, %q", arm, code, text)
			continue
		}

		if strings.Contains(text, "page+16:") || code != statusStackBufferOverrun {
			t.Errorf("%s: a call to page+16 should end the process with the CFG fast fail (%#x); read exit %#x, %q", arm,
				statusStackBufferOverrun, code, text)
		}
	}
}
