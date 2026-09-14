// projectFileWriter.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// This file owns everything the converter writes that is NOT C# source: the .csproj, the icons
// and publish profiles beside it, and the mechanics of getting an output file onto disk at all.
//
// The .csproj is what makes converted code buildable — it carries the golib runtime reference, the
// go2cs-gen analyzer reference, and one ProjectReference per imported Go package — so this file is
// effectively the converter's build-system emitter.
//
// needToWriteFile is the reason a reconvert of an unchanged package leaves the tree clean: a file
// whose bytes would not change is not rewritten, so timestamps (and any up-to-date check built on
// them) stay meaningful.

package main

import (
	"bytes"
	"fmt"
	"go/types"
	"os"
	"os/exec"
	"path"
	"path/filepath"
	"sort"
	"strconv"
	"strings"
)

func getGoEnv(name string) (string, error) {
	return getGoEnvFrom("", name)
}

// getGoEnvFrom runs `go env <name>` from dir (the process working directory when dir is empty).
// The directory matters for anything GOTOOLCHAIN can change: with GOTOOLCHAIN=auto the go command
// re-execs whichever toolchain the module found by walking up from dir asks for, so asking from the
// same directory go/packages loads from is what makes the two agree.
func getGoEnvFrom(dir string, name string) (string, error) {
	cmd := exec.Command("go", "env", name)
	cmd.Dir = dir

	var out bytes.Buffer

	cmd.Stdout = &out
	err := cmd.Run()

	if err != nil {
		return "", fmt.Errorf("failed to get Go environment %s: %w", name, err)
	}

	return strings.TrimSpace(out.String()), nil
}

// prepareProjectFiles writes the project related files for the given project name and path,
// and returns project file contents with template parameters to be written to a file later.
func prepareProjectFiles(projectName string, packageNamespace string, projectPath string) (string, string, error) {
	// Make sure project path ends with a directory separator
	projectPath = strings.TrimRight(projectPath, string(filepath.Separator)) + string(filepath.Separator)

	// Ensure project directory exists
	if err := os.MkdirAll(projectPath, 0755); err != nil {
		return "", "", fmt.Errorf("failed to create project directory \"%s\": %s", projectPath, err)
	}

	iconFileName := projectPath + "go2cs.ico"

	// Check if icon file needs to be written
	if needToWriteFile(iconFileName, iconFileBytes) {
		iconFile, err := os.Create(iconFileName)

		if err != nil {
			return "", "", fmt.Errorf("failed to create icon file \"%s\": %s", iconFileName, err)
		}

		defer iconFile.Close()

		_, err = iconFile.Write(iconFileBytes)

		if err != nil {
			return "", "", fmt.Errorf("failed to write to icon file \"%s\": %s", iconFileName, err)
		}
	}

	// Generate project file contents
	projectFileContents := fmt.Sprintf(string(csprojTemplate),
		OutputTypeMarker,
		packageNamespace,
		projectName,
		UnsafeMarker,
		ProjectReferenceMarker,
	)

	if hasSiblingInternalTestFiles {
		projectFileContents = insertFriendAssemblyAccess(projectFileContents)
	}

	// The FILE is named through the path budget; the CONTENTS above already carry the full canonical
	// projectName as AssemblyName (hence PackageId) and the full namespace. Identity and label are
	// deliberately allowed to differ here — see projectFileBaseName.
	projectFileName := projectPath + projectFileBaseName(projectName) + ".csproj"

	return projectFileName, projectFileContents, nil
}

// declaredAssemblyName returns the value of the single <AssemblyName> element a rendered project file
// carries, or "" when the template has none. It reads the rendered CONTENTS rather than inferring the
// name from the .csproj file name, which since the projectFileBaseName path budget is no longer the
// same string for a deep import path.
func declaredAssemblyName(contents string) string {
	const open = "<AssemblyName>"
	const close = "</AssemblyName>"

	start := strings.Index(contents, open)

	if start < 0 {
		return ""
	}

	start += len(open)
	end := strings.Index(contents[start:], close)

	if end < 0 {
		return ""
	}

	return contents[start : start+end]
}

// insertFriendAssemblyAccess grants the package's colocated test assembly access to its internal
// members, and the reflection bridge's STRUCT-SYNTHESIS assembly the same. Inserted AFTER template
// rendering — never as a template verb — so the template keeps its historical verb count and a
// user-supplied `-csproj` template (which cannot know about the slot) renders exactly as before;
// Sprintf would otherwise append a `%!s(EXTRA …)` diagnostic into every generated project. Anchored
// on the first closing PropertyGroup, which every usable template has.
//
// THE SECOND FRIEND, and why it is an accessibility grant rather than anything cleverer.
// `reflect.StructOf` mints a CLR value type into a fixed-name dynamic assembly
// (golib GoStructSynthesis, `go2cs.SynthesizedStructs`). A synthesized struct whose FIELD TYPE is
// internal to a converted package cannot be LOADED from there without a grant, and the failure is
// undiagnosable in place: CreateType throws a bare TypeLoadException naming nothing, and the
// half-built TypeBuilder then poisons the whole assembly for every later GetTypes(). Measured with a
// positive control inside that assembly — `array<ж<int>>` (all public) mints, `array<ж<rtype>>` does
// not, and `rtype` is internal; same shape, same layout, accessibility the only difference.
// reflect.FuncOf hits it head-on, since initFuncTypes asks for `struct{ FuncType abi.FuncType;
// Args [n]*rtype }`.
//
// IVT and not IgnoresAccessChecksTo: that attribute was tried first and measured INERT (identical
// verdicts and identical TypeLoadException counts, base and branch in one batch). It governs the
// JIT's access checks for compiled method bodies; CreateType is the TYPE LOADER's field validation,
// a different enforcement layer. IVT changes accessibility itself, which is the layer that refuses.
//
// IVT and not emitting Go-unexported types as C# public: that would be MORE permissive than Go —
// a public rtype is reachable by every C# consumer of the corpus, where Go grants external packages
// nothing — and Go exportedness here is carried by the NAME's case, not by C# accessibility, so
// widening the C# surface buys nothing Go asked for. One named friend assembly is the narrow grant.
func insertFriendAssemblyAccess(projectFileContents string) string {
	const anchor = "</PropertyGroup>"
	const friendItemGroup = "\r\n\r\n  <!-- Same-package Go tests run in a separate assembly but retain package-private access. -->\r\n  <ItemGroup>\r\n    <InternalsVisibleTo Include=\"$(AssemblyName).tests\" />\r\n    <!-- reflect.StructOf mints synthesized structs into this fixed-name dynamic assembly; a field type that is internal here cannot be loaded from there without the grant. -->\r\n    <InternalsVisibleTo Include=\"go2cs.SynthesizedStructs\" />\r\n  </ItemGroup>"

	idx := strings.Index(projectFileContents, anchor)

	if idx < 0 {
		return projectFileContents
	}

	insertAt := idx + len(anchor)
	return projectFileContents[:insertAt] + friendItemGroup + projectFileContents[insertAt:]
}

// validationPackBlock renders the .csproj block that packs a converted stdlib package's versioned
// validation proof sheet into its .nupkg as VALIDATION.md, or "" for any other conversion.
//
// The block is Exists-guarded on BOTH ends of the question: a package that has not validated has no
// sheet under the versioned directory, and a build outside a repository checkout has no docs tree at
// all — so the same emitted .csproj is correct for a validated package, an unvalidated one, and a
// deployed GOPATH runtime root. That is what lets EVERY stdlib project carry the block: a package
// that validates later starts shipping its sheet with no .csproj change.
//
// The path is composed from $(go2csPath) (the src root, pinned by src\core\Directory.Build.props)
// and the version properties from src\version.props, which that same props file imports — so the
// sheet a package packs is always the one for the version it is being published as.
//
// A run that re-emits the production .csproj of an ALREADY-CONVERTED core package must KEEP the
// block a -stdlib conversion put there: gating on convertStdLib alone stripped it on every -tests
// pipeline run, silently un-shipping the package's proof sheet from the next NuGet pack (the
// standing "0 8" restore family). The second arm is therefore scoped to the OUTPUT LOCATION —
// under the runtime root's core\ tree — and not to the invocation mode, so a behavioral fixture or
// an end-user module keeps its historical .csproj bytes while every regeneration of a corpus
// package keeps its proof sheet.
func validationPackBlock(projectFileName string, options Options) string {
	if !options.convertStdLib && !rewriteOfCorePackage(projectFileName, options) {
		return ""
	}

	dotID := strings.TrimSuffix(filepath.Base(projectFileName), ".csproj")

	return "\r\n" +
		"  <!-- Ship this package's versioned validation proof sheet as VALIDATION.md inside the nupkg -->\r\n" +
		"  <PropertyGroup>\r\n" +
		"    <GoValidationProofFile>$(go2csPath)../docs/validation/$(GoStdLibVersion).$(GoBuildNumber)/" + dotID + ".md</GoValidationProofFile>\r\n" +
		"  </PropertyGroup>\r\n" +
		"  <ItemGroup Condition=\"'$(OutputType)'=='Library' AND Exists('$(GoValidationProofFile)')\">\r\n" +
		"    <None Include=\"$(GoValidationProofFile)\" Pack=\"true\" PackagePath=\"VALIDATION.md\" Visible=\"false\" />\r\n" +
		"  </ItemGroup>\r\n"
}

// rewriteOfCorePackage reports whether this conversion is re-emitting the production .csproj of a
// converted STDLIB package — the one case outside -stdlib where the validation pack block belongs.
// The runtime root is authoritative (self-located by the -tests and single-package entry points
// walking the output dir up to core\golib), so "under <go2csPath>\core\" is a structural test no
// fixture, -recurse output or end-user output path satisfies.
//
// The test is on the OUTPUT LOCATION alone, deliberately. It was scoped to -tests when it was
// first written, which left the plain SINGLE-PACKAGE form (`go2cs <goroot-pkg> <core-pkg>`, how a
// lane regenerates one corpus package after a converter change) stripping the block from that
// package's .csproj — the same silent un-shipping, reached by a different door, and one that reads
// as ordinary reconvert drift because only the .csproj moves.
func rewriteOfCorePackage(projectFileName string, options Options) bool {
	if len(options.go2csPath) == 0 {
		return false
	}

	coreRoot := filepath.Join(options.go2csPath, "core") + string(filepath.Separator)
	cleaned := filepath.Clean(projectFileName)

	return len(cleaned) > len(coreRoot) && strings.EqualFold(cleaned[:len(coreRoot)], coreRoot)
}

// allowUnsafeBlocks is the value the emitted .csproj's <AllowUnsafeBlocks> takes: the UNION of what
// the converter's own emission needs (usesUnsafeCode) and what any HAND-OWNED file in the package
// declares it needs ([module: go.GoRequiresUnsafe]).
//
// The union exists because usesUnsafeCode is an emission fact — it is set while visiting Go code, so
// it sees only C# the converter wrote. A hand-owned file is by definition code the converter did not
// write, and the .csproj is regenerated on every transpile, so before this a package whose only
// unsafe need was hand-written had no way to express it: setting the property by hand was undone by
// the next reconvert overlay, which is precisely what kept the FFI surface on `DllImport` (the
// [LibraryImport] source generator requires /unsafe unconditionally, SYSLIB1062).
//
// It is the same union, and inert for the same reason, as the cross-platform one in platformEmit.go:
// AllowUnsafeBlocks grants a capability rather than using one, so raising it changes no IL for code
// that contains nothing unsafe.
func allowUnsafeBlocks(packageOutputPath string) bool {
	return usesUnsafeCode || packageDeclaresRequiresUnsafe(packageOutputPath)
}

// packageDeclaresRequiresUnsafe reports whether any hand-owned C# file already in this package's
// OUTPUT directory declares [module: go.GoRequiresUnsafe].
//
// Reading the output tree is the same contract the hand-own marker itself runs on (conversionDriver
// probes the destination before converting over it), which means it inherits the same prerequisite:
// a reconvert must be SEEDED from the committed corpus, or there is nothing on disk to declare
// anything. That is already CLAUDE.md's non-negotiable reconvert ritual, so this adds no new rule.
//
// The walk is the package's own files plus its per-GOOS source folders — never recursive — because a
// converted package directory can hold NESTED PACKAGES (internal/runtime holds syscall, atomic, …)
// whose own .csproj answers for them. isPlatformSourceFolder is the same discriminator layout L3
// uses everywhere else, so `internal/syscall/windows` (a real package whose name is a GOOS) is not
// read as its parent's Windows sources.
//
// The declaration is per PACKAGE rather than per platform on purpose: a .csproj is ONE file serving
// every $(GoTargetOS), so a per-GOOS hand-own's requirement has to reach the shared property anyway.
func packageDeclaresRequiresUnsafe(packageOutputPath string) bool {
	packageDir := packageOutputPath

	if info, err := os.Stat(packageDir); err != nil || !info.IsDir() {
		// A single-FILE conversion hands its output file path here, not a directory.
		packageDir = filepath.Dir(packageDir)
	}

	if declaresRequiresUnsafeInDir(packageDir) {
		return true
	}

	entries, err := os.ReadDir(packageDir)

	if err != nil {
		return false
	}

	for _, entry := range entries {
		if !entry.IsDir() || !isPlatformSourceFolder(packageDir, entry.Name()) {
			continue
		}

		if declaresRequiresUnsafeInDir(filepath.Join(packageDir, entry.Name())) {
			return true
		}
	}

	return false
}

// declaresRequiresUnsafeInDir scans the `.cs` files directly in one directory for the marker. The
// `.cs.auto` review siblings are excluded by the extension test — they are not compiled, so what
// they contain cannot need a compiler flag.
func declaresRequiresUnsafeInDir(dir string) bool {
	entries, err := os.ReadDir(dir)

	if err != nil {
		return false
	}

	for _, entry := range entries {
		if entry.IsDir() || !strings.EqualFold(filepath.Ext(entry.Name()), ".cs") {
			continue
		}

		if declared, err := containsRequiresUnsafeMarker(filepath.Join(dir, entry.Name())); err == nil && declared {
			return true
		}
	}

	return false
}

func writeProjectFile(projectFileName string, projectFileContents string, outputFilePath string, pkg *types.Package, options Options) error {
	// Get assembly output type from the package details
	outputType := getAssemblyOutputType(pkg)

	// Replace the output type marker with the actual output type
	newContents := []byte(strings.ReplaceAll(string(projectFileContents), OutputTypeMarker, outputType))

	// Replace the unsafe code marker with the actual unsafe code setting
	newContents = []byte(strings.ReplaceAll(string(newContents), UnsafeMarker, strconv.FormatBool(allowUnsafeBlocks(outputFilePath))))

	// Go's `go build` names an executable after the LAST element of the main package's import path
	// (`example.com/colordemo` → `colordemo`), not the full dotted module path. Mirror that for an
	// app's AssemblyName so the emitted exe matches `go build`'s name. LIBRARY assemblies keep the full
	// dotted name — their DLL/NuGet PackageId identity ($(AssemblyName)) must stay unique across the
	// package graph (e.g. github.com.fatih.color). Only the AssemblyName changes; the .csproj filename
	// (the project's identity in the solution/references) is left on the full path.
	//
	// The full name is read back out of the CONTENTS, not off the file name: since the path budget
	// landed (projectFileBaseName), a deep package's file name can be a compressed label while its
	// AssemblyName is still the full canonical path, and deriving it from the file name would then
	// match nothing and silently leave the app's AssemblyName on the full dotted path. Reading the
	// element the template just wrote is also what makes this correct for a user `-csproj` template.
	if outputType == "Exe" {
		fullName := declaredAssemblyName(string(newContents))

		if idx := strings.LastIndex(fullName, "."); idx >= 0 {
			lastSegment := fullName[idx+1:]
			newContents = []byte(strings.ReplaceAll(string(newContents),
				"<AssemblyName>"+fullName+"</AssemblyName>",
				"<AssemblyName>"+lastSegment+"</AssemblyName>"))
		}
	}

	// -recurse=nuget: reference the published go2cs NuGet packages (go.<pkg> stdlib, go.lib runtime,
	// go.gen analyzer) instead of local $(go2csPath) project references. Gated OFF the stdlib
	// self-conversion — its packages must reference each other locally to build the very assemblies that
	// get published — mirroring the README-emission gate below (options.convertStdLib).
	emitNuGet := options.nugetRefs && !options.convertStdLib

	// The golib runtime and the go2cs-gen analyzer are FIXED ProjectReferences hardcoded in
	// csproj-template.xml (NOT part of the ProjectReferenceMarker block), so swap them here. Match strings
	// must stay in sync with csproj-template.xml (~line 78 analyzer, ~line 118 golib); TestRecurseNuGetReferences
	// guards against drift. The analyzer keeps PrivateAssets="all" (go.gen is a DevelopmentDependency
	// analyzer package, delivered under analyzers/dotnet/cs — analyzer-only, no compile/runtime asset).
	if emitNuGet {
		newContents = []byte(strings.ReplaceAll(string(newContents),
			`<ProjectReference Include="$(go2csPath)core/golib/golib.csproj" />`,
			`<PackageReference Include="go.lib" Version="$(GoStdLibVersion)" />`))
		newContents = []byte(strings.ReplaceAll(string(newContents),
			`<ProjectReference Include="$(go2csPath)gen/go2cs-gen/go2cs-gen.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" PrivateAssets="All" />`,
			`<PackageReference Include="go.gen" Version="$(GoStdLibVersion)" PrivateAssets="all" />`))
	}

	// The published NuGet package carries its own audit sheet: a validated package's versioned
	// validation proof page is packed as VALIDATION.md, so whoever extracts the .nupkg holds the
	// per-test differential offline. Only the stdlib self-conversion has a docs/validation tree to
	// point at, so every other conversion collapses the marker's line back to the blank line the
	// template has always had there and emits the .csproj it always did.
	newContents = []byte(strings.ReplaceAll(string(newContents), ValidationPackMarker, validationPackBlock(projectFileName, options)))

	// Extract project references from imports
	packageInfoMap := getImportPackageInfo(projectImports.Keys(), options)
	projectReferences := &strings.Builder{}

	// Ensure project references are sorted so that the project file output is deterministic
	references := make([]string, 0, len(packageInfoMap))

	// References to converted stdlib packages are emitted as `$(go2csPath)core\...`; a LOCAL/USER
	// module reference (getLocalModulePackageInfo) is an ABSOLUTE path, which is rewritten here
	// relative to THIS project's directory so the generated .csproj is portable. projectDir is made
	// absolute first — projectFileName can be relative (a relative output path), which would make
	// filepath.Rel fail against the absolute reference and leave a machine-specific path behind.
	projectDir := filepath.Dir(projectFileName)

	if absDir, absErr := filepath.Abs(projectDir); absErr == nil {
		projectDir = absDir
	}

	// Under -recurse=nuget, stdlib imports become go.<name> NuGet PackageReferences; the app's own
	// converted packages (main-module + third-party, IsStdLib=false) stay local ProjectReferences.
	var packageIds []string

	for _, info := range packageInfoMap {
		reference := info.ProjectReference

		if len(reference) == 0 {
			continue
		}

		// Load imported type aliases for the current package, if not already loaded — needed regardless of
		// whether this import is emitted as a ProjectReference or a NuGet PackageReference.
		loadImportedTypeAliases(info, options)

		if emitNuGet && info.IsStdLib {
			// PackageId is `go.` + the referenced project's AssemblyName. That AssemblyName is the .csproj
			// base name — the dotted import path, uniform across every resolver (e.g.
			// net\http\net.http.csproj → go.net.http). Derive it from the csproj base name, NOT
			// info.PackageName (a class-qualification name that differs for non-stdlib packages).
			packageIds = append(packageIds, "go."+strings.TrimSuffix(filepath.Base(reference), ".csproj"))
			continue
		}

		if filepath.IsAbs(reference) {
			if rel, relErr := filepath.Rel(projectDir, reference); relErr == nil {
				reference = rel
			}
		}

		// Track project references, in the corpus's one separator form. filepath.Rel returns an
		// OS-NATIVE relative path, so on Windows a sibling reference arrives here as `..\lib\x.csproj`;
		// every other producer already emits forward slashes (emittedProjectReference). ToSlash is the
		// single point that makes the emitted line host-independent — see F5.
		references = append(references, filepath.ToSlash(reference))
	}

	sort.Strings(references)
	sort.Strings(packageIds)

	// Build reference XML — NuGet PackageReferences first (stdlib/runtime/analyzer under -recurse=nuget),
	// then local ProjectReferences; both share the one ItemGroup ProjectReferenceMarker. When not in NuGet
	// mode packageIds is empty and this is byte-identical to the prior ProjectReference-only output.
	// Both values land in an XML attribute, so both are escaped — the same treatment the test
	// project writer has always given its identical reference loop (testConversion.go).
	//
	// A stdlib reference (`$(go2csPath)core\net\http\net.http.csproj`) and a NuGet package id are
	// built from Go import paths, whose character set excludes everything XML cares about, so for
	// them this is a no-op. A `-recurse` reference is not: it starts as an absolute path under the
	// user's output root and is only made relative when filepath.Rel succeeds, which it cannot do
	// across Windows volumes — so an output root like `D:\R&D\out` reaches here with its `&`
	// intact and emits a .csproj MSBuild refuses to parse.
	for _, packageID := range packageIds {
		projectReferences.WriteString(fmt.Sprintf("\r\n    <PackageReference Include=\"%s\" Version=\"$(GoStdLibVersion)\" />", escapeXMLAttributeValue(packageID)))
	}

	// The EMITTED spelling of each reference — escaped once, here, so the marker substitution below
	// and layout L3's reference adoption (which reads escaped values back out of the previous
	// project file) speak the same one spelling and cannot disagree about an escaped character.
	emittedReferences := make([]string, 0, len(references))

	for _, reference := range references {
		emittedReferences = append(emittedReferences, escapeXMLAttributeValue(reference))
	}

	for _, reference := range emittedReferences {
		projectReferences.WriteString(fmt.Sprintf("\r\n    <ProjectReference Include=\"%s\" />", reference))
	}

	// Replace the project reference marker with the actual project references
	newContents = []byte(strings.ReplaceAll(string(newContents), ProjectReferenceMarker, projectReferences.String()))

	// Layout L3 (docs/phase4/DESIGN-multiplatform-corpus.md §8): a package whose output directory
	// carries per-GOOS source folders gets the $(GoTargetOS) default and the conditioned
	// <Compile Include> that selects one of them. Asked of the OUTPUT TREE rather than of this
	// conversion, for the reason platformLayout.go's header gives: one target's emission cannot see
	// the platform axis, but it can honor a layout the tree already carries — which is also what
	// keeps a -tests rewrite of an L3 package's production .csproj from stripping the block.
	if packageCarriesPlatformLayout(outputFilePath) {
		newContents = []byte(applyPlatformLayoutBlocks(string(newContents), projectFileName))
	}

	// Layout L3's second half (platformProject.go): a package whose DIRECT IMPORT SET differs by
	// GOOS carries conditioned <ProjectReference> groups. Same adoption rule as the block above and
	// for the same reason — one target's conversion holds the truth for one platform only, so the
	// others' reference sets are recovered from the project file this write is about to replace.
	// Asked of the file rather than of the package directory: the reference axis and the source
	// axis are measured separately (design §4 vs §4.3) and need not coincide.
	newContents = []byte(applyPlatformReferenceAdoption(string(newContents), projectFileName,
		goosOfTarget(options.targetPlatform), emittedReferences))

	// Check if project file needs to be written
	if needToWriteFile(projectFileName, newContents) {
		// Write project file atomically
		err := os.WriteFile(projectFileName, newContents, 0644)

		if err != nil {
			return fmt.Errorf("failed to write project file: %s", err)
		}
	}

	// For executable projects, write OS-specific publish profiles
	if outputType == "Exe" {
		err := writePublishProfiles(outputFilePath)

		if err != nil {
			return fmt.Errorf("failed to write publish profiles for project \"%s\": %s", outputFilePath, err)
		}
	}

	// For library projects, write package files, like icon
	if outputType == "Library" {
		err := writePackageFiles(outputFilePath)

		if err != nil {
			return fmt.Errorf("failed to write package files for project \"%s\": %s", outputFilePath, err)
		}

		// Emit the per-package NuGet README from the package's Go doc.
		if emitsPackageReadme(projectFileName, options) {
			projectName := strings.TrimSuffix(filepath.Base(projectFileName), ".csproj")

			if err := writeReadmeFile(outputFilePath, projectName, packageDoc, packageSourceDir, options); err != nil {
				return fmt.Errorf("failed to write README file for project \"%s\": %s", outputFilePath, err)
			}

			// Capture what this README was composed from, so the -tests pipeline can re-emit it after
			// the compare writes the proof page its Tests badge reads (refreshPackageReadmeAfterProof).
			// Inside the gate on purpose: a package that gets no README here can get none there either.
			recordPackageReadmeEmission(outputFilePath, projectName, packageDoc, packageSourceDir)
		}
	}

	return nil
}

// emitsPackageReadme reports whether this conversion should write the package's NuGet README — the
// file that carries the four-badge line, including the Tests validation badge.
//
// The gate is the PACKAGE's provenance, NOT the run's mode. Every regeneration of a corpus package
// keeps its README current whatever the invocation, while a behavioral-test, example or -recurse
// conversion still writes none — the litter rule the gate was introduced for, preserved.
//
// It was `options.convertStdLib` alone until 2026-08-20, which made the Tests badge refreshable
// ONLY by a whole-stdlib reconvert. The badge is composed at CONVERSION time from
// docs/validation/current/<dot-id>.md and src/version.props, and the run that WRITES that proof
// page is a `-tests` compare — which then did not write the README. So every bank left its own
// package advertising a stale badge until some later, unrelated -stdlib run happened to level it,
// and re-leveling it by hand was a standing step every bank owed
// (BOARD-next-validation-candidates.md, "The stale-validation-badge class is a PIPELINE gap").
//
// The widening is exactly the one validationPackBlock's gate already took, for exactly the same
// reason and against the same predicate: rewriteOfCorePackage tests the OUTPUT LOCATION — under the
// runtime root's core\ tree — which is structural, so no fixture, example, -recurse output or
// end-user output path satisfies it whatever mode the converter was invoked in.
//
// This gate decides WHETHER a package gets a README, not WHEN its badge can be current, and the two
// are different questions. Within one `-test-action all` the README is composed during CONVERSION
// while the proof page it reads is written at the END of the COMPARE (emitValidationProofPage,
// testConversion.go), so a package whose counts CHANGE — every FRESH bank — emitted one run behind
// and read `not_yet_validated` beside its own green page. That ordering is closed by a SECOND
// emission point rather than by this gate: refreshPackageReadmeAfterProof re-emits the README once
// the page exists, from the record this write leaves behind (readme.go).
func emitsPackageReadme(projectFileName string, options Options) bool {
	return options.convertStdLib || rewriteOfCorePackage(projectFileName, options)
}

func writePackageFiles(projectPath string) error {
	// Make sure project path ends with a directory separator
	projectPath = strings.TrimRight(projectPath, string(filepath.Separator)) + string(filepath.Separator)

	pngFileName := projectPath + "go2cs.png"

	// Check if icon file needs to be written
	if needToWriteFile(pngFileName, pngFileBytes) {
		iconFile, err := os.Create(pngFileName)

		if err != nil {
			return fmt.Errorf("failed to create package icon file \"%s\": %s", pngFileName, err)
		}

		defer iconFile.Close()

		_, err = iconFile.Write(pngFileBytes)

		if err != nil {
			return fmt.Errorf("failed to write to package icon file \"%s\": %s", pngFileName, err)
		}
	}

	return nil
}

func writePublishProfiles(projectPath string) error {
	// Make sure "Properties/PublishProfiles" directory exists
	publishProfilesDir := filepath.Join(projectPath, "Properties", "PublishProfiles")

	if err := os.MkdirAll(publishProfilesDir, 0755); err != nil {
		return fmt.Errorf("failed to create directory \"%s\": %s", publishProfilesDir, err)
	}

	// Get list of publish profiles
	profiles, err := publishProfiles.ReadDir("profiles")

	if err != nil {
		return fmt.Errorf("failed to read publish profiles: %s", err)
	}

	// Write each publish profile file
	for _, profile := range profiles {
		profileBytes, err := publishProfiles.ReadFile(path.Join("profiles", profile.Name()))

		if err != nil {
			return fmt.Errorf("failed to read publish profile \"%s\": %s", profile.Name(), err)
		}

		profileFileName := filepath.Join(publishProfilesDir, profile.Name())

		// Check if profile file already exists - user may change default parameters, so we don't overwrite
		if _, err := os.Stat(profileFileName); err == nil {
			continue
		}

		profileFile, err := os.Create(profileFileName)

		if err != nil {
			return fmt.Errorf("failed to create publish profile \"%s\": %s", profileFileName, err)
		}

		defer profileFile.Close()

		_, err = profileFile.Write(profileBytes)

		if err != nil {
			return fmt.Errorf("failed to write to publish profile \"%s\": %s", profileFileName, err)
		}
	}

	return nil
}

func needToWriteFile(fileName string, fileBytes []byte) bool {
	existingFileBytes, err := os.ReadFile(fileName)

	if err != nil {
		return true
	}

	return !bytes.Equal(existingFileBytes, fileBytes)
}

func (v *Visitor) writeOutputFile(outputFileName string) error {
	// Resolve this file's position map before anything reaches disk: the sentinels the walk left
	// in the text are only readable as C# LINE numbers now, and they must not survive into the
	// emitted source (positionMapOperations).
	v.finalizePositionMap(outputFileName)

	outputFile, err := os.Create(outputFileName)

	if err != nil {
		return fmt.Errorf("failed to create output source file \"%s\": %s", outputFileName, err)
	}

	defer outputFile.Close()

	_, err = outputFile.WriteString(v.outputBuilder.String())

	if err != nil {
		return fmt.Errorf("failed to write to output source file \"%s\": %s", outputFileName, err)
	}

	return nil
}

func getAssemblyOutputType(pkg *types.Package) string {
	if hasMainFunction(pkg) {
		return "Exe"
	}

	return "Library"
}

func hasMainFunction(pkg *types.Package) bool {
	if pkg == nil {
		return false
	}

	// First check if this is a main package
	if pkg.Name() != "main" {
		return false
	}

	// Look through all objects in the package scope
	scope := pkg.Scope()
	mainObj := scope.Lookup("main")

	if mainObj == nil {
		return false
	}

	// Check if it's a function
	mainFunc, ok := mainObj.(*types.Func)

	if !ok {
		return false
	}

	// Get the function's type
	funcType, ok := mainFunc.Type().(*types.Signature)

	if !ok {
		return false
	}

	// main function should have no parameters and no return values
	return funcType.Params().Len() == 0 && funcType.Results().Len() == 0
}
