package main

// A converted package whose method signatures name types of packages called internal and checked (both C#
// keywords) declares an alias for each in its package_info.cs; the alias must be spelled @internal and @checked.

import (
	"fmt"

	"AliasKeywordPackage/checked"
	"AliasKeywordPackage/flags"
	"AliasKeywordPackage/internal"
	"AliasKeywordPackage/opts"
	"AliasKeywordPackage/record"
	"AliasKeywordPackage/tokens"
	"AliasKeywordPackage/union"
)

func main() {
	b := flags.Bools(5)
	fmt.Println("options:", b.Options(internal.NotForPublicUse{}))
	fmt.Println("check:", b.Check(checked.Token(3)))
	fmt.Println("has 0:", b.Has(flags.Flag(0)), "has 1:", b.Has(1), "has 2:", b.Has(internal.Flag(2)))
	fmt.Println("lock:", b.Lock(tokens.Kind(7)), "union:", b.Union(union.Set(2)), "record:", b.Record(record.Entry(4)))
	fmt.Println("pick 2:", opts.Mode(1).Pick(b, 2), "pick 1:", opts.Mode(1).Pick(b, 1))
}
