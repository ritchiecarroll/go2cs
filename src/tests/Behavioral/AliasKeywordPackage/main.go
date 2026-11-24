package main

// A converted package whose method signatures name types of packages called internal and checked (both C#
// keywords) declares an alias for each in its package_info.cs; the alias must be spelled @internal and @checked.

import (
	"fmt"

	"AliasKeywordPackage/checked"
	"AliasKeywordPackage/flags"
	"AliasKeywordPackage/internal"
)

func main() {
	b := flags.Bools(5)
	fmt.Println("options:", b.Options(internal.NotForPublicUse{}))
	fmt.Println("check:", b.Check(checked.Token(3)))
	fmt.Println("has 0:", b.Has(flags.Flag(0)), "has 1:", b.Has(1), "has 2:", b.Has(internal.Flag(2)))
}
