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
// invalid. This row measures, in a process built with CFG, which mapping leaves exactly offset 0 callable:
//
//   - default:    VirtualAlloc(PAGE_READWRITE), write, VirtualProtect(PAGE_EXECUTE_READ). The control: page+16 is
//     callable, so a termination in the invalid arm is the narrowing's doing, not the instrument's.
//   - invalid:    VirtualAlloc(PAGE_EXECUTE_READ | PAGE_TARGETS_INVALID), VirtualProtect(PAGE_READWRITE), write,
//     VirtualProtect(PAGE_EXECUTE_READ | PAGE_TARGETS_NO_UPDATE), then SetProcessValidCallTargets for offset 0 only.
//     The mapping the trampoline uses: page+0 callable, page+16 the CFG fast fail.
//   - both:       invalid with offsets 0 and 16 marked. The instrument's check that SetProcessValidCallTargets takes
//     effect: page+16 is callable once marked, so its termination in the invalid arm is the missing mark.
//   - rw-narrow:  VirtualAlloc(PAGE_READWRITE), write, VirtualProtect(PAGE_EXECUTE_READ | PAGE_TARGETS_NO_UPDATE), then
//     SetProcessValidCallTargets for offset 0. Recorded, not asserted (reading 4, 46083273402: the call fails with
//     error 998, ERROR_NOACCESS).
//   - plain:      default protection, then SetProcessValidCallTargets for offset 0. Recorded, not asserted (reading 4:
//     the call succeeds and page+16 stays callable).
//   - gpa:        invalid with SetProcessValidCallTargets called through GetProcAddress's pointer, a CFG-checked
//     indirect call, where every other arm calls the import directly. Recorded, not asserted (readings 2 to 4: the
//     process ends with a fast fail before the call returns).
//
// The row reads NOT MEASURED where the C compiler cannot build with CFG (-mguard=cf: clang, llvm-mingw's on the
// converter-cgo leg) or the process does not run with CFG on.
const cfgProbeSource = `#define SetProcessValidCallTargets go2cs_hidden_SetProcessValidCallTargets
#include <windows.h>
#include <stdio.h>
#include <string.h>
#undef SetProcessValidCallTargets

typedef struct { ULONG_PTR Offset; ULONG_PTR Flags; } go2cs_cfg_target;
typedef BOOL (WINAPI *go2cs_set_targets)(HANDLE, PVOID, SIZE_T, ULONG, go2cs_cfg_target *);

/* Imported directly (an import library the row builds), as the C# side's [LibraryImport] binds it: the "gpa" arm
   calls it through GetProcAddress's pointer instead, a CFG-checked indirect call. */
__declspec(dllimport) BOOL WINAPI SetProcessValidCallTargets(HANDLE, PVOID, SIZE_T, ULONG, go2cs_cfg_target *);
typedef BOOL (WINAPI *go2cs_get_policy)(HANDLE, int, PVOID, SIZE_T);
typedef int (*go2cs_entry)(void);

#define GO2CS_TARGETS 0x40000000 /* PAGE_TARGETS_INVALID for VirtualAlloc, PAGE_TARGETS_NO_UPDATE for VirtualProtect */

static void fail(const char *step) { printf("step-failed: %s %lu\n", step, GetLastError()); fflush(stdout); ExitProcess(3); }

int main(int argc, char **argv) {
    const char *arm = argc > 1 ? argv[1] : "";
    HMODULE kernelbase = LoadLibraryW(L"kernelbase.dll");
    go2cs_set_targets gpa_targets = (go2cs_set_targets)GetProcAddress(kernelbase, "SetProcessValidCallTargets");
    go2cs_get_policy get_policy = (go2cs_get_policy)GetProcAddress(GetModuleHandleW(L"kernel32.dll"), "GetProcessMitigationPolicy");
    DWORD policy = 0, old;
    SYSTEM_INFO si;
    unsigned char *page;
    volatile go2cs_entry entry;
    BOOL narrow = strcmp(arm, "default") != 0 && strcmp(arm, "plain") != 0;
    BOOL mark = strcmp(arm, "default") != 0;
    BOOL ok;

    if (get_policy == NULL || !get_policy(GetCurrentProcess(), 7 /* ProcessControlFlowGuardPolicy */, &policy, sizeof policy)) fail("GetProcessMitigationPolicy");
    printf("cfg: %lu\n", policy & 1);
    fflush(stdout);
    if ((policy & 1) == 0) return 2;

    GetSystemInfo(&si);

    if (strcmp(arm, "invalid") == 0 || strcmp(arm, "both") == 0 || strcmp(arm, "gpa") == 0) {
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

    if (mark) {
        /* "both" marks offset 16 too: the instrument's check that SetProcessValidCallTargets takes effect */
        go2cs_cfg_target targets[2] = { { 0, 1 /* CFG_CALL_TARGET_VALID */ }, { 16, 1 } };
        ULONG count = strcmp(arm, "both") == 0 ? 2 : 1;
        printf("calling SetProcessValidCallTargets\n");
        fflush(stdout);
        /* A direct call through the import (not CFG-checked) in every arm but "gpa", whose call through
           GetProcAddress's pointer is a CFG-checked indirect call. */
        if (strcmp(arm, "gpa") == 0)
            ok = gpa_targets != NULL && gpa_targets(GetCurrentProcess(), page, si.dwPageSize, count, targets);
        else
            ok = SetProcessValidCallTargets(GetCurrentProcess(), page, si.dwPageSize, count, targets);
        if (!ok) fail("SetProcessValidCallTargets");
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

	// The import library for kernelbase's SetProcessValidCallTargets, from the dlltool beside the compiler.
	dlltool := strings.TrimSuffix(toolchain.cc[0], filepath.Ext(toolchain.cc[0]))
	dlltool = strings.TrimSuffix(dlltool, "clang") + "dlltool" + filepath.Ext(toolchain.cc[0])
	def := writeCgoFixture(t, dir, "kernelbase.def", "LIBRARY kernelbase.dll\nEXPORTS\nSetProcessValidCallTargets\n")
	implib := filepath.Join(dir, "libgo2cskernelbase.a")

	if out, err := exec.Command(dlltool, "-m", "i386:x86-64", "-d", def, "-l", implib).CombinedOutput(); err != nil {
		t.Skipf("NOT MEASURED: no dlltool beside %s to import SetProcessValidCallTargets (%v)\n%s", toolchain.cc[0], err, out)
	}

	args := append(slices.Clone(toolchain.cc[1:]), "-O1", "-mguard=cf", "-o", probe, source, implib)

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

	// Recorded, not asserted: the two mappings that do not narrow, and the call through GetProcAddress's pointer.
	run("rw-narrow")
	run("plain")
	run("gpa")

	text, code := run("default")

	if code == 2 && strings.Contains(text, "cfg: 0") {
		t.Skip("NOT MEASURED: the probe was built with -mguard=cf but does not run with Control Flow Guard on")
	}

	if code != 0 || !strings.Contains(text, "page+0: 7\n") || !strings.Contains(text, "page+16: 16\n") {
		t.Fatalf("control (default protection): page+0 and page+16 should both be callable; read exit %#x, %q", code, text)
	}

	text, code = run("both")

	if code != 0 || !strings.Contains(text, "page+0: 7\n") || !strings.Contains(text, "page+16: 16\n") {
		t.Fatalf("both (offsets 0 and 16 marked): page+0 and page+16 should both be callable; read exit %#x, %q", code, text)
	}

	text, code = run("invalid")

	if !strings.Contains(text, "page+0: 7\n") {
		t.Fatalf("invalid: page+0 should be callable; read exit %#x, %q", code, text)
	}

	if strings.Contains(text, "page+16:") || code != statusStackBufferOverrun {
		t.Errorf("invalid: a call to page+16 should end the process with the CFG fast fail (%#x); read exit %#x, %q",
			statusStackBufferOverrun, code, text)
	}
}
