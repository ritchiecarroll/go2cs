// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
global using driverꓸValue = object;
global using sqlite3ꓸSqlite3_index_info = go.modernc.org.sqlite.lib.sqlite3_package.Tsqlite3_index_infoᴛ1;
global using sqlite3ꓸSqlite3_int64 = long;
global using sqlite3ꓸSqlite3_module = go.modernc.org.sqlite.lib.sqlite3_package.Tsqlite3_moduleᴛ1;
global using sqlite3ꓸSqlite3_vtab = go.modernc.org.sqlite.lib.sqlite3_package.Tsqlite3_vtabᴛ1;
global using sqlite3ꓸSqlite3_vtab_cursor = go.modernc.org.sqlite.lib.sqlite3_package.Tsqlite3_vtab_cursorᴛ1;
global using sqlite3ꓸTi64 = long;
global using sqlite3ꓸTsqlite3 = go.modernc.org.sqlite.lib.sqlite3_package.Tsqlite3ᴛ1;
global using sqlite3ꓸTsqlite3_int64 = long;
global using sqlite3ꓸTsqlite3_uint64 = ulong;
global using sqlite3ꓸTsqlite_int64 = long;
global using typesꓸSize_t = ulong;
// </ImportedTypeAliases>

using go;
using static go.modernc.org.sqlite_package;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b426567696e2829206572726f727d", "vtabBeginTrampoline_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b436f6d6d69742829206572726f727d", "vtabCommitTrampoline_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b44656c657465286f6c64526f77696420696e74363429206572726f723b20496e7365727428636f6c73205b5d6d6f6465726e632e6f72672f73716c6974652f767461622e56616c75652c20726f776964202a696e74363429206572726f723b20557064617465286f6c64526f77696420696e7436342c20636f6c73205b5d6d6f6465726e632e6f72672f73716c6974652f767461622e56616c75652c206e6577526f776964202a696e74363429206572726f727d", "vtabUpdateTrampoline_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b52656c6561736528696e7429206572726f727d", "vtabReleaseTrampoline_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b52656e616d6528737472696e6729206572726f727d", "vtabRenameTrampoline_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b526f6c6c6261636b2829206572726f727d", "vtabRollbackTrampoline_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b526f6c6c6261636b546f28696e7429206572726f727d", "vtabRollbackToTrampoline_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b53617665706f696e7428696e7429206572726f727d", "vtabSavepointTrampoline_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b53796e632829206572726f727d", "vtabSyncTrampoline_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b556e7772617028292064617461626173652f73716c2f6472697665722e436f6e6e7d", "unwrapConn_type")]
[assembly: GoDynamicTypeLift("7374727563747b6d752073796e632e52574d757465783b206d206d61705b737472696e675d75696e747074727d", "nativeModulesᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d752073796e632e52574d757465783b206d206d61705b75696e747074725d2a6d6f6465726e632e6f72672f73716c6974652e676f437572736f727d", "vtabCursorsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d752073796e632e52574d757465783b206d206d61705b75696e747074725d2a6d6f6465726e632e6f72672f73716c6974652e676f4d6f64756c653b20696473206d6f6465726e632e6f72672f73716c6974652e696447656e3b206e616d65326964206d61705b6d6f6465726e632e6f72672f73716c6974652e6d6f64756c654b65795d75696e747074727d", "vtabModulesᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d752073796e632e52574d757465783b206d206d61705b75696e747074725d2a6d6f6465726e632e6f72672f73716c6974652e676f5461626c657d", "vtabTablesᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d752073796e632e52574d757465783b206d206d61705b75696e747074725d2a6d6f6465726e632e6f72672f73716c6974652e70636163686542696e64696e673b2067656e206d6f6465726e632e6f72672f73716c6974652e706361636865494447656e7d", "pcacheBindingsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d752073796e632e52574d757465783b206d206d61705b75696e747074725d66756e63286d6f6465726e632e6f72672f73716c6974652e53514c69746550726555706461746544617461297d", "xPreUpdateHandlersᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d752073796e632e52574d757465783b206d206d61705b75696e747074725d66756e6328737472696e672c20737472696e672920696e743b20696473206d6f6465726e632e6f72672f73716c6974652e696447656e7d", "xCollationsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d752073796e632e52574d757465783b206d206d61705b75696e747074725d6d6f6465726e632e6f72672f73716c6974652e417574686f72697a6572466e7d", "xAuthorizersᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d752073796e632e52574d757465783b206d206d61705b75696e747074725d6d6f6465726e632e6f72672f73716c6974652e436f6d6d6974486f6f6b466e7d", "xCommitHandlersᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d752073796e632e52574d757465783b206d206d61705b75696e747074725d6d6f6465726e632e6f72672f73716c6974652e526f6c6c6261636b486f6f6b466e7d", "xRollbackHandlersᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d752073796e632e52574d757465783b206d206d61705b75696e747074725d6d6f6465726e632e6f72672f73716c6974652e78416767726567617465436f6e74657874456e7472793b20696473206d6f6465726e632e6f72672f73716c6974652e696447656e7d", "xAggregateContextᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d752073796e632e52574d757465783b206d206d61705b75696e747074725d6d6f6465726e632e6f72672f73716c6974652e78416767726567617465466163746f7279456e7472793b20696473206d6f6465726e632e6f72672f73716c6974652e696447656e7d", "xAggregateFactoriesᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d752073796e632e52574d757465783b206d206d61705b75696e747074725d6d6f6465726e632e6f72672f73716c6974652e7846756e63456e7472793b20696473206d6f6465726e632e6f72672f73716c6974652e696447656e7d", "xFuncsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6f70656e476174652073796e632e52574d757465783b206f70656e65642073796e632f61746f6d69632e426f6f6c3b20636f6e6669674f6e63652073796e632e4f6e63653b20636f6e666967457272206572726f723b2072656769737465726564206d6f6465726e632e6f72672f73716c6974652e5061676543616368653b20634d6574686f64732075696e747074727d", "pcacheStateᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Driver, go.database.sql.driver_package.Driver>(Pointer = true)]
[assembly: GoImplement<Driver, go.modernc.org.sqlite.vtab_package.ModuleRegisterer>(Pointer = true)]
[assembly: GoImplement<Error, error>(Pointer = true)]
[assembly: GoImplement<conn, AuthorizerRegisterer>(Pointer = true)]
[assembly: GoImplement<conn, DBStatus>(Pointer = true)]
[assembly: GoImplement<conn, ExecQuerierContext>(Pointer = true)]
[assembly: GoImplement<conn, FileControl>(Pointer = true)]
[assembly: GoImplement<conn, go.database.sql.driver_package.Conn>(Pointer = true)]
[assembly: GoImplement<conn, go.database.sql.driver_package.Execer>(Pointer = true)]
[assembly: GoImplement<conn, go.database.sql.driver_package.Queryer>(Pointer = true)]
[assembly: GoImplement<connector, go.database.sql.driver_package.Connector>(Pointer = true)]
[assembly: GoImplement<result, go.database.sql.driver_package.Result>(Pointer = true)]
[assembly: GoImplement<rows, go.database.sql.driver_package.Rows>(Pointer = true)]
[assembly: GoImplement<rows, go.database.sql.driver_package.RowsColumnTypeDatabaseTypeName>(Pointer = true)]
[assembly: GoImplement<rows, go.database.sql.driver_package.RowsColumnTypeLength>(Pointer = true)]
[assembly: GoImplement<rows, go.database.sql.driver_package.RowsColumnTypeNullable>(Pointer = true)]
[assembly: GoImplement<rows, go.database.sql.driver_package.RowsColumnTypePrecisionScale>(Pointer = true)]
[assembly: GoImplement<rows, go.database.sql.driver_package.RowsColumnTypeScanType>(Pointer = true)]
[assembly: GoImplement<stmt, go.database.sql.driver_package.Stmt>(Pointer = true)]
[assembly: GoImplement<tx, go.database.sql.driver_package.Tx>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Driver))]
[assembly: GoFieldAccess(typeof(conn))]
[assembly: GoFieldAccess(typeof(go.modernc.org.sqlite.vtab_package.IndexInfo))]
[assembly: GoFieldAccess(typeof(goCursor))]
[assembly: GoFieldAccess(typeof(goModule))]
[assembly: GoFieldAccess(typeof(goTable))]
[assembly: GoFieldAccess(typeof(mutex))]
[assembly: GoFieldAccess(typeof(pcacheBinding))]
[assembly: GoFieldAccess(typeof(result))]
[assembly: GoFieldAccess(typeof(rows))]
[assembly: GoFieldAccess(typeof(sqlite3ꓸTsqlite3))]
[assembly: GoFieldAccess(typeof(stmt))]
[assembly: GoFieldAccess(typeof(strconv_package.NumError))]
[assembly: GoFieldAccess(typeof(time_package.Timer))]
[assembly: GoFieldAccess(typeof(udfCall))]
[assembly: GoFieldAccess(typeof(userDefinedFunction))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/authorizer.go", "authorizer.cs", "AEOKAqq0goKCgoKClKaCgoKAgoKCgqTGgoKCpoKCgoKCpoKClJQ=", "177-182:1", "", "122=Invoke/1/5/2,Invoke/2/5/3,Invoke/3/5/4,Invoke/4/5/5")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/backup.go", "backup.cs", "ABY6goKCgoKUvIKCgoKUAAMaggACEIIAAhCClIKUgg==")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/busy.go", "busy.cs", "ABUwggADEqKCgqqCgoLOgoKCgpKClIKUgoK4goKCgpSCgoKmgrSCgqQ=")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/conn.go", "conn.cs", "ACySAaKqgoKCgoKCpoIAAxSigpSCgpSOgoCSvpSCtoKCgq6CgoKUggABEoKCroKCpoKCpgAHHKKCppSCgpSogoK6goKUgrgAAhCCgoKUgoKmpoKClNr8goiClKyygoKCpoKmgoKsgoKsooKCgqaCAAIagoKsooKsooKsgoKsgqyCggACMvKCgpSUgoKClISUgoK4goKUgoKCgpQABxDsgoKsgqyCgqyigqSCzKQABRaigoKCAAQQgoKCrIKCgoKCgqKCuIKCloKsgpSCpoKUAAsY4oKCqIKCpoKCgoKmgoKCpqaClILGgsaCgpSCxoLGgsakgsaCxoLGgsaC+oLWlIKmAA8WgoKCgpQABBKCgqaChoKCgriCgpSCgqaCpqiEgoKCpqaCgoKCuILKgoKMggACEIKmgrjMgoKmrIKCgqaCgqasgoKmrIKCpqyCgqasgoKsgoKsgoKmAAIY8qSCgqaCpoKmhqSCpILGAAkQ0pSUAAAQgoKU7IKCpgACFsK0goKUgpSCuIKCpoKmgoK4goiCgoKUlKbWgoKmpoKCuIKCAAMcwoKClIKCgpSC7oIABBiCgoKCgpSClIKChqaCpKTuwoKCpuaCAAMS0oKCppSUgoKCgoKCgqamgoKU7IKCpqyCgqaqgqaCgp6ClAAPGqKUggAKGAANHoKUpqIABxKClAACFsKCkqbm4oKCppKCuOjCgoKm5oK+woKk6oKCkKSClIKGlAAFEsKCpoKCkKSCgoKmpgAFFsKCkqbmtoKCpoKCggACFISWqoKCpq7igpSCgqaCgoKUgoKCpoKC6IKCgpSCgqaUgoKCpoKClKqCgoKUgoKUqoKCgpSCgpSmsoKCpoKCpoKClJSCgoKCgpSCpqyCrIKCgpSmgoKUgpSAgoK2qLKCgqaC2MKCkqbowoKCpujCgpKm6MKCkqYABhDCgoKmgoKmlKSkpKQ=", "115-124:1;194-199:1;398-402:1;502-505:1;543-552:1;844-847:1;919-929:1;1057-1059:1;1076-1078:1;1220-1222:1;1233-1237:1;1245-1247:1;1271-1271:1;1298-1298:1;1322-1324:1;1518-1520:1;1529-1531:1;1539-1541:1;1549-1551:1;1559-1561:1;1573-1575:1", "", "309=columnDeclType/1/1/2,Xsqlite3_column_database_name/1/1/3,GoString/1/3/3,Xsqlite3_column_table_name/1/1/4,GoString/2/3/4,Xsqlite3_column_origin_name/1/1/5,GoString/3/3/5;873=VaList/1/1/1;1026=cFuncPointer/1/1/7;1037=cFuncPointer/1/4/7,cFuncPointer/2/4/8,cFuncPointer/3/4/9,cFuncPointer/4/4/10;1060=cFuncPointer/1/1/6", "", "293:21,294:13,428:21,429:13,717:21,718:13,793:21,794:13,922:25,923:17,945:25,946:17,1081:25,1082:17,1105:21,1106:13,1124:25,1125:17,1159:21,1160:13,1188:21,1189:13,1214:25,1215:17,1406:25,1407:17,1424:25,1425:17,1441:25,1442:17,1459:25,1460:17,1477:25,1478:17,1494:25,1495:17")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/connector.go", "connector.cs", "AA6cAYyCpgAIIIKCpqy6goKm")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/convert.go", "convert.cs", "ABIqmoSUlIKUgqSClIKkgpSCxpSClIKkgpSCpIKUgqSClILGlIKkgqSClIKkgpSCxpSClIKkgpSCpIKUgsiUlIKagtaCgIKC1oKAgoLWgoKUpIKmgqaCgpSCpoKmgoKUxKSmgoIAAhCUgoKUgqSCgoKClIKkgoKCgpSCpIKCgoKUgqSUgqSCyKaCgpSmgoKUgoKmgpSklIKUpKSkpKSmspSkpKSkpIKk")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/dbstatus.go", "dbstatus.cs", "ACOgAbT2gpSCgoKUgqY=")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/driver.go", "driver.cs", "ABmQAe4AA7QD8oKCpoKCpoKCgqaCgoKmgoKCroKClAAGFNKClKKquIKCpuiIgr66goKm6oqCAAMQuoKCpuqKgr64goKm6oiC", "299-301:1;359-361:1;385-387:1;414-416:1;441-443:1", "", "", "", "46:25,47:17,104:25,105:17,126:25,127:17,148:25,149:17,170:25,171:17")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/error.go", "error.cs", "ABIklrg=")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/fcntl.go", "fcntl.cs", "ABM0tMKClIKC1sKClIKC1sKCgpSUgqY=")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/fdatasync.go", "fdatasync.cs", "AAoYAAI8goKClIKCpqqC")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/fdatasync_stub.go", "fdatasync_stub.cs", "AAgWgg==")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/mutex.go", "mutex.cs", "ABk0goKmgsaCpg==")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/nodmesg.go", "nodmesg.cs", "AAwY")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/norlimit.go", "norlimit.cs", "AAgU")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/ofd.go", "ofd.cs", "AAoaAAISAAJugoKClKSkpAAEEoI=")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/ofd_stub.go", "ofd_stub.cs", "AAgYgg==")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/pagecache.go", "pagecache.cs", "ABYwAAIQACj+AoKuooKCggAFTKKCpoKcgoKUgpSUggACEKKCgpSCuIKkgoKClJSCgoKClJSmgoKUqIKipN6CggADFoKCgoKCgoKCgoKCgoI=", "303-344:1;304-311:1.1", "", "137=VaList/1/1/2", "", "109:29,110:21")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/pagecache_trampolines.go", "pagecache_trampolines.cs", "ACGYAYIACBIABBaigoLaooKCgoLcooKCgqIACBSCgoKCgoKCpoKmgoIACCSCAAIQAAEkgoKIlIKClMqqgoKClKiCgoKUAAImooKClJSCkoKCgqaIgoKUgoKUlIqCgpSMgpSClIKCgpSCgoKCAAUUgoKClIKCgoKUgoKCgoKUgpSCgoKCrqKCgpSCgoKClIKCkoKCgpSCgoKiroKCgpSCgoKCgoKmroKCgpSCgoKUgoKCrIKCgpQ=")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/pre_update_hook.go", "pre_update_hook.cs", "ABUeAAgMAAgMAAgUhIKCgoKClIKClAAICoKCgoKCgpSCgoIACAqCgoKCgoKUgoKCzIKCgoKCgoKCggAOHIKogqbCgoKClJSCgoKUgoKm2rKClKqygpTqgoKClMaCgoKUlIKUtLSClIKCgpS0tLTGgoKClIKUAAgSpoKCgpSCpqaCgoKUgqY=", "", "", "251=GoString/1/2/4,GoString/2/2/5")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/result.go", "result.cs", "AA8YsoKCpoKmyqKCpqiigqY=")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/rows.go", "rows.cs", "ACJ2AAgCgoKClJSCgoK4goKmgoKCgoKUgqYACAyigoKUkoSClKa+ggACFrKCpoKCgriClJKmgoKCppSCgqaCAAEQqKSCtIIAAxKElsb4goKmtIKCpqSCgpQAAxiCgoKUgqbmgoKmtLS2pKTMwoKmkoKUAAIQggACHoKCgqaUpKSkpKQABBCCAAIUgqyCgoKmlKSkpNakgqS2pKSk7oKClA==", "67-72:1;101-111:1;270-273:1", "", "", "", "51:21,52:13")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/sqlite.go", "sqlite.cs", "ACM8goKEhIKCgoKCgoKCgoIABRKCgq4ADCa6goKagoKUgoaUmIKClJK6goKClPymgoKCgoK4goKUprKCgqaCgqamAAIggoKClIKClIKClISUuIKmAAIWgoKClIKClIKUgoKUrIKClIKUAAIigoKClIKClIKClAACErKClIKUqIKkpKiCgoKmrqKCggACKoKCgsqCgoK4goKCuIKCgriCgoIAARCCuIKCgriCgoK4gIKCgpSkgIIADQyktoCCgoKUtoCCgoKUtoCCgoKmtoCCgoKmAAMSgoK+goK4goKakpKClIKmlIKCgoK4goK4goK4goK8goK4poKCgoIAGIQCiKiIgtjagpSClIKokoKmgoKClNwAHGKIqIiCAAMSuoKCpuqKgpSCvIqClIIAAxi6goKmAAgG6IKUgpSCqJKCpqSCptyCgoKClIKUgoKClKaU/oKmgoKCgoKCgqamgqKCgpSCgqIACCAABBKCgoKUlIKqooKUgoIAAhyCgoKClKSCgoKClKbGtLS0goKCgpSmgoKUxtimopS0tLS0tIKCgpSCtIKCgpSCgpSClMSmAAsiAAkQAAkQAAkQABlKgoKCgoKCgqaCpoKCxoKCgoKCgpSEgoKCpoKCgpSCgoKUgoKmgoKCgqoAABamooKCgpSCgoKUgoKmgoK0qoKCgoKUgoKCpqKCgqaCgoKCgrSkooKCpoKCgoKCtKSigoKmgoKCgoKUgoLGpKKCgqaCgoKCgpSCgqaUgoKCoqSCgoKUgpiClKSkpgAEKrKCgoKmgpTugoKUpMQ=", "113-127:1;130-139:2;480-490:1;788-790:1;839-841:1;922-930:1;1381-1389:1", "", "", "", "575:25,576:17,609:25,610:17")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/stmt.go", "stmt.cs", "ABkuopKClJiCgoKCqoKEgqyCgoK4zKKCgpSUgoKUroLIooKClMbigoKCgIKkxsiGgpSElIbMgoK0goKUgoKCloKAyoKCqJTKgoKUgoKmgpTEpNyClIKUqKKCpoKksoKCpoKCgqaCkMiCgqaUyoKClIKCpoKUxKbWgpSGpoK2AAUYggACEIKm4oKCgoCCpMbIhoKClIKCtoSUhs6ChIKClIKCuoKEgoKCqJaSgoKUggACFpKCgpSCqIKCgsqigoKUpoK20oKApIKCpoKCuIKCppSClJKUgqSCkpSCyqaCgpSSlJTmgpSGpoKClLbYwoKSpujCgpKmAAgsgoKCpg==", "109-125:1;130-178:2;143-143:2.1;201-248:3;214-214:3.1;302-325:1;402-456:2;404-404:2.1;480-482:1;490-492:1", "", "", "", "120:21,121:13,149:41,150:33,230:41,231:33,325:21,326:13,417:33,418:25,509:25,510:17,527:25,528:17")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/strictpragma.go", "strictpragma.cs", "AA0eAAU4gqiCAAIihICCtoKCpoKotLS0tqqCgqSktLS0tMasgoKClIKClJSqgoKCpqqCgoKm")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/tx.go", "tx.cs", "ABYuopSCgqaCpgAEFLKCggACEoamAAIQggACFOKCgqaSgriCgqaCgoKCgsqC", "88-92:1;102-106:2", "", "", "", "75:21,76:13,89:25,90:17")]
[assembly: GoPositionMap("modernc.org/sqlite@v1.62.0/vtab.go", "vtab.cs", "ABsmggAJEAAJLgAIEAAIEgAEFgAWUoIABCjSgoK4gpSClIKUggAIKKKCgoKmgpSCgpSCpqbGgoKCgoKUgoKUgpSCgoKCmIKCgoKCgpSUgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKUqIKClJaClPbCgoKUgoKCgpSCgpSClNyCgoKClIKigoKUgoKU2JSCgoKUgoKCgpSCgpSUgoKCgqqCgoKClIKigoKUgoKU2JSCgoKUgoKCgpSCgpSUgoKCgqyCgoKCgqaCloKCgoKCgoKClLS0tLS0tLS0tLS0tLS0tLS0AAcQqIKCgoKCgoLKqIKUgqaEgqqCgoKCgoKChJSCuoKClIKGgoKCpoKUgpSClIKUqoKCgoKCgoKClIKogoKCgoKCgoKUgqqCgoKCgqaCgqaCgoKUgoKUloKUgoKCgqqCgoKCgoKCgpSCqKKCgoKCpoKClIKCgoKEgoKCppTYgoKCgoKClIKUqIKCgoKCgpSoooKCgoKClIKEgoKCgpSUlICGgoKCgqaCpNiCgoKCgoKClIKCgpSCpqKCgtaCgoKClKaCgpSCgpSqgoKUgoKClIKClK6CgoKCgoKUgpTogoKCgoKUgoKCgqYACAiigoKCgpSKgqiCgoKClIKClKyClISCgoKCloKUgpSEgoKUgoKUgpSogoKClICCgqSClAAJCIKCgoKClIKCgqbmgoKCgoKUgoKCpuaCgoKCgpSCgoKm5oKCgoKClIKCgqbmgoKCgoKUgoKCpuaCgoKCgpSCgoKm5oKCgoKClIKCgqY=", "143-145:1;307-317:1;317-319:2;319-321:3;355-365:1;365-367:2;367-369:3", "", "", "", "108:25,109:17")]
// </GoSourcePositionMaps>

namespace go.modernc.org;

[GoPackage("sqlite", ImportPath = "modernc.org/sqlite")]
public static partial class sqlite_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface unwrapConn_type {}
    internal partial interface vtabBeginTrampoline_type {}
    internal partial interface vtabCommitTrampoline_type {}
    internal partial interface vtabReleaseTrampoline_type {}
    internal partial interface vtabRenameTrampoline_type {}
    internal partial interface vtabRollbackToTrampoline_type {}
    internal partial interface vtabRollbackTrampoline_type {}
    internal partial interface vtabSavepointTrampoline_type {}
    internal partial interface vtabSyncTrampoline_type {}
    internal partial interface vtabUpdateTrampoline_type {}
    internal partial struct argFinder {}
    internal partial struct cBinding {}
    internal partial struct collation {}
    internal partial struct conn {}
    internal partial struct connector {}
    internal partial struct goCursor {}
    internal partial struct goModule {}
    internal partial struct goTable {}
    internal partial struct idGen {}
    internal partial struct moduleKey {}
    internal partial struct mutex {}
    internal partial struct nativeModulesᴛ1 {}
    internal partial struct pcacheBinding {}
    internal partial struct pcacheBindingsᴛ1 {}
    internal partial struct pcacheEntry {}
    internal partial struct pcacheIDGen {}
    internal partial struct pcacheStateᴛ1 {}
    internal partial struct result {}
    internal partial struct rows {}
    internal partial struct stmt {}
    internal partial struct tx {}
    internal partial struct udfCall {}
    internal partial struct userDefinedFunction {}
    internal partial struct vtabCursorsᴛ1 {}
    internal partial struct vtabModulesᴛ1 {}
    internal partial struct vtabTablesᴛ1 {}
    internal partial struct xAggregateContextEntry {}
    internal partial struct xAggregateContextᴛ1 {}
    internal partial struct xAggregateFactoriesᴛ1 {}
    internal partial struct xAggregateFactoryEntry {}
    internal partial struct xAuthorizersᴛ1 {}
    internal partial struct xCollationsᴛ1 {}
    internal partial struct xCommitHandlersᴛ1 {}
    internal partial struct xFuncEntry {}
    internal partial struct xFuncsᴛ1 {}
    internal partial struct xPreUpdateHandlersᴛ1 {}
    internal partial struct xRollbackHandlersᴛ1 {}
    public partial interface AggregateFunction {}
    public partial interface AuthorizerRegisterer {}
    public partial interface Cache {}
    public partial interface DBStatus {}
    public partial interface ExecQuerierContext {}
    public partial interface FileControl {}
    public partial interface HookRegisterer {}
    public partial interface Page {}
    public partial interface PageCache {}
    public partial struct AuthorizerActionCode {}
    public partial struct AuthorizerReturnCode {}
    public partial struct Backup {}
    public partial struct ColumnInfo {}
    public partial struct DBStatusOp {}
    public partial struct Driver {}
    public partial struct Error {}
    public partial struct FetchMode {}
    public partial struct FunctionContext {}
    public partial struct FunctionImpl {}
    public partial struct SQLitePreUpdateData {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸdatabaseꓸsql() => builtin.initPackage(typeof(database.sql_package));
    [GoInit] internal static void initᴛᴛimportꓸdatabaseꓸsqlꓸdriver() => builtin.initPackage(typeof(database.sql.driver_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸmodernc_orgꓸlibc() => builtin.initPackage(typeof(go.modernc.org.libc_package));
    [GoInit] internal static void initᴛᴛimportꓸmodernc_orgꓸlibcꓸsysꓸtypes() => builtin.initPackage(typeof(go.modernc.org.libc.sys.types_package));
    [GoInit] internal static void initᴛᴛimportꓸmodernc_orgꓸsqliteꓸlib() => builtin.initPackage(typeof(go.modernc.org.sqlite.lib.sqlite3_package));
    [GoInit] internal static void initᴛᴛimportꓸmodernc_orgꓸsqliteꓸvtab() => builtin.initPackage(typeof(go.modernc.org.sqlite.vtab_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(sqlite_package.Backup)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqliteᴛBackup;
[GoMethodHost(typeof(sqlite_package.Driver)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqliteᴛDriver;
[GoMethodHost(typeof(sqlite_package.Error)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqliteᴛError;
[GoMethodHost(typeof(sqlite_package.SQLitePreUpdateData)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqliteᴛSQLitePreUpdateData;
[GoMethodHost(typeof(sqlite_package.argFinder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqliteᴛargFinder;
[GoMethodHost(typeof(sqlite_package.conn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqliteᴛconn;
[GoMethodHost(typeof(sqlite_package.connector)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqliteᴛconnector;
[GoMethodHost(typeof(sqlite_package.idGen)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqliteᴛidGen;
[GoMethodHost(typeof(sqlite_package.mutex)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqliteᴛmutex;
[GoMethodHost(typeof(sqlite_package.pcacheIDGen)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqliteᴛpcacheIDGen;
[GoMethodHost(typeof(sqlite_package.result)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqliteᴛresult;
[GoMethodHost(typeof(sqlite_package.rows)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqliteᴛrows;
[GoMethodHost(typeof(sqlite_package.stmt)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqliteᴛstmt;
[GoMethodHost(typeof(sqlite_package.tx)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqliteᴛtx;
// </MethodHolders>
