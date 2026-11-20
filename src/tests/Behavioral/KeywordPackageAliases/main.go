// Each imported package is named with a C# contextual keyword that C# refuses as a type or
// alias name (CS9056 file, CS9029 required, CS9062 scoped, CS9306 extension), so the import
// alias the converter emits for it must be escaped.
package main

import (
	"fmt"

	"go/kwlib/extension"
	"go/kwlib/file"
	"go/kwlib/required"
	"go/kwlib/scoped"
)

func main() {
	fmt.Println(extension.Name(), file.Name(), required.Name(), scoped.Name())

	var e extension.Kind = 1
	var f file.Kind = 2
	var r required.Kind = 3
	var s scoped.Kind = 4
	fmt.Println(e.Double(), f.Double(), r.Double(), s.Double())
	fmt.Println(extension.Scaled(5), local())
}

// local names a variable like the imported package. C# accepts that name bare everywhere but at a type or alias name,
// so the local is not escaped.
func local() int {
	extension := 7
	return extension
}
