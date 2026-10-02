// corpusRelease.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	_ "embed"
	"strings"
)

//go:generate go run ./internal/gencorpusrelease

// corpusReleaseAsset is the go2cs corpus release this converter belongs to -- the LAST PUBLISHED one, e.g.
// 1.24.13.3 -- as releasestamp.PublishedStamp reads it from src/version.props and the write-once snapshots
// under docs/validation. A deployed converter has neither beside it (go:embed cannot reach ../version.props
// either), so the value travels in the binary, generated like stdlib-metadata.txt. The -recurse=nuget
// third-party gate maps a published conversion only when its self-description's go2cs-release equals this
// (docs/PLAN-nugetgo.md 4.6). repoguard's TestCorpusReleaseMatchesThePublishedStamp holds it to the record.
//
//go:embed corpus-release.txt
var corpusReleaseAsset string

// corpusRelease returns the go2cs corpus release this converter belongs to.
func corpusRelease() string {
	return strings.TrimSpace(corpusReleaseAsset)
}
