// asmForwardBodies_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// Guards asmForwardBodies (sqlite-cs S21): golang.org/x/sys/cpu's cpuid and xgetbv, assembly in Go, forward
// to golib's GoCpu when their signatures are the table's; any other signature refuses the package
// (GO2CS2011); and an assembly function nothing forwards keeps its stub and is named (GO2CS2012). The
// fixtures are a module named golang.org/x/sys, so the package path is x/sys/cpu's.

const asmForwardCpuGo = `package cpu

func cpuid(eaxArg, ecxArg uint32) (eax, ebx, ecx, edx uint32)

func xgetbv() (eax, edx uint32)

func unlisted() uint32

func Read() uint32 {
	maxID, _, _, _ := cpuid(0, 0)
	xcr0, _ := xgetbv()
	return maxID + xcr0 + unlisted()
}
`

const asmForwardCpuAsm = `#include "textflag.h"

TEXT ·cpuid(SB), NOSPLIT, $0-24
	RET

TEXT ·xgetbv(SB), NOSPLIT, $0-8
	RET

TEXT ·unlisted(SB), NOSPLIT, $0-4
	RET
`

const asmForwardMainGo = `package main

import (
	"fmt"

	"golang.org/x/sys/cpu"
)

func main() { fmt.Println(cpu.Read()) }
`

func TestAsmForwardBodiesEmission(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module golang.org/x/sys\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), asmForwardMainGo)
	writeModuleFile(t, filepath.Join(appDir, "cpu", "cpu.go"), asmForwardCpuGo)
	writeModuleFile(t, filepath.Join(appDir, "cpu", "cpu_amd64.s"), asmForwardCpuAsm)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      "windows/amd64",
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	var convertErr error

	stderr := captureStderr(t, func() {
		convertErr = NewModuleConverter(options).ConvertModule(appDir)
	})

	if convertErr != nil {
		t.Fatalf("ConvertModule: %v", convertErr)
	}

	cpuCs := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "golang.org", "x", "sys", "cpu", "cpu.cs")), "\r\n", "\n")

	for name, call := range map[string]string{
		"cpuid":  "global::go.golib.GoCpu.Cpuid(eaxArg, ecxArg)",
		"xgetbv": "global::go.golib.GoCpu.Xgetbv()",
	} {
		if forwarder := emittedFunction(t, cpuCs, name); !strings.Contains(forwarder, call) {
			t.Errorf("%s does not forward to %s:\n%s", name, call, forwarder)
		}
	}

	if !strings.Contains(cpuCs, "internal static partial uint32 unlisted();") {
		t.Errorf("an assembly function the table does not list must keep its partial declaration (its stub):\n%s", cpuCs)
	}

	if !strings.Contains(stderr, "warning GO2CS2012: golang.org/x/sys/cpu.unlisted has no Go body") {
		t.Errorf("the stub left for an unlisted assembly function must be named as GO2CS2012:\n%s", stderr)
	}

	for _, name := range []string{"cpuid", "xgetbv"} {
		if strings.Contains(stderr, "golang.org/x/sys/cpu."+name+" has no Go body") {
			t.Errorf("%s is forwarded, so it must not be named as a stub:\n%s", name, stderr)
		}
	}
}

// The sibling rows (zeebo/xxh3): a fixture module named github.com/zeebo/xxh3 whose two NEON functions are assembly
// with no body, and whose scalar siblings are Go with identical signatures (`ptr` an alias of unsafe.Pointer, as in
// xxh3 v1.1.0's utils.go).
const asmSiblingXxh3Go = `package xxh3

import "unsafe"

type (
	u64 = uint64
	ptr = unsafe.Pointer
)

func accumNEON(acc *[8]u64, data, key unsafe.Pointer, len u64)

func accumBlockNEON(acc *[8]u64, data, key unsafe.Pointer)

func accumScalar(accs *[8]u64, p, secret ptr, l u64) { accs[0] += l }

func accumBlockScalar(accs *[8]u64, p, secret ptr) { accs[1]++ }

func Sum(n u64) u64 {
	var acc [8]u64
	var block [64]byte
	accumNEON(&acc, unsafe.Pointer(&block), unsafe.Pointer(&block), n)
	accumBlockNEON(&acc, unsafe.Pointer(&block), unsafe.Pointer(&block))
	return acc[0] + acc[1]
}
`

const asmSiblingXxh3Asm = `#include "textflag.h"

TEXT ·accumNEON(SB), NOSPLIT, $0-40
	RET

TEXT ·accumBlockNEON(SB), NOSPLIT, $0-24
	RET
`

func TestAsmSiblingForwardEmission(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module github.com/zeebo/xxh3\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "xxh3.go"), asmSiblingXxh3Go)
	writeModuleFile(t, filepath.Join(appDir, "accum_amd64.s"), asmSiblingXxh3Asm)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      "windows/amd64",
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	var convertErr error

	stderr := captureStderr(t, func() {
		convertErr = NewModuleConverter(options).ConvertModule(appDir)
	})

	if convertErr != nil {
		t.Fatalf("ConvertModule: %v", convertErr)
	}

	xxh3Cs := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "github.com", "zeebo", "xxh3", "xxh3.cs")), "\r\n", "\n")

	// A forwarder is the partial's implementing declaration, with a body: `{ accumScalar(acc, data, key, len); }`.
	for name, call := range map[string]string{"accumNEON": "accumScalar(acc, data, key, len);", "accumBlockNEON": "accumBlockScalar(acc, data, key);"} {
		forwarder := emittedFunction(t, xxh3Cs, name)

		if !strings.Contains(forwarder, call) {
			t.Errorf("%s must forward to its same-package sibling, `%s`, not stay a bodyless stub:\n%s", name, call, forwarder)
		}

		if strings.Contains(stderr, "github.com/zeebo/xxh3."+name+" has no Go body") {
			t.Errorf("%s is forwarded, so it must not be named as a stub (GO2CS2012):\n%s", name, stderr)
		}
	}
}

func TestAsmSiblingForwardIsRefusedBySite(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	write := func(xxh3Go string) string {
		dir := t.TempDir()

		writeModuleFiles(t, dir, map[string]string{
			"go.mod":        "module github.com/zeebo/xxh3\n\ngo 1.23\n",
			"xxh3.go":       xxh3Go,
			"accum_amd64.s": asmSiblingXxh3Asm,
		})

		return dir
	}

	if refusal := refuseAsmForwardSignatures(loadProductionForDir(t, write(asmSiblingXxh3Go))); refusal != nil {
		t.Fatalf("identical siblings were refused: %v", refusal)
	}

	// accumScalar drops a parameter, and accumBlockScalar is gone: both rows must be refused, each by name.
	wrong := strings.Replace(asmSiblingXxh3Go, "func accumScalar(accs *[8]u64, p, secret ptr, l u64) { accs[0] += l }", "func accumScalar(accs *[8]u64, p ptr, l u64) { accs[0] += l }", 1)
	wrong = strings.Replace(wrong, "func accumBlockScalar(accs *[8]u64, p, secret ptr) { accs[1]++ }\n", "", 1)

	refusal := refuseAsmForwardSignatures(loadProductionForDir(t, write(wrong)))

	if refusal == nil {
		t.Fatal("a sibling with another signature, and a missing sibling, were not refused")
	}

	if len(refusal.sites) != 2 {
		t.Fatalf("want exactly two refused sites (accumNEON, accumBlockNEON), got %d: %v", len(refusal.sites), refusal.sites)
	}

	texts := refusal.sites[0].text + "\n" + refusal.sites[1].text

	// Signatures are spelled as go/types prints them, aliases by name (github.com/zeebo/xxh3.u64).
	for _, want := range []string{
		"github.com/zeebo/xxh3.accumNEON is assembly that go2cs forwards to its same-package sibling accumScalar, which is func(*[8]github.com/zeebo/xxh3.u64, github.com/zeebo/xxh3.ptr, github.com/zeebo/xxh3.u64); this declaration is func(",
		"github.com/zeebo/xxh3.accumBlockNEON is assembly that go2cs forwards to its same-package sibling accumBlockScalar, which this package does not declare",
	} {
		if !strings.Contains(texts, want) {
			t.Errorf("the refusal must say %q:\n%s", want, texts)
		}
	}
}

func TestAsmForwardSignatureIsRefusedBySite(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	write := func(cpuGo string) string {
		dir := t.TempDir()

		writeModuleFiles(t, dir, map[string]string{
			"go.mod":           "module golang.org/x/sys\n\ngo 1.23\n",
			"cpu/cpu.go":       cpuGo,
			"cpu/cpu_amd64.s":  asmForwardCpuAsm,
			"cpu/read_test.go": "package cpu\n",
		})

		return filepath.Join(dir, "cpu")
	}

	if refusal := refuseAsmForwardSignatures(loadProductionForDir(t, write(asmForwardCpuGo))); refusal != nil {
		t.Fatalf("the table's own signatures were refused: %v", refusal)
	}

	wrong := strings.Replace(asmForwardCpuGo, "func cpuid(eaxArg, ecxArg uint32) (eax, ebx, ecx, edx uint32)", "func cpuid(eaxArg uint32) uint32", 1)
	wrong = strings.Replace(wrong, "maxID, _, _, _ := cpuid(0, 0)", "maxID := cpuid(0)", 1)

	refusal := refuseAsmForwardSignatures(loadProductionForDir(t, write(wrong)))

	if refusal == nil {
		t.Fatal("cpuid declared as func(uint32) uint32 was not refused; golib's GoCpu.Cpuid answers another signature")
	}

	if len(refusal.sites) != 1 {
		t.Fatalf("want exactly one refused site (cpuid), got %d: %v", len(refusal.sites), refusal.sites)
	}

	text := refusal.sites[0].text

	for _, want := range []string{"golang.org/x/sys/cpu.cpuid", "go.golib.GoCpu.Cpuid", "func(uint32, uint32) (uint32, uint32, uint32, uint32)", "func(uint32) uint32"} {
		if !strings.Contains(text, want) {
			t.Errorf("the refusal must name %q: %s", want, text)
		}
	}
}
