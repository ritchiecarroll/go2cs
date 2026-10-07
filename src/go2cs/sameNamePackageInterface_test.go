// sameNamePackageInterface_test.go - Gbtc
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

// A module package that shares its NAME with a standard-library package, and declares a type with the
// same simple name as one of that package's: logrus' hooks/slog is `package slog` and declares
// `Handler`, whose pointer implements log/slog's `Handler` interface. log/slog publishes the alias
// `slog.Handler` -> ΔHandler (its Handler collides with a method), and the type-name renderer took that
// alias for hooks/slog's type too, keyed by package name alone. The conversion `slog.New(h)` then
// compared the interface's name with what it took for the argument's, found them equal and emitted an
// identity (`slog.New(~h)`) with no GoImplement record (CS1503). The argument must render as its own
// package's type and the conversion must be recorded and emitted like any other.
func TestSameNamedPackageTypeConvertsToTheStdInterface(t *testing.T) {
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/samehandler\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "handler.go"), `package slog

import (
	"context"
	"log/slog"
)

type Handler struct{ n int }

func NewHandler() *Handler { return &Handler{} }

func (h *Handler) Enabled(context.Context, slog.Level) bool { return true }

func (h *Handler) Handle(_ context.Context, r slog.Record) error { h.n++; return nil }

func (h *Handler) WithAttrs([]slog.Attr) slog.Handler { return h }

func (h *Handler) WithGroup(string) slog.Handler { return h }

func (h *Handler) Count() int { return h.n }

type Sink struct{ n int }

func NewSink() *Sink { return &Sink{} }

func (s *Sink) Enabled(context.Context, slog.Level) bool { return true }

func (s *Sink) Handle(context.Context, slog.Record) error { s.n++; return nil }

func (s *Sink) WithAttrs([]slog.Attr) slog.Handler { return s }

func (s *Sink) WithGroup(string) slog.Handler { return s }

type Writer struct{ n int }

func NewWriter() *Writer { return &Writer{} }

func (w *Writer) Write(p []byte) (int, error) { w.n += len(p); return len(p), nil }
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"io"
	"log/slog"

	lslog "example.com/samehandler/lib"
)

func use(w io.Writer) { fmt.Fprint(w, "x") }

func main() {
	h := lslog.NewHandler()
	s := slog.New(lslog.NewHandler())
	t := slog.New(h)
	u := slog.New(lslog.NewSink())
	s.Info("x")
	t.Info("y")
	u.Info("z")
	use(lslog.NewWriter())
	fmt.Println(h.Count())
}
`)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	outDir := filepath.Join(options.go2csPath, "src", "example.com", "samehandler")
	mainCs := readGenerated(t, filepath.Join(outDir, "main.cs"))
	packageInfo := readGenerated(t, filepath.Join(outDir, "package_info.cs"))

	// The same-named type: never an identity conversion to log/slog's interface.
	for _, wrong := range []string{"slog.New(~h)", "slog.New(~lslog.NewHandler())"} {
		if strings.Contains(mainCs, wrong) {
			t.Errorf("hooks/slog's *Handler converts to log/slog.Handler as an identity (%q) in:\n%s", wrong, mainCs)
		}
	}

	// It converts exactly as the CONTROL does -- a differently named type (Sink) to the same interface --
	// through the adapter its own package declares (it returns itself as a slog.Handler); and the other
	// control, a same-package type (Writer) to an interface of another name, keeps its local record.
	for _, want := range []string{
		"slog.New(new lslog.HandlerжΔHandler(lslog.NewHandler()))",
		"slog.New(new lslog.HandlerжΔHandler(h))",
		"slog.New(new lslog.SinkжΔHandler(lslog.NewSink()))",
		"use(new slog_WriterжWriter(lslog.NewWriter()))",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("missing %q in:\n%s", want, mainCs)
		}
	}

	if !strings.Contains(packageInfo, "GoImplement<go.example.com.samehandler.lib.slog_package.Writer, io_package.Writer>(Pointer = true)") {
		t.Errorf("missing the Writer control's record in:\n%s", packageInfo)
	}

	if strings.Contains(packageInfo, "log.slog_package.ΔHandler, ") {
		t.Errorf("a record names log/slog's interface as the CONCRETE type in:\n%s", packageInfo)
	}
}
