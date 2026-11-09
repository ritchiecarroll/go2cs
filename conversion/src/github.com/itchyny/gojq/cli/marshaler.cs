namespace go.github.com.itchyny.gojq;

using fmt = fmt_package;
using io = io_package;
using strings = strings_package;
using yaml = go.github.com.itchyny.go_yaml.yaml_package;
using go.github.com.itchyny.go_yaml;

partial class cli_package {

partial interface marshaler {
    error marshal(any _Δp0, io.Writer _Δp1);
}

partial struct rawMarshaler {
    internal marshaler m;
    internal bool checkNul;
}

internal static error marshal(this ref rawMarshaler m, any v, io.Writer w) {
    var (s, ok) = v._<@string>(ᐧ); if (ok) {
        if (m.checkNul && strings.ContainsRune(s, '\x00')) {
            return fmt.Errorf("cannot output a string containing NUL character: %q"u8, s);
        }
        var (_, err) = w.Write(slice<byte>(s));
        return err;
    }
    return m.m.marshal(v, w);
}

internal static ж<yamlMarshaler> yamlFormatter(ж<nint> indent) {
    return Ꮡ(new yamlMarshaler(indent));
}

partial struct yamlMarshaler {
    internal ж<nint> indent;
}

internal static error marshal(this ref yamlMarshaler m, any v, io.Writer w) {
    var enc = yaml.NewEncoder(w);
    var i = m.indent; if (i != nil) {
        enc.SetIndent(i.Value);
    } else {
        enc.SetIndent(2);
    }
    var err = enc.Encode(v); if (err != nil) {
        return err;
    }
    return enc.Close();
}

} // end cli_package
