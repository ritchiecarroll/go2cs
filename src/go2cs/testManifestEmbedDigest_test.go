// testManifestEmbedDigest_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// A package embedding a file OUTSIDE testdata/ (crypto/internal/fips140test's `//go:embed
// acvp_capabilities.entropy.json` from its own root) was refused as stale by `-test-action build` right after
// its own convert, by the same converter binary: "test manifest is stale: input digest changed". The convert
// hashed the payload through the //go:embed registry, which only a CONVERSION fills; build, run and compare
// validate without converting, so the payload dropped out of the recomputed digest. `-test-action all` hid it by
// converting in the same process.
//
// The convert now records the payloads in the manifest (stampTestManifestDigest) and validation hashes the
// recorded paths' current bytes. The arms drive the real convert-side stamp, then validate the way a separate
// build process does: with the registry empty.

func writeEmbedDigestFixture(t *testing.T) (inputPath, outputPath string) {
	t.Helper()

	inputPath = t.TempDir()
	outputPath = t.TempDir()

	for name, contents := range map[string]string{
		"value.go":             "package value\n",
		"value_test.go":        "package value\n\nimport _ \"embed\"\n\n//go:embed payload.json\nvar payload []byte\n",
		"payload.json":         "{\"v\": 1}\n",
		"testdata/fixture.txt": "fixture\n",
	} {
		path := filepath.Join(inputPath, filepath.FromSlash(name))

		if err := os.MkdirAll(filepath.Dir(path), 0755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(path, []byte(contents), 0644); err != nil {
			t.Fatal(err)
		}
	}

	return inputPath, outputPath
}

// convertSideManifest writes the manifest the way a -tests convert does, with the embed registry holding what the
// conversion resolved, and returns it. The registry is cleared afterwards: what follows is another process.
func convertSideManifest(t *testing.T, inputPath, outputPath string, options Options) testManifest {
	t.Helper()

	resetEmbedTargets()
	t.Cleanup(resetEmbedTargets)

	registerEmbedTarget(embedTarget{
		VarName:  "payload",
		Patterns: []string{"payload.json"},
		Entries:  []embedEntry{{Name: "payload.json", SourceOS: filepath.Join(inputPath, "payload.json")}},
		TestHalf: true,
	})

	target := strings.Split(options.targetPlatform, "/")
	manifest := testManifest{TargetGOOS: target[0], TargetGOARCH: target[1], ConverterRevision: converterRevision()}

	if err := stampTestManifestDigest(&manifest, inputPath, outputPath, options); err != nil {
		t.Fatalf("stampTestManifestDigest: %v", err)
	}

	if err := writeJSONFile(filepath.Join(outputPath, testManifestFileName), manifest); err != nil {
		t.Fatalf("writing the manifest: %v", err)
	}

	resetEmbedTargets()

	return manifest
}

// THE SUBJECT: a build process validates the manifest its convert just wrote. Red before the fix with the
// stale-digest refusal.
func TestBuildAfterConvertAcceptsAPayloadOutsideTestdata(t *testing.T) {
	inputPath, outputPath := writeEmbedDigestFixture(t)
	options := Options{targetPlatform: runtime.GOOS + "/" + runtime.GOARCH}

	manifest := convertSideManifest(t, inputPath, outputPath, options)

	if len(manifest.EmbedPayloads) != 1 || manifest.EmbedPayloads[0] != "payload.json" {
		t.Fatalf("the convert must record the payload it hashed, input-relative: %v", manifest.EmbedPayloads)
	}

	if err := validateTestManifest(inputPath, outputPath, options); err != nil {
		t.Fatalf("a build right after its own convert must pass the manifest check: %v", err)
	}
}

// THE CONTROL: the payload's bytes are really hashed, so an edit after the convert still reads stale. A fix that
// simply dropped payloads from both sides would pass the subject and fail here.
func TestAnEditedPayloadStillReadsStale(t *testing.T) {
	inputPath, outputPath := writeEmbedDigestFixture(t)
	options := Options{targetPlatform: runtime.GOOS + "/" + runtime.GOARCH}

	convertSideManifest(t, inputPath, outputPath, options)

	if err := os.WriteFile(filepath.Join(inputPath, "payload.json"), []byte("{\"v\": 2}\n"), 0644); err != nil {
		t.Fatal(err)
	}

	err := validateTestManifest(inputPath, outputPath, options)

	if err == nil || !strings.Contains(err.Error(), "input digest changed") {
		t.Fatalf("an embedded payload edited after the convert must read stale, got %v", err)
	}
}
