// Package lib is a module whose path's first host label is `go` (go.shadowlib.in/lib), the shape of
// go.yaml.in/yaml/v3, go.uber.org/zap and go.opentelemetry.io: its C# namespace is go.go.shadowlib.in.
package lib

// Greeting is something for the consumer to call.
func Greeting(name string) string { return "hello, " + name }
