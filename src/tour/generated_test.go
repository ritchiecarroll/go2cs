package main

import (
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"net/url"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// generatedFixture saves a conversion whose workspace is laid out as a built one is: go2cs's
// .artifacts/gen/<token>/<generator assembly>/<generator>/ files beside .artifacts/obj/<token>/ named for the
// project, plus files the tab must never show.
func generatedFixture(t *testing.T) (*server, *conversionArtifact) {
	t.Helper()
	root := t.TempDir()
	runner := newPipelineRunner(root)
	t.Cleanup(runner.close)
	s := &server{repoRoot: &root, pipeline: runner, tour: newTourProxy("127.0.0.1:0", root), static: http.NotFoundHandler()}

	workDir := t.TempDir()
	outputDir := filepath.Join(workDir, "cs")
	write := func(rel, content string) {
		p := filepath.Join(outputDir, filepath.FromSlash(rel))
		if err := os.MkdirAll(filepath.Dir(p), 0o755); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(p, []byte(content), 0o644); err != nil {
			t.Fatal(err)
		}
	}
	write(".artifacts/gen/0a1b2c3d4e5f/go2cs-gen/go2cs.ImplementedInterfaceGenerator/main_S.cs", "// S implements I\n")
	write(".artifacts/gen/0a1b2c3d4e5f/go2cs-gen/go2cs.RecvGenerator/main_S_Inc.cs", "// Inc overloads\n")
	write(".artifacts/gen/0a1b2c3d4e5f/go2cs-gen/notes.txt", "not C#\n")
	write(".artifacts/gen/0a1b2c3d4e5f/go2cs-gen/.editorconfig", "root = true\n")
	write(".artifacts/obj/0a1b2c3d4e5f/session.csproj.nuget.g.props", "<Project />\n")
	write("src/tour.local/session/main.cs", "// the transpiled program, outside the root\n")

	artifact := &conversionArtifact{id: "conv1", workDir: workDir, outputDir: outputDir}
	runner.mu.Lock()
	runner.saved["conv1"] = artifact
	runner.mu.Unlock()
	return s, artifact
}

func getGenerated(t *testing.T, s *server, id, path string) (int, generatedResult, string) {
	t.Helper()
	query := url.Values{"conversionId": {id}, "path": {path}}
	recorder := httptest.NewRecorder()
	s.routes().ServeHTTP(recorder, httptest.NewRequest(http.MethodGet, "/api/generated?"+query.Encode(), nil))
	var result generatedResult
	body := recorder.Body.String()
	if recorder.Code == http.StatusOK {
		if err := json.Unmarshal([]byte(body), &result); err != nil {
			t.Fatalf("GET %q: %v\n%s", path, err, body)
		}
	}
	return recorder.Code, result, body
}

func entryNames(entries []generatedEntry) string {
	var names []string
	for _, e := range entries {
		names = append(names, e.Kind+":"+e.Name)
	}
	return strings.Join(names, " ")
}

func TestGeneratedListsTheRootWithProjectLabels(t *testing.T) {
	s, _ := generatedFixture(t)
	code, result, body := getGenerated(t, s, "conv1", "")
	if code != http.StatusOK || result.State != "ready" || result.Kind != "dir" {
		t.Fatalf("root listing: %d %s", code, body)
	}
	if len(result.Entries) != 1 || result.Entries[0].Name != "0a1b2c3d4e5f" || result.Entries[0].Label != "session" {
		t.Fatalf("root entries = %+v, want the one token folder labelled with its project", result.Entries)
	}
}

func TestGeneratedListingShowsFoldersAndCSharpOnly(t *testing.T) {
	s, _ := generatedFixture(t)
	_, result, _ := getGenerated(t, s, "conv1", "0a1b2c3d4e5f/go2cs-gen")
	if got, want := entryNames(result.Entries), "dir:go2cs.ImplementedInterfaceGenerator dir:go2cs.RecvGenerator"; got != want {
		t.Fatalf("listing = %q, want %q (folders only: notes.txt and .editorconfig are not C#)", got, want)
	}
	_, result, _ = getGenerated(t, s, "conv1", "0a1b2c3d4e5f/go2cs-gen/go2cs.RecvGenerator")
	if len(result.Entries) != 1 || result.Entries[0].Kind != "file" || result.Entries[0].Path != "0a1b2c3d4e5f/go2cs-gen/go2cs.RecvGenerator/main_S_Inc.cs" ||
		result.Entries[0].Size == 0 {
		t.Fatalf("file entry = %+v", result.Entries)
	}
}

func TestGeneratedServesACSharpFile(t *testing.T) {
	s, _ := generatedFixture(t)
	code, result, body := getGenerated(t, s, "conv1", "0a1b2c3d4e5f/go2cs-gen/go2cs.ImplementedInterfaceGenerator/main_S.cs")
	if code != http.StatusOK || result.Kind != "file" || result.Content != "// S implements I\n" {
		t.Fatalf("serve: %d %s", code, body)
	}
}

// Every way out of the root is refused by name, and so is a file that is not C#.
func TestGeneratedRefusesPathsOutsideTheRoot(t *testing.T) {
	s, artifact := generatedFixture(t)
	outside := filepath.Join(artifact.outputDir, "src")
	link := filepath.Join(generatedRoot(artifact), "0a1b2c3d4e5f", "escape")
	symlinked := runtime.GOOS != "windows"
	if symlinked {
		if err := os.Symlink(outside, link); err != nil {
			t.Fatal(err)
		}
		// A link is never followed, even one that stays inside the root, and a link named like a C# file is
		// neither listed nor served.
		inside := filepath.Join(generatedRoot(artifact), "0a1b2c3d4e5f", "inside")
		if err := os.Symlink(filepath.Join(generatedRoot(artifact), "0a1b2c3d4e5f", "go2cs-gen"), inside); err != nil {
			t.Fatal(err)
		}
		leak := filepath.Join(generatedRoot(artifact), "0a1b2c3d4e5f", "leak.cs")
		if err := os.Symlink(filepath.Join(outside, "tour.local", "session", "main.cs"), leak); err != nil {
			t.Fatal(err)
		}
	}

	refused := []string{
		"..",
		"../src/tour.local/session/main.cs",
		"0a1b2c3d4e5f/../../src",
		"0a1b2c3d4e5f/go2cs-gen/../../../src",
		"/etc/passwd",
		`0a1b2c3d4e5f\go2cs-gen`,
		"0a1b2c3d4e5f/go2cs-gen/notes.txt",
	}
	if symlinked {
		refused = append(refused, "0a1b2c3d4e5f/escape", "0a1b2c3d4e5f/escape/tour.local/session/main.cs",
			"0a1b2c3d4e5f/inside", "0a1b2c3d4e5f/inside/go2cs.RecvGenerator/main_S_Inc.cs", "0a1b2c3d4e5f/leak.cs")
	}
	for _, p := range refused {
		code, _, body := getGenerated(t, s, "conv1", p)
		if code != http.StatusBadRequest || !strings.Contains(body, "refused") {
			t.Errorf("%q: %d %s, want 400 refused", p, code, body)
		}
		if strings.Contains(body, "transpiled program") {
			t.Errorf("%q served a file outside the generated-code folder: %s", p, body)
		}
	}

	if symlinked {
		_, result, _ := getGenerated(t, s, "conv1", "0a1b2c3d4e5f")
		if names := entryNames(result.Entries); strings.Contains(names, "escape") || strings.Contains(names, "inside") || strings.Contains(names, "leak") {
			t.Errorf("the listing shows a link: %s", entryNames(result.Entries))
		}
	}
}

func TestGeneratedStates(t *testing.T) {
	s, artifact := generatedFixture(t)

	if code, _, _ := getGenerated(t, s, "nope", ""); code != http.StatusNotFound {
		t.Errorf("unknown conversion: %d, want 404", code)
	}
	if code, _, _ := getGenerated(t, s, "", ""); code != http.StatusBadRequest {
		t.Errorf("no conversionId: %d, want 400", code)
	}
	if code, _, _ := getGenerated(t, s, "conv1", "0a1b2c3d4e5f/missing.cs"); code != http.StatusNotFound {
		t.Errorf("missing file: %d, want 404", code)
	}

	// While the conversion's build runs, nothing is read: a half-written folder is not shown.
	building := &prebuild{cancel: func() {}, done: make(chan struct{})}
	s.pipeline.mu.Lock()
	artifact.prebuild = building
	s.pipeline.mu.Unlock()
	if _, result, _ := getGenerated(t, s, "conv1", ""); result.State != "pending" {
		t.Errorf("during the build: state %q, want pending", result.State)
	}
	close(building.done)
	if _, result, _ := getGenerated(t, s, "conv1", ""); result.State != "ready" {
		t.Errorf("after the build: state %q, want ready", result.State)
	}

	// Before any build has written the folder, the tab says so rather than failing.
	if err := os.RemoveAll(generatedRoot(artifact)); err != nil {
		t.Fatal(err)
	}
	if code, result, _ := getGenerated(t, s, "conv1", ""); code != http.StatusOK || result.State != "unavailable" {
		t.Errorf("no generator output: %d %q, want 200 unavailable", code, result.State)
	}
}
