// t2b: the test imports database/sql and t2b/lib (not database/sql/driver) and calls (*sql.Tx).Commit, as
// modernc.org/sqlite/pcache's e2e_test.go does. Natively: ok.
package t2b

import (
	"database/sql"
	"testing"

	"t2b/lib"
)

func commitIfOpen(tx *sql.Tx) error {
	if tx == nil {
		return nil
	}
	return tx.Commit()
}

func TestCommitCompiles(t *testing.T) {
	if err := commitIfOpen(nil); err != nil {
		t.Fatal(err)
	}
	if lib.Version() != 1 || Answer() != 42 {
		t.Fatal("unexpected")
	}
}
