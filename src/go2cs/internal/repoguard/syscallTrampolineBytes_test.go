// syscallTrampolineBytes_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"bytes"
	"crypto/sha256"
	"encoding/hex"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"strconv"
	"strings"
	"testing"
)

// syscall's windows/amd64 trampoline (docs/phase4/DESIGN-syscall-windows-amd64-trampoline.md) is machine code
// that a process maps executable: StdCallTrampoline.Image in src/core/syscall/windows/dll_windows.cs, the whole page
// (the code, its UNWIND_INFO and its RUNTIME_FUNCTION), with the GNU as source it was assembled from in the comment
// between ASM-BEGIN and ASM-END. A reader reviews the assembly, so the bytes must be exactly what it assembles to.
// Two checks:
//
//   - On every host, the bytes' SHA-256 equals trampolinePin, so a byte edit cannot land without this file
//     changing in the same diff.
//   - Where as and objcopy run and as assembles x86-64 (a one-line probe, GNU as on every linux runner), the
//     comment is assembled and its .text must equal the bytes. Elsewhere that half is NOT RUN, which is logged,
//     never read as a pass.
const trampolinePin = "b18b0ad28ddf0112e2acd035eba0c3c2345ebf652d616526437aa0b42f62b2e6"

var (
	trampolineAsm  = regexp.MustCompile(`(?s)// ASM-BEGIN\r?\n(.*?)// ASM-END`)
	trampolineCode = regexp.MustCompile(`(?s)internal static ReadOnlySpan<byte> Image => \[(.*?)\];`)
	hexByte        = regexp.MustCompile(`0x([0-9A-Fa-f]{2})`)
)

func TestSyscallTrampolineBytesAreTheirAssembly(t *testing.T) {
	root := repoRootFromPackageDir(t)
	path := filepath.Join(root, "src", "core", "syscall", "windows", "dll_windows.cs")

	data, err := os.ReadFile(path)
	if err != nil {
		t.Fatalf("cannot read %s: %v", path, err)
	}

	code, source := trampolineParts(t, string(data))

	sum := sha256.Sum256(code)
	if got := hex.EncodeToString(sum[:]); got != trampolinePin {
		t.Errorf("StdCallTrampoline.Image (%d bytes) has SHA-256 %s, pinned %s: a changed byte needs the assembly beside it and this pin changed in the same review", len(code), got, trampolinePin)
	}

	assembler, asErr := exec.LookPath("as")
	objcopy, objErr := exec.LookPath("objcopy")

	if asErr != nil || objErr != nil {
		t.Logf("NOT RUN: GNU as and objcopy are not both on PATH, so the assembly was not re-assembled here (the pin above still holds)")
		return
	}

	dir := t.TempDir()
	asmFile, objFile, binFile := filepath.Join(dir, "t.s"), filepath.Join(dir, "t.o"), filepath.Join(dir, "t.bin")
	probeFile, probeObj := filepath.Join(dir, "probe.s"), filepath.Join(dir, "probe.o")

	if err := os.WriteFile(probeFile, []byte(".intel_syntax noprefix\n.text\nnop\n"), 0o644); err != nil {
		t.Fatal(err)
	}

	if out, err := exec.Command(assembler, "--64", "-o", probeObj, probeFile).CombinedOutput(); err != nil {
		t.Logf("NOT RUN: %s does not assemble x86-64 here (%v: %s), so the assembly was not re-assembled (the pin above still holds)", assembler, err, bytes.TrimSpace(out))
		return
	}

	if err := os.WriteFile(asmFile, []byte(source), 0o644); err != nil {
		t.Fatal(err)
	}

	if out, err := exec.Command(assembler, "--64", "-o", objFile, asmFile).CombinedOutput(); err != nil {
		t.Fatalf("as did not assemble the trampoline's source: %v\n%s", err, out)
	}

	if out, err := exec.Command(objcopy, "-O", "binary", "--only-section=.text", objFile, binFile).CombinedOutput(); err != nil {
		t.Fatalf("objcopy did not extract the trampoline's .text: %v\n%s", err, out)
	}

	assembled, err := os.ReadFile(binFile)
	if err != nil {
		t.Fatal(err)
	}

	if !bytes.Equal(assembled, code) {
		t.Errorf("StdCallTrampoline.Image is not its assembly:\n  source assembles to (%d bytes) % x\n  the array holds    (%d bytes) % x", len(assembled), assembled, len(code), code)
	}
}

// trampolineParts returns the bytes of StdCallTrampoline.Image and the assembler source from the comment above it,
// with the comment markers removed.
func trampolineParts(t *testing.T, text string) ([]byte, string) {
	t.Helper()

	asm := trampolineAsm.FindStringSubmatch(text)
	if asm == nil {
		t.Fatal("dll_windows.cs has no ASM-BEGIN/ASM-END block")
	}

	var source strings.Builder

	for _, line := range strings.Split(strings.ReplaceAll(asm[1], "\r\n", "\n"), "\n") {
		line = strings.TrimSpace(line)

		if line == "" {
			continue
		}

		if !strings.HasPrefix(line, "//") {
			t.Fatalf("the ASM block holds a line that is not a comment: %q", line)
		}

		source.WriteString(strings.TrimSpace(strings.TrimPrefix(line, "//")) + "\n")
	}

	array := trampolineCode.FindStringSubmatch(text)
	if array == nil {
		t.Fatal("dll_windows.cs has no StdCallTrampoline.Image array")
	}

	var code []byte

	for _, m := range hexByte.FindAllStringSubmatch(array[1], -1) {
		v, err := strconv.ParseUint(m[1], 16, 8)
		if err != nil {
			t.Fatal(err)
		}

		code = append(code, byte(v))
	}

	if len(code) == 0 {
		t.Fatal("StdCallTrampoline.Image holds no bytes")
	}

	return code, source.String()
}
