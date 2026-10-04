// GoHostModuleShadow guards the `go.go` namespace shadow. A module whose path's first host label is `go`
// (go.shadowlib.in/lib here; go.yaml.in/yaml/v3, go.uber.org/zap in the wild) converts to namespace
// go.go.shadowlib.in, so every compilation that references it has a `go.go` member in namespace `go`.
// C# binds the leading `go` of a using target inner-to-outer, so inside any `go.*` namespace a
// root-qualified alias such as `using utf8 = go.unicode.utf8_package;` binds to that `go.go` and fails
// (CS0234) -- what blocked testify, logrus and cobra/doc. A consumer package whose import closure holds
// such a module must root its aliases with `global::`; one without it emits exactly as before.
package main

import (
	"fmt"

	"GoHostModuleShadow/consumer"
)

func main() {
	fmt.Println(consumer.Describe("GoLang"))
}
