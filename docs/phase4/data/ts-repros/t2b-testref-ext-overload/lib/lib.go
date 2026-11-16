// Package lib declares an exported method named Commit whose result names a database/sql/driver type, as
// modernc.org/sqlite's (*Backup).Commit does (backup.go). lib imports database/sql/driver; the test does not.
package lib

import "database/sql/driver"

type Backup struct{}

func (b *Backup) Commit() (driver.Conn, error) { return nil, nil }

func Version() int { return 1 }
