package main

import (
	"errors"
	"io/fs"
	"net/http"
	"os"
	"path"
	"path/filepath"
	"sort"
	"strings"
)

// The Generated Code tab: the source generators' output for one conversion, read-only.
//
// A converted project emits its generators' output (EmitCompilerGeneratedFiles is set in the project, and the
// Directory.Build.targets go2cs writes beside the converted tree routes CompilerGeneratedFilesOutputPath to
// .artifacts/gen/<project token>), so after a build the conversion's workspace holds every file go2cs-gen wrote:
// interface implementations, pointer-receiver overloads, embedding forwarders, field properties and literal
// interceptors. The tour adds nothing to the build; this endpoint only lists and serves that folder.
//
// GET /api/generated?conversionId=<id>&path=<slash path under the root>
//
// The root is the conversion's .artifacts/gen, and nothing outside it is reachable: a path is refused when it is
// absolute, holds a `..` segment, names a symbolic link anywhere along it, or resolves outside the root. A listing
// carries folders and .cs files only, and only a .cs file is served.

// maxGeneratedFileBytes bounds one served file. Generated files are small; a larger one is refused rather than
// shipped to the page whole.
const maxGeneratedFileBytes = 4 << 20

type generatedEntry struct {
	Name string `json:"name"`
	// Label is a folder's display name where its own name is opaque: the per-project token directories at the root
	// carry the project they belong to.
	Label string `json:"label,omitempty"`
	Path  string `json:"path"`
	Kind  string `json:"kind"`
	Size  int64  `json:"size,omitempty"`
}

type generatedResult struct {
	// State is "ready", "pending" (the conversion's build has not finished) or "unavailable" (no build has
	// produced generator output).
	State   string           `json:"state"`
	Message string           `json:"message,omitempty"`
	Kind    string           `json:"kind,omitempty"`
	Path    string           `json:"path"`
	Entries []generatedEntry `json:"entries,omitempty"`
	Content string           `json:"content,omitempty"`
}

func generatedRoot(artifact *conversionArtifact) string {
	return filepath.Join(artifact.outputDir, ".artifacts", "gen")
}

// generatedArtifact answers the saved conversion without taking a pipeline slot: a listing must not wait behind a
// build.
func (p *pipelineRunner) generatedArtifact(id string) (*conversionArtifact, bool, error) {
	if strings.TrimSpace(id) == "" {
		return nil, false, validationError("conversionId is required")
	}
	p.mu.Lock()
	defer p.mu.Unlock()
	artifact := p.saved[id]
	if artifact == nil {
		return nil, false, conversionNotFoundError("that conversion has expired; convert the current Go source again")
	}
	building := false
	if artifact.prebuild != nil {
		select {
		case <-artifact.prebuild.done:
		default:
			building = true
		}
	}
	return artifact, building, nil
}

// cleanGeneratedPath validates a requested path and answers it as a clean slash path ("" for the root).
func cleanGeneratedPath(requested string) (string, error) {
	if requested == "" || requested == "/" {
		return "", nil
	}
	if strings.ContainsAny(requested, "\\\x00") || strings.HasPrefix(requested, "/") || filepath.IsAbs(requested) ||
		filepath.VolumeName(requested) != "" {
		return "", validationError("refused: the path must be relative to the generated-code folder")
	}
	for _, segment := range strings.Split(requested, "/") {
		if segment == ".." {
			return "", validationError("refused: the path may not leave the generated-code folder")
		}
	}
	clean := path.Clean(requested)
	if clean == "." {
		return "", nil
	}
	return clean, nil
}

// resolveGenerated answers the on-disk path of a clean slash path under root, refusing a symbolic link anywhere
// along it and any path that does not resolve inside root.
func resolveGenerated(root, clean string) (string, fs.FileInfo, error) {
	current := root
	info, err := os.Lstat(current)
	if err != nil {
		return "", nil, err
	}
	if clean != "" {
		for _, segment := range strings.Split(clean, "/") {
			current = filepath.Join(current, segment)
			if info, err = os.Lstat(current); err != nil {
				return "", nil, err
			}
			if info.Mode()&fs.ModeSymlink != 0 {
				return "", nil, validationError("refused: a link inside the generated-code folder is not followed")
			}
		}
	}
	realRoot, err := filepath.EvalSymlinks(root)
	if err != nil {
		return "", nil, err
	}
	realPath, err := filepath.EvalSymlinks(current)
	if err != nil {
		return "", nil, err
	}
	if rel, err := filepath.Rel(realRoot, realPath); err != nil || rel == ".." || strings.HasPrefix(rel, ".."+string(filepath.Separator)) {
		return "", nil, validationError("refused: the path resolves outside the generated-code folder")
	}
	return current, info, nil
}

func (p *pipelineRunner) generated(id, requested string) (generatedResult, error) {
	clean, err := cleanGeneratedPath(requested)
	if err != nil {
		return generatedResult{}, err
	}
	artifact, building, err := p.generatedArtifact(id)
	if err != nil {
		return generatedResult{}, err
	}
	result := generatedResult{Path: clean}
	if building {
		result.State, result.Message = "pending", "The generated code appears here when the conversion's build finishes."
		return result, nil
	}

	root := generatedRoot(artifact)
	target, info, err := resolveGenerated(root, clean)
	if errors.Is(err, fs.ErrNotExist) {
		if clean == "" {
			result.State, result.Message = "unavailable", "No build of this conversion has produced generated code yet. Build or run it to see what go2cs-gen wrote."
			return result, nil
		}
		return generatedResult{}, conversionNotFoundError("no generated file or folder " + clean)
	}
	if err != nil {
		return generatedResult{}, err
	}

	result.State = "ready"
	if !info.IsDir() {
		if !strings.EqualFold(filepath.Ext(target), ".cs") {
			return generatedResult{}, validationError("refused: only .cs files are shown")
		}
		if info.Size() > maxGeneratedFileBytes {
			return generatedResult{}, validationError("refused: the file is larger than the viewer shows")
		}
		content, err := os.ReadFile(target)
		if err != nil {
			return generatedResult{}, err
		}
		result.Kind, result.Content = "file", string(content)
		return result, nil
	}

	entries, err := os.ReadDir(target)
	if err != nil {
		return generatedResult{}, err
	}
	labels := map[string]string{}
	if clean == "" {
		labels = projectTokenLabels(artifact)
	}
	result.Kind = "dir"
	for _, entry := range entries {
		if entry.Type()&fs.ModeSymlink != 0 {
			continue
		}
		item := generatedEntry{Name: entry.Name(), Path: path.Join(clean, entry.Name())}
		switch {
		case entry.IsDir():
			item.Kind, item.Label = "dir", labels[entry.Name()]
		case strings.EqualFold(filepath.Ext(entry.Name()), ".cs"):
			item.Kind = "file"
			if fileInfo, err := entry.Info(); err == nil {
				item.Size = fileInfo.Size()
			}
		default:
			continue
		}
		result.Entries = append(result.Entries, item)
	}
	sort.Slice(result.Entries, func(i, j int) bool {
		a, b := result.Entries[i], result.Entries[j]
		if a.Kind != b.Kind {
			return a.Kind == "dir"
		}
		return strings.ToLower(a.Name) < strings.ToLower(b.Name)
	})
	return result, nil
}

// projectTokenLabels names the root's per-project token directories: go2cs's Directory.Build.props keys a
// project's obj, bin and gen folders by the same token, and the obj folder holds files named after the project.
func projectTokenLabels(artifact *conversionArtifact) map[string]string {
	labels := map[string]string{}
	objRoot := filepath.Join(artifact.outputDir, ".artifacts", "obj")
	tokens, err := os.ReadDir(objRoot)
	if err != nil {
		return labels
	}
	for _, token := range tokens {
		if !token.IsDir() {
			continue
		}
		files, err := os.ReadDir(filepath.Join(objRoot, token.Name()))
		if err != nil {
			continue
		}
		for _, f := range files {
			if name, _, ok := strings.Cut(f.Name(), ".csproj"); ok && name != "" {
				labels[token.Name()] = name
				break
			}
		}
	}
	return labels
}

func (s *server) handleGenerated(w http.ResponseWriter, r *http.Request) {
	result, err := s.pipeline.generated(r.URL.Query().Get("conversionId"), r.URL.Query().Get("path"))
	if err != nil {
		s.writePipelineError(w, err)
		return
	}
	writeJSON(w, http.StatusOK, result)
}
