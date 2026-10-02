// linknameForwardDarwin_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/build"
	"go/parser"
	"go/token"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestDarwinSyscallPullsForward guards the eight darwin //go:linkname pulls of syscall that had nothing
// behind them until S7 (2026-10-02). Each is a bodyless declaration in a darwin-built file whose
// directive names an ORDINARY CONVERTED Go function in syscall that syscall authorizes with a one-arg
// handle (linkname_darwin.go, linkname_bsd.go, linkname_libc.go). Without a linknameForwardTargets row
// the converter emitted each as a bodyless partial that PartialStubGenerator filled with a throw, and the
// 1.24.13 darwin re-baseline (run 36947612442) lost three projects on both mac legs to three of them.
//
// The test reads each pull from GOROOT, from the exact file Go declares it in, and drives the converter's
// own recognizer (funcLinknameForward) over that declaration: the pull must resolve to a forwarder calling
// the same-named syscall function. TestLinknameForwardTargetsMatchGoSource separately verifies each row's
// target has a body and a handle; this test is the other half, that the PULLER's real declaration reaches
// the row (its directive is in the doc comment funcLinknameForward reads, and its name matches).
func TestDarwinSyscallPullsForward(t *testing.T) {
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	if goRoot == "" {
		t.Skip("GOROOT not resolvable; nothing to read the pulls from")
	}

	pulls := []struct{ file, local, target string }{
		{"internal/poll/fd_opendir_darwin.go", "fdopendir", "syscall.fdopendir"},
		{"internal/poll/fd_writev_libc.go", "writev", "syscall.writev"},
		{"internal/syscall/unix/at_libc2.go", "unlinkat", "syscall.unlinkat"},
		{"internal/syscall/unix/at_libc2.go", "openat", "syscall.openat"},
		{"internal/syscall/unix/at_libc2.go", "fstatat", "syscall.fstatat"},
		{"internal/syscall/unix/tcsetpgrp_bsd.go", "ioctlPtr", "syscall.ioctlPtr"},
		{"os/dir_darwin.go", "closedir", "syscall.closedir"},
		{"vendor/golang.org/x/net/route/syscall.go", "sysctl", "syscall.sysctl"},
	}

	for _, pull := range pulls {
		path := filepath.Join(goRoot, "src", filepath.FromSlash(pull.file))
		file, err := parser.ParseFile(token.NewFileSet(), path, nil, parser.ParseComments)

		if err != nil {
			t.Errorf("%s: %v", pull.file, err)
			continue
		}

		var decl *ast.FuncDecl

		for _, d := range file.Decls {
			if funcDecl, isFunc := d.(*ast.FuncDecl); isFunc && funcDecl.Recv == nil && funcDecl.Name.Name == pull.local && funcDecl.Body == nil {
				decl = funcDecl
			}
		}

		if decl == nil {
			t.Errorf("%s: no bodyless func %s -- the pull moved or gained a body at this pin, so this row's premise is stale", pull.file, pull.local)
			continue
		}

		v := &Visitor{importQueue: HashSet[string]{}, importPathAliases: map[string]string{"syscall": "syscall"}}
		alias, targetFunc, ok := v.funcLinknameForward(decl)

		if !ok {
			t.Errorf("%s: %s -> %s does not forward: it converts to a bodyless partial that PartialStubGenerator fills with a throw. Add %q to linknameForwardTargets",
				pull.file, pull.local, pull.target, pull.target)
			continue
		}

		if want := pull.target[strings.LastIndex(pull.target, ".")+1:]; alias != "syscall" || targetFunc != want {
			t.Errorf("%s: %s forwards to %s.%s, want syscall.%s", pull.file, pull.local, alias, targetFunc, want)
		}
	}
}
