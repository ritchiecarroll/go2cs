package main

// PROBE, never landed (claude/h-cgo-res-probe only). Increment 3.4's first reading, P8 of DESIGN-cgo-phase3-marshaling
// section 9: what does `go2cs -cgo` say about GOROOT's net, cgo on, for the leg's own target? It logs and always passes.

import (
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

func TestZzCgoNetProbe(t *testing.T) {
	goRoot, err := exec.Command("go", "env", "GOROOT").Output()

	if err != nil {
		t.Skipf("go env GOROOT: %v", err)
	}

	dir := filepath.Join(strings.TrimSpace(string(goRoot)), "src", "net")
	stderr, output, err := convertCgoGateFixture(t, dir, []string{"-cgo=true"}, "CGO_ENABLED=1")

	t.Logf("PROBE exit: %v", err)

	for _, line := range strings.Split(stderr, "\n") {
		t.Logf("PROBE stderr: %s", line)
	}

	emitted := emittedProjectFiles(t, output)
	t.Logf("PROBE emitted %d .cs/.csproj files", len(emitted))

	for _, name := range emitted {
		if strings.Contains(name, "cgo") {
			t.Logf("PROBE cgo file: %s", name)

			if body, readErr := os.ReadFile(filepath.Join(output, name)); readErr == nil {
				for i, line := range strings.Split(string(body), "\n") {
					if i >= 400 {
						break
					}

					t.Logf("PROBE %s:%d: %s", filepath.Base(name), i+1, strings.TrimRight(line, "\r"))
				}
			}
		}
	}
}
