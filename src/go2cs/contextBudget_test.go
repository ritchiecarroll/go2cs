// contextBudget_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"crypto/sha1" //nolint:gosec // git object names ARE SHA-1; this reproduces git's hash, not a security digest
	"encoding/hex"
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
)

// CLAUDE.md is loaded in full at the start of every session, after every /compact, and into every
// subagent. Between 2026-08-01 and 2026-09-08 it grew from 48 KB to 745 KB -- 7,836 lines, roughly
// 200K tokens, about 35-40% of an average turn's context -- and at that size NO SUBAGENT COULD BE
// SPAWNED IN THIS REPO AT ALL, because the project instructions alone exceeded a subagent's context
// window. That is not a hypothetical: it killed one on 2026-09-12 during the analysis that produced
// docs/PLAN-context-diet.md.
//
// The split moved the detail into .claude/rules (path-scoped, loads only when a matching file is
// read) and .claude/skills (loads on demand), leaving CLAUDE.md as an index plus a safety floor.
// This guard is what keeps it that way. It exists because the repo has learned, repeatedly, that a
// lesson living in attention rather than in a script fails under exactly the conditions the script
// exists for -- and "do not let CLAUDE.md grow" is the most attention-dependent rule imaginable,
// since every individual addition looks small and justified.
//
// The budget is measured on EFFECTIVE lines: block HTML comments are stripped by Claude Code before
// a file enters context, so provenance kept in <!-- --> costs zero tokens and is deliberately not
// counted. That is what makes "keep the evidence, move it into a comment" an honest instruction
// rather than a way to game the cap.
//
// POSITIVE CONTROL: append 25 non-comment lines to CLAUDE.md and TestContextBudgetCLAUDEmd must go
// RED naming the effective count; wrap the same 25 lines in <!-- --> and it must stay GREEN. Both
// directions were verified when this guard landed.

const (
	// claudeMdMaxEffectiveLines is the cap on CLAUDE.md after HTML comments are stripped. Raising
	// it is a deliberate act that belongs in the same commit as the reason, per the "This file's
	// own budget" section of CLAUDE.md itself.
	claudeMdMaxEffectiveLines = 200

	// safetyFloorMaxItems caps the numbered list under "## The safety floor" -- the only
	// always-loaded rule list. Adding a rule there means removing one, or raising this cap on
	// purpose and recording why.
	safetyFloorMaxItems = 20

	// contextJournalPath is the byte-identical pre-split CLAUDE.md. Every rule that reads thin in
	// its new home has its full dated derivation here, so the split lost nothing.
	contextJournalPath = "docs/doctrine/JOURNAL-2026-09-12.md"

	// contextJournalBlob is the git object name the journal MUST hash to: it is the very same
	// object as CLAUDE.md@1800b04f8, the commit before the split. That identity is the whole value
	// of the file -- it is what makes the journal a PROOF rather than another copy. Until
	// 2026-09-12 nothing asserted it: TestContextBudgetJournalPresent checked existence and a
	// minimum size, so the journal could be appended to, edited, or truncated to 600 KB and the
	// suite stayed green, while every distilled rule's claim to be recoverable quietly stopped
	// being true. The SHA was verified by hand on each commit of the split, which is exactly the
	// "lives in attention rather than in a guard" failure this file exists to prevent.
	//
	// NEVER APPEND TO THE JOURNAL. It is a frozen point-in-time object, not a running log. New
	// doctrine goes to a rule, a skill or a record (see the routing table in CLAUDE.md); material
	// that was never in the pre-split CLAUDE.md -- batch19's 1,355 lines, for instance -- has its
	// own loss-proof in its own git objects, not here.
	contextJournalBlob = "4730713b8ae419aa4282379cad291e9f97ea8380"
)

var htmlCommentPattern = regexp.MustCompile(`(?s)<!--.*?-->`)

// effectiveLines returns the line count of content that actually reaches Claude's context: the file
// with block HTML comments removed.
func effectiveLines(content string) int {
	return len(strings.Split(htmlCommentPattern.ReplaceAllString(content, ""), "\n"))
}

func readRepoFile(t *testing.T, relative string) string {
	t.Helper()

	full := filepath.Join(repoRootFromPackageDir(t), filepath.FromSlash(relative))

	data, err := os.ReadFile(full)

	if err != nil {
		t.Fatalf("cannot read %s: %v", relative, err)
	}

	return string(data)
}

// TestContextBudgetCLAUDEmd holds the always-loaded surface to its cap.
func TestContextBudgetCLAUDEmd(t *testing.T) {
	content := readRepoFile(t, "CLAUDE.md")
	effective := effectiveLines(content)

	if effective > claudeMdMaxEffectiveLines {
		t.Errorf("CLAUDE.md is %d effective lines, over the %d-line cap.\n"+
			"CLAUDE.md loads into EVERY session, after every /compact, and into every subagent.\n"+
			"Route the addition instead, per the \"This file's own budget\" table in CLAUDE.md:\n"+
			"  - must be known before any file is read -> the safety floor (which is itself capped)\n"+
			"  - applies to one part of the tree       -> .claude/rules/<topic>.md with paths: frontmatter\n"+
			"  - is a procedure you invoke             -> .claude/skills/<name>/SKILL.md\n"+
			"  - is a finding or measurement           -> the BOARD or a docs/ record\n"+
			"Dated provenance belongs in an HTML comment: it is stripped before the file enters\n"+
			"context, so it does not count against this cap and costs zero tokens.",
			effective, claudeMdMaxEffectiveLines)
	}
}

// TestContextBudgetSafetyFloor caps the one always-loaded rule list.
func TestContextBudgetSafetyFloor(t *testing.T) {
	content := readRepoFile(t, "CLAUDE.md")

	const heading = "## The safety floor"

	start := strings.Index(content, heading)

	if start < 0 {
		t.Fatalf("CLAUDE.md has no %q section; the safety floor is the one list that must stay "+
			"always-loaded, so its absence is a defect rather than a passing check", heading)
	}

	section := content[start+len(heading):]

	if next := strings.Index(section, "\n## "); next >= 0 {
		section = section[:next]
	}

	item := regexp.MustCompile(`(?m)^\d+\.\s+\*\*`)
	items := item.FindAllString(section, -1)

	if len(items) == 0 {
		t.Fatal("the safety floor section matched zero numbered items -- this guard would pass " +
			"vacuously, so the item pattern and the section have drifted apart")
	}

	if len(items) > safetyFloorMaxItems {
		t.Errorf("the safety floor carries %d items, over the cap of %d. Adding one means removing "+
			"one, or raising the cap deliberately and recording why in CLAUDE.md.",
			len(items), safetyFloorMaxItems)
	}
}

// TestContextBudgetRulesArePathScoped reports any .claude/rules file that loads unconditionally.
//
// A rule WITHOUT paths: frontmatter is loaded at launch with the same priority as CLAUDE.md, so it
// spends the budget this guard exists to protect. That is legitimate for a genuinely universal rule
// and a defect for anything else, so this fails loudly enough to force the decision.
func TestContextBudgetRulesArePathScoped(t *testing.T) {
	rulesDir := filepath.Join(repoRootFromPackageDir(t), ".claude", "rules")

	entries, err := os.ReadDir(rulesDir)

	if err != nil {
		if os.IsNotExist(err) {
			t.Skip(".claude/rules does not exist; nothing to check")
		}

		t.Fatalf("cannot read .claude/rules: %v", err)
	}

	var unscoped []string

	for _, entry := range entries {
		if entry.IsDir() || !strings.HasSuffix(entry.Name(), ".md") {
			continue
		}

		// A probe file is a disposable instrument, not doctrine.
		if strings.HasPrefix(entry.Name(), "_") {
			continue
		}

		data, err := os.ReadFile(filepath.Join(rulesDir, entry.Name()))

		if err != nil {
			t.Fatalf("cannot read rule %s: %v", entry.Name(), err)
		}

		content := string(data)

		if !strings.HasPrefix(content, "---") || !strings.Contains(content, "\npaths:") {
			unscoped = append(unscoped, entry.Name())
		}
	}

	sort.Strings(unscoped)

	if len(unscoped) > 0 {
		t.Errorf("these .claude/rules files carry no paths: frontmatter and therefore load into "+
			"EVERY session: %s\nScope them to the subtree they describe, or move them to a skill.",
			strings.Join(unscoped, ", "))
	}
}

// TestContextBudgetSkillsAreDescribed asserts every skill is invokable.
//
// Only a skill's name and description are always loaded; the body loads on demand. A skill missing
// either is unreachable, which silently turns its content into material nothing can pull in.
func TestContextBudgetSkillsAreDescribed(t *testing.T) {
	skillsDir := filepath.Join(repoRootFromPackageDir(t), ".claude", "skills")

	entries, err := os.ReadDir(skillsDir)

	if err != nil {
		if os.IsNotExist(err) {
			t.Skip(".claude/skills does not exist; nothing to check")
		}

		t.Fatalf("cannot read .claude/skills: %v", err)
	}

	found := 0

	for _, entry := range entries {
		if !entry.IsDir() {
			continue
		}

		path := filepath.Join(skillsDir, entry.Name(), "SKILL.md")

		data, err := os.ReadFile(path)

		if err != nil {
			t.Errorf("skill %s has no SKILL.md: %v", entry.Name(), err)
			continue
		}

		found++
		content := string(data)

		if !strings.HasPrefix(content, "---") {
			t.Errorf("skill %s: SKILL.md has no YAML frontmatter", entry.Name())
			continue
		}

		if !strings.Contains(content, "\nname: ") {
			t.Errorf("skill %s: SKILL.md frontmatter has no name field", entry.Name())
		}

		if !strings.Contains(content, "\ndescription: ") {
			t.Errorf("skill %s: SKILL.md frontmatter has no description field; without one the "+
				"skill cannot be pulled in by relevance and its content is unreachable", entry.Name())
		}
	}

	if found == 0 {
		t.Error("no skills found under .claude/skills -- this guard would pass vacuously")
	}
}

// TestContextBudgetJournalPresent asserts the pre-split original is still on disk.
//
// The journal is what makes the split lossless: it is the byte-identical CLAUDE.md from before the
// 2026-09-12 extraction. Every distilled rule's full dated derivation is recoverable from it, so a
// compression mistake in .claude/rules or .claude/skills costs prominence, never evidence.
func TestContextBudgetJournalPresent(t *testing.T) {
	info, err := os.Stat(filepath.Join(repoRootFromPackageDir(t), filepath.FromSlash(contextJournalPath)))

	if err != nil {
		t.Fatalf("the doctrine journal %s is missing: %v\nIt is the loss-proof for the CLAUDE.md "+
			"split; without it the distilled files are the only copy.", contextJournalPath, err)
	}

	const minimumJournalBytes = 512 * 1024

	if info.Size() < minimumJournalBytes {
		t.Errorf("the doctrine journal %s is %d bytes, under the %d expected for the pre-split "+
			"CLAUDE.md -- it looks truncated rather than complete",
			contextJournalPath, info.Size(), minimumJournalBytes)
	}

	// Size and existence do not make the journal a proof; BLOB IDENTITY does. Hash it the way git
	// does and require the object name the pre-split CLAUDE.md had.
	if got := gitBlobName(t, contextJournalPath); got != contextJournalBlob {
		t.Errorf("the doctrine journal %s hashes to %s, not %s.\n"+
			"It is supposed to be the SAME GIT OBJECT as CLAUDE.md@1800b04f8 -- that identity is\n"+
			"what makes every distilled rule's derivation recoverable, and it is the only property\n"+
			"of this file that matters. If you appended to it: don't. The journal is frozen.\n"+
			"Route new doctrine per the table in CLAUDE.md (safety floor / rules / skills / records).\n"+
			"If you edited or reflowed it: restore with `git checkout %s -- %s`.",
			contextJournalPath, got, contextJournalBlob, contextJournalBlob[:9], contextJournalPath)
	}
}

// gitBlobName reproduces `git hash-object` for a text file: sha1("blob <len>\x00" + LF-normalized
// content). The normalization is not optional -- the repo checks out with core.autocrlf=true, so
// the journal is CRLF on disk (770,694 bytes) while its blob is LF (762,859). Hashing the bytes as
// they sit on disk would fail on every Windows checkout and pass on Linux, which is a guard that
// reports the platform rather than the file.
func gitBlobName(t *testing.T, relative string) string {
	t.Helper()

	raw, err := os.ReadFile(filepath.Join(repoRootFromPackageDir(t), filepath.FromSlash(relative)))

	if err != nil {
		t.Fatalf("cannot read %s: %v", relative, err)
	}

	content := bytes.ReplaceAll(raw, []byte("\r\n"), []byte("\n"))
	digest := sha1.Sum(append([]byte(fmt.Sprintf("blob %d\x00", len(content))), content...)) //nolint:gosec

	return hex.EncodeToString(digest[:])
}
