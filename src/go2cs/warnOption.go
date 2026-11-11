// warnOption.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The -warn option: an OPT-IN, per-code downgrade of a converter refusal to a warning, so a package that holds a
// refused site still produces code (the owner's ruling for .4: "some code" rather than none).
//
// A code is allowed only when every site it covers becomes code that FAILS LOUD WHEN REACHED, a panic carrying the
// refusal text, and never a copy that runs and quietly gives the wrong result. Each downgraded site is reported as a
// warning (the same canonical line, `warning` in place of `error`) and recorded in package_info.cs as
// `[assembly: GoRefusalDowngraded(...)]`, which go2cs-gen reports in every C# build of the project (GO2CS0011), so
// a project nobody reconverts still shows it carries a known-broken path.

package main

import (
	"fmt"
	"path/filepath"
	"sort"
	"strings"
)

// warnAllowedCodes are the refusal codes -warn may downgrade, each with the reason its downgraded sites fail loud.
var warnAllowedCodes = map[string]string{
	// A self-referential constraint proxy's unforwardable method is emitted by go2cs-gen as a forwarder that panics
	// with the refusal text (ImplementGenerator.RefusedForwarderBody), so the program runs until that call.
	diagnosticProxyRefused: "the proxy's unforwardable method panics with the refusal text when called",
}

// warnRefusedCodes are the KNOWN refusal codes -warn may not downgrade, each with the reason.
var warnRefusedCodes = map[string]string{
	diagnosticCgoRefused:                  "a whole-package refusal, so there is no per-site code to produce",
	diagnosticManualTypeReceiverRefused:   "its downgraded site has no loud form: the conversion would take the receiver's value through a box-from-value record, which go2cs-gen refuses (GO2CS0008)",
	diagnosticGenericInstantiationRefused: "retired, so it is never reported",
	diagnosticNonSelfProxyRefused:         "its downgraded sites are not loud: most would not compile, and a zero value boxed to an interface would be silently wrong",
	diagnosticSliceWideningRefused:        "its downgraded sites are silent: the callee's writes go to an element-wise copy and the program runs on with a wrong result",
}

// parseWarnCodes reads -warn's comma-separated list. An unknown code, or a known code that is not allowed, is an
// error that names it (and, for a known one, why it cannot be downgraded).
func parseWarnCodes(value string) (map[string]bool, error) {
	codes := map[string]bool{}

	for _, field := range strings.Split(value, ",") {
		code := strings.ToUpper(strings.TrimSpace(field))

		if code == "" {
			continue
		}

		if _, allowed := warnAllowedCodes[code]; allowed {
			codes[code] = true
			continue
		}

		if reason, known := warnRefusedCodes[code]; known {
			return nil, fmt.Errorf("-warn cannot downgrade %s: %s", code, reason)
		}

		return nil, fmt.Errorf("-warn: %q is not a go2cs refusal code (allowed: %s)", field, strings.Join(warnAllowedCodeList(), ", "))
	}

	return codes, nil
}

// warnAllowedCodeList is the allow-list, sorted, for messages and help.
func warnAllowedCodeList() []string {
	list := make([]string, 0, len(warnAllowedCodes))

	for code := range warnAllowedCodes {
		list = append(list, code)
	}

	sort.Strings(list)

	return list
}

// downgradedRefusal is one site converted under -warn, recorded for package_info.cs.
type downgradedRefusal struct {
	code string
	site string
	text string
}

// downgradedRefusals are this package's downgraded sites (reset with the rest of the package state).
var downgradedRefusals []downgradedRefusal

// recordDowngradedRefusals reports each site as a WARNING and records it for package_info.cs. The site names the
// file by its base name and line, never an absolute path, so the record is the same on every machine.
func recordDowngradedRefusals(code string, sites []constraintProxyRefusalSite) {
	for _, site := range sites {
		reportWarningDiagnostic(site.position.String(), code, site.text)

		downgradedRefusals = append(downgradedRefusals, downgradedRefusal{
			code: code,
			site: fmt.Sprintf("%s:%d", filepath.Base(site.position.Filename), site.position.Line),
			// One line, as the canonical diagnostic prints it, so the C# string literal needs no line escapes.
			text: diagnosticLineBreaks.ReplaceAllString(strings.TrimSpace(site.text), " "),
		})
	}
}

// downgradedRefusalRecords renders the package's downgraded sites as package_info.cs assembly attributes.
func downgradedRefusalRecords() []string {
	records := make([]string, 0, len(downgradedRefusals))

	for _, refusal := range downgradedRefusals {
		records = append(records, fmt.Sprintf("[assembly: GoRefusalDowngraded(%s, %s, %s)]", csharpStringLiteral(refusal.code), csharpStringLiteral(refusal.site), csharpStringLiteral(refusal.text)))
	}

	return records
}
