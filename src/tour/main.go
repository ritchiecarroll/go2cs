// Tour of go2cs hosts the official Tour of Go beside a live go2cs/.NET pipeline.
package main

import (
	"context"
	"embed"
	"encoding/json"
	"errors"
	"flag"
	"fmt"
	"io/fs"
	"log"
	"net"
	"net/http"
	"os"
	"os/signal"
	"path/filepath"
	"runtime"
	"strings"
	"syscall"
	"time"
)

//go:embed web/*
var webFiles embed.FS

// The header carries one optional outbound link, aimed by default at a community
// repository of worked Tour exercises. -solutions-url retargets it; an empty value
// removes it from the header entirely.
const (
	defaultSolutionsURL  = "https://github.com/martishin/a-tour-of-go"
	defaultSolutionsText = "Exercise solutions"
)

type server struct {
	repoRoot  *string
	pipeline  *pipelineRunner
	tour      *tourProxy
	static    http.Handler
	solutions solutionsLink
}

type solutionsLink struct {
	URL  string `json:"url,omitempty"`
	Text string `json:"text,omitempty"`
}

// newSolutionsLink reports the header link to show, or an empty link when the
// operator cleared it. Only http and https are honored: the value reaches the page
// as an href, and a link the header cannot open is worse than no link at all.
func newSolutionsLink(url, text string) solutionsLink {
	url = strings.TrimSpace(url)
	if url == "" {
		return solutionsLink{}
	}
	if !strings.HasPrefix(url, "http://") && !strings.HasPrefix(url, "https://") {
		log.Printf("ignoring -solutions-url %q: only http and https links are shown", url)
		return solutionsLink{}
	}
	if text = strings.TrimSpace(text); text == "" {
		text = defaultSolutionsText
	}
	return solutionsLink{URL: url, Text: text}
}

// serverOptions is the parsed command line.
type serverOptions struct {
	addr           string
	tourAddr       string
	repoRoot       string
	defaultRuntime string
	deployedRoot   string
	nugetSource    string
	nugetVersion   string
	toolTimeout    string
	solutionsURL   string
	solutionsText  string
	noTour         bool
	noOpen         bool
	noWarmup       bool
}

func parseServerOptions(args []string) (serverOptions, error) {
	var o serverOptions
	flags := flag.NewFlagSet(filepath.Base(os.Args[0]), flag.ExitOnError)
	flags.StringVar(&o.addr, "addr", "127.0.0.1:4000", "Tour of go2cs address")
	flags.StringVar(&o.tourAddr, "tour-addr", "127.0.0.1:3999", "official Tour of Go address")
	flags.StringVar(&o.repoRoot, "repo", "", "go2cs repository root (auto-detected by default)")
	flags.StringVar(&o.defaultRuntime, "runtime", "", ".NET runtime source: core, deployed, or nuget (default: nuget, then a detected deployed stdlib, then core)")
	flags.StringVar(&o.deployedRoot, "deployed-root", "", "deploy-core root for the deployed stdlib runtime")
	flags.StringVar(&o.nugetSource, "nuget-source", "", "NuGet feed or folder containing go2cs packages")
	flags.StringVar(&o.nugetVersion, "nuget-version", "", "version of go2cs NuGet packages")
	flags.StringVar(&o.toolTimeout, "tool-timeout", "", fmt.Sprintf("budget for the one-time go2cs build (duration such as 15m, or a whole number of seconds; default %s; %s)", defaultToolTimeout, toolTimeoutEnv))
	flags.StringVar(&o.solutionsURL, "solutions-url", defaultSolutionsURL, "header link to exercise solutions (empty hides the link)")
	flags.StringVar(&o.solutionsText, "solutions-text", defaultSolutionsText, "text for the exercise-solutions link")
	flags.BoolVar(&o.noTour, "no-tour", false, "do not start the official Tour process")
	flags.BoolVar(&o.noOpen, "no-open", false, "do not open a browser")
	flags.BoolVar(&o.noWarmup, "no-warmup", false, "do not warm up .NET at start (the first lesson then pays the cold start)")
	err := flags.Parse(args)
	return o, err
}

func main() {
	options, err := parseServerOptions(os.Args[1:])
	if err != nil {
		log.Fatal(err)
	}

	root, err := resolveRepoRoot(options.repoRoot)
	if err != nil {
		log.Fatal(err)
	}

	// A budget typed on the command line is checked here rather than defaulted quietly: the
	// operator asked for a specific one, and starting with a different budget than the one they
	// named is how a timeout stops meaning what its caller thinks it means.
	toolBudget, err := parseTimeoutSetting(options.toolTimeout)
	if err != nil {
		log.Fatalf("invalid -tool-timeout option: %v", err)
	}

	pipeline := newPipelineRunner(root, pipelineOptions{
		defaultRuntime: options.defaultRuntime,
		deployedRoot:   options.deployedRoot,
		nugetSource:    options.nugetSource,
		nugetVersion:   options.nugetVersion,
		toolTimeout:    toolBudget,
		noWarmup:       options.noWarmup,
	})
	if _, err := pipeline.resolveRuntime(""); err != nil {
		log.Fatalf("invalid -runtime option: %v", err)
	}
	defer pipeline.close()

	ctx, stop := signal.NotifyContext(context.Background(), os.Interrupt, syscall.SIGTERM)
	defer stop()

	// The warmup starts before the Tour and the listener, and runs beside them: nothing waits for
	// it except a .NET build that arrives while it is still running.
	pipeline.startWarmup(ctx)

	tour := newTourProxy(options.tourAddr, root)
	if !options.noTour {
		if err := tour.start(ctx); err != nil {
			log.Printf("official Tour unavailable: %v", err)
		}
	}
	defer tour.close()

	staticFS, err := fs.Sub(webFiles, "web")
	if err != nil {
		log.Fatal(err)
	}

	s := &server{
		repoRoot:  &root,
		pipeline:  pipeline,
		tour:      tour,
		static:    http.FileServer(http.FS(staticFS)),
		solutions: newSolutionsLink(options.solutionsURL, options.solutionsText),
	}

	httpServer := &http.Server{
		Addr:              options.addr,
		Handler:           s.routes(),
		ReadHeaderTimeout: 5 * time.Second,
	}

	listener, err := net.Listen("tcp", options.addr)
	if err != nil {
		log.Fatal(err)
	}

	url := "http://" + listener.Addr().String()
	log.Printf("Tour of go2cs is ready at %s", url)
	if !options.noOpen {
		go openBrowser(url)
	}

	errs := make(chan error, 1)
	go func() { errs <- httpServer.Serve(listener) }()

	select {
	case <-ctx.Done():
	case err := <-errs:
		if err != nil && !errors.Is(err, http.ErrServerClosed) {
			log.Printf("server stopped: %v", err)
		}
	}

	shutdownCtx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()
	_ = httpServer.Shutdown(shutdownCtx)
}

func (s *server) routes() http.Handler {
	mux := http.NewServeMux()
	mux.HandleFunc("POST /api/convert", s.handleConvert)
	mux.HandleFunc("POST /api/build", s.handleBuild)
	mux.HandleFunc("POST /api/execute", s.handleExecute)
	mux.HandleFunc("POST /api/run", s.handleRun)
	mux.HandleFunc("GET /api/health", s.handleHealth)
	mux.HandleFunc("GET /assets/go2cs.ico", s.handleImage("go2cs.ico"))
	mux.HandleFunc("GET /assets/go2cs-small.png", s.handleImage("go2cs-small.png"))
	mux.HandleFunc("GET /assets/GopherDotNetBotFrisbee.png", s.handleImage("GopherDotNetBotFrisbee.png"))
	mux.Handle("/tour/", s.tour)
	mux.Handle("/images/", s.tour)
	mux.Handle("/_/", s.tour)
	mux.Handle("/socket", s.tour)
	mux.Handle("/", s.static)
	return withSecurityHeaders(mux)
}

func (s *server) handleHealth(w http.ResponseWriter, _ *http.Request) {
	writeJSON(w, http.StatusOK, map[string]any{
		"ok":        true,
		"tour":      s.tour.available(),
		"repoRoot":  *s.repoRoot,
		"runtime":   s.pipeline.defaultRuntime,
		"runtimes":  s.pipeline.runtimeOptions(),
		"solutions": s.solutions,
		"warmup":    s.pipeline.warm.status(),
	})
}

func (s *server) handleConvert(w http.ResponseWriter, r *http.Request) {
	r.Body = http.MaxBytesReader(w, r.Body, maxRequestBytes)
	var request convertRequest
	if err := decodeJSON(r, &request); err != nil {
		writeJSON(w, http.StatusBadRequest, apiError{Error: "invalid request: " + err.Error()})
		return
	}

	result, err := s.pipeline.convert(r.Context(), request)
	if err != nil {
		s.writePipelineError(w, err)
		return
	}
	writeJSON(w, http.StatusOK, result)
}

func (s *server) handleRun(w http.ResponseWriter, r *http.Request) {
	r.Body = http.MaxBytesReader(w, r.Body, 16<<10)
	var request runRequest
	if err := decodeJSON(r, &request); err != nil {
		writeJSON(w, http.StatusBadRequest, apiError{Error: "invalid request: " + err.Error()})
		return
	}

	result, err := s.pipeline.run(r.Context(), request)
	if err != nil {
		s.writePipelineError(w, err)
		return
	}
	writeJSON(w, http.StatusOK, result)
}

func decodeJSON(r *http.Request, value any) error {
	decoder := json.NewDecoder(r.Body)
	decoder.DisallowUnknownFields()
	return decoder.Decode(value)
}

func (s *server) writePipelineError(w http.ResponseWriter, err error) {
	var validation validationError
	if errors.As(err, &validation) {
		writeJSON(w, http.StatusBadRequest, apiError{Error: validation.Error()})
		return
	}
	var notFound conversionNotFoundError
	if errors.As(err, &notFound) {
		writeJSON(w, http.StatusNotFound, apiError{Error: notFound.Error()})
		return
	}
	if errors.Is(err, context.Canceled) {
		return
	}
	writeJSON(w, http.StatusInternalServerError, apiError{Error: err.Error()})
}

func (s *server) handleImage(name string) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		path := filepath.Join(*s.repoRoot, "docs", "images", name)
		http.ServeFile(w, r, path)
	}
}

func writeJSON(w http.ResponseWriter, status int, value any) {
	w.Header().Set("Content-Type", "application/json; charset=utf-8")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(value)
}

func withSecurityHeaders(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("X-Content-Type-Options", "nosniff")
		w.Header().Set("Referrer-Policy", "no-referrer")
		w.Header().Set("Permissions-Policy", "camera=(), microphone=(), geolocation=()")
		next.ServeHTTP(w, r)
	})
}

func resolveRepoRoot(explicit string) (string, error) {
	if explicit != "" {
		return validateRepoRoot(explicit)
	}

	candidates := []string{}
	if cwd, err := os.Getwd(); err == nil {
		candidates = append(candidates, cwd)
	}
	if executable, err := os.Executable(); err == nil {
		candidates = append(candidates, filepath.Dir(executable))
	}

	for _, candidate := range candidates {
		for {
			if root, err := validateRepoRoot(candidate); err == nil {
				return root, nil
			}
			parent := filepath.Dir(candidate)
			if parent == candidate {
				break
			}
			candidate = parent
		}
	}

	return "", fmt.Errorf("could not locate the go2cs repository; start from src/tour or pass -repo")
}

func validateRepoRoot(root string) (string, error) {
	absolute, err := filepath.Abs(root)
	if err != nil {
		return "", err
	}
	for _, required := range []string{
		filepath.Join("src", "go2cs", "go.mod"),
		filepath.Join("docs", "images", "go2cs.ico"),
	} {
		if _, err := os.Stat(filepath.Join(absolute, required)); err != nil {
			return "", fmt.Errorf("%s is not a go2cs repository root", absolute)
		}
	}
	return absolute, nil
}

func openBrowser(url string) {
	var name string
	var args []string
	switch runtime.GOOS {
	case "windows":
		name, args = "rundll32", []string{"url.dll,FileProtocolHandler", url}
	case "darwin":
		name, args = "open", []string{url}
	default:
		name, args = "xdg-open", []string{url}
	}
	_ = startDetached(name, args...)
}

func startDetached(name string, args ...string) error {
	command := newCommand(context.Background(), name, args...)
	command.Stdout = nil
	command.Stderr = nil
	return command.Start()
}

type apiError struct {
	Error string `json:"error"`
}

type validationError string

func (e validationError) Error() string { return string(e) }

func cleanDisplayPath(value, root string) string {
	if value == "" {
		return value
	}
	clean := strings.ReplaceAll(value, root, "<repo>")
	return strings.ReplaceAll(clean, filepath.ToSlash(root), "<repo>")
}
