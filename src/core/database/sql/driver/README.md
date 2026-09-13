# go.database.sql.driver

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-1%2F1_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/database.sql.driver.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/database/sql/driver@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/database/sql/driver) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/database/sql/driver)

Package driver defines interfaces to be implemented by database drivers as used by package sql.

Most code should use the [database/sql](https://pkg.go.dev/database/sql@go1.23.12) package.

The driver interface has evolved over time. Drivers should implement \[Connector] and \[DriverContext] interfaces. The Connector.Connect and Driver.Open methods should never return \[ErrBadConn]. \[ErrBadConn] should only be returned from \[Validator], \[SessionResetter], or a query method if the connection is already in an invalid (e.g. closed) state.

All \[Conn] implementations should implement the following interfaces: \[Pinger], \[SessionResetter], and \[Validator].

If named parameters or context are supported, the driver's \[Conn] should implement: \[ExecerContext], \[QueryerContext], \[ConnPrepareContext], and \[ConnBeginTx].

To support custom data types, implement \[NamedValueChecker]. \[NamedValueChecker] also allows queries to accept per-query options as a parameter by returning \[ErrRemoveArgument] from CheckNamedValue.

If multiple result sets are supported, \[Rows] should implement \[RowsNextResultSet]. If the driver knows how to describe the types present in the returned result it should implement the following interfaces: \[RowsColumnTypeScanType], \[RowsColumnTypeDatabaseTypeName], \[RowsColumnTypeLength], \[RowsColumnTypeNullable], and \[RowsColumnTypePrecisionScale]. A given row value may also return a \[Rows] type, which may represent a database cursor value.

If a \[Conn] implements \[Validator], then the IsValid method is called before returning the connection to the connection pool. If an entry in the connection pool implements \[SessionResetter], then ResetSession is called before reusing the connection for another query. If a connection is never returned to the connection pool but is immediately reused, then ResetSession is called prior to reuse but IsValid is not called.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
