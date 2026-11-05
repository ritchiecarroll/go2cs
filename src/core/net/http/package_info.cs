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
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using tlsꓸConnectionState = go.crypto.tls_package.ΔConnectionState;
global using urlꓸError = go.net.url_package.ΔError;
// </ImportedTypeAliases>

using go;
using static go.net.http_package;

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
[assembly: GoTypeAlias("Cookie", "ΔCookie")]
[assembly: GoTypeAlias("Handler", "ΔHandler")]
[assembly: GoTypeAlias("Header", "ΔHeader")]
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
[assembly: GoImplement<HandlerFunc, ΔHandler>]
[assembly: GoImplement<MaxBytesError, error>(Pointer = true)]
[assembly: GoImplement<ProtocolError, error>(Pointer = true)]
[assembly: GoImplement<ResponseController, Hijacker>(Pointer = true)]
[assembly: GoImplement<ResponseWriter, io_package.Writer>]
[assembly: GoImplement<ServeMux, ΔHandler>(Pointer = true)]
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
[assembly: GoImplement<fileHandler, ΔHandler>(Pointer = true)]
[assembly: GoImplement<fileInfoDirs, anyDirs>]
[assembly: GoImplement<fileTransport, RoundTripper>]
[assembly: GoImplement<finishAsyncByteRead, io_package.Reader>]
[assembly: GoImplement<globalOptionsHandler, ΔHandler>]
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
[assembly: GoImplement<initALPNRequest, ΔHandler>]
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
[assembly: GoImplement<noopHandler, ΔHandler>(Pointer = true)]
[assembly: GoImplement<noopHandler, ΔHandler>]
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
[assembly: GoImplement<redirectHandler, ΔHandler>(Pointer = true)]
[assembly: GoImplement<requestBodyReadError, error>(Promoted = true)]
[assembly: GoImplement<requestBodyReadError, error>]
[assembly: GoImplement<response, ResponseWriter>(Pointer = true)]
[assembly: GoImplement<response, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<rᴛ1, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<rᴛ1, io_package.Reader>]
[assembly: GoImplement<rᴛ1, io_package.WriterTo>(Promoted = true)]
[assembly: GoImplement<serverHandler, ΔHandler>]
[assembly: GoImplement<socksAddr, net_package.ΔAddr>(Pointer = true)]
[assembly: GoImplement<socksConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<socksConn, net_package.Conn>(Promoted = true)]
[assembly: GoImplement<statusError, error>]
[assembly: GoImplement<stringWriter, io_package.StringWriter>]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<textSig, sniffSig>]
[assembly: GoImplement<timeoutError, error>(Pointer = true)]
[assembly: GoImplement<timeoutHandler, ΔHandler>(Pointer = true)]
[assembly: GoImplement<timeoutWriter, Pusher>(Pointer = true)]
[assembly: GoImplement<timeoutWriter, ResponseWriter>(Pointer = true)]
[assembly: GoImplement<tlsHandshakeTimeoutError, error>]
[assembly: GoImplement<transportReadFromServerError, error>]
[assembly: GoImplement<unencryptedHTTP2Request, ΔHandler>]
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
[assembly: GoFieldAccess(typeof(go.crypto.tls_package.ΔConnectionState))]
[assembly: GoFieldAccess(typeof(go.mime.multipart_package.FileHeader))]
[assembly: GoFieldAccess(typeof(go.mime.multipart_package.Form))]
[assembly: GoFieldAccess(typeof(go.net.http.httptrace_package.ClientTrace))]
[assembly: GoFieldAccess(typeof(go.net.textproto_package.Reader))]
[assembly: GoFieldAccess(typeof(go.net.url_package.URL))]
[assembly: GoFieldAccess(typeof(go.net.url_package.ΔError))]
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
[assembly: GoFieldAccess(typeof(ΔCookie))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("net/http/client.go", "client.cs", "AGzaAQAiTAAJCoKUgqaC7oKUqOKCgoKUgoKmgoKUgoKmpoKClKaCgpS4AAsI8pSCgqaCgqaCgsqCgoLugoKmgoKCgoKmgpSUgoKCgpS4gqaUgpQAChaClJSC3KyygoKUAAIQ0pSClMQABxCClAAIGAAMAoKUgpSmgqaCgqSUgoKmgpSkhoS4gqKCgriClMLEgrSCtMgAAhDSggACOAAZAgAGNAAZAoKClKaA7JiigoKUqvKUgoLcgsaCgpTKtuqigpSClAACUAAlAgAKCvKCgKSCgsqUAAkYlIKUgoKUlNymgoK4lIKCgpSCuIKmggAIEoKCgoKUgu6CgqamgpTKggACENKClJTKgoK4goKClIKClKaSgoKUpqYACw7muIKCgrgADhiCgoKCgoKmgoKCgoKmgtyCgoKm7qaCAAkMgoKUAAImABACAAgiABACgoKUggACJgAQAgAGIgAOAgACIgAOAgAGHgAOAoKClAAGFPKIAA0csoKClIKUgpSmgoKCpgANHIKCrsKCyoLKgpSmooKClA==", "236-241:1;372-372:1;384-393:2;396-401:3;406-417:4;599-599:1;622-638:2;774-830:1", "", "685=urlErrorOp/1/1/1,New/1/1/2;713=urlErrorOp/1/1/1")]
[assembly: GoPositionMap("net/http/clientconn.go", "clientconn.cs", "AGLiAQAYApTWppKClIKmgpQADh6CgoK4AA0cgoKCgtyCkqaCgpSCpoKCgpTa4oLcsgAIBoKClIKmgIK2gIKkgpSClAACFgAMAoKCgpTcsqyyAAIgABECgtrigqKo8oKCgpSClIKUgpSqgoKClIKCgtaCgoLu3IKCgoKCAAUkAA4CgoKCAAgSooKmgoKmgoKClILmgoKmgqSkuKKCkoKU1qKCkoKUAAkGooKSgpTWlILWooKSgtaktA==", "345-350:1", "", "169=JoinHostPort/1/1/2", "452:29")]
[assembly: GoPositionMap("net/http/clone.go", "clone.cs", "AA4uAAkCgrgAAhgACwKClIKCgoKUAAIYAAsCgpSmgoKCgoKUlJQAAhgACwKClIKCggACHgAMAoKClA==", "", "", "72=Clone/1/1/1")]
[assembly: GoPositionMap("net/http/cookie.go", "cookie.cs", "ABUoACZYgoKCgrqCgoKUgoKClJSssoKUgoKUgqKCooKUgpSSgpSU6qKCgpSCooKUgoKUkoKU3IKCgqaCgoKUgoKCpqSCgoKUpLS0tKS0grSCtIK0goKUgpSCtIKCgoKCgqaCtIK0gqSUAAYS4oKCyoKUgoKCpqyyggAbEOKCmKKCgoKClIKClILKgoKUgpSmgoKClIKCgoKUgpSClNi0tKSClAAOCLKClIKUgpSCgqaCgoK4goKmgoKmAAIWAAgCgoLcgoKUgqaCopSCgoKCgpSigoKUgpSSgpSmqJKClIKUqKSssoKUgqamlIKCgoKClLaCxsaClMaClIKUpJSCpqaUggAGIgAOAoKClKaC6qKmgqaigoKClIKClIKUgoKCpgACGAAMBIKClIKCpg==")]
[assembly: GoPositionMap("net/http/csrf.go", "csrf.cs", "ACtYkgACGAAOAoKClIKUgpSClIKCgpSCAAgKpAABHgANAqaCgoKUgoK4AAIUAAkCgoKUAAgKwram+qSClKaCpqbupoKUqIKcAAgCgpS4goKUAAUS4oKCgoKUgpQ=", "207-217:1")]
[assembly: GoPositionMap("net/http/filetransport.go", "filetransport.cs", "ABE+AA0CAAIiAA4Cpu6CkoKUpoKCAAsYAA8ggoKUgpSmgoKUlIKUpoKmgoKUlIKmgoKUgoKU", "61-64:1")]
[assembly: GoPositionMap("net/http/fs.go", "fs.cs", "ADJksoKmgoKClIKClIKm3JiigoKUgoKUgoKUgoKClAAjRICigKKAyICigKKAAAgEiLKCgoKClIKCpoKCgpSkgoKCgoKCgsqClMqcwpSC3IKUgpSmgqYAAkQAHwKCgoKUgoKUlO64ABEM4oKCgqa4goKCgoSSgoKCgoKmgoKmgoKClJSCuIKCgtbKgpSCtIKmypQADBqCgoKUgoK0gpSCgoKCkrKygoKClIKClIKCpoLIABo2gpSUgrSqsoKCgpSCuIKC2KSmqqKqogAQGoKCgpSCgoKUgoKUgpSCgpSClKbmgoKClIKCyoKClOaCgoKUgoKCgpSCgpSClIKClIKUlOaCgpSCgpSCgriCgpTmgoKUgoKUgoKClMqClIKClIKUppaS5oKCuNyCgoKCgpTq1IKClIKClJSCgpSCxoKCyIKClAAKCNLKgoKmgoKCgpSkgoKCgqamgoKCgpSCgpSCgpSCuIKUgoK4goKCkoKCgtyCgoKUgoK4kqIACg7SgpSClIKmqqKClIIABjAAFwLcgpSCAAIuABYC3IKUpoKClIKCpqaAAA4cgoKUlIKCqKaAooCigKSSlIKCgpSmgoKClKaCgoKUgoKCgoKUlJSClIKmrLIAAiAADQIAAhgACQKmooKCgpQABxCCpoIABxC4goKCgoKCuAAKCqKClIKClIKCgoKClIKUkoKClIKClIKC3IKUgoKUgpSClIKClKaClIKUlIKClIKUppSUlNyCgqrCgoKCgpSCgqaigpQ=", "161-161:1;247-257:1;376-394:1;761-761:1;896-898:1", "", "1155=contentRange/1/1/1", "430:25,811:21,855:29")]
[assembly: GoPositionMap("net/http/h2_bundle.go", "h2_bundle.cs", "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", "2063-2063:1;2069-2075:2;3226-3276:1;3278-3278:2;3356-3363:1;4308-4308:1;4359-4381:2;4382-4384:3;4388-4400:4;4909-4909:1;5191-5201:1;5547-5547:1;6478-6505:1;6629-6636:1;6906-6920:1;6932-6946:1;6995-6998:1;7274-7319:1;7378-7380:1;7639-7655:1;7659-7661:2;7663-7670:3;7860-7863:1;7890-7893:1;8570-8584:1;8637-8643:1;8745-8754:1;8756-8781:2;8783-8804:3;9018-9020:1;9609-9611:1;9829-9831:1;9915-9917:2;10311-10347:1;10438-10449:1;10684-10688:1;11947-11969:1", "", "1656=Uint32/1/1/4;2004=Sprintf/1/1/1;2010=Sprintf/1/1/1;2245=Uint16/1/1/1,Uint32/1/1/2;2405=Uint32/1/2/2,Uint32/2/2/3;4557=RemoteAddr/1/1/5,String/1/1/5,http2newBufferedWriter/1/1/6,handler/1/1/7,http2newGoroutineLock/1/1/21;5102=maxHeaderListSize/1/1/3;6708=PseudoValue/1/5/1,PseudoValue/2/5/2,PseudoValue/3/5/3,PseudoValue/4/5/4,PseudoValue/5/5/5;6795=WithContext/1/1/13;7545=hasTrailers/1/1/4;7705=http2asciiEqualFold/2/6/1,http2asciiEqualFold/3/6/2,http2asciiEqualFold/4/6/3,http2asciiEqualFold/5/6/4,http2asciiEqualFold/6/6/5;7720=http2cloneHeader/1/1/4,getErrChan/1/1/5;7821=RequestURI/1/1/4,http2cloneHeader/1/1/5;8757=Now/1/1/18;9090=tooIdleLocked/1/1/5;9433=http2actualContentLength/1/1/6,ContextClientTrace/1/1/7;9837=http2actualContentLength/1/1/7;10655=New/1/1/3;10745=StatusText/1/1/5;12442=Bytes/1/1/2;13071=get/1/1/1;13126=get/1/1/1", "4451:23,5093:21,5096:21,5136:25,5142:25,5149:21,5381:21,8166:27,8173:27,8191:23,8860:21,9155:21,9317:25,9735:29,10443:21")]
[assembly: GoPositionMap("net/http/h2_error.go", "h2_error.cs", "AAoagoKCgpSCgoKClIKCgoKmgoKU")]
[assembly: GoPositionMap("net/http/header.go", "header.cs", "ABs8wgACENIAAhLiAAIQ0qiSgpSqooKssqiSpoKokoK4goKUgoKCpoKUgoKUtgAFEuKCgoKmpu6CAAwazuKCgpSCgoKmgoKssqaigoKUgoKCypSigoKCgoKmgqaCgqaCAAIU8KzCgpSClO6CpoKmgpSCpqaC", "179-179:1")]
[assembly: GoPositionMap("net/http/http.go", "http.cs", "ACRSkKaQppCmkKaQppCkgoKUuKKCgpSClIKUAA0cAA8ggKigqKKClKikqJKCgoKmpqKCgoKUpoKUgoKCgoKUgoKmgpTu2ICigKKAuII=")]
[assembly: GoPositionMap("net/http/mapping.go", "mapping.cs", "ABgykoKUgoKClJS+8oKUgoKUgoKmqsKClIKCgriCgg==")]
[assembly: GoPositionMap("net/http/pattern.go", "pattern.cs", "ACNKgKSCAC5YABUCgpSCgoK4soKUgoKUgpSUgpSCgpSCgoKCpriCpoKUgoKUgpSCgpSCgpSCpoKUgpSCgoKUgpSCgpSClIKUgpSCpuaCgqaCgqamgoKUlAAQNgAPAriUgqaClIKUggACFAAJAoKUlJSClJSUgpSq5oKoooKCgoLcgsqClIKUqJKClIKUgpSClIKUlJSCgpSmgpQAAhYACAKkpKSClKSkpKTGzKKkpKS6soKCgoKUgpSC/IKUgpSokoK4goKCvKKCkoKClKaCgoKUqqKUkoKCgpSClsqCgoKUppSCgqaCyoKUgpSCuIKUpqaCgpQ=", "89-93:1", "", "466=commonPath/1/1/4,differencePath/1/2/4,differencePath/2/2/4")]
[assembly: GoPositionMap("net/http/request.go", "request.cs", "ACtSAAkUgKaSAAgUyri4ysrKloC2ANwB+gMACwKClAACGAAJAoKUgoKCAAIWAAgCgpSCgoKCgoKAgoKCpIKCpoCCgoKkgqqi6pKokqqigpS4nMKClIKUAAISAAgCgoKUAAMYAAgC3AAKEsKClIKUggAIBoKCgpSClIKClIKClKqiqJKClAAJLgAOAgACEuLK+gAJAoKCksqCgoKUggAHEIKCgpSUgoLuAAwagpTulIKCgpSCgqaCAAUUwoKCpoKCuIKClILKgoKUgoKCgoKUgsqCgpSCgqaCgqaCgoK4goKmgriCgoKCpoKUgoKCuICCgtqCgoKClKaClAANEAAJFIKUqqKClIKClIKClOyypKSUgpSClIKUgoKUgoKUpgANHKiSAAY0ABcCuJSClIKUgoKUgoKmggAKFoKUgoKSgsaCgpKCxoKCkoIAFCKCgrissoKClAAGHgAQApSClIKClIKCgpQAAhwACwKo0oKCgpTagoKCgpSmooIAAhLigoKmggAKHAARAoKUlpKClIKCuIKCgpSClIKCAAsYgoKmgqaUuIKClIKCAAkUgoKmlJSCgqaU3JQABhoACgKClAALEJQAFRaygpSC3IKUlIKCgqaCAAASiJSCpoKmgoIACQiygoKUpoKUgpSCgoKClIKCgpSUgoKUgoIADBQAAioAFAKCgoKUgqaCgoKUgoKCgoKmgpSClKYABhYACgKClIKmlIKmgoKmgoKmgpSClKaUAAIcAA0CgpSClAACEPKClIKUqsKClIKCgqaCgoKmrsKClKqigpSClLzGgpSCgoKClKamguaCgpSmgoKUpoKClAAIBoKCpMqCpqqigpSClAACFPKklAAKCqI=", "585-589:1;592-599:2;928-931:1;935-938:2;942-945:3;963-963:4;1090-1094:1", "", "1714=Get/2/2/1,EqualFold/1/1/1")]
[assembly: GoPositionMap("net/http/response.go", "response.cs", "ABgwAFrKAZLKnMKCgpSClAAIEuKCyoKCgpSUgoKUgpSCgpSCgpSCuIKCgpSUlJSCgqYACBTygoLOogALKAARBIKCgoLKpoK4goKEkoKClKaUggAJGoK4goKUgoK4goLKgoKCyoK4goK4poKCAAMW8oKqoqqiqqI=", "", "", "291=NewReader/1/1/4,MultiReader/1/1/4;378=HeaderValuesContainsToken/1/1/1")]
[assembly: GoPositionMap("net/http/responsecontroller.go", "responsecontroller.cs", "ABNMABECAAoQkoKClKSCpMTOooKClKTEAAgU0oKClKTEAAgW4oKClKTEAAgeAAoCgoKUpMTOog==")]
[assembly: GoPositionMap("net/http/roundtrip.go", "roundtrip.cs", "AAooAAoS8oKU")]
[assembly: GoPositionMap("net/http/routing_index.go", "routing_index.cs", "AB8+ooKUgpSCgoKUAAQcABIOgoKUgoKCpsqClMqe4oKCgoKUgoKCgoKCgriCgoLKgpQ=", "64-75:1")]
[assembly: GoPositionMap("net/http/routing_tree.go", "routing_tree.cs", "ACZY1JSUrNKCgpSCgoKUgoKCgpS8ooKUgqrCgoKUlIKUgoKqwoKUgq7iuIKmrNKClICUpJSCuK7igtyCgpSmyoCCAAgQgoLKpoKUlK7CgpSCgoKUqsKClIKCuIKClJKClA==", "233-238:1")]
[assembly: GoPositionMap("net/http/servemux121.go", "servemux121.cs", "ABcyvKKCggAQJOKClIKUgpSCpoKUgoKCpoK0pIKCloKmgoKCqJKClKjWuIKmyoK4gIK2goKCpqoACAKCpoKUgpSClNq0goLKgoKmrvKCgoKClIKCrLKUgoK4goKUgoK4", "82-84:1")]
[assembly: GoPositionMap("net/http/server.go", "server.cs", "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", "987-989:1;1273-1285:1;1913-1932:1;2181-2186:1;2358-2372:1;2725-2728:1;2934-2941:1;3182-3191:1;3346-3357:1;3561-3575:1;3851-3859:1;3852-3856:1.1;4088-4092:1", "", "1128=wantsHttp10KeepAlive/1/1/11,wantsClose/1/1/12;1647=get/1/1/1,hasToken/1/1/1,isProtocolSwitchResponse/1/1/2;4483=Sprintf/1/1/1", "2154:21,3475:21,3740:21,4091:21,4215:25")]
[assembly: GoPositionMap("net/http/sniff.go", "sniff.cs", "AA4q4oK4gqaCgriqopSUqqKUlAAfEgA5kAKCgpQACBK4gpSClIKUgoKCpsqCgoKUgoKClIK4gpSmkgAICKaClIKClIKUgpSUgqbalIKapg==")]
[assembly: GoPositionMap("net/http/socks_bundle.go", "socks_bundle.cs", "ABMuggANBuKCgpSAgoKkgoKCooKCpqK0grTagoKClIKClIKCpoKmgpSClIKClIKCuIKCgoKCgoKClKaClIKClIKCpoKUgpSClIKUgoKUgrSCtIKUtJSClJSClIKUlILmgoKClIKClIKUAAsMgpSkpAAdFIKUpKSkpKSkpKSkAB88gOSigpSCgpQACBjCgpQAGEQADAKCgpSCgpSCgoKUgpSCgpSCgoKClAACEgAIAoKClIKClIKCgpQAAhIACAKCgpSCgoKUlIKClIKClAAIBoLWlMaUpsKCkoKUgoKClIKUpqqiABQiopSkgpSCgoKCpoCCpICCpIKUgpSU", "39-44:1;45-53:2")]
[assembly: GoPositionMap("net/http/status.go", "status.cs", "AMkBogGilKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSkpKSk")]
[assembly: GoPositionMap("net/http/transfer.go", "transfer.cs", "ACE+3IIABxCCgpSClIKCABUuoqaClIKUgoKCgoKCgoKCAAgSgqa0goKUgoKCgoKCgoLIgoKCpoKUgoKCyoKmAAIqABYGgpSClLiC3AACJAARAoKCgoKCgpSClIK0gpSCgoKClJSCAAgOuMiCpoKClIKUgqaClIKClKYADAaigoKUgu6CgIKkgIKkgpSCgpSCyoKCooKklJSCpoKUgsqo4oKCgoKUggAHEIKCgoKUgoKClIKCgpSUgoKUgoKUgqaCgoK4griUgoK4lN4ACgKClIKClAAFENKAgqSAgoKkABEkgqqilKSklKiCgpaCpqSUqLKmgpSCgoKCgoKCxoKCgqaCxMiCuIKmgoKUgoKUpriCgtyUlOyUgpTGtMaUptqUgoKClIK0goKClIK2qJCmkP6CqqKCqJKCgpSmgu6ClIKmgqzigqbKgoKCyoKUuIKCgsqClIKUlAAPIIKCuIKmlAAHELissoKmgoKCpoKm6JKCgpQABxCUlIKCgpKCpIKCtqaClIKUABYwlOKCgoKU6MKClJSClIKCyoKUuIIACRSCgoK4gqaogpaipoKClIKmptSUgoKClIKUggAKFoKmgoKClJSUtKSmgoKClKyygpSm0oKCgpSCAAwSppSmgoKUgvykgtbSgoLa8oKC1tKCgqIACBCCgpSm+rKClLiCgoKUlIKClAAIErKClIKCgpSClKbSnqKUpL6ytJSClIKUAAgOsoKCgoKm", "210-219:1;341-348:1;787-797:1")]
[assembly: GoPositionMap("net/http/transport.go", "transport.cs", "ADVcAO8BmgSCgpSmgoKUpoKClKiyggAWLoKUgoKUgoKUgoKClJQADBqCppjCgoKCAAcQgoKCgoLKlJSCgpSClIKCgpQABxCCgoKUAAgSpoKClIKCuIIAEBakAAImABICqqIADyiCgpSmooKClKrCypSs4oKUgqaCgoKUgqa46AAIAoKClIKClIKClIKClIKCuIKCuIKUgoKUgoKCpoKClIKClIKCAAsYpoLulJKCuIKkguqCgoKC7oKCgqaClJSUgtyUgriCgpSmgIKkgIKkgLiklKaCggAICoK0+gAHEIKCpqKClK7igpSCgq7ygpSClIKUgoKUgoKs0u6UlJQABxCUppSUlKaUuJS4AAEYAA8CgoKCgpSCgpSCoqzigoKCgoKCgoKCpoKCgqaCgroACQ6CgoKClIKClIKCAAISAAgCgoKCggAKHqKClKiSgqbCgoKClIKqooKUgoKClLqCgoKCgoKC3AANHoCkgqaCgriCgpQAAhAACgKClIKUgpSCgoKClILWpKaCyoLugoKCpoKCgoIABxCCgqaClJSCuIKUgpSCgpSCgqaCgoKCgtyCgpSmgtwACQKCpoK4lJSssoK4gIKCgoLKgrik3IKUgoLcgqaUgpSUgtqClIKCgoLo8oKUgoLYsoKUgoKCgtiCgsaCgriCgoK2AAsKgoKCgpSUgoKClJQAHULigpTY4oKU2OKClIKUgpSClIKU2sKCgoKCppSCgsqCABQskqiSqJKCgqaUgoKCqJKClIKUqsKCgoKUgrqSgoKClLyigpSC+LKClJSClK4ACgKCgoKCAAcQlAAIEpKCyoK42IK4goKUlLikgoKU6qSCgpT8AAgClIKUgoKmgoKUgoKmgpSCgoKiqMKCgqKCgoKivPKCgoKCpoKCgriUgrSo8oKmgoKCpu6CgoKCgoKCpoK4lILKgpS0quSCgpSClIKCgoKAgpK2soKUgoKUlICCgqaUgpSkgoKUgoLuAAsE4gAKFoKClJSUgoKCgpSApoKUgIKCgpSkgoKUtoKClIKCgoKUgtzYgoKAgqaCuLSCgsaCgIKC6IKCgoKCgoKmlIKUgIKCpAAJFIKkgqrSgoKCuIKikrSCgtiCgqaCgoKCuIKCgoKUyIKCypqCgoKClIKCgpSmgoKClIKUlKaCgoKUlLiClIKCABAasoKCrOKCgqYAHDqCgoKCgoKmAAcSkoKUqJKClKqigoKUAAoWhJKClAAvaoKmsoKUgpSCgpSCqLKCgoKq8oKC2LKCgoKm0oKCgqKq8oKCkpSUgqIAAhQACwKCAAkUyoK4goKCgqaUpoKCppSCgpSU3JTCgoKCgoK4goKCgoKUlIKU3IKmgoKUgoKClIKCgoKUlIKUgoKUgqaCgqbmlJSUgoKUgoKUAAcQptyagqbmpsqCpoL2goKogoKCgoKCpriCgoKCgoKm5tzEioLGgrS2grSkgoKUgoKCgpSmlJS+soKUgpSs8oKCuIKCgoKUgoKClIKUlIKCgoIACBKUlJSClAAMGoKUuIKssoKUsoKkxKSkAAgKooKClAAVHLKCgpSCgpSU5oKClAAHEIKmwoKCtIKCggAHEJSClIKCpoKCgoLGABAe4sgADBiCkrSkADRogKKAooCigKTIkriUuJKCkoKUgpaCxKSkAAgI8pSCgoKCgpSUgpSCgpSC7oIADCCCpoKCpoamgpTKgoKUggAHEIKClIKUgpSmgoKCgoLkgpSCgpSAgoKUgpLW2saClKSClIKkpNrGAA4SooK8woKCAAIQAAoCgoKipIKClIKCpoKClKamgqSkpKS4ooKClKiygoKUABYukpTigoKCgpSCpoKCgoKClJTW0oKCgpSCgpSC2JKClIKCABEigKKApKaSgoKClKi2ooKq8oKCgpSCgoKmgoLY8oKCgpS0qPKCgoKClKKkwoKClJTmopTagKKA4oAACA6iAAIeAA0CgpQABxKSgoKUgoKUpoKCgoKCqJKCgrqS", "514-516:1;667-671:1;911-915:1;930-935:1;972-974:1;1540-1544:1;1628-1633:1;1728-1730:1;1732-1741:2;1782-1788:1;1856-1858:2;1899-1909:3;2293-2299:1;2301-2314:2;2417-2422:3;2423-2434:4;2590-2602:1;2898-2909:1", "", "54=defaultTransportDialContext/1/1/2;320=Clone/1/1/16;1668=key/1/1/2;1697=isReused/1/1/2;1975=key/1/1/2;2051=Username/1/1/1;2182=UnencryptedHTTP2/1/1/2,HTTP1/1/1/3;2668=wroteRequest/1/1/2,Invoke/1/1/3;2747=wroteRequest/1/1/3,Invoke/1/1/4;3228=Get/1/2/1,Get/2/2/2;3250=wantsClose/1/1/1,isProtocolSwitchHeader/1/1/2", "664:33,2009:39,2114:25,2126:33,2594:21,2894:25,2987:21,3067:25,3255:21,3314:37")]
[assembly: GoPositionMap("net/http/transport_default_other.go", "transport_default_other.cs", "AAscgg==")]
// </GoSourcePositionMaps>

namespace go.net;

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
    internal partial struct writerOnly {}
    internal partial struct ΔwriteRequest {}
    public partial interface CloseNotifier {}
    public partial interface CookieJar {}
    public partial interface File {}
    public partial interface FileSystem {}
    public partial interface Flusher {}
    public partial interface Hijacker {}
    public partial interface Pusher {}
    public partial interface ResponseWriter {}
    public partial interface RoundTripper {}
    public partial interface http2ClientConnPool {}
    public partial interface http2WriteScheduler {}
    public partial interface ΔHandler {}
    public partial struct Client {}
    public partial struct ClientConn {}
    public partial struct ConnState {}
    public partial struct CrossOriginProtection {}
    public partial struct Dir {}
    public partial struct HTTP2Config {}
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
    public partial struct ΔCookie {}
    public partial struct ΔHeader {}
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
