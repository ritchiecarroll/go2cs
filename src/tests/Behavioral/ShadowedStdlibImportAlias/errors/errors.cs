namespace go.ShadowedStdlibImportAlias;

using stderrors = go.errors_package;

partial class errors_package {

public static bool Is(error err, error target) {
    return stderrors.Is(err, target);
}

public static error Unwrap(error err) {
    return stderrors.Unwrap(err);
}

} // end errors_package
