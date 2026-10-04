// versionReport.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"runtime"
	"runtime/debug"
	"strings"
)

// versionReport returns what `go2cs -version` prints: the release tuple a bug report or a build package
// needs, one key=value line per key in a fixed order. Every key is always present, "unknown" when its
// value cannot be resolved, so a consumer can parse the report without probing for keys. New keys are
// appended; the order and names are pinned by versionReport_test.go and documented in docs/README.md.
func versionReport() string {
	var report strings.Builder

	line := func(key, value string) {
		if value == "" {
			value = "unknown"
		}

		fmt.Fprintf(&report, "%s=%s\n", key, value)
	}

	// The go.* NuGet version -recurse=nuget defaults $(GoStdLibVersion) to (see moduleConverter.go): the
	// active toolchain's release, floating on the NuGet revision.
	packages, toolchain := "", ""

	if version := goVersion(); version != "" {
		packages, toolchain = version+".*", "go"+version
	}

	revision, modified := "", ""

	if info, ok := debug.ReadBuildInfo(); ok {
		for _, setting := range info.Settings {
			switch setting.Key {
			case "vcs.revision":
				revision = setting.Value
			case "vcs.modified":
				modified = setting.Value
			}
		}
	}

	if revision == "" {
		modified = ""
	}

	line("go2cs.release", corpusRelease())
	line("go2cs.converter", converterRevision())
	line("go.packages", packages)
	line("go.toolchain", toolchain)
	line("go.build", runtime.Version())
	line("vcs.revision", revision)
	line("vcs.modified", modified)

	return report.String()
}
