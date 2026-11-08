// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.net.http_package;
global using static global::go.net.http_internal_test_package;

// <ImportedTypeAliases>
global using httpꓸCookie = go.net.http_package.ΔCookie;
global using httpꓸHandler = go.net.http_package.ΔHandler;
global using httpꓸHeader = go.net.http_package.ΔHeader;
global using netipꓸAddr = go.net.netip_package.ΔAddr;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectꓸType = go.reflect_package.ΔType;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using tlsꓸConnectionState = go.crypto.tls_package.ΔConnectionState;
global using urlꓸError = go.net.url_package.ΔError;
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
[assembly: GoDynamicTypeLift("7374727563747b70726f787920737472696e673b206b657920737472696e677d", "proxyAuthTestsᴛ1")]
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
[assembly: GoImplement<fakeConnectionStateConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<fakeConnectionStateConn, net_package.Conn>(Promoted = true)]
[assembly: GoImplement<fakeFS, go.net.http_package.FileSystem>(Pointer = true)]
[assembly: GoImplement<fakeFS, go.net.http_package.FileSystem>]
[assembly: GoImplement<fakeFile, go.net.http_package.File>(Pointer = true)]
[assembly: GoImplement<fakeFile, io_package.ReadSeeker>(Promoted = true)]
[assembly: GoImplement<fakeFileInfo, go.io.fs_package.FileInfo>(Pointer = true)]
[assembly: GoImplement<fakeNetConn, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<fakeNetConn, io_package.Writer>(Pointer = true)]
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
[assembly: GoImplement<go.testing.fstest_package.MapFS, go.io.fs_package.FS>]
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
[assembly: GoImplement<roundTripperGetBody, go.net.http_package.RoundTripper>(Pointer = true)]
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
[assembly: GoImplement<wgReadCloser, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<wgReadCloser, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<wrapWriter, go.net.http_package.ResponseWriter>(Promoted = true)]
[assembly: GoImplement<wrapWriter, go.net.http_package.ResponseWriter>]
[assembly: GoImplement<writeCountingConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<writerFuncConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<writerFuncConn, net_package.Conn>(Promoted = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(RecordingJar))]
[assembly: GoFieldAccess(typeof(TestRedirectContentTypeAndBody_ctHeader))]
[assembly: GoFieldAccess(typeof(asyncResult<>))]
[assembly: GoFieldAccess(typeof(blockingRemoteAddrConn))]
[assembly: GoFieldAccess(typeof(bodyLimitReader))]
[assembly: GoFieldAccess(typeof(bufio_package.ReadWriter))]
[assembly: GoFieldAccess(typeof(clientServerTest))]
[assembly: GoFieldAccess(typeof(contextCounter))]
[assembly: GoFieldAccess(typeof(countCloseListener))]
[assembly: GoFieldAccess(typeof(countedConn))]
[assembly: GoFieldAccess(typeof(countedContext))]
[assembly: GoFieldAccess(typeof(countingDialer))]
[assembly: GoFieldAccess(typeof(deadlineContext))]
[assembly: GoFieldAccess(typeof(fakeFileInfo))]
[assembly: GoFieldAccess(typeof(fakeNetConn))]
[assembly: GoFieldAccess(typeof(fakeNetConnHalf))]
[assembly: GoFieldAccess(typeof(fakeNetListener))]
[assembly: GoFieldAccess(typeof(global::go.net.http_package.Request))]
[assembly: GoFieldAccess(typeof(global::go.net.http_package.Response))]
[assembly: GoFieldAccess(typeof(global::go.net.http_package.Transport))]
[assembly: GoFieldAccess(typeof(global::go.net.http_package.conn))]
[assembly: GoFieldAccess(typeof(global::go.net.http_package.http2Transport))]
[assembly: GoFieldAccess(typeof(global::go.net.http_package.http2clientConnPool))]
[assembly: GoFieldAccess(typeof(global::go.net.http_package.pattern))]
[assembly: GoFieldAccess(typeof(global::go.net.http_package.persistConn))]
[assembly: GoFieldAccess(typeof(global::go.net.http_package.response))]
[assembly: GoFieldAccess(typeof(global::go.net.http_package.routingNode))]
[assembly: GoFieldAccess(typeof(global::go.net.http_package.ΔCookie))]
[assembly: GoFieldAccess(typeof(go.crypto.tls_package.Config))]
[assembly: GoFieldAccess(typeof(go.crypto.tls_package.ΔConnectionState))]
[assembly: GoFieldAccess(typeof(go.mime.multipart_package.FileHeader))]
[assembly: GoFieldAccess(typeof(go.mime.multipart_package.Part))]
[assembly: GoFieldAccess(typeof(go.net.http.httptest_package.ResponseRecorder))]
[assembly: GoFieldAccess(typeof(go.net.http.httptest_package.Server))]
[assembly: GoFieldAccess(typeof(go.net.http.httptrace_package.ClientTrace))]
[assembly: GoFieldAccess(typeof(go.net.http.httputil_package.ReverseProxy))]
[assembly: GoFieldAccess(typeof(go.net.http_package.Client))]
[assembly: GoFieldAccess(typeof(go.net.http_package.HTTP2Config))]
[assembly: GoFieldAccess(typeof(go.net.http_package.MaxBytesError))]
[assembly: GoFieldAccess(typeof(go.net.http_package.Request))]
[assembly: GoFieldAccess(typeof(go.net.http_package.Response))]
[assembly: GoFieldAccess(typeof(go.net.http_package.Server))]
[assembly: GoFieldAccess(typeof(go.net.http_package.Transport))]
[assembly: GoFieldAccess(typeof(go.net.http_package.ΔCookie))]
[assembly: GoFieldAccess(typeof(go.net.http_test_package.delegateReader))]
[assembly: GoFieldAccess(typeof(go.net.url_package.URL))]
[assembly: GoFieldAccess(typeof(go.net.url_package.ΔError))]
[assembly: GoFieldAccess(typeof(go.os.exec_package.Cmd))]
[assembly: GoFieldAccess(typeof(go.testing.fstest_package.MapFile))]
[assembly: GoFieldAccess(typeof(handlerTest))]
[assembly: GoFieldAccess(typeof(io_package.LimitedReader))]
[assembly: GoFieldAccess(typeof(issue40382Body))]
[assembly: GoFieldAccess(typeof(logWritesConn))]
[assembly: GoFieldAccess(typeof(oneConnListener))]
[assembly: GoFieldAccess(typeof(recordingTransport))]
[assembly: GoFieldAccess(typeof(rwTestConn))]
[assembly: GoFieldAccess(typeof(slowTestConn))]
[assembly: GoFieldAccess(typeof(testConn))]
[assembly: GoFieldAccess(typeof(testContentEncodingNoSniffing_setting))]
[assembly: GoFieldAccess(typeof(testMockTCPConn))]
[assembly: GoFieldAccess(typeof(testServerConnState_stateLog))]
[assembly: GoFieldAccess(typeof(time_package.Timer))]
[assembly: GoFieldAccess(typeof(transport100ContinueTest))]
[assembly: GoFieldAccess(typeof(transportDialTester))]
[assembly: GoFieldAccess(typeof(transportDialTesterConn))]
[assembly: GoFieldAccess(typeof(transportDialTesterRoundTrip))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("net/http/alpn_test.go", "alpn_test.cs", "ACEoooKCgoKClIKUgqampoK2goKCgpSCgpSC3IKCgtySpoKCkoKC3IKCgoKCgpSCgoKUgsYACgiigoKClIKCgpSCgoKCggAHEII=", "23-34:1", "", "", "96:29")]
[assembly: GoPositionMap("net/http/async_test.go", "async_test.cs", "AAwYAAcYwqaygqKSgqiSgqqipKSC", "28-31:1", "", "", "33:25")]
[assembly: GoPositionMap("net/http/client_test.go", "client_test.cs", "AChEgoKqwoKCgoKClIKCgoKUlILKwoKUgoKCgoKUgoKAgsjSgoKCgpSCAAsQooLmooKCgoKCgoKUgpSCtAALBKKCgpSCgoKUgpSClIKUgpSCtAAUBKKCgpSCgoKCgpSClIKUgpSAkqSCpoKCgIKkgoKUgILEpAARAoKCgpSCkqaCgpSmgoKAksiCgJLIgoKAksiCgoCStoKCgoKCgpSCgpSCgoCSpIKUgJLIgoKCgpSClIKmgoKClIKUgoK6gMKCgqaCgpKCtKS2goKCgpSCAAkUggALGAAXMIKi+IIACRQAFSyCogAQCIKIgoKCgoKClIKCgoKCgoKCgqaCgpS4goKCgoKUgpSCpoKClIKUgoK4ooKCgpSCgpSCgrgACwKigoKCgpSCgriCgoKUlIKClIKUkoKClIK0qJDigoKCgoKClIKClIKClILegAAIAoKCgoKUgoKUgoKCgpSCgpSCuNyCgpSCgpSCuOKCgoKCuIKClIKCgpSCgoKClJSCAAgGooKCgoKCgpSClIKUgpSClIKClIKCogAJEuKCgoKUoqTigoLWgoKCgoKCgoKCgpSCpoKCgpSCgoKCgqaCygARAoKCgoKUgoKmgoKCloKClIKClIKMggAKFoKmgoKm4oKCoqSCgoKSgoKCuIKCgpSCgoKCgpSClIKCpoKCggAkEoKCqpCCgqaCgoKClJSClIKClIKmgoKClIK4ggAJBIKClIKCyoKyuIKCgoKUgriCggALCKKCgoKCgpSCtKSC1IKCgoK4goKCuILUgpKCgpSCgoKClIKmgoIAAxoACQL0goKmgoKCloKClKaC5KKCpoKCgoKWgoKUkoKUgrSqsgAJBIKSlIKCuoDSgoKCpgAEEIKCgoKUgpSCgpSCygAIAqKCgoKCgoKClIKCgqamkoKClIKCgoKUsgAIBKKCgpSCgpSClIKUgpSCgoKCgpSCgqa0AAoEooKCpoKCgpSCgpSClIKUgpSCgoKCgpSCgqa2AAgEgoIAFTSyooKC7gAKAqKqgoKCgpSCgoKUlJSCgoIACRSCgoKmlIKUgpSCgoKCgoKClIKUgpSmgoKCgqaClIKUgoKCgpSClIKClKa6gOKigpIACBKQpIKCgoKUgpSCgpSClIKUgoKUtKiQgqKClJKClJSCgoKCgpSCgoK0poCCgoKmgoKCgpSClIK48oKCkoKCuIKClIKCgqaC1raC1raC7oKC9oIAGDiCgoKUgoKUgoIACRKCyuiSyq6wABgCopiCggAHEIKUgpSmkpSUgoLugpSmgoKCgoKCgpSSgpSCtKiiAAkEooKCgoKCgILspLS0tLS2lIKogoKCgoKClJKCtKSCAA0EooKCgoKCgpSCgsqUgoKCgoKCgpSypoAADgK0goKClJKCgpaSgoKCtoKCgpSCxoKCgpSCtIKCgpSCtIK2goKClIKCgoKClJKClIK0poDCooKCgpSmkoKUgqTugoK03IKCgrTcgoK0AAoSgqaCuIKClIKCgoKCgoKCgoKClJKCtAAIBpIAHkaCgoKClIKCgpSCgtziggAlVJSSgqaCsqKCpoKCgqaikpSmpoKCgqYADBqCgqaCgqqQ4oKCgpSCgpSCyoKClJSCgoKUgoKUgoKC3IKCgpSCuoD0goKCgpSClIKUgoKUlIKCgoKUgoKUgoKUgpSAggAMEqKClNq4gsSCgpSCgpSCgoLcggAJBoKmgoKClJSmqpCygoKmlIKygoKClIKUpIKCgqaUgoKCgpSAgqSAgqSCtPyCAAsQopSCgqaClJKCuIKCgrSmgIKCgsqCgoKClIKClAAMGoKUpIKkgpSC+KKClKbCgoKSgoK4goKCgsKCgoKCgpSipPi4koKCgoI=", "192-205:1;236-240:2;288-290:1;294-302:2;358-360:1;397-399:1;408-433:1;439-439:2;482-490:1;493-498:2;521-538:1;523-526:1.1;546-549:1;590-595:1;707-716:1;720-722:2;768-774:1;817-818:1;821-827:2;853-855:1;904-908:1;921-921:1;955-957:1;962-964:2;976-978:1;984-986:2;1007-1007:1;1019-1023:1;1054-1070:1;1189-1195:1;1206-1226:1;1303-1305:1;1313-1313:2;1349-1352:1;1372-1374:1;1393-1398:1;1498-1498:1;1516-1533:1;1534-1536:2;1540-1552:3;1579-1602:1;1606-1608:2;1628-1641:1;1661-1665:1;1675-1707:2;1732-1738:1;1740-1795:2;1922-1925:1;1929-1932:2;1940-1948:3;1944-1946:3.1;1983-1990:1;2035-2051:1;2100-2102:1;2119-2128:1;2137-2139:1;2144-2178:2;2204-2208:1;2220-2222:1;2275-2280:1;2286-2295:2", "", "1555=Error/1/1/1;1900=NewReader/1/1/3,NopCloser/1/1/3;2781=StatusText/1/1/2", "664:21,1295:21,1412:21,1744:21,2006:21,2086:21,2140:21,2235:21,2349:21", "1696:21,1697:13,2807:21,2808:13")]
[assembly: GoPositionMap("net/http/clientconn_test.go", "clientconn_test.cs", "ABgkgqKCpoKCgpTIgoKCgpSCgpSmpoKmgoKmgoKCgpSipKKCpIKCgpSSlIKogIKCgsqCpKSCgriCgpSAkqSAktqAgsyQgoKCuLyCgoKygoKCgpS0pJSAkqSAksqAgoKCyoKCgpSmkgAODKIABRLcgoKCgu6CgoKU3IKSgoKCpoKmAAcQgoKClIKmgoIACAqSsoKCooK4goKClIKUgoCCpIKAkraCyoKmgoCSpICSpIKmgJKkgJKkgriCgsYABRCQgoKCgpKClILKgoKCgpKCgoKCgpS4goCSpICStoKCgpI=", "20-22:1;69-69:1;76-78:2;86-90:1;125-129:1;139-148:2;163-167:1;193-195:1;199-206:2;210-217:3;221-232:4;223-229:4.1;233-235:5;241-249:6;250-254:7;258-323:8;259-322:8.1;262-266:8.1.1;270-272:8.1.2;334-337:1;338-342:2;348-357:3", "", "", "39:21,316:33")]
[assembly: GoPositionMap("net/http/clientserver_test.go", "clientserver_test.cs", "AEFsgqSklNwABx4ADAKCgoKClIKC2Mi0xLaAgqSCgoKClIKUAAQQ0pKCAA4egoKmwoKClJKCgpTWgoKUtoKmgoK4AAEgABECgpTclICCgoK4krq2goKCpoKUtLTEyIKmpLS0grSCgrSkgoKCgqaCpoKCgqaSlO6CggAICJKCkpKmkoIADwiiiqKCgoKikoKClIKCpIK0grSCpIKUgrjCooKCgoKCpoKClJKClIKClIKUgrQADhyCgpSmooKCkoKkgoKClIKCgqaAgoK2goKUgoKCpoKUgIKCgsSkooKCgoKCAAgS1KKClJSUgsqClIKUgrqSAAoOgqbKgqaCgtyCtoKClIKCAAgIgoLIgoKCyIKCgsiCgoKCyIKmgpTKgqaClMqClIKmgoKClIKUggAFGNCCooKSgoKCgoKCgqaCuIKClKSCgrQACgiilIKUgoKCyoKktoKCAAUSoIKigoKCgqaSgoKUgpSCgpSCtOSClIIACAqCpoIACgaC5oKUgrySAAUQkOKigoKigoKCgpSUgoKUgoKUkriCgoKUlJSCgoKUgrSmgAAKAoKCgoKUgpSClILugpS4lriCgoKUgrqSkqaCkgAZCIKCgoKUgoLugoKmgoKmpoLcgpSCpoKCpoDKtoKmgMrKgMKCgoKUgoKUgoKCuAAOAqKCgoKCgpSygoKClIKUoqSygsqUopKUgoKCgpSCkoKUgrSk0oKCkqaCgqYAEy6CgoKClIKCgpSClILKAA0CgoKmgoKUpgAHEtjY2MiCgoKCgoKUgoKCgpSCyoKCgpLKgoKSgoKmgoKUlNyCgpSUgoKCgpSClIKUgpTWpIKClJSClIKUgrqSlKKClKSkgraCgoKUgoCSpoCCpKSUgoKCgoLSgoKmgoKCgqaSgoKUoqSClIKCgoKUgriSgoKClJSUlKIACAaSkoCSgPaCgoKCuIKigoKAkoKClIKUgqaCtKTa0oKClJQAChyCgpKClIKCgoKCgpSCpOiCgoIAEAqCkoCSgICSgAALBqKUgpSCgrKClAAICJSUgoKUgpKCgpSClKKCgsSUkoKCgpSUgoKUlIKClJS0/tKCggALCJKUgoKCpoKUgoKClIKUgt6A0oKClJKCgpSCgoKUlIKCgoIACBKCggAJCoKCpoKigriUgoKUgoKCgpSSgrSkAAwCgoKCgoKClIKClIKUgoKUgIKkgILIgtSCooKClIKClJKipIKCgoKUgrjSgoKygpKCgoKClMqmopKCgpSClIK8opKmggAJBKKCooKCkoKCgpSCgoKikpSCgpSSgoKUkrimlIKCgpSCtKSC9KKCgriCgpSCgpKmgoKCgqKCgpKCgpSmgoKUkoKCgpSClLSktILoxgAKBpKUgqaCgoKmgpTKAAoCooKCgpKUgoKClIKCgpSSgoKUgJLEpAALAqKCgoKUgoKClJSClKaClIKmgoLKgpSCgsqCtJSClLSCtraUppSCgpSkgoKmgoK0", "95-97:1;110-119:2;115-117:2.1;127-131:1;128-130:1.1;174-176:1;214-218:1;215-217:1.1;278-280:2;296-300:1;297-299:1.1;301-305:2;302-304:2.1;313-318:1;348-352:1;482-483:1;490-492:1;499-504:1;509-509:1;517-519:1;523-525:1;529-532:1;536-539:1;543-547:1;553-558:1;565-570:1;576-579:1;580-592:2;605-619:1;637-645:1;652-652:1;654-659:2;668-674:1;694-697:1;702-702:1;706-706:1;710-710:1;715-718:1;719-721:2;722-726:3;736-742:1;780-797:1;801-803:2;805-807:3;825-827:1;830-832:1;837-853:1;907-909:1;925-954:1;930-940:1.1;942-952:1.2;974-976:1;1028-1030:1;1032-1037:2;1044-1044:3;1048-1048:4;1052-1052:5;1056-1056:6;1060-1060:7;1086-1090:1;1087-1089:1.1;1095-1099:1;1161-1163:1;1171-1180:2;1178-1178:2.1;1194-1213:3;1246-1249:1;1247-1247:1.1;1248-1248:1.2;1252-1257:1;1260-1274:2;1263-1263:2.1;1287-1289:1;1308-1311:2;1336-1340:1;1337-1337:1.1;1338-1338:1.2;1339-1339:1.3;1350-1359:1;1359-1361:2;1375-1379:3;1382-1403:4;1420-1425:1;1426-1441:2;1448-1450:1;1451-1462:2;1490-1492:1;1515-1521:1;1545-1556:1;1572-1590:1;1574-1588:1.1;1606-1608:1;1615-1628:1;1628-1630:2;1664-1668:1;1676-1678:2;1684-1694:3;1727-1730:1;1731-1736:2;1737-1742:3;1749-1758:1;1778-1792:1;1794-1806:2;1808-1816:3;1820-1838:4", "", "587=NewReader/1/1/1,NopCloser/1/1/1;605=run/1/1/4;617=run/1/1/5;626=run/1/1/8;647=run/1/1/2;658=run/1/1/2;665=run/1/1/3;673=run/1/1/3;681=run/1/1/4;690=run/1/1/8;702=run/1/1/8;714=run/1/1/18;790=run/1/1/10;804=run/1/1/10;859=run/1/1/5;896=run/1/1/13;997=Get/1/2/2,Get/2/2/3;1005=NewReader/1/1/4;1323=Invoke/1/1/6;1830=run/1/1/23;2246=run/1/1/17", "192:21,461:21,768:21,934:21,945:21,1227:21,1519:33,1725:21,1755:21,1932:21,2005:25,2035:25,2098:33,2119:21,2187:23,2200:21,2298:21,2406:21", "2037:25,2038:17")]
[assembly: GoPositionMap("net/http/csrf_test.go", "csrf_test.cs", "ABAgkoKCgqaCABAGgoKUABAusqKCgpSCpoKUggAQDIKCgoKUlAAGGLKigoKCpoKUggAeDIKCgoKCgoKCgpQAGDyyooKCgqaClIIACwyClJSClIKUgqaCgpSUgpSCpoKmlIKUgqaUgpSClIIACQiClAAOJrKigoIACwyCgpSCgpSClIKmgoLigoKCgrS2goKClIKUggAMCKKCgoKUgqQACCCysoKClIKUgpSCgoKUkoK0xA==", "55-70:1;95-108:1;154-167:1;186-189:1;248-253:1;274-281:1;323-343:1", "", "", "352:25,430:33")]
[assembly: GoPositionMap("net/http/example_filesystem_test.go", "example_filesystem_test.cs", "ABAksoKCgqYACRjigoKCpoKUAAkYsoKmgoKUpoKCgg==")]
[assembly: GoPositionMap("net/http/example_test.go", "example_test.cs", "AB0kgqKCgoKUgoKCppKCgoKCgpSCovaCgoKUgoKClIKU5pQACAa4prja1IKCgqaCgpQAGQyigsqClIKUgoKCuIKUgpKCgqaUlKaAlLYACgaCgriCggAIBqaCpoIACgaCgpSCpoKU1oKCAAkIgqamlKaCyoKUpoKmgoKUgoKClOaClIKmAAgS", "19-41:1;86-94:1;101-116:1;123-134:1;145-147:1;158-160:1;167-169:1;170-172:2;181-183:1;229-231:1", "", "330=NewCrossOriginProtection/1/1/6,Handler/1/1/6", "46:25")]
[assembly: GoPositionMap("net/http/fs_test.go", "fs_test.cs", "ADpcABk6AA4CgoKUlJSCgqiSgoLuAAgSgoKCyoKCgpSSuIKCgoKUgoKUgpSCgoKUgoKUgoKCgoKUgqaCgoKClIKClIKClICCgqSCgoKCgpSCkpSCgoKUgoKmgoIADAyCAAsegoKCgpSCgoIACAySgoKCgoKC+pK4goKCgoKClIKC+gAFEMKClIKCgpSCkgAIEoIADAaigoKSgqTOgoKCgoKCxqQADAKCgoIACBaCgoLKpoKCgoKClIKClIKClIKUuAAIAqKCgoIAECaUgoKUpIKClIKCtKSCgoK4AAsCgoKAgqSCkoKClIKClIKUgIKkgIIAFAiCgpSCgpSygoKUkoKClIK0koKCgoKCuIKC5oKigoKClLKSgoKmAAgCgoKCpMaklJKCgpSClJSCgqbigoKUgoKUgoKCuIKCgpSCgpSCgrqAwoKCgpSCgpSCgryQgoKCggAIEpSCgpSCgryQ8oKCgoKCAAgSlIKClIKCgpSCgoKUgpSAkraAgqSClIK4AAoCgoKCgpSCtIKkgoKUgoKClIKClIKU3MKigoKkgoKClIKClIKUtKSyopSCgpSSgoKUgoKCgpSCtKSCgpTKgoKUgoLMgoKCuIKClIKCAA0egoKCgoKCgpSmggAJFIKCgoKUlIKClIKmgpTcgoKCgpSClKYACAKCgoKCgsoACBKUgoKUgoKUgpSUgoKmgpSCgoKUgpSmlIKClIKUpoKCgpSmAB4CogAAEJKCgpSClJYAAB4A1QGsA4KCgoKClJKUlJSCpu6CgpSCpoKCgpSCgoKUgJKkgJKkgJLoAAoGkoKCgoKCgoLs2IL0gpKClIIAFQiCgpQABxCCkoKCgoKUgoKCgoKClIKClIKCgoKUABEOsoKCgpSCpoKClIKClLaCpoKUgpSUgoKCgoKCgqaCgpSCgpSmgpSCgoK0pKKCgpSCgpQACgrCgpSCgoKClIKCgpSCgoK0ppKSgpSCABAKgpSSgpSCgqaCgoKClIK4goKUgtyCgqaCAAgGggAEEoKCgoKCgpSCAAgSgoKUlAAMCoIACRyCgoLOovSClAAKHrKigoKUgoKClICSpIKCgpSAkuyClIKUgoLuAAkUgoKCgpSCgoKUgoKUgpSSAAgKooKCpoKkgoKUgoKUgpSipKKCgqaSlKSCgpSCgpSClKKkAA8cgoKmsoKCkqKSpIKClIKClIKUov6CpoKCgrqA4oKigqSCgpSSlKKkkqaSuIKClIIAFgaigpSCkoKCgoKCgoKUpIKClJSCgoKmgpSCgpSUgJKkgJKkgJKkgJKkgJKkgJKkgJKkgJKkgJKkgJLE", "77-79:1;305-308:1;431-442:1;459-472:1;488-495:1;504-513:1;514-523:2;531-533:1;547-549:1;564-566:1;581-593:1;608-622:1;668-686:1;929-938:1;1248-1255:1;1249-1251:1.1;1252-1254:1.2;1270-1276:1;1392-1394:1;1404-1411:1;1405-1407:1.1;1406-1406:1.1.1;1408-1410:1.2;1409-1409:1.2.1;1426-1446:1;1427-1445:1.1;1543-1564:1;1646-1648:1;1684-1689:1;1725-1737:1;1739-1741:2;1743-1745:3;1749-1751:1;1752-1754:2;1761-1770:1;1788-1793:2", "", "416=Unix/1/1/2,UTC/1/1/2;1173=UTC/1/7/10,Format/1/7/10,UTC/2/7/18,Format/2/7/18,UTC/3/7/28,Format/3/7/28,Date/1/3/121,Date/2/3/133,Date/3/3/144,NewReader/1/1/155,Unix/1/1/156,UTC/4/7/198,Format/4/7/198,UTC/5/7/202,Format/5/7/202,Add/1/1/208,UTC/6/7/208,Format/6/7/208,UTC/7/7/211,Format/7/7/211;1406=Base/1/1/1;1528=New/1/1/2;1748=Is/1/1/1;1757=Is/1/1/1", "485:21,585:25,625:25,918:21,1395:29,1603:21,2045:29")]
[assembly: GoPositionMap("net/http/main_test.go", "main_test.cs", "ABUolIKCgoKUABgGooKCgoKCAAIYlJSCqJKmpoKCgoKCgoKUgqaUgoKU7LKClIL4goKClIKmyoKCgpTcAAgSlIIABxCCgoKCgoKmgoK4lKqigoKCgoKC", "", "", "63=Contains/1/10/1,Contains/2/10/2,Contains/3/10/3,Contains/4/10/4,Contains/5/10/5,Contains/6/10/6,Contains/7/10/8,Contains/8/10/9,Contains/9/10/10,Contains/10/10/11")]
[assembly: GoPositionMap("net/http/netconn_test.go", "netconn_test.cs", "ABUogtyCAA4iggAKDIKClLjSgpSCgoKCgoKCgoKU1tKCgoKUgoLW0oKCgtaCAAIQ0oKCgoKCgoKCAA8okoKUqqKClKjygpTo4oKUgqbYsoKmgoKCgpSCgpSCgoKUqJKokqiSgoKokoKokoKqogAYOIIABxCCqJIAEhKSgoKUtLS0yta0pNaCxKSkyta0pNaCxKSkyNKCgtbigpSCgpTm0oKCoqSigoKCgqam4oKUgoKUgoKUAA4Y4oKCgpTY0oKmgqaClJSUgpSCgpSUgqaigoKCorI=", "434-439:1", "", "23=MustParseAddrPort/1/1/3")]
[assembly: GoPositionMap("net/http/request_test.go", "request_test.cs", "ACc+goKCgvyigpSCgpSCggAUCoKUlIKUgpSClIKUgpSClIKUgpSClIL6koKUgoKClIIACgqCAAoYpMqClLS0AAoOgoK4goKClIIACQqCAAUSgsqCgoKUlIIAGQySAAAi3O6CgqaCgqbugqa4gviCyoKCpoKCggAQCpKOyoKClIK+stSCgriCgqaCgoKClIKUgpSCgpSCgoKUgpK4woKCpIK0tMiCgoKUgoKCAA0IgoKCkvjYgoCCpJKCgIKkotjCgoKClJKipMaCkoKmoqSmggAKCJKCgoKUgoLqkoKClIKC7KKCgIKkgIL8woKAgqSSgILE2KKCgIKkgIIACQgAKmCCgoKCgqaClKaC+gAOIoKCgoKClIIADwqCgoKUgoKUgoKCpoKCgoIAEgiCgoKClAANIoKCgpSCAAkKABkygoKCggARJAAFEoKCgoKCgsqCgoKUgoIACAoADyaCgoKCgoIAChaCgqaCgqaCgoKC3IIACQiCgoKClIKCgtyC+IKCgoKUgoLcggAKCIKCgpSClIIACBKClIKCpoKUgoK4goKCgoKCggAHEIIABxCygoKUpoKCuIIACAyigoKCgoKCgoKUgoKUgpSCpgAEEIKCggAPDqKCAESSAYKUlIKmggAJCoKCgriCgoIACwyigoK4lJSClIIACQyigpSCuICSpICSyICSpICSyoCCgqSCgpSCuIIACwqSzoKCgoKUgoKUgoKUgoKUgoKUgoKUggAICoKCgpSClIIACAiCgoKClIKCABEGooCSpICSpICCtoKCpoKSgoKSgpSCuKLUgoKClIKUgoKClIKUAAgKogAKHoKClIKCgriClIKUggAKCoIAKVyykoKClIKmlIKCgpQAE0iigoKCgoKCgoKCAAsYgoKCABEGlAAMGJQADA6UAA4OlAAKEpSqgqaCpoLmooKCppKCpoKmorSkoqKCgoKmgrSklIKUgoKmgoKCppSCgoKmguiCgriEkoCSpIKAkgAKCKIAJlCCkoKCgqaCpoKSgoKUtOSigoLKgpSCgriCkoKClKIAFgS0koKCgoKCgoKkAA0kgoKUgoKUgoCSpICS1g==", "123-138:1;300-308:1;338-348:1;402-406:1;601-605:1;894-914:1;1088-1088:1;1180-1184:1;1309-1327:1;1321-1324:1.1;1457-1460:1;1466-1468:2;1472-1482:1;1573-1583:1;1596-1610:1;1622-1622:1", "", "56=NewReader/1/1/1;86=NewReader/1/1/1;129=NewReader/1/1/1;163=NewReader/1/1/3,NopCloser/1/1/3;220=NopCloser/1/1/3;262=NewReader/1/1/3,NopCloser/1/1/3;310=NopCloser/1/1/3;343=NewReader/1/1/3,NopCloser/1/1/3;642=Error/1/2/7,Error/2/2/8;791=NewReader/1/6/4,NewBuffer/1/1/5,NewReader/2/6/6,NewReader/3/6/7,NewReader/4/6/13,NewReader/5/6/14,NewSectionReader/1/2/14,NewReader/6/6/15,NewSectionReader/2/2/15,Invoke/1/1/15;902=EncodeToString/1/7/4,EncodeToString/2/7/7,EncodeToString/3/7/8,EncodeToString/4/7/10,EncodeToString/5/7/11,EncodeToString/6/7/12,EncodeToString/7/7/13;1088=NewReader/1/1/2,NopCloser/1/1/2;1361=NewReader/1/2/3,NewReader/2/2/4,NewBuffer/1/1/5", "478:21,505:21,607:21,1474:21,1478:21,1808:25", "519:21,520:13,1783:21,1784:13")]
[assembly: GoPositionMap("net/http/responsecontroller_test.go", "responsecontroller_test.cs", "ABUkwqKCkoKCgoKUgqaCgpSkgoKCgqaCgrSkAAgCgoKCgoKCgoKUgpSCgpSUgoKUkriClKKCgoKClIKmgoLcgpSCgriCgpSSgoK0pIKUooKCooKCgpSCgpSCpoKCgpSSgoKUgoK0pILUooKCwoKCgoKCgoKUgoKUgoKUuIKClIKCtKSCgoLSgpKC5taCtqKSgoKCxKSC9KKCgoKClIKClJSCgoKUkoKClKLsgqaCooKCgoKUgpSCpoKClIKypIKUooKCyoKmgoKCgoKClIKmgoKClJKCgpSCgpSCpqKkgoKCgpSUgoKU", "21-30:1;55-70:1;84-109:1;128-140:1;164-194:1;199-214:2;227-237:1;261-273:1;286-308:1;331-337:1", "", "", "48:21,142:21,182:21,213:29,258:29,287:25,324:21,366:21,407:21")]
[assembly: GoPositionMap("net/http/serve_test.go", "serve_test.cs", "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", "190-193:1;195-197:2;315-317:1;411-415:1;479-481:1;541-543:1;549-551:2;653-655:1;669-671:1;684-684:1;739-741:1;746-748:1;748-751:2;824-825:1;828-836:2;839-844:3;846-871:4;847-870:4.1;881-886:1;886-888:2;908-914:1;914-918:2;921-926:3;950-965:1;965-968:2;993-997:1;997-1000:2;1020-1025:3;1057-1060:1;1060-1063:2;1089-1089:1;1090-1092:2;1135-1139:1;1136-1138:1.1;1144-1152:1;1152-1154:2;1195-1199:1;1196-1198:1.1;1204-1212:1;1242-1258:1;1258-1260:2;1264-1272:3;1307-1322:1;1439-1441:1;1446-1448:1;1454-1456:1;1460-1462:1;1466-1469:1;1493-1496:1;1521-1523:1;1572-1574:1;1574-1579:2;1585-1600:3;1639-1650:1;1679-1681:1;1706-1706:1;1707-1710:2;1733-1740:1;1740-1742:2;1795-1803:1;1807-1817:2;1845-1847:1;1848-1848:2;1855-1855:3;1883-1885:1;1885-1888:2;1970-1972:1;1988-1990:1;2010-2012:1;2018-2018:2;2100-2110:1;2112-2187:2;2127-2169:2.1;2210-2214:1;2217-2230:2;2253-2262:1;2408-2412:1;2417-2424:2;2468-2474:1;2503-2509:1;2631-2637:1;2665-2669:1;2720-2729:1;2745-2753:2;2747-2747:2.1;2762-2764:1;2779-2791:2;2781-2781:2.1;2801-2806:1;2859-2861:1;2885-2899:1;2925-2927:1;2945-2947:1;2948-2950:2;3084-3093:1;3121-3123:1;3127-3129:1;3134-3136:1;3151-3160:1;3164-3166:2;3170-3178:3;3206-3219:1;3219-3221:2;3233-3235:1;3239-3241:1;3245-3248:1;3261-3264:1;3280-3301:2;3317-3319:1;3395-3411:1;3449-3449:1;3476-3481:1;3507-3558:1;3517-3519:1.1;3530-3533:1.2;3534-3540:1.3;3563-3567:1;3587-3587:1;3619-3624:1;3630-3638:2;3661-3670:1;3677-3686:2;3716-3721:1;3744-3767:1;3790-3813:1;3835-3837:1;3887-3889:1;3889-3891:2;3926-3928:1;3929-3937:2;3941-3946:3;3947-3958:4;3962-3965:5;3966-3971:6;3975-3979:7;3980-3988:8;3992-3997:9;3998-4009:10;4013-4016:11;4017-4022:12;4026-4029:13;4030-4035:14;4039-4040:15;4041-4046:16;4050-4052:17;4053-4058:18;4062-4065:19;4066-4074:20;4117-4117:1;4135-4148:1;4141-4147:1.1;4166-4177:1;4194-4194:1;4246-4265:1;4249-4252:1.1;4288-4296:1;4319-4323:1;4356-4426:1;4363-4378:1.1;4381-4384:1.2;4387-4416:1.3;4403-4403:1.3.1;4443-4449:1;4444-4447:1.1;4453-4468:2;4482-4485:1;4500-4502:1;4503-4506:2;4507-4511:3;4512-4517:4;4532-4547:5;4549-4551:6;4551-4573:7;4553-4572:7.1;4574-4577:8;4581-4601:9;4603-4606:10;4608-4611:11;4613-4615:12;4617-4619:13;4621-4627:14;4629-4639:15;4641-4657:16;4664-4665:1;4665-4667:2;4682-4684:1;4689-4708:2;4719-4723:1;4724-4724:2;4768-4779:1;4809-4813:1;4813-4815:2;4818-4827:3;4858-4861:1;4904-4906:1;4933-4937:1;4968-4974:1;4997-5000:1;5011-5014:1;5015-5026:2;5088-5088:1;5108-5130:1;5192-5192:1;5212-5220:1;5244-5248:1;5264-5270:1;5283-5285:1;5303-5306:1;5318-5323:1;5339-5341:1;5366-5370:1;5367-5369:1.1;5376-5378:1;5381-5400:2;5441-5444:1;5474-5477:1;5514-5520:1;5539-5542:2;5556-5566:3;5583-5586:1;5620-5623:1;5674-5678:1;5703-5706:1;5722-5726:1;5731-5734:1;5739-5742:1;5747-5749:1;5763-5766:1;5780-5786:1;5805-5808:1;5832-5832:1;5833-5833:2;5847-5910:1;5848-5851:1.1;5851-5854:1.2;5861-5875:1.3;5932-5934:1;5939-5939:2;5959-5967:3;5982-6016:1;5984-5990:1.1;5986-5988:1.1.1;6018-6020:2;6019-6019:2.1;6044-6046:1;6046-6051:2;6058-6060:3;6107-6107:1;6112-6120:2;6124-6127:3;6158-6196:1;6159-6166:1.1;6166-6169:1.2;6174-6179:1.3;6212-6250:1;6213-6216:1.1;6292-6317:1;6302-6305:1.1;6336-6359:1;6390-6405:1;6436-6460:1;6544-6547:1;6578-6580:1;6581-6583:2;6601-6603:1;6603-6616:2;6604-6609:2.1;6610-6615:2.2;6637-6639:1;6639-6646:2;6640-6645:2.1;6670-6672:1;6726-6732:1;6738-6744:2;6749-6755:3;6771-6796:4;6772-6777:4.1;6834-6902:1;6839-6847:1.1;6924-6930:1;6941-6954:1;6954-6956:2;7005-7026:1;7026-7029:2;7080-7080:1;7094-7105:2;7096-7099:2.1;7100-7103:2.2;7109-7124:1;7132-7134:2;7158-7162:1;7159-7161:1.1;7177-7253:1;7185-7189:1.1;7201-7208:1.2;7257-7267:1;7276-7279:1;7302-7315:1;7352-7355:1;7353-7353:1.1;7354-7354:1.2;7361-7364:1;7362-7362:1.1;7363-7363:1.2;7368-7376:1;7413-7416:1;7429-7431:2;7448-7451:1;7493-7498:1;7498-7500:2;7524-7530:1;7525-7529:1.1;7530-7532:2;7551-7558:1;7552-7556:1.1;7558-7560:2;7583-7617:1;7585-7591:1.1;7656-7656:1;7669-7671:2;7672-7675:3;7690-7737:1;7694-7697:1.1;7741-7781:1;7745-7749:1.1", "", "209=NewReader/1/1/1;215=New/1/1/1;378=serve/1/4/4,serve/2/4/5,serve/3/4/6,serve/4/4/7;2015=ParseIP/1/1/1;2028=ParseIP/1/1/1;2574=expectTest/1/8/2,expectTest/2/8/3,expectTest/3/8/6,expectTest/4/8/10,expectTest/5/8/12,expectTest/6/8/15,expectTest/7/8/18,expectTest/8/8/20;2668=NopCloser/1/1/5;2937=connectionHeader/1/1/2;2943=connectionHeader/1/1/3;4309=Bytes/1/1/1;4608=NewReader/1/1/1;5117=New/1/1/2;5136=NewReader/1/1/1;5172=NewReader/1/1/1;5866=NewReader/1/1/2;6077=NewReader/1/1/1;6114=NewReader/1/1/1;6156=NewReader/1/1/1;6184=Repeat/1/1/4;6386=Repeat/1/1/12;6743=Sprintf/1/2/1,Sprintf/2/2/2,Environ/1/1/3;7972=Repeat/1/1/1;8392=Sprintf/1/1/12;8885=NewReader/1/1/1", "220:15,331:21,971:25,972:29,1029:33,1038:31,1128:25,1180:25,1248:25,1292:25,1696:21,1732:21,1987:25,2177:21,2194:21,2281:21,2322:21,2364:21,2531:21,2540:21,2640:25,2752:25,2755:29,2795:21,2970:21,3031:29,3064:29,3239:23,3435:29,3547:21,3629:21,3884:21,3896:33,3965:25,3973:25,4120:25,4301:21,4558:21,4620:29,4714:29,4746:21,4778:21,4839:21,5160:21,5190:21,5511:21,5524:29,5703:29,5798:21,5835:29,5938:21,5982:25,6044:21,6084:21,6120:21,6163:21,6190:21,6217:25,6300:19,6337:29,6363:21,6427:19,6504:21,7022:21,7059:21,7124:21,7239:33,7279:25,7303:21,7456:21,7472:21,7670:25,7750:29,7807:29,7818:29,7844:21,7878:29,7886:29,7903:21,7937:29,7945:29,7971:21,8002:25,8061:21,8340:29,8414:29,8507:21,8556:29,8576:21,8655:29,8676:21,9042:21,9200:21,9278:21,9502:25,9504:23,9508:25,9565:25,9567:23,9571:25", "496:21,497:13,3379:29,3380:21,3424:29,3425:21,4372:25,4373:17,5426:25,5427:17,5675:21,5676:13")]
[assembly: GoPositionMap("net/http/sniff_test.go", "sniff_test.cs", "ABkmADp+goKCgsqCooKCgoKCpqSCgoKC7oKClIKUgoKCgpS0qJCCgoKCpoKCpoKCgpTugoKUgpSCgqYAEgKCqr6Cgu6CgoKCAAYQhIKC7oKCgu6CgviSlIKClIKUkpSCgoKClM7CgoKCgoKClIKmgoKClIKUgg==", "93-100:1;135-138:1;181-187:1;190-199:2;202-210:3;213-220:4;223-229:5;231-252:6;259-269:1")]
[assembly: GoPositionMap("net/http/transport_dial_test.go", "transport_dial_test.cs", "ABoskoKmgoKCgqaCgrqSgqaCgoK4goKCvKKCgtyCgoKmgqaCgoKUgoKmgoKCgoKCgoLuspSCgpTcgoKCpoKmpoKUgrqSgsiCgoKmgoKUgoK8ooKmgoKCyoKCgtyCgoKUupKCgriCgoKCgsySgqaCgoKmgoKCgpS6koKCuIKCgqamgoKCgrqSgriCgoKCgqaCgoKCgvqSgoK4goKCpoKCgqaCgoKmgoKCpoKCgoKC7KKClIK4uIKCgqaCgoK4gqaCgoIAKViigqaS7oKClLSCxoKmgpK4lMSCgqaUoqSigoKUuIKUgoKUturSgoKClMS2koKC3IKSgpSipIKClKaCgoKCgpSosoKCgpSClIKUgIKkgJLKkoKCgpSCvKKUgoKUgqaClIKCgoKo4oKCgoKClIKCgoKC2JKCgoKmgoKmgtaCpA==", "23-37:1;42-57:1;63-100:1;64-68:1.1;109-137:1;110-112:1.1;112-116:1.2;142-160:1;143-143:1.1;166-193:1;198-211:1;199-201:1.1;216-233:1;238-258:1;239-241:1.1;263-282:1;287-323:1;288-290:1.1;329-362:1;332-336:1.1;336-338:1.2;410-439:1;433-435:1.1;440-448:2;448-470:3;449-451:3.1;452-469:3.2;497-500:1;501-517:2;503-509:2.1")]
[assembly: GoPositionMap("net/http/transport_test.go", "transport_test.cs", "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", "109-117:1;144-146:1;178-188:1;214-236:1;270-296:1;360-390:1;371-378:1.1;435-445:1;486-499:1;509-526:2;510-516:2.1;570-575:1;580-584:2;591-608:3;594-596:3.1;640-645:1;655-662:2;664-687:3;666-670:3.1;671-673:3.2;692-695:4;745-750:1;789-791:1;796-812:2;820-829:3;843-867:1;844-850:1.1;900-908:1;924-938:2;949-955:1;984-991:1;997-997:2;1035-1050:1;1101-1128:1;1118-1120:1.1;1219-1227:1;1233-1235:2;1243-1250:3;1336-1397:1;1406-1408:2;1410-1432:3;1447-1549:1;1449-1451:1.1;1453-1492:1.2;1487-1490:1.2.1;1566-1598:1;1602-1604:2;1635-1637:1;1639-1681:2;1676-1679:2.1;1696-1708:3;1704-1706:3.1;1711-1720:4;1746-1756:1;1748-1754:1.1;1763-1793:2;1797-1799:3;1824-1826:1;1827-1829:2;1872-1876:1;1903-1906:1;1930-1933:1;1974-1979:1;1988-1997:2;2045-2046:1;2103-2103:1;2123-2131:1;2175-2175:1;2199-2202:1;2209-2229:2;2213-2226:2.1;2257-2260:1;2265-2273:2;2285-2290:1;2322-2351:1;2327-2329:1.1;2358-2360:1;2388-2390:1;2401-2401:2;2409-2433:3;2444-2446:1;2451-2461:2;2483-2485:1;2486-2489:2;2494-2504:3;2555-2558:1;2559-2563:2;2636-2651:1;2639-2641:1.1;2642-2644:1.2;2645-2649:1.3;2657-2657:1;2660-2663:2;2664-2666:3;2667-2671:4;2680-2682:1;2683-2685:2;2686-2690:3;2695-2707:1;2697-2699:1.1;2701-2703:1.2;2704-2706:1.3;2720-2724:1;2752-2761:2;2769-2771:1;2780-2783:2;2786-2797:3;2801-2803:1;2807-2809:1;2828-2835:1;2841-2846:2;2858-2860:3;2886-2890:1;2919-2928:2;2933-2940:1;2934-2936:1.1;2937-2939:1.2;2944-2946:1;2968-2972:1;2978-2981:2;3009-3018:1;3115-3119:1;3120-3122:2;3128-3139:3;3153-3175:4;3176-3178:5;3196-3196:1;3197-3197:2;3199-3243:3;3246-3260:4;3252-3256:4.1;3266-3282:5;3298-3303:1;3310-3313:2;3330-3336:1;3365-3371:1;3377-3380:2;3399-3405:1;3430-3434:1;3511-3518:1;3526-3533:1;3545-3549:1;3553-3553:2;3558-3560:3;3600-3602:1;3628-3636:1;3639-3641:2;3674-3683:1;3757-3759:1;3767-3777:2;3778-3781:3;3783-3799:4;3795-3798:4.1;3825-3834:1;3878-3884:1;3903-3905:2;3915-3917:3;3926-3928:4;3939-3941:5;3947-4027:6;3952-3957:6.1;3959-3962:6.2;3966-3984:6.3;3975-3982:6.3.1;3986-3988:6.4;4035-4038:1;4047-4053:2;4079-4083:1;4085-4094:2;4120-4124:1;4126-4131:2;4164-4168:1;4170-4179:2;4203-4205:1;4221-4228:1;4261-4268:1;4270-4270:2;4276-4279:3;4304-4306:1;4307-4308:2;4316-4318:3;4320-4322:4;4325-4332:5;4356-4362:6;4380-4384:1;4385-4387:2;4391-4398:3;4425-4428:1;4429-4431:2;4436-4443:3;4467-4474:1;4489-4493:1;4529-4552:1;4531-4536:1.1;4561-4561:1;4562-4574:2;4563-4573:2.1;4566-4568:2.1.1;4569-4571:2.1.2;4581-4583:3;4636-4638:1;4641-4646:2;4648-4659:3;4691-4693:1;4701-4709:2;4750-4804:1;4755-4762:1.1;4774-4781:1.2;4857-4859:1;4885-4888:1;4919-4919:1;4924-4940:2;4954-4959:3;4956-4958:3.1;4961-4963:4;4964-4978:5;4993-4995:1;4999-5001:1;5007-5014:1;5018-5018:2;5019-5019:3;5020-5020:4;5021-5021:5;5022-5022:6;5052-5056:1;5083-5085:1;5090-5092:1;5098-5110:1;5110-5114:2;5121-5126:3;5135-5141:4;5147-5147:5;5148-5148:6;5149-5149:7;5150-5150:8;5151-5151:9;5152-5152:10;5153-5153:11;5154-5159:12;5160-5162:13;5163-5165:14;5166-5166:15;5167-5167:16;5168-5171:17;5174-5174:18;5175-5177:19;5212-5216:20;5217-5221:21;5283-5288:1;5290-5292:2;5292-5297:3;5293-5296:3.1;5310-5310:4;5311-5313:5;5327-5331:6;5365-5370:1;5374-5374:2;5375-5375:3;5376-5376:4;5377-5377:5;5391-5395:6;5430-5430:1;5435-5439:2;5440-5447:3;5476-5478:1;5487-5489:2;5491-5499:3;5535-5537:1;5543-5549:2;5552-5587:3;5555-5559:3.1;5598-5606:4;5624-5626:1;5636-5658:2;5689-5689:1;5692-5704:2;5694-5697:2.1;5698-5701:2.2;5719-5732:1;5732-5736:2;5744-5750:3;5754-5759:4;5760-5764:5;5788-5799:1;5802-5804:2;5830-5841:1;5844-5846:2;5852-5857:3;5976-5985:1;6013-6017:1;6028-6033:1;6094-6115:1;6102-6102:1.1;6156-6184:1;6165-6165:1.1;6221-6233:1;6269-6299:1;6338-6340:1;6360-6363:2;6446-6451:1;6471-6491:1;6485-6488:1.1;6493-6495:2;6494-6494:2.1;6535-6598:3;6543-6559:3.1;6544-6558:3.1.1;6564-6568:3.2;6604-6604:1;6605-6607:2;6608-6608:3;6609-6609:4;6610-6610:5;6611-6611:6;6623-6623:7;6629-6629:8;6694-6703:1;6723-6731:2;6741-6750:1;6780-6782:1;6843-6857:2;6887-6887:1;6894-6902:2;6915-6918:3;6919-6925:4;6954-6959:1;6968-6979:2;6984-6987:3;7032-7036:1;7038-7044:2;7056-7059:1;7109-7111:1;7117-7133:2;7177-7182:1;7194-7213:2;7197-7204:2.1;7232-7245:3;7260-7263:1;7268-7279:2;7290-7294:1;7310-7321:1;7342-7357:1;7360-7360:2;7386-7395:3;7403-7405:1;7417-7433:2;7466-7468:1;7473-7479:2;7484-7488:3;7493-7496:4;7501-7509:5;7514-7518:6;7523-7530:7;7535-7538:8;7543-7547:9;7552-7554:10;7555-7561:11;7566-7568:12;7569-7575:13;7580-7583:14;7584-7588:15;7593-7596:16;7597-7600:17;7605-7608:18;7609-7612:19;7617-7620:20;7621-7624:21;7629-7632:22;7633-7636:23;7639-7696:24;7646-7648:24.1;7672-7679:24.2;7680-7683:24.3;7713-7714:1;7714-7716:2;7720-7728:3", "", "1308=FormValue/1/1/1;2027=New/1/1/10;3833=String/1/1/4,NewReader/1/1/4,NopCloser/1/1/4;4343=New/1/1/20;4880=New/1/2/39,New/2/2/50;5061=LimitReader/1/1/4,ErrReader/1/1/4,MultiReader/1/1/4;5936=NewReader/1/1/1", "306:29,481:33,576:29,610:21,885:25,976:21,994:21,1036:25,1513:21,1529:29,1674:21,1675:21,1687:29,1747:33,1861:41,1949:21,1951:21,1966:29,2081:33,2188:21,2196:29,2203:29,2410:21,2868:21,3002:21,3081:21,3112:25,3398:21,3452:21,3466:29,3543:21,3590:29,3632:21,3696:21,3755:21,3958:21,4049:29,4050:29,4189:21,4293:21,4562:21,4564:21,4810:21,5079:25,5781:29,5818:21,5856:25,6093:21,6102:21,6286:25,7076:21,7080:21,7261:21,7494:25,7806:21,7817:29,7843:21,7901:29,7921:21,7932:21,7971:21,8004:21,8250:29,8392:29,8457:21,8756:25,8931:21,8933:21,8952:29,9005:25,9142:25,9691:33", "653:29,654:21,1408:29,1409:21,4004:21,4005:13,4716:36,4717:13,4740:21,4741:13,7538:21,7539:13,7649:29,7650:21,7732:25,7733:17")]
// </GoSourcePositionMaps>

namespace go.net;

[GoPackage("http_test")]
public static partial class http_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestClientConnReserveAndConsume_type {}
    internal partial struct TestCrossOriginProtectionAddTrustedOriginErrors_tests {}
    internal partial struct TestCrossOriginProtectionPatternBypass_tests {}
    internal partial struct TestCrossOriginProtectionSecFetchSite_tests {}
    internal partial struct TestCrossOriginProtectionServer_tests {}
    internal partial struct TestCrossOriginProtectionTrustedOriginBypass_tests {}
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
    internal partial struct TestServerUnencryptedHTTP2HeaderTimeout_type {}
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
    internal partial struct fakeConnectionStateConn {}
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
    internal partial struct roundTripperGetBody {}
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
    internal partial struct testAddMode {}
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
    internal partial struct testSkipMode {}
    internal partial struct testStripPrefix_cases {}
    internal partial struct testTimeoutHandlerSuperfluousLogs_tests {}
    [GoLocalName("roundTripResult")] internal partial struct testTransportCONNECTRejected_roundTripResult {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsynctest() => builtin.initPackage(typeof(global::go.@internal.synctest_package));
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
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸmetrics() => builtin.initPackage(typeof(global::go.runtime.metrics_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(global::go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸfstest() => builtin.initPackage(typeof(global::go.testing.fstest_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸiotest() => builtin.initPackage(typeof(global::go.testing.iotest_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸsynctest() => builtin.initPackage(typeof(global::go.testing.synctest_package));
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
