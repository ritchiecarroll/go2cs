// parsecorpus-extract: the typescript-go-cs corpus extractor. It is not part of TypeScript. It runs natively only:
// its C# is not built or published, so both sides of the comparison read the units it writes.
//
// Writes the parser fuzz corpus exactly as internal/parser's FuzzParser enumerates its seeds,
// one file per unit plus a manifest, so the native and the converted parsedemo read identical bytes.
//
//	parsecorpus-extract <tsc module dir> <out dir>
package main

import (
	"crypto/sha256"
	"fmt"
	"io/fs"
	"os"
	"path/filepath"
	"slices"
	"strconv"
	"strings"

	"github.com/microsoft/TypeScript/tsc/internal/collections"
	"github.com/microsoft/TypeScript/tsc/internal/testrunner"
	"github.com/microsoft/TypeScript/tsc/internal/tspath"
)

type unit struct {
	ext, src, name, content string
	jsx, force              bool
}

// walk is FuzzParser's allParsableFiles: every file under root whose extension tspath recognizes, in WalkDir order.
func walk(root string, yield func(path, rel string)) {
	err := filepath.WalkDir(root, func(path string, d fs.DirEntry, err error) error {
		if err != nil {
			return err
		}
		if d.IsDir() || tspath.TryGetExtensionFromPath(path) == "" {
			return nil
		}
		rel, err := filepath.Rel(root, path)
		if err != nil {
			return err
		}
		yield(path, filepath.ToSlash(rel))
		return nil
	})
	if err != nil {
		panic(err)
	}
}

func main() {
	tscDir, out := os.Args[1], os.Args[2]
	testData := filepath.Join(tscDir, "testdata")
	var supported collections.Set[string]
	for _, es := range tspath.AllSupportedExtensionsWithJson {
		for _, e := range es {
			supported.Add(e)
		}
	}
	var units []unit
	walk(filepath.Join(testData, "fixtures"), func(path, rel string) {
		text, err := os.ReadFile(path)
		if err != nil {
			panic(err)
		}
		units = append(units, unit{ext: tspath.TryGetExtensionFromPath(path), src: "fixtures/" + rel, content: string(text)})
	})
	for _, dir := range []string{"tests/cases/compiler", "tests/cases/conformance"} {
		root := filepath.Join(testData, dir)
		if _, err := os.Stat(root); os.IsNotExist(err) {
			continue
		}
		walk(root, func(path, rel string) {
			text, err := os.ReadFile(path)
			if err != nil {
				panic(err)
			}
			type tf struct{ content, name string }
			tus, _, _, _, err := testrunner.ParseTestFilesAndSymlinks(string(text), path,
				func(filename string, content string, _ map[string]string) (tf, error) {
					return tf{content, filename}, nil
				})
			if err != nil {
				panic(err)
			}
			for _, u := range tus {
				ext := tspath.TryGetExtensionFromPath(u.name)
				if ext == "" {
					continue
				}
				units = append(units, unit{ext: ext, src: dir + "/" + rel, name: u.name, content: u.content})
			}
		})
	}
	// The stored fuzz inputs (testdata/fuzz/FuzzParser), which go test runs after the seeds, in name order.
	fuzzDir := filepath.Join(tscDir, "internal", "parser", "testdata", "fuzz", "FuzzParser")
	entries, err := os.ReadDir(fuzzDir)
	if err != nil {
		panic(err)
	}
	names := make([]string, 0, len(entries))
	for _, e := range entries {
		names = append(names, e.Name())
	}
	slices.Sort(names)
	for _, name := range names {
		b, err := os.ReadFile(filepath.Join(fuzzDir, name))
		if err != nil {
			panic(err)
		}
		lines := strings.Split(strings.TrimSpace(string(b)), "\n")
		if len(lines) != 5 || lines[0] != "go test fuzz v1" {
			panic("unexpected fuzz file " + name)
		}
		str := func(l string) string {
			v, err := strconv.Unquote(strings.TrimSuffix(strings.TrimPrefix(l, "string("), ")"))
			if err != nil {
				panic(err)
			}
			return v
		}
		units = append(units, unit{ext: str(lines[1]), src: "internal/parser/testdata/fuzz/FuzzParser/" + name, content: str(lines[2]),
			jsx: lines[3] == "bool(true)", force: lines[4] == "bool(true)"})
	}
	if err := os.MkdirAll(filepath.Join(out, "units"), 0o755); err != nil {
		panic(err)
	}
	man, err := os.Create(filepath.Join(out, "manifest.tsv"))
	if err != nil {
		panic(err)
	}
	defer man.Close()
	n := 0
	for i, u := range units {
		if !supported.Has(u.ext) {
			continue // FuzzParser's t.Skip
		}
		n++
		if err := os.WriteFile(filepath.Join(out, "units", fmt.Sprintf("%05d.txt", i)), []byte(u.content), 0o644); err != nil {
			panic(err)
		}
		fmt.Fprintf(man, "%05d\t%s\t%t\t%t\t%x\t%s\t%s\n", i, u.ext, u.jsx, u.force, sha256.Sum256([]byte(u.content)), u.src, u.name)
	}
	fmt.Printf("seeds %d, parsed (supported extension) %d\n", len(units), n)
}
