// foreignFieldConsumer_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Seat #45 (class B, COORD's condition): what the converter emits where a consumer reads a FOREIGN struct's field
// whose type is a third package's, without importing that package: `resp.TLS != nil` on an *http.Response, or
// httptest.Server.TLS, in a package that does not import crypto/tls. P1 reads it through the generated property, as
// it reads any field of a served struct, and records the struct. Whether that compiles is the compile view's
// question, which go2cs-gen's arm answers under the SDK compiler (FieldAccessGeneratorTests,
// AConsumerOfAForeignFieldCompilesWhereItsTypeIsReferenced): a production project references its packages
// transitively, so the third assembly is in view and the property serves the site; a -tests project disables
// transitive references, and there no spelling compiles, the whole value's `.Value.TLS` included (CS0012).

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

func TestForeignFieldOfAnUnimportedTypeReadsThroughTheProperty(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/fieldaccess\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "tlsx", "tlsx.go"), "package tlsx\n\ntype Config struct{ MinVersion int }\n")
	writeModuleFile(t, filepath.Join(appDir, "srv", "srv.go"), `package srv

import "example.com/fieldaccess/tlsx"

type Server struct {
	Port int
	TLS  *tlsx.Config
}

func New(secure bool) *Server {
	if secure {
		return &Server{Port: 443, TLS: &tlsx.Config{MinVersion: 3}}
	}

	return &Server{Port: 80}
}
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"

	"example.com/fieldaccess/srv"
)

func secure(servers []*srv.Server) int {
	n := 0

	for _, s := range servers {
		if s.TLS != nil {
			n++
		}
	}

	return n
}

func main() {
	fmt.Println(secure([]*srv.Server{srv.New(true), srv.New(false)}), srv.New(true).TLS != nil)
}
`)

	out := convertFieldAccessModule(t, root)
	cs := readGenerated(t, filepath.Join(out, "main.cs"))
	info := readGenerated(t, filepath.Join(out, PackageInfoFileName))

	body := funcBody(t, cs, "nint secure(")
	t.Logf("secure: %s", body)

	if !strings.Contains(body, "if (s.TLS != nil) {") {
		t.Errorf("a foreign field of an unimported type does not read through the property:\n%s", body)
	}

	if main := funcBody(t, cs, "void Main("); !strings.Contains(main, "srv.New(true).TLS != nil") {
		t.Errorf("a call result's foreign field does not read through the property:\n%s", main)
	}

	if records := fieldAccessRecordsOf(info); strings.Join(records, " ") != "Server" {
		t.Errorf("package_info.cs records %v, want the foreign Server alone:\n%s", records, info)
	}

	// The compile view: the consumer references srv alone, and transitively, so tlsx is in view.
	projects, err := filepath.Glob(filepath.Join(out, "*.csproj"))

	if err != nil || len(projects) != 1 {
		t.Fatalf("want the consumer's one project in %s, got %v (%v)", out, projects, err)
	}

	project, err := os.ReadFile(projects[0])

	if err != nil {
		t.Fatal(err)
	}

	if !strings.Contains(string(project), "srv.csproj") || strings.Contains(string(project), "tlsx.csproj") {
		t.Errorf("the consumer must reference srv and not tlsx, which reaches it transitively:\n%s", project)
	}

	if strings.Contains(string(project), "DisableTransitiveProjectReferences") {
		t.Errorf("a production project's references are transitive; this one disables them:\n%s", project)
	}
}
