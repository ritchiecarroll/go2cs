// Package varhost is the library half of the VariadicPromotedForwarder guard: an interface with a VARIADIC method,
// declared here and embedded by the importer, as TypeScript's compiler.CompilerHost (Trace) is embedded by execute's
// watchCompilerHost and cachedCompilerHost.
package varhost

import "fmt"

type Host interface {
	Name() string
	Trace(format string, args ...any) string
}

type Base struct{ Tag string }

func (b *Base) Name() string { return "base:" + b.Tag }
func (b *Base) Trace(format string, args ...any) string {
	return "trace[" + b.Tag + "] " + fmt.Sprintf(format, args...)
}

// Use consumes the interface on this side of the boundary.
func Use(h Host) string {
	return h.Name() + " | " + h.Trace("%s=%d", "n", 2) + " | " + h.Trace("plain")
}
