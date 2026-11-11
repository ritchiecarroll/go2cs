// toolchainCommand.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"flag"
	"fmt"
	"io"
	"os"
	"path/filepath"
	"runtime"
	"slices"
	"sort"
	"strings"
)

const toolchainUsage = `usage: go2cs toolchain <command>

The Go release go2cs converts with, which a release build downloads on first use (see GO2CS_GOROOT).

  install [-from <file>]  download the pinned Go now, or install it from a release archive or
                          golang.org/toolchain module zip you already have (checked against the pin)
  env                     print the GOROOT, GOVERSION and go command go2cs converts with, downloading
                          if needed
  prune [-all]            remove cached Go releases other than the pinned one (-all: the pinned one too)

The C toolchain a -cgo load provisions when no host compiler resolves (see -cc):

  install -cc [-pin zig|llvm-mingw] [-from <file>]
                          download the pinned C toolchain now (llvm-mingw on windows, zig elsewhere, or the
                          one -pin names), or install it from a release archive you already have (checked
                          against the pin)
`

// runToolchainCommand runs `go2cs toolchain ...` and returns the process exit code. main dispatches here
// when the first argument is "toolchain"; a directory of that name converts as ./toolchain.
func runToolchainCommand(args []string, stdout, stderr io.Writer) int {
	if len(args) == 0 || args[0] == "-h" || args[0] == "-help" || args[0] == "--help" || args[0] == "help" {
		fmt.Fprint(stdout, toolchainUsage)
		return 0
	}

	// `install -cc` is the C toolchain's (ccPinnedToolchain.go), whatever Go this build provisions.
	if args[0] == "install" && slices.Contains(args[1:], "-cc") {
		return runCCInstall(args[1:], stdout, stderr)
	}

	provisioner := newToolchainProvisioner()

	if provisioner == nil {
		fmt.Fprintf(stderr, "go2cs toolchain: this go2cs was built with %s and provisions no Go; it converts with the go on PATH\n", runtime.Version())
		return 1
	}

	if err := provisioner.command(args[0], args[1:], stdout); err != nil {
		fmt.Fprintf(stderr, "go2cs toolchain %s: %v\n", args[0], err)
		return 1
	}

	return 0
}

// runCCInstall runs `go2cs toolchain install -cc [-from <file>]`.
func runCCInstall(args []string, stdout, stderr io.Writer) int {
	flags := flag.NewFlagSet("go2cs toolchain install -cc", flag.ContinueOnError)
	flags.SetOutput(io.Discard)
	flags.Bool("cc", false, "")
	from := flags.String("from", "", "")
	pin := flags.String("pin", "", "")

	if err := flags.Parse(args); err != nil || flags.NArg() > 0 {
		fmt.Fprintf(stderr, "go2cs toolchain install -cc: unexpected arguments %q\n%s", args, toolchainUsage)
		return 1
	}

	if *pin != "" && *pin != ccToolchainZig && *pin != ccToolchainLLVMMingw {
		fmt.Fprintf(stderr, "go2cs toolchain install -cc: -pin %s names no pinned C toolchain; the pinned ones are %s and %s\n", *pin, ccToolchainZig, ccToolchainLLVMMingw)
		return 1
	}

	z := newPinnedProvisioner(*pin)

	if z == nil {
		name := *pin

		if name == "" {
			name = defaultCCToolchain(runtime.GOOS)
		}

		fmt.Fprintf(stderr, "go2cs toolchain install -cc: this go2cs has no pinned C toolchain (%s) for %s/%s\n", name, runtime.GOOS, runtime.GOARCH)
		return 1
	}

	if err := z.installCommand(*from); err != nil {
		fmt.Fprintf(stderr, "go2cs toolchain install -cc: %v\n", err)
		return 1
	}

	fmt.Fprintf(stdout, "%s is installed at %s\n", z.label(), z.root())

	return 0
}

func (p *toolchainProvisioner) command(name string, args []string, stdout io.Writer) error {
	flags := flag.NewFlagSet("go2cs toolchain "+name, flag.ContinueOnError)
	flags.SetOutput(io.Discard)

	from := flags.String("from", "", "")
	all := flags.Bool("all", false, "")

	if err := flags.Parse(args); err != nil {
		return err
	}

	if flags.NArg() > 0 {
		return fmt.Errorf("unexpected argument %q\n%s", flags.Arg(0), toolchainUsage)
	}

	switch name {
	case "install":
		root := p.cachedRoot()

		switch {
		case *from != "":
			// Checked even when the release is already installed, so a wrong file is never reported fine.
			if err := p.installLocal(*from); err != nil {
				return err
			}
		case p.checkRelease(root) != nil:
			if !filepath.IsAbs(p.cacheRoot) {
				return fmt.Errorf("no user cache directory to install into; set %s to an existing %s instead", goRootOverrideEnv, p.goVersion)
			}

			if err := p.install(root); err != nil {
				return err
			}
		}

		fmt.Fprintf(stdout, "%s is installed at %s\n", p.goVersion, root)
	case "env":
		if err := p.ensure(true); err != nil {
			return err
		}

		root := p.getenv("GOROOT")

		if root == "" {
			value, err := getGoEnv("GOROOT")
			if err != nil {
				return err
			}

			root = value
		}

		// GO is the command itself, for a script that puts this Go first on PATH (src/tour/scripts).
		fmt.Fprintf(stdout, "GOROOT=%s\nGOVERSION=%s\nGO=%s\n", root, p.goVersion, filepath.Join(root, "bin", "go"+exeSuffix(p.goos)))
	case "prune":
		removed, err := p.prune(*all)
		if err != nil {
			return err
		}

		if len(removed) == 0 {
			fmt.Fprintln(stdout, "nothing to remove")
		}

		for _, dir := range removed {
			fmt.Fprintf(stdout, "removed %s\n", dir)
		}
	default:
		return fmt.Errorf("unknown command\n%s", toolchainUsage)
	}

	return nil
}

// prune removes the Go releases in the cache other than the pinned one, and with all the pinned one
// too. It touches nothing outside the go2cs toolchain cache.
func (p *toolchainProvisioner) prune(all bool) ([]string, error) {
	entries, err := os.ReadDir(p.cacheRoot)
	if os.IsNotExist(err) {
		return nil, nil
	}

	if err != nil {
		return nil, err
	}

	keep := filepath.Base(p.cachedRoot())
	var removed []string

	for _, entry := range entries {
		if !entry.IsDir() || (!all && entry.Name() == keep) {
			continue
		}

		// Only what this cache creates: go<release>.<goos>-<goarch> and interrupted unpacks.
		if !strings.HasPrefix(entry.Name(), "go") && !strings.HasPrefix(entry.Name(), ".unpack-") {
			continue
		}

		dir := filepath.Join(p.cacheRoot, entry.Name())

		if err := os.RemoveAll(dir); err != nil {
			return removed, err
		}

		removed = append(removed, dir)
	}

	sort.Strings(removed)

	return removed, nil
}
