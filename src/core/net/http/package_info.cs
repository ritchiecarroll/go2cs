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
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using context = go.context_package;
using fs = go.io.fs_package;
using gzip = go.compress.gzip_package;
using hpack = go.vendor.golang.org.x.net.http2.hpack_package;
using httpcommon = go.net.http.@internal.httpcommon_package;
using httptrace = go.net.http.httptrace_package;
using io = go.io_package;
using multipart = go.mime.multipart_package;
using net = go.net_package;
using textproto = go.net.textproto_package;
using time = go.time_package;
using tls = go.crypto.tls_package;
using url = go.net.url_package;
using urlpkg = go.net.url_package;
// </ImportedTypeAliases>

using go;
using static go.net.http_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
[assembly: GoDynamicTypeLift("696e746572666163657b42617365436f6e74657874282920636f6e746578742e436f6e746578747d", "http2ConfigureServer_baseContexter")]
[assembly: GoDynamicTypeLift("696e746572666163657b43616e63656c52657175657374282a6e65742f687474702e52657175657374297d", "setRequestCancel_canceler")]
[assembly: GoDynamicTypeLift("696e746572666163657b436c6f736549646c65436f6e6e656374696f6e7328297d", "CloseIdleConnections_closeIdler")]
[assembly: GoDynamicTypeLift("696e746572666163657b436c6f736557726974652829206572726f727d", "CloseWrite_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b456e61626c6546756c6c4475706c65782829206572726f727d", "EnableFullDuplex_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b466c7573684572726f722829206572726f727d", "Flush_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b497348545450324e6f436163686564436f6e6e4572726f7228297d", "http2isNoCachedConnError_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b53657452656164446561646c696e652874696d652e54696d6529206572726f727d", "SetReadDeadline_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b5365745772697465446561646c696e652874696d652e54696d6529206572726f727d", "SetWriteDeadline_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b556e656e637279707465644e6574436f6e6e2829206e65742e436f6e6e7d", "http2unencryptedNetConnFromTLSConn_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b646f4b656570416c69766573282920626f6f6c7d", "http2h1ServerKeepAlivesDisabled_I")]
[assembly: GoDynamicTypeLift("696e746572666163657b72657175657374546f6f4c6172676528297d", "Read_requestTooLarger")]
[assembly: GoDynamicTypeLift("6e65742f687474702e49", "http2h1ServerKeepAlivesDisabled_I")]
[assembly: GoDynamicTypeLift("6e65742f687474702e62617365436f6e746578746572", "http2ConfigureServer_baseContexter")]
[assembly: GoDynamicTypeLift("6e65742f687474702e63616e63656c6572", "setRequestCancel_canceler")]
[assembly: GoDynamicTypeLift("6e65742f687474702e636c6f736549646c6572", "CloseIdleConnections_closeIdler")]
[assembly: GoDynamicTypeLift("6e65742f687474702e72657175657374546f6f4c6172676572", "Read_requestTooLarger")]
[assembly: GoDynamicTypeLift("7374727563747b696f2e5265616465723b20696f2e577269746572546f7d", "rᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<ClientConn, RoundTripper>(Pointer = true)]
[assembly: GoImplement<Dir, FileSystem>(Pointer = true)]
[assembly: GoImplement<Dir, FileSystem>]
[assembly: GoImplement<File, io_package.ReadSeeker>]
[assembly: GoImplement<HandlerFunc, Handler>]
[assembly: GoImplement<MaxBytesError, error>(Pointer = true)]
[assembly: GoImplement<ProtocolError, error>(Pointer = true)]
[assembly: GoImplement<ResponseController, Hijacker>(Pointer = true)]
[assembly: GoImplement<ResponseWriter, io_package.Writer>]
[assembly: GoImplement<ServeMux, Handler>(Pointer = true)]
[assembly: GoImplement<Transport, RoundTripper>(Pointer = true)]
[assembly: GoImplement<Write_r1, io_package.Closer>(Promoted = true)]
[assembly: GoImplement<Write_r1, io_package.ReadCloser>]
[assembly: GoImplement<Write_r1, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<body, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<bodyEOFSignal, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<bodyEOFSignal, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bodyLocked, io_package.Reader>]
[assembly: GoImplement<bufioFlushWriter, io_package.Writer>]
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bufio_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<byteReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<cancelTimerBody, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<checkConnErrorWriter, io_package.Writer>]
[assembly: GoImplement<chunkWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<connReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<countingWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<dirEntryDirs, anyDirs>]
[assembly: GoImplement<eofReader, go.compress.flate_package.Reader>]
[assembly: GoImplement<errorReader, io_package.Reader>]
[assembly: GoImplement<exactSig, sniffSig>(Pointer = true)]
[assembly: GoImplement<expectContinueReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<fakeLocker, sync_package.Locker>]
[assembly: GoImplement<fileHandler, Handler>(Pointer = true)]
[assembly: GoImplement<fileInfoDirs, anyDirs>]
[assembly: GoImplement<fileTransport, RoundTripper>]
[assembly: GoImplement<finishAsyncByteRead, io_package.Reader>]
[assembly: GoImplement<globalOptionsHandler, Handler>]
[assembly: GoImplement<go.net.http.internal_package.FlushAfterChunkWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<gzipReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<htmlSig, sniffSig>]
[assembly: GoImplement<http1ClientConn, RoundTripper>]
[assembly: GoImplement<http1ClientConn, genericClientConn>]
[assembly: GoImplement<http2ConnectionError, error>]
[assembly: GoImplement<http2ContinuationFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2ContinuationFrame, http2headersOrContinuation>(Pointer = true)]
[assembly: GoImplement<http2DataFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2GoAwayError, error>]
[assembly: GoImplement<http2GoAwayFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2HeadersFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2HeadersFrame, http2headersOrContinuation>(Pointer = true)]
[assembly: GoImplement<http2MetaHeadersFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2PingFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2PriorityFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2PushPromiseFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2RSTStreamFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2SettingsFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2StreamError, error>]
[assembly: GoImplement<http2StreamError, http2writeFramer>]
[assembly: GoImplement<http2Transport, RoundTripper>(Pointer = true)]
[assembly: GoImplement<http2Transport, h2Transport>(Pointer = true)]
[assembly: GoImplement<http2UnknownFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2WindowUpdateFrame, http2Frame>(Pointer = true)]
[assembly: GoImplement<http2bufferedWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<http2bufferedWriterTimeoutWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<http2chunkWriter, io_package.Writer>]
[assembly: GoImplement<http2clientConnPool, http2ClientConnPool>(Pointer = true)]
[assembly: GoImplement<http2clientConnPool, http2clientConnPoolIdleCloser>(Pointer = true)]
[assembly: GoImplement<http2connError, error>]
[assembly: GoImplement<http2dataBuffer, http2pipeBuffer>(Pointer = true)]
[assembly: GoImplement<http2duplicatePseudoHeaderError, error>]
[assembly: GoImplement<http2eofReader, go.compress.flate_package.Reader>]
[assembly: GoImplement<http2erringRoundTripper, RoundTripper>]
[assembly: GoImplement<http2flushFrameWriter, http2writeFramer>]
[assembly: GoImplement<http2goAwayFlowError, error>]
[assembly: GoImplement<http2gzipReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<http2handlerPanicRST, http2writeFramer>]
[assembly: GoImplement<http2headerFieldNameError, error>]
[assembly: GoImplement<http2headerFieldValueError, error>]
[assembly: GoImplement<http2httpError, error>(Pointer = true)]
[assembly: GoImplement<http2missingBody, io_package.ReadCloser>]
[assembly: GoImplement<http2netHTTPClientConn, RoundTripper>]
[assembly: GoImplement<http2noBodyReader, io_package.ReadCloser>]
[assembly: GoImplement<http2noCachedConnError, error>]
[assembly: GoImplement<http2noDialClientConnPool, http2clientConnPoolIdleCloser>]
[assembly: GoImplement<http2noDialH2RoundTripper, RoundTripper>]
[assembly: GoImplement<http2priorityWriteSchedulerRFC7540, http2WriteScheduler>(Pointer = true)]
[assembly: GoImplement<http2priorityWriteSchedulerRFC9218, http2WriteScheduler>(Pointer = true)]
[assembly: GoImplement<http2pseudoHeaderError, error>]
[assembly: GoImplement<http2randomWriteScheduler, http2WriteScheduler>(Pointer = true)]
[assembly: GoImplement<http2requestBody, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<http2responseWriter, CloseNotifier>(Pointer = true)]
[assembly: GoImplement<http2responseWriter, Flusher>(Pointer = true)]
[assembly: GoImplement<http2responseWriter, Pusher>(Pointer = true)]
[assembly: GoImplement<http2responseWriter, ResponseWriter>(Pointer = true)]
[assembly: GoImplement<http2responseWriter, http2stringWriter>(Pointer = true)]
[assembly: GoImplement<http2roundRobinWriteScheduler, http2WriteScheduler>(Pointer = true)]
[assembly: GoImplement<http2serverConn, http2writeContext>(Pointer = true)]
[assembly: GoImplement<http2sortPriorityNodeSiblingsRFC7540, sort_package.Interface>]
[assembly: GoImplement<http2sorter, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<http2stickyErrWriter, io_package.Writer>]
[assembly: GoImplement<http2transportResponseBody, io_package.ReadCloser>]
[assembly: GoImplement<http2unencryptedTransport, RoundTripper>(Pointer = true)]
[assembly: GoImplement<http2write100ContinueHeadersFrame, http2writeFramer>]
[assembly: GoImplement<http2writeData, http2writeFramer>(Pointer = true)]
[assembly: GoImplement<http2writeGoAway, http2writeFramer>(Pointer = true)]
[assembly: GoImplement<http2writePing, http2writeFramer>(Pointer = true)]
[assembly: GoImplement<http2writePingAck, http2writeFramer>]
[assembly: GoImplement<http2writePushPromise, http2writeFramer>(Pointer = true)]
[assembly: GoImplement<http2writeResHeaders, http2writeFramer>(Pointer = true)]
[assembly: GoImplement<http2writeSettings, http2writeFramer>]
[assembly: GoImplement<http2writeSettingsAck, http2writeFramer>]
[assembly: GoImplement<http2writeWindowUpdate, http2writeFramer>]
[assembly: GoImplement<initALPNRequest, Handler>]
[assembly: GoImplement<ioFS, FileSystem>]
[assembly: GoImplement<ioFile, File>]
[assembly: GoImplement<io_package.ReadWriteCloser, io_package.ReadCloser>]
[assembly: GoImplement<loggingConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<maskedSig, sniffSig>(Pointer = true)]
[assembly: GoImplement<maxBytesReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<mp4Sig, sniffSig>]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriteCloser>]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriter>]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<net_package.Conn, io_package.Writer>]
[assembly: GoImplement<net_package.TCPConn, closeWriter>(Pointer = true)]
[assembly: GoImplement<noBody, io_package.ReadCloser>]
[assembly: GoImplement<noBody, io_package.WriterTo>]
[assembly: GoImplement<noopHandler, Handler>(Pointer = true)]
[assembly: GoImplement<noopHandler, Handler>]
[assembly: GoImplement<nothingWrittenError, error>(Promoted = true)]
[assembly: GoImplement<nothingWrittenError, error>]
[assembly: GoImplement<onceCloseListener, net_package.Listener>(Pointer = true)]
[assembly: GoImplement<os_package.File, File>(Pointer = true)]
[assembly: GoImplement<persistConn, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<persistConnWriter, io_package.ReaderFrom>(Pointer = true)]
[assembly: GoImplement<persistConnWriter, io_package.Writer>]
[assembly: GoImplement<populateResponse, ResponseWriter>(Pointer = true)]
[assembly: GoImplement<readTrackingBody, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<readWriteCloserBody, io_package.ReadWriteCloser>(Pointer = true)]
[assembly: GoImplement<redirectHandler, Handler>(Pointer = true)]
[assembly: GoImplement<requestBodyReadError, error>(Promoted = true)]
[assembly: GoImplement<requestBodyReadError, error>]
[assembly: GoImplement<response, ResponseWriter>(Pointer = true)]
[assembly: GoImplement<response, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<rᴛ1, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<rᴛ1, io_package.Reader>]
[assembly: GoImplement<rᴛ1, io_package.WriterTo>(Promoted = true)]
[assembly: GoImplement<serverHandler, Handler>]
[assembly: GoImplement<socksAddr, net_package.Addr>(Pointer = true)]
[assembly: GoImplement<socksConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<socksConn, net_package.Conn>(Promoted = true)]
[assembly: GoImplement<statusError, error>]
[assembly: GoImplement<stringWriter, io_package.StringWriter>]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<textSig, sniffSig>]
[assembly: GoImplement<timeoutError, error>(Pointer = true)]
[assembly: GoImplement<timeoutHandler, Handler>(Pointer = true)]
[assembly: GoImplement<timeoutWriter, Pusher>(Pointer = true)]
[assembly: GoImplement<timeoutWriter, ResponseWriter>(Pointer = true)]
[assembly: GoImplement<tlsHandshakeTimeoutError, error>]
[assembly: GoImplement<transportReadFromServerError, error>]
[assembly: GoImplement<unencryptedHTTP2Request, Handler>]
[assembly: GoImplement<unencryptedNetConnInTLSConn, net_package.Conn>(Promoted = true)]
[assembly: GoImplement<unencryptedNetConnInTLSConn, net_package.Conn>]
[assembly: GoImplement<unsupportedTEError, error>(Pointer = true)]
[assembly: GoImplement<writerOnly, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<writerOnly, io_package.Writer>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(ClientConn))]
[assembly: GoFieldAccess(typeof(Cookie))]
[assembly: GoFieldAccess(typeof(CrossOriginProtection))]
[assembly: GoFieldAccess(typeof(Request))]
[assembly: GoFieldAccess(typeof(Response))]
[assembly: GoFieldAccess(typeof(ServeMux))]
[assembly: GoFieldAccess(typeof(Server))]
[assembly: GoFieldAccess(typeof(Transport))]
[assembly: GoFieldAccess(typeof(body))]
[assembly: GoFieldAccess(typeof(bodyEOFSignal))]
[assembly: GoFieldAccess(typeof(conn))]
[assembly: GoFieldAccess(typeof(connReader))]
[assembly: GoFieldAccess(typeof(expectContinueReader))]
[assembly: GoFieldAccess(typeof(go.crypto.tls_package.Config))]
[assembly: GoFieldAccess(typeof(go.crypto.tls_package.ConnectionState))]
[assembly: GoFieldAccess(typeof(go.mime.multipart_package.FileHeader))]
[assembly: GoFieldAccess(typeof(go.mime.multipart_package.Form))]
[assembly: GoFieldAccess(typeof(go.net.http.httptrace_package.ClientTrace))]
[assembly: GoFieldAccess(typeof(go.net.textproto_package.Reader))]
[assembly: GoFieldAccess(typeof(go.net.url_package.Error))]
[assembly: GoFieldAccess(typeof(go.net.url_package.URL))]
[assembly: GoFieldAccess(typeof(headerSorter))]
[assembly: GoFieldAccess(typeof(http2ClientConn))]
[assembly: GoFieldAccess(typeof(http2DataFrame))]
[assembly: GoFieldAccess(typeof(http2Framer))]
[assembly: GoFieldAccess(typeof(http2GoAwayFrame))]
[assembly: GoFieldAccess(typeof(http2HeadersFrame))]
[assembly: GoFieldAccess(typeof(http2MetaHeadersFrame))]
[assembly: GoFieldAccess(typeof(http2PingFrame))]
[assembly: GoFieldAccess(typeof(http2PushPromiseFrame))]
[assembly: GoFieldAccess(typeof(http2RSTStreamFrame))]
[assembly: GoFieldAccess(typeof(http2Server))]
[assembly: GoFieldAccess(typeof(http2Transport))]
[assembly: GoFieldAccess(typeof(http2WindowUpdateFrame))]
[assembly: GoFieldAccess(typeof(http2addConnCall))]
[assembly: GoFieldAccess(typeof(http2clientConnPool))]
[assembly: GoFieldAccess(typeof(http2clientStream))]
[assembly: GoFieldAccess(typeof(http2connOutflow))]
[assembly: GoFieldAccess(typeof(http2dialCall))]
[assembly: GoFieldAccess(typeof(http2frameCache))]
[assembly: GoFieldAccess(typeof(http2pipe))]
[assembly: GoFieldAccess(typeof(http2priorityNodeRFC7540))]
[assembly: GoFieldAccess(typeof(http2priorityWriteSchedulerRFC7540))]
[assembly: GoFieldAccess(typeof(http2requestBody))]
[assembly: GoFieldAccess(typeof(http2responseWriterState))]
[assembly: GoFieldAccess(typeof(http2serverConn))]
[assembly: GoFieldAccess(typeof(http2serverInternalState))]
[assembly: GoFieldAccess(typeof(http2startPushRequest))]
[assembly: GoFieldAccess(typeof(http2stream))]
[assembly: GoFieldAccess(typeof(http2transportTestHooks))]
[assembly: GoFieldAccess(typeof(http2writeData))]
[assembly: GoFieldAccess(typeof(http2writePushPromise))]
[assembly: GoFieldAccess(typeof(http2writeQueue))]
[assembly: GoFieldAccess(typeof(http2writeResHeaders))]
[assembly: GoFieldAccess(typeof(io_package.LimitedReader))]
[assembly: GoFieldAccess(typeof(net_package.OpError))]
[assembly: GoFieldAccess(typeof(onceCloseListener))]
[assembly: GoFieldAccess(typeof(os_package.SyscallError))]
[assembly: GoFieldAccess(typeof(pattern))]
[assembly: GoFieldAccess(typeof(persistConn))]
[assembly: GoFieldAccess(typeof(populateResponse))]
[assembly: GoFieldAccess(typeof(readTrackingBody))]
[assembly: GoFieldAccess(typeof(readWriteCloserBody))]
[assembly: GoFieldAccess(typeof(response))]
[assembly: GoFieldAccess(typeof(routingNode))]
[assembly: GoFieldAccess(typeof(serveMux121))]
[assembly: GoFieldAccess(typeof(socksAddr))]
[assembly: GoFieldAccess(typeof(socksDialer))]
[assembly: GoFieldAccess(typeof(socksUsernamePassword))]
[assembly: GoFieldAccess(typeof(time_package.Timer))]
[assembly: GoFieldAccess(typeof(timeoutWriter))]
[assembly: GoFieldAccess(typeof(transferReader))]
[assembly: GoFieldAccess(typeof(transferWriter))]
[assembly: GoFieldAccess(typeof(transportRequest))]
[assembly: GoFieldAccess(typeof(wantConn))]

// The go statements whose goroutine the converter proved cannot recurse, by the file, line and column of
// their goǃ call: go2cs-gen starts each on golib's bounded stack reservation rather than the Go-scale default.
[assembly: GoBoundedGoroutine("h2_bundle.cs", 7426, 9, 10)]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("net/http/client.go", "client.cs", "AGzaAQAiTAAJCoKUgqaC7oKUqOKCgoKUgoKmgoKUgoKmpoKClKaCgpS4AAsI8pSCgqaCgqaCgsqCgoLugoKmgoKCgoKmgpSUgoKCgpS4gqaUgpQAChaClJSC3KyygoKUAAIQ0pSClMQABxCClAAIGAAMAoKUgpSmgqaCgqSUgoKmgpSkhoS4gqKCgriClMLEgrSCtMgAAhDSggACOAAZAgAGNAAZAoKClKbcmKKCgpSq8pSCgtyCxoKClMq26qKClIKUAAJQACUCAAoK8oKApIKCypQACRiUgpSCgpSU3KaCgriUgoKClIK4gqaCAAgSgoKCgpSC7oKCpqaClMqCAAIQ0oKUlMqCgriCgoKUgoKUppKCgpSmpgALDua4goKCuAAOGIKCgoKCgqaCgoKCgqaC3IKCgqbupoIACQyCgpQAAiYAEAIACCIAEAKCgpSCAAImABACAAYiAA4CAAIiAA4CAAYeAA4CgoKUAAYU8ogADRyygoKUgpSClKaCgoKmAA0cgoKuwoLKgsqClKaigoKU", "236-241:1;372-372:1;384-393:2;396-401:3;406-417:4;599-599:1;622-638:2;774-830:1", "", "683=urlErrorOp/1/1/1,New/1/1/2;711=urlErrorOp/1/1/1", "", "677:56,678:17")]
[assembly: GoPositionMap("net/http/clientconn.go", "clientconn.cs", "AGLiAQAYApTGppKClIKmgpQADh6CgoK4AA0cgoKCgtyCkqaCgpSCpoKCgpTa4oLcsgAIBoKClIKmgIK2gIKkgpSClAACFgAMAoKCgpTcsqyyAAIgABECgtrigqKo8oKCgpSClIKUgpSqgoKClIKCgtaCgoLu3IKCgoKCAAUkAA4CgoKCAAgSooKmgoKmgoKClILmgoKmgqSkuKKCkoKU1qKCkoKUAAkGooKSgpTWlILWooKSgtaktA==", "345-350:1", "", "168=JoinHostPort/1/1/2", "451:29")]
[assembly: GoPositionMap("net/http/clone.go", "clone.cs", "AA4uAAkCgrgAAhgACwKClIKCgoKUAAIYAAsCgpSmgoKCgoKUlJQAAhgACwKClIKCggACHgAMAoKClA==", "", "", "72=Clone/1/1/1")]
[assembly: GoPositionMap("net/http/cookie.go", "cookie.cs", "ABUoACZYgoKCgrqCgoKUgoKClJSssoKUgoKUgqKCooKUgpSSgpSU6qKCgpSCooKUgoKUkoKU3IKCgqaCgoKUgoKCppSCgoKUlLS0tKS0grSCtIK0goKUgpSCtIKCgoKCgqaCtIK0gqSUAAYS4oKCyoKUgoKCpqyyggAbEOKCmKKCgoKClIKClILKgoKUgpSmgoKClIKCgoKUgpSClNi0tKSClAAOCLKClIKUgpSCgqaCgoK4goKmgoKmAAIWAAgCgoLcgoKUgqaCopSCgoKCgpSigoKUgpSSgpSmqJKClIKUqKSssoKUgqamlIKCgoKClLaCxsaClMaClIKUpJSCpqaUggAGIgAOAoKClKaC6qKmgqaigoKClIKClIKUgoKCpgACGAAMBIKClIKCpg==")]
[assembly: GoPositionMap("net/http/csrf.go", "csrf.cs", "ACtYkgACGAAOAoKClIKUgpSClIKCgpSCAAgKlAABHgANAqaCgoKUgoK4AAIUAAkCgoKUAAgKwram+qSClKaCpqbupoKUqIKcAAgCgpS4goKUAAUS4oKCgoKUgpQ=", "207-217:1")]
[assembly: GoPositionMap("net/http/filetransport.go", "filetransport.cs", "ABE+AA0CAAIiAA4Cpu6CkoKUpoKCAAsYAA8ggoKUgpSmgoKUlIKUpoKmgoKUlIKmgoKUgoKU", "61-64:1")]
[assembly: GoPositionMap("net/http/fs.go", "fs.cs", "ADJksoKmgoKClIKClIKm3JiigoKUgoKUgoKUgoKClAAjRIKCuIKC9IiygoKCgpSCgqaCgoKUpIKCgoKCgoLKgpTKnMKUgtyClIKUpoKmAAJEAB8CgoKClIKClJTuuAARDOKCgoKmuIKCgoKEkoKCgoKCpoKCpoKCgpSUgriCgoLGyoKUgrSCpsqUAAwagoKClIKCtIKUgoKCgpKysoKCgpSCgpSCgqaCyAAaNoKUlIK0qrKCgoKUgriCgtikpqqiqqIAEBqCgoKUgoKClIKClIKUgoKUgpSm5oKCgpSCgsqCgpTmgoKClIKCgoKUgoKUgpSCgpSClJTmgoKUgoKUgoK4goKU5oKClIKClIKCgpTKgpSCgpSClKaWkuaCgrjcgoKCgoKU6tSCgpSCgpSUgoKUgsaCgsiCgpQACgjSyoKCpoKCgoKUpIKCgoKmpoKCgoKUgoKUgoKUgriClIKCuIKCgpKCgoLcgoKClIKCuJKiAAoO0oKUgpSCpqqigpSCAAYwABcC3IKUggACLgAWAtyClKaCgpSCgqamAA0cgoKUlIKCqKaCgpSClIKCgpSmgoKClKaCgoKUgoKCgoKUlJSClIKmrLIAAiAADQIAAhgACQKmooKCgpQABxCCpoIABxC4goKCgoKCuAAKCqKClIKClIKCgoKClIKUkoKClIKClIKC3IKUgoKUgpSClIKClKaClIKUlIKClIKUppSUlNyCgqrCgoKCgpSCgqaigpQ=", "161-161:1;247-257:1;376-394:1;761-761:1;896-898:1", "", "1127=contentRange/1/1/1", "413:25,794:21,838:29")]
[assembly: GoPositionMap("net/http/h2_bundle.go", "h2_bundle.cs", "AER+4oKUgoKmqJKClKqigoKmqqKClADYAsYF8gABqgSkABcwggANIIIABRDElIKCgoKUlIKCgriClIKCpoKClIKCgoKClIKClIIAECSylJSCgpSCkqiSgpSCgoKUlAACFgAKAoKCgoKmgoKClLiCpJSCgpQACRSClIKCgpSClIKCqJKCgqaClIKUgqbSgoKCgoKUgoKUpqKk0oLugoLGpIKCgoLKgpQACRCCruKUlLiUprgAIEaiAAwagoKqogALGIKCgqaClIKmooK4goKCgoKUlIKUlIKqpqKCpqKClIKUgpSClIKUgpSClIKUgpSClIKUgpSClIK4ggAMGgAIEIKUpKSkpLiClLS0tLS0ABEimKKClIKCgoKCgoKUgoKCgoKCpqaCgpSokqiSgriCgpSCgoKCgpSmgoKCgqaCgoIAFC4AECKCgpSmgoKU7oIADRyUgqaCgpQADhQADRyCyoLKgsqCyoKoggAQJpIAAhTygpSGooKUgqaUgoKqooKUgqyygpSCggAJFIKCpoKCgpSCpoKCgoKUggAQJoKClJSUgoKClKaCgpSCgpSCqqKClIKmgpSmlILKABAmAAwagoKUAAYQkgAZOAAYPgAMGoKClAAXPJDUgoKCgoLmooKCgoKCgpSCgpSCgpS4gpSmgoK4uJSCAAgIgoKqwoKC1oKCgpQAUMABgoKUppQACxjGgoKUuIKmgoKUpqKCgoKmlIKCgoKUppSUlIIABRaygpTugoKUqJIACRKSgpSClIKuwoKUAAIU8sqYooKUAAIaAAoCgoKClIKClJSClK7igpSCgoKUlIKCgpSUgoKUgpQAAhoADAKCgpSuwoKssoKCgqaCgsqCuIKmlIKUyAAKFoKuwoIACgaC3IKUgpSCgoKCgoKmyoKUgqiCgoKWgqaCAAIQ0gACGAALAoKUqqKClIKClIKClMqCgpSClIKClIKCABYagu6ClAAHEIKUgpSUgoK4lKaCpoKCgoKmqqKC3JaSgoK4goKCgoKCuJSCgoKClJSqooKCgqYAAhAACAKCgoKUruKCAAsY9IKCgpSCgpSCgqaygoKUgoIACyDCggAIBoKCgpSCgpQABxCigoKCggAJHtKCpoIAERSCgoKUgu6CgpSClAAJFPSClIKCAAscgoKmgqaCpoIADgaipsqClIKCgoKmgoKCgoKUgoKCgoKmgoKUggAZTAAKAoKUgoKUgpSClIKUgoKUgoKClIKUgpSCggAJFAAjUIIACAaCgoKUgoKUgoIACRzigpSClIKCgpSCggAPFIKCgpSCgpSu4oKUgoIADRSCgoKUpoKCpoKu4oKUgoKUgoIAChaCgqaCAAwGoqbugpiigoKCuIKCgpSUlIKUggAUQAAJAoKUgoKUgpSCgpSClIKCgqrCgoKmgoKUpoKClAApWqKCgpSCpqqigoKmqqKCgqamgpKCgqS0tMqCgriClKaCgpSU7PKClKaiopSCgoKCooKUlJSCgoKmgoK4goKmgoKCgtyUgpSCgoKCgpSUprSCggAJFIKCuNyCgrimgIK2gpSAgpTIgpSAgqSCgoKUlICCgoKUpAALBoKCgpSCgoKClIKUgsaCgoKUgoLGgpS0tMakpgAOHIKClKaCgpSCuIKClIK4lKKCgoKUgoKClIKCgpTWlILKwqSCpoKUggAIDpSCgoKCxrTYgraCgpSCgoKUtLS0goKkgoKCppSCgpSUgpSCgpSmlIKokoKUAA8kgoKClIKCgpSCABg0ABUyAAcQggALGoKopJSCxoLGgsaCtgAOIgAJFIKClAACGAAJAoKUgoKUgqamgpSklAAKHsKokqiSAAwaggANHMyCgpSmooKCgpSmgoKClIKCgoLKgqzSgpSCgoKCpoLKgoKUqqKUpKSUAAgSlJSU3OyUlJzigoKUgoKmxoKCggAdPPKCgoKUoqTSgoKClNoACAKCgoKUgoKUgpSCgoKUgpT6gpoACAKCgoKUgoLKgpQABhDAmqCYkJTygpSCgoKUgpSUgoKClJSCoqaSgrjWyuKCgoKU2vKCgoKClKYADyCCgoKWlIKCAF7eAaKClIKCpqKClIKCpqKClIKClMrIgoKUpoKCgpQAChAACAKClIKUuIKClKaUgoK4goK6toIAChaUgpSCpoKUgoKe0oiU7pLKkoKCgpSUgpSUABtCooKUpqKClKaigoKUgqYAAiIAEAKClAAIBtKCpIKCABgygqaC7oKmgpTcgoKClIKClIKCgpSCggAKFoKCpgALGAAKFoK4griAgoKkpoKmgoKmoqSygoKClKaClIKCAEWSAYKCgpSmgoIAIkqUlJSipoKUggAHEIKCpoKmrLKCuKKCuKKClAADEMKClKqigqaC7oKCgoKCgsqmooKUlJQAChaCgoKClIKClIKUgoKCgpQACyLCgpCSgoLmlOaUggALIOKCgpSUpqKCgriCgoK41IKClIKCgtikwoKCgoKClIKm7oKUpriCpoKC3IKUgoKmgoKCppSCpIKCgoLkgoKUtLSmgqTogpSCgoLGtJS0lIKkgrS0gqS0tNa0xAAIDoKC3IKCggALCqKCgoKUgqaCgqaCpqaCpt6CgoKCgpaUlJSUgoIACgya8oKUgqSCgoKClKaCkrSkgoKm+MzCgoKCyoKUgsS0AAkSpLS2goKUAAIU8oK0yAAEGAAKApYAFCiCgoLclLSmgpTIgoKmgqaUrNKCgqaCgpQACArWtoKCgoKCgriCgtyCgpSCgoKCuIKClILumMKCgpSClIKmlIKCgpQACxjKtLamgsbalAACGgAMAoKClIKCgoKClIKC3JSCgoKUgoKClIKUgqaCgoKUlAACFAAJAoKAABUqgqaigoKClJSCgoKmooKmooKCgr7SgoKCgoKUggAIEqaCgpSCgriUgqSCpIKCpoKCtIKUlAALCKKmgoKU7oSCgpSUppSkpKSkpKSkyLSC+LKCgpSCuJTclIIACAaygpSCypTclIKUlMaCtoLmspSC3JSCgpSmsoKClIKClIKUgpSUgoKCgpSCpoKUgoKUpoKCggAIBrKCgoK4lJS4lICCyIKCpqKCgpSClKaClIK0tLSktAALEIK25qKC7pQADAaygpSCggAKFtwACRSClJSUlJSCuIKClJS4lJSCpoKCgriClIIACBKCgpSClKaygoKUlKaCqJKqwoKUgqaClKqigpTOoqa8ogATDrKC3ILcgqbcgpQABxCClJSCAAgSgpTupoKClJSCgpSmgoKUgoKUgpSClIKCAAkUgoKmprKCgoKCgoKUuILcggAMBrKCgoKUgoKmgpSCgoK4lKaC5oLKlKaygpSCpqKCgqaC7oKCgoKmgoKClJSCpgAWBsKUAAgSgqaCgoKUAAoWpoKCgpSClIKmgoKUgoKCgpSmlLimspSCgqaCgqbKAA0cgqaigoKCgoKCgoIADhrSgoKCgoKUgpTupqKCgoKCgoKUgoKUlIKUgoKUgoK6soKCgoK4lIKUgoLKgoKCgpSUlIKi5IrCgqrSgoLKlIDKpILEgqSktqiyAAwgsoKCAAsOsoKCprqSqMKCgoKClIKUgpQAESSCgoKCgrimsoKClIKUgoKUggANHoKCABo8soKmlKaUgoKCpqyygpSClIIACxQACQKCpoKmgoKCkoKCgpSmgoKUpoKCgpSClKaCAAcQgoKCgriCAAgSgpSCpoKUgsqCgpSCuILclAASTgAVAoKClIKCpoKCgriCgqaClKKClKaCgoKUpqaCgqaClKKClKaCgoKUpqaUpoKmgoKClIKC3IKCpPqmgoKClIKCgoKCgqKCpoKmgoKClIKUqAALFoK4goKClOaCgqamlJSCgoKCgqbupoKCgriCgoKCgpQAAhYACAKmguiSgoKUgpSClIKUpoKUuIKCgoKCuoKWABMEooKCuIKmgriClIKUgoK4goKUgoKUgpSClIKmgoLcjKaC3IKmAAcQxKTYxKSkggAMGLLKlIK4goLcgriCpoLugoKUgu6C7pSmgoKmAAweooKClIKClKKC3AALFrKCgqaCgpSmgoL+soKIlAAIBqKClIKClIKClIK0gsSUgoKUggCMAcICgoKCgoKmgpSClKaCrsKCrLKmooK4goCCpIKUgpSClKKCgIKCgsqkgpSUgpSokoKCgpSU7oKmooKmooKUAIUBtgKigpSmwoKCoqSigoKUgqaUuMKCgpKCgrSkooKUgoKSggAJFLKClIKCAAsW1NqygqYACxqCqqKCgoKUgoKCpoKmgpQACAjC6ILGpoKCgoKClIKCgoKCgJSCgpSCgoKCtIK0gtgAChaClJSCgpS+soK6goKCgp7igriCyoKCgpSCgtyCpqaCgpSClJSU5qKClIKClIKClKaCgoKUgpSClOaCgoKCpoKClIKClIKUqqKmgoKUpqKmsoIAFCqCgpSCpoK4yoKCgoKUgoKUgoKUgoKmuIKClIKmgoKCgoKCgriAgoK2kqbCpoKSgoKCgpS0puKCgqKk4oKUgqaClIKUgoK4lLi4xq4ACgKCgtwACAKCgoKUggAgSuKCgoKUlIKCABMi0oKC1qKClILKAAcQpgAIEoKmpoIAAhzigpSClKqipoKCqJKClKrqAAIS4qbCgpKCoqiigoKUgriigoKClIKClJSClKbSgoLWprKCpoKCsoKCgoKCgpSClLSSgrSCpJSCgoLI0oKCgoKClKaClICCpICCttaigoKCgoKClIKUgrKCgoKCopIACAiqwoKCgpSCgqyyguiygoKUypSCgtyssoKUgpSm0oKCoqSCgriCprKCAA0clKSixKSkyKKCAAkUlIKCuIKmpoKCggANHIKUpoLUpOykgsaCgqSCAAQQsoKmAAcSAA0CgpiigtyClILEpKSCyPakpoKClIKCgoKUgoKUlIKmgoKClAAHEIKCgqaCgpSCgoLktLS0tKSCgoK4goKCpriUgoKAgoKSgtqC9KSkgrSkpAAICsKClIK2xKSkAAoSgpKUgpSmgoKCgtaiAA4m4pSU7oKCgoKUgpSCgoKClIKmuKTogoKCgoKCggATKIKCpoKClJSUppSClJSCpoKCgoKmgqrCgqaUgoKUgoKUgoKCpP6SgoKCgpSCgoLclKaCzKaUAAEQ0oKCgpTKlIKUAA0agoKUgoKCgpSm4oKClIKClIKCpoKCgoCCgpSCtoKCgoKCAAAQ8oKClIKCpoKCgoKUpIK0uIKCgoKClIKCgoKC7pSUgri43IKCgoKCpoKSgoKCgtyClJSClN4ACgKCgoKSgoKUgpTEpKTGgoKUgoKElIKUgpT6kpSCgoKCpoKmgoKmuIKmpoKClAAIFMKCgoKCgoK40oKCgoKUgoKCuJSCgoKUgqaiAAgS0oKSgoKCgrQAChSCuIKClIKmwoKSlILcgoKCyoKUAAcQgoKUgoKSpoKCpoL6toKUpoK0AAwIooKClIKCgpSCgpSCgpSCgpSmooKCgoKClIKCgpSClIKCgpSUgoKClIKUgoKClKaUtLS0tLS0tMSkgoKUAAgKsoK4lILKlILKlJSClKaCgoKmypSUlIKCgoKUABASAAkCgqaCgpSSgqaCgoLugoKCgoKClIKmgsqCgpTKgoKUuIIACRSCgpSClIKClKaCggAICoIADh6ClIKA7paCgpSCyIKmgoKmgoKUlKaCgpSCgoKCgpSmspSUgqaUpqaCgoKUlIIADBLCgpSClIKCgoKCgpSClIKCgoKmlKaCgoKClJSCgpKClIKUlNaUgoKUgpSCgpSCuJSClIKmAAoMpJSmsoKCgoKCgoKUggAHEIKCgoKCgpSCgoKCppSCgriUgoK4lIKCgrimgoKCmKKCpoKCgqaCuIKCgpSUgoKClIKUgqaCgriClKb2gsqCgoK0pKKCpqKCAAYW4oKC1oKmlIKCgpSmgoKCgpS4ooKClIKCpoKm0qaCpIKUgoKU1tKCkoKkgoKClKaCkoKUlLSCtLSClLSCAAsUgsaklIKmgsqUgqbW0oKCkoKCpoKClIKUuLimgtaygpSUgoKClIKUlIKosoSSgoKUlIKCgpSUgoLCgoKCgpSCgrSS1KSktsjSgoKSgqSCgpSUgoKUlIKCkoKU1gAHEKoACAqCgoKCgpSCqIKWoqaipqKCuKKmyJTIlJSCgoKm6pSUABAglJSmkoKCgpSotqKCqvKCgoKUgoKCpoKC2PKCgoKUtKjygoKCgpSipMKCgpSU5qKU6tiiqtKCgqaCABIagoKClKaCgoKCypQACBKCpoLmooKSgpTWopKCkoKUgtaikoLagrSkooKS1qKCktaCgri4gqaCpqaigoKUprKCgpSCgoKCgpSUpqKCuKKCuKKCuKKCuKKCuKKClKqipoKClIIADRwACwKGgpQAHkaylKTapNqCpriCgqiCAAcQgoKCpgAHEIKmgqaCAAgSgqaUgqbcoqai6oKmgtqCppq+9IKCgoKUgoKUlAAPIoKClKYABxAACgaigpSCppSClIKUgqaCgqamgoLuAA8ilAAMBqKClIKCgoKUgoKmpoKC7gAHEIKCgoIABxCUAAcQlIKqwoK4gpSCgoKmlLiUgoKmpoKUxgA2fKKCypSUqqKqooKUAAIaAAoCpoKCuIKClIKClIKUgoIADRwACBLKgqiSgoKUlKqigpTWpAAYNIKmgqaCgpSClIKCgqaCgpSClK7CgpSClKS0pAAEENKClIKUgoKCqJKCgpSCgoKCACpg0qbu3IKClJQAGDiygpSCpoKClJSC3IKCgpSCgoKUuKKCggADFAAJAoKUgsqC3IKCgoKCpoKCgqbKgoKClIKClIKCpsqUlKaCgoKUgpQAGUC0goKUgu6CgpTcgoKCuKKClIKUgqaCgpSCgoKUuKKC3IKCgpSC3IKCyoKCgoK4ggAJFIKCgu6CgoKCgpS4gqaigoKUgoK4gpSmprKCgoKUgoKUuIKCgpSClJSmooKUlIKCgpSmooKUggAcQoKmpoKClILKgoKCgriCgoK4goKCgpSUlIKCgqaCpoKCgoK4lJSCgoLKgoKCgriCgoK43IKCgpSCuIKUgpSmlIIACBKmgraClIKClIKCuAAJFJSUgoLMrsIADSC4goKClIKmuIKCgpSCgoKClKaUgqaCgoKClKYADi7SpqaCgpSCgoKCgriCgoK4goKClJSUgoKCpoKmlIKCgpSCuIKUgpSmlIKUgpSCgoKClIKCpg==", "2063-2063:1;2069-2075:2;3226-3276:1;3278-3278:2;3356-3363:1;4308-4308:1;4359-4381:2;4382-4384:3;4388-4400:4;4909-4909:1;5191-5201:1;5547-5547:1;6478-6505:1;6629-6636:1;6906-6920:1;6932-6946:1;6995-6998:1;7274-7319:1;7378-7380:1;7639-7655:1;7659-7661:2;7663-7670:3;7860-7863:1;7890-7893:1;8570-8584:1;8637-8643:1;8745-8754:1;8756-8781:2;8783-8804:3;9018-9020:1;9609-9611:1;9829-9831:1;9915-9917:2;10311-10347:1;10438-10449:1;10684-10688:1;11947-11969:1", "", "1650=Uint32/1/1/4;1992=Sprintf/1/1/1;1998=Sprintf/1/1/1;2233=Uint16/1/1/1,Uint32/1/1/2;2389=Uint32/1/2/2,Uint32/2/2/3;4523=RemoteAddr/1/1/5,String/1/1/5,http2newBufferedWriter/1/1/6,handler/1/1/7,http2newGoroutineLock/1/1/21;5062=maxHeaderListSize/1/1/3;6660=PseudoValue/1/5/1,PseudoValue/2/5/2,PseudoValue/3/5/3,PseudoValue/4/5/4,PseudoValue/5/5/5;6747=WithContext/1/1/13;7495=hasTrailers/1/1/4;7655=http2asciiEqualFold/2/6/1,http2asciiEqualFold/3/6/2,http2asciiEqualFold/4/6/3,http2asciiEqualFold/5/6/4,http2asciiEqualFold/6/6/5;7670=http2cloneHeader/1/1/4,getErrChan/1/1/5;7771=RequestURI/1/1/4,http2cloneHeader/1/1/5;8704=Now/1/1/18;9037=tooIdleLocked/1/1/5;9380=http2actualContentLength/1/1/6,ContextClientTrace/1/1/7;9784=http2actualContentLength/1/1/7;10602=New/1/1/3;10692=StatusText/1/1/5;12361=Bytes/1/1/2;12983=get/1/1/1;13038=get/1/1/1", "4417:23,5053:21,5056:21,5096:25,5102:25,5109:21,5333:21,8116:27,8123:27,8141:23,8807:21,9102:21,9264:25,9682:29,10390:21", "6853:111,6854:13,11743:66,11744:13")]
[assembly: GoPositionMap("net/http/h2_error.go", "h2_error.cs", "AAoagoKCgpSCgoKClIKCgoKmgoKU")]
[assembly: GoPositionMap("net/http/header.go", "header.cs", "ABs8wgACENIAAhLiAAIQ0qiSgpSqooKssqiSpoKokoK4goKUgoKCpoKUgoKUtgAFEuKCgoKmpu6CAAwazuKCgpSCgoKmgoKssqaigoKUgoKCypSigoKCgoKmgqaCgqaCAAIU4JzCgpSClO6CpoKmgpSCpqaC", "179-179:1")]
[assembly: GoPositionMap("net/http/http.go", "http.cs", "ACRSgJaAloCWgJaAloCUgoKUuKKCgpSClIKUAA0cAA8gmJCYooKUqKSokoKCgqamooKCgpSmgpSCgoKCgpSCgqaClO7YgoKogg==")]
[assembly: GoPositionMap("net/http/mapping.go", "mapping.cs", "ABgykoKUgoKClJS+8oKUgoKUgoKmqsKClIKCgriCgg==")]
[assembly: GoPositionMap("net/http/pattern.go", "pattern.cs", "ACNKlIIALlgAFQKClIKCgriygpSCgpSClJSClIKClIKCgoKmuIKmgpSCgpSClIKClIKClIKmgpSClIKCgpSClIKClIKUgpSClIKm5oKCpoKCpqaCgpSUABA2AA8CuJSCpoKUgpSCAAIUAAkCgpSUlIKUlJSClKrmgqiigoKCgtyCyoKUgpSokoKUgpSClIKUgpSUlIKClKaClAACFgAIApSkpIKUpKSkpMbMopSkpLqygoKCgpSClIL8gpSClKiSgriCgoK8ooKSgoKUpoKCgpSqopSSgoKClIKWyoKCgpSmlIKCpoLKgpSClIK4gpSmpoKClA==", "89-93:1", "", "462=commonPath/1/1/4,differencePath/1/2/4,differencePath/2/2/4", "", "112:52,113:13")]
[assembly: GoPositionMap("net/http/request.go", "request.cs", "ACtSAAkUlpIACBTKuLjKysqWpgDcAfoDAAsCgpQAAhgACQKClIKCggACFgAIAoKUgoKCgoKCgIKCgqSCgqaAgoKCpIKqouqSqJKqooKUuJzCgpSClAACEgAIAoKClAADGAAIAtwAChLCgpSClIIACAaCgoKUgpSCgpSCgpSqoqiSgpQACS4ADgIAAhLiyvoACQKCgpLKgoKClIIABxCCgoKUlIKC7gAMGoKU7pSCgoKUgoKmggAFFMKCgqaCgriCgpSCyoKClIKCgoKClILKgoKUgoKmgoKmgoKCuIKCpoK4goKCgqaClIKCgriAgoLagoKCgpSmgpQADRAACRSClKqigpSCgpSCgpTsspSklIKUgpSClIKClIKClKYADRyokgAGNAAXAriUgpSClIKClIKCpoIAChaClIKCkoLGgoKSgsaCgpKCABQigoK4rLKCgpQABh4AEAKUgpSCgpSCgoKUAAIcAAsCqNKCgoKU2oKCgoKUpqKCAAIS4oKCpoIAChwAEQKClJaSgpSCgriCgoKUgpSCggALGIKCpoKmlLiCgpSCggAJFIKCppSUgoKmlNyUAAYaAAoCgpQACxCUABUWsoKUgtyClJSCgoKmggAAEoiUgqaCpoKCAAkIsoKClKaClIKUgoKCgpSCgoKUlIKClIKCAAwUAAIqABQCgoKClIKmgoKClIKCgoKCpoKUgpSmAAYWAAoCgpSCppSCpoKCpoKCpoKUgpSmlAACHAANAoKUgpQAAhDygpSClKrCgpSCgoKmgoKCpq7CgpSqooKUgpS8xoKUgoKCgpSmpoLmgoKUpoKClKaCgpQACAaCgqTKgqaqooKUgpQAAhTylJQACgqi", "585-589:1;592-599:2;928-931:1;935-938:2;942-945:3;963-963:4;1090-1094:1", "", "1708=Get/2/2/1,EqualFold/1/1/1", "", "600:60,601:17,607:56,608:13,1162:51,1163:13")]
[assembly: GoPositionMap("net/http/response.go", "response.cs", "ABgwAFrKAZLKnMKCgpSClAAIEuKCyoKCgpSUgoKUgpSCgpSCgpSCuIKCgpSUlJSCgqYACBTygoLOogALKAARBIKCgoLKpoK4goKEkoKClKaUggAJGoK4goKUgoK4goLKgoKCyoK4goK4poKCAAMW8oKqoqqiqqI=", "", "", "291=NewReader/1/1/4,MultiReader/1/1/4;378=HeaderValuesContainsToken/1/1/1")]
[assembly: GoPositionMap("net/http/responsecontroller.go", "responsecontroller.cs", "ABNMABECAAoQkoKClKSCpMTOooKClKTEAAgU0oKClKTEAAgW4oKClKTEAAgeAAoCgoKUpMTOog==")]
[assembly: GoPositionMap("net/http/roundtrip.go", "roundtrip.cs", "AAooAAoS8oKU")]
[assembly: GoPositionMap("net/http/routing_index.go", "routing_index.cs", "AB8+ooKUgpSCgoKUAAQcABIOgoKUgoKCpsqClMqe4oKCgoKUgoKCgoKCgriCgoLKgpQ=", "64-75:1")]
[assembly: GoPositionMap("net/http/routing_tree.go", "routing_tree.cs", "ACZY1JSUrNKCgpSCgoKUgoKCgpS8ooKUgqrCgoKUlIKUgoKqwoKUgq7iuIKmrNKClICUpJSCuK7igtyCgpSmyoCCAAgQgoLKpoKUlK7CgpSCgoKUqsKClIKCuIKClJKClA==", "233-238:1")]
[assembly: GoPositionMap("net/http/servemux121.go", "servemux121.cs", "ABcyvKKCggAQJOKClIKUgpSCpoKUgoKCpoK0pIKCloKmgoKCqJKClKjWuIKmyoK4gIK2goKCpqoACAKCpoKUgpSClNq0goLKgoKmrvKCgoKClIKCrLKUgoK4goKUgoK4", "82-84:1")]
[assembly: GoPositionMap("net/http/server.go", "server.cs", "ADBW7tzKALQB7gLcAC140oKC2OKClJSCgpSCgqaClIIAH0iClrKClJSUgoKCgqaCgpSClKaCgpSmgoKUgpSCgrgAQ54BgqaCpoKCABIoooKCgoKUpoKClIKmrLKClJSqoqrCgoIACBgACQKCyoKC7oKCgoK4gqaCgoKCpoKCAA0QkriClAAcPKKCgrigpNKCgqKk0oKCgpSClIKCoqSigoKCABcwpoKUgoKCptKCgoKUgoKClKKkgoIAARgACgKClIKCuMKCgoKUgoKCgpSUgoKUgoKUgpSCgoKClIKClIKCgpSClIIACxakgqaCgpSmgpSklKaCgoKCuKaCgqaCgoKCgoKmpqKCggAIEoKClKaCAAIQ0oLKgpSCpgAOFqKClIKCgoKCgpSUpqKCAAsWAAsG8oKmgoKAgqSAgoLIgpSClIKCgpSmgqaClIKCgpSClIKClIKCuJSCgoKCgsaCxLaUAA0cgpSCguqigriEuKaCuJSCAA0cAAoWgvyigoKCgoKCgpSCpqaigoKClIKCgpSUpu6CpoKClKaClIKmgoKClIIAECLeggABEPKCgoKUgoKClIKCgoKCAAgaAAgCgpSUgriC7oKCgpSCkoKClIKUgpSUpoKCgoKUgqaCgqaCAA8ggoLKgoKCypSCgoKUgqaCABMogqaUpoIAECKCkoKUlMa0pJSCgoKmgoKCuIKUuIKCgqaCuIKmppSCpoKUgoKClIKUgtyCgoK4goKU7oK4gpSCpoLchoK4gpS4goKCqqKCgpSCgpSSggAIEuKClJSCgoKCpryigpQAAkoAIgKmgqiygoKClKaUpoKUgpSCpoKClIKUuKKUgqaCgoKUgtyUgrzCuKaUyoKmgqamgqaCpqKClIKCgpSmgqaCpoKmgrqSggATKAABEOKCggAWLqyylJQABRCigpS0pIKUgoKClIK4goKsoAAIEtycwoKUgIKkgIKkABgQ0oKUgoKCgoKCgpSCgpSCgoKUgriCgoKCgpSIsoKCgpSUgqaCgpSCgoKCuIKU7oKCgriCgqSCgpSAgraCgoKmgqaCgpSUgpSClJ7SgoLuuIKmpoKClIKCyoKClIKClIKCpsqClAAJFIKCgoKCgpSCgoKClJSClMqmgIKU/oKmgIKUABIelKKClIKUAAkUgqaC/qKCgpSCgoKClIKUgpSCgoKCgqYADhqCgqoACQKClIKCpoKCyIKCgpTW0oKCgpSCgoKm1tKCgoKUgoK0AAgSkgAKHgAMAgAIEriCgoKogJiQAAEQ4oKUgoKCgoKCgoKCgpQABCIADgLcgoKCuJSCpoKCuIKCgpS4ypSCgpSmgoK4AAgUggAIEoIAAhLiAIABhAKSyMqSgpSClKaUgpSmqKSClIKClAACLgAWAoKUgu7ygoKClLiCgri4gpiigoKUlIKClIKmuIKCkoKmlAACEgANAoKU3JSCgsqmAAY6ABwCgtyCyoLuqOaCgoKUgpTawoKClIKUgoKUlAACFgAIAoKUAAMS0oKUvKKClLyigpS4goIACwjSgpSClIKmgoLKgoKUpoKUgJKCgqaUpIKCAAUcAAsCggACHgAMAoIAcJoCAA4CgoKC3IKClIKClAAOQAAYApSCgoKkgpSClJSCgpSmgpKCgpS0pAAIFPKCgqrygoKCgriClKaClIKU1oKCgoKmAChWAAcQggAIJAALAoKClIKmpgABFgAJAoKCgoKCgoKUAAgaAAoCgpSCgpSCgpTOou6UggAIEsoAARgADwKCpoKClICCtoKUlIKCgoK4lIKCgoKClIKClJSClIKClJSCgoKCpoKCggAHJAASBoCCtoKUgoKCgoKCuIKmgoLKgsqCpoKCgpSq/IKCkpSClLSClKSUgpSClAACGgAPAoKCgpSCgpSClIKU1tKCgoKUgpS0pIKClKaCgpSmgqaCrsKCgpSmyqKClL7igoKUAAMW8oIAAhDSggAGJAATAoKUgoKmgoKmpNzSggACFgAKAoKmooK4mtKClIKClIKClLiUggACHAALAgAIEgAOGIKClKbSgoKCgqSCgsqC0pKCpoKiksS0gpKCgoKUgrSCkqSCgrSC5gANHJaSgpSmlNKCgoKUgpTWgpSUpIKCxoLI0oKCogASFKKCptqigtyCAAwisJSigoKUgpSClAAeFpaigoKCAAgMsoKCgqaygoKCpqKCgoIACRSygoKClKaigoKClJSqwqSUqJKCgoI=", "979-981:1;1271-1283:1;1896-1915:1;2168-2173:1;2318-2332:1;2685-2688:1;2894-2901:1;3142-3151:1;3306-3317:1;3521-3535:1;3811-3819:1;3812-3816:1.1;4048-4052:1", "", "1122=wantsHttp10KeepAlive/1/1/11,wantsClose/1/1/12;1609=get/1/1/1,hasToken/1/1/1,isProtocolSwitchResponse/1/1/2;4398=Sprintf/1/1/1", "2115:21,3397:21,3662:21,4012:21,4136:25", "1053:67,1054:21,2039:95,2040:13,4153:77,4154:21")]
[assembly: GoPositionMap("net/http/sniff.go", "sniff.cs", "AA4q4oK4gqaCgriqopSUqqKUlAAfEgA5kAKCgpQACBK4gpSClIKUgoKCpsqCgoKUgoKClIK4gpSmggAICKaClIKClIKUgpSUgqbalIKapg==")]
[assembly: GoPositionMap("net/http/socks_bundle.go", "socks_bundle.cs", "ABMuggANBuKCgpSAgoKkgoKCooKCpqK0grTagoKClIKClIKCpoKmgpSClIKClIKCuIKCgoKCgoKClKaClIKClIKCpoKUgpSClIKUgoKUgrSCtIKUtJSClJSClIKUlILmgoKClIKClIKUAAsMgpSkpAAdFIKUpKSkpKSkpKSkAB881KKClIKClAAIGMKClAAYRAAMAoKClIKClIKCgpSClIKClIKCgoKUAAISAAgCgoKUgoKUgoKClAACEgAIAoKClIKCgpSUgoKUgoKUAAgGgsaUxpSmwoKSgpSCgoKUgpSmqqIAFCKilKSClIKCgoKmgIKkgIKkgpSClJQ=", "39-44:1;45-53:2", "", "", "", "57:66,58:17")]
[assembly: GoPositionMap("net/http/status.go", "status.cs", "AMkBogGilKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSk")]
[assembly: GoPositionMap("net/http/transfer.go", "transfer.cs", "ACE+3IIABxCCgpSClIKCABUuoqaClIKUgoKCgoKCgoKCAAgSgqa0goKUgoKCgoKCgoLIgoKCpoKUgoKCyoKmAAIqABYGgpSClLiC3AACJAARAoKCgoKCgpSClIK0gpSCgoKClJSCAAgOuMiCpoKClIKUgqaClIKClKYADAaigoKUgu6CgIKkgIKkgpSCgpSCyoKCooKUlJSCpoKUgsqo4oKCgoKUggAHEIKCgoKUgoKClIKCgpSUgoKUgoKUgqaCgoK4griUgoK4lN4ACgKClIKClAAFENKAgqSAgoKkABEkgqqilKSklKiCgpaCpqSUqLKmgpSCgoKCgoKCxoKCgqaCxMiCuIKmgoKUgoKUpriCgtyUlOyUgpTGtMaUptqUgoKClIK0goKClIK2qICWgO6CqqKCqJKCgpSmgu6ClIKmgqzigqbKgoKCyoKUuIKCgsqClIKUlAAPIIKCuIKmlAAHELissoKmgoKCpoKm6JKCgpQABxCUlIKCgpKClIKCtqaClIKUABYwlOKClIKCgpTowoKUlIKUgoLKgpS4ggAJFIKCgriCpqiClqKmgoKUgqam1JSCgoKUgpSCAAoWgqaCgoKUlJS0pKaCgoKUrLKClKbSgpSCgoKUggAMEqaUpoKCgoKm/KSC1tKCgtrygpSCgtbSgpSCgqIACBCCgpSm+rKClLiCgoKUlIKClAAIErKClIKCgpSClKbCnqKUpL6ytJSClIKUAAgOsoKCgoKm", "210-219:1;341-348:1;787-797:1", "", "", "", "380:67,381:13")]
[assembly: GoPositionMap("net/http/transport.go", "transport.cs", "ADVcAO8BmgSCgpSmgoKUpoKClKiyggAWLoKUgoKUgoKUgoKClJQADBqCppjCgoKCAAcQgoKCgoLKlJSCgpSClIKCgpQABxCCgoKUAAgSpoKClIKCuIIAEBakAAImABICqqIADyiCgpSmooKClKrCypSs4oKUgqaCgoKUgqa46AAIAoKClIKClIKClIKClIKCuIKCuIKUgoKUgoKCpoKClIKClIKCAAsYpoLulJKCuIKkguqCgoKC7oKCgqaClJSUgtyUgriCgpSmgIKkgIKkgLiklKaCggAICoK0+gAHEIKCpqKClK7igpSCgq7ygpSClIKUgoKUgoKs0u6UlJQABxCUppSUlKaUuJS4AAEYAA8CgoKCgpSCgpSCoqzigoKCgoKCgoKCpoKCgqaCgroACQ6CgoKClIKClIKCAAISAAgCgoKCggAKHqKClKiSgqbCgoKClIKqooKUgoKClLqCgoKCgoKC3AANHpSCpoKCuIKClAACEAAKAoKUgpSClIKCgoKUgtakpoLKgu6CgoKmgoKCggAHEIKCpoKUlIK4gpSClIKClIKCpoKCgoKC3IKClKaC3AAJAoKmgriUlKyygriAgoKCgsqCuKTcgpSCgtyCppSClJSC2oKUgoKCgujygpSCgtiygpSCgoKC2IKCxoKCuIKCgrYACwqCgoKClJSCgoKUlAAdQuKClNjigpTY4oKUgpSClIKUgpTawoKCgoKmlIKCyoIAFCySqJKokoKCppSCgoKokoKUgpSqwoKCgpSCupKCgoKUvKKClIL4soKUlIKUrgAKAoKCgoIABxCUAAgSkoLKgrjYgriCgpSUuKSCgpTqpIKClPwACAKUgpSCgqaCgpSCgqaClIKCgqKowoKCooKCgqK88oKCgoKmgoKCuJSCtKjygqaCgoKm7oKCgoKCgoKmgriUgsqClLSq5IKClIKUgoKCgoCCkraygpSCgpSUgIKCppSClKSCgpSCgu4ACwTiAAoWgoKUlJSCgoKClICmgpSAgoKClKSCgpS2goKUgoKCgpSC3NiCgoCCpoK4tIKCxoKAgoLogoKCgoKCgqaUgpSAgoKkAAkUgqSCqtKCgoK4gqKStIKC2IKCpoKCgoK4goKCgpTIgoLKmoKCgoKUgoKClKaCgoKUgpSUpoKCgpSUuIKUgoIAEBqygoKs4oKCpgAcOoKCgoKCgqYABxKSgpSokoKUqqKCgpQAChaEkoKUAC9qgqaygpSClIKClIKosoKCgqrygoLYsoKCgqbSgoKCoqrygoKSlJSCogACFAALAoIACRTKgriCgoKCppSmgoKmlIKClJTclMKCgoKCgriCgoKCgpSUgpTcgqaCgpSCgoKUgoKCgpSUgpSCgpSCpoKCpuaUlJSCgpSCgpQABxCm3JqCpuamyoKmgvaCgqiCgoKCgoKmuIKCgoKCgqbm3MSKgsaCtLaCtKSCgpSCgoKClKaUlL6ygpSClKzygoK4goKCgpSCgoKUgpSUgoKCggAIEpSUlIKUAAwagpS4gqyygpSygqTEpKQACAqigoKUABUcsoKClIKClJTmgoKUAAcQgqbCgoK0goKCAAcQlIKUgoKmgoKCgsYAEB7iyAAMGIKStKQANGiCgoKUyIK4lKiSgpKClIKWgsSkpAAICPKUgoKCgoKUlIKUgoKUgu6CAAwggqaCgqaGpoKUyoKClIIABxCCgpSClIKUpoKCgoKC5IKUgoKUgIKClIKS1trGgpSkgpSCpKTaxgAOEqKCvMKCggACEAAKAoKCoqSCgpSCgqaCgpSmpoKUpKSkuKKCgpSosoKClAAWLoKU4oKCgoKUgqaCgoKCgpSU1tKCgoKUgoKUgtiSgpSCggARIoKUppKCgoKUqLaigqrygoKClIKCgqaCgtjygoKClLSo8oKCgoKUoqTCgoKUlOailNqCwv6CAAEeAA0CgpQABxKSgoKUgoKUpoKCgoKCqJKCgrqS", "514-516:1;667-671:1;911-915:1;930-935:1;972-974:1;1540-1544:1;1628-1633:1;1728-1730:1;1732-1741:2;1782-1788:1;1856-1858:2;1899-1909:3;2293-2299:1;2301-2314:2;2417-2422:3;2423-2434:4;2590-2602:1;2898-2909:1", "", "54=defaultTransportDialContext/1/1/2;320=Clone/1/1/16;1666=key/1/1/2;1695=isReused/1/1/2;1973=key/1/1/2;2049=Username/1/1/1;2180=UnencryptedHTTP2/1/1/2,HTTP1/1/1/3;2666=wroteRequest/1/1/2,Invoke/1/1/3;2745=wroteRequest/1/1/3,Invoke/1/1/4;3213=Get/1/2/1,Get/2/2/2;3235=wantsClose/1/1/1,isProtocolSwitchHeader/1/1/2", "664:33,2007:39,2112:25,2124:33,2592:21,2892:25,2985:21,3065:25,3240:21,3299:37", "674:62,675:13,1676:60,1677:13,2565:63,2566:13")]
[assembly: GoPositionMap("net/http/transport_default_other.go", "transport_default_other.cs", "AAscgg==")]
// </GoSourcePositionMaps>

namespace go.net;

using static go.net.http_package;
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using context = go.context_package;
using fs = go.io.fs_package;
using gzip = go.compress.gzip_package;
using hpack = go.vendor.golang.org.x.net.http2.hpack_package;
using httpcommon = go.net.http.@internal.httpcommon_package;
using httptrace = go.net.http.httptrace_package;
using io = go.io_package;
using multipart = go.mime.multipart_package;
using net = go.net_package;
using textproto = go.net.textproto_package;
using time = go.time_package;
using tls = go.crypto.tls_package;
using url = go.net.url_package;
using urlpkg = go.net.url_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸhttp2Setting = Span<http_package.http2Setting>;

[GoPackage("http")]
public static partial class http_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("closeIdler")] internal partial interface CloseIdleConnections_closeIdler {}
    internal partial interface CloseWrite_type {}
    internal partial interface EnableFullDuplex_type {}
    internal partial interface Flush_type {}
    [GoLocalName("requestTooLarger")] internal partial interface Read_requestTooLarger {}
    internal partial interface SetReadDeadline_type {}
    internal partial interface SetWriteDeadline_type {}
    internal partial interface anyDirs {}
    internal partial interface closeWriter {}
    internal partial interface connectionStater {}
    internal partial interface erringRoundTripper {}
    internal partial interface genericClientConn {}
    internal partial interface h2Transport {}
    [GoLocalName("baseContexter")] internal partial interface http2ConfigureServer_baseContexter {}
    internal partial interface http2Frame {}
    internal partial interface http2clientConnPoolIdleCloser {}
    internal partial interface http2connectionStater {}
    [GoLocalName("I")] internal partial interface http2h1ServerKeepAlivesDisabled_I {}
    internal partial interface http2headersEnder {}
    internal partial interface http2headersOrContinuation {}
    internal partial interface http2isNoCachedConnError_type {}
    internal partial interface http2pipeBuffer {}
    internal partial interface http2streamEnder {}
    internal partial interface http2stringWriter {}
    internal partial interface http2unencryptedNetConnFromTLSConn_type {}
    internal partial interface http2writeContext {}
    internal partial interface http2writeFramer {}
    internal partial interface newClientConner {}
    internal partial interface rwUnwrapper {}
    [GoLocalName("canceler")] internal partial interface setRequestCancel_canceler {}
    internal partial interface sniffSig {}
    internal partial struct Write_r1 {}
    internal partial struct body {}
    internal partial struct bodyEOFSignal {}
    internal partial struct bodyLocked {}
    internal partial struct bufioFlushWriter {}
    internal partial struct byteReader {}
    internal partial struct cancelTimerBody {}
    internal partial struct checkConnErrorWriter {}
    internal partial struct chunkWriter {}
    internal partial struct condResult {}
    internal partial struct conn {}
    internal partial struct connLRU {}
    internal partial struct connOrError {}
    [GoValueClone("byteBuf")] internal partial struct connReader {}
    internal partial struct connectMethod {}
    internal partial struct connectMethodKey {}
    internal partial struct countingWriter {}
    internal partial struct dirEntryDirs {}
    internal partial struct entry<K, V> {}
    internal partial struct eofReader {}
    internal partial struct errorReader {}
    internal partial struct exactSig {}
    internal partial struct expectContinueReader {}
    internal partial struct extraHeader {}
    internal partial struct fakeLocker {}
    internal partial struct fileHandler {}
    internal partial struct fileInfoDirs {}
    internal partial struct fileTransport {}
    internal partial struct finishAsyncByteRead {}
    internal partial struct globalOptionsHandler {}
    internal partial struct gzipReader {}
    internal partial struct headerSorter {}
    internal partial struct htmlSig {}
    internal partial struct http1ClientConn {}
    internal partial struct http2ConnectionError {}
    internal partial struct http2ContinuationFrame {}
    internal partial struct http2DataFrame {}
    internal partial struct http2FrameHeader {}
    [GoValueClone("headerBuf")] internal partial struct http2Framer {}
    internal partial struct http2GoAwayError {}
    internal partial struct http2GoAwayFrame {}
    internal partial struct http2HeadersFrame {}
    internal partial struct http2HeadersFrameParam {}
    internal partial struct http2MetaHeadersFrame {}
    [GoValueClone("Data")] internal partial struct http2PingFrame {}
    internal partial struct http2PriorityFrame {}
    internal partial struct http2PriorityWriteSchedulerConfig {}
    internal partial struct http2PushPromiseFrame {}
    internal partial struct http2PushPromiseParam {}
    internal partial struct http2RSTStreamFrame {}
    internal partial struct http2RoundTripOpt {}
    internal partial struct http2ServeConnOpts {}
    internal partial struct http2Server {}
    internal partial struct http2Setting {}
    internal partial struct http2SettingsFrame {}
    internal partial struct http2StreamError {}
    internal partial struct http2Transport {}
    internal partial struct http2UnknownFrame {}
    internal partial struct http2WindowUpdateFrame {}
    internal partial struct http2addConnCall {}
    internal partial struct http2bodyReadMsg {}
    internal partial struct http2bufferedWriter {}
    internal partial struct http2bufferedWriterTimeoutWriter {}
    internal partial struct http2chunkWriter {}
    internal partial struct http2clientConnIdleState {}
    internal partial struct http2clientConnPool {}
    internal partial struct http2clientConnReadLoop {}
    internal partial struct http2clientStream {}
    internal partial struct http2closeWaiter {}
    internal partial struct http2connError {}
    internal partial struct http2connOutflow {}
    internal partial struct http2dataBuffer {}
    internal partial struct http2dialCall {}
    internal partial struct http2duplicatePseudoHeaderError {}
    internal partial struct http2eofReader {}
    internal partial struct http2erringRoundTripper {}
    internal partial struct http2errorReader {}
    internal partial struct http2flushFrameWriter {}
    internal partial struct http2frameCache {}
    internal partial struct http2frameWriteResult {}
    internal partial struct http2goAwayFlowError {}
    internal partial struct http2goroutineLock {}
    internal partial struct http2gzipReader {}
    internal partial struct http2handlerPanicRST {}
    internal partial struct http2headerFieldNameError {}
    internal partial struct http2headerFieldValueError {}
    internal partial struct http2http2Config {}
    internal partial struct http2httpError {}
    internal partial struct http2incomparable {}
    internal partial struct http2inflow {}
    internal partial struct http2missingBody {}
    internal partial struct http2netHTTPClientConn {}
    internal partial struct http2noBodyReader {}
    internal partial struct http2noCachedConnError {}
    internal partial struct http2noDialClientConnPool {}
    internal partial struct http2noDialH2RoundTripper {}
    internal partial struct http2outflow {}
    internal partial struct http2pipe {}
    internal partial struct http2priorityNodeRFC7540 {}
    internal partial struct http2priorityNodeStateRFC7540 {}
    internal partial struct http2priorityWriteSchedulerRFC7540 {}
    [GoValueClone("heads")] internal partial struct http2priorityWriteSchedulerRFC9218 {}
    internal partial struct http2pseudoHeaderError {}
    internal partial struct http2randomWriteScheduler {}
    internal partial struct http2readFrameResult {}
    internal partial struct http2requestBody {}
    internal partial struct http2resAndError {}
    internal partial struct http2responseWriter {}
    internal partial struct http2responseWriterState {}
    internal partial struct http2roundRobinWriteScheduler {}
    [GoValueClone("sentPingData")] internal partial struct http2serverConn {}
    internal partial struct http2serverInternalState {}
    internal partial struct http2serverMessage {}
    internal partial struct http2sortPriorityNodeSiblingsRFC7540 {}
    internal partial struct http2sorter {}
    internal partial struct http2startPushRequest {}
    internal partial struct http2stickyErrWriter {}
    internal partial struct http2stream {}
    internal partial struct http2streamMetadata {}
    internal partial struct http2streamState {}
    internal partial struct http2transportResponseBody {}
    internal partial struct http2transportTestHooks {}
    internal partial struct http2unencryptedTransport {}
    internal partial struct http2unstartedHandler {}
    internal partial struct http2write100ContinueHeadersFrame {}
    internal partial struct http2writeData {}
    internal partial struct http2writeGoAway {}
    [GoValueClone("data")] internal partial struct http2writePing {}
    internal partial struct http2writePingAck {}
    internal partial struct http2writePushPromise {}
    internal partial struct http2writeQueue {}
    internal partial struct http2writeQueuePool {}
    internal partial struct http2writeResHeaders {}
    internal partial struct http2writeSettings {}
    internal partial struct http2writeSettingsAck {}
    internal partial struct http2writeWindowUpdate {}
    internal partial struct httpRange {}
    internal partial struct incomparable {}
    internal partial struct initALPNRequest {}
    internal partial struct ioFS {}
    internal partial struct ioFile {}
    internal partial struct keyValues {}
    internal partial struct loggingConn {}
    internal partial struct mapping<K, V> {}
    internal partial struct maskedSig {}
    internal partial struct maxBytesReader {}
    internal partial struct mp4Sig {}
    internal partial struct muxEntry {}
    internal partial struct noopHandler {}
    internal partial struct nothingWrittenError {}
    internal partial struct onceCloseListener {}
    internal partial struct pattern {}
    internal partial struct persistConn {}
    internal partial struct persistConnWriter {}
    internal partial struct populateResponse {}
    internal partial struct readTrackingBody {}
    internal partial struct readWriteCloserBody {}
    internal partial struct redirectHandler {}
    internal partial struct relationship {}
    internal partial struct requestAndChan {}
    internal partial struct requestBodyReadError {}
    [GoValueClone("dateBuf", "clenBuf", "statusBuf")] internal partial struct response {}
    internal partial struct responseAndError {}
    internal partial struct routingIndex {}
    internal partial struct routingIndexKey {}
    internal partial struct routingNode {}
    internal partial struct rᴛ1 {}
    internal partial struct segment {}
    internal partial struct serveMux121 {}
    internal partial struct serverHandler {}
    internal partial struct socksAddr {}
    internal partial struct socksCommand {}
    internal partial struct socksConn {}
    internal partial struct socksDialer {}
    internal partial struct socksReply {}
    internal partial struct socksUsernamePassword {}
    internal partial struct statusError {}
    internal partial struct stringWriter {}
    internal partial struct tLogKey {}
    internal partial struct textSig {}
    internal partial struct timeoutError {}
    internal partial struct timeoutHandler {}
    internal partial struct timeoutWriter {}
    internal partial struct tlsHandshakeTimeoutError {}
    internal partial struct transferReader {}
    internal partial struct transferWriter {}
    internal partial struct transportReadFromServerError {}
    internal partial struct transportRequest {}
    internal partial struct unencryptedHTTP2Request {}
    internal partial struct unencryptedNetConnInTLSConn {}
    internal partial struct unsupportedTEError {}
    internal partial struct wantConn {}
    internal partial struct wantConnQueue {}
    internal partial struct writeRequest {}
    internal partial struct writerOnly {}
    public partial interface CloseNotifier {}
    public partial interface CookieJar {}
    public partial interface File {}
    public partial interface FileSystem {}
    public partial interface Flusher {}
    public partial interface Handler {}
    public partial interface Hijacker {}
    public partial interface Pusher {}
    public partial interface ResponseWriter {}
    public partial interface RoundTripper {}
    public partial interface http2ClientConnPool {}
    public partial interface http2WriteScheduler {}
    public partial struct Client {}
    public partial struct ClientConn {}
    public partial struct ConnState {}
    public partial struct Cookie {}
    public partial struct CrossOriginProtection {}
    public partial struct Dir {}
    public partial struct HTTP2Config {}
    public partial struct Header {}
    public partial struct MaxBytesError {}
    public partial struct ProtocolError {}
    public partial struct Protocols {}
    public partial struct PushOptions {}
    public partial struct Request {}
    public partial struct Response {}
    public partial struct ResponseController {}
    public partial struct SameSite {}
    public partial struct ServeMux {}
    public partial struct Server {}
    public partial struct Transport {}
    public partial struct contextKey {}
    public partial struct http2ClientConn {}
    public partial struct http2ClientConnState {}
    public partial struct http2ErrCode {}
    public partial struct http2Flags {}
    public partial struct http2FrameType {}
    public partial struct http2FrameWriteRequest {}
    public partial struct http2OpenStreamOptions {}
    public partial struct http2PriorityParam {}
    public partial struct http2SettingID {}
    public partial struct noBody {}
    public partial struct readResult {}
    public partial struct socksAuthMethod {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸflate() => builtin.initPackage(typeof(compress.flate_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸgzip() => builtin.initPackage(typeof(compress.gzip_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸtls() => builtin.initPackage(typeof(crypto.tls_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(go.@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸmime() => builtin.initPackage(typeof(mime_package));
    [GoInit] internal static void initᴛᴛimportꓸmimeꓸmultipart() => builtin.initPackage(typeof(go.mime.multipart_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸhttptrace() => builtin.initPackage(typeof(go.net.http.httptrace_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸinternal() => builtin.initPackage(typeof(go.net.http.internal_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸinternalꓸascii() => builtin.initPackage(typeof(go.net.http.@internal.ascii_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸinternalꓸhttpcommon() => builtin.initPackage(typeof(go.net.http.@internal.httpcommon_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸtextproto() => builtin.initPackage(typeof(go.net.textproto_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(go.net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸhttp2ꓸhpack() => builtin.initPackage(typeof(vendor.golang.org.x.net.http2.hpack_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸhttpꓸhttpguts() => builtin.initPackage(typeof(vendor.golang.org.x.net.http.httpguts_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸhttpꓸhttpproxy() => builtin.initPackage(typeof(vendor.golang.org.x.net.http.httpproxy_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸidna() => builtin.initPackage(typeof(vendor.golang.org.x.net.idna_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(http_package.Client)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛClient;
[GoMethodHost(typeof(http_package.ClientConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛClientConn;
[GoMethodHost(typeof(http_package.ConnState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛConnState;
[GoMethodHost(typeof(http_package.Cookie)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛCookie;
[GoMethodHost(typeof(http_package.CrossOriginProtection)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛCrossOriginProtection;
[GoMethodHost(typeof(http_package.Dir)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛDir;
[GoMethodHost(typeof(http_package.HandlerFunc)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛHandlerFunc;
[GoMethodHost(typeof(http_package.Header)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛHeader;
[GoMethodHost(typeof(http_package.MaxBytesError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛMaxBytesError;
[GoMethodHost(typeof(http_package.ProtocolError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛProtocolError;
[GoMethodHost(typeof(http_package.Protocols)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛProtocols;
[GoMethodHost(typeof(http_package.Request)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛRequest;
[GoMethodHost(typeof(http_package.Response)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛResponse;
[GoMethodHost(typeof(http_package.ResponseController)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛResponseController;
[GoMethodHost(typeof(http_package.ServeMux)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛServeMux;
[GoMethodHost(typeof(http_package.Server)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛServer;
[GoMethodHost(typeof(http_package.Transport)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛTransport;
[GoMethodHost(typeof(http_package.body)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛbody;
[GoMethodHost(typeof(http_package.bodyEOFSignal)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛbodyEOFSignal;
[GoMethodHost(typeof(http_package.bodyLocked)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛbodyLocked;
[GoMethodHost(typeof(http_package.bufioFlushWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛbufioFlushWriter;
[GoMethodHost(typeof(http_package.byteReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛbyteReader;
[GoMethodHost(typeof(http_package.cancelTimerBody)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛcancelTimerBody;
[GoMethodHost(typeof(http_package.checkConnErrorWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛcheckConnErrorWriter;
[GoMethodHost(typeof(http_package.chunkWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛchunkWriter;
[GoMethodHost(typeof(http_package.conn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛconn;
[GoMethodHost(typeof(http_package.connLRU)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛconnLRU;
[GoMethodHost(typeof(http_package.connReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛconnReader;
[GoMethodHost(typeof(http_package.connectMethod)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛconnectMethod;
[GoMethodHost(typeof(http_package.connectMethodKey)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛconnectMethodKey;
[GoMethodHost(typeof(http_package.contextKey)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛcontextKey;
[GoMethodHost(typeof(http_package.countingWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛcountingWriter;
[GoMethodHost(typeof(http_package.dirEntryDirs)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛdirEntryDirs;
[GoMethodHost(typeof(http_package.eofReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛeofReader;
[GoMethodHost(typeof(http_package.errorReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛerrorReader;
[GoMethodHost(typeof(http_package.exactSig)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛexactSig;
[GoMethodHost(typeof(http_package.expectContinueReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛexpectContinueReader;
[GoMethodHost(typeof(http_package.extraHeader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛextraHeader;
[GoMethodHost(typeof(http_package.fakeLocker)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛfakeLocker;
[GoMethodHost(typeof(http_package.fileHandler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛfileHandler;
[GoMethodHost(typeof(http_package.fileInfoDirs)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛfileInfoDirs;
[GoMethodHost(typeof(http_package.fileTransport)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛfileTransport;
[GoMethodHost(typeof(http_package.finishAsyncByteRead)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛfinishAsyncByteRead;
[GoMethodHost(typeof(http_package.globalOptionsHandler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛglobalOptionsHandler;
[GoMethodHost(typeof(http_package.gzipReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛgzipReader;
[GoMethodHost(typeof(http_package.htmlSig)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhtmlSig;
[GoMethodHost(typeof(http_package.http1ClientConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp1ClientConn;
[GoMethodHost(typeof(http_package.http2ClientConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2ClientConn;
[GoMethodHost(typeof(http_package.http2ConnectionError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2ConnectionError;
[GoMethodHost(typeof(http_package.http2ContinuationFrame)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2ContinuationFrame;
[GoMethodHost(typeof(http_package.http2DataFrame)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2DataFrame;
[GoMethodHost(typeof(http_package.http2ErrCode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2ErrCode;
[GoMethodHost(typeof(http_package.http2Flags)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2Flags;
[GoMethodHost(typeof(http_package.http2FrameHeader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2FrameHeader;
[GoMethodHost(typeof(http_package.http2FrameType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2FrameType;
[GoMethodHost(typeof(http_package.http2FrameWriteRequest)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2FrameWriteRequest;
[GoMethodHost(typeof(http_package.http2Framer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2Framer;
[GoMethodHost(typeof(http_package.http2GoAwayError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2GoAwayError;
[GoMethodHost(typeof(http_package.http2GoAwayFrame)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2GoAwayFrame;
[GoMethodHost(typeof(http_package.http2HeadersFrame)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2HeadersFrame;
[GoMethodHost(typeof(http_package.http2MetaHeadersFrame)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2MetaHeadersFrame;
[GoMethodHost(typeof(http_package.http2PingFrame)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2PingFrame;
[GoMethodHost(typeof(http_package.http2PriorityParam)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2PriorityParam;
[GoMethodHost(typeof(http_package.http2PushPromiseFrame)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2PushPromiseFrame;
[GoMethodHost(typeof(http_package.http2ServeConnOpts)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2ServeConnOpts;
[GoMethodHost(typeof(http_package.http2Server)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2Server;
[GoMethodHost(typeof(http_package.http2Setting)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2Setting;
[GoMethodHost(typeof(http_package.http2SettingID)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2SettingID;
[GoMethodHost(typeof(http_package.http2SettingsFrame)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2SettingsFrame;
[GoMethodHost(typeof(http_package.http2StreamError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2StreamError;
[GoMethodHost(typeof(http_package.http2Transport)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2Transport;
[GoMethodHost(typeof(http_package.http2UnknownFrame)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2UnknownFrame;
[GoMethodHost(typeof(http_package.http2addConnCall)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2addConnCall;
[GoMethodHost(typeof(http_package.http2bufferedWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2bufferedWriter;
[GoMethodHost(typeof(http_package.http2bufferedWriterTimeoutWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2bufferedWriterTimeoutWriter;
[GoMethodHost(typeof(http_package.http2chunkWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2chunkWriter;
[GoMethodHost(typeof(http_package.http2clientConnPool)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2clientConnPool;
[GoMethodHost(typeof(http_package.http2clientConnReadLoop)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2clientConnReadLoop;
[GoMethodHost(typeof(http_package.http2clientStream)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2clientStream;
[GoMethodHost(typeof(http_package.http2closeWaiter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2closeWaiter;
[GoMethodHost(typeof(http_package.http2connError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2connError;
[GoMethodHost(typeof(http_package.http2connOutflow)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2connOutflow;
[GoMethodHost(typeof(http_package.http2dataBuffer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2dataBuffer;
[GoMethodHost(typeof(http_package.http2dialCall)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2dialCall;
[GoMethodHost(typeof(http_package.http2duplicatePseudoHeaderError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2duplicatePseudoHeaderError;
[GoMethodHost(typeof(http_package.http2eofReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2eofReader;
[GoMethodHost(typeof(http_package.http2erringRoundTripper)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2erringRoundTripper;
[GoMethodHost(typeof(http_package.http2errorReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2errorReader;
[GoMethodHost(typeof(http_package.http2flushFrameWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2flushFrameWriter;
[GoMethodHost(typeof(http_package.http2frameCache)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2frameCache;
[GoMethodHost(typeof(http_package.http2goAwayFlowError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2goAwayFlowError;
[GoMethodHost(typeof(http_package.http2goroutineLock)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2goroutineLock;
[GoMethodHost(typeof(http_package.http2gzipReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2gzipReader;
[GoMethodHost(typeof(http_package.http2handlerPanicRST)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2handlerPanicRST;
[GoMethodHost(typeof(http_package.http2headerFieldNameError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2headerFieldNameError;
[GoMethodHost(typeof(http_package.http2headerFieldValueError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2headerFieldValueError;
[GoMethodHost(typeof(http_package.http2httpError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2httpError;
[GoMethodHost(typeof(http_package.http2inflow)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2inflow;
[GoMethodHost(typeof(http_package.http2missingBody)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2missingBody;
[GoMethodHost(typeof(http_package.http2netHTTPClientConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2netHTTPClientConn;
[GoMethodHost(typeof(http_package.http2noBodyReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2noBodyReader;
[GoMethodHost(typeof(http_package.http2noCachedConnError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2noCachedConnError;
[GoMethodHost(typeof(http_package.http2noDialClientConnPool)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2noDialClientConnPool;
[GoMethodHost(typeof(http_package.http2noDialH2RoundTripper)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2noDialH2RoundTripper;
[GoMethodHost(typeof(http_package.http2outflow)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2outflow;
[GoMethodHost(typeof(http_package.http2pipe)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2pipe;
[GoMethodHost(typeof(http_package.http2priorityNodeRFC7540)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2priorityNodeRFC7540;
[GoMethodHost(typeof(http_package.http2priorityWriteSchedulerRFC7540)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2priorityWriteSchedulerRFC7540;
[GoMethodHost(typeof(http_package.http2priorityWriteSchedulerRFC9218)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2priorityWriteSchedulerRFC9218;
[GoMethodHost(typeof(http_package.http2pseudoHeaderError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2pseudoHeaderError;
[GoMethodHost(typeof(http_package.http2randomWriteScheduler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2randomWriteScheduler;
[GoMethodHost(typeof(http_package.http2requestBody)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2requestBody;
[GoMethodHost(typeof(http_package.http2responseWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2responseWriter;
[GoMethodHost(typeof(http_package.http2responseWriterState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2responseWriterState;
[GoMethodHost(typeof(http_package.http2roundRobinWriteScheduler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2roundRobinWriteScheduler;
[GoMethodHost(typeof(http_package.http2serverConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2serverConn;
[GoMethodHost(typeof(http_package.http2serverInternalState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2serverInternalState;
[GoMethodHost(typeof(http_package.http2sortPriorityNodeSiblingsRFC7540)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2sortPriorityNodeSiblingsRFC7540;
[GoMethodHost(typeof(http_package.http2sorter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2sorter;
[GoMethodHost(typeof(http_package.http2stickyErrWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2stickyErrWriter;
[GoMethodHost(typeof(http_package.http2stream)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2stream;
[GoMethodHost(typeof(http_package.http2streamState)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2streamState;
[GoMethodHost(typeof(http_package.http2transportResponseBody)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2transportResponseBody;
[GoMethodHost(typeof(http_package.http2unencryptedTransport)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2unencryptedTransport;
[GoMethodHost(typeof(http_package.http2write100ContinueHeadersFrame)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2write100ContinueHeadersFrame;
[GoMethodHost(typeof(http_package.http2writeData)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2writeData;
[GoMethodHost(typeof(http_package.http2writeGoAway)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2writeGoAway;
[GoMethodHost(typeof(http_package.http2writePing)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2writePing;
[GoMethodHost(typeof(http_package.http2writePingAck)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2writePingAck;
[GoMethodHost(typeof(http_package.http2writePushPromise)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2writePushPromise;
[GoMethodHost(typeof(http_package.http2writeQueue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2writeQueue;
[GoMethodHost(typeof(http_package.http2writeQueuePool)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2writeQueuePool;
[GoMethodHost(typeof(http_package.http2writeResHeaders)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2writeResHeaders;
[GoMethodHost(typeof(http_package.http2writeSettings)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2writeSettings;
[GoMethodHost(typeof(http_package.http2writeSettingsAck)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2writeSettingsAck;
[GoMethodHost(typeof(http_package.http2writeWindowUpdate)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttp2writeWindowUpdate;
[GoMethodHost(typeof(http_package.httpRange)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛhttpRange;
[GoMethodHost(typeof(http_package.initALPNRequest)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛinitALPNRequest;
[GoMethodHost(typeof(http_package.ioFS)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛioFS;
[GoMethodHost(typeof(http_package.ioFile)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛioFile;
[GoMethodHost(typeof(http_package.loggingConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛloggingConn;
[GoMethodHost(typeof(http_package.mapping<,>)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛmapping;
[GoMethodHost(typeof(http_package.maskedSig)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛmaskedSig;
[GoMethodHost(typeof(http_package.maxBytesReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛmaxBytesReader;
[GoMethodHost(typeof(http_package.mp4Sig)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛmp4Sig;
[GoMethodHost(typeof(http_package.noBody)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛnoBody;
[GoMethodHost(typeof(http_package.noopHandler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛnoopHandler;
[GoMethodHost(typeof(http_package.nothingWrittenError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛnothingWrittenError;
[GoMethodHost(typeof(http_package.onceCloseListener)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛonceCloseListener;
[GoMethodHost(typeof(http_package.pattern)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛpattern;
[GoMethodHost(typeof(http_package.persistConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛpersistConn;
[GoMethodHost(typeof(http_package.persistConnWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛpersistConnWriter;
[GoMethodHost(typeof(http_package.populateResponse)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛpopulateResponse;
[GoMethodHost(typeof(http_package.readTrackingBody)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛreadTrackingBody;
[GoMethodHost(typeof(http_package.readWriteCloserBody)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛreadWriteCloserBody;
[GoMethodHost(typeof(http_package.redirectHandler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛredirectHandler;
[GoMethodHost(typeof(http_package.response)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛresponse;
[GoMethodHost(typeof(http_package.routingIndex)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛroutingIndex;
[GoMethodHost(typeof(http_package.routingNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛroutingNode;
[GoMethodHost(typeof(http_package.serveMux121)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛserveMux121;
[GoMethodHost(typeof(http_package.serverHandler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛserverHandler;
[GoMethodHost(typeof(http_package.socksAddr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛsocksAddr;
[GoMethodHost(typeof(http_package.socksCommand)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛsocksCommand;
[GoMethodHost(typeof(http_package.socksConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛsocksConn;
[GoMethodHost(typeof(http_package.socksDialer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛsocksDialer;
[GoMethodHost(typeof(http_package.socksReply)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛsocksReply;
[GoMethodHost(typeof(http_package.socksUsernamePassword)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛsocksUsernamePassword;
[GoMethodHost(typeof(http_package.statusError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛstatusError;
[GoMethodHost(typeof(http_package.stringWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛstringWriter;
[GoMethodHost(typeof(http_package.textSig)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛtextSig;
[GoMethodHost(typeof(http_package.timeoutError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛtimeoutError;
[GoMethodHost(typeof(http_package.timeoutHandler)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛtimeoutHandler;
[GoMethodHost(typeof(http_package.timeoutWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛtimeoutWriter;
[GoMethodHost(typeof(http_package.tlsHandshakeTimeoutError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛtlsHandshakeTimeoutError;
[GoMethodHost(typeof(http_package.transferReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛtransferReader;
[GoMethodHost(typeof(http_package.transferWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛtransferWriter;
[GoMethodHost(typeof(http_package.transportReadFromServerError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛtransportReadFromServerError;
[GoMethodHost(typeof(http_package.transportRequest)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛtransportRequest;
[GoMethodHost(typeof(http_package.unencryptedHTTP2Request)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛunencryptedHTTP2Request;
[GoMethodHost(typeof(http_package.unencryptedNetConnInTLSConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛunencryptedNetConnInTLSConn;
[GoMethodHost(typeof(http_package.unsupportedTEError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛunsupportedTEError;
[GoMethodHost(typeof(http_package.wantConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛwantConn;
[GoMethodHost(typeof(http_package.wantConnQueue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpᴛwantConnQueue;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class httpᴛClient {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Response> resp, Func<bool> didTimeout, error err) send(this ref Client c, ж<Request> Ꮡreq, time.Time deadline) => http_package.send⁔(ref c, Ꮡreq, deadline);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Time deadline(this ref Client c) => http_package.deadline⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static RoundTripper transport(this ref Client c) => http_package.transport⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Response> resp, error err) Get(this ж<Client> Ꮡc, @string url) => http_package.Get⁔(Ꮡc, url);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error checkRedirect(this ref Client c, ж<Request> Ꮡreq, slice<ж<Request>> via) => http_package.checkRedirect⁔(ref c, Ꮡreq, via);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Response>, error) Do(this ж<Client> Ꮡc, ж<Request> Ꮡreq) => http_package.Do⁔(Ꮡc, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Response> retres, error reterr) @do(this ж<Client> Ꮡc, ж<Request> Ꮡreq) => http_package.do⁔(Ꮡc, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Action<ж<Request>, bool, bool> makeHeadersCopier(this ж<Client> Ꮡc, ж<Request> Ꮡireq) => http_package.makeHeadersCopier⁔(Ꮡc, Ꮡireq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Response> resp, error err) Post(this ж<Client> Ꮡc, @string url, @string contentType, io.Reader body) => http_package.Post⁔(Ꮡc, url, contentType, body);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Response> resp, error err) PostForm(this ж<Client> Ꮡc, @string urlΔ1, url.Values data) => http_package.PostForm⁔(Ꮡc, urlΔ1, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Response> resp, error err) Head(this ж<Client> Ꮡc, @string url) => http_package.Head⁔(Ꮡc, url);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void CloseIdleConnections(this ref Client c) => http_package.CloseIdleConnections⁔(ref c);
}
partial class httpᴛClientConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<ClientConn> Ꮡcc) => http_package.Close⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Err(this ref ClientConn cc) => http_package.Err⁔(ref cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Response>, error) RoundTrip(this ж<ClientConn> Ꮡcc, ж<Request> Ꮡreq) => http_package.RoundTrip⁔(Ꮡcc, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Available(this ref ClientConn cc) => http_package.Available⁔(ref cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint InFlight(this ref ClientConn cc) => http_package.InFlight⁔(ref cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Reserve(this ж<ClientConn> Ꮡcc) => http_package.Reserve⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Release(this ж<ClientConn> Ꮡcc) => http_package.Release⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Action<ж<ClientConn>> shouldRunStateHook(this ж<ClientConn> Ꮡcc, bool stopRunning) => http_package.shouldRunStateHook⁔(Ꮡcc, stopRunning);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void maybeRunStateHook(this ж<ClientConn> Ꮡcc) => http_package.maybeRunStateHook⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetStateHook(this ж<ClientConn> Ꮡcc, Action<ж<ClientConn>> f) => http_package.SetStateHook⁔(Ꮡcc, f);
}
partial class httpᴛConnState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ConnState c) => http_package.String⁔(c);
}
partial class httpᴛCookie {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Cookie> Ꮡc) => http_package.String⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Valid(this ж<Cookie> Ꮡc) => http_package.Valid⁔(Ꮡc);
}
partial class httpᴛCrossOriginProtection {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error AddTrustedOrigin(this ж<CrossOriginProtection> Ꮡc, @string origin) => http_package.AddTrustedOrigin⁔(Ꮡc, origin);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddInsecureBypassPattern(this ж<CrossOriginProtection> Ꮡc, @string pattern) => http_package.AddInsecureBypassPattern⁔(Ꮡc, pattern);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetDenyHandler(this ж<CrossOriginProtection> Ꮡc, Handler hʗp) => http_package.SetDenyHandler⁔(Ꮡc, hʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Check(this ж<CrossOriginProtection> Ꮡc, ж<Request> Ꮡreq) => http_package.Check⁔(Ꮡc, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isRequestExempt(this ж<CrossOriginProtection> Ꮡc, ж<Request> Ꮡreq) => http_package.isRequestExempt⁔(Ꮡc, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Handler Handler(this ж<CrossOriginProtection> Ꮡc, Handler h) => http_package.Handler⁔(Ꮡc, h);
}
partial class httpᴛDir {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (File, error) Open(this Dir d, @string name) => http_package.Open⁔(d, name);
}
partial class httpᴛHandlerFunc {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void ServeHTTP(this HandlerFunc f, ResponseWriter w, ж<Request> Ꮡr) => http_package.ServeHTTP⁔(f, w, Ꮡr);
}
partial class httpᴛHeader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Add(this Header h, @string key, @string value) => http_package.Add⁔(h, key, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Set(this Header h, @string key, @string value) => http_package.Set⁔(h, key, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Get(this Header h, @string key) => http_package.Get⁔(h, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> Values(this Header h, @string key) => http_package.Values⁔(h, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string get(this Header h, @string key) => http_package.get⁔(h, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool has(this Header h, @string key) => http_package.has⁔(h, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Del(this Header h, @string key) => http_package.Del⁔(h, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Write(this Header h, io.Writer w) => http_package.Write⁔(h, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error write(this Header h, io.Writer w, ж<httptrace.ClientTrace> Ꮡtrace) => http_package.write⁔(h, w, Ꮡtrace);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Header Clone(this Header h) => http_package.Clone⁔(h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<keyValues> kvs, ж<headerSorter> hs) sortedKeyValues(this Header h, map<@string, bool> exclude) => http_package.sortedKeyValues⁔(h, exclude);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error WriteSubset(this Header h, io.Writer w, map<@string, bool> exclude) => http_package.WriteSubset⁔(h, w, exclude);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeSubset(this Header h, io.Writer w, map<@string, bool> exclude, ж<httptrace.ClientTrace> Ꮡtrace) => http_package.writeSubset⁔(h, w, exclude, Ꮡtrace);
}
partial class httpᴛMaxBytesError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref MaxBytesError e) => http_package.Error⁔(ref e);
}
partial class httpᴛProtocolError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref ProtocolError pe) => http_package.Error⁔(ref pe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Is(this ж<ProtocolError> Ꮡpe, error err) => http_package.Is⁔(Ꮡpe, err);
}
partial class httpᴛProtocols {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool HTTP1(this Protocols p) => http_package.HTTP1⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetHTTP1(this ref Protocols p, bool ok) => http_package.SetHTTP1⁔(ref p, ok);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool HTTP2(this Protocols p) => http_package.HTTP2⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetHTTP2(this ref Protocols p, bool ok) => http_package.SetHTTP2⁔(ref p, ok);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool UnencryptedHTTP2(this Protocols p) => http_package.UnencryptedHTTP2⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetUnencryptedHTTP2(this ref Protocols p, bool ok) => http_package.SetUnencryptedHTTP2⁔(ref p, ok);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setBit(this ref Protocols p, uint8 bit, bool ok) => http_package.setBit⁔(ref p, bit, ok);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Protocols p) => http_package.String⁔(p);
}
partial class httpᴛRequest {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static context.Context Context(this ref Request r) => http_package.Context⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Request> WithContext(this ref Request r, context.Context ctx) => http_package.WithContext⁔(ref r, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Request> Clone(this ref Request r, context.Context ctx) => http_package.Clone⁔(ref r, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ProtoAtLeast(this ref Request r, nint major, nint minor) => http_package.ProtoAtLeast⁔(ref r, major, minor);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string UserAgent(this ref Request r) => http_package.UserAgent⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<ж<Cookie>> Cookies(this ref Request r) => http_package.Cookies⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<ж<Cookie>> CookiesNamed(this ref Request r, @string name) => http_package.CookiesNamed⁔(ref r, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Cookie>, error) Cookie(this ref Request r, @string name) => http_package.Cookie⁔(ref r, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddCookie(this ref Request r, ж<Cookie> Ꮡc) => http_package.AddCookie⁔(ref r, Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Referer(this ref Request r) => http_package.Referer⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<multipart.Reader>, error) MultipartReader(this ref Request r) => http_package.MultipartReader⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<multipart.Reader>, error) multipartReader(this ref Request r, bool allowMixed) => http_package.multipartReader⁔(ref r, allowMixed);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isH2Upgrade(this ref Request r) => http_package.isH2Upgrade⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Write(this ж<Request> Ꮡr, io.Writer w) => http_package.Write⁔(Ꮡr, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error WriteProxy(this ж<Request> Ꮡr, io.Writer w) => http_package.WriteProxy⁔(Ꮡr, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error /*err*/ write(this ж<Request> Ꮡr, io.Writer w, bool usingProxy, Header extraHeaders, Func<bool> waitForContinue) => http_package.write⁔(Ꮡr, w, usingProxy, extraHeaders, waitForContinue);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string username, @string password, bool ok) BasicAuth(this ref Request r) => http_package.BasicAuth⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetBasicAuth(this ref Request r, @string username, @string password) => http_package.SetBasicAuth⁔(ref r, username, password);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error ParseForm(this ж<Request> Ꮡr) => http_package.ParseForm⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error ParseMultipartForm(this ж<Request> Ꮡr, int64 maxMemory) => http_package.ParseMultipartForm⁔(Ꮡr, maxMemory);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string FormValue(this ж<Request> Ꮡr, @string key) => http_package.FormValue⁔(Ꮡr, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string PostFormValue(this ж<Request> Ꮡr, @string key) => http_package.PostFormValue⁔(Ꮡr, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (multipart.File, ж<multipart.FileHeader>, error) FormFile(this ж<Request> Ꮡr, @string key) => http_package.FormFile⁔(Ꮡr, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string PathValue(this ref Request r, @string name) => http_package.PathValue⁔(ref r, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetPathValue(this ref Request r, @string name, @string value) => http_package.SetPathValue⁔(ref r, name, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint patIndex(this ref Request r, @string name) => http_package.patIndex⁔(ref r, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool expectsContinue(this ref Request r) => http_package.expectsContinue⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool wantsHttp10KeepAlive(this ref Request r) => http_package.wantsHttp10KeepAlive⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool wantsClose(this ref Request r) => http_package.wantsClose⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error closeBody(this ref Request r) => http_package.closeBody⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isReplayable(this ref Request r) => http_package.isReplayable⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 outgoingLength(this ref Request r) => http_package.outgoingLength⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool requiresHTTP1(this ref Request r) => http_package.requiresHTTP1⁔(ref r);
}
partial class httpᴛResponse {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<ж<Cookie>> Cookies(this ref Response r) => http_package.Cookies⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<url.URL>, error) Location(this ref Response r) => http_package.Location⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ProtoAtLeast(this ref Response r, nint major, nint minor) => http_package.ProtoAtLeast⁔(ref r, major, minor);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Write(this ref Response r, io.Writer w) => http_package.Write⁔(ref r, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeBody(this ref Response r) => http_package.closeBody⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool bodyIsWritable(this ref Response r) => http_package.bodyIsWritable⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isProtocolSwitch(this ref Response r) => http_package.isProtocolSwitch⁔(ref r);
}
partial class httpᴛResponseController {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Flush(this ref ResponseController c) => http_package.Flush⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (net.Conn, ж<bufio.ReadWriter>, error) Hijack(this ref ResponseController c) => http_package.Hijack⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetReadDeadline(this ref ResponseController c, time.Time deadline) => http_package.SetReadDeadline⁔(ref c, deadline);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetWriteDeadline(this ref ResponseController c, time.Time deadline) => http_package.SetWriteDeadline⁔(ref c, deadline);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error EnableFullDuplex(this ref ResponseController c) => http_package.EnableFullDuplex⁔(ref c);
}
partial class httpᴛServeMux {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Handler h, @string pattern) Handler(this ж<ServeMux> Ꮡmux, ж<Request> Ꮡr) => http_package.Handler⁔(Ꮡmux, Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Handler h, @string patStr, ж<pattern>, slice<@string> matches) findHandler(this ж<ServeMux> Ꮡmux, ж<Request> Ꮡr) => http_package.findHandler⁔(Ꮡmux, Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<routingNode>, slice<@string> matches, ж<urlpkg.URL> redirectTo) matchOrRedirect(this ж<ServeMux> Ꮡmux, @string host, @string method, @string path, ж<urlpkg.URL> Ꮡu) => http_package.matchOrRedirect⁔(Ꮡmux, host, method, path, Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<@string> matchingMethods(this ж<ServeMux> Ꮡmux, @string host, @string path) => http_package.matchingMethods⁔(Ꮡmux, host, path);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void ServeHTTP(this ж<ServeMux> Ꮡmux, ResponseWriter w, ж<Request> Ꮡr) => http_package.ServeHTTP⁔(Ꮡmux, w, Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Handle(this ж<ServeMux> Ꮡmux, @string pattern, Handler handler) => http_package.Handle⁔(Ꮡmux, pattern, handler);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void HandleFunc(this ж<ServeMux> Ꮡmux, @string pattern, Action<ResponseWriter, ж<Request>> handler) => http_package.HandleFunc⁔(Ꮡmux, pattern, handler);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void register(this ж<ServeMux> Ꮡmux, @string pattern, Handler handler) => http_package.register⁔(Ꮡmux, pattern, handler);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error registerErr(this ж<ServeMux> Ꮡmux, @string patstr, Handler handler) => http_package.registerErr⁔(Ꮡmux, patstr, handler);
}
partial class httpᴛServer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<conn> newConn(this ж<Server> Ꮡs, net.Conn rwc) => http_package.newConn⁔(Ꮡs, rwc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint maxHeaderBytes(this ref Server s) => http_package.maxHeaderBytes⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 initialReadLimitSize(this ref Server s) => http_package.initialReadLimitSize⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Duration tlsHandshakeTimeout(this ref Server s) => http_package.tlsHandshakeTimeout⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<Server> Ꮡs) => http_package.Close⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Shutdown(this ж<Server> Ꮡs, context.Context ctx) => http_package.Shutdown⁔(Ꮡs, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void RegisterOnShutdown(this ж<Server> Ꮡs, Action f) => http_package.RegisterOnShutdown⁔(Ꮡs, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool closeIdleConns(this ж<Server> Ꮡs) => http_package.closeIdleConns⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error closeListenersLocked(this ref Server s) => http_package.closeListenersLocked⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error ListenAndServe(this ж<Server> Ꮡs) => http_package.ListenAndServe⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool shouldConfigureHTTP2ForServe(this ref Server s) => http_package.shouldConfigureHTTP2ForServe⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Serve(this ж<Server> Ꮡs, net.Listener lʗp) => http_package.Serve⁔(Ꮡs, lʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error ServeTLS(this ж<Server> Ꮡs, net.Listener l, @string certFile, @string keyFile) => http_package.ServeTLS⁔(Ꮡs, l, certFile, keyFile);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Protocols protocols(this ref Server s) => http_package.protocols⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool trackListener(this ж<Server> Ꮡs, ж<net.Listener> Ꮡln, bool add) => http_package.trackListener⁔(Ꮡs, Ꮡln, add);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void trackConn(this ж<Server> Ꮡs, ж<conn> Ꮡc, bool add) => http_package.trackConn⁔(Ꮡs, Ꮡc, add);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Duration idleTimeout(this ref Server s) => http_package.idleTimeout⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Duration readHeaderTimeout(this ref Server s) => http_package.readHeaderTimeout⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool doKeepAlives(this ж<Server> Ꮡs) => http_package.doKeepAlives⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool shuttingDown(this ж<Server> Ꮡs) => http_package.shuttingDown⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetKeepAlivesEnabled(this ж<Server> Ꮡs, bool v) => http_package.SetKeepAlivesEnabled⁔(Ꮡs, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void logf(this ref Server s, @string format, params ꓸꓸꓸany argsʗp) => http_package.logf⁔(ref s, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error ListenAndServeTLS(this ж<Server> Ꮡs, @string certFile, @string keyFile) => http_package.ListenAndServeTLS⁔(Ꮡs, certFile, keyFile);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error setupHTTP2_ServeTLS(this ж<Server> Ꮡs) => http_package.setupHTTP2_ServeTLS⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error setupHTTP2_Serve(this ж<Server> Ꮡs) => http_package.setupHTTP2_Serve⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void onceSetNextProtoDefaults_Serve(this ж<Server> Ꮡs) => http_package.onceSetNextProtoDefaults_Serve⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void onceSetNextProtoDefaults(this ж<Server> Ꮡs) => http_package.onceSetNextProtoDefaults⁔(Ꮡs);
}
partial class httpᴛTransport {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<ClientConn>, error) NewClientConn(this ж<Transport> Ꮡt, context.Context ctx, @string scheme, @string address) => http_package.NewClientConn⁔(Ꮡt, ctx, scheme, address);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Response>, error) RoundTrip(this ж<Transport> Ꮡt, ж<Request> Ꮡreq) => http_package.RoundTrip⁔(Ꮡt, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint writeBufferSize(this ref Transport t) => http_package.writeBufferSize⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint readBufferSize(this ref Transport t) => http_package.readBufferSize⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 maxHeaderResponseSize(this ref Transport t) => http_package.maxHeaderResponseSize⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Transport> Clone(this ж<Transport> Ꮡt) => http_package.Clone⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasCustomTLSDialer(this ref Transport t) => http_package.hasCustomTLSDialer⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void onceSetNextProtoDefaults(this ж<Transport> Ꮡt) => http_package.onceSetNextProtoDefaults⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Protocols protocols(this ref Transport t) => http_package.protocols⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool useRegisteredProtocol(this ref Transport t, ж<Request> Ꮡreq) => http_package.useRegisteredProtocol⁔(ref t, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static RoundTripper alternateRoundTripper(this ж<Transport> Ꮡt, ж<Request> Ꮡreq) => http_package.alternateRoundTripper⁔(Ꮡt, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Response>, error err) roundTrip(this ж<Transport> Ꮡt, ж<Request> Ꮡreq) => http_package.roundTrip⁔(Ꮡt, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void RegisterProtocol(this ж<Transport> Ꮡt, @string scheme, RoundTripper rt) => http_package.RegisterProtocol⁔(Ꮡt, scheme, rt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void CloseIdleConnections(this ж<Transport> Ꮡt) => http_package.CloseIdleConnections⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Action<error> prepareTransportCancel(this ж<Transport> Ꮡt, ж<Request> Ꮡreq, Action<error> origCancel) => http_package.prepareTransportCancel⁔(Ꮡt, Ꮡreq, origCancel);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void CancelRequest(this ж<Transport> Ꮡt, ж<Request> Ꮡreq) => http_package.CancelRequest⁔(Ꮡt, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (connectMethod cm, error err) connectMethodForRequest(this ref Transport t, ж<transportRequest> Ꮡtreq) => http_package.connectMethodForRequest⁔(ref t, Ꮡtreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void putOrCloseIdleConn(this ж<Transport> Ꮡt, ж<persistConn> Ꮡpconn) => http_package.putOrCloseIdleConn⁔(Ꮡt, Ꮡpconn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint maxIdleConnsPerHost(this ref Transport t) => http_package.maxIdleConnsPerHost⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error tryPutIdleConn(this ж<Transport> Ꮡt, ж<persistConn> Ꮡpconn) => http_package.tryPutIdleConn⁔(Ꮡt, Ꮡpconn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*delivered*/ queueForIdleConn(this ж<Transport> Ꮡt, ж<wantConn> Ꮡw) => http_package.queueForIdleConn⁔(Ꮡt, Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool removeIdleConn(this ж<Transport> Ꮡt, ж<persistConn> Ꮡpconn) => http_package.removeIdleConn⁔(Ꮡt, Ꮡpconn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool removeIdleConnLocked(this ref Transport t, ж<persistConn> Ꮡpconn) => http_package.removeIdleConnLocked⁔(ref t, Ꮡpconn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (net.Conn, error) dial(this ref Transport t, context.Context ctx, @string network, @string addr) => http_package.dial⁔(ref t, ctx, network, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (net.Conn conn, error err) customDialTLS(this ref Transport t, context.Context ctx, @string network, @string addr) => http_package.customDialTLS⁔(ref t, ctx, network, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<persistConn>, error err) getConn(this ж<Transport> Ꮡt, ж<transportRequest> Ꮡtreq, connectMethod cm) => http_package.getConn⁔(Ꮡt, Ꮡtreq, cm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void queueForDial(this ж<Transport> Ꮡt, ж<wantConn> Ꮡw) => http_package.queueForDial⁔(Ꮡt, Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void startDialConnForLocked(this ж<Transport> Ꮡt, ж<wantConn> Ꮡw) => http_package.startDialConnForLocked⁔(Ꮡt, Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void dialConnFor(this ж<Transport> Ꮡt, ж<wantConn> Ꮡw) => http_package.dialConnFor⁔(Ꮡt, Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void decConnsPerHost(this ж<Transport> Ꮡt, connectMethodKey key) => http_package.decConnsPerHost⁔(Ꮡt, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<persistConn> pconn, error err) dialConn(this ж<Transport> Ꮡt, context.Context ctx, connectMethod cm, bool isClientConn, Action internalStateHook) => http_package.dialConn⁔(Ꮡt, ctx, cm, isClientConn, internalStateHook);
}
partial class httpᴛbody {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ж<body> Ꮡb, slice<byte> p) => http_package.Read⁔(Ꮡb, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) readLocked(this ref body b, slice<byte> p) => http_package.readLocked⁔(ref b, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readTrailer(this ref body b) => http_package.readTrailer⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 unreadDataSizeLocked(this ref body b) => http_package.unreadDataSizeLocked⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<body> Ꮡb) => http_package.Close⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool didEarlyClose(this ж<body> Ꮡb) => http_package.didEarlyClose⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool bodyRemains(this ж<body> Ꮡb) => http_package.bodyRemains⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void registerOnHitEOF(this ж<body> Ꮡb, Action fn) => http_package.registerOnHitEOF⁔(Ꮡb, fn);
}
partial class httpᴛbodyEOFSignal {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ж<bodyEOFSignal> Ꮡes, slice<byte> p) => http_package.Read⁔(Ꮡes, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<bodyEOFSignal> Ꮡes) => http_package.Close⁔(Ꮡes);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error condfn(this ref bodyEOFSignal es, error err) => http_package.condfn⁔(ref es, err);
}
partial class httpᴛbodyLocked {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this bodyLocked bl, slice<byte> p) => http_package.Read⁔(bl, p);
}
partial class httpᴛbufioFlushWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this bufioFlushWriter fw, slice<byte> p) => http_package.Write⁔(fw, p);
}
partial class httpᴛbyteReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ref byteReader br, slice<byte> p) => http_package.Read⁔(ref br, p);
}
partial class httpᴛcancelTimerBody {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ref cancelTimerBody b, slice<byte> p) => http_package.Read⁔(ref b, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref cancelTimerBody b) => http_package.Close⁔(ref b);
}
partial class httpᴛcheckConnErrorWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this checkConnErrorWriter w, slice<byte> p) => http_package.Write⁔(w, p);
}
partial class httpᴛchunkWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ref chunkWriter cw, slice<byte> p) => http_package.Write⁔(ref cw, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error flush(this ref chunkWriter cw) => http_package.flush⁔(ref cw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void close(this ref chunkWriter cw) => http_package.close⁔(ref cw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeHeader(this ref chunkWriter cw, slice<byte> p) => http_package.writeHeader⁔(ref cw, p);
}
partial class httpᴛconn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hijacked(this ж<conn> Ꮡc) => http_package.hijacked⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (net.Conn rwc, ж<bufio.ReadWriter> buf, error err) hijackLocked(this ж<conn> Ꮡc) => http_package.hijackLocked⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<response> w, error err) readRequest(this ж<conn> Ꮡc, context.Context ctx) => http_package.readRequest⁔(Ꮡc, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void finalFlush(this ref conn c) => http_package.finalFlush⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void close(this ref conn c) => http_package.close⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeWriteAndWait(this ref conn c) => http_package.closeWriteAndWait⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setState(this ж<conn> Ꮡc, net.Conn nc, ConnState state, bool runHook) => http_package.setState⁔(Ꮡc, nc, state, runHook);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ConnState state, int64 unixSec) getState(this ж<conn> Ꮡc) => http_package.getState⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void serve(this ж<conn> Ꮡc, context.Context ctx) => http_package.serve⁔(Ꮡc, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool maybeServeUnencryptedHTTP2(this ж<conn> Ꮡc, context.Context ctx) => http_package.maybeServeUnencryptedHTTP2⁔(Ꮡc, ctx);
}
partial class httpᴛconnLRU {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add(this ref connLRU cl, ж<persistConn> Ꮡpc) => http_package.add⁔(ref cl, Ꮡpc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<persistConn> removeOldest(this ref connLRU cl) => http_package.removeOldest⁔(ref cl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void remove(this ref connLRU cl, ж<persistConn> Ꮡpc) => http_package.remove⁔(ref cl, Ꮡpc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint len(this ref connLRU cl) => http_package.len⁔(ref cl);
}
partial class httpᴛconnReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void @lock(this ж<connReader> Ꮡcr) => http_package.lock⁔(Ꮡcr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void unlock(this ж<connReader> Ꮡcr) => http_package.unlock⁔(Ꮡcr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void releaseConn(this ж<connReader> Ꮡcr) => http_package.releaseConn⁔(Ꮡcr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void startBackgroundRead(this ж<connReader> Ꮡcr) => http_package.startBackgroundRead⁔(Ꮡcr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void backgroundRead(this ж<connReader> Ꮡcr) => http_package.backgroundRead⁔(Ꮡcr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void abortPendingRead(this ж<connReader> Ꮡcr) => http_package.abortPendingRead⁔(Ꮡcr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setReadLimit(this ref connReader cr, int64 remain) => http_package.setReadLimit⁔(ref cr, remain);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setInfiniteReadLimit(this ref connReader cr) => http_package.setInfiniteReadLimit⁔(ref cr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hitReadLimit(this ref connReader cr) => http_package.hitReadLimit⁔(ref cr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void handleReadErrorLocked(this ref connReader cr, error _) => http_package.handleReadErrorLocked⁔(ref cr, _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ж<connReader> Ꮡcr, slice<byte> p) => http_package.Read⁔(Ꮡcr, p);
}
partial class httpᴛconnectMethod {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string proxyAuth(this ref connectMethod cm) => http_package.proxyAuth⁔(ref cm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static connectMethodKey key(this ref connectMethod cm) => http_package.key⁔(ref cm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string scheme(this ref connectMethod cm) => http_package.scheme⁔(ref cm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string addr(this ref connectMethod cm) => http_package.addr⁔(ref cm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string tlsHost(this ref connectMethod cm) => http_package.tlsHost⁔(ref cm);
}
partial class httpᴛconnectMethodKey {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this connectMethodKey k) => http_package.String⁔(k);
}
partial class httpᴛcontextKey {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref contextKey k) => http_package.String⁔(ref k);
}
partial class httpᴛcountingWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ref countingWriter w, slice<byte> p) => http_package.Write⁔(ref w, p);
}
partial class httpᴛdirEntryDirs {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint len(this dirEntryDirs d) => http_package.len⁔(d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isDir(this dirEntryDirs d, nint i) => http_package.isDir⁔(d, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string name(this dirEntryDirs d, nint i) => http_package.name⁔(d, i);
}
partial class httpᴛeofReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this eofReader _Δp0, slice<byte> _Δp1) => http_package.Read⁔(_Δp0, _Δp1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (byte, error) ReadByte(this eofReader _) => http_package.ReadByte⁔(_);
}
partial class httpᴛerrorReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this errorReader r, slice<byte> p) => http_package.Read⁔(r, p);
}
partial class httpᴛexactSig {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string match(this ref exactSig e, slice<byte> data, nint firstNonWS) => http_package.match⁔(ref e, data, firstNonWS);
}
partial class httpᴛexpectContinueReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ж<expectContinueReader> Ꮡecr, slice<byte> p) => http_package.Read⁔(Ꮡecr, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<expectContinueReader> Ꮡecr) => http_package.Close⁔(Ꮡecr);
}
partial class httpᴛextraHeader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Write(this extraHeader h, ж<bufio.Writer> Ꮡw) => http_package.Write⁔(h, Ꮡw);
}
partial class httpᴛfakeLocker {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Lock(this fakeLocker _) => http_package.Lock⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Unlock(this fakeLocker _) => http_package.Unlock⁔(_);
}
partial class httpᴛfileHandler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ServeHTTP(this ref fileHandler f, ResponseWriter w, ж<Request> Ꮡr) => http_package.ServeHTTP⁔(ref f, w, Ꮡr);
}
partial class httpᴛfileInfoDirs {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint len(this fileInfoDirs d) => http_package.len⁔(d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isDir(this fileInfoDirs d, nint i) => http_package.isDir⁔(d, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string name(this fileInfoDirs d, nint i) => http_package.name⁔(d, i);
}
partial class httpᴛfileTransport {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Response> resp, error err) RoundTrip(this fileTransport t, ж<Request> Ꮡreq) => http_package.RoundTrip⁔(t, Ꮡreq);
}
partial class httpᴛfinishAsyncByteRead {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this finishAsyncByteRead fr, slice<byte> p) => http_package.Read⁔(fr, p);
}
partial class httpᴛglobalOptionsHandler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ServeHTTP(this globalOptionsHandler _, ResponseWriter w, ж<Request> Ꮡr) => http_package.ServeHTTP⁔(_, w, Ꮡr);
}
partial class httpᴛgzipReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<gzip.Reader>, error) acquire(this ж<gzipReader> Ꮡgz) => http_package.acquire⁔(Ꮡgz);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void release(this ж<gzipReader> Ꮡgz, ж<gzip.Reader> Ꮡzr) => http_package.release⁔(Ꮡgz, Ꮡzr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void close(this ж<gzipReader> Ꮡgz) => http_package.close⁔(Ꮡgz);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ж<gzipReader> Ꮡgz, slice<byte> p) => http_package.Read⁔(Ꮡgz, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<gzipReader> Ꮡgz) => http_package.Close⁔(Ꮡgz);
}
partial class httpᴛhtmlSig {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string match(this htmlSig h, slice<byte> data, nint firstNonWS) => http_package.match⁔(h, data, firstNonWS);
}
partial class httpᴛhttp1ClientConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Response>, error) RoundTrip(this http1ClientConn cc, ж<Request> Ꮡreq) => http_package.RoundTrip⁔(cc, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this http1ClientConn cc) => http_package.Close⁔(cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Err(this http1ClientConn cc) => http_package.Err⁔(cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Available(this http1ClientConn cc) => http_package.Available⁔(cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint InFlight(this http1ClientConn cc) => http_package.InFlight⁔(cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Reserve(this http1ClientConn cc) => http_package.Reserve⁔(cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Release(this http1ClientConn cc) => http_package.Release⁔(cc);
}
partial class httpᴛhttp2ClientConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void healthCheck(this ж<http2ClientConn> Ꮡcc) => http_package.healthCheck⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetDoNotReuse(this ж<http2ClientConn> Ꮡcc) => http_package.SetDoNotReuse⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setGoAway(this ж<http2ClientConn> Ꮡcc, ж<http2GoAwayFrame> Ꮡf) => http_package.setGoAway⁔(Ꮡcc, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CanTakeNewRequest(this ж<http2ClientConn> Ꮡcc) => http_package.CanTakeNewRequest⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReserveNewRequest(this ж<http2ClientConn> Ꮡcc) => http_package.ReserveNewRequest⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static http2ClientConnState State(this ж<http2ClientConn> Ꮡcc) => http_package.State⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static http2clientConnIdleState idleState(this ж<http2ClientConn> Ꮡcc) => http_package.idleState⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static http2clientConnIdleState /*st*/ idleStateLocked(this ref http2ClientConn cc) => http_package.idleStateLocked⁔(ref cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isUsableLocked(this ref http2ClientConn cc) => http_package.isUsableLocked⁔(ref cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool canReserveLocked(this ref http2ClientConn cc) => http_package.canReserveLocked⁔(ref cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint currentRequestCountLocked(this ref http2ClientConn cc) => http_package.currentRequestCountLocked⁔(ref cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool canTakeNewRequestLocked(this ref http2ClientConn cc) => http_package.canTakeNewRequestLocked⁔(ref cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint availableLocked(this ref http2ClientConn cc) => http_package.availableLocked⁔(ref cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool tooIdleLocked(this ref http2ClientConn cc) => http_package.tooIdleLocked⁔(ref cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void onIdleTimeout(this ж<http2ClientConn> Ꮡcc) => http_package.onIdleTimeout⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeConn(this ж<http2ClientConn> Ꮡcc) => http_package.closeConn⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void forceCloseConn(this ref http2ClientConn cc) => http_package.forceCloseConn⁔(ref cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeIfIdle(this ж<http2ClientConn> Ꮡcc) => http_package.closeIfIdle⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isDoNotReuseAndIdle(this ж<http2ClientConn> Ꮡcc) => http_package.isDoNotReuseAndIdle⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Shutdown(this ж<http2ClientConn> Ꮡcc, context.Context ctx) => http_package.Shutdown⁔(Ꮡcc, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error sendGoAway(this ж<http2ClientConn> Ꮡcc) => http_package.sendGoAway⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void goAwayAndClose(this ж<http2ClientConn> Ꮡcc, http2ErrCode code) => http_package.goAwayAndClose⁔(Ꮡcc, code);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeForError(this ж<http2ClientConn> Ꮡcc, error err) => http_package.closeForError⁔(Ꮡcc, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<http2ClientConn> Ꮡcc) => http_package.Close⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeForLostPing(this ж<http2ClientConn> Ꮡcc) => http_package.closeForLostPing⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Duration responseHeaderTimeout(this ref http2ClientConn cc) => http_package.responseHeaderTimeout⁔(ref cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void decrStreamReservations(this ж<http2ClientConn> Ꮡcc) => http_package.decrStreamReservations⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void decrStreamReservationsLocked(this ref http2ClientConn cc) => http_package.decrStreamReservationsLocked⁔(ref cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Response>, error) RoundTrip(this ж<http2ClientConn> Ꮡcc, ж<Request> Ꮡreq) => http_package.RoundTrip⁔(Ꮡcc, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Response>, error) roundTrip(this ж<http2ClientConn> Ꮡcc, ж<Request> Ꮡreq, Action<ж<http2clientStream>> streamf) => http_package.roundTrip⁔(Ꮡcc, Ꮡreq, streamf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error awaitOpenSlotForStreamLocked(this ref http2ClientConn cc, ж<http2clientStream> Ꮡcs) => http_package.awaitOpenSlotForStreamLocked⁔(ref cc, Ꮡcs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeHeaders(this ref http2ClientConn cc, uint32 streamID, bool endStream, nint maxFrameSize, slice<byte> hdrs) => http_package.writeHeaders⁔(ref cc, streamID, endStream, maxFrameSize, hdrs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) encodeTrailers(this ref http2ClientConn cc, Header trailer) => http_package.encodeTrailers⁔(ref cc, trailer);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeHeader(this ref http2ClientConn cc, @string name, @string value) => http_package.writeHeader⁔(ref cc, name, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addStreamLocked(this ж<http2ClientConn> Ꮡcc, ж<http2clientStream> Ꮡcs) => http_package.addStreamLocked⁔(Ꮡcc, Ꮡcs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void forgetStreamID(this ж<http2ClientConn> Ꮡcc, uint32 id) => http_package.forgetStreamID⁔(Ꮡcc, id);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readLoop(this ж<http2ClientConn> Ꮡcc) => http_package.readLoop⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void countReadFrameError(this ref http2ClientConn cc, error err) => http_package.countReadFrameError⁔(ref cc, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Ping(this ж<http2ClientConn> Ꮡcc, context.Context ctx) => http_package.Ping⁔(Ꮡcc, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeStreamReset(this ж<http2ClientConn> Ꮡcc, uint32 streamID, http2ErrCode code, bool ping, error err) => http_package.writeStreamReset⁔(Ꮡcc, streamID, code, ping, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void logf(this ref http2ClientConn cc, @string format, params ꓸꓸꓸany argsʗp) => http_package.logf⁔(ref cc, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void vlogf(this ref http2ClientConn cc, @string format, params ꓸꓸꓸany argsʗp) => http_package.vlogf⁔(ref cc, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void maybeCallStateHook(this ref http2ClientConn cc) => http_package.maybeCallStateHook⁔(ref cc);
}
partial class httpᴛhttp2ConnectionError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this http2ConnectionError e) => http_package.Error⁔(e);
}
partial class httpᴛhttp2ContinuationFrame {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> HeaderBlockFragment(this ref http2ContinuationFrame f) => http_package.HeaderBlockFragment⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool HeadersEnded(this ref http2ContinuationFrame f) => http_package.HeadersEnded⁔(ref f);
}
partial class httpᴛhttp2DataFrame {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool StreamEnded(this ref http2DataFrame f) => http_package.StreamEnded⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> Data(this ref http2DataFrame f) => http_package.Data⁔(ref f);
}
partial class httpᴛhttp2ErrCode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this http2ErrCode e) => http_package.String⁔(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string stringToken(this http2ErrCode e) => http_package.stringToken⁔(e);
}
partial class httpᴛhttp2Flags {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Has(this http2Flags f, http2Flags v) => http_package.Has⁔(f, v);
}
partial class httpᴛhttp2FrameHeader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static http2FrameHeader Header(this http2FrameHeader h) => http_package.Header⁔(h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this http2FrameHeader h) => http_package.String⁔(h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeDebug(this http2FrameHeader h, ж<bytes.Buffer> Ꮡbuf) => http_package.writeDebug⁔(h, Ꮡbuf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkValid(this ref http2FrameHeader h) => http_package.checkValid⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void invalidate(this ref http2FrameHeader h) => http_package.invalidate⁔(ref h);
}
partial class httpᴛhttp2FrameType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this http2FrameType t) => http_package.String⁔(t);
}
partial class httpᴛhttp2FrameWriteRequest {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint32 StreamID(this http2FrameWriteRequest wr) => http_package.StreamID⁔(wr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isControl(this http2FrameWriteRequest wr) => http_package.isControl⁔(wr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint DataSize(this http2FrameWriteRequest wr) => http_package.DataSize⁔(wr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (http2FrameWriteRequest, http2FrameWriteRequest, nint) Consume(this http2FrameWriteRequest wr, int32 n) => http_package.Consume⁔(wr, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this http2FrameWriteRequest wr) => http_package.String⁔(wr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void replyToWriter(this ref http2FrameWriteRequest wr, error err) => http_package.replyToWriter⁔(ref wr, err);
}
partial class httpᴛhttp2Framer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 maxHeaderListSize(this ref http2Framer fr) => http_package.maxHeaderListSize⁔(ref fr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void startWrite(this ref http2Framer f, http2FrameType ftype, http2Flags flags, uint32 streamID) => http_package.startWrite⁔(ref f, ftype, flags, streamID);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error endWrite(this ж<http2Framer> Ꮡf) => http_package.endWrite⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void logWrite(this ж<http2Framer> Ꮡf) => http_package.logWrite⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeByte(this ref http2Framer f, byte v) => http_package.writeByte⁔(ref f, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeBytes(this ref http2Framer f, slice<byte> v) => http_package.writeBytes⁔(ref f, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeUint16(this ref http2Framer f, uint16 v) => http_package.writeUint16⁔(ref f, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeUint32(this ref http2Framer f, uint32 v) => http_package.writeUint32⁔(ref f, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void SetReuseFrames(this ref http2Framer fr) => http_package.SetReuseFrames⁔(ref fr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void SetMaxReadFrameSize(this ref http2Framer fr, uint32 v) => http_package.SetMaxReadFrameSize⁔(ref fr, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error ErrorDetail(this ref http2Framer fr) => http_package.ErrorDetail⁔(ref fr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (http2FrameHeader, error) ReadFrameHeader(this ref http2Framer fr) => http_package.ReadFrameHeader⁔(ref fr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (http2Frame, error) ReadFrameForHeader(this ж<http2Framer> Ꮡfr, http2FrameHeader fh) => http_package.ReadFrameForHeader⁔(Ꮡfr, fh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (http2Frame, error) ReadFrame(this ж<http2Framer> Ꮡfr) => http_package.ReadFrame⁔(Ꮡfr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error connError(this ref http2Framer fr, http2ErrCode code, @string reason) => http_package.connError⁔(ref fr, code, reason);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error checkFrameOrder(this ref http2Framer fr, http2FrameHeader fh) => http_package.checkFrameOrder⁔(ref fr, fh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error WriteData(this ж<http2Framer> Ꮡf, uint32 streamID, bool endStream, slice<byte> data) => http_package.WriteData⁔(Ꮡf, streamID, endStream, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error WriteDataPadded(this ж<http2Framer> Ꮡf, uint32 streamID, bool endStream, slice<byte> data, slice<byte> pad) => http_package.WriteDataPadded⁔(Ꮡf, streamID, endStream, data, pad);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error startWriteDataPadded(this ref http2Framer f, uint32 streamID, bool endStream, slice<byte> data, slice<byte> pad) => http_package.startWriteDataPadded⁔(ref f, streamID, endStream, data, pad);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error WriteSettings(this ж<http2Framer> Ꮡf, params ꓸꓸꓸhttp2Setting settingsʗp) => http_package.WriteSettings⁔(Ꮡf, settingsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error WriteSettingsAck(this ж<http2Framer> Ꮡf) => http_package.WriteSettingsAck⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error WritePing(this ж<http2Framer> Ꮡf, bool ack, /*[8]*/ array<byte> data) => http_package.WritePing⁔(Ꮡf, ack, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error WriteGoAway(this ж<http2Framer> Ꮡf, uint32 maxStreamID, http2ErrCode code, slice<byte> debugData) => http_package.WriteGoAway⁔(Ꮡf, maxStreamID, code, debugData);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error WriteWindowUpdate(this ж<http2Framer> Ꮡf, uint32 streamID, uint32 incr) => http_package.WriteWindowUpdate⁔(Ꮡf, streamID, incr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error WriteHeaders(this ж<http2Framer> Ꮡf, http2HeadersFrameParam p) => http_package.WriteHeaders⁔(Ꮡf, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error WritePriority(this ж<http2Framer> Ꮡf, uint32 streamID, http2PriorityParam p) => http_package.WritePriority⁔(Ꮡf, streamID, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error WriteRSTStream(this ж<http2Framer> Ꮡf, uint32 streamID, http2ErrCode code) => http_package.WriteRSTStream⁔(Ꮡf, streamID, code);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error WriteContinuation(this ж<http2Framer> Ꮡf, uint32 streamID, bool endHeaders, slice<byte> headerBlockFragment) => http_package.WriteContinuation⁔(Ꮡf, streamID, endHeaders, headerBlockFragment);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error WritePushPromise(this ж<http2Framer> Ꮡf, http2PushPromiseParam p) => http_package.WritePushPromise⁔(Ꮡf, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error WriteRawFrame(this ж<http2Framer> Ꮡf, http2FrameType t, http2Flags flags, uint32 streamID, slice<byte> payload) => http_package.WriteRawFrame⁔(Ꮡf, t, flags, streamID, payload);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint maxHeaderStringLen(this ref http2Framer fr) => http_package.maxHeaderStringLen⁔(ref fr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (http2Frame, error) readMetaFrame(this ж<http2Framer> Ꮡfr, ж<http2HeadersFrame> Ꮡhf) => http_package.readMetaFrame⁔(Ꮡfr, Ꮡhf);
}
partial class httpᴛhttp2GoAwayError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this http2GoAwayError e) => http_package.Error⁔(e);
}
partial class httpᴛhttp2GoAwayFrame {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> DebugData(this ref http2GoAwayFrame f) => http_package.DebugData⁔(ref f);
}
partial class httpᴛhttp2HeadersFrame {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> HeaderBlockFragment(this ref http2HeadersFrame f) => http_package.HeaderBlockFragment⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool HeadersEnded(this ref http2HeadersFrame f) => http_package.HeadersEnded⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool StreamEnded(this ref http2HeadersFrame f) => http_package.StreamEnded⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool HasPriority(this ref http2HeadersFrame f) => http_package.HasPriority⁔(ref f);
}
partial class httpᴛhttp2MetaHeadersFrame {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string PseudoValue(this ref http2MetaHeadersFrame mh, @string pseudo) => http_package.PseudoValue⁔(ref mh, pseudo);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<hpack.HeaderField> RegularFields(this ref http2MetaHeadersFrame mh) => http_package.RegularFields⁔(ref mh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<hpack.HeaderField> PseudoFields(this ref http2MetaHeadersFrame mh) => http_package.PseudoFields⁔(ref mh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error checkPseudos(this ref http2MetaHeadersFrame mh) => http_package.checkPseudos⁔(ref mh);
}
partial class httpᴛhttp2PingFrame {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool IsAck(this ref http2PingFrame f) => http_package.IsAck⁔(ref f);
}
partial class httpᴛhttp2PriorityParam {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsZero(this http2PriorityParam p) => http_package.IsZero⁔(p);
}
partial class httpᴛhttp2PushPromiseFrame {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> HeaderBlockFragment(this ref http2PushPromiseFrame f) => http_package.HeaderBlockFragment⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool HeadersEnded(this ref http2PushPromiseFrame f) => http_package.HeadersEnded⁔(ref f);
}
partial class httpᴛhttp2ServeConnOpts {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static context.Context context(this ж<http2ServeConnOpts> Ꮡo) => http_package.context⁔(Ꮡo);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Server> baseConfig(this ж<http2ServeConnOpts> Ꮡo) => http_package.baseConfig⁔(Ꮡo);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Handler handler(this ж<http2ServeConnOpts> Ꮡo) => http_package.handler⁔(Ꮡo);
}
partial class httpᴛhttp2Server {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ServeConn(this ж<http2Server> Ꮡs, net.Conn c, ж<http2ServeConnOpts> Ꮡopts) => http_package.ServeConn⁔(Ꮡs, c, Ꮡopts);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void serveConn(this ж<http2Server> Ꮡs, net.Conn c, ж<http2ServeConnOpts> Ꮡopts, Action<ж<http2serverConn>> newf) => http_package.serveConn⁔(Ꮡs, c, Ꮡopts, newf);
}
partial class httpᴛhttp2Setting {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this http2Setting s) => http_package.String⁔(s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Valid(this http2Setting s) => http_package.Valid⁔(s);
}
partial class httpᴛhttp2SettingID {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this http2SettingID s) => http_package.String⁔(s);
}
partial class httpᴛhttp2SettingsFrame {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool IsAck(this ref http2SettingsFrame f) => http_package.IsAck⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint32 v, bool ok) Value(this ref http2SettingsFrame f, http2SettingID id) => http_package.Value⁔(ref f, id);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static http2Setting Setting(this ref http2SettingsFrame f, nint i) => http_package.Setting⁔(ref f, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint NumSettings(this ref http2SettingsFrame f) => http_package.NumSettings⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool HasDuplicates(this ref http2SettingsFrame f) => http_package.HasDuplicates⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error ForeachSetting(this ref http2SettingsFrame f, Func<http2Setting, error> fn) => http_package.ForeachSetting⁔(ref f, fn);
}
partial class httpᴛhttp2StreamError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this http2StreamError e) => http_package.Error⁔(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeFrame(this http2StreamError se, http2writeContext ctx) => http_package.writeFrame⁔(se, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool staysWithinBuffer(this http2StreamError se, nint max) => http_package.staysWithinBuffer⁔(se, max);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool As(this http2StreamError e, any target) => http_package.As⁔(e, target);
}
partial class httpᴛhttp2Transport {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 maxHeaderListSize(this ref http2Transport t) => http_package.maxHeaderListSize⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool disableCompression(this ref http2Transport t) => http_package.disableCompression⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static http2ClientConnPool connPool(this ж<http2Transport> Ꮡt) => http_package.connPool⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initConnPool(this ж<http2Transport> Ꮡt) => http_package.initConnPool⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Response>, error) RoundTrip(this ж<http2Transport> Ꮡt, ж<Request> Ꮡreq) => http_package.RoundTrip⁔(Ꮡt, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Response>, error) RoundTripOpt(this ж<http2Transport> Ꮡt, ж<Request> Ꮡreq, http2RoundTripOpt opt) => http_package.RoundTripOpt⁔(Ꮡt, Ꮡreq, opt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void CloseIdleConnections(this ж<http2Transport> Ꮡt) => http_package.CloseIdleConnections⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<http2ClientConn>, error) dialClientConn(this ж<http2Transport> Ꮡt, context.Context ctx, @string addr, bool singleUse) => http_package.dialClientConn⁔(Ꮡt, ctx, addr, singleUse);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<tls.Config> newTLSConfig(this ref http2Transport t, @string host) => http_package.newTLSConfig⁔(ref t, host);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (net.Conn, error) dialTLS(this ref http2Transport t, context.Context ctx, @string network, @string addr, ж<tls.Config> ᏑtlsCfg) => http_package.dialTLS⁔(ref t, ctx, network, addr, ᏑtlsCfg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool disableKeepAlives(this ref http2Transport t) => http_package.disableKeepAlives⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Duration expectContinueTimeout(this ref http2Transport t) => http_package.expectContinueTimeout⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<http2ClientConn>, error) NewClientConn(this ж<http2Transport> Ꮡt, net.Conn c) => http_package.NewClientConn⁔(Ꮡt, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<http2ClientConn>, error) newClientConn(this ж<http2Transport> Ꮡt, net.Conn c, bool singleUse, Action internalStateHook) => http_package.newClientConn⁔(Ꮡt, c, singleUse, internalStateHook);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void vlogf(this ref http2Transport t, @string format, params ꓸꓸꓸany argsʗp) => http_package.vlogf⁔(ref t, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void logf(this ref http2Transport t, @string format, params ꓸꓸꓸany argsʗp) => http_package.logf⁔(ref t, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Duration idleConnTimeout(this ref http2Transport t) => http_package.idleConnTimeout⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<tls.Conn>, error) dialTLSWithContext(this ref http2Transport t, context.Context ctx, @string network, @string addr, ж<tls.Config> Ꮡcfg) => http_package.dialTLSWithContext⁔(ref t, ctx, network, addr, Ꮡcfg);
}
partial class httpᴛhttp2UnknownFrame {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> Payload(this ref http2UnknownFrame f) => http_package.Payload⁔(ref f);
}
partial class httpᴛhttp2addConnCall {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void run(this ref http2addConnCall c, ж<http2Transport> Ꮡt, @string key, net.Conn nc) => http_package.run⁔(ref c, Ꮡt, key, nc);
}
partial class httpᴛhttp2bufferedWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Available(this ref http2bufferedWriter w) => http_package.Available⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ж<http2bufferedWriter> Ꮡw, slice<byte> p) => http_package.Write⁔(Ꮡw, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Flush(this ref http2bufferedWriter w) => http_package.Flush⁔(ref w);
}
partial class httpᴛhttp2bufferedWriterTimeoutWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ref http2bufferedWriterTimeoutWriter w, slice<byte> p) => http_package.Write⁔(ref w, p);
}
partial class httpᴛhttp2chunkWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this http2chunkWriter cw, slice<byte> p) => http_package.Write⁔(cw, p);
}
partial class httpᴛhttp2clientConnPool {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<http2ClientConn>, error) GetClientConn(this ж<http2clientConnPool> Ꮡp, ж<Request> Ꮡreq, @string addr) => http_package.GetClientConn⁔(Ꮡp, Ꮡreq, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<http2ClientConn>, error) getClientConn(this ж<http2clientConnPool> Ꮡp, ж<Request> Ꮡreq, @string addr, bool dialOnMiss) => http_package.getClientConn⁔(Ꮡp, Ꮡreq, addr, dialOnMiss);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<http2dialCall> getStartDialLocked(this ж<http2clientConnPool> Ꮡp, context.Context ctx, @string addr) => http_package.getStartDialLocked⁔(Ꮡp, ctx, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bool used, error err) addConnIfNeeded(this ж<http2clientConnPool> Ꮡp, @string key, ж<http2Transport> Ꮡt, net.Conn c) => http_package.addConnIfNeeded⁔(Ꮡp, key, Ꮡt, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addConnLocked(this ref http2clientConnPool p, @string key, ж<http2ClientConn> Ꮡcc) => http_package.addConnLocked⁔(ref p, key, Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void MarkDead(this ж<http2clientConnPool> Ꮡp, ж<http2ClientConn> Ꮡcc) => http_package.MarkDead⁔(Ꮡp, Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeIdleConnections(this ж<http2clientConnPool> Ꮡp) => http_package.closeIdleConnections⁔(Ꮡp);
}
partial class httpᴛhttp2clientConnReadLoop {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cleanup(this ж<http2clientConnReadLoop> Ꮡrl) => http_package.cleanup⁔(Ꮡrl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error run(this ж<http2clientConnReadLoop> Ꮡrl) => http_package.run⁔(Ꮡrl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processHeaders(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2MetaHeadersFrame> Ꮡf) => http_package.processHeaders⁔(Ꮡrl, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Response>, error) handleResponse(this ref http2clientConnReadLoop rl, ж<http2clientStream> Ꮡcs, ж<http2MetaHeadersFrame> Ꮡf) => http_package.handleResponse⁔(ref rl, Ꮡcs, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processTrailers(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2clientStream> Ꮡcs, ж<http2MetaHeadersFrame> Ꮡf) => http_package.processTrailers⁔(Ꮡrl, Ꮡcs, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processData(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2DataFrame> Ꮡf) => http_package.processData⁔(Ꮡrl, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void endStream(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2clientStream> Ꮡcs) => http_package.endStream⁔(Ꮡrl, Ꮡcs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void endStreamError(this ref http2clientConnReadLoop rl, ж<http2clientStream> Ꮡcs, error err) => http_package.endStreamError⁔(ref rl, Ꮡcs, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void endStreamErrorLocked(this ref http2clientConnReadLoop rl, ж<http2clientStream> Ꮡcs, error err) => http_package.endStreamErrorLocked⁔(ref rl, Ꮡcs, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<http2clientStream> streamByID(this ж<http2clientConnReadLoop> Ꮡrl, uint32 id, bool headerOrData) => http_package.streamByID⁔(Ꮡrl, id, headerOrData);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<http2clientStream> streamByIDLocked(this ref http2clientConnReadLoop rl, uint32 id, bool headerOrData) => http_package.streamByIDLocked⁔(ref rl, id, headerOrData);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processGoAway(this ref http2clientConnReadLoop rl, ж<http2GoAwayFrame> Ꮡf) => http_package.processGoAway⁔(ref rl, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processSettings(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2SettingsFrame> Ꮡf) => http_package.processSettings⁔(Ꮡrl, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processSettingsNoWrite(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2SettingsFrame> Ꮡf) => http_package.processSettingsNoWrite⁔(Ꮡrl, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processWindowUpdate(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2WindowUpdateFrame> Ꮡf) => http_package.processWindowUpdate⁔(Ꮡrl, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processResetStream(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2RSTStreamFrame> Ꮡf) => http_package.processResetStream⁔(Ꮡrl, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processPing(this ж<http2clientConnReadLoop> Ꮡrl, ж<http2PingFrame> Ꮡf) => http_package.processPing⁔(Ꮡrl, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processPushPromise(this ref http2clientConnReadLoop rl, ж<http2PushPromiseFrame> Ꮡf) => http_package.processPushPromise⁔(ref rl, Ꮡf);
}
partial class httpᴛhttp2clientStream {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Func<nint, textproto.MIMEHeader, error> get1xxTraceFunc(this ref http2clientStream cs) => http_package.get1xxTraceFunc⁔(ref cs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void abortStream(this ж<http2clientStream> Ꮡcs, error err) => http_package.abortStream⁔(Ꮡcs, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void abortStreamLocked(this ж<http2clientStream> Ꮡcs, error err) => http_package.abortStreamLocked⁔(Ꮡcs, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void abortRequestBodyWrite(this ж<http2clientStream> Ꮡcs) => http_package.abortRequestBodyWrite⁔(Ꮡcs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeReqBodyLocked(this ж<http2clientStream> Ꮡcs) => http_package.closeReqBodyLocked⁔(Ꮡcs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void doRequest(this ж<http2clientStream> Ꮡcs, ж<Request> Ꮡreq, Action<ж<http2clientStream>> streamf) => http_package.doRequest⁔(Ꮡcs, Ꮡreq, streamf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error /*err*/ writeRequest(this ж<http2clientStream> Ꮡcs, ж<Request> Ꮡreq, Action<ж<http2clientStream>> streamf) => http_package.writeRequest⁔(Ꮡcs, Ꮡreq, streamf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error encodeAndWriteHeaders(this ж<http2clientStream> Ꮡcs, ж<Request> Ꮡreq) => http_package.encodeAndWriteHeaders⁔(Ꮡcs, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cleanupWriteRequest(this ж<http2clientStream> Ꮡcs, error err) => http_package.cleanupWriteRequest⁔(Ꮡcs, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint frameScratchBufferLen(this ref http2clientStream cs, nint maxFrameSize) => http_package.frameScratchBufferLen⁔(ref cs, maxFrameSize);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error /*err*/ writeRequestBody(this ж<http2clientStream> Ꮡcs, ж<Request> Ꮡreq) => http_package.writeRequestBody⁔(Ꮡcs, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int32 taken, error err) awaitFlowControl(this ж<http2clientStream> Ꮡcs, nint maxBytes) => http_package.awaitFlowControl⁔(Ꮡcs, maxBytes);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void copyTrailers(this ref http2clientStream cs) => http_package.copyTrailers⁔(ref cs);
}
partial class httpᴛhttp2closeWaiter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Init(this ref http2closeWaiter cw) => http_package.Init⁔(ref cw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Close(this http2closeWaiter cw) => http_package.Close⁔(cw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Wait(this http2closeWaiter cw) => http_package.Wait⁔(cw);
}
partial class httpᴛhttp2connError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this http2connError e) => http_package.Error⁔(e);
}
partial class httpᴛhttp2connOutflow {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref http2connOutflow f) => http_package.init⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool changeInitialWindowSize(this ref http2connOutflow f, int64 size) => http_package.changeInitialWindowSize⁔(ref f, size);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool add(this ref http2connOutflow f, int32 n) => http_package.add⁔(ref f, n);
}
partial class httpᴛhttp2dataBuffer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ref http2dataBuffer b, slice<byte> p) => http_package.Read⁔(ref b, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytesFromFirstChunk(this ref http2dataBuffer b) => http_package.bytesFromFirstChunk⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this ref http2dataBuffer b) => http_package.Len⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this ref http2dataBuffer b, slice<byte> p) => http_package.Write⁔(ref b, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> lastChunkOrAlloc(this ref http2dataBuffer b, int64 want) => http_package.lastChunkOrAlloc⁔(ref b, want);
}
partial class httpᴛhttp2dialCall {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void dial(this ref http2dialCall c, context.Context ctx, @string addr) => http_package.dial⁔(ref c, ctx, addr);
}
partial class httpᴛhttp2duplicatePseudoHeaderError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this http2duplicatePseudoHeaderError e) => http_package.Error⁔(e);
}
partial class httpᴛhttp2eofReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this http2eofReader _Δp0, slice<byte> _Δp1) => http_package.Read⁔(_Δp0, _Δp1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (byte, error) ReadByte(this http2eofReader _) => http_package.ReadByte⁔(_);
}
partial class httpᴛhttp2erringRoundTripper {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error RoundTripErr(this http2erringRoundTripper rt) => http_package.RoundTripErr⁔(rt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Response>, error) RoundTrip(this http2erringRoundTripper rt, ж<Request> _) => http_package.RoundTrip⁔(rt, _);
}
partial class httpᴛhttp2errorReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this http2errorReader r, slice<byte> p) => http_package.Read⁔(r, p);
}
partial class httpᴛhttp2flushFrameWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeFrame(this http2flushFrameWriter _, http2writeContext ctx) => http_package.writeFrame⁔(_, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool staysWithinBuffer(this http2flushFrameWriter _, nint max) => http_package.staysWithinBuffer⁔(_, max);
}
partial class httpᴛhttp2frameCache {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<http2DataFrame> getDataFrame(this ж<http2frameCache> Ꮡfc) => http_package.getDataFrame⁔(Ꮡfc);
}
partial class httpᴛhttp2goAwayFlowError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this http2goAwayFlowError _) => http_package.Error⁔(_);
}
partial class httpᴛhttp2goroutineLock {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void check(this http2goroutineLock g) => http_package.check⁔(g);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkNotOn(this http2goroutineLock g) => http_package.checkNotOn⁔(g);
}
partial class httpᴛhttp2gzipReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<gzip.Reader>, error) acquire(this ж<http2gzipReader> Ꮡgz) => http_package.acquire⁔(Ꮡgz);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void release(this ж<http2gzipReader> Ꮡgz, ж<gzip.Reader> Ꮡzr) => http_package.release⁔(Ꮡgz, Ꮡzr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void close(this ж<http2gzipReader> Ꮡgz) => http_package.close⁔(Ꮡgz);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ж<http2gzipReader> Ꮡgz, slice<byte> p) => http_package.Read⁔(Ꮡgz, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<http2gzipReader> Ꮡgz) => http_package.Close⁔(Ꮡgz);
}
partial class httpᴛhttp2handlerPanicRST {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeFrame(this http2handlerPanicRST hp, http2writeContext ctx) => http_package.writeFrame⁔(hp, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool staysWithinBuffer(this http2handlerPanicRST hp, nint max) => http_package.staysWithinBuffer⁔(hp, max);
}
partial class httpᴛhttp2headerFieldNameError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this http2headerFieldNameError e) => http_package.Error⁔(e);
}
partial class httpᴛhttp2headerFieldValueError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this http2headerFieldValueError e) => http_package.Error⁔(e);
}
partial class httpᴛhttp2httpError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this ref http2httpError e) => http_package.Error⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Timeout(this ref http2httpError e) => http_package.Timeout⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Temporary(this ref http2httpError e) => http_package.Temporary⁔(ref e);
}
partial class httpᴛhttp2inflow {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref http2inflow f, int32 n) => http_package.init⁔(ref f, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int32 /*connAdd*/ add(this ref http2inflow f, nint n) => http_package.add⁔(ref f, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool take(this ref http2inflow f, uint32 n) => http_package.take⁔(ref f, n);
}
partial class httpᴛhttp2missingBody {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this http2missingBody _) => http_package.Close⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this http2missingBody _Δp0, slice<byte> _Δp1) => http_package.Read⁔(_Δp0, _Δp1);
}
partial class httpᴛhttp2netHTTPClientConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Response>, error) RoundTrip(this http2netHTTPClientConn cc, ж<Request> Ꮡreq) => http_package.RoundTrip⁔(cc, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this http2netHTTPClientConn cc) => http_package.Close⁔(cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Err(this http2netHTTPClientConn cc) => http_package.Err⁔(cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Reserve(this http2netHTTPClientConn cc) => http_package.Reserve⁔(cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Release(this http2netHTTPClientConn cc) => http_package.Release⁔(cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Available(this http2netHTTPClientConn cc) => http_package.Available⁔(cc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint InFlight(this http2netHTTPClientConn cc) => http_package.InFlight⁔(cc);
}
partial class httpᴛhttp2noBodyReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this http2noBodyReader _) => http_package.Close⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this http2noBodyReader _Δp0, slice<byte> _Δp1) => http_package.Read⁔(_Δp0, _Δp1);
}
partial class httpᴛhttp2noCachedConnError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void IsHTTP2NoCachedConnError(this http2noCachedConnError _) => http_package.IsHTTP2NoCachedConnError⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this http2noCachedConnError _) => http_package.Error⁔(_);
}
partial class httpᴛhttp2noDialClientConnPool {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<http2ClientConn>, error) GetClientConn(this http2noDialClientConnPool p, ж<Request> Ꮡreq, @string addr) => http_package.GetClientConn⁔(p, Ꮡreq, addr);
}
partial class httpᴛhttp2noDialH2RoundTripper {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Response>, error) RoundTrip(this http2noDialH2RoundTripper rt, ж<Request> Ꮡreq) => http_package.RoundTrip⁔(rt, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (RoundTripper, error) NewClientConn(this http2noDialH2RoundTripper rt, net.Conn conn, Action internalStateHook) => http_package.NewClientConn⁔(rt, conn, internalStateHook);
}
partial class httpᴛhttp2outflow {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int32, bool) available(this ref http2outflow f) => http_package.available⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void take(this ref http2outflow f, int32 n) => http_package.take⁔(ref f, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool add(this ref http2outflow f, int32 n) => http_package.add⁔(ref f, n);
}
partial class httpᴛhttp2pipe {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setBuffer(this ж<http2pipe> Ꮡp, http2pipeBuffer b) => http_package.setBuffer⁔(Ꮡp, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this ж<http2pipe> Ꮡp) => http_package.Len⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ж<http2pipe> Ꮡp, slice<byte> d) => http_package.Read⁔(Ꮡp, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ж<http2pipe> Ꮡp, slice<byte> d) => http_package.Write⁔(Ꮡp, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void CloseWithError(this ж<http2pipe> Ꮡp, error err) => http_package.CloseWithError⁔(Ꮡp, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void BreakWithError(this ж<http2pipe> Ꮡp, error err) => http_package.BreakWithError⁔(Ꮡp, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeWithErrorAndCode(this ж<http2pipe> Ꮡp, error err, Action fn) => http_package.closeWithErrorAndCode⁔(Ꮡp, err, fn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeWithError(this ж<http2pipe> Ꮡp, ж<error> Ꮡdst, error err, Action fn) => http_package.closeWithError⁔(Ꮡp, Ꮡdst, err, fn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeDoneLocked(this ref http2pipe p) => http_package.closeDoneLocked⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Err(this ж<http2pipe> Ꮡp) => http_package.Err⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static /*<-*/channel<EmptyStruct> Done(this ж<http2pipe> Ꮡp) => http_package.Done⁔(Ꮡp);
}
partial class httpᴛhttp2priorityNodeRFC7540 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setParent(this ж<http2priorityNodeRFC7540> Ꮡn, ж<http2priorityNodeRFC7540> Ꮡparent) => http_package.setParent⁔(Ꮡn, Ꮡparent);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addBytes(this ж<http2priorityNodeRFC7540> Ꮡn, int64 b) => http_package.addBytes⁔(Ꮡn, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool walkReadyInOrder(this ж<http2priorityNodeRFC7540> Ꮡn, bool openParent, ж<slice<ж<http2priorityNodeRFC7540>>> Ꮡtmp, Func<ж<http2priorityNodeRFC7540>, bool, bool> f) => http_package.walkReadyInOrder⁔(Ꮡn, openParent, Ꮡtmp, f);
}
partial class httpᴛhttp2priorityWriteSchedulerRFC7540 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void OpenStream(this ж<http2priorityWriteSchedulerRFC7540> Ꮡws, uint32 streamID, http2OpenStreamOptions options) => http_package.OpenStream⁔(Ꮡws, streamID, options);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void CloseStream(this ж<http2priorityWriteSchedulerRFC7540> Ꮡws, uint32 streamID) => http_package.CloseStream⁔(Ꮡws, streamID);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void AdjustStream(this ж<http2priorityWriteSchedulerRFC7540> Ꮡws, uint32 streamID, http2PriorityParam priority) => http_package.AdjustStream⁔(Ꮡws, streamID, priority);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Push(this ж<http2priorityWriteSchedulerRFC7540> Ꮡws, http2FrameWriteRequest wr) => http_package.Push⁔(Ꮡws, wr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (http2FrameWriteRequest wr, bool ok) Pop(this ж<http2priorityWriteSchedulerRFC7540> Ꮡws) => http_package.Pop⁔(Ꮡws);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addClosedOrIdleNode(this ref http2priorityWriteSchedulerRFC7540 ws, ж<slice<ж<http2priorityNodeRFC7540>>> Ꮡlist, nint maxSize, ж<http2priorityNodeRFC7540> Ꮡn) => http_package.addClosedOrIdleNode⁔(ref ws, Ꮡlist, maxSize, Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void removeNode(this ref http2priorityWriteSchedulerRFC7540 ws, ж<http2priorityNodeRFC7540> Ꮡn) => http_package.removeNode⁔(ref ws, Ꮡn);
}
partial class httpᴛhttp2priorityWriteSchedulerRFC9218 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void OpenStream(this ref http2priorityWriteSchedulerRFC9218 ws, uint32 streamID, http2OpenStreamOptions opt) => http_package.OpenStream⁔(ref ws, streamID, opt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void CloseStream(this ref http2priorityWriteSchedulerRFC9218 ws, uint32 streamID) => http_package.CloseStream⁔(ref ws, streamID);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void AdjustStream(this ref http2priorityWriteSchedulerRFC9218 ws, uint32 streamID, http2PriorityParam priority) => http_package.AdjustStream⁔(ref ws, streamID, priority);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Push(this ref http2priorityWriteSchedulerRFC9218 ws, http2FrameWriteRequest wr) => http_package.Push⁔(ref ws, wr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (http2FrameWriteRequest, bool) Pop(this ref http2priorityWriteSchedulerRFC9218 ws) => http_package.Pop⁔(ref ws);
}
partial class httpᴛhttp2pseudoHeaderError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this http2pseudoHeaderError e) => http_package.Error⁔(e);
}
partial class httpᴛhttp2randomWriteScheduler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void OpenStream(this ref http2randomWriteScheduler ws, uint32 streamID, http2OpenStreamOptions options) => http_package.OpenStream⁔(ref ws, streamID, options);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void CloseStream(this ref http2randomWriteScheduler ws, uint32 streamID) => http_package.CloseStream⁔(ref ws, streamID);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void AdjustStream(this ref http2randomWriteScheduler ws, uint32 streamID, http2PriorityParam priority) => http_package.AdjustStream⁔(ref ws, streamID, priority);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Push(this ref http2randomWriteScheduler ws, http2FrameWriteRequest wr) => http_package.Push⁔(ref ws, wr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (http2FrameWriteRequest, bool) Pop(this ref http2randomWriteScheduler ws) => http_package.Pop⁔(ref ws);
}
partial class httpᴛhttp2requestBody {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<http2requestBody> Ꮡb) => http_package.Close⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ref http2requestBody b, slice<byte> p) => http_package.Read⁔(ref b, p);
}
partial class httpᴛhttp2responseWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetReadDeadline(this ref http2responseWriter w, time.Time deadline) => http_package.SetReadDeadline⁔(ref w, deadline);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetWriteDeadline(this ref http2responseWriter w, time.Time deadline) => http_package.SetWriteDeadline⁔(ref w, deadline);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error EnableFullDuplex(this ref http2responseWriter w) => http_package.EnableFullDuplex⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Flush(this ref http2responseWriter w) => http_package.Flush⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error FlushError(this ref http2responseWriter w) => http_package.FlushError⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static /*<-*/channel<bool> CloseNotify(this ref http2responseWriter w) => http_package.CloseNotify⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Header Header(this ref http2responseWriter w) => http_package.Header⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void WriteHeader(this ref http2responseWriter w, nint code) => http_package.WriteHeader⁔(ref w, code);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ref http2responseWriter w, slice<byte> p) => http_package.Write⁔(ref w, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) WriteString(this ref http2responseWriter w, @string s) => http_package.WriteString⁔(ref w, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) write(this ref http2responseWriter w, nint lenData, slice<byte> dataB, @string dataS) => http_package.write⁔(ref w, lenData, dataB, dataS);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void handlerDone(this ref http2responseWriter w) => http_package.handlerDone⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Push(this ref http2responseWriter w, @string target, ж<PushOptions> Ꮡopts) => http_package.Push⁔(ref w, target, Ꮡopts);
}
partial class httpᴛhttp2responseWriterState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasTrailers(this ref http2responseWriterState rws) => http_package.hasTrailers⁔(ref rws);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasNonemptyTrailers(this ref http2responseWriterState rws) => http_package.hasNonemptyTrailers⁔(ref rws);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void declareTrailer(this ref http2responseWriterState rws, @string k) => http_package.declareTrailer⁔(ref rws, k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) writeChunk(this ж<http2responseWriterState> Ꮡrws, slice<byte> p) => http_package.writeChunk⁔(Ꮡrws, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void promoteUndeclaredTrailers(this ref http2responseWriterState rws) => http_package.promoteUndeclaredTrailers⁔(ref rws);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeHeader(this ref http2responseWriterState rws, nint code) => http_package.writeHeader⁔(ref rws, code);
}
partial class httpᴛhttp2roundRobinWriteScheduler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void OpenStream(this ref http2roundRobinWriteScheduler ws, uint32 streamID, http2OpenStreamOptions options) => http_package.OpenStream⁔(ref ws, streamID, options);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void CloseStream(this ref http2roundRobinWriteScheduler ws, uint32 streamID) => http_package.CloseStream⁔(ref ws, streamID);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void AdjustStream(this ref http2roundRobinWriteScheduler ws, uint32 streamID, http2PriorityParam priority) => http_package.AdjustStream⁔(ref ws, streamID, priority);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Push(this ref http2roundRobinWriteScheduler ws, http2FrameWriteRequest wr) => http_package.Push⁔(ref ws, wr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (http2FrameWriteRequest, bool) Pop(this ref http2roundRobinWriteScheduler ws) => http_package.Pop⁔(ref ws);
}
partial class httpᴛhttp2serverConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void rejectConn(this ref http2serverConn sc, http2ErrCode err, @string debug) => http_package.rejectConn⁔(ref sc, err, debug);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 maxHeaderListSize(this ref http2serverConn sc) => http_package.maxHeaderListSize⁔(ref sc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 curOpenStreams(this ref http2serverConn sc) => http_package.curOpenStreams⁔(ref sc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<http2Framer> Framer(this ref http2serverConn sc) => http_package.Framer⁔(ref sc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error CloseConn(this ref http2serverConn sc) => http_package.CloseConn⁔(ref sc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Flush(this ref http2serverConn sc) => http_package.Flush⁔(ref sc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<hpack.Encoder>, ж<bytes.Buffer>) HeaderEncoder(this ж<http2serverConn> Ꮡsc) => http_package.HeaderEncoder⁔(Ꮡsc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (http2streamState, ж<http2stream>) state(this ref http2serverConn sc, uint32 streamID) => http_package.state⁔(ref sc, streamID);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setConnState(this ref http2serverConn sc, ConnState state) => http_package.setConnState⁔(ref sc, state);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void vlogf(this ref http2serverConn sc, @string format, params ꓸꓸꓸany argsʗp) => http_package.vlogf⁔(ref sc, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void logf(this ref http2serverConn sc, @string format, params ꓸꓸꓸany argsʗp) => http_package.logf⁔(ref sc, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void condlogf(this ref http2serverConn sc, error err, @string format, params ꓸꓸꓸany argsʗp) => http_package.condlogf⁔(ref sc, err, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string canonicalHeader(this ref http2serverConn sc, @string v) => http_package.canonicalHeader⁔(ref sc, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readFrames(this ref http2serverConn sc) => http_package.readFrames⁔(ref sc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeFrameAsync(this ж<http2serverConn> Ꮡsc, http2FrameWriteRequest wr, ж<http2writeData> Ꮡwd) => http_package.writeFrameAsync⁔(Ꮡsc, wr, Ꮡwd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeAllStreamsOnConnClose(this ж<http2serverConn> Ꮡsc) => http_package.closeAllStreamsOnConnClose⁔(Ꮡsc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stopShutdownTimer(this ref http2serverConn sc) => http_package.stopShutdownTimer⁔(ref sc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void notePanic(this ж<http2serverConn> Ꮡsc) => http_package.notePanic⁔(Ꮡsc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void serve(this ж<http2serverConn> Ꮡsc, http2http2Config conf) => http_package.serve⁔(Ꮡsc, conf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void handlePingTimer(this ж<http2serverConn> Ꮡsc, time.Time lastFrameReadTime) => http_package.handlePingTimer⁔(Ꮡsc, lastFrameReadTime);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void onSettingsTimer(this ref http2serverConn sc) => http_package.onSettingsTimer⁔(ref sc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void onIdleTimer(this ref http2serverConn sc) => http_package.onIdleTimer⁔(ref sc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void onReadIdleTimer(this ref http2serverConn sc) => http_package.onReadIdleTimer⁔(ref sc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void onShutdownTimer(this ref http2serverConn sc) => http_package.onShutdownTimer⁔(ref sc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sendServeMsg(this ref http2serverConn sc, any msg) => http_package.sendServeMsg⁔(ref sc, msg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readPreface(this ж<http2serverConn> Ꮡsc) => http_package.readPreface⁔(Ꮡsc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeDataFromHandler(this ref http2serverConn sc, ж<http2stream> Ꮡstream, slice<byte> data, bool endStream) => http_package.writeDataFromHandler⁔(ref sc, Ꮡstream, data, endStream);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeFrameFromHandler(this ref http2serverConn sc, http2FrameWriteRequest wr) => http_package.writeFrameFromHandler⁔(ref sc, wr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeFrame(this ж<http2serverConn> Ꮡsc, http2FrameWriteRequest wr) => http_package.writeFrame⁔(Ꮡsc, wr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void startFrameWrite(this ж<http2serverConn> Ꮡsc, http2FrameWriteRequest wr) => http_package.startFrameWrite⁔(Ꮡsc, wr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void wroteFrame(this ж<http2serverConn> Ꮡsc, http2frameWriteResult res) => http_package.wroteFrame⁔(Ꮡsc, res);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void scheduleFrameWrite(this ж<http2serverConn> Ꮡsc) => http_package.scheduleFrameWrite⁔(Ꮡsc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void startGracefulShutdown(this ж<http2serverConn> Ꮡsc) => http_package.startGracefulShutdown⁔(Ꮡsc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void startGracefulShutdownInternal(this ж<http2serverConn> Ꮡsc) => http_package.startGracefulShutdownInternal⁔(Ꮡsc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void goAway(this ж<http2serverConn> Ꮡsc, http2ErrCode code) => http_package.goAway⁔(Ꮡsc, code);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void shutDownIn(this ж<http2serverConn> Ꮡsc, time.Duration d) => http_package.shutDownIn⁔(Ꮡsc, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void resetStream(this ж<http2serverConn> Ꮡsc, http2StreamError se) => http_package.resetStream⁔(Ꮡsc, se);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool processFrameFromReader(this ж<http2serverConn> Ꮡsc, http2readFrameResult res) => http_package.processFrameFromReader⁔(Ꮡsc, res);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processFrame(this ж<http2serverConn> Ꮡsc, http2Frame f) => http_package.processFrame⁔(Ꮡsc, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processPing(this ж<http2serverConn> Ꮡsc, ж<http2PingFrame> Ꮡf) => http_package.processPing⁔(Ꮡsc, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processWindowUpdate(this ж<http2serverConn> Ꮡsc, ж<http2WindowUpdateFrame> Ꮡf) => http_package.processWindowUpdate⁔(Ꮡsc, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processResetStream(this ж<http2serverConn> Ꮡsc, ж<http2RSTStreamFrame> Ꮡf) => http_package.processResetStream⁔(Ꮡsc, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeStream(this ж<http2serverConn> Ꮡsc, ж<http2stream> Ꮡst, error err) => http_package.closeStream⁔(Ꮡsc, Ꮡst, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processSettings(this ж<http2serverConn> Ꮡsc, ж<http2SettingsFrame> Ꮡf) => http_package.processSettings⁔(Ꮡsc, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processSetting(this ж<http2serverConn> Ꮡsc, http2Setting s) => http_package.processSetting⁔(Ꮡsc, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processSettingInitialWindowSize(this ж<http2serverConn> Ꮡsc, uint32 val) => http_package.processSettingInitialWindowSize⁔(Ꮡsc, val);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processData(this ж<http2serverConn> Ꮡsc, ж<http2DataFrame> Ꮡf) => http_package.processData⁔(Ꮡsc, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processGoAway(this ж<http2serverConn> Ꮡsc, ж<http2GoAwayFrame> Ꮡf) => http_package.processGoAway⁔(Ꮡsc, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processHeaders(this ж<http2serverConn> Ꮡsc, ж<http2MetaHeadersFrame> Ꮡf) => http_package.processHeaders⁔(Ꮡsc, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void upgradeRequest(this ж<http2serverConn> Ꮡsc, ж<Request> Ꮡreq) => http_package.upgradeRequest⁔(Ꮡsc, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error checkPriority(this ж<http2serverConn> Ꮡsc, uint32 streamID, http2PriorityParam p) => http_package.checkPriority⁔(Ꮡsc, streamID, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processPriority(this ж<http2serverConn> Ꮡsc, ж<http2PriorityFrame> Ꮡf) => http_package.processPriority⁔(Ꮡsc, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<http2stream> newStream(this ж<http2serverConn> Ꮡsc, uint32 id, uint32 pusherID, http2streamState state) => http_package.newStream⁔(Ꮡsc, id, pusherID, state);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<http2responseWriter>, ж<Request>, error) newWriterAndRequest(this ж<http2serverConn> Ꮡsc, ж<http2stream> Ꮡst, ж<http2MetaHeadersFrame> Ꮡf) => http_package.newWriterAndRequest⁔(Ꮡsc, Ꮡst, Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<http2responseWriter>, ж<Request>, error) newWriterAndRequestNoBody(this ж<http2serverConn> Ꮡsc, ж<http2stream> Ꮡst, httpcommon.ServerRequestParam rp) => http_package.newWriterAndRequestNoBody⁔(Ꮡsc, Ꮡst, rp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<http2responseWriter> newResponseWriter(this ж<http2serverConn> Ꮡsc, ж<http2stream> Ꮡst, ж<Request> Ꮡreq) => http_package.newResponseWriter⁔(Ꮡsc, Ꮡst, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error scheduleHandler(this ж<http2serverConn> Ꮡsc, ж<http2stream> Ꮡst, ж<http2responseWriter> Ꮡrw, ж<Request> Ꮡreq, Action<ResponseWriter, ж<Request>> handler) => http_package.scheduleHandler⁔(Ꮡsc, Ꮡst, Ꮡrw, Ꮡreq, handler);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void handlerDone(this ж<http2serverConn> Ꮡsc) => http_package.handlerDone⁔(Ꮡsc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void runHandler(this ж<http2serverConn> Ꮡsc, ж<http2responseWriter> Ꮡrw, ж<Request> Ꮡreq, Action<ResponseWriter, ж<Request>> handler) => http_package.runHandler⁔(Ꮡsc, Ꮡrw, Ꮡreq, handler);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeHeaders(this ref http2serverConn sc, ж<http2stream> Ꮡst, ж<http2writeResHeaders> ᏑheaderData) => http_package.writeHeaders⁔(ref sc, Ꮡst, ᏑheaderData);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void write100ContinueHeaders(this ref http2serverConn sc, ж<http2stream> Ꮡst) => http_package.write100ContinueHeaders⁔(ref sc, Ꮡst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void noteBodyReadFromHandler(this ref http2serverConn sc, ж<http2stream> Ꮡst, nint n, error err) => http_package.noteBodyReadFromHandler⁔(ref sc, Ꮡst, n, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void noteBodyRead(this ж<http2serverConn> Ꮡsc, ж<http2stream> Ꮡst, nint n) => http_package.noteBodyRead⁔(Ꮡsc, Ꮡst, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sendWindowUpdate32(this ж<http2serverConn> Ꮡsc, ж<http2stream> Ꮡst, int32 n) => http_package.sendWindowUpdate32⁔(Ꮡsc, Ꮡst, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sendWindowUpdate(this ж<http2serverConn> Ꮡsc, ж<http2stream> Ꮡst, nint n) => http_package.sendWindowUpdate⁔(Ꮡsc, Ꮡst, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void startPush(this ж<http2serverConn> Ꮡsc, ж<http2startPushRequest> Ꮡmsg) => http_package.startPush⁔(Ꮡsc, Ꮡmsg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error countError(this ж<http2serverConn> Ꮡsc, @string name, error err) => http_package.countError⁔(Ꮡsc, name, err);
}
partial class httpᴛhttp2serverInternalState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void registerConn(this ж<http2serverInternalState> Ꮡs, ж<http2serverConn> Ꮡsc) => http_package.registerConn⁔(Ꮡs, Ꮡsc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void unregisterConn(this ж<http2serverInternalState> Ꮡs, ж<http2serverConn> Ꮡsc) => http_package.unregisterConn⁔(Ꮡs, Ꮡsc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void startGracefulShutdown(this ж<http2serverInternalState> Ꮡs) => http_package.startGracefulShutdown⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static channel<error> getErrChan(this ж<http2serverInternalState> Ꮡs) => http_package.getErrChan⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void putErrChan(this ж<http2serverInternalState> Ꮡs, channel<error> ch) => http_package.putErrChan⁔(Ꮡs, ch);
}
partial class httpᴛhttp2sortPriorityNodeSiblingsRFC7540 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this http2sortPriorityNodeSiblingsRFC7540 z) => http_package.Len⁔(z);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Swap(this http2sortPriorityNodeSiblingsRFC7540 z, nint i, nint k) => http_package.Swap⁔(z, i, k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Less(this http2sortPriorityNodeSiblingsRFC7540 z, nint i, nint k) => http_package.Less⁔(z, i, k);
}
partial class httpᴛhttp2sorter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this ref http2sorter s) => http_package.Len⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Swap(this ref http2sorter s, nint i, nint j) => http_package.Swap⁔(ref s, i, j);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Less(this ref http2sorter s, nint i, nint j) => http_package.Less⁔(ref s, i, j);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<@string> Keys(this ж<http2sorter> Ꮡs, Header h) => http_package.Keys⁔(Ꮡs, h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void SortStrings(this ж<http2sorter> Ꮡs, slice<@string> ss) => http_package.SortStrings⁔(Ꮡs, ss);
}
partial class httpᴛhttp2stickyErrWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this http2stickyErrWriter sew, slice<byte> p) => http_package.Write⁔(sew, p);
}
partial class httpᴛhttp2stream {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isPushed(this ref http2stream st) => http_package.isPushed⁔(ref st);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void endStream(this ж<http2stream> Ꮡst) => http_package.endStream⁔(Ꮡst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void copyTrailersToHandlerRequest(this ref http2stream st) => http_package.copyTrailersToHandlerRequest⁔(ref st);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void onReadTimeout(this ref http2stream st) => http_package.onReadTimeout⁔(ref st);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void onWriteTimeout(this ref http2stream st) => http_package.onWriteTimeout⁔(ref st);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error processTrailerHeaders(this ж<http2stream> Ꮡst, ж<http2MetaHeadersFrame> Ꮡf) => http_package.processTrailerHeaders⁔(Ꮡst, Ꮡf);
}
partial class httpᴛhttp2streamState {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this http2streamState st) => http_package.String⁔(st);
}
partial class httpᴛhttp2transportResponseBody {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this http2transportResponseBody b, slice<byte> p) => http_package.Read⁔(b, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this http2transportResponseBody b) => http_package.Close⁔(b);
}
partial class httpᴛhttp2unencryptedTransport {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Response>, error) RoundTrip(this ж<http2unencryptedTransport> Ꮡt, ж<Request> Ꮡreq) => http_package.RoundTrip⁔(Ꮡt, Ꮡreq);
}
partial class httpᴛhttp2write100ContinueHeadersFrame {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeFrame(this http2write100ContinueHeadersFrame w, http2writeContext ctx) => http_package.writeFrame⁔(w, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool staysWithinBuffer(this http2write100ContinueHeadersFrame w, nint max) => http_package.staysWithinBuffer⁔(w, max);
}
partial class httpᴛhttp2writeData {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref http2writeData w) => http_package.String⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeFrame(this ref http2writeData w, http2writeContext ctx) => http_package.writeFrame⁔(ref w, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool staysWithinBuffer(this ref http2writeData w, nint max) => http_package.staysWithinBuffer⁔(ref w, max);
}
partial class httpᴛhttp2writeGoAway {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeFrame(this ref http2writeGoAway p, http2writeContext ctx) => http_package.writeFrame⁔(ref p, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool staysWithinBuffer(this ref http2writeGoAway _, nint max) => http_package.staysWithinBuffer⁔(ref _, max);
}
partial class httpᴛhttp2writePing {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeFrame(this http2writePing w, http2writeContext ctx) => http_package.writeFrame⁔(w, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool staysWithinBuffer(this http2writePing w, nint max) => http_package.staysWithinBuffer⁔(w, max);
}
partial class httpᴛhttp2writePingAck {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeFrame(this http2writePingAck w, http2writeContext ctx) => http_package.writeFrame⁔(w, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool staysWithinBuffer(this http2writePingAck w, nint max) => http_package.staysWithinBuffer⁔(w, max);
}
partial class httpᴛhttp2writePushPromise {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool staysWithinBuffer(this ref http2writePushPromise w, nint max) => http_package.staysWithinBuffer⁔(ref w, max);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeFrame(this ж<http2writePushPromise> Ꮡw, http2writeContext ctx) => http_package.writeFrame⁔(Ꮡw, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeHeaderBlock(this ref http2writePushPromise w, http2writeContext ctx, slice<byte> frag, bool firstFrag, bool lastFrag) => http_package.writeHeaderBlock⁔(ref w, ctx, frag, firstFrag, lastFrag);
}
partial class httpᴛhttp2writeQueue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this ref http2writeQueue q) => http_package.empty⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void push(this ref http2writeQueue q, http2FrameWriteRequest wr) => http_package.push⁔(ref q, wr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static http2FrameWriteRequest shift(this ref http2writeQueue q) => http_package.shift⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<http2FrameWriteRequest> peek(this ref http2writeQueue q) => http_package.peek⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (http2FrameWriteRequest, bool) consume(this ref http2writeQueue q, int32 n) => http_package.consume⁔(ref q, n);
}
partial class httpᴛhttp2writeQueuePool {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void put(this ref http2writeQueuePool p, ж<http2writeQueue> Ꮡq) => http_package.put⁔(ref p, Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<http2writeQueue> get(this ref http2writeQueuePool p) => http_package.get⁔(ref p);
}
partial class httpᴛhttp2writeResHeaders {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool staysWithinBuffer(this ref http2writeResHeaders w, nint max) => http_package.staysWithinBuffer⁔(ref w, max);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeFrame(this ж<http2writeResHeaders> Ꮡw, http2writeContext ctx) => http_package.writeFrame⁔(Ꮡw, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeHeaderBlock(this ref http2writeResHeaders w, http2writeContext ctx, slice<byte> frag, bool firstFrag, bool lastFrag) => http_package.writeHeaderBlock⁔(ref w, ctx, frag, firstFrag, lastFrag);
}
partial class httpᴛhttp2writeSettings {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool staysWithinBuffer(this http2writeSettings s, nint max) => http_package.staysWithinBuffer⁔(s, max);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeFrame(this http2writeSettings s, http2writeContext ctx) => http_package.writeFrame⁔(s, ctx);
}
partial class httpᴛhttp2writeSettingsAck {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeFrame(this http2writeSettingsAck _, http2writeContext ctx) => http_package.writeFrame⁔(_, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool staysWithinBuffer(this http2writeSettingsAck _, nint max) => http_package.staysWithinBuffer⁔(_, max);
}
partial class httpᴛhttp2writeWindowUpdate {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool staysWithinBuffer(this http2writeWindowUpdate wu, nint max) => http_package.staysWithinBuffer⁔(wu, max);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeFrame(this http2writeWindowUpdate wu, http2writeContext ctx) => http_package.writeFrame⁔(wu, ctx);
}
partial class httpᴛhttpRange {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string contentRange(this httpRange r, int64 size) => http_package.contentRange⁔(r, size);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static textproto.MIMEHeader mimeHeader(this httpRange r, @string contentType, int64 size) => http_package.mimeHeader⁔(r, contentType, size);
}
partial class httpᴛinitALPNRequest {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static context.Context BaseContext(this initALPNRequest h) => http_package.BaseContext⁔(h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ServeHTTP(this initALPNRequest h, ResponseWriter rw, ж<Request> Ꮡreq) => http_package.ServeHTTP⁔(h, rw, Ꮡreq);
}
partial class httpᴛioFS {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (File, error) Open(this ioFS f, @string name) => http_package.Open⁔(f, name);
}
partial class httpᴛioFile {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ioFile f) => http_package.Close⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ioFile f, slice<byte> b) => http_package.Read⁔(f, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (fs.FileInfo, error) Stat(this ioFile f) => http_package.Stat⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) Seek(this ioFile f, int64 offset, nint whence) => http_package.Seek⁔(f, offset, whence);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<fs.DirEntry>, error) ReadDir(this ioFile f, nint count) => http_package.ReadDir⁔(f, count);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<fs.FileInfo>, error) Readdir(this ioFile f, nint count) => http_package.Readdir⁔(f, count);
}
partial class httpᴛloggingConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static net.Addr LocalAddr(this loggingConn recvᴛ) => http_package.LocalAddr⁔(recvᴛ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static net.Addr RemoteAddr(this loggingConn recvᴛ) => http_package.RemoteAddr⁔(recvᴛ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetDeadline(this loggingConn recvᴛ, time.Time t) => http_package.SetDeadline⁔(recvᴛ, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetReadDeadline(this loggingConn recvᴛ, time.Time t) => http_package.SetReadDeadline⁔(recvᴛ, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetWriteDeadline(this loggingConn recvᴛ, time.Time t) => http_package.SetWriteDeadline⁔(recvᴛ, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ref loggingConn c, slice<byte> p) => http_package.Write⁔(ref c, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ref loggingConn c, slice<byte> p) => http_package.Read⁔(ref c, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error /*err*/ Close(this ref loggingConn c) => http_package.Close⁔(ref c);
}
partial class httpᴛmapping {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add<K, V>(this ref mapping<K, V> h, K k, V v) => http_package.add⁔<K, V>(ref h, k, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (V v, bool found) find<K, V>(this ж<mapping<K, V>> Ꮡh, K k) => http_package.find⁔<K, V>(Ꮡh, k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void eachPair<K, V>(this ж<mapping<K, V>> Ꮡh, Func<K, V, bool> f) => http_package.eachPair⁔<K, V>(Ꮡh, f);
}
partial class httpᴛmaskedSig {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string match(this ref maskedSig m, slice<byte> data, nint firstNonWS) => http_package.match⁔(ref m, data, firstNonWS);
}
partial class httpᴛmaxBytesReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ref maxBytesReader l, slice<byte> p) => http_package.Read⁔(ref l, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref maxBytesReader l) => http_package.Close⁔(ref l);
}
partial class httpᴛmp4Sig {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string match(this mp4Sig _, slice<byte> data, nint firstNonWS) => http_package.match⁔(_, data, firstNonWS);
}
partial class httpᴛnoBody {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) Read(this noBody _Δp0, slice<byte> _Δp1) => http_package.Read⁔(_Δp0, _Δp1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this noBody _) => http_package.Close⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64, error) WriteTo(this noBody _Δp0, io.Writer _Δp1) => http_package.WriteTo⁔(_Δp0, _Δp1);
}
partial class httpᴛnoopHandler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ServeHTTP(this noopHandler _Δp0, ResponseWriter _Δp1, ж<Request> _Δp2) => http_package.ServeHTTP⁔(_Δp0, _Δp1, _Δp2);
}
partial class httpᴛnothingWrittenError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Unwrap(this nothingWrittenError nwe) => http_package.Unwrap⁔(nwe);
}
partial class httpᴛonceCloseListener {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (net.Conn, error) Accept(this onceCloseListener recvᴛ) => http_package.Accept⁔(recvᴛ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static net.Addr Addr(this onceCloseListener recvᴛ) => http_package.Addr⁔(recvᴛ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<onceCloseListener> Ꮡoc) => http_package.Close⁔(Ꮡoc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void close(this ref onceCloseListener oc) => http_package.close⁔(ref oc);
}
partial class httpᴛpattern {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref pattern p) => http_package.String⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static segment lastSegment(this ref pattern p) => http_package.lastSegment⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool conflictsWith(this ref pattern p1, ж<pattern> Ꮡp2) => http_package.conflictsWith⁔(ref p1, Ꮡp2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static relationship comparePathsAndMethods(this ref pattern p1, ж<pattern> Ꮡp2) => http_package.comparePathsAndMethods⁔(ref p1, Ꮡp2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static relationship compareMethods(this ref pattern p1, ж<pattern> Ꮡp2) => http_package.compareMethods⁔(ref p1, Ꮡp2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static relationship comparePaths(this ref pattern p1, ж<pattern> Ꮡp2) => http_package.comparePaths⁔(ref p1, Ꮡp2);
}
partial class httpᴛpersistConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool shouldRetryRequest(this ж<persistConn> Ꮡpc, ж<Request> Ꮡreq, error err) => http_package.shouldRetryRequest⁔(Ꮡpc, Ꮡreq, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error addTLS(this ref persistConn pconn, context.Context ctx, @string name, ж<httptrace.ClientTrace> Ꮡtrace) => http_package.addTLS⁔(ref pconn, ctx, name, Ꮡtrace);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 maxHeaderResponseSize(this ref persistConn pc) => http_package.maxHeaderResponseSize⁔(ref pc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ref persistConn pc, slice<byte> p) => http_package.Read⁔(ref pc, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isBroken(this ж<persistConn> Ꮡpc) => http_package.isBroken⁔(Ꮡpc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error canceled(this ж<persistConn> Ꮡpc) => http_package.canceled⁔(Ꮡpc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isReused(this ж<persistConn> Ꮡpc) => http_package.isReused⁔(Ꮡpc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cancelRequest(this ж<persistConn> Ꮡpc, error err) => http_package.cancelRequest⁔(Ꮡpc, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeConnIfStillIdle(this ж<persistConn> Ꮡpc) => http_package.closeConnIfStillIdle⁔(Ꮡpc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error mapRoundTripError(this ж<persistConn> Ꮡpc, ж<transportRequest> Ꮡreq, int64 startBytesWritten, error err) => http_package.mapRoundTripError⁔(Ꮡpc, Ꮡreq, startBytesWritten, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readLoop(this ж<persistConn> Ꮡpc) => http_package.readLoop⁔(Ꮡpc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void readLoopPeekFailLocked(this ref persistConn pc, error peekErr) => http_package.readLoopPeekFailLocked⁔(ref pc, peekErr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Response> resp, error err) readResponse(this ref persistConn pc, requestAndChan rc, ж<httptrace.ClientTrace> Ꮡtrace) => http_package.readResponse⁔(ref pc, rc, Ꮡtrace);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Func<bool> waitForContinue(this ж<persistConn> Ꮡpc, /*<-*/channel<EmptyStruct> continueCh) => http_package.waitForContinue⁔(Ꮡpc, continueCh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeLoop(this ж<persistConn> Ꮡpc) => http_package.writeLoop⁔(Ꮡpc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool wroteRequest(this ж<persistConn> Ꮡpc) => http_package.wroteRequest⁔(Ꮡpc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error waitForAvailability(this ref persistConn pc, context.Context ctx) => http_package.waitForAvailability⁔(ref pc, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Response> resp, error err) roundTrip(this ж<persistConn> Ꮡpc, ж<transportRequest> Ꮡreq) => http_package.roundTrip⁔(Ꮡpc, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void markReused(this ж<persistConn> Ꮡpc) => http_package.markReused⁔(Ꮡpc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void close(this ж<persistConn> Ꮡpc, error err) => http_package.close⁔(Ꮡpc, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeLocked(this ref persistConn pc, error err) => http_package.closeLocked⁔(ref pc, err);
}
partial class httpᴛpersistConnWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this persistConnWriter w, slice<byte> p) => http_package.Write⁔(w, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64 n, error err) ReadFrom(this persistConnWriter w, io.Reader r) => http_package.ReadFrom⁔(w, r);
}
partial class httpᴛpopulateResponse {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void finish(this ref populateResponse pr) => http_package.finish⁔(ref pr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sendResponse(this ref populateResponse pr) => http_package.sendResponse⁔(ref pr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Header Header(this ref populateResponse pr) => http_package.Header⁔(ref pr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void WriteHeader(this ref populateResponse pr, nint code) => http_package.WriteHeader⁔(ref pr, code);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ref populateResponse pr, slice<byte> p) => http_package.Write⁔(ref pr, p);
}
partial class httpᴛreadTrackingBody {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ref readTrackingBody r, slice<byte> data) => http_package.Read⁔(ref r, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<readTrackingBody> Ꮡr) => http_package.Close⁔(Ꮡr);
}
partial class httpᴛreadWriteCloserBody {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this readWriteCloserBody recvᴛ) => http_package.Close⁔(recvᴛ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this readWriteCloserBody recvᴛ, slice<byte> p) => http_package.Write⁔(recvᴛ, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ref readWriteCloserBody b, slice<byte> p) => http_package.Read⁔(ref b, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error CloseWrite(this ref readWriteCloserBody b) => http_package.CloseWrite⁔(ref b);
}
partial class httpᴛredirectHandler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ServeHTTP(this ref redirectHandler rh, ResponseWriter w, ж<Request> Ꮡr) => http_package.ServeHTTP⁔(ref rh, w, Ꮡr);
}
partial class httpᴛresponse {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetReadDeadline(this ref response c, time.Time deadline) => http_package.SetReadDeadline⁔(ref c, deadline);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetWriteDeadline(this ref response c, time.Time deadline) => http_package.SetWriteDeadline⁔(ref c, deadline);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error EnableFullDuplex(this ref response c) => http_package.EnableFullDuplex⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Header finalTrailers(this ref response w) => http_package.finalTrailers⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void declareTrailer(this ref response w, @string k) => http_package.declareTrailer⁔(ref w, k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void requestTooLarge(this ж<response> Ꮡw) => http_package.requestTooLarge⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void disableWriteContinue(this ж<response> Ꮡw) => http_package.disableWriteContinue⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64 n, error err) ReadFrom(this ж<response> Ꮡw, io.Reader src) => http_package.ReadFrom⁔(Ꮡw, src);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Header Header(this ref response w) => http_package.Header⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void WriteHeader(this ж<response> Ꮡw, nint code) => http_package.WriteHeader⁔(Ꮡw, code);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool bodyAllowed(this ref response w) => http_package.bodyAllowed⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ж<response> Ꮡw, slice<byte> data) => http_package.Write⁔(Ꮡw, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) WriteString(this ж<response> Ꮡw, @string data) => http_package.WriteString⁔(Ꮡw, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) write(this ж<response> Ꮡw, nint lenData, slice<byte> dataB, @string dataS) => http_package.write⁔(Ꮡw, lenData, dataB, dataS);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void finishRequest(this ж<response> Ꮡw) => http_package.finishRequest⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool shouldReuseConnection(this ж<response> Ꮡw) => http_package.shouldReuseConnection⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool closedRequestBodyEarly(this ref response w) => http_package.closedRequestBodyEarly⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Flush(this ж<response> Ꮡw) => http_package.Flush⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error FlushError(this ж<response> Ꮡw) => http_package.FlushError⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sendExpectationFailed(this ж<response> Ꮡw) => http_package.sendExpectationFailed⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (net.Conn rwc, ж<bufio.ReadWriter> buf, error err) Hijack(this ж<response> Ꮡw) => http_package.Hijack⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static /*<-*/channel<bool> CloseNotify(this ж<response> Ꮡw) => http_package.CloseNotify⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeNotify(this ж<response> Ꮡw) => http_package.closeNotify⁔(Ꮡw);
}
partial class httpᴛroutingIndex {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addPattern(this ref routingIndex idx, ж<pattern> Ꮡpat) => http_package.addPattern⁔(ref idx, Ꮡpat);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error /*err*/ possiblyConflictingPatterns(this ref routingIndex idx, ж<pattern> Ꮡpat, Func<ж<pattern>, error> f) => http_package.possiblyConflictingPatterns⁔(ref idx, Ꮡpat, f);
}
partial class httpᴛroutingNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addPattern(this ж<routingNode> Ꮡroot, ж<pattern> Ꮡp, Handler h) => http_package.addPattern⁔(Ꮡroot, Ꮡp, h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addSegments(this ж<routingNode> Ꮡn, slice<segment> segs, ж<pattern> Ꮡp, Handler h) => http_package.addSegments⁔(Ꮡn, segs, Ꮡp, h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void set(this ref routingNode n, ж<pattern> Ꮡp, Handler h) => http_package.set⁔(ref n, Ꮡp, h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<routingNode> addChild(this ж<routingNode> Ꮡn, @string key) => http_package.addChild⁔(Ꮡn, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<routingNode> findChild(this ж<routingNode> Ꮡn, @string key) => http_package.findChild⁔(Ꮡn, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<routingNode>, slice<@string>) match(this ж<routingNode> Ꮡroot, @string host, @string method, @string path) => http_package.match⁔(Ꮡroot, host, method, path);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<routingNode>, slice<@string>) matchMethodAndPath(this ж<routingNode> Ꮡn, @string method, @string path) => http_package.matchMethodAndPath⁔(Ꮡn, method, path);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<routingNode>, slice<@string>) matchPath(this ж<routingNode> Ꮡn, @string path, slice<@string> matches) => http_package.matchPath⁔(Ꮡn, path, matches);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void matchingMethods(this ж<routingNode> Ꮡroot, @string host, @string path, map<@string, bool> methodSet) => http_package.matchingMethods⁔(Ꮡroot, host, path, methodSet);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void matchingMethodsPath(this ж<routingNode> Ꮡn, @string path, map<@string, bool> set) => http_package.matchingMethodsPath⁔(Ꮡn, path, set);
}
partial class httpᴛserveMux121 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void handle(this ж<serveMux121> Ꮡmux, @string pattern, Handler handler) => http_package.handle⁔(Ꮡmux, pattern, handler);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void handleFunc(this ж<serveMux121> Ꮡmux, @string pattern, Action<ResponseWriter, ж<Request>> handler) => http_package.handleFunc⁔(Ꮡmux, pattern, handler);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Handler h, @string pattern) findHandler(this ж<serveMux121> Ꮡmux, ж<Request> Ꮡr) => http_package.findHandler⁔(Ꮡmux, Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Handler h, @string pattern) handler(this ж<serveMux121> Ꮡmux, @string host, @string path) => http_package.handler⁔(Ꮡmux, host, path);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Handler h, @string pattern) match(this ref serveMux121 mux, @string path) => http_package.match⁔(ref mux, path);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<url.URL>, bool) redirectToPathSlash(this ж<serveMux121> Ꮡmux, @string host, @string path, ж<url.URL> Ꮡu) => http_package.redirectToPathSlash⁔(Ꮡmux, host, path, Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool shouldRedirectRLocked(this ref serveMux121 mux, @string host, @string path) => http_package.shouldRedirectRLocked⁔(ref mux, host, path);
}
partial class httpᴛserverHandler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ServeHTTP(this serverHandler sh, ResponseWriter rw, ж<Request> Ꮡreq) => http_package.ServeHTTP⁔(sh, rw, Ꮡreq);
}
partial class httpᴛsocksAddr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Network(this ref socksAddr a) => http_package.Network⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ж<socksAddr> Ꮡa) => http_package.String⁔(Ꮡa);
}
partial class httpᴛsocksCommand {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this socksCommand cmd) => http_package.String⁔(cmd);
}
partial class httpᴛsocksConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static net.Addr BoundAddr(this ж<socksConn> Ꮡc) => http_package.BoundAddr⁔(Ꮡc);
}
partial class httpᴛsocksDialer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (net.Addr, error ctxErr) connect(this ж<socksDialer> Ꮡd, context.Context ctx, net.Conn c, @string address) => http_package.connect⁔(Ꮡd, ctx, c, address);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (net.Conn, error) DialContext(this ж<socksDialer> Ꮡd, context.Context ctx, @string network, @string address) => http_package.DialContext⁔(Ꮡd, ctx, network, address);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (net.Addr, error) DialWithConn(this ж<socksDialer> Ꮡd, context.Context ctx, net.Conn c, @string network, @string address) => http_package.DialWithConn⁔(Ꮡd, ctx, c, network, address);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (net.Conn, error) Dial(this ж<socksDialer> Ꮡd, @string network, @string address) => http_package.Dial⁔(Ꮡd, network, address);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error validateTarget(this ref socksDialer d, @string network, @string address) => http_package.validateTarget⁔(ref d, network, address);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (net.Addr proxy, net.Addr dst, error err) pathAddrs(this ref socksDialer d, @string address) => http_package.pathAddrs⁔(ref d, address);
}
partial class httpᴛsocksReply {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this socksReply code) => http_package.String⁔(code);
}
partial class httpᴛsocksUsernamePassword {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Authenticate(this ref socksUsernamePassword up, context.Context ctx, io.ReadWriter rw, socksAuthMethod auth) => http_package.Authenticate⁔(ref up, ctx, rw, auth);
}
partial class httpᴛstatusError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this statusError e) => http_package.Error⁔(e);
}
partial class httpᴛstringWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) WriteString(this stringWriter w, @string s) => http_package.WriteString⁔(w, s);
}
partial class httpᴛtextSig {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string match(this textSig _, slice<byte> data, nint firstNonWS) => http_package.match⁔(_, data, firstNonWS);
}
partial class httpᴛtimeoutError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this ref timeoutError e) => http_package.Error⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Timeout(this ref timeoutError e) => http_package.Timeout⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Temporary(this ref timeoutError e) => http_package.Temporary⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Is(this ref timeoutError e, error err) => http_package.Is⁔(ref e, err);
}
partial class httpᴛtimeoutHandler {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string errorBody(this ref timeoutHandler h) => http_package.errorBody⁔(ref h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ServeHTTP(this ж<timeoutHandler> Ꮡh, ResponseWriter w, ж<Request> Ꮡr) => http_package.ServeHTTP⁔(Ꮡh, w, Ꮡr);
}
partial class httpᴛtimeoutWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Push(this ref timeoutWriter tw, @string target, ж<PushOptions> Ꮡopts) => http_package.Push⁔(ref tw, target, Ꮡopts);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Header Header(this ref timeoutWriter tw) => http_package.Header⁔(ref tw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this ж<timeoutWriter> Ꮡtw, slice<byte> p) => http_package.Write⁔(Ꮡtw, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeHeaderLocked(this ref timeoutWriter tw, nint code) => http_package.writeHeaderLocked⁔(ref tw, code);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void WriteHeader(this ж<timeoutWriter> Ꮡtw, nint code) => http_package.WriteHeader⁔(Ꮡtw, code);
}
partial class httpᴛtlsHandshakeTimeoutError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Timeout(this tlsHandshakeTimeoutError _) => http_package.Timeout⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Temporary(this tlsHandshakeTimeoutError _) => http_package.Temporary⁔(_);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this tlsHandshakeTimeoutError _) => http_package.Error⁔(_);
}
partial class httpᴛtransferReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool protoAtLeast(this ref transferReader t, nint m, nint n) => http_package.protoAtLeast⁔(ref t, m, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error parseTransferEncoding(this ref transferReader t) => http_package.parseTransferEncoding⁔(ref t);
}
partial class httpᴛtransferWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool shouldSendChunkedRequestBody(this ж<transferWriter> Ꮡt) => http_package.shouldSendChunkedRequestBody⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void probeRequestBody(this ж<transferWriter> Ꮡt) => http_package.probeRequestBody⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool shouldSendContentLength(this ref transferWriter t) => http_package.shouldSendContentLength⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeHeader(this ref transferWriter t, io.Writer w, ж<httptrace.ClientTrace> Ꮡtrace) => http_package.writeHeader⁔(ref t, w, Ꮡtrace);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error /*err*/ writeBody(this ж<transferWriter> Ꮡt, io.Writer w) => http_package.writeBody⁔(Ꮡt, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64 n, error err) doBodyCopy(this ж<transferWriter> Ꮡt, io.Writer dst, io.Reader src) => http_package.doBodyCopy⁔(Ꮡt, dst, src);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static io.Reader unwrapBody(this ref transferWriter t) => http_package.unwrapBody⁔(ref t);
}
partial class httpᴛtransportReadFromServerError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Unwrap(this transportReadFromServerError e) => http_package.Unwrap⁔(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this transportReadFromServerError e) => http_package.Error⁔(e);
}
partial class httpᴛtransportRequest {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Header extraHeaders(this ref transportRequest tr) => http_package.extraHeaders⁔(ref tr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setError(this ж<transportRequest> Ꮡtr, error err) => http_package.setError⁔(Ꮡtr, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void logf(this ref transportRequest tr, @string format, params ꓸꓸꓸany argsʗp) => http_package.logf⁔(ref tr, format, argsʗp);
}
partial class httpᴛunencryptedHTTP2Request {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static context.Context BaseContext(this unencryptedHTTP2Request h) => http_package.BaseContext⁔(h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ServeHTTP(this unencryptedHTTP2Request h, ResponseWriter rw, ж<Request> Ꮡreq) => http_package.ServeHTTP⁔(h, rw, Ꮡreq);
}
partial class httpᴛunencryptedNetConnInTLSConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static net.Conn UnencryptedNetConn(this unencryptedNetConnInTLSConn c) => http_package.UnencryptedNetConn⁔(c);
}
partial class httpᴛunsupportedTEError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this ref unsupportedTEError uste) => http_package.Error⁔(ref uste);
}
partial class httpᴛwantConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool waiting(this ж<wantConn> Ꮡw) => http_package.waiting⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static context.Context getCtxForDial(this ж<wantConn> Ꮡw) => http_package.getCtxForDial⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool tryDeliver(this ж<wantConn> Ꮡw, ж<persistConn> Ꮡpc, error err, time.Time idleAt) => http_package.tryDeliver⁔(Ꮡw, Ꮡpc, err, idleAt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cancel(this ж<wantConn> Ꮡw, ж<Transport> Ꮡt) => http_package.cancel⁔(Ꮡw, Ꮡt);
}
partial class httpᴛwantConnQueue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint len(this ref wantConnQueue q) => http_package.len⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pushBack(this ref wantConnQueue q, ж<wantConn> Ꮡw) => http_package.pushBack⁔(ref q, Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<wantConn> popFront(this ref wantConnQueue q) => http_package.popFront⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<wantConn> peekFront(this ref wantConnQueue q) => http_package.peekFront⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*cleaned*/ cleanFrontNotWaiting(this ref wantConnQueue q) => http_package.cleanFrontNotWaiting⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cleanFrontCanceled(this ref wantConnQueue q) => http_package.cleanFrontCanceled⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void all(this ref wantConnQueue q, Action<ж<wantConn>> f) => http_package.all⁔(ref q, f);
}
// </MethodForwarders>
