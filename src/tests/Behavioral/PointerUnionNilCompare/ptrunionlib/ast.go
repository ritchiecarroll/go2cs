// Package ptrunion is the library half of the PointerUnionNilCompare guard: the FOREIGN pointer types a checker-side
// union names, as TypeScript's checker unions *ast.Node and *ast.Symbol with its own *Type.
package ptrunion

type Node struct{ Kind int }
type Symbol struct{ Name string }
