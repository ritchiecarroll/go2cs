package main

import (
	"context"
	"os"
	"path/filepath"
	"strings"
	"testing"
	"time"
)

// buildThenExecute builds a page conversion (its background build) and executes it, as the page's
// Run does, and reports the run stage and the conversion's project path.
func buildThenExecute(t *testing.T, s *server, id string) (stageResult, string) {
	t.Helper()
	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()
	build, err := s.pipeline.build(ctx, runRequest{ConversionID: id})
	if err != nil {
		t.Fatal(err)
	}
	if build.Status != "passed" {
		t.Fatalf("build = %+v, want passed", build)
	}
	run, err := s.pipeline.execute(ctx, runRequest{ConversionID: id})
	if err != nil {
		t.Fatal(err)
	}
	if run.Status != "passed" || !strings.Contains(run.Output, "hello from the stub") {
		t.Fatalf("execute = %+v, want the program's output", run)
	}
	s.pipeline.mu.Lock()
	project := s.pipeline.saved[id].project
	s.pipeline.mu.Unlock()
	return run, project
}

// Execute launches the program the build produced, `dotnet <assembly>`, at the path the build's own
// output named, rather than `dotnet run`, which evaluates the project again before every launch.
func TestExecuteLaunchesTheBuiltAssembly(t *testing.T) {
	stub := stubToolchain(t)
	s := newStubbedServer(t, pipelineOptions{noWarmup: true})

	id := convertForPage(t, s)
	_, project := buildThenExecute(t, s, id)

	want := "run-dll " + filepath.Join(filepath.Dir(project), "bin", "Debug", "net10.0", "tour.local.session.dll")
	if launches := stub.calls("run-dll "); len(launches) != 1 || launches[0] != want {
		t.Fatalf("direct launches = %q, want exactly %q", launches, want)
	}
	if launches := stub.calls("run "); len(launches) != 0 {
		t.Fatalf("`dotnet run` launches = %q, want none once the build named its assembly", launches)
	}

	// The combined /api/run path launches the same way.
	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()
	result, err := s.pipeline.run(ctx, runRequest{ConversionID: id})
	if err != nil {
		t.Fatal(err)
	}
	if !result.Successful || stub.count("run-dll ") != 2 || stub.count("run ") != 0 {
		t.Fatalf("run = %+v with %d direct and %d `dotnet run` launches, want a second direct launch",
			result, stub.count("run-dll "), stub.count("run "))
	}
}

// The build's own output names the program: the project's line, not a dependency's, and only an
// assembly with its runtimeconfig.json beside it.
func TestBuiltAssemblyReadsTheProjectsOutputLine(t *testing.T) {
	root := t.TempDir()
	app := filepath.Join(root, "cs", "src", "tour.local", "session")
	project := filepath.Join(app, tourProjectName)
	bin := filepath.Join(app, "bin", "Debug", "net10.0")
	assembly := filepath.Join(bin, "tour.local.session.dll")
	dependency := filepath.Join(root, "cs", "pkg", "example.com", "helper", "bin", "example.com.helper.dll")
	for _, file := range []string{assembly, filepath.Join(bin, "tour.local.session.runtimeconfig.json"), dependency} {
		if err := os.MkdirAll(filepath.Dir(file), 0o755); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(file, nil, 0o600); err != nil {
			t.Fatal(err)
		}
	}
	output := "  Determining projects to restore...\r\n" +
		"  All projects are up-to-date for restore.\r\n" +
		"  example.com.helper -> " + dependency + "\r\n" +
		"  tour.local.session -> " + assembly + "\r\n"

	if got := builtAssembly(output, project, ""); got != assembly {
		t.Fatalf("builtAssembly = %q, want %q", got, assembly)
	}
	// The displayed transcript spells the repository root as <repo>.
	displayed := strings.ReplaceAll(output, root, "<repo>")
	if got := builtAssembly(displayed, project, root); got != assembly {
		t.Fatalf("builtAssembly of the displayed transcript = %q, want %q", got, assembly)
	}
	if got := builtAssembly("  example.com.helper -> "+dependency+"\nBuild succeeded.\n", project, ""); got != "" {
		t.Fatalf("builtAssembly with only a dependency's line = %q, want none", got)
	}
	if err := os.Remove(filepath.Join(bin, "tour.local.session.runtimeconfig.json")); err != nil {
		t.Fatal(err)
	}
	if got := builtAssembly(output, project, ""); got != "" {
		t.Fatalf("builtAssembly without a runtimeconfig.json = %q, want none", got)
	}
}

// A build whose output names no assembly leaves execute on `dotnet run --no-build --project`, with
// the runtime's MSBuild properties, exactly as before the direct launch existed.
func TestExecuteFallsBackToDotnetRunWithoutAnAssemblyPath(t *testing.T) {
	stub := stubToolchain(t)
	stub.omitAssemblyPath(t)
	s := newStubbedServer(t, pipelineOptions{noWarmup: true})

	id := convertForPage(t, s)
	_, project := buildThenExecute(t, s, id)

	if launches := stub.calls("run-dll "); len(launches) != 0 {
		t.Fatalf("direct launches = %q, want none without an assembly path", launches)
	}
	launches := stub.calls("run ")
	if len(launches) != 1 || !strings.Contains(launches[0], "run --no-build --project "+project+" -p:go2csPath=") {
		t.Fatalf("`dotnet run` launches = %q, want one `run --no-build --project %s` with the runtime's properties", launches, project)
	}
}
