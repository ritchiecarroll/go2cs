// packageInputDigest.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"crypto/sha256"
	"encoding/hex"
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"runtime"
	"sort"
	"strings"
)

// GoInputDigest: what a converted MODULE package's production conversion is a function of (the nugetgo rehearsal,
// 2026-10-09, gap 4). A module's validation proof is made in a `-recurse -tests` root and its package is packed from a
// separate `-recurse=nuget` root, so the two emissions are never the same bytes: -tests forces comments on, and its
// production pass is steered by the sibling test files. What binds a proof to the packed assembly is therefore the
// INPUT both were converted from. Each module package's project records it as GoInputDigest; the package's proof page
// and its module's MODULE.md (packed as VALIDATION.md) record the value the -tests run's own production pass wrote; and
// nugetgo-pack.ps1 refuses a proof whose value is not the packed project's.
//
// The digest covers the package's production Go sources and //go:embed payloads (named relative to the package, so it
// is the same on any host), its hand-owned files and *_impl.cs companions, the target platform, the output-affecting
// conversion options other than comments, the Go toolchain and the converter revision. Comments are left out: they move
// only the emitted C#'s line layout, and a -tests proof could otherwise never match a -recurse=nuget root converted
// without -comments.

const inputDigestProperty = "GoInputDigest"

var inputDigestPropertyPattern = regexp.MustCompile(`<` + inputDigestProperty + `>([^<]*)</` + inputDigestProperty + `>`)

// recordsInputDigest reports whether a project records GoInputDigest: a module package converted under -recurse
// (either reference mode). The standard library and a single-package conversion keep the project they always had.
func recordsInputDigest(projectFileName string, options Options) bool {
	return options.recurse && !isStdLibProjectEmission(projectFileName, options)
}

// moduleInputDigest is the GoInputDigest a package's project records, or "" when it records none. A failure to read an
// input is a warning, never a stop: the project then records no digest, and nugetgo-pack.ps1 refuses to pack it.
func moduleInputDigest(projectFileName string, files []FileEntry, outputPath string, options Options) string {
	if !recordsInputDigest(projectFileName, options) {
		return ""
	}

	sources := make([]string, 0, len(files))

	for _, entry := range files {
		sources = append(sources, entry.filePath)
	}

	digest, err := packageInputDigest(sources, outputPath, options, converterRevision())

	if err != nil {
		showWarning("%s records no %s: %v", projectFileName, inputDigestProperty, err)
		return ""
	}

	return digest
}

// packageInputDigest is the digest of the production inputs, as sha256-<64 hex>. sourceFiles are the package's parsed
// production Go files; outputPath is the package's output directory, where its hand-owned companions live.
func packageInputDigest(sourceFiles []string, outputPath string, options Options, revision string) (string, error) {
	if len(sourceFiles) == 0 {
		return "", nil
	}

	packageDir := filepath.Dir(sourceFiles[0])
	inputs := map[string]string{}

	for _, source := range sourceFiles {
		inputs["source:"+filepath.Base(source)] = source
	}

	for _, payload := range embedPayloadPaths(currentEmbedTargets()) {
		if rel, err := filepath.Rel(packageDir, payload); err == nil && !strings.HasPrefix(rel, "..") {
			inputs["source:"+filepath.ToSlash(rel)] = payload
		}
	}

	// A hand-owned file stands in for its source's emission (conversionDriver.go), so its own bytes are an input too.
	for _, source := range sourceFiles {
		fileName := strings.TrimSuffix(filepath.Base(source), ".go") + ".cs"
		handOwned := platformLayoutPath(outputPath, goosOfTarget(options.targetPlatform), fileName)

		if marked, err := containsManualConversionMarker(handOwned); err == nil && marked {
			if rel, err := filepath.Rel(outputPath, handOwned); err == nil {
				inputs["output:"+filepath.ToSlash(rel)] = handOwned
			}
		}
	}

	companions, err := filepath.Glob(filepath.Join(outputPath, "*_impl.cs"))

	if err != nil {
		return "", err
	}

	for _, companion := range companions {
		inputs["output:"+filepath.Base(companion)] = companion
	}

	names := make([]string, 0, len(inputs))

	for name := range inputs {
		names = append(names, name)
	}

	sort.Strings(names)
	hash := sha256.New()

	for _, name := range names {
		data, err := os.ReadFile(inputs[name])

		if err != nil {
			return "", err
		}

		fmt.Fprintf(hash, "%s\x00%d\x00", name, len(data))
		hash.Write(data)
	}

	fmt.Fprintf(hash, "\x00%s\x00uco=%t;var=%t;indent=%d;cgo=%t\x00%s\x00%s", options.targetPlatform,
		options.useChannelOperators, options.preferVarDecl, options.indentSpaces, options.parseCgoTargets,
		runtime.Version(), revision)

	return "sha256-" + hex.EncodeToString(hash.Sum(nil)), nil
}

// insertInputDigestProperty records digest as the last property of the project's first PropertyGroup; an empty digest
// leaves the project as it is.
func insertInputDigestProperty(contents string, digest string) string {
	const anchor = "</PropertyGroup>"

	at := strings.Index(contents, anchor)

	if digest == "" || at < 0 {
		return contents
	}

	lineStart := strings.LastIndex(contents[:at], "\n") + 1
	newline := "\n"

	if strings.Contains(contents, "\r\n") {
		newline = "\r\n"
	}

	property := "    <!-- What this package's conversion is a function of; nugetgo-pack.ps1 checks its validation proof names the same -->" + newline +
		"    <" + inputDigestProperty + ">" + digest + "</" + inputDigestProperty + ">" + newline

	return contents[:lineStart] + property + contents[lineStart:]
}

// projectInputDigest reads GoInputDigest back out of a project's contents, or "".
func projectInputDigest(contents string) string {
	if match := inputDigestPropertyPattern.FindStringSubmatch(contents); match != nil {
		return strings.TrimSpace(match[1])
	}

	return ""
}
