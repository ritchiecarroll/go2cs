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
	"fmt"
	"slices"
	"sort"
	"strings"

	"golang.org/x/mod/module"
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
//
// DORMANT BY DEFAULT until S3b (COORD ruling 2026-10-02): a -recurse=nuget run that names no -nuget-map
// source resolves nothing, fetches nothing and writes no lock, so it is byte-identical to the run before
// this file existed. S3b flips active() together with the substitution.

// nugetMapOptions is the -nuget-map flag family, as parsed.
type nugetMapOptions struct {
	sources       []string // -nuget-map <url|file>: in listed order, which is precedence (first row naming a module wins)
	off           bool     // -nuget-map off: no source is read and no lock is read or written
	only          bool     // -nuget-map-only: drop the registry fallback, so only the listed sources answer
	exclude       []string // -nuget-map-exclude <module-path>: modules that are never mapped
	refresh       bool     // -nuget-map-refresh: bypass the cache and re-resolve deliberately instead of keeping the lock
	canonicalOnly bool     // -nuget-map-canonical-only: a community mapping is treated as unmapped
}

// active reports whether this run resolves mappings at all: only when a source is named (the dormant
// default above).
func (o nugetMapOptions) active() bool {
	return !o.off && len(o.sources) > 0
}

// newNuGetMapOptions builds the options from the raw flag values. `off` is a value of -nuget-map, not a
// source, and it cannot be combined with one: "read these sources" and "read none" contradict each other.
func newNuGetMapOptions(maps []string, excludes []string, only, refresh, canonicalOnly bool) (nugetMapOptions, error) {
	o := nugetMapOptions{only: only, refresh: refresh, canonicalOnly: canonicalOnly}

	for _, value := range maps {
		switch {
		case value == "":
			return nugetMapOptions{}, errors.New("-nuget-map needs a value: a mapping file, an https:// URL, or off")
		case value == "off":
			o.off = true
		default:
			o.sources = append(o.sources, value)
		}
	}

	if o.off && len(o.sources) > 0 {
		return nugetMapOptions{}, fmt.Errorf("-nuget-map off disables mapping entirely and cannot be combined with a mapping source (%s)", strings.Join(o.sources, ", "))
	}

	for _, value := range excludes {
		if value == "" {
			return nugetMapOptions{}, errors.New("-nuget-map-exclude needs a module path")
		}

		o.exclude = append(o.exclude, value)
	}

	return o, nil
}

// validateNuGetMapFlags refuses the combinations that would otherwise be silent misconfigurations. given
// lists the -nuget-map* flags that appeared on the command line, by name.
func validateNuGetMapFlags(given []string, o nugetMapOptions, recurseNuGet bool) error {
	if len(given) > 0 && !recurseNuGet {
		return fmt.Errorf("%s applies only with -recurse=nuget (mappings choose NuGet packages for third-party modules, and only that mode references NuGet); without it the flag would do nothing", strings.Join(given, ", "))
	}

	if o.only && len(o.sources) == 0 {
		return errors.New("-nuget-map-only drops the nugetgo.net fallback, so it needs at least one -nuget-map <file|https-url> source to answer instead")
	}

	for _, src := range o.sources {
		if scheme, _, isURL := strings.Cut(src, "://"); isURL && scheme != "https" {
			return fmt.Errorf("-nuget-map %s: only https:// URLs are fetched (a plain http:// source can be rewritten in transit); a local file is named by its path", src)
		}
	}

	return nil
}

// nugetMapRow is one schema-v1 mapping row (docs/PLAN-nugetgo.md section 1). The converter reads the
// first three fields; the rest are provenance for humans, kept for the report.
type nugetMapRow struct {
	module     string
	nugetID    string
	status     string
	sourceRepo string
	registered string
	contact    string
	line       int
}

// nugetMapSource is one parsed mapping source, named the way the user named it (or the registry URL).
type nugetMapSource struct {
	name string
	rows map[string]nugetMapRow
}

const nugetMapFieldCount = 6

var nugetMapStatuses = []string{"canonical", "community", "withdrawn"}

// parseNuGetMap parses one schema-v1 mapping file: '#' comments and blank lines are skipped; every other
// line is exactly six TAB-separated fields. A malformed line or a second row for a module REFUSES THE
// WHOLE SOURCE, naming it with file:line -- the registry's CI guarantees well-formed rows, so a bad one
// signals corruption, and skipping it would silently change what the source answers. These rules mirror
// PLAN section 1; the registry repo's own lint is cross-checked against them at S3b.
func parseNuGetMap(name string, data []byte) (nugetMapSource, error) {
	src := nugetMapSource{name: name, rows: make(map[string]nugetMapRow)}

	for index, line := range strings.Split(string(data), "\n") {
		lineNumber := index + 1
		line = strings.TrimSuffix(line, "\r")

		if strings.TrimSpace(line) == "" || strings.HasPrefix(line, "#") {
			continue
		}

		fields := strings.Split(line, "\t")

		if len(fields) != nugetMapFieldCount {
			return nugetMapSource{}, fmt.Errorf("%s:%d: want 6 TAB-separated fields (module-path, nuget-id, status, source-repo, registered, contact), got %d", name, lineNumber, len(fields))
		}

		row := nugetMapRow{module: fields[0], nugetID: fields[1], status: fields[2], sourceRepo: fields[3], registered: fields[4], contact: fields[5], line: lineNumber}

		if err := module.CheckPath(row.module); err != nil {
			return nugetMapSource{}, fmt.Errorf("%s:%d: invalid module path %q: %v", name, lineNumber, row.module, err)
		}

		if row.nugetID == "" || strings.ContainsAny(row.nugetID, " \t") || len(row.nugetID) > 100 {
			return nugetMapSource{}, fmt.Errorf("%s:%d: invalid nuget-id %q (non-empty, no whitespace, at most 100 characters)", name, lineNumber, row.nugetID)
		}

		if !slices.Contains(nugetMapStatuses, row.status) {
			return nugetMapSource{}, fmt.Errorf("%s:%d: invalid status %q (want canonical, community or withdrawn)", name, lineNumber, row.status)
		}

		if first, duplicate := src.rows[row.module]; duplicate {
			return nugetMapSource{}, fmt.Errorf("%s:%d: a second row for module %s (first at line %d); a source maps each module path once", name, lineNumber, row.module, first.line)
		}

		src.rows[row.module] = row
	}

	return src, nil
}

// thirdPartyModule is one third-party module of the closure being converted, as packages.Load reports
// it. A replaced module's source is the user's own choice, so it is never mapped.
type thirdPartyModule struct {
	path     string
	version  string
	replaced bool
}

// nugetMapDecision is the resolver's answer for one module: mapped to nugetID by layer (the source that
// answered, or the lock), or unmapped with the reason in note.
type nugetMapDecision struct {
	module   string
	version  string
	mapped   bool
	nugetID  string
	status   string
	layer    string
	fromLock bool
	note     string

	// S3b: whether the mapping was APPLIED (the module is referenced as its published package rather than
	// converted), and the exact package version it is pinned to.
	applied        bool
	packageVersion string
}

// resolveNuGetMappings decides every module, in module-path order, against the sources and the lock:
//
//	a replace directive          -> unmapped, never overridden
//	-nuget-map-exclude           -> unmapped
//	a lock entry (no -refresh)   -> the lock wins; a source that now disagrees is a WARNING, never adopted
//	the first source naming it   -> canonical/community map it; withdrawn unmaps it and ends the lookup,
//	                                so a user file can withdraw a registry row
//	-nuget-map-canonical-only    -> a community answer (from a source or the lock) is unmapped
//	no source names it           -> unmapped (converted locally)
func resolveNuGetMappings(modules []thirdPartyModule, sources []nugetMapSource, lock map[string]nugetLockEntry, o nugetMapOptions) ([]nugetMapDecision, []string) {
	ordered := slices.Clone(modules)
	sort.Slice(ordered, func(i, j int) bool { return ordered[i].path < ordered[j].path })

	var decisions []nugetMapDecision
	var warnings []string

	for _, mod := range ordered {
		decision := nugetMapDecision{module: mod.path, version: mod.version}

		if mod.replaced {
			decision.note = "replaced locally (a replace directive); a mapping never overrides it"
			decisions = append(decisions, decision)
			continue
		}

		if slices.Contains(o.exclude, mod.path) {
			decision.note = "excluded (-nuget-map-exclude)"
			decisions = append(decisions, decision)
			continue
		}

		row, answeredBy, found := firstNuGetMapRow(sources, mod.path)

		if locked, isLocked := lock[mod.path]; isLocked && !o.refresh {
			decision.mapped, decision.nugetID, decision.status, decision.layer, decision.fromLock = true, locked.nugetID, locked.status, locked.layer, true

			if locked.version != mod.version {
				decision.note = fmt.Sprintf("module version moved from %s", locked.version)
			}

			if disagreement := nugetLockDisagreement(locked, row, answeredBy, found); disagreement != "" {
				warnings = append(warnings, fmt.Sprintf("%s: %s; the lock is kept -- pass -nuget-map-refresh to adopt the sources' answer", mod.path, disagreement))
			}
		} else if found {
			decision.status, decision.layer = row.status, answeredBy

			if row.status == "withdrawn" {
				decision.note = "withdrawn in " + answeredBy
			} else {
				decision.mapped, decision.nugetID = true, row.nugetID
			}
		} else {
			decision.note = "no mapping in any source"
		}

		if decision.mapped && o.canonicalOnly && decision.status != "canonical" {
			decision.mapped = false
			decision.note = fmt.Sprintf("%s mapping to %s in %s skipped (-nuget-map-canonical-only)", decision.status, decision.nugetID, decision.layer)
		}

		decisions = append(decisions, decision)
	}

	return decisions, warnings
}

// firstNuGetMapRow returns the first source's row for a module, and that source's name.
func firstNuGetMapRow(sources []nugetMapSource, modulePath string) (nugetMapRow, string, bool) {
	for _, src := range sources {
		if row, ok := src.rows[modulePath]; ok {
			return row, src.name, true
		}
	}

	return nugetMapRow{}, "", false
}

// nugetLockDisagreement describes how the sources' current answer differs from a locked mapping, or
// returns "" when they agree.
func nugetLockDisagreement(locked nugetLockEntry, row nugetMapRow, answeredBy string, found bool) string {
	lockedText := fmt.Sprintf("%s (%s, %s)", locked.nugetID, locked.status, locked.layer)

	switch {
	case !found:
		return fmt.Sprintf("%s locks %s, but no source maps the module now", nugetLockFileName, lockedText)
	case row.status == "withdrawn":
		return fmt.Sprintf("%s locks %s, but %s now marks it withdrawn", nugetLockFileName, lockedText, answeredBy)
	case row.nugetID != locked.nugetID || row.status != locked.status:
		return fmt.Sprintf("%s locks %s, but the sources now answer %s (%s, %s)", nugetLockFileName, lockedText, row.nugetID, row.status, answeredBy)
	}

	return ""
}

// runNuGetMapResolution is what ModuleConverter.ConvertModule calls: when the options are active it
// reads every source, decides every module against them and the output root's lock, and records the
// mapped modules in the lock. Entries for modules outside this run are kept -- one output root can hold
// several modules' conversions, and the lock is that root's record (the go2cs.modules.lock rule).
func runNuGetMapResolution(modules []thirdPartyModule, o nugetMapOptions, outRoot string) ([]nugetMapDecision, []string, error) {
	if !o.active() {
		return nil, nil, nil
	}

	var sources []nugetMapSource
	var warnings []string

	for _, name := range nugetMapSourceList(o) {
		src, sourceWarnings, err := loadNuGetMapSource(name, o.refresh)

		if err != nil {
			return nil, nil, err
		}

		sources = append(sources, src)
		warnings = append(warnings, sourceWarnings...)
	}

	lock, err := readNuGetLock(outRoot)

	if err != nil {
		return nil, nil, err
	}

	decisions, resolveWarnings := resolveNuGetMappings(modules, sources, lock, o)
	warnings = append(warnings, resolveWarnings...)

	for _, decision := range decisions {
		if !decision.mapped {
			delete(lock, decision.module)
			continue
		}

		entry := nugetLockEntry{module: decision.module, version: decision.version, nugetID: decision.nugetID, status: decision.status, layer: decision.layer, packageVersion: "-", contentHash: "-"}

		if previous, kept := lock[decision.module]; kept && decision.fromLock {
			entry.packageVersion, entry.contentHash = previous.packageVersion, previous.contentHash
		}

		lock[decision.module] = entry
	}

	if err := writeNuGetLock(outRoot, lock); err != nil {
		return nil, nil, fmt.Errorf("writing %s: %w", nugetLockPath(outRoot), err)
	}

	return decisions, warnings, nil
}

// nugetMapNotApplied is the S3a column on every mapped row: the mapping is resolved and locked, but the
// substitution is stage S3b, so the module is still converted locally. S3b removes it.
const nugetMapNotApplied = "resolved, not yet applied (S3b): converted locally"

// formatNuGetMapReport renders the provenance report (docs/PLAN-nugetgo.md 4.5): every warning first,
// then one line per module. A canonical mapping reads as routine; a community mapping is marked TRUST,
// because using it is a trust decision the user is making.
func formatNuGetMapReport(decisions []nugetMapDecision, warnings []string) string {
	var report strings.Builder
	mapped := 0

	for _, decision := range decisions {
		if decision.mapped {
			mapped++
		}
	}

	for _, warning := range warnings {
		fmt.Fprintf(&report, "WARNING (-nuget-map): %s\n", warning)
	}

	fmt.Fprintf(&report, "\nNuGet mappings (-nuget-map): %d third-party module(s), %d mapped\n", len(decisions), mapped)

	for _, decision := range decisions {
		subject := decision.module

		if decision.version != "" {
			subject += "@" + decision.version
		}

		if !decision.mapped {
			fmt.Fprintf(&report, "  %s  ->  (unmapped)  %s\n", subject, decision.note)
			continue
		}

		trust := ""

		if decision.status == "community" {
			trust = "  TRUST: a community mapping, not the module owner's own"
		}

		layer := decision.layer

		if decision.fromLock {
			layer += ", from " + nugetLockFileName
		}

		note := ""

		if decision.note != "" {
			note = "  (" + decision.note + ")"
		}

		fmt.Fprintf(&report, "  %s  ->  %s  %s  [%s]%s  %s%s\n", subject, decision.nugetID, decision.status, layer, trust, nugetMapNotApplied, note)
	}

	return report.String()
}
