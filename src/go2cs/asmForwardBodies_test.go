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
