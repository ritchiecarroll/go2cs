// Package shadow imports strings under the alias st and declares its own identifier named strings, the name the
// converter would mint for an unaliased import of that package.
package shadow

import st "strings"

var strings = "shadowed"

// Get uses both: the aliased package and the package-level identifier that shares the package's name.
func Get() string { return st.ToUpper(strings) }
