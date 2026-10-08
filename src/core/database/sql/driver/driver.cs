// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package driver defines interfaces to be implemented by database
// drivers as used by package sql.
//
// Most code should use the [database/sql] package.
//
// The driver interface has evolved over time. Drivers should implement
// [Connector] and [DriverContext] interfaces.
// The Connector.Connect and Driver.Open methods should never return [ErrBadConn].
// [ErrBadConn] should only be returned from [Validator], [SessionResetter], or
// a query method if the connection is already in an invalid (e.g. closed) state.
//
// All [Conn] implementations should implement the following interfaces:
// [Pinger], [SessionResetter], and [Validator].
//
// If named parameters or context are supported, the driver's [Conn] should implement:
// [ExecerContext], [QueryerContext], [ConnPrepareContext], and [ConnBeginTx].
//
// To support custom data types, implement [NamedValueChecker]. [NamedValueChecker]
// also allows queries to accept per-query options as a parameter by returning
// [ErrRemoveArgument] from CheckNamedValue.
//
// If multiple result sets are supported, [Rows] should implement [RowsNextResultSet].
// If the driver knows how to describe the types present in the returned result
// it should implement the following interfaces: [RowsColumnTypeScanType],
// [RowsColumnTypeDatabaseTypeName], [RowsColumnTypeLength], [RowsColumnTypeNullable],
// and [RowsColumnTypePrecisionScale]. A given row value may also return a [Rows]
// type, which may represent a database cursor value.
//
// If a [Conn] implements [Validator], then the IsValid method is called
// before returning the connection to the connection pool. If an entry in the
// connection pool implements [SessionResetter], then ResetSession
// is called before reusing the connection for another query. If a connection is
// never returned to the connection pool but is immediately reused, then
// ResetSession is called prior to reuse but IsValid is not called.
global using Value = object;

namespace go.database.sql;

using context = context_package;
using errors = errors_package;
using reflect = reflect_package;

partial class driver_package {

// NamedValue holds both the value name and value.
partial struct NamedValue {
    // If the Name is not empty it should be used for the parameter identifier and
    // not the ordinal position.
    //
    // Name will not have a symbol prefix.
    public @string Name;
    // Ordinal position of the parameter starting from one and is always set.
    public nint Ordinal;
    // Value is the parameter value.
    public Value Value;
}

// Driver is the interface that must be implemented by a database
// driver.
//
// Database drivers may implement [DriverContext] for access
// to contexts and to parse the name only once for a pool of connections,
// instead of once per connection.
partial interface Driver {
    // Open returns a new connection to the database.
    // The name is a string in a driver-specific format.
    //
    // Open may return a cached connection (one previously
    // closed), but doing so is unnecessary; the sql package
    // maintains a pool of idle connections for efficient re-use.
    //
    // The returned connection is only used by one goroutine at a
    // time.
    (Conn, error) Open(@string name);
}

// If a [Driver] implements DriverContext, then [database/sql.DB] will call
// OpenConnector to obtain a [Connector] and then invoke
// that [Connector]'s Connect method to obtain each needed connection,
// instead of invoking the [Driver]'s Open method for each connection.
// The two-step sequence allows drivers to parse the name just once
// and also provides access to per-[Conn] contexts.
partial interface DriverContext {
    // OpenConnector must parse the name in the same format that Driver.Open
    // parses the name parameter.
    (Connector, error) OpenConnector(@string name);
}

// A Connector represents a driver in a fixed configuration
// and can create any number of equivalent Conns for use
// by multiple goroutines.
//
// A Connector can be passed to [database/sql.OpenDB], to allow drivers
// to implement their own [database/sql.DB] constructors, or returned by
// [DriverContext]'s OpenConnector method, to allow drivers
// access to context and to avoid repeated parsing of driver
// configuration.
//
// If a Connector implements [io.Closer], the [database/sql.DB.Close]
// method will call the Close method and return error (if any).
partial interface Connector {
    // Connect returns a connection to the database.
    // Connect may return a cached connection (one previously
    // closed), but doing so is unnecessary; the sql package
    // maintains a pool of idle connections for efficient re-use.
    //
    // The provided context.Context is for dialing purposes only
    // (see net.DialContext) and should not be stored or used for
    // other purposes. A default timeout should still be used
    // when dialing as a connection pool may call Connect
    // asynchronously to any query.
    //
    // The returned connection is only used by one goroutine at a
    // time.
    (Conn, error) Connect(context.Context _);
    // Driver returns the underlying Driver of the Connector,
    // mainly to maintain compatibility with the Driver method
    // on sql.DB.
    Driver Driver();
}

// ErrSkip may be returned by some optional interfaces' methods to
// indicate at runtime that the fast path is unavailable and the sql
// package should continue as if the optional interface was not
// implemented. ErrSkip is only supported where explicitly
// documented.
public static error ErrSkip = errors.New("driver: skip fast-path; continue as if unimplemented"u8);

// ErrBadConn should be returned by a driver to signal to the [database/sql]
// package that a driver.[Conn] is in a bad state (such as the server
// having earlier closed the connection) and the [database/sql] package should
// retry on a new connection.
//
// To prevent duplicate operations, ErrBadConn should NOT be returned
// if there's a possibility that the database server might have
// performed the operation. Even if the server sends back an error,
// you shouldn't return ErrBadConn.
//
// Errors will be checked using [errors.Is]. An error may
// wrap ErrBadConn or implement the Is(error) bool method.
public static error ErrBadConn = errors.New("driver: bad connection"u8);

// Pinger is an optional interface that may be implemented by a [Conn].
//
// If a [Conn] does not implement Pinger, the [database/sql.DB.Ping] and
// [database/sql.DB.PingContext] will check if there is at least one [Conn] available.
//
// If Conn.Ping returns [ErrBadConn], [database/sql.DB.Ping] and [database/sql.DB.PingContext] will remove
// the [Conn] from pool.
partial interface Pinger {
    error Ping(context.Context ctx);
}

// Execer is an optional interface that may be implemented by a [Conn].
//
// If a [Conn] implements neither [ExecerContext] nor [Execer],
// the [database/sql.DB.Exec] will first prepare a query, execute the statement,
// and then close the statement.
//
// Exec may return [ErrSkip].
//
// Deprecated: Drivers should implement [ExecerContext] instead.
partial interface Execer {
    (Result, error) Exec(@string query, slice<Value> args);
}

// ExecerContext is an optional interface that may be implemented by a [Conn].
//
// If a [Conn] does not implement [ExecerContext], the [database/sql.DB.Exec]
// will fall back to [Execer]; if the Conn does not implement Execer either,
// [database/sql.DB.Exec] will first prepare a query, execute the statement, and then
// close the statement.
//
// ExecContext may return [ErrSkip].
//
// ExecContext must honor the context timeout and return when the context is canceled.
partial interface ExecerContext {
    (Result, error) ExecContext(context.Context ctx, @string query, slice<NamedValue> args);
}

// Queryer is an optional interface that may be implemented by a [Conn].
//
// If a [Conn] implements neither [QueryerContext] nor [Queryer],
// the [database/sql.DB.Query] will first prepare a query, execute the statement,
// and then close the statement.
//
// Query may return [ErrSkip].
//
// Deprecated: Drivers should implement [QueryerContext] instead.
partial interface Queryer {
    (Rows, error) Query(@string query, slice<Value> args);
}

// QueryerContext is an optional interface that may be implemented by a [Conn].
//
// If a [Conn] does not implement QueryerContext, the [database/sql.DB.Query]
// will fall back to [Queryer]; if the [Conn] does not implement [Queryer] either,
// [database/sql.DB.Query] will first prepare a query, execute the statement, and then
// close the statement.
//
// QueryContext may return [ErrSkip].
//
// QueryContext must honor the context timeout and return when the context is canceled.
partial interface QueryerContext {
    (Rows, error) QueryContext(context.Context ctx, @string query, slice<NamedValue> args);
}

// Conn is a connection to a database. It is not used concurrently
// by multiple goroutines.
//
// Conn is assumed to be stateful.
partial interface Conn {
    // Prepare returns a prepared statement, bound to this connection.
    (Stmt, error) Prepare(@string query);
    // Close invalidates and potentially stops any current
    // prepared statements and transactions, marking this
    // connection as no longer in use.
    //
    // Because the sql package maintains a free pool of
    // connections and only calls Close when there's a surplus of
    // idle connections, it shouldn't be necessary for drivers to
    // do their own connection caching.
    //
    // Drivers must ensure all network calls made by Close
    // do not block indefinitely (e.g. apply a timeout).
    error Close();
    // Begin starts and returns a new transaction.
    //
    // Deprecated: Drivers should implement ConnBeginTx instead (or additionally).
    (Tx, error) Begin();
}

// ConnPrepareContext enhances the [Conn] interface with context.
partial interface ConnPrepareContext {
    // PrepareContext returns a prepared statement, bound to this connection.
    // context is for the preparation of the statement,
    // it must not store the context within the statement itself.
    (Stmt, error) PrepareContext(context.Context ctx, @string query);
}

partial struct IsolationLevel /*num:nint*/;

// TxOptions holds the transaction options.
//
// This type should be considered identical to [database/sql.TxOptions].
partial struct TxOptions {
    public IsolationLevel Isolation;
    public bool ReadOnly;
}

// ConnBeginTx enhances the [Conn] interface with context and [TxOptions].
partial interface ConnBeginTx {
    // BeginTx starts and returns a new transaction.
    // If the context is canceled by the user the sql package will
    // call Tx.Rollback before discarding and closing the connection.
    //
    // This must check opts.Isolation to determine if there is a set
    // isolation level. If the driver does not support a non-default
    // level and one is set or if there is a non-default isolation level
    // that is not supported, an error must be returned.
    //
    // This must also check opts.ReadOnly to determine if the read-only
    // value is true to either set the read-only transaction property if supported
    // or return an error if it is not supported.
    (Tx, error) BeginTx(context.Context ctx, TxOptions opts);
}

// SessionResetter may be implemented by [Conn] to allow drivers to reset the
// session state associated with the connection and to signal a bad connection.
partial interface SessionResetter {
    // ResetSession is called prior to executing a query on the connection
    // if the connection has been used before. If the driver returns ErrBadConn
    // the connection is discarded.
    error ResetSession(context.Context ctx);
}

// Validator may be implemented by [Conn] to allow drivers to
// signal if a connection is valid or if it should be discarded.
//
// If implemented, drivers may return the underlying error from queries,
// even if the connection should be discarded by the connection pool.
partial interface Validator {
    // IsValid is called prior to placing the connection into the
    // connection pool. The connection will be discarded if false is returned.
    bool IsValid();
}

// Result is the result of a query execution.
partial interface Result {
    // LastInsertId returns the database's auto-generated ID
    // after, for example, an INSERT into a table with primary
    // key.
    (int64, error) LastInsertId();
    // RowsAffected returns the number of rows affected by the
    // query.
    (int64, error) RowsAffected();
}

// Stmt is a prepared statement. It is bound to a [Conn] and not
// used by multiple goroutines concurrently.
partial interface Stmt {
    // Close closes the statement.
    //
    // As of Go 1.1, a Stmt will not be closed if it's in use
    // by any queries.
    //
    // Drivers must ensure all network calls made by Close
    // do not block indefinitely (e.g. apply a timeout).
    error Close();
    // NumInput returns the number of placeholder parameters.
    //
    // If NumInput returns >= 0, the sql package will sanity check
    // argument counts from callers and return errors to the caller
    // before the statement's Exec or Query methods are called.
    //
    // NumInput may also return -1, if the driver doesn't know
    // its number of placeholders. In that case, the sql package
    // will not sanity check Exec or Query argument counts.
    nint NumInput();
    // Exec executes a query that doesn't return rows, such
    // as an INSERT or UPDATE.
    //
    // Deprecated: Drivers should implement StmtExecContext instead (or additionally).
    (Result, error) Exec(slice<Value> args);
    // Query executes a query that may return rows, such as a
    // SELECT.
    //
    // Deprecated: Drivers should implement StmtQueryContext instead (or additionally).
    (Rows, error) Query(slice<Value> args);
}

// StmtExecContext enhances the [Stmt] interface by providing Exec with context.
partial interface StmtExecContext {
    // ExecContext executes a query that doesn't return rows, such
    // as an INSERT or UPDATE.
    //
    // ExecContext must honor the context timeout and return when it is canceled.
    (Result, error) ExecContext(context.Context ctx, slice<NamedValue> args);
}

// StmtQueryContext enhances the [Stmt] interface by providing Query with context.
partial interface StmtQueryContext {
    // QueryContext executes a query that may return rows, such as a
    // SELECT.
    //
    // QueryContext must honor the context timeout and return when it is canceled.
    (Rows, error) QueryContext(context.Context ctx, slice<NamedValue> args);
}

// ErrRemoveArgument may be returned from [NamedValueChecker] to instruct the
// [database/sql] package to not pass the argument to the driver query interface.
// Return when accepting query specific options or structures that aren't
// SQL query arguments.
public static error ErrRemoveArgument = errors.New("driver: remove argument from query"u8);

// NamedValueChecker may be optionally implemented by [Conn] or [Stmt]. It provides
// the driver more control to handle Go and database types beyond the default
// [Value] types allowed.
//
// The [database/sql] package checks for value checkers in the following order,
// stopping at the first found match: Stmt.NamedValueChecker, Conn.NamedValueChecker,
// Stmt.ColumnConverter, [DefaultParameterConverter].
//
// If CheckNamedValue returns [ErrRemoveArgument], the [NamedValue] will not be included in
// the final query arguments. This may be used to pass special options to
// the query itself.
//
// If [ErrSkip] is returned the column converter error checking
// path is used for the argument. Drivers may wish to return [ErrSkip] after
// they have exhausted their own special cases.
partial interface NamedValueChecker {
    // CheckNamedValue is called before passing arguments to the driver
    // and is called in place of any ColumnConverter. CheckNamedValue must do type
    // validation and conversion as appropriate for the driver.
    error CheckNamedValue(ж<NamedValue> _);
}

// ColumnConverter may be optionally implemented by [Stmt] if the
// statement is aware of its own columns' types and can convert from
// any type to a driver [Value].
//
// Deprecated: Drivers should implement [NamedValueChecker].
partial interface ColumnConverter {
    // ColumnConverter returns a ValueConverter for the provided
    // column index. If the type of a specific column isn't known
    // or shouldn't be handled specially, [DefaultParameterConverter]
    // can be returned.
    ValueConverter ColumnConverter(nint idx);
}

// Rows is an iterator over an executed query's results.
partial interface Rows {
    // Columns returns the names of the columns. The number of
    // columns of the result is inferred from the length of the
    // slice. If a particular column name isn't known, an empty
    // string should be returned for that entry.
    slice<@string> Columns();
    // Close closes the rows iterator.
    error Close();
    // Next is called to populate the next row of data into
    // the provided slice. The provided slice will be the same
    // size as the Columns() are wide.
    //
    // Next should return io.EOF when there are no more rows.
    //
    // The dest should not be written to outside of Next. Care
    // should be taken when closing Rows not to modify
    // a buffer held in dest.
    error Next(slice<Value> dest);
}

// RowsNextResultSet extends the [Rows] interface by providing a way to signal
// the driver to advance to the next result set.
partial interface RowsNextResultSet :
    Rows
{
    // HasNextResultSet is called at the end of the current result set and
    // reports whether there is another result set after the current one.
    bool HasNextResultSet();
    // NextResultSet advances the driver to the next result set even
    // if there are remaining rows in the current result set.
    //
    // NextResultSet should return io.EOF when there are no more result sets.
    error NextResultSet();
}

// RowsColumnTypeScanType may be implemented by [Rows]. It should return
// the value type that can be used to scan types into. For example, the database
// column type "bigint" this should return "[reflect.TypeOf](int64(0))".
partial interface RowsColumnTypeScanType :
    Rows
{
    reflectꓸType ColumnTypeScanType(nint index);
}

// RowsColumnTypeDatabaseTypeName may be implemented by [Rows]. It should return the
// database system type name without the length. Type names should be uppercase.
// Examples of returned types: "VARCHAR", "NVARCHAR", "VARCHAR2", "CHAR", "TEXT",
// "DECIMAL", "SMALLINT", "INT", "BIGINT", "BOOL", "[]BIGINT", "JSONB", "XML",
// "TIMESTAMP".
partial interface RowsColumnTypeDatabaseTypeName :
    Rows
{
    @string ColumnTypeDatabaseTypeName(nint index);
}

// RowsColumnTypeLength may be implemented by [Rows]. It should return the length
// of the column type if the column is a variable length type. If the column is
// not a variable length type ok should return false.
// If length is not limited other than system limits, it should return [math.MaxInt64].
// The following are examples of returned values for various types:
//
//	TEXT          (math.MaxInt64, true)
//	varchar(10)   (10, true)
//	nvarchar(10)  (10, true)
//	decimal       (0, false)
//	int           (0, false)
//	bytea(30)     (30, true)
partial interface RowsColumnTypeLength :
    Rows
{
    (int64 length, bool ok) ColumnTypeLength(nint index);
}

// RowsColumnTypeNullable may be implemented by [Rows]. The nullable value should
// be true if it is known the column may be null, or false if the column is known
// to be not nullable.
// If the column nullability is unknown, ok should be false.
partial interface RowsColumnTypeNullable :
    Rows
{
    (bool nullable, bool ok) ColumnTypeNullable(nint index);
}

// RowsColumnTypePrecisionScale may be implemented by [Rows]. It should return
// the precision and scale for decimal types. If not applicable, ok should be false.
// The following are examples of returned values for various types:
//
//	decimal(38, 4)    (38, 4, true)
//	int               (0, 0, false)
//	decimal           (math.MaxInt64, math.MaxInt64, true)
partial interface RowsColumnTypePrecisionScale :
    Rows
{
    (int64 precision, int64 scale, bool ok) ColumnTypePrecisionScale(nint index);
}

// Tx is a transaction.
partial interface Tx {
    error Commit();
    error Rollback();
}

partial struct ΔRowsAffected /*num:int64*/;

internal static Result _ᴛ1ʗ = ((ΔRowsAffected)0);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lastInsertIdIsNotˢ = "LastInsertId is not supported by this driver"u8;

public static (int64, error) LastInsertId(this ΔRowsAffected _) {
    return (0, errors.New(lastInsertIdIsNotˢ));
}

public static (int64, error) RowsAffected(this ΔRowsAffected v) {
    return ((int64)v, default!);
}

// ResultNoRows is a pre-defined [Result] for drivers to return when a DDL
// command (such as a CREATE TABLE) succeeds. It returns an error for both
// LastInsertId and [RowsAffected].
public static noRows ResultNoRows;

public partial struct noRows {
}

internal static Result _ᴛ2ʗ = new noRows(nil);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noLastInsertIdAvailableˢ = "no LastInsertId available after DDL statement"u8;

public static (int64, error) LastInsertId(this noRows _) {
    return (0, errors.New(noLastInsertIdAvailableˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noRowsAffectedAvailableˢ = "no RowsAffected available after DDL statement"u8;

public static (int64, error) RowsAffected(this noRows _) {
    return (0, errors.New(noRowsAffectedAvailableˢ));
}

} // end driver_package
