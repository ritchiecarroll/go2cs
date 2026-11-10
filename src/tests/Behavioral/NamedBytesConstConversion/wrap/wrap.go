// Package wrap re-exports lib's types by alias, as tsc's json package re-exports jsontext.Value.
package wrap

import "NamedBytesConstConversion/lib"

type Value = lib.Value

type Name = lib.Name

type Handler = lib.Handler
