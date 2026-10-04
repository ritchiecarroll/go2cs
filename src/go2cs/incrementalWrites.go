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
// Two cases need more than the byte compare:
//   - The PLATFORM CENSUS tells "emitted by this run" from "seeded" by modification time against a sentinel it stamps
//     on its seeded root (platformCensus.go snapshotConvertedRoot; h8-comparand.sh reads the staged root the same
//     way). A census therefore writes every source, as before: Options.alwaysWriteSources, set only by runCensusTarget.
//   - A source still holding DEFERRED MARKERS (deferredMarkerOperations.go) is written with them and rewritten by the
//     marker passes after the package is visited, so its text at write time never equals the previous run's resolved
//     file. Its previous bytes and time are remembered, and once the passes have run, a source whose resolved bytes
//     equal its previous ones gets its previous time back: the run did not change it.

package main

import (
	"bytes"
	"os"
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
	if alwaysWriteSources.Load() || !(bytes.Contains(content, []byte(dynamicTypeMarkerPrefix)) || bytes.Contains(content, []byte(adapterNameMarkerPrefix))) {
		return
	}

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

// markedSourcesRestored counts the marker-bearing sources given their previous time back in this process (a
// statistic, and what lets a test tell the marker path ran from a fixture that happened to emit no marker).
var markedSourcesRestored atomic.Int64
