// Package user sits in namespace go.PackageNameNotDirName, beside the child namespace go.PackageNameNotDirName.lib
// that package sqlite3 keeps for its directory, so its import alias `lib` is renamed.
package user

import lib "PackageNameNotDirName/lib"

func Rows(n int64) int64 { return lib.Twice(n) + 1 }
