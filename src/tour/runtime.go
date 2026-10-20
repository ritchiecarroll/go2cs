package main

import (
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"strings"
	"time"
)

const (
	runtimeCore     = "core"
	runtimeDeployed = "deployed"
	runtimeNuGet    = "nuget"
)

type pipelineOptions struct {
	defaultRuntime string
	deployedRoot   string
	nugetSource    string
	nugetVersion   string
	toolTimeout    time.Duration
	noWarmup       bool
}

type runtimeConfiguration struct {
	mode          string
	label         string
	converterRoot string
	projectRoot   string
	nugetSource   string
	nugetVersion  string
}

type runtimeOption struct {
	Value     string `json:"value"`
	Label     string `json:"label"`
	Available bool   `json:"available"`
	Detail    string `json:"detail,omitempty"`
}

var (
	stdlibVersionPattern    = regexp.MustCompile(`<GoStdLibVersion>([^<]+)</GoStdLibVersion>`)
	buildNumberPattern      = regexp.MustCompile(`<GoBuildNumber>([^<]+)</GoBuildNumber>`)
	projectReferencePattern = regexp.MustCompile(`(?m)^([ \t]*)<ProjectReference Include="([^"]+)"[^>]*/>[^\r\n]*`)
)

func resolvePipelineOptions(repoRoot string, supplied pipelineOptions) pipelineOptions {
	supplied.defaultRuntime = strings.ToLower(strings.TrimSpace(supplied.defaultRuntime))
	supplied.toolTimeout = resolveToolTimeout(supplied.toolTimeout)

	if supplied.deployedRoot == "" {
		supplied.deployedRoot = strings.TrimSpace(os.Getenv("GO2CS_DEPLOYED_ROOT"))
	}
	if supplied.deployedRoot == "" {
		if output, err := exec.Command("go", "env", "GOPATH").Output(); err == nil {
			supplied.deployedRoot = filepath.Join(strings.TrimSpace(string(output)), "src", "go2cs")
		}
	}

	if supplied.nugetSource == "" {
		supplied.nugetSource = strings.TrimSpace(os.Getenv("GO2CS_NUGET_SOURCE"))
	}
	if supplied.nugetSource == "" {
		localFeed := filepath.Join(repoRoot, "src", "artifacts", "nupkg")
		if entries, err := os.ReadDir(localFeed); err == nil && len(entries) > 0 {
			supplied.nugetSource = localFeed
		} else {
			supplied.nugetSource = "https://api.nuget.org/v3/index.json"
		}
	}

	if supplied.nugetVersion == "" {
		supplied.nugetVersion = strings.TrimSpace(os.Getenv("GO2CS_NUGET_VERSION"))
	}
	if supplied.nugetVersion == "" {
		if content, err := os.ReadFile(filepath.Join(repoRoot, "src", "version.props")); err == nil {
			base := stdlibVersionPattern.FindSubmatch(content)
			build := buildNumberPattern.FindSubmatch(content)
			if len(base) == 2 && len(build) == 2 {
				supplied.nugetVersion = string(base[1]) + "." + string(build[1])
			}
		}
	}

	// Published packages come first so a fresh clone converts and builds lessons without staging
	// a standard library. The local trees remain the fallback when no package version is known.
	if supplied.defaultRuntime == "" {
		switch {
		case supplied.nugetSource != "" && supplied.nugetVersion != "":
			supplied.defaultRuntime = runtimeNuGet
		case validRuntimeRoot(supplied.deployedRoot):
			supplied.defaultRuntime = runtimeDeployed
		default:
			supplied.defaultRuntime = runtimeCore
		}
	}
	return supplied
}

func (p *pipelineRunner) resolveRuntime(value string) (runtimeConfiguration, error) {
	mode := strings.ToLower(strings.TrimSpace(value))
	if mode == "" {
		mode = p.defaultRuntime
	}
	if mode == "" {
		mode = runtimeCore
	}
	switch mode {
	case runtimeCore:
		return runtimeConfiguration{
			mode:          runtimeCore,
			label:         "Core source",
			converterRoot: filepath.Join(p.repoRoot, "src"),
			projectRoot:   filepath.Join(p.repoRoot, "src"),
		}, nil
	case runtimeDeployed:
		if !validRuntimeRoot(p.deployedRoot) {
			return runtimeConfiguration{}, validationError(
				"deployed runtime is unavailable; run src/deploy-core.ps1 or pass -deployed-root",
			)
		}
		return runtimeConfiguration{
			mode:          runtimeDeployed,
			label:         "Deployed stdlib",
			converterRoot: p.deployedRoot,
			projectRoot:   p.deployedRoot,
		}, nil
	case runtimeNuGet:
		if p.nugetSource == "" || p.nugetVersion == "" {
			return runtimeConfiguration{}, validationError(
				"NuGet runtime is unavailable; configure -nuget-source and -nuget-version",
			)
		}
		return runtimeConfiguration{
			mode:          runtimeNuGet,
			label:         "NuGet packages",
			converterRoot: filepath.Join(p.repoRoot, "src"),
			nugetSource:   p.nugetSource,
			nugetVersion:  p.nugetVersion,
		}, nil
	default:
		return runtimeConfiguration{}, validationError("runtime must be core, deployed, or nuget")
	}
}

func (p *pipelineRunner) runtimeOptions() []runtimeOption {
	deployed := validRuntimeRoot(p.deployedRoot)
	nuget := p.nugetSource != "" && p.nugetVersion != ""
	return []runtimeOption{
		{Value: runtimeCore, Label: "Core source", Available: true, Detail: filepath.Join(p.repoRoot, "src", "core")},
		{Value: runtimeDeployed, Label: "Deployed stdlib", Available: deployed, Detail: p.deployedRoot},
		{Value: runtimeNuGet, Label: "NuGet packages", Available: nuget, Detail: fmt.Sprintf("%s (%s)", p.nugetVersion, p.nugetSource)},
	}
}

func validRuntimeRoot(root string) bool {
	if root == "" {
		return false
	}
	for _, required := range []string{
		filepath.Join("core", "VERSION"),
		filepath.Join("core", "golib", "golib.csproj"),
		filepath.Join("gen", "go2cs-gen", "go2cs-gen.csproj"),
	} {
		if info, err := os.Stat(filepath.Join(root, required)); err != nil || info.IsDir() {
			return false
		}
	}
	return true
}

func prepareRuntimeProject(project, configRoot string, runtime runtimeConfiguration) error {
	if runtime.mode != runtimeNuGet {
		return nil
	}

	content, err := os.ReadFile(project)
	if err != nil {
		return err
	}
	converted := projectReferencePattern.ReplaceAllStringFunc(string(content), func(reference string) string {
		match := projectReferencePattern.FindStringSubmatch(reference)
		if len(match) != 3 {
			return reference
		}
		packageID, ok := packageIDForProjectReference(match[2])
		if !ok {
			return reference
		}
		private := ""
		if packageID == "go.gen" {
			private = ` PrivateAssets="all"`
		}
		return fmt.Sprintf(`%s<PackageReference Include="%s" Version="%s"%s />`,
			match[1], packageID, runtime.nugetVersion, private)
	})
	if converted == string(content) && !strings.Contains(converted, `<PackageReference Include="go.`) {
		return fmt.Errorf("generated project contained no go2cs project references to convert to NuGet packages")
	}
	if converted != string(content) {
		if err := os.WriteFile(project, []byte(converted), 0o600); err != nil {
			return err
		}
	}

	config := fmt.Sprintf(`<configuration>
  <packageSources>
    <clear />
    <add key="go2cs" value="%s" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
`, htmlEscape(runtime.nugetSource))
	return os.WriteFile(filepath.Join(configRoot, "NuGet.config"), []byte(config), 0o600)
}

func runtimeMSBuildArgs(runtime runtimeConfiguration) []string {
	var args []string

	if runtime.projectRoot != "" {
		args = append(args, "-p:go2csPath="+runtime.projectRoot+string(filepath.Separator))
	}

	if runtime.mode == runtimeNuGet && runtime.nugetVersion != "" {
		args = append(args, "-p:GoStdLibVersion="+runtime.nugetVersion)
	}

	return args
}

func packageIDForProjectReference(reference string) (string, bool) {
	normalized := strings.ToLower(strings.ReplaceAll(reference, `\`, "/"))
	switch {
	case strings.Contains(normalized, "gen/go2cs-gen/"):
		return "go.gen", true
	case strings.Contains(normalized, "core/golib/"):
		return "go.lib", true
	}

	// The converted standard library has one home — core/ — so one marker covers every
	// stdlib project reference (the historical go-src-converted/ spelling retired 2026-08-01).
	const marker = "core/"

	if index := strings.Index(normalized, marker); index >= 0 {
		relative := strings.TrimSuffix(normalized[index+len(marker):], ".csproj")
		directory := filepath.ToSlash(filepath.Dir(relative))
		if directory == "." || directory == "" {
			return "", false
		}
		return "go." + strings.ReplaceAll(directory, "/", "."), true
	}

	return "", false
}
