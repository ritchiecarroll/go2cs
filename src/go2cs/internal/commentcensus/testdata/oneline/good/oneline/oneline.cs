namespace go.example.com;

using context = context_package;

partial class oneline_package {

// timeoutError reports a timeout.
partial struct timeoutError {
    internal @string err;
}

internal static @string Error(this ref timeoutError e) => e.err;
internal static bool Timeout(this ref timeoutError e) => true;
internal static bool Is(this ref timeoutError e, error err) => AreEqual(err, context_package.DeadlineExceeded);
internal static void marker(this ref timeoutError _) { }

// wrap returns a timeout error.
internal static error wrap(@string s) {
    return new timeoutErrorжerror(Ꮡ(new timeoutError(s)));
}

} // end oneline_package
