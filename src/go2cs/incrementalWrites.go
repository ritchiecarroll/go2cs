// incrementalWrites.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// INCREMENTAL SOURCE WRITES. A converted C# source is written only when its bytes differ from the file already on
// disk, as every other converter output already is (needToWriteFile): an unchanged package keeps its sources'
// timestamps, and MSBuild does not recompile its project after a re-conversion.
//
// Three cases need more than the byte compare:
//   - The PLATFORM CENSUS tells "emitted by this run" from "seeded" by modification time against a sentinel it stamps
//     on its seeded root (platformCensus.go snapshotConvertedRoot; h8-comparand.sh reads the staged root the same
//     way). A census therefore writes every source, as before: Options.alwaysWriteSources, set only by runCensusTarget.
//   - A source still holding DEFERRED MARKERS (deferredMarkerOperations.go) is written with them and rewritten by the
//     marker passes after the package is visited, so its text at write time never equals the previous run's resolved
//     file. Its previous bytes and time are remembered, and once the passes have run, a source whose resolved bytes
//     equal its previous ones gets its previous time back: the run did not change it.
//   - A -tests conversion re-seeds its METADATA ANCHORS (package_test_info.cs, package_info_internal_test.cs) on every
//     run and then merges into them, so the merge's byte compare reads the seed; and its RECOMPILE-MODEL FALLBACK
//     re-runs the whole conversion over files the abandoned reference attempt already rewrote (removing
//     package_info_external_test.cs on the way). Every source in its output directory is therefore remembered before
//     its first write (rememberSourcesIn) and restored once the conversion's writes are done.
//
// The FIRST state remembered for a file wins, so a re-run inside the same conversion compares against the file from
// before the run, never against an abandoned attempt's.

package main

import (
	"bytes"
	"os"
	"path/filepath"
	"sync"
	"sync/atomic"
	"time"
)

// alwaysWriteSources mirrors Options.alwaysWriteSources for the source writers that take no Options (package_info.cs,
// the init-order files, the .cs.auto sibling). processConversion sets it from its options on every call.
var alwaysWriteSources atomic.Bool

// writeSourceIfChanged writes a converted C# source unless its bytes already match the file on disk (always, under the
// census). It reports whether it wrote.
func writeSourceIfChanged(fileName string, content []byte) (written bool, err error) {
	if !alwaysWriteSources.Load() && !needToWriteFile(fileName, content) {
		return false, nil
	}

	return true, os.WriteFile(fileName, content, 0644)
}

// markedSourceState is a marker-bearing source's bytes and modification time from before this run rewrote it.
type markedSourceState struct {
	content  []byte
	modified time.Time
}

var markedSources sync.Map // output file name -> markedSourceState

// rememberIfMarked records the current state of fileName when the content about to be written still holds a deferred
// marker. Nothing is recorded under the census, or when the file does not exist yet.
func rememberIfMarked(fileName string, content []byte) {
	if !(bytes.Contains(content, []byte(dynamicTypeMarkerPrefix)) || bytes.Contains(content, []byte(adapterNameMarkerPrefix))) {
		return
	}

	rememberSource(fileName)
}

// rememberSourcesIn remembers every .cs directly in dir and returns their names: a -tests conversion's snapshot of its
// output directory before its first write.
func rememberSourcesIn(dir string) []string {
	if alwaysWriteSources.Load() {
		return nil
	}

	fileNames, _ := filepath.Glob(filepath.Join(dir, "*.cs"))

	for _, fileName := range fileNames {
		rememberSource(fileName)
	}

	return fileNames
}

// rememberSource records the current state of fileName unless a state is already recorded for it (the first wins).
// Nothing is recorded under the census, or when the file does not exist yet.
func rememberSource(fileName string) {
	if alwaysWriteSources.Load() {
		return
	}

	if _, recorded := markedSources.Load(fileName); recorded {
		return
	}

	storeSourceState(fileName)
}

func storeSourceState(fileName string) {
	info, err := os.Stat(fileName)

	if err != nil {
		return
	}

	previous, err := os.ReadFile(fileName)

	if err != nil {
		return
	}

	markedSources.Store(fileName, markedSourceState{content: previous, modified: info.ModTime()})
}

// restoreUnchangedMarkedSources runs after a package's deferred-marker passes: a remembered source whose resolved bytes
// equal what it held before this run gets its previous modification time back.
func restoreUnchangedMarkedSources(outputFileNames []string) {
	for _, fileName := range outputFileNames {
		value, ok := markedSources.LoadAndDelete(fileName)

		if !ok {
			continue
		}

		previous := value.(markedSourceState)
		current, err := os.ReadFile(fileName)

		if err != nil || !bytes.Equal(current, previous.content) {
			continue
		}

		if os.Chtimes(fileName, previous.modified, previous.modified) == nil {
			markedSourcesRestored.Add(1)
		}
	}
}

// markedSourcesRestored counts the remembered sources given their previous time back in this process (a statistic, and
// what lets a test tell the marker path ran from a fixture that happened to emit no marker).
var markedSourcesRestored atomic.Int64
