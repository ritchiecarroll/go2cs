// droppedFiles.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// A file the converter failed on. The per-file visit recovers a panic (conversionDriver.go, and
// autoSiblingOperations.go for a `.cs.auto` review sibling) so the rest of the package still converts, and that
// file's output is not written. Its declarations are then missing from the package, and so are its records in
// package_info.cs (interface implementations, position maps) and any import only it made. Until 2026-11-13 the run
// printed a warning and exited 0, so a user conversion, a -stdlib reconvert or a -tests run could lose a file and
// read as success (docs/phase4/BOARD-next-validation-candidates.md, the dropped-file row; measured: a footprint run
// that dropped 61 files with rc=0).
//
// Each dropped file is now reported as GO2CS2008 when it happens, and the run fails at the end, naming every one,
// the way an unresolved dynamic type does (unresolvedDynamicTypeError): the -tests path returns it before its build
// step, and main checks it once more after whichever driver ran. The files that did convert are still written.
// -warn cannot downgrade it: a dropped file is not loud, since an init function, a method or an interface record can
// vanish with no compile error (warnOption.go).
package main

import (
	"fmt"
	"path/filepath"
	"strings"
	"sync"
)

// droppedFile is one file the converter failed on.
type droppedFile struct {
	path    string // the Go source file
	reason  string // the recovered panic value
	sibling bool   // the file's destination .cs is hand-owned, so the lost output is its `.cs.auto` review sibling
}

var (
	droppedFilesLock sync.Mutex
	droppedFiles     []droppedFile
)

// visitFileFaultHook, when set, is called before each file's visit; a panic from it is recovered exactly as a
// visitor panic is. Tests set it to drop a chosen file without depending on a converter defect.
var visitFileFaultHook func(filePath string)

// recordDroppedFile reports a file the converter failed on, as GO2CS2008, and keeps it for droppedFileError. The
// "visit file error" warning the recover already printed stays: the behavioral harnesses match on it
// (src/tests/BestEffortConversion.cs).
func recordDroppedFile(filePath string, reason any, sibling bool) {
	file := droppedFile{path: filePath, reason: fmt.Sprint(reason), sibling: sibling}

	droppedFilesLock.Lock()
	droppedFiles = append(droppedFiles, file)
	droppedFilesLock.Unlock()

	reportDiagnostic(filePath, diagnosticFileDropped, file.text())
}

// text is the diagnostic's message for one dropped file.
func (file droppedFile) text() string {
	if file.sibling {
		return fmt.Sprintf("the converter failed on this file (%s), so its `.cs.auto` review sibling was not written, and the package-wide records it feeds (interface implementations, position maps) may be incomplete", file.reason)
	}

	return fmt.Sprintf("the converter failed on this file (%s), so its .cs was not written, and its records are missing from package_info.cs", file.reason)
}

// takeDroppedFiles returns the recorded files and clears the record.
func takeDroppedFiles() []droppedFile {
	droppedFilesLock.Lock()
	defer droppedFilesLock.Unlock()

	files := droppedFiles
	droppedFiles = nil

	return files
}

// droppedFileError fails the run when any file was dropped, naming each, and clears the record either way.
func droppedFileError() error {
	files := takeDroppedFiles()

	if len(files) == 0 {
		return nil
	}

	var summary strings.Builder

	summary.WriteString(fmt.Sprintf("%s: the converter failed on %d %s, whose output was not written; every other file was:", diagnosticFileDropped, len(files), plural(len(files), "file", "files")))

	for _, file := range files {
		kind := ".cs"

		if file.sibling {
			kind = ".cs.auto review sibling"
		}

		summary.WriteString(fmt.Sprintf("\n  %s (%s): %s", filepath.ToSlash(file.path), kind, file.reason))
	}

	summary.WriteString("\n  This is a converter defect: report it with the file. -debug re-raises the panic with its stack.")

	return fmt.Errorf("%s", summary.String())
}
