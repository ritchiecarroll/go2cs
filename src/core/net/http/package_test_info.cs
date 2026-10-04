// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.net.http_package;
global using static global::go.net.http_internal_test_package;

// <ImportedTypeAliases>
global using execꓸError = go.os.exec_package.ΔError;
global using httpꓸCookie = go.net.http_package.ΔCookie;
global using httpꓸHandler = go.net.http_package.ΔHandler;
global using httpꓸHeader = go.net.http_package.ΔHeader;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using netipꓸAddr = go.net.netip_package.ΔAddr;
global using netipꓸPrefix = go.net.netip_package.ΔPrefix;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using syscallꓸHandle = go.syscall_package.ΔHandle;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
global using syscallꓸSockaddr = go.syscall_package.ΔSockaddr;
global using textprotoꓸError = go.net.textproto_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using tlsꓸConnectionState = go.crypto.tls_package.ΔConnectionState;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
global using urlꓸError = go.net.url_package.ΔError;
using bufio = go.bufio_package;
using testing = go.testing_package;
using Δhttp = go.net.http_package;
// </ImportedTypeAliases>

using go;
using static global::go.net.http_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b436f6f6b6965202a6e65742f687474702e436f6f6b69653b2052617720737472696e677d", "writeSetCookiesTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b436f6f6b696573205b5d2a6e65742f687474702e436f6f6b69653b2052617720737472696e677d", "addCookieTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b61636365707420737472696e673b2065787065637441636365707420737472696e673b20636f6d7072657373656420626f6f6c7d", "roundTripTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6368756e6b656420626f6f6c3b20636f6d7072657373656420626f6f6c7d", "readResponseCloseInMiddleTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6465736320737472696e673b2064617461205b5d627974653b20636f6e74656e745479706520737472696e677d", "sniffTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b68206e65742f687474702e4865616465723b2065727220626f6f6c7d", "parseTimeTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b68206e65742f687474702e4865616465723b206578636c756465206d61705b737472696e675d626f6f6c3b20657870656374656420737472696e677d", "headerWriteTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b686561646572206e65742f687474702e4865616465723b20636f6f6b696573205b5d2a6e65742f687474702e436f6f6b69653b20676f646562756720737472696e677d", "readSetCookiesTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b686561646572206e65742f687474702e4865616465723b2066696c74657220737472696e673b20636f6f6b696573205b5d2a6e65742f687474702e436f6f6b69653b20676f646562756720737472696e677d", "readCookiesTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b68656164657220737472696e673b20757365726e616d6520737472696e673b2070617373776f726420737472696e673b206f6b20626f6f6c7d", "parseBasicAuthTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b2065727220737472696e673b20686561646572206e65742f687474702e4865616465727d", "readRequestErrorTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e677d", "newRequestHostTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696f2e5265616465723b20696f2e436c6f7365727d", "testTransportClosesBodyOnError_body")]
[assembly: GoDynamicTypeLift("7374727563747b696f2e5265616465727d", "testHeadResponses_src")]
[assembly: GoDynamicTypeLift("7374727563747b6d6574686f6420737472696e673b20686f737420737472696e673b207061746820737472696e673b20636f646520696e743b207061747465726e20737472696e677d", "serveMuxTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d6574686f6420737472696e673b20686f737420737472696e673b2075726c20737472696e673b20636f646520696e743b2072656469724f6b20626f6f6c7d", "serveMuxTests2ᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20726571205b5d627974657d", "badRequestTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6f726967696e616c20737472696e673b20726564697265637420737472696e677d", "fsRedirectTestDataᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7061747465726e20737472696e673b2068206e65742f687474702e48616e646c65727d", "serveMuxRegisterᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7061747465726e20737472696e673b206d736720737472696e677d", "handlersᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b70726f787920737472696e673b20736368656d6520737472696e673b206164647220737472696e673b206b657920737472696e677d", "cacheKeysTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7220737472696e673b20636f646520696e743b2072616e676573205b5d6e65742f687474705f746573742e77616e7452616e67657d", "ServeFileRangeTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b206c656e67746820696e7436343b2072205b5d6e65742f687474702e6874747052616e67657d", "ParseRangeTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b75726c20737472696e673b20657870656374656420737472696e677d", "vtestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b757365726e616d6520737472696e673b2070617373776f726420737472696e673b206f6b20626f6f6c7d", "getBasicAuthTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7665727320737472696e673b206d616a6f7220696e743b206d696e6f7220696e743b206f6b20626f6f6c7d", "parseHTTPVersionTestsᴛ1")]
[assembly: GoTypeAlias("Cookie", "ΔCookie")]
[assembly: GoTypeAlias("Handler", "ΔHandler")]
[assembly: GoTypeAlias("Header", "ΔHeader")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<RecordingJar, go.net.http_package.CookieJar>(Pointer = true)]
[assembly: GoImplement<TestH12_RequestContentLength_Unknown_type, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<TestH12_RequestContentLength_Unknown_type, io_package.Reader>]
[assembly: GoImplement<TestJar, go.net.http_package.CookieJar>(Pointer = true)]
[assembly: GoImplement<TestNewRequestContentLength_type, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<TestNewRequestContentLength_type, io_package.Reader>]
[assembly: GoImplement<apiHandler, go.net.http_package.ΔHandler>]
[assembly: GoImplement<blockingRemoteAddrConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<blockingRemoteAddrListener, net_package.Listener>(Pointer = true)]
[assembly: GoImplement<bodyCloser, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<bodyLimitReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<breakableConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<bufio_package.ReadWriter, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bufio_package.ReadWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<byteAtATimeReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<byteFromChanReader, io_package.Reader>]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.ReadSeeker>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<cancelProto, go.net.http_package.RoundTripper>]
[assembly: GoImplement<cancelableTimeoutContext, context_package.Context>(Promoted = true)]
[assembly: GoImplement<cancelableTimeoutContext, context_package.Context>]
[assembly: GoImplement<cleanupT, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<closeWriteTestConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<closerFunc, io_package.Closer>]
[assembly: GoImplement<countCloseListener, net_package.Listener>(Pointer = true)]
[assembly: GoImplement<countCloseReader, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<countCloseReader, io_package.Reader>]
[assembly: GoImplement<countedConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<countedConn, net_package.Conn>(Promoted = true)]
[assembly: GoImplement<countedContext, context_package.Context>(Pointer = true)]
[assembly: GoImplement<countedContext, context_package.Context>(Promoted = true)]
[assembly: GoImplement<delayedEOFReader, io_package.Reader>]
[assembly: GoImplement<doneContext, context_package.Context>(Promoted = true)]
[assembly: GoImplement<doneContext, context_package.Context>]
[assembly: GoImplement<dotFileHidingFile, go.net.http_package.File>(Promoted = true)]
[assembly: GoImplement<dotFileHidingFile, go.net.http_package.File>]
[assembly: GoImplement<dotFileHidingFileSystem, go.net.http_package.FileSystem>(Promoted = true)]
[assembly: GoImplement<dotFileHidingFileSystem, go.net.http_package.FileSystem>]
[assembly: GoImplement<dummyAddr, net_package.ΔAddr>]
[assembly: GoImplement<eofListenerNotComparable, net_package.Listener>]
[assembly: GoImplement<eofReaderFunc, io_package.Reader>]
[assembly: GoImplement<errorListener, net_package.Listener>(Pointer = true)]
[assembly: GoImplement<fakeFS, go.net.http_package.FileSystem>(Pointer = true)]
[assembly: GoImplement<fakeFS, go.net.http_package.FileSystem>]
[assembly: GoImplement<fakeFile, go.net.http_package.File>(Pointer = true)]
[assembly: GoImplement<fakeFile, io_package.ReadSeeker>(Promoted = true)]
[assembly: GoImplement<fakeFileInfo, go.io.fs_package.FileInfo>(Pointer = true)]
[assembly: GoImplement<fakeNetConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<fakeNetListener, net_package.Listener>(Pointer = true)]
[assembly: GoImplement<fileServerCleanPathDir, go.net.http_package.FileSystem>]
[assembly: GoImplement<fooProto, go.net.http_package.RoundTripper>]
[assembly: GoImplement<funcConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<funcConn, net_package.Conn>(Promoted = true)]
[assembly: GoImplement<funcConn, net_package.Conn>]
[assembly: GoImplement<funcRoundTripper, go.net.http_package.RoundTripper>]
[assembly: GoImplement<funcWriter, io_package.Writer>]
[assembly: GoImplement<global::go.net.http_package.Dir, global::go.net.http_package.FileSystem>]
[assembly: GoImplement<global::go.net.http_package.HandlerFunc, global::go.net.http_package.ΔHandler>]
[assembly: GoImplement<global::go.net.http_package.ResponseWriter, io_package.Writer>]
[assembly: GoImplement<global::go.net.http_package.http2StreamError, error>]
[assembly: GoImplement<global::go.net.http_package.http2noCachedConnError, error>]
[assembly: GoImplement<global::go.net.http_package.noBody, io_package.ReadCloser>]
[assembly: GoImplement<global::go.net.http_package.nothingWrittenError, error>]
[assembly: GoImplement<global::go.net.http_package.transportReadFromServerError, error>]
[assembly: GoImplement<go.compress.gzip_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<go.compress.gzip_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<go.crypto.tls_package.Conn, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<go.crypto.tls_package.Conn, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<go.net.http.cookiejar_package.Jar, go.net.http_package.CookieJar>(Pointer = true)]
[assembly: GoImplement<go.net.http.httptest_package.ResponseRecorder, go.net.http_package.ResponseWriter>(Pointer = true)]
[assembly: GoImplement<go.net.http_package.noBody, io_package.Reader>]
[assembly: GoImplement<go.net.http_test_package.delegateReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<go.net.http_test_package.dumpConn, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<go.net.http_test_package.dumpConn, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<go.net.http_test_package.dumpConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<go.net.http_test_package.roundTripFunc, go.net.http_package.RoundTripper>]
[assembly: GoImplement<go.testing_package.B, TBRun<go.testing_package.B>>(ConstraintProxy = true)]
[assembly: GoImplement<go.testing_package.T, TBRun<go.testing_package.T>>(ConstraintProxy = true)]
[assembly: GoImplement<gzipResponseWriter, go.net.http_package.ResponseWriter>(Promoted = true)]
[assembly: GoImplement<gzipResponseWriter, go.net.http_package.ResponseWriter>]
[assembly: GoImplement<http09Writer, go.net.http_package.ResponseWriter>(Pointer = true)]
[assembly: GoImplement<http09Writer, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<infiniteReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<io_package.ReadCloser, io_package.Reader>]
[assembly: GoImplement<io_package.WriteCloser, io_package.Writer>]
[assembly: GoImplement<issue12991FS, go.net.http_package.FileSystem>]
[assembly: GoImplement<issue12991File, go.net.http_package.File>(Promoted = true)]
[assembly: GoImplement<issue12991File, go.net.http_package.File>]
[assembly: GoImplement<issue15577Tripper, go.net.http_package.RoundTripper>]
[assembly: GoImplement<issue18239Body, io_package.Reader>]
[assembly: GoImplement<issue40382Body, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<lockedBytesBuffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<logWrites, io_package.Writer>]
[assembly: GoImplement<logWritesConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<net_package.Conn, io_package.Writer>]
[assembly: GoImplement<neverEnding, io_package.Reader>]
[assembly: GoImplement<nilBodyRoundTripper, go.net.http_package.RoundTripper>]
[assembly: GoImplement<noteCloseConn, net_package.Conn>(Promoted = true)]
[assembly: GoImplement<noteCloseConn, net_package.Conn>]
[assembly: GoImplement<oneConnListener, net_package.Listener>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.ReadSeeker>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<panicOnSeek, io_package.ReadSeeker>(Promoted = true)]
[assembly: GoImplement<panicOnSeek, io_package.ReadSeeker>]
[assembly: GoImplement<recordingTransport, go.net.http_package.RoundTripper>(Pointer = true)]
[assembly: GoImplement<repeatReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<responseWriterJustWriter, go.net.http_package.ResponseWriter>]
[assembly: GoImplement<responseWriterJustWriter, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<roundTripperWithCloseIdle, go.net.http_package.RoundTripper>]
[assembly: GoImplement<roundTripperWithoutCloseIdle, go.net.http_package.RoundTripper>]
[assembly: GoImplement<rwTestConn, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<rwTestConn, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<rwTestConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<slowTestConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<slurpResult, io_package.ReadCloser>(Promoted = true)]
[assembly: GoImplement<slurpResult, io_package.ReadCloser>]
[assembly: GoImplement<stringHandler, go.net.http_package.ΔHandler>]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.ReadSeeker>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<terrorWriter, io_package.Writer>]
[assembly: GoImplement<testCloseConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<testConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<testContentTypeWithVariousSources_readerOnly, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<testContentTypeWithVariousSources_readerOnly, io_package.Reader>]
[assembly: GoImplement<testErrorReader, io_package.Reader>]
[assembly: GoImplement<testFileSystem, go.net.http_package.FileSystem>(Pointer = true)]
[assembly: GoImplement<testHeadResponses_src, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<testHeadResponses_src, io_package.Reader>]
[assembly: GoImplement<testLogWriter, io_package.Writer>]
[assembly: GoImplement<testMockTCPConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<testRoundTripper, go.net.http_package.RoundTripper>]
[assembly: GoImplement<testServerExpect_type, io_package.Closer>(Promoted = true)]
[assembly: GoImplement<testServerExpect_type, io_package.WriteCloser>]
[assembly: GoImplement<testServerExpect_type, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<testTransportClosesBodyOnError_body, io_package.Closer>(Promoted = true)]
[assembly: GoImplement<testTransportClosesBodyOnError_body, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<testTransportClosesBodyOnError_body, io_package.Reader>]
[assembly: GoImplement<testing_package.B, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<tlogWriter, io_package.Writer>]
[assembly: GoImplement<trackLastConnListener, net_package.Listener>(Promoted = true)]
[assembly: GoImplement<trackLastConnListener, net_package.Listener>]
[assembly: GoImplement<transportDialTesterConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<transportDialTesterConn, net_package.Conn>(Promoted = true)]
[assembly: GoImplement<wgReadCloser, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<wgReadCloser, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<wrapWriter, go.net.http_package.ResponseWriter>(Promoted = true)]
[assembly: GoImplement<wrapWriter, go.net.http_package.ResponseWriter>]
[assembly: GoImplement<writeCountingConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<writerFuncConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<writerFuncConn, net_package.Conn>(Promoted = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<closeWriteTestConn, ж<closeWriteTestConn>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.net.http_package.Request, ж<global::go.net.http_package.Request>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.net.http_package.Response, ж<global::go.net.http_package.Response>>(Indirect = true)]
[assembly: GoImplicitConv<global::go.net.http_package.routingIndex, ж<global::go.net.http_package.routingIndex>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("net/http/alpn_test.go", "alpn_test.cs", "AB4oooKCgoKClIKUgqampoKmgoKCgpSCgpSAgtyCgoLckqaCgpKCgsyCgoKCgoKUgoKClICC1viigoKClIKCgpSCgoKCggAHEICi", "23-34:1")]
[assembly: global::go.GoPositionMap("net/http/async_test.go", "async_test.cs", "AA4YAAcYwqaygqKSgqiSgqqipKSC", "28-31:1")]
[assembly: global::go.GoPositionMap("net/http/client_test.go", "client_test.cs", "ACpEgoKqwoKCgoKClIKCgoKUlILKgNKChIKCgoKClIKigsiA4oKCgoKUgIIACxCigtbCgoKCgoKCgpSClIK0AAgEwoKChIKCgoSClIKUgpSClICCxAAMBMKCgoSCgoKCgoSClIKUgpSAkqSCpoKCgIKkgoKUgILEpIAADAKCgoKUgoCStoKClJaCgoCSuIKAkriCgoCSuIKCgJKmgoKCgoKClIKClIKCgJKkgpSAkriCgoKAgqSClIKWgoKAgqSClIKCupDSgoKWgoKSgrSktoKCgoKUggAJFIIACxgAFzCCouiCAAgUABUsgqIADQiCiIKCgoKCgIKkgoKCgIKCgIKCgoK2goKUuIKCgoKChIKUgqaCgoSChIKCuKKCgoKUgoKUgoK4gAAKAqKCgoKClIKCqIKCgpSUgoKUgpSSgoKUgrSooOKCgoKCgoKUgoKUgoKUgILukAAIAqKCgoKUgoKUgoKCgpSCgpSAgsjcgoKUgoKUgvjCgoKCgoKChIKEgoSChIKEgoKEgoKiAAkS4oKCgpSipOKCgtaAooKCgoKCgoKCgpSCpoKCgpSCgoKCgqaCyoAADwKigoKClIKCpoKCgpaCgpSCgpSCjIIAChaCpoKCpuKCgqKkgKKCgpKCgoKogoKClIKCgoKClIKUgoKmgoKCACQSgoKqoKKClpKCgoKUlIKEgoKUgpaCgoKUgriC1IKClIKCuoKypoKClIKogoIACAiigoKCgoKUgrSkgtSCgoKCqIKCgILIgtSCkoKChIKCgoKUgpaCggADGgAJAuSCgpaCgoKWgoKUpoLkooKWgoKCgpaCgpSSgpSAgsSqsvSCkoSCgILKkPKCgoCCtgAEEIKCgoKUgpSCgpSCyoDyooKCgoKCgoKUgoKCpqaSgoKUgoKCgpSy5KKCgoSCgoSClIKUgpSCgoKCgpSCgqa09KKCgpaCgoKUgoKEgpSClIKUgoKCgoKUgoKmtgAIBKKCABU0sqKCgu6AAAoCoqqCgoKClIKCgpSUhIKCggAIFIKCgqaUgIKkgoSCgoKCgoKClIKUgpSWgoKCgpaChIKUgoKklIKUgIKClKa6kPLCgpIACBKQlIKCgoKUgIKkgoKUgpSClICCgpTEqKCiooKEkoKUhIKCgoKClIKCgrSmkKKCgpaCgoKClICCpICCyIAACAKCgpKCgqiCgpSCgoKWgtamgtamgt6CgvaiABM4goKClIKClIKCAAkSgsrYssquwAARAqKYgoIABxCClIKUppKUhIKC7oKUloKCgoKCgoKUkoKUgILEqKL0ooKCgoCCooL8lKSkpKS2hIKYgoKCgoKClJKCtKaQAAsCtIKCgpSSgoKGkoKCgpaSgoKUgqaSgoKUgqSCgoKUgqSCtoKCgoSCgoKCgpSSgpSAgsSmkNLCgoKClJaSgoSClO6CgqTcgoKCpNyCgqQACBKCtoKogoKEgoKCgoKCgoKCgoKUkoK0AAgGkgAcRoKCgoKUgoKClIKC3IAACAKCAB9UhJKCloKyooKWgoKClqKAkqSmloKCgpYADBqCgqaCgqqg4qKCgpSCgpSCuoKClISCgoKEgoKUgoKCzIKCgpSC7IDIgKKA1KKChIKClIKCgtyC9oKmgoKClJSmqqDSooKWhIKygoKClIKUlIKCgpaEgoKCgpSAgqSAgqSAgsT8ggAKEKKEgoKWgpSSgIK4gIKkxKaQooKCuoKCgoKUgoKUAAwagpSkgqSClIIACAiigpSmgNKCgpKCgIK4goKCgsKCgoKCgpSilOi4koKCgoI=", "192-205:1;236-240:2;288-290:1;294-302:2;358-360:1;397-399:1;408-433:1;439-439:2;482-490:1;493-498:2;521-538:1;523-526:1.1;546-549:1;677-686:1;690-692:2;738-744:1;787-788:1;791-797:2;823-825:1;871-875:1;888-888:1;922-924:1;929-931:2;943-945:1;951-953:2;974-974:1;986-990:1;1021-1037:1;1156-1162:1;1173-1193:1;1270-1272:1;1280-1280:2;1316-1319:1;1339-1341:1;1360-1365:1;1465-1465:1;1483-1500:1;1501-1503:2;1507-1519:3;1546-1569:1;1573-1575:2;1595-1599:1;1609-1641:2;1666-1672:1;1674-1729:2;1856-1859:1;1863-1866:2;1874-1882:3;1878-1880:3.1;1917-1924:1;1979-1981:1;1998-2007:1;2016-2018:1;2023-2057:2;2083-2087:1;2099-2101:1;2154-2159:1;2165-2174:2")]
[assembly: global::go.GoPositionMap("net/http/clientserver_test.go", "clientserver_test.cs", "ADxoAAceAAsCgoKCgpS0xLaAgqSCgoKAgqSClAALGpKmgoK+0pKigpKiAA4cgoKmwoKClJKCgpTWgoKUtoKmgoK4AAEgABACgpTMhICCgoK4krqmgoKCloKUtMS4gpaUpKSCpIKCpKSCgoKAgraCloKCgpaSlO6CguiSgpKSppKCAA4IooqigoKCopKCgIKkgoKUgqSCpIKkgpSCuIDSooKCgoKCloKClJKAgqSCgpSClICCxAAOHIKClKbCgoKSgpSCgoKUgoKCloCCgqaCgoSCgoKmgpSAgoKCxKSikoKCgoIACBKA1KKClJSEgsqClIKUgrqSAAkOgqbKgqaCgtyCtoCigKKApIKC+IKCyIKCgsiCgoLIgoKCgsiCpoKUyoKmgpTKgpSCpoKCgpSClIIABRjgoqKCkoKCgoKCgoKmgqiCgpSUgoK0AAgIopSAgqSCgoLKgqS2goCCAAYSsKKigoKCgqaSgoKUgpSCgpSCtNSClIL6gqaCAAkGgtaClIK8gJIABhCg4qKCgqKCgoKClISCgoSCgpSSqIKCgpSEhIKCgpSCtKaQAAgCgoKCgpSClIKUgt6ClLiW2IKCgpSAgsqSkraCkgAQCIKCgoKEgoLegoKWgoKWpoLcgpSCloKCloDKpoCCpoDKypDSgoKClIKClIKCgriAAAwCwoKCgoKClLKCgoKUgpSipLKCypSikpSCgoKClIKSgpSCtKSA8oKCkpaCgpYAEi6CgoKClIKCgpSClILKgAAMAoKCloKClJYABxLY2NjIgoKCgoKClIKCgoKUgsqCgoKS2oKCkoKCloKClITMgoKUhIKCgoKUgpSClICCpNakgoKUlIKUgpSCupKkooKUlJSCtoKCgpSCgJKmgIKklISCgoKCgtKCgqaCgoKCppKCgpSipIKEgoKCgpSCqJKCgoKUlJSUouaSkoCSgPaCgoKCqIKigoKQkoKClICCpICCtoK0pNqA8oKClIQAExyCgpKClIKCgoKCgpSCpNiCpAALCoKSgJKAgJKAAAoGwoSChIKCsoKEAAgIlJSCgpSCkoKClIKUooKCxISSgoKClJSCgpSUgoKUlLT+0oKCAAkIkpSCgoKmgpSCgoKUgpSC3pDiooKUkoKClIKCgpSUgoKCggAIEoKCAAkKgoKmgKLCgqiEgoKUgoKCgpSSgrSkgAAJAoKCgoKCgpSCgpSAgqSCgoSAgqSAgsiC9IKigoKUgoKUkqKUkoKCgpSCuIDiooKygpKAgoKCgpS6tqKSgoKUgpSCvKKStoL0woKigoKSgoKClIKCgqKSlIKClJKCgpSAkrimlIKCgpSCtKSC5MKCgIK4goKUgoKSloKCgoKigoKSgoKUpoKClJKCgoKUgpS0pLSC6MYACAaSlIKmgoKCpoKUyoAACQKigoKCgJKkgoKClIKCgpSSgoKUgJLEpIAACQKigoKChIKCgoSEgoSWgoSCloKCuoKEgoCCyoK0lIKEtIK2toSmhIKClJSCgIKmgoK0", "85-94:1;90-92:1.1;120-126:1;121-125:1.1;169-171:1;209-213:1;210-212:1.1;271-273:2;289-293:1;290-292:1.1;294-298:2;295-297:2.1;306-311:1;341-345:1;475-476:1;483-485:1;492-497:1;502-502:1;510-512:1;516-518:1;522-525:1;529-532:1;536-540:1;546-551:1;558-563:1;569-572:1;573-585:2;598-612:1;630-638:1;645-645:1;647-652:2;661-667:1;687-690:1;695-695:1;699-699:1;703-703:1;708-711:1;712-714:2;715-719:3;729-735:1;773-790:1;794-796:2;798-800:3;818-820:1;823-825:1;830-846:1;900-902:1;918-947:1;923-933:1.1;935-945:1.2;967-969:1;1021-1023:1;1025-1030:2;1037-1037:3;1041-1041:4;1045-1045:5;1049-1049:6;1053-1053:7;1079-1083:1;1080-1082:1.1;1088-1092:1;1154-1156:1;1164-1173:2;1171-1171:2.1;1187-1206:3;1239-1242:1;1240-1240:1.1;1241-1241:1.2;1245-1250:1;1253-1267:2;1256-1256:2.1;1280-1282:1;1301-1304:2;1329-1333:1;1330-1330:1.1;1331-1331:1.2;1332-1332:1.3;1343-1352:1;1352-1354:2;1368-1372:3;1375-1396:4;1413-1418:1;1419-1434:2;1441-1443:1;1444-1455:2;1483-1485:1;1508-1514:1;1538-1549:1;1565-1583:1;1567-1581:1.1;1599-1601:1;1608-1621:1;1621-1623:2;1657-1661:1;1669-1671:2;1677-1687:3;1720-1723:1;1724-1729:2;1730-1735:3;1742-1751:1;1771-1785:1;1787-1799:2;1801-1809:3;1813-1831:4")]
[assembly: global::go.GoPositionMap("net/http/example_filesystem_test.go", "example_filesystem_test.cs", "ABAksoKCgqYACRjSgpKCpoKUAAgYspKWgoKUpoKCgg==")]
[assembly: global::go.GoPositionMap("net/http/example_test.go", "example_test.cs", "ABsigqKCgoKUgoKCppKCgoKCgpSCouaCgoKUgoKClIKU1pTmuKa42tSCgoKmgoKUAA8MooLKgoSChIKCgriChIKSgoKWgJSkloCUpvaCgqiCguaWgpaC9oKClIKWgoTWgoL4gpaWhKaCyoKEpoKWgoKEgoKClA==", "18-40:1;85-93:1;100-115:1;122-133:1;144-146:1;157-159:1;166-168:1;169-171:2;180-182:1")]
[assembly: global::go.GoPositionMap("net/http/fs_test.go", "fs_test.cs", "ADtcABg6gAALAoKClISEgoKYkoKAgu4ACBKCgoK6goKClICSuIKCgoKUgoKUgpSCgoKUgoKUgoKCgoKUgqaCgoKClIKClIKClICCgqSCgoKCgpSCgJKkgoKClIKCpoKCAAwMggALHoKCgoKUgoKC/JKCgoKCgoLqkriCgoKCgoKUgoIACAoABRCA0oKEgoKClIKAkgAJEoIACgaigoKSgqTOgoKCgoKAgtakgAAMAoKCggAHFpKCgsqWgoKCgoKUgoKUgoKUgIKkuIAACAKigpKCABAmhIKClJSCgpSCgrSkgoKCuIAACQKCgoCCpIKSgoKUgoKUgpSAgqSAggAOCIKClIKClLKCgpSSgoKUgrSSgoKCgoKogoLWgqKCgoKUspKCgqaAAAgCgoKClKa0lJKCgpSAgqSUgoKmgOKCgpSCgpSCgoCCyICigoKUgoKUgoK6kNKCgoKUgoKUgoCCzKCiooKCAAcSlIKClIKAgsyg8qKCgoKCAAcSlIKClIKCgpSCgoKUgpSAkraAgqSClIK4gAAJAoKCgoKUgrSCtIKChIKCgpSCgpSAgqTcgNKigoKUgoKClIKClICCpLSkgNKihIKClJKCgpSCgoKClIK0pICiooLIgoKUgoIADR6AooCigKKAooCigoKUpoIACRSAooCigoKUhIKClIKWgpTcgoKCgpSClKaAAAgCgoKSgpLKAAcShIKClIKClIKUhIKCloKEgoKClIKUloSCgpSClKaCgoKUpoAAHQKiAAAQkoKClIKUlgAAHgDdAawDgoKCgoKUkpSUlICCpu6CgpSCloKCgpSCgoKUgJKkgJKkgJLo9pKCgoKCgoKAgvyA6ICigOSCkoKUggAOCIKClAAHEIKSgoKCgpSCgoKCgoKUgoKUgoCCgoKUAA4OsoKCgpSAgqaCgpSCgpSmgIKmgoSAgqSEgoKCgoKCgIKmgoKUgoKUloKEgoKCtKSigoKUgoKU+sKClIKCgoKUgoKClIKCgrSmkpKClIIADAqChJKClIKCloKCgoKUgqiCgpSCzIKCloIACAaCAAQSgoKCgoKClIIACBKCgpSUAAwKggAJHIKCgs6iAAgEgoQACR6CkqKCgpSCgoKUgJKkgoKClICS7IKkgoSCgt4ACRSCgoKClIKCgpSCgpSClICSAAgKooKCpoKUgoKUgoKUgIKkoqSigoKmkpSUgoKUgoKUgIKkoqQAERyCgqaygoKSopKUgoKUgoKUgIKkov6CpoKCgILKkOKCooKUgoKUgJKkopSSlpK4goKUggAOBqKClIKSgoKCgoKCgpSUgoKUhIKCgpaChIKClJSAkqSAkqSAkqSAkqSAkqSAkqSAkqSAkqSAkqSAksQ=", "77-79:1;305-308:1;431-442:1;459-472:1;488-495:1;504-513:1;514-523:2;531-533:1;547-549:1;564-566:1;581-593:1;608-622:1;668-686:1;908-917:1;1227-1234:1;1228-1230:1.1;1231-1233:1.2;1249-1255:1;1371-1373:1;1383-1390:1;1384-1386:1.1;1385-1385:1.1.1;1387-1389:1.2;1388-1388:1.2.1;1405-1425:1;1406-1424:1.1;1523-1544:1;1626-1628:1;1664-1669:1;1705-1717:1;1719-1721:2;1723-1725:3;1729-1731:1;1732-1734:2;1741-1750:1;1768-1773:2")]
[assembly: global::go.GoPositionMap("net/http/main_test.go", "main_test.cs", "ABYolIKCgoKUAA4GooKCgoKCAAEYlJSCqJKmloKCgoKCgoKUgqaUgoKU3LKClILogoKClIKmyoKCgpTcAAcShIIABxCCgoKCgoKmgoK4lKqigoKCgoKC")]
[assembly: global::go.GoPositionMap("net/http/netconn_test.go", "netconn_test.cs", "ABUogtyCAA0gggAKDIKClLjSgpSCgoKCgoKClNbSgoKClIKC1tKCgoLWggACENKCgoKCgoKCggAPKJKClKqigpSo8oKU6OKClIKm2JKCpoKCgoKUgoKUgoKUqJKokqiSgoKokoKokoKqogAYOIIACBCCqJIAEhLigoKUtLS02ta0pMaCxKSkyta0pMaCxKSkyNKCgtbigIKkgoKU5tKCgqKkooKCgoKmpuKAgqSCgpSCgpQADhjigoKClNjSgqaCpoKUlJSClIKClJSCpqKCgoKisg==", "430-435:1")]
[assembly: global::go.GoPositionMap("net/http/request_test.go", "request_test.cs", "ACY+goKCgIL8ooKUgoKUgoIADQqClISAgqSAgqSAgqSAgqSAgqSAgqSAgqSAgqSAgqSAgvqSgpSCgoKUgIIACwqCAAoYpMqClLS0AAoOgoK4goKClIIACQqCAAQSgsqCgoKUlIIAGAySAAAizN6CgpaCgpbugpa4gviCyoKCloKCggAOCpKOyoKClIK+stSCgriAgoK2goKCgpSAgqSAgqSCgpSCgoKUgoCSyIDigoKUgqSkuIKCgpSCgoIACQiCgoKAkgAICNiCgIKkkoKAgqSi2MKCgoCCpJKipMaCkoKmoqSmggAICJKCgoKUgoLqkoKClIKC7KKCgIKkgIL8woKAgqSSgILE2KKCgIKkgIIADQgAJmCCgoKCgpaClJaCAAgKAAwigoKCgoKUggALCoKCgpSCgpSCgoKWgoKkABAIgoKCgpQADCKCgoKUggAKCgAYMoKCgoIAEiQABRKCgoKCgoLKgoKClIKCAAoKAAwmgoKCgoKCAAoWgoKmgoKmgpKCgtyC+IKSgoKUgoKC3ILogpKCgpSCgtyCAAgIooKClIKUggAHEpKEgoCCpoKUgoK4goKCgoKCggAHEICigAAIELKCgpSmgoK4ggAIDKKCgoKCgoKCgpSCgpSClIKmAAQQgoKAggAPDsKCAEWSAYKEhIKWggAICoKCgqiCgoIACQyigoKolISClIL8ooKEgqiAkqSAkriAkqSAksqQooKUgoKUgriAggALCpLOgoKCgpSCgpSCgpSCgpSCgpSCgpSC+oKCgpSClIL4goKCgpSCggALBqKAkqSAkqSAgqaCgIK2gpKCgpKClICCuKLUgoKClIKUgoKClICCpAAICqIACh6CgpSCgoKogpSClIIACgqCAChcspKCgpSCloSCgoKUAA1IooKCgoKCgoKCggALGIKCggAQBpQACxiUAAsOlAANDpQACRKUqoKmgqaC1qKCgpaSgpaAgqaitKSiooKCgpaCtJSEgpSCgIKmgoKClpSCgoKWguiCgriEkoCSpIKAkgAKCKIAJlCCkoKCgqaCpoKSgoKUtNSigoLKgpSCgILIgpKCgpSiABAEtJKCgoKCgoKClAAPJIKClIKClIKAkqSAktY=", "123-138:1;300-308:1;338-348:1;402-406:1;601-605:1;894-914:1;1088-1088:1;1180-1184:1;1309-1327:1;1321-1324:1.1;1457-1460:1;1466-1468:2;1472-1482:1;1573-1583:1;1596-1610:1;1622-1622:1")]
[assembly: global::go.GoPositionMap("net/http/responsecontroller_test.go", "responsecontroller_test.cs", "ABckgOKigpKCgoCCgqSCloKClJSCgoKCloKCtKSAAAgCgoKCgoKCgoKUgpSCgpSUgoKUgJLIgqSigoKCgIKkgIKmgoCC7ICCpIKAgriCgpSSgoK0pIKkwoKCooKCgIKkgoCCpIKWgoKClJKCgpSCgrSkgtSigoLCgoKCgoKCgpSAgoKkgoKUuICCgqSCgrSUgoKC0oKSgubWgraikoKCgsSkguSigoKCgIKkgoKUlIKCgpSSgoKUouyCpoCiooKCgoCCpICCpICCtoKClIKypIKkooKCgMqCtoKCgoKCgpSCpoKCgpSSgoCCpIKAgqSCpqKkgoKCgIKklIKClA==", "21-30:1;55-70:1;84-109:1;128-140:1;164-194:1;199-214:2;227-237:1;261-273:1;286-308:1;331-337:1")]
[assembly: global::go.GoPositionMap("net/http/serve_test.go", "serve_test.cs", "AEdssoKCgpSCgqaC1oKmgqaCAAgKgNKAooCigKKAAAoWgoKU+AAKFoKm0oKC1oKmgviqogAHEIKmooKCyoK4koLWooKCgoIAARCCgoKCkoKWspaCgoKUgqiCgpSCqICCxMiC9gAMFAALIICiooKClISCgpSSgoKCgoCCgqSAgoKkgoKClJSCgraCgrbW9AAHGJKCvtKCgoKCgpQADQgAGkiCgoKCloLugoKCgvyygoKAgraCogAKBAAFHKKCgoKWgoKCgoKClMqCgoKCgpSUgoKUgpSC/JKCgoKCgpSChISAkoKmgJKCAAUSwgAPBIKCloKCgoKCloQADCiCgoKClIKCgoCStoCSABUKgoSCgoKCgoKEAA4sgoKChICSpoKAkraAkgAGErKCgpSCgoKAkv6ygoKUgoKCgJLIgKKCgoK2gKKAAAgCogABEIKCgpLugoKWgoKCgoKCgoKCgu6AooIADBbCgoKUgpSSloKCgpSCgoK6goKClIKCgoKClIKCzIKClIKCgoKWgoKClJKCgoKClKbWgNLCgpKCgoKUlIKCloKCgpSWgoKCgoKUkoKClIK0pIDiwoKCgoKCgqaAgoKkgoKUlIKWgoKClKaCgoSCgsakgKLCkoKigoKUggABKAARAoKClJaClIKClJKCgpSkgoKUpoIACAqQosKCgoKUgpaCgpSSgoK4grSmktSCgpSkmISCgoKWgoKUguyigoKCgpSCgqaokoKUgpLsooKi2taUkoSEgoKUgoKUhIKClIKCgpSCgoKm2LKClIKCkrikooKS2taShISCgoKUgoKUlNyw4oKYgsKCgoKClIKCgoKUgoKikpaEooKClIKClIKWgIIACxiygoKCgpSokAAIAqKCloKCgpSCgsaCtMiCzIKCgoKUgIKkgJKkgJK2qIKCgpSEgrqCgpSCgqiCgoIACgqigoSCgpSUgoKWgoKCloKCloK0pKKCgoKClJKCgoCCpIKClICCpLTWkoLqkv7CguiCguiCvKCigKaS1oLWgsyQAAkCgoKClIiCgoKUgIKkgpSUgriAooKCloKClIKClIKCABASgoKClLiCACMQgqiS5IKCkpTMlITCgoKCgpSSgoKCgpSipoKmloKSlqiCgJK4qIKAksyg4oKCgoKogoKmgoKUgpSAgqSAgqSCgpSCvKCilIKClIKClIKCgpSCgoKU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", "189-192:1;194-196:2;314-316:1;397-401:1;496-498:1;504-506:2;608-610:1;624-626:1;639-639:1;694-696:1;701-703:1;703-706:2;777-783:1;783-787:2;790-795:3;819-834:1;834-837:2;862-866:1;866-869:2;889-894:3;926-929:1;929-932:2;958-958:1;959-961:2;1004-1008:1;1005-1007:1.1;1013-1021:1;1021-1023:2;1064-1068:1;1065-1067:1.1;1073-1081:1;1111-1127:1;1127-1129:2;1133-1141:3;1176-1191:1;1308-1310:1;1315-1317:1;1323-1325:1;1329-1331:1;1335-1338:1;1362-1365:1;1390-1392:1;1441-1443:1;1443-1448:2;1454-1469:3;1508-1519:1;1548-1550:1;1575-1575:1;1576-1579:2;1602-1609:1;1609-1611:2;1667-1669:1;1670-1670:2;1677-1677:3;1705-1707:1;1707-1710:2;1792-1794:1;1810-1812:1;1832-1834:1;1840-1840:2;1922-1932:1;1934-2009:2;1949-1991:2.1;2032-2036:1;2039-2052:2;2075-2084:1;2230-2234:1;2239-2246:2;2290-2296:1;2325-2331:1;2453-2459:1;2487-2491:1;2542-2551:1;2567-2575:2;2569-2569:2.1;2584-2586:1;2601-2613:2;2603-2603:2.1;2623-2628:1;2681-2683:1;2707-2721:1;2747-2749:1;2767-2769:1;2770-2772:2;2893-2902:1;2930-2932:1;2936-2938:1;2943-2945:1;2960-2969:1;2973-2975:2;2979-2987:3;3015-3028:1;3028-3030:2;3042-3044:1;3048-3050:1;3054-3057:1;3070-3073:1;3089-3110:2;3126-3128:1;3204-3220:1;3258-3258:1;3285-3290:1;3316-3367:1;3326-3328:1.1;3339-3342:1.2;3343-3349:1.3;3372-3376:1;3396-3396:1;3428-3433:1;3439-3447:2;3470-3479:1;3486-3495:2;3525-3530:1;3553-3576:1;3599-3622:1;3644-3646:1;3696-3698:1;3698-3700:2;3735-3737:1;3738-3746:2;3750-3755:3;3756-3767:4;3771-3774:5;3775-3780:6;3784-3788:7;3789-3797:8;3801-3806:9;3807-3818:10;3822-3825:11;3826-3831:12;3835-3838:13;3839-3844:14;3848-3849:15;3850-3855:16;3859-3861:17;3862-3867:18;3871-3874:19;3875-3883:20;3926-3926:1;3944-3957:1;3950-3956:1.1;3975-3986:1;4003-4003:1;4055-4074:1;4058-4061:1.1;4097-4105:1;4128-4132:1;4165-4235:1;4172-4187:1.1;4190-4193:1.2;4196-4225:1.3;4212-4212:1.3.1;4252-4258:1;4253-4256:1.1;4262-4277:2;4291-4294:1;4322-4324:1;4325-4328:2;4329-4333:3;4334-4339:4;4354-4369:5;4371-4373:6;4373-4395:7;4375-4394:7.1;4396-4399:8;4403-4423:9;4425-4428:10;4430-4433:11;4435-4437:12;4439-4441:13;4443-4449:14;4451-4461:15;4463-4479:16;4486-4487:1;4487-4489:2;4504-4506:1;4511-4530:2;4541-4545:1;4546-4546:2;4590-4601:1;4631-4635:1;4635-4637:2;4640-4649:3;4680-4683:1;4726-4728:1;4755-4759:1;4790-4796:1;4819-4822:1;4833-4836:1;4837-4848:2;4910-4910:1;4930-4952:1;5014-5014:1;5034-5042:1;5066-5070:1;5086-5092:1;5105-5107:1;5125-5128:1;5140-5145:1;5161-5163:1;5188-5192:1;5189-5191:1.1;5198-5200:1;5203-5222:2;5263-5266:1;5311-5318:1;5337-5340:2;5341-5344:3;5395-5398:1;5449-5453:1;5478-5481:1;5497-5501:1;5506-5509:1;5514-5517:1;5522-5524:1;5538-5541:1;5555-5561:1;5580-5583:1;5607-5607:1;5608-5608:2;5622-5685:1;5623-5626:1.1;5626-5629:1.2;5636-5650:1.3;5707-5709:1;5714-5714:2;5734-5742:3;5757-5791:1;5759-5765:1.1;5761-5763:1.1.1;5793-5795:2;5794-5794:2.1;5819-5821:1;5821-5826:2;5833-5835:3;5882-5882:1;5887-5895:2;5899-5902:3;5933-5971:1;5934-5941:1.1;5941-5944:1.2;5949-5954:1.3;5987-6025:1;5988-5991:1.1;6067-6092:1;6077-6080:1.1;6111-6134:1;6167-6191:1;6275-6278:1;6309-6311:1;6312-6314:2;6332-6334:1;6334-6347:2;6335-6340:2.1;6341-6346:2.2;6368-6370:1;6370-6377:2;6371-6376:2.1;6401-6403:1;6457-6463:1;6469-6475:2;6480-6486:3;6502-6527:4;6503-6508:4.1;6566-6634:1;6571-6579:1.1;6656-6662:1;6673-6686:1;6686-6688:2;6737-6758:1;6758-6761:2;6812-6812:1;6826-6837:2;6828-6831:2.1;6832-6835:2.2;6841-6856:1;6864-6866:2;6890-6894:1;6891-6893:1.1;6909-6985:1;6917-6921:1.1;6933-6940:1.2;6989-6999:1;7008-7011:1;7034-7047:1;7084-7087:1;7085-7085:1.1;7086-7086:1.2;7093-7096:1;7094-7094:1.1;7095-7095:1.2;7100-7108:1;7149-7152:1;7165-7167:2;7184-7187:1;7229-7234:1;7234-7236:2;7260-7266:1;7261-7265:1.1;7266-7268:2;7287-7294:1;7288-7292:1.1;7294-7296:2;7343-7343:1;7356-7358:2;7359-7362:3;7387-7421:1;7389-7395:1.1")]
[assembly: global::go.GoPositionMap("net/http/sniff_test.go", "sniff_test.cs", "ACEmACh+goKCgsqAoqKCgoKCgqaUgoKCgu6CgpSAgqSCgqSUtKigooKCgpaCgpaCgoKU7oKClIKUgoKmgAASAoKavoKC7oKCgoIABhCEgoLugoKC7oKC+JKEgoKUgIKkgJKkgoKklM6A0oKCgoKCgpSCpoKCgpSAgqSAgg==", "93-100:1;135-138:1;181-187:1;190-199:2;202-210:3;213-220:4;223-229:5;231-252:6;259-269:1")]
[assembly: global::go.GoPositionMap("net/http/transport_dial_test.go", "transport_dial_test.cs", "ABQggpaCgoKCloKCpoKWgoKCqIKCgqaCloKCgrqCgoLMgoKChAAhUIKCuKSCgqaUktyCgIKkgoK4gqaqwoKSgu6CgpKClKKkpoKCgoKUqLKCgoKUgryihIKUhISClIKCgqiSgoKCgqiSgoI=", "121-129:1;129-150:2;130-149:2.1;169-172:1;173-184:2;175-177:2.1")]
[assembly: global::go.GoPositionMap("net/http/transport_test.go", "transport_test.cs", "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loKAkqSAksiC1IKCkoKUgoKCgpSWgqi4voKCgpaCgJKkgJLIuIKCpqKClIKClIKUgIKk1oKCgpSClAAHEIKCuAAoSqKEhIKCgoIAABDChIKCgoK0lIKCloSCgoKUgoKWgpaCptKCgIK2gtqipIKCgoKC3oSCgpaC3ID+goKCpoDmkoKCgoKCggADEtLUooKCgoKC0oS0xpCmgoKClIKClKKUhIKCgpaCurKCgoK0gqQAAxLSpIKCgoLSgoS0xpCUgoKClITcgpSi3IKCgoSCgoLMgoKCqIKmggAJBKKCgrKCgoKUkoKCgoKilIKCgoKClIKUgoKUkoKClICSpIKClICSxKSAAAgCooKigoKClIKCgpSCgoKUkpSCgoKClJSC1oKSgoKUgoKClJKClIKCgoKClIKCgsYACASCggAkUrKSgoIADByCgqaAAAwCgoKigoKogIKkgIKmkoKWlpgAHkqysoKClJSCkpKCgoKWgoKWgqi4goKqgoKUgoKCgpSSgpaCgpSCloK0yIKSuKKiogATJNqCgoKCgoKCgpSCqICCuIKCggAICIIAChqCgILagtS0hIKEhKKCgoKUkoKikoKClIKClIKClIKUgpaSgIKClKSUgILEpILUooKCioKUgoKUkoCCpICCxMiCgqSCqqIADASCgpaEADJwsqKCgoKCgpSClICSADEk4oKCgpTokqSClISClIKCgoKClJaCgoK6hJSCxoKCgrSkgoKUgoKmgJKkgJIABBDCpIKEgoKCqISCgoSC0oKCgpSSgoK0lIKCgrKCoqSChIIACA7SgoK4goKCpLSkyKKCyoCmkPKCgoCCtoKUgIKklICCzKLkgoKCloKCgpSAkgAKFICigKKAooCigKKAAAwSgoKCgIK2psKCgpKShMqUlNKEgqaClOjEpKiChKKs4oKUgoKCgpSCgoKWtKqypIKCkoKCgpaCgoKEhIKCguKCyIKCgqaCgoKUtLi0tKSCtNiCguKCgoKClIKooriChISCpoCigoKCloKCgrKCgoKCgoKCxqSmgPKClJSCgIK2goKCgriUAAsEgoKChqKmgoKmgoKUgoKCgoKU2LKChIKigoKCuICCgqSCgpaCkJKEloKCloKClqiEqJKCgoKUlLSkggAKBqKClgADELKCooKClIKCgpSAkqSCABMMooKEkoKUgoKUgoQAEDCCggAHEIIABxAACRSCgoIACBIACBKCgoLugu6Cgu7KgoLuyoKC7oKmgoLugqaC7oKmgu6CpoLugqaC+NbKpsyClIKUgpSCgpaCgsKUpLaigpaCgoKClJSAguyC1MKCzISUlIKCgoKygpKCgoKUosQ=", "109-117:1;144-146:1;178-188:1;214-236:1;270-296:1;360-390:1;371-378:1.1;435-445:1;486-499:1;509-526:2;510-516:2.1;570-575:1;580-584:2;591-608:3;594-596:3.1;640-645:1;655-662:2;664-687:3;666-670:3.1;671-673:3.2;692-695:4;745-750:1;789-791:1;796-812:2;820-829:3;843-867:1;844-850:1.1;900-908:1;924-938:2;949-955:1;984-991:1;997-997:2;1035-1050:1;1101-1128:1;1118-1120:1.1;1219-1227:1;1233-1235:2;1243-1250:3;1336-1397:1;1406-1408:2;1410-1432:3;1447-1549:1;1449-1451:1.1;1453-1492:1.2;1487-1490:1.2.1;1569-1571:1;1573-1615:2;1610-1613:2.1;1630-1642:3;1638-1640:3.1;1645-1654:4;1680-1690:1;1682-1688:1.1;1697-1727:2;1731-1733:3;1758-1760:1;1761-1763:2;1806-1810:1;1837-1840:1;1864-1867:1;1908-1913:1;1922-1931:2;1979-1980:1;2057-2065:1;2133-2136:1;2143-2163:2;2147-2160:2.1;2191-2194:1;2199-2207:2;2219-2224:1;2256-2285:1;2261-2263:1.1;2292-2294:1;2322-2324:1;2335-2335:2;2343-2367:3;2378-2380:1;2385-2395:2;2417-2419:1;2420-2423:2;2428-2438:3;2489-2492:1;2493-2497:2;2570-2585:1;2573-2575:1.1;2576-2578:1.2;2579-2583:1.3;2591-2591:1;2594-2597:2;2598-2600:3;2601-2605:4;2614-2616:1;2617-2619:2;2620-2624:3;2629-2641:1;2631-2633:1.1;2635-2637:1.2;2638-2640:1.3;2654-2658:1;2686-2695:2;2703-2705:1;2714-2717:2;2720-2731:3;2735-2737:1;2741-2743:1;2762-2769:1;2775-2780:2;2792-2794:3;2820-2824:1;2853-2862:2;2867-2874:1;2868-2870:1.1;2871-2873:1.2;2878-2880:1;2902-2906:1;2912-2915:2;2943-2952:1;3049-3053:1;3054-3056:2;3062-3073:3;3087-3109:4;3110-3112:5;3130-3130:1;3131-3131:2;3133-3177:3;3180-3194:4;3186-3190:4.1;3200-3216:5;3232-3237:1;3244-3247:2;3264-3270:1;3299-3305:1;3311-3314:2;3333-3339:1;3364-3368:1;3445-3452:1;3460-3467:1;3479-3483:1;3487-3487:2;3492-3494:3;3534-3536:1;3562-3570:1;3573-3575:2;3608-3617:1;3691-3693:1;3701-3711:2;3712-3715:3;3717-3733:4;3729-3732:4.1;3759-3768:1;3812-3818:1;3837-3839:2;3849-3851:3;3860-3862:4;3873-3875:5;3881-3961:6;3886-3891:6.1;3893-3896:6.2;3900-3918:6.3;3909-3916:6.3.1;3920-3922:6.4;3969-3972:1;3981-3987:2;4013-4017:1;4019-4028:2;4054-4058:1;4060-4065:2;4098-4102:1;4104-4113:2;4137-4139:1;4155-4162:1;4195-4202:1;4204-4204:2;4210-4213:3;4238-4240:1;4241-4242:2;4250-4252:3;4255-4262:4;4283-4285:5;4290-4296:6;4314-4318:1;4319-4321:2;4325-4332:3;4359-4362:1;4363-4365:2;4370-4377:3;4401-4408:1;4423-4427:1;4464-4487:1;4466-4471:1.1;4496-4496:1;4497-4509:2;4498-4508:2.1;4501-4503:2.1.1;4504-4506:2.1.2;4516-4518:3;4571-4573:1;4576-4581:2;4583-4594:3;4626-4628:1;4636-4644:2;4685-4739:1;4690-4697:1.1;4709-4716:1.2;4792-4794:1;4820-4823:1;4854-4854:1;4859-4875:2;4889-4894:3;4891-4893:3.1;4896-4898:4;4899-4913:5;4928-4930:1;4934-4936:1;4942-4949:1;4953-4953:2;4954-4954:3;4955-4955:4;4956-4956:5;4957-4957:6;4987-4991:1;5018-5020:1;5025-5027:1;5033-5045:1;5045-5049:2;5056-5061:3;5070-5076:4;5082-5082:5;5083-5083:6;5084-5084:7;5085-5085:8;5086-5086:9;5087-5087:10;5088-5088:11;5089-5094:12;5095-5097:13;5098-5100:14;5101-5101:15;5102-5102:16;5103-5106:17;5109-5109:18;5110-5112:19;5147-5151:20;5152-5156:21;5218-5223:1;5225-5227:2;5227-5232:3;5228-5231:3.1;5245-5245:4;5246-5248:5;5262-5266:6;5300-5305:1;5309-5309:2;5310-5310:3;5311-5311:4;5312-5312:5;5326-5330:6;5365-5365:1;5370-5374:2;5375-5382:3;5411-5413:1;5422-5424:2;5426-5434:3;5470-5472:1;5478-5484:2;5487-5522:3;5490-5494:3.1;5533-5541:4;5559-5561:1;5571-5593:2;5624-5624:1;5627-5639:2;5629-5632:2.1;5633-5636:2.2;5654-5667:1;5667-5671:2;5679-5685:3;5689-5694:4;5695-5699:5;5723-5734:1;5737-5739:2;5765-5776:1;5779-5781:2;5787-5792:3;5911-5920:1;5948-5952:1;5963-5968:1;6029-6050:1;6037-6037:1.1;6091-6119:1;6100-6100:1.1;6156-6168:1;6204-6234:1;6309-6314:1;6334-6354:1;6348-6351:1.1;6356-6358:2;6357-6357:2.1;6398-6461:3;6406-6422:3.1;6407-6421:3.1.1;6427-6431:3.2;6467-6467:1;6468-6470:2;6471-6471:3;6472-6472:4;6473-6473:5;6474-6474:6;6486-6486:7;6492-6492:8;6557-6566:1;6586-6594:2;6604-6613:1;6643-6645:1;6706-6720:2;6750-6750:1;6757-6765:2;6778-6781:3;6782-6788:4;6817-6822:1;6831-6842:2;6847-6850:3;6895-6899:1;6901-6907:2;6919-6922:1;6972-6974:1;6980-6996:2;7040-7045:1;7057-7076:2;7060-7067:2.1;7095-7108:3;7123-7126:1;7131-7142:2;7153-7157:1;7173-7184:1;7205-7220:1;7223-7223:2;7249-7258:3;7266-7268:1;7280-7296:2;7329-7331:1;7336-7342:2;7347-7351:3;7356-7359:4;7364-7372:5;7377-7381:6;7386-7393:7;7398-7401:8;7406-7410:9;7415-7417:10;7418-7424:11;7429-7431:12;7432-7438:13;7443-7446:14;7447-7451:15;7456-7459:16;7460-7463:17;7468-7471:18;7472-7475:19;7480-7483:20;7484-7487:21;7492-7495:22;7496-7499:23;7502-7559:24;7509-7511:24.1;7535-7542:24.2;7543-7546:24.3;7576-7577:1;7577-7579:2;7584-7593:3")]
// </GoSourcePositionMaps>

namespace go.net;

[GoPackage("http_test")]
public static partial class http_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestFileServerCleanPath_tests {}
    internal partial struct TestFileServerCleans_tests {}
    internal partial struct TestH12_RequestContentLength_Unknown_type {}
    internal partial struct TestHeaderToWire_tests {}
    internal partial struct TestInvalidChunkedBodies_type {}
    internal partial struct TestIs408_tests {}
    internal partial struct TestMaxBytesReaderDifferentLimits_tests {}
    internal partial struct TestMaxBytesReaderStickyError_tests {}
    internal partial struct TestMultipartReader_tests {}
    internal partial struct TestNewRequestContentLength_tests {}
    internal partial struct TestNewRequestContentLength_type {}
    internal partial struct TestNewRequestGetBody_tests {}
    internal partial struct TestOnProxyConnectResponse_type {}
    internal partial struct TestParseFormUnknownContentType_type {}
    [GoLocalName("version")] internal partial struct TestParseHTTPVersion_version {}
    internal partial struct TestPathValueAndPattern_type {}
    internal partial struct TestQuerySemicolon_tests {}
    [GoLocalName("ctHeader")] internal partial struct TestRedirectContentTypeAndBody_ctHeader {}
    internal partial struct TestRedirectContentTypeAndBody_type {}
    internal partial struct TestRedirect_type {}
    internal partial struct TestReferer_tests {}
    internal partial struct TestRequestCookie_type {}
    internal partial struct TestRequestCookiesByName_tests {}
    internal partial struct TestScanETag_tests {}
    internal partial struct TestServeFile_DotDot_tests {}
    internal partial struct TestServeWithSlashRedirectForHostPatterns_tests {}
    internal partial struct TestServerValidatesHeaders_tests {}
    internal partial struct TestServerValidatesHostHeader_tests {}
    internal partial struct TestServerValidatesMethod_tests {}
    internal partial struct TestShouldCopyHeaderOnRedirect_tests {}
    internal partial struct TestStatus_type {}
    internal partial struct TestStripPasswordFromError_testCases {}
    internal partial struct TestTransportProxy_testCases {}
    internal partial struct TestTransportRequestReplayable_tests {}
    internal partial struct TestTransportServerProtocols_type {}
    internal partial struct apiHandler {}
    internal partial struct asyncResult<T> {}
    internal partial struct basicAuthCredentialsTest {}
    [GoLocalName("test")] internal partial struct benchmarkServeMux_test {}
    internal partial struct blockingRemoteAddrConn {}
    internal partial struct blockingRemoteAddrListener {}
    internal partial struct bodyCloser {}
    internal partial struct bodyLimitReader {}
    internal partial struct breakableConn {}
    internal partial struct brokenState {}
    internal partial struct byteAtATimeReader {}
    internal partial struct byteFromChanReader {}
    internal partial struct cancelProto {}
    internal partial struct cancelTest {}
    internal partial struct cancelableTimeoutContext {}
    internal partial struct cleanupT {}
    internal partial struct clientServerTest {}
    internal partial struct closeWriteTestConn {}
    internal partial struct contextCounter {}
    internal partial struct countCloseListener {}
    internal partial struct countCloseReader {}
    internal partial struct countedConn {}
    internal partial struct countedContext {}
    internal partial struct countingDialer {}
    internal partial struct deadlineContext {}
    internal partial struct delayedEOFReader {}
    internal partial struct delegateReader {}
    internal partial struct doneContext {}
    internal partial struct dotFileHidingFile {}
    internal partial struct dotFileHidingFileSystem {}
    internal partial struct dummyAddr {}
    internal partial struct dumpConn {}
    internal partial struct eofListenerNotComparable {}
    internal partial struct errorListener {}
    internal partial struct fakeFS {}
    internal partial struct fakeFile {}
    internal partial struct fakeFileInfo {}
    internal partial struct fakeNetConn {}
    internal partial struct fakeNetConnHalf {}
    internal partial struct fakeNetListener {}
    internal partial struct fileServerCleanPathDir {}
    internal partial struct fooProto {}
    internal partial struct fsRedirectTestDataᴛ1 {}
    internal partial struct funcConn {}
    internal partial struct getBasicAuthTest {}
    internal partial struct getBasicAuthTestsᴛ1 {}
    internal partial struct gzipResponseWriter {}
    internal partial struct h12Compare {}
    internal partial struct handlerBodyCloseTest {}
    internal partial struct handlerTest {}
    internal partial struct handlersᴛ1 {}
    internal partial struct http09Writer {}
    internal partial struct infiniteReader {}
    internal partial struct issue12991FS {}
    internal partial struct issue12991File {}
    internal partial struct issue15577Tripper {}
    internal partial struct issue18239Body {}
    internal partial struct issue40382Body {}
    internal partial struct lockedBytesBuffer {}
    internal partial struct logWrites {}
    internal partial struct logWritesConn {}
    internal partial struct neverEnding {}
    internal partial struct newRequestHostTestsᴛ1 {}
    internal partial struct nilBodyRoundTripper {}
    internal partial struct noopConn {}
    internal partial struct noteCloseConn {}
    internal partial struct oneConnListener {}
    internal partial struct panicOnSeek {}
    internal partial struct parseBasicAuthTestsᴛ1 {}
    internal partial struct parseHTTPVersionTestsᴛ1 {}
    internal partial struct proxyFromEnvTest {}
    internal partial struct readRequestErrorTestsᴛ1 {}
    internal partial struct recordingTransport {}
    internal partial struct redirectTest {}
    internal partial struct repeatReader {}
    internal partial struct responseWriterJustWriter {}
    internal partial struct roundTripTestsᴛ1 {}
    internal partial struct roundTripperWithoutCloseIdle {}
    internal partial struct rwTestConn {}
    internal partial struct serveMuxRegisterᴛ1 {}
    internal partial struct serveMuxTests2ᴛ1 {}
    internal partial struct serveMuxTestsᴛ1 {}
    internal partial struct serverExpectTest {}
    internal partial struct slowTestConn {}
    internal partial struct slurpResult {}
    internal partial struct sniffTestsᴛ1 {}
    internal partial struct stringHandler {}
    internal partial struct terrorWriter {}
    internal partial struct testClientHeadContentLength_tests {}
    internal partial struct testClientRedirectTypes_tests {}
    internal partial struct testCloseConn {}
    internal partial struct testConn {}
    [GoLocalName("connKey")] internal partial struct testConnContextNotModifyingAllContexts_connKey {}
    internal partial struct testConnSet {}
    internal partial struct testConnectRequest_tests {}
    [GoLocalName("setting")] internal partial struct testContentEncodingNoSniffing_setting {}
    [GoLocalName("readerOnly")] internal partial struct testContentTypeWithVariousSources_readerOnly {}
    internal partial struct testContentTypeWithVariousSources_type {}
    internal partial struct testErrorReader {}
    internal partial struct testFileServerEscapesNames_tests {}
    internal partial struct testFileSystem {}
    internal partial struct testHTTP10ConnectionHeader_tests {}
    internal partial struct testHandlerBodyConsumer {}
    internal partial struct testHeadResponses_src {}
    [GoLocalName("data")] internal partial struct testKeepAliveFinalChunkWithEOF_data {}
    internal partial struct testLogWriter {}
    internal partial struct testMockTCPConn {}
    internal partial struct testMode {}
    internal partial struct testNewClientServerTest_got {}
    internal partial struct testNotParallelOpt {}
    internal partial struct testRedirectsByMethod_log {}
    internal partial struct testRetryRequestsOnError_testCases {}
    [GoLocalName("serveParam")] internal partial struct testServeContent_serveParam {}
    [GoLocalName("testCase")] internal partial struct testServeContent_testCase {}
    internal partial struct testServeFileRejectsInvalidSuffixLengths_tests {}
    internal partial struct testServeWithSlashRedirectKeepsQueryString_tests {}
    [GoLocalName("stateLog")] internal partial struct testServerConnState_stateLog {}
    [GoLocalName("baseKey")] internal partial struct testServerContexts_baseKey {}
    [GoLocalName("connKey")] internal partial struct testServerContexts_connKey {}
    internal partial struct testServerExpect_type {}
    internal partial struct testStripPrefix_cases {}
    internal partial struct testTimeoutHandlerSuperfluousLogs_tests {}
    internal partial struct testTransportClosesBodyOnError_body {}
    internal partial struct testTransportClosesBodyOnInvalidRequests_tests {}
    internal partial struct testTransportNoReuseAfterEarlyResponse_sconn {}
    internal partial struct testTransportRejectsInvalidHeaders_tests {}
    internal partial struct testTransportRequestWriteRoundTrip_cases {}
    internal partial struct testTransportRespectRequestWantsClose_tests {}
    internal partial struct testTransportResponseHeaderTimeout_tests {}
    internal partial struct testTransportUserAgent_tests {}
    internal partial struct testValidateClientRequestTrailers_cases {}
    internal partial struct tlogWriter {}
    internal partial struct trackLastConnListener {}
    internal partial struct transport100ContinueTest {}
    internal partial struct transportDialTester {}
    internal partial struct transportDialTesterConn {}
    internal partial struct transportDialTesterRoundTrip {}
    internal partial struct vtestsᴛ1 {}
    internal partial struct wantRange {}
    internal partial struct wgReadCloser {}
    internal partial struct wrapWriter {}
    internal partial struct writeCountingConn {}
    internal partial struct writerFuncConn {}
    public partial interface TBRun<T> {}
    public partial struct RecordingJar {}
    public partial struct ServeFileRangeTestsᴛ1 {}
    public partial struct TestJar {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸgzip() => builtin.initPackage(typeof(compress.gzip_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸzlib() => builtin.initPackage(typeof(compress.zlib_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha1() => builtin.initPackage(typeof(crypto.sha1_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸtls() => builtin.initPackage(typeof(crypto.tls_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509() => builtin.initPackage(typeof(crypto.x509_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(global::go.@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(global::go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(global::go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(global::go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸmime() => builtin.initPackage(typeof(mime_package));
    [GoInit] internal static void initᴛᴛimportꓸmimeꓸmultipart() => builtin.initPackage(typeof(global::go.mime.multipart_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(global::go.net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸcookiejar() => builtin.initPackage(typeof(global::go.net.http.cookiejar_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸhttptest() => builtin.initPackage(typeof(global::go.net.http.httptest_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸhttptrace() => builtin.initPackage(typeof(global::go.net.http.httptrace_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸhttputil() => builtin.initPackage(typeof(global::go.net.http.httputil_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸinternal() => builtin.initPackage(typeof(global::go.net.http.internal_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸinternalꓸtestcert() => builtin.initPackage(typeof(global::go.net.http.@internal.testcert_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸnetip() => builtin.initPackage(typeof(global::go.net.netip_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸtextproto() => builtin.initPackage(typeof(global::go.net.textproto_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(global::go.net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(global::go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸsignal() => builtin.initPackage(typeof(global::go.os.signal_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(global::go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(global::go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸfstest() => builtin.initPackage(typeof(global::go.testing.fstest_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸiotest() => builtin.initPackage(typeof(global::go.testing.iotest_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸhttpꓸhttpguts() => builtin.initPackage(typeof(vendor.golang.org.x.net.http.httpguts_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.net.http_package));
    }
}
