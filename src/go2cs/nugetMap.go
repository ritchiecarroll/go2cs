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

	"go2cs/internal/sourcemeta"

	"golang.org/x/mod/module"
)

// The -nuget-map resolver (docs/PLAN-nugetgo.md section 4; stages S3a and S3b).
//
// nugetgo.net maps Go module paths to published NuGet packages holding their go2cs conversions. Under
// -recurse=nuget this file decides, per third-party module, whether a mapping answers it: it reads the
// mapping sources (the user's -nuget-map files and URLs in listed order, then the registry unless
// -nuget-map-only), applies the exclusions and the lock (nugetLock.go), records the result in
// go2cs.nuget.lock and prints the provenance report. A mapped module is then SUBSTITUTED when a published
// version of its package qualifies (nugetPackage.go) and its dependency closure stays consistent
// (nugetSubstitution.go): it is referenced as that package instead of being converted.
//
// ON BY DEFAULT since S3b (COORD ruling 2026-10-02): a -recurse=nuget run consults the registry with no
// -nuget-map flag at all; -nuget-map off is the one way to opt out. S3a shipped the resolver dormant, so
// the default and the substitution switched on together.

// nugetMapOptions is the -nuget-map flag family, as parsed.
type nugetMapOptions struct {
	sources       []string // -nuget-map <url|file>: in listed order, which is precedence (first row naming a module wins)
	off           bool     // -nuget-map off: no source is read and no lock is read or written
	only          bool     // -nuget-map-only: drop the registry fallback, so only the listed sources answer
	exclude       []string // -nuget-map-exclude <module-path>: modules that are never mapped
	refresh       bool     // -nuget-map-refresh: bypass the cache and re-resolve deliberately instead of keeping the lock
	canonicalOnly bool     // -nuget-map-canonical-only: a community mapping is treated as unmapped
}

// active reports whether this run resolves mappings at all: always, unless -nuget-map off.
func (o nugetMapOptions) active() bool {
	return !o.off
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
//
// A well-formed row whose ID the ID PATTERN refuses (sourcemeta.CheckModuleNuGetID: the reserved "go."
// prefix) is the exception: it is SKIPPED, and a warning names it by file:line. The pattern is a POLICY,
// which a newer converter can apply more strictly than an older registry row was linted against, so one
// such row must never refuse the source -- under -recurse=nuget the registry is read by default, and a
// refused source fails every user's conversion (COORD 2026-10-03).
func parseNuGetMap(name string, data []byte) (nugetMapSource, []string, error) {
	src := nugetMapSource{name: name, rows: make(map[string]nugetMapRow)}
	var warnings []string

	for index, line := range strings.Split(string(data), "\n") {
		lineNumber := index + 1
		line = strings.TrimSuffix(line, "\r")

		if strings.TrimSpace(line) == "" || strings.HasPrefix(line, "#") {
			continue
		}

		fields := strings.Split(line, "\t")

		if len(fields) != nugetMapFieldCount {
			return nugetMapSource{}, nil, fmt.Errorf("%s:%d: want 6 TAB-separated fields (module-path, nuget-id, status, source-repo, registered, contact), got %d", name, lineNumber, len(fields))
		}

		row := nugetMapRow{module: fields[0], nugetID: fields[1], status: fields[2], sourceRepo: fields[3], registered: fields[4], contact: fields[5], line: lineNumber}

		if err := module.CheckPath(row.module); err != nil {
			return nugetMapSource{}, nil, fmt.Errorf("%s:%d: invalid module path %q: %v", name, lineNumber, row.module, err)
		}

		if row.nugetID == "" || strings.ContainsAny(row.nugetID, " \t") || len(row.nugetID) > 100 {
			return nugetMapSource{}, nil, fmt.Errorf("%s:%d: invalid nuget-id %q (non-empty, no whitespace, at most 100 characters)", name, lineNumber, row.nugetID)
		}

		if !slices.Contains(nugetMapStatuses, row.status) {
			return nugetMapSource{}, nil, fmt.Errorf("%s:%d: invalid status %q (want canonical, community or withdrawn)", name, lineNumber, row.status)
		}

		if first, duplicate := src.rows[row.module]; duplicate {
			return nugetMapSource{}, nil, fmt.Errorf("%s:%d: a second row for module %s (first at line %d); a source maps each module path once", name, lineNumber, row.module, first.line)
		}

		// After every format check, so a malformed ID is refused for its malformation alone.
		if err := sourcemeta.CheckModuleNuGetID(row.nugetID); err != nil {
			warnings = append(warnings, fmt.Sprintf("%s:%d: %v; the row is skipped and %s stays unmapped by this source", name, lineNumber, err, row.module))
			continue
		}

		src.rows[row.module] = row
	}

	return src, warnings, nil
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

		locked, isLocked := lock[mod.path]

		// The lock is the fourth reader of a mapping's ID: a pin the ID pattern refuses (written before the
		// rule, or edited by hand) is dropped with a warning, and the module re-resolves from the sources.
		if isLocked && !o.refresh {
			if err := sourcemeta.CheckModuleNuGetID(locked.nugetID); err != nil {
				warnings = append(warnings, fmt.Sprintf("%s: %s pins %v; the pin is dropped and the module re-resolves from the sources", mod.path, nugetLockFileName, err))
				isLocked = false
			}
		}

		if isLocked && !o.refresh {
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
	// A module with no third-party module has nothing to map: no source is read, so the default registry is not
	// fetched, and no lock is written (the nugetgo rehearsal, 2026-10-09, gap 8).
	if !o.active() || len(modules) == 0 {
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

// formatNuGetMapReport renders the provenance report (docs/PLAN-nugetgo.md 4.5): every warning first,
// then one line per module. A mapping that was APPLIED names the exact-pinned PackageReference it became; a
// mapping that was not says the module converted locally, and why. A community mapping is marked TRUST,
// because using it is a trust decision the user is making.
func formatNuGetMapReport(decisions []nugetMapDecision, warnings []string) string {
	var report strings.Builder
	mapped, applied := 0, 0

	for _, decision := range decisions {
		if decision.mapped {
			mapped++
		}

		if decision.applied {
			applied++
		}
	}

	for _, warning := range warnings {
		fmt.Fprintf(&report, "WARNING (-nuget-map): %s\n", warning)
	}

	fmt.Fprintf(&report, "\nNuGet mappings (-nuget-map): %d third-party module(s), %d mapped, %d referenced as packages\n", len(decisions), mapped, applied)

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

		outcome := ""

		if decision.applied {
			outcome = fmt.Sprintf("PackageReference %s [%s]", decision.nugetID, decision.packageVersion)
		} else {
			outcome = "converted locally: " + decision.note
		}

		fmt.Fprintf(&report, "  %s  ->  %s  %s  [%s]%s  %s\n", subject, decision.nugetID, decision.status, layer, trust, outcome)
	}

	return report.String()
}
