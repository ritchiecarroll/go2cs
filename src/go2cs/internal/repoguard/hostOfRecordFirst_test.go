// hostOfRecordFirst_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

// The host-of-record warning (golib's GoHostOfRecord, called from src/core/runtime/hostofrecord_impl.cs) names a
// standard library built for another platform than the one it runs on. docs/ConvertingAModule.md promises that a
// program built for the wrong flavour stops at start-up with that warning. C# does not order one module initializer
// against another, and goenvs_impl.cs's ᴛInitEnvs, schedinit's start-up steps, reaches the platform first: a Windows
// build run on Linux died "GOMAXPROCS at startup: System.DllNotFoundException: ... kernel32.dll" and the warning
// never printed (measured at d6805be2e8, the .NET 11 usability pass, 2026-11-26). So the check is not a module
// initializer of its own: it is ᴛInitEnvs's FIRST statement, ahead of every start-up step that can fail. This guard
// holds both halves.
var (
	initEnvsBody      = regexp.MustCompile(`(?s)internal static void ᴛInitEnvs\(\)\s*\{(.*)`)
	verifyHostMethod  = regexp.MustCompile(`(?s)(\[ModuleInitializer\]\s*)?internal static void ᴛVerifyHostOfRecord\(\)`)
	csLineComment     = regexp.MustCompile(`(?m)^\s*//.*$`)
	firstCSStatement  = regexp.MustCompile(`^\s*([^;{}]*;)`)
	verifyHostCallStr = "ᴛVerifyHostOfRecord();"
)

func TestHostOfRecordRunsFirstAtStartup(t *testing.T) {
	root := repoRootFromPackageDir(t)
	runtimeDir := filepath.Join(root, "src", "core", "runtime")

	goenvs, err := os.ReadFile(filepath.Join(runtimeDir, "goenvs_impl.cs"))
	if err != nil {
		t.Fatalf("cannot read goenvs_impl.cs: %v", err)
	}

	body := initEnvsBody.FindStringSubmatch(string(goenvs))
	if body == nil {
		t.Fatalf("goenvs_impl.cs declares no ᴛInitEnvs(); this guard names the start-up initializer by that name")
	}

	first := firstCSStatement.FindStringSubmatch(csLineComment.ReplaceAllString(body[1], ""))
	if first == nil || strings.TrimSpace(first[1]) != verifyHostCallStr {
		got := "(none)"
		if first != nil {
			got = strings.TrimSpace(first[1])
		}
		t.Errorf("ᴛInitEnvs's first statement is %q; want %q, so the host-of-record warning prints before any start-up step that can fail on the wrong platform", got, verifyHostCallStr)
	}

	host, err := os.ReadFile(filepath.Join(runtimeDir, "hostofrecord_impl.cs"))
	if err != nil {
		t.Fatalf("cannot read hostofrecord_impl.cs: %v", err)
	}

	method := verifyHostMethod.FindStringSubmatch(string(host))
	if method == nil {
		t.Fatalf("hostofrecord_impl.cs declares no internal static void ᴛVerifyHostOfRecord()")
	}

	if method[1] != "" {
		t.Errorf("ᴛVerifyHostOfRecord is a [ModuleInitializer]; it must be called from ᴛInitEnvs instead, because C# does not order one module initializer against another")
	}
}
