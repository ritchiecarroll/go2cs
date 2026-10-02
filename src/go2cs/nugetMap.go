// nugetMap.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"errors"
)

// The -nuget-map resolver (docs/PLAN-nugetgo.md section 4, stage S3a).
//
// nugetgo.net maps Go module paths to published NuGet packages holding their go2cs conversions. Under
// -recurse=nuget this file decides, per third-party module, whether a mapping answers it: it reads the
// mapping sources (the user's -nuget-map files and URLs in listed order, then the registry unless
// -nuget-map-only), applies the exclusions and the lock (nugetLock.go), records the result in
// go2cs.nuget.lock and prints the provenance report. It does NOT substitute anything yet: turning a
// mapped module into a PackageReference is stage S3b, so every module is still converted locally and
// the report says so on every mapped row.

// nugetMapOptions is the -nuget-map flag family, as parsed.
type nugetMapOptions struct {
	sources       []string // -nuget-map <url|file>: in listed order, which is precedence (first row naming a module wins)
	off           bool     // -nuget-map off: no source is read and no lock is read or written
	only          bool     // -nuget-map-only: drop the registry fallback, so only the listed sources answer
	exclude       []string // -nuget-map-exclude <module-path>: modules that are never mapped
	refresh       bool     // -nuget-map-refresh: bypass the cache and re-resolve deliberately instead of keeping the lock
	canonicalOnly bool     // -nuget-map-canonical-only: a community mapping is treated as unmapped
}

// active reports whether this run resolves mappings at all.
func (o nugetMapOptions) active() bool {
	return false
}

// newNuGetMapOptions builds the options from the raw flag values.
func newNuGetMapOptions(maps []string, excludes []string, only, refresh, canonicalOnly bool) (nugetMapOptions, error) {
	return nugetMapOptions{}, errors.New("not implemented")
}

// validateNuGetMapFlags refuses the combinations that would otherwise be silent misconfigurations.
func validateNuGetMapFlags(given []string, o nugetMapOptions, recurseNuGet bool) error {
	return nil
}

// nugetMapRow is one schema-v1 mapping row.
type nugetMapRow struct {
	module     string
	nugetID    string
	status     string
	sourceRepo string
	registered string
	contact    string
	line       int
}

// nugetMapSource is one parsed mapping source.
type nugetMapSource struct {
	name string
	rows map[string]nugetMapRow
}

// parseNuGetMap parses one schema-v1 mapping file.
func parseNuGetMap(name string, data []byte) (nugetMapSource, error) {
	return nugetMapSource{}, errors.New("not implemented")
}

// thirdPartyModule is one third-party module of the closure being converted.
type thirdPartyModule struct {
	path     string
	version  string
	replaced bool
}

// nugetMapDecision is the resolver's answer for one module.
type nugetMapDecision struct {
	module   string
	version  string
	mapped   bool
	nugetID  string
	status   string
	layer    string
	fromLock bool
	note     string
}

// resolveNuGetMappings decides every module against the sources and the lock.
func resolveNuGetMappings(modules []thirdPartyModule, sources []nugetMapSource, lock map[string]nugetLockEntry, o nugetMapOptions) ([]nugetMapDecision, []string) {
	return nil, nil
}

// runNuGetMapResolution is what ModuleConverter.ConvertModule calls.
func runNuGetMapResolution(modules []thirdPartyModule, o nugetMapOptions, outRoot string) ([]nugetMapDecision, []string, error) {
	return nil, nil, nil
}

// formatNuGetMapReport renders the provenance report.
func formatNuGetMapReport(decisions []nugetMapDecision, warnings []string) string {
	return ""
}
