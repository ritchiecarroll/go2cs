// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

partial class http_package {

// HTTP status codes as registered with IANA.
// See: https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml
public static UntypedInt StatusContinue => 100; // RFC 9110, 15.2.1
public static UntypedInt StatusSwitchingProtocols => 101; // RFC 9110, 15.2.2
public static UntypedInt StatusProcessing => 102; // RFC 2518, 10.1
public static UntypedInt StatusEarlyHints => 103; // RFC 8297

public static UntypedInt StatusOK => 200; // RFC 9110, 15.3.1
public static UntypedInt StatusCreated => 201; // RFC 9110, 15.3.2
public static UntypedInt StatusAccepted => 202; // RFC 9110, 15.3.3
public static UntypedInt StatusNonAuthoritativeInfo => 203; // RFC 9110, 15.3.4
public static UntypedInt StatusNoContent => 204; // RFC 9110, 15.3.5
public static UntypedInt StatusResetContent => 205; // RFC 9110, 15.3.6
public static UntypedInt StatusPartialContent => 206; // RFC 9110, 15.3.7
public static UntypedInt StatusMultiStatus => 207; // RFC 4918, 11.1
public static UntypedInt StatusAlreadyReported => 208; // RFC 5842, 7.1
public static UntypedInt StatusIMUsed => 226; // RFC 3229, 10.4.1

public static UntypedInt StatusMultipleChoices => 300; // RFC 9110, 15.4.1
public static UntypedInt StatusMovedPermanently => 301; // RFC 9110, 15.4.2
public static UntypedInt StatusFound => 302; // RFC 9110, 15.4.3
public static UntypedInt StatusSeeOther => 303; // RFC 9110, 15.4.4
public static UntypedInt StatusNotModified => 304; // RFC 9110, 15.4.5
public static UntypedInt StatusUseProxy => 305; // RFC 9110, 15.4.6
internal static UntypedInt _ᴛ11ʗ => 306; // RFC 9110, 15.4.7 (Unused)
public static UntypedInt StatusTemporaryRedirect => 307; // RFC 9110, 15.4.8
public static UntypedInt StatusPermanentRedirect => 308; // RFC 9110, 15.4.9

public static UntypedInt StatusBadRequest => 400; // RFC 9110, 15.5.1
public static UntypedInt StatusUnauthorized => 401; // RFC 9110, 15.5.2
public static UntypedInt StatusPaymentRequired => 402; // RFC 9110, 15.5.3
public static UntypedInt StatusForbidden => 403; // RFC 9110, 15.5.4
public static UntypedInt StatusNotFound => 404; // RFC 9110, 15.5.5
public static UntypedInt StatusMethodNotAllowed => 405; // RFC 9110, 15.5.6
public static UntypedInt StatusNotAcceptable => 406; // RFC 9110, 15.5.7
public static UntypedInt StatusProxyAuthRequired => 407; // RFC 9110, 15.5.8
public static UntypedInt StatusRequestTimeout => 408; // RFC 9110, 15.5.9
public static UntypedInt StatusConflict => 409; // RFC 9110, 15.5.10
public static UntypedInt StatusGone => 410; // RFC 9110, 15.5.11
public static UntypedInt StatusLengthRequired => 411; // RFC 9110, 15.5.12
public static UntypedInt StatusPreconditionFailed => 412; // RFC 9110, 15.5.13
public static UntypedInt StatusRequestEntityTooLarge => 413; // RFC 9110, 15.5.14
public static UntypedInt StatusRequestURITooLong => 414; // RFC 9110, 15.5.15
public static UntypedInt StatusUnsupportedMediaType => 415; // RFC 9110, 15.5.16
public static UntypedInt StatusRequestedRangeNotSatisfiable => 416; // RFC 9110, 15.5.17
public static UntypedInt StatusExpectationFailed => 417; // RFC 9110, 15.5.18
public static UntypedInt StatusTeapot => 418; // RFC 9110, 15.5.19 (Unused)
public static UntypedInt StatusMisdirectedRequest => 421; // RFC 9110, 15.5.20
public static UntypedInt StatusUnprocessableEntity => 422; // RFC 9110, 15.5.21
public static UntypedInt StatusLocked => 423; // RFC 4918, 11.3
public static UntypedInt StatusFailedDependency => 424; // RFC 4918, 11.4
public static UntypedInt StatusTooEarly => 425; // RFC 8470, 5.2.
public static UntypedInt StatusUpgradeRequired => 426; // RFC 9110, 15.5.22
public static UntypedInt StatusPreconditionRequired => 428; // RFC 6585, 3
public static UntypedInt StatusTooManyRequests => 429; // RFC 6585, 4
public static UntypedInt StatusRequestHeaderFieldsTooLarge => 431; // RFC 6585, 5
public static UntypedInt StatusUnavailableForLegalReasons => 451; // RFC 7725, 3

public static UntypedInt StatusInternalServerError => 500; // RFC 9110, 15.6.1
public static UntypedInt StatusNotImplemented => 501; // RFC 9110, 15.6.2
public static UntypedInt StatusBadGateway => 502; // RFC 9110, 15.6.3
public static UntypedInt StatusServiceUnavailable => 503; // RFC 9110, 15.6.4
public static UntypedInt StatusGatewayTimeout => 504; // RFC 9110, 15.6.5
public static UntypedInt StatusHTTPVersionNotSupported => 505; // RFC 9110, 15.6.6
public static UntypedInt StatusVariantAlsoNegotiates => 506; // RFC 2295, 8.1
public static UntypedInt StatusInsufficientStorage => 507; // RFC 4918, 11.5
public static UntypedInt StatusLoopDetected => 508; // RFC 5842, 7.2
public static UntypedInt StatusNotExtended => 510; // RFC 2774, 7
public static UntypedInt StatusNetworkAuthenticationRequired => 511; // RFC 6585, 6

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string continueˢ2 = "Continue"u8;
internal static readonly @string switchingProtocolsˢ = "Switching Protocols"u8;
internal static readonly @string processingˢ = "Processing"u8;
internal static readonly @string earlyHintsˢ = "Early Hints"u8;
internal static readonly @string createdˢ = "Created"u8;
internal static readonly @string acceptedˢ = "Accepted"u8;
internal static readonly @string nonAuthoritativeˢ = "Non-Authoritative Information"u8;
internal static readonly @string noContentˢ = "No Content"u8;
internal static readonly @string resetContentˢ = "Reset Content"u8;
internal static readonly @string partialContentˢ = "Partial Content"u8;
internal static readonly @string multiStatusˢ = "Multi-Status"u8;
internal static readonly @string alreadyReportedˢ = "Already Reported"u8;
internal static readonly @string imUsedˢ = "IM Used"u8;
internal static readonly @string multipleChoicesˢ = "Multiple Choices"u8;
internal static readonly @string movedPermanentlyˢ = "Moved Permanently"u8;
internal static readonly @string foundˢ = "Found"u8;
internal static readonly @string seeOtherˢ = "See Other"u8;
internal static readonly @string notModifiedˢ = "Not Modified"u8;
internal static readonly @string useProxyˢ = "Use Proxy"u8;
internal static readonly @string temporaryRedirectˢ = "Temporary Redirect"u8;
internal static readonly @string permanentRedirectˢ = "Permanent Redirect"u8;
internal static readonly @string badRequestˢ = "Bad Request"u8;
internal static readonly @string unauthorizedˢ = "Unauthorized"u8;
internal static readonly @string paymentRequiredˢ = "Payment Required"u8;
internal static readonly @string forbiddenˢ2 = "Forbidden"u8;
internal static readonly @string notFoundˢ = "Not Found"u8;
internal static readonly @string methodNotAllowedˢ = "Method Not Allowed"u8;
internal static readonly @string notAcceptableˢ = "Not Acceptable"u8;
internal static readonly @string proxyAuthenticationˢ = "Proxy Authentication Required"u8;
internal static readonly @string requestTimeoutˢ = "Request Timeout"u8;
internal static readonly @string conflictˢ = "Conflict"u8;
internal static readonly @string goneˢ = "Gone"u8;
internal static readonly @string lengthRequiredˢ = "Length Required"u8;
internal static readonly @string preconditionFailedˢ = "Precondition Failed"u8;
internal static readonly @string requestEntityTooLargeˢ = "Request Entity Too Large"u8;
internal static readonly @string requestUriTooLongˢ = "Request URI Too Long"u8;
internal static readonly @string unsupportedMediaTypeˢ = "Unsupported Media Type"u8;
internal static readonly @string requestedRangeNotˢ = "Requested Range Not Satisfiable"u8;
internal static readonly @string expectationFailedˢ = "Expectation Failed"u8;
internal static readonly @string iMATeapotˢ = "I'm a teapot"u8;
internal static readonly @string misdirectedRequestˢ = "Misdirected Request"u8;
internal static readonly @string unprocessableEntityˢ = "Unprocessable Entity"u8;
internal static readonly @string lockedˢ = "Locked"u8;
internal static readonly @string failedDependencyˢ = "Failed Dependency"u8;
internal static readonly @string tooEarlyˢ = "Too Early"u8;
internal static readonly @string upgradeRequiredˢ = "Upgrade Required"u8;
internal static readonly @string preconditionRequiredˢ = "Precondition Required"u8;
internal static readonly @string tooManyRequestsˢ = "Too Many Requests"u8;
internal static readonly @string requestHeaderFieldsTooˢ = "Request Header Fields Too Large"u8;
internal static readonly @string unavailableForLegalˢ = "Unavailable For Legal Reasons"u8;
internal static readonly @string internalServerErrorˢ2 = "Internal Server Error"u8;
internal static readonly @string notImplementedˢ = "Not Implemented"u8;
internal static readonly @string badGatewayˢ = "Bad Gateway"u8;
internal static readonly @string serviceUnavailableˢ = "Service Unavailable"u8;
internal static readonly @string gatewayTimeoutˢ = "Gateway Timeout"u8;
internal static readonly @string httpVersionNotSupportedˢ = "HTTP Version Not Supported"u8;
internal static readonly @string variantAlsoNegotiatesˢ = "Variant Also Negotiates"u8;
internal static readonly @string insufficientStorageˢ = "Insufficient Storage"u8;
internal static readonly @string loopDetectedˢ = "Loop Detected"u8;
internal static readonly @string notExtendedˢ = "Not Extended"u8;
internal static readonly @string networkAuthenticationˢ = "Network Authentication Required"u8;

// StatusText returns a text for the HTTP status code. It returns the empty
// string if the code is unknown.
public static @string StatusText(nint code) {
    var exprᴛ1 = code;
    if (exprᴛ1 == StatusContinue) {
        return continueˢ2;
    }
    if (exprᴛ1 == StatusSwitchingProtocols) {
        return switchingProtocolsˢ;
    }
    if (exprᴛ1 == StatusProcessing) {
        return processingˢ;
    }
    if (exprᴛ1 == StatusEarlyHints) {
        return earlyHintsˢ;
    }
    if (exprᴛ1 == StatusOK) {
        return "OK"u8;
    }
    if (exprᴛ1 == StatusCreated) {
        return createdˢ;
    }
    if (exprᴛ1 == StatusAccepted) {
        return acceptedˢ;
    }
    if (exprᴛ1 == StatusNonAuthoritativeInfo) {
        return nonAuthoritativeˢ;
    }
    if (exprᴛ1 == StatusNoContent) {
        return noContentˢ;
    }
    if (exprᴛ1 == StatusResetContent) {
        return resetContentˢ;
    }
    if (exprᴛ1 == StatusPartialContent) {
        return partialContentˢ;
    }
    if (exprᴛ1 == StatusMultiStatus) {
        return multiStatusˢ;
    }
    if (exprᴛ1 == StatusAlreadyReported) {
        return alreadyReportedˢ;
    }
    if (exprᴛ1 == StatusIMUsed) {
        return imUsedˢ;
    }
    if (exprᴛ1 == StatusMultipleChoices) {
        return multipleChoicesˢ;
    }
    if (exprᴛ1 == StatusMovedPermanently) {
        return movedPermanentlyˢ;
    }
    if (exprᴛ1 == StatusFound) {
        return foundˢ;
    }
    if (exprᴛ1 == StatusSeeOther) {
        return seeOtherˢ;
    }
    if (exprᴛ1 == StatusNotModified) {
        return notModifiedˢ;
    }
    if (exprᴛ1 == StatusUseProxy) {
        return useProxyˢ;
    }
    if (exprᴛ1 == StatusTemporaryRedirect) {
        return temporaryRedirectˢ;
    }
    if (exprᴛ1 == StatusPermanentRedirect) {
        return permanentRedirectˢ;
    }
    if (exprᴛ1 == StatusBadRequest) {
        return badRequestˢ;
    }
    if (exprᴛ1 == StatusUnauthorized) {
        return unauthorizedˢ;
    }
    if (exprᴛ1 == StatusPaymentRequired) {
        return paymentRequiredˢ;
    }
    if (exprᴛ1 == StatusForbidden) {
        return forbiddenˢ2;
    }
    if (exprᴛ1 == StatusNotFound) {
        return notFoundˢ;
    }
    if (exprᴛ1 == StatusMethodNotAllowed) {
        return methodNotAllowedˢ;
    }
    if (exprᴛ1 == StatusNotAcceptable) {
        return notAcceptableˢ;
    }
    if (exprᴛ1 == StatusProxyAuthRequired) {
        return proxyAuthenticationˢ;
    }
    if (exprᴛ1 == StatusRequestTimeout) {
        return requestTimeoutˢ;
    }
    if (exprᴛ1 == StatusConflict) {
        return conflictˢ;
    }
    if (exprᴛ1 == StatusGone) {
        return goneˢ;
    }
    if (exprᴛ1 == StatusLengthRequired) {
        return lengthRequiredˢ;
    }
    if (exprᴛ1 == StatusPreconditionFailed) {
        return preconditionFailedˢ;
    }
    if (exprᴛ1 == StatusRequestEntityTooLarge) {
        return requestEntityTooLargeˢ;
    }
    if (exprᴛ1 == StatusRequestURITooLong) {
        return requestUriTooLongˢ;
    }
    if (exprᴛ1 == StatusUnsupportedMediaType) {
        return unsupportedMediaTypeˢ;
    }
    if (exprᴛ1 == StatusRequestedRangeNotSatisfiable) {
        return requestedRangeNotˢ;
    }
    if (exprᴛ1 == StatusExpectationFailed) {
        return expectationFailedˢ;
    }
    if (exprᴛ1 == StatusTeapot) {
        return iMATeapotˢ;
    }
    if (exprᴛ1 == StatusMisdirectedRequest) {
        return misdirectedRequestˢ;
    }
    if (exprᴛ1 == StatusUnprocessableEntity) {
        return unprocessableEntityˢ;
    }
    if (exprᴛ1 == StatusLocked) {
        return lockedˢ;
    }
    if (exprᴛ1 == StatusFailedDependency) {
        return failedDependencyˢ;
    }
    if (exprᴛ1 == StatusTooEarly) {
        return tooEarlyˢ;
    }
    if (exprᴛ1 == StatusUpgradeRequired) {
        return upgradeRequiredˢ;
    }
    if (exprᴛ1 == StatusPreconditionRequired) {
        return preconditionRequiredˢ;
    }
    if (exprᴛ1 == StatusTooManyRequests) {
        return tooManyRequestsˢ;
    }
    if (exprᴛ1 == StatusRequestHeaderFieldsTooLarge) {
        return requestHeaderFieldsTooˢ;
    }
    if (exprᴛ1 == StatusUnavailableForLegalReasons) {
        return unavailableForLegalˢ;
    }
    if (exprᴛ1 == StatusInternalServerError) {
        return internalServerErrorˢ2;
    }
    if (exprᴛ1 == StatusNotImplemented) {
        return notImplementedˢ;
    }
    if (exprᴛ1 == StatusBadGateway) {
        return badGatewayˢ;
    }
    if (exprᴛ1 == StatusServiceUnavailable) {
        return serviceUnavailableˢ;
    }
    if (exprᴛ1 == StatusGatewayTimeout) {
        return gatewayTimeoutˢ;
    }
    if (exprᴛ1 == StatusHTTPVersionNotSupported) {
        return httpVersionNotSupportedˢ;
    }
    if (exprᴛ1 == StatusVariantAlsoNegotiates) {
        return variantAlsoNegotiatesˢ;
    }
    if (exprᴛ1 == StatusInsufficientStorage) {
        return insufficientStorageˢ;
    }
    if (exprᴛ1 == StatusLoopDetected) {
        return loopDetectedˢ;
    }
    if (exprᴛ1 == StatusNotExtended) {
        return notExtendedˢ;
    }
    if (exprᴛ1 == StatusNetworkAuthenticationRequired) {
        return networkAuthenticationˢ;
    }
    { /* default: */
        return ""u8;
    }
}

} // end http_package
