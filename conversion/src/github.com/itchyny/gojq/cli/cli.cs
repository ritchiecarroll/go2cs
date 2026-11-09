namespace go.github.com.itchyny.gojq;

using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using runtime = runtime_package;
using strings = strings_package;
using isatty = go.github.com.mattn.go_isatty.isatty_package;
using gojq = go.github.com.itchyny.gojq_package;
using go.github.com.itchyny;
using go.github.com.mattn.go_isatty;

partial class cli_package {

internal static readonly @string name = "gojq"u8;

internal static readonly @string version = "0.12.19"u8;

internal static @string revision = "HEAD";

internal static UntypedInt exitCodeOK => iota;
internal static UntypedInt exitCodeFalsyErr => 1;
internal static UntypedInt exitCodeFlagParseErr => 2;
internal static UntypedInt exitCodeCompileErr => 3;
internal static UntypedInt exitCodeNoValueErr => 4;
internal static UntypedInt exitCodeDefaultErr => 5;

partial struct cli {
    internal io.Reader inStream;
    internal io.Writer outStream;
    internal io.Writer errStream;
    internal bool outputRaw;
    internal bool outputRaw0;
    internal bool outputJoin;
    internal bool outputCompact;
    internal ж<nint> outputIndent;
    internal bool outputTab;
    internal bool outputYAML;
    internal bool inputRaw;
    internal bool inputStream;
    internal bool inputYAML;
    internal bool inputSlurp;
    internal slice<@string> argnames;
    internal slice<any> argvalues;
    internal bool outputYAMLSeparator;
    internal error exitCodeError;
}

partial struct flagopts {
    public bool OutputRaw; /*`short:"r" long:"raw-output" description:"output raw strings"`*/
    public bool OutputRaw0; /*`long:"raw-output0" description:"implies -r with NUL character delimiter"`*/
    public bool OutputJoin; /*`short:"j" long:"join-output" description:"implies -r with no newline delimiter"`*/
    public bool OutputCompact; /*`short:"c" long:"compact-output" description:"output without pretty-printing"`*/
    public ж<nint> OutputIndent; /*`long:"indent" args:"number" description:"number of spaces for indentation"`*/
    public bool OutputTab; /*`long:"tab" description:"use tabs for indentation"`*/
    public bool OutputYAML; /*`long:"yaml-output" description:"output in YAML format"`*/
    public bool OutputColor; /*`short:"C" long:"color-output" description:"output with colors even if piped"`*/
    public bool OutputMono; /*`short:"M" long:"monochrome-output" description:"output without colors"`*/
    public bool InputNull; /*`short:"n" long:"null-input" description:"use null as input value"`*/
    public bool InputRaw; /*`short:"R" long:"raw-input" description:"read input as raw strings"`*/
    public bool InputStream; /*`long:"stream" description:"parse input in stream fashion"`*/
    public bool InputYAML; /*`long:"yaml-input" description:"read input as YAML format"`*/
    public bool InputSlurp; /*`short:"s" long:"slurp" description:"read all inputs into an array"`*/
    public bool FromFile; /*`short:"f" long:"from-file" description:"load query from file"`*/
    public slice<@string> ModulePaths; /*`short:"L" long:"library-path" args:"dir" description:"directory to search modules from"`*/
    public map<@string, @string> Arg; /*`long:"arg" args:"name value" description:"set a string value to a variable"`*/
    public map<@string, @string> ArgJSON; /*`long:"argjson" args:"name value" description:"set a JSON value to a variable"`*/
    public map<@string, @string> SlurpFile; /*`long:"slurpfile" args:"name file" description:"set the JSON contents of a file to a variable"`*/
    public map<@string, @string> RawFile; /*`long:"rawfile" args:"name file" description:"set the contents of a file to a variable"`*/
    public slice<any> Args; /*`long:"args" positional:"" description:"consume remaining arguments as positional string values"`*/
    public slice<any> JSONArgs; /*`long:"jsonargs" positional:"" description:"consume remaining arguments as positional JSON values"`*/
    public bool ExitStatus; /*`short:"e" long:"exit-status" description:"exit 1 when the last value is false or null"`*/
    public bool Version; /*`short:"v" long:"version" description:"display version information"`*/
    public bool Help; /*`short:"h" long:"help" description:"display this help information"`*/
}

internal static bool addDefaultModulePaths = true;

internal partial interface run_type /*dyn*/ {
    void isEmptyError();
}

internal partial interface run_typeᴛ1 /*dyn*/ {
    nint ExitCode();
}

internal static nint run(this ж<cli> Ꮡcli, slice<@string> args) {
    ref var cli = ref Ꮡcli.DerefOrNull();

    var err = Ꮡcli.runInternal(args); if (err != nil) {
        if (!err._<run_type>(out _)) {
            fmt.Fprintf(cli.errStream, "%s: %s\n"u8, name, err);
        }
        if (err._<run_typeᴛ1>(out var errΔ1)) {
            return errΔ1.ExitCode();
        }
        return exitCodeDefaultErr;
    }
    return exitCodeOK;
}

internal partial interface runInternal_type /*dyn*/ {
    uintptr Fd();
}

internal partial interface runInternal_typeᴛ1 /*dyn*/ {
    (@string, @string, error) QueryParseError();
}

internal partial interface runInternal_typeᴛ2 /*dyn*/ {
    (@string, @string, error) JSONParseError();
}

internal static error /*err*/ runInternal(this ж<cli> Ꮡcli, slice<@string> args) {
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var cli = ref Ꮡcli.DerefOrNull();
        ref var err = ref Ꮡerr.ValueSlot;

        ref var opts = ref heap(new flagopts(), out var Ꮡopts);
        (args, err) = parseFlags(args, Ꮡopts);
        if (err != nil) {
            err = new flagParseErrorжerror(Ꮡ(new flagParseError(err))); goto ᒐdone;
        }
        if (opts.Help) {
            fmt.Fprintf(cli.outStream, """
%[1]s - Go implementation of jq

Version: %s (rev: %s/%s)

Synopsis:
  %% echo '{"foo": 128}' | %[1]s '.foo'

Usage:
  %[1]s [OPTIONS]


"""u8.Raw(),
                cli_package.name, version, revision, runtime.Version());
            fmt.Fprintln(cli.outStream, formatFlags(Ꮡopts));
            err = default; goto ᒐdone;
        }
        if (opts.Version) {
            fmt.Fprintf(cli.outStream, "%s %s (rev: %s/%s)\n"u8, cli_package.name, version, revision, runtime.Version());
            err = default; goto ᒐdone;
        }
        (cli.outputRaw, cli.outputRaw0, cli.outputJoin, cli.outputCompact, cli.outputIndent, cli.outputTab, cli.outputYAML) = (opts.OutputRaw, opts.OutputRaw0, opts.OutputJoin, opts.OutputCompact, opts.OutputIndent, opts.OutputTab, opts.OutputYAML);
        defer((bool x) => {
            noColor = x;
        }, noColor, ref ᒐ);
        if (opts.OutputColor || opts.OutputMono) {
            noColor = opts.OutputMono;
        } else if (os.Getenv("NO_COLOR") != ""u8 || os.Getenv("TERM") == "dumb"u8) {
            noColor = true;
        } else {
            var (f, ok) = cli.outStream._<runInternal_type>(ᐧ);
            noColor = !(ok && (isatty.IsTerminal(f.Fd()) || isatty.IsCygwinTerminal(f.Fd())));
        }
        if (!noColor) {
            @string colors = os.Getenv("GOJQ_COLORS"); if (colors != ""u8) {
                var errΔ1 = setColors(colors); if (errΔ1 != nil) {
                    err = errΔ1; goto ᒐdone;
                }
            }
        }
        {
            var i = cli.outputIndent; if (i != nil) {
                if (i.Value > 9) {
                    err = fmt.Errorf("too many indentation count: %d"u8, i.Value); goto ᒐdone;
                } else if (i.Value < 0) {
                    err = fmt.Errorf("negative indentation count: %d"u8, i.Value); goto ᒐdone;
                }
            }
        }
        if (opts.OutputYAML && opts.OutputTab) {
            err = errors.New("cannot use tabs for YAML output"); goto ᒐdone;
        }
        (cli.inputRaw, cli.inputStream, cli.inputYAML, cli.inputSlurp) = (opts.InputRaw, opts.InputStream, opts.InputYAML, opts.InputSlurp);
        foreach (var (k, v) in opts.Arg) {
            cli.argnames = append(cli.argnames, "$"u8 + k);
            cli.argvalues = append(cli.argvalues, (any)(v));
        }
        foreach (var (k, v) in opts.ArgJSON) {
            var (val, _) = newJSONInputIter(new strings_ReaderжReader(strings.NewReader(v)), "$"u8 + k).Next();
            var (errΔ2, ok) = val._<error>(ᐧ); if (ok) {
                err = errΔ2; goto ᒐdone;
            }
            cli.argnames = append(cli.argnames, "$"u8 + k);
            cli.argvalues = append(cli.argvalues, val);
        }
        foreach (var (k, v) in opts.SlurpFile) {
            var (val, errΔ3) = slurpFile(v);
            if (errΔ3 != nil) {
                err = errΔ3; goto ᒐdone;
            }
            cli.argnames = append(cli.argnames, "$"u8 + k);
            cli.argvalues = append(cli.argvalues, val);
        }
        foreach (var (k, v) in opts.RawFile) {
            var (val, errΔ4) = os.ReadFile(v);
            if (errΔ4 != nil) {
                err = errΔ4; goto ᒐdone;
            }
            cli.argnames = append(cli.argnames, "$"u8 + k);
            cli.argvalues = append(cli.argvalues, (any)((@string)val));
        }
        var named = new map<@string, any>(len(cli.argnames));
        foreach (var (i, name) in range(cli.argnames)) {
            named[name[1..]] = cli.argvalues[i];
        }
        var positional = opts.Args;
        foreach (var (i, v) in range(opts.JSONArgs)) {
            if (v != nil) {
                var (val, _) = newJSONInputIter(new strings_ReaderжReader(strings.NewReader(v._<@string>())), "--jsonargs").Next();
                var (errΔ5, ok) = val._<error>(ᐧ); if (ok) {
                    err = errΔ5; goto ᒐdone;
                }
                if (i < len(positional)) {
                    positional[i] = val;
                } else {
                    positional = append(positional, val);
                }
            }
        }
        cli.argnames = append(cli.argnames, (@string)"$ARGS");
        cli.argvalues = append(cli.argvalues, (any)(new map<@string, any>{
            ["named"] = named,
            ["positional"] = positional
        }));
        ref var arg = ref heap(new @string(), out var Ꮡarg);
        ref var fname = ref heap(new @string(), out var Ꮡfname);
        if (opts.FromFile) {
            if (len(args) == 0) {
                err = errors.New("expected a query file for flag `-f'"); goto ᒐdone;
            }
            var (src, errΔ6) = os.ReadFile(args[0]);
            if (errΔ6 != nil) {
                err = errΔ6; goto ᒐdone;
            }
            (arg, args, fname) = ((@string)src, args[1..], args[0]);
        } else if (len(args) == 0) {
            arg = ".";
        } else {
            (arg, args, fname) = (strings.TrimSpace(args[0]), args[1..], "<arg>");
        }
        if (opts.ExitStatus) {
            cli.exitCodeError = new exitCodeErrorжerror(Ꮡ(new exitCodeError(exitCodeNoValueErr)));
            defer(() => {
                var (_, ok) = Ꮡerr.ValueSlot._<run_typeᴛ1>(ᐧ); if (!ok) {
                    Ꮡerr.ValueSlot = Ꮡcli.Value.exitCodeError;
                }
            }, ref ᒐ);
        }
        (var query, err) = gojq.Parse(arg);
        if (err != nil) {
            err = new queryParseErrorжerror(Ꮡ(new queryParseError(fname, arg, err))); goto ᒐdone;
        }
        var modulePaths = opts.ModulePaths;
        if (len(modulePaths) == 0 && addDefaultModulePaths) {
            modulePaths = ["~/.jq", "$ORIGIN/../lib/gojq", "$ORIGIN/../lib"];
        }
        var iter = Ꮡcli.createInputIter(args);
        var iterʗ1 = iter;
        defer(() => iterʗ1.Close(), ref ᒐ);
                var argsʗ1 = args;
        (var code, err) = gojq.Compile(query,
            gojq.WithModuleLoader(gojq.NewModuleLoader(modulePaths)),
            gojq.WithEnvironLoader(os.Environ),
            gojq.WithVariables(cli.argnames),
            gojq.WithFunction("debug", 0, 0, Ꮡcli.funcDebug),
            gojq.WithFunction("stderr", 0, 0, Ꮡcli.funcStderr),
            gojq.WithFunction("input_filename", 0, 0,
                func(Func<any, slice<any>, any> (inputIter iterΔ1) => {
                    var argsʗ2 = argsʗ1;
                    return (any _Δp0, slice<any> _Δp1) => {
                        @string fnameΔ1 = iterΔ1.Name(); if (fnameΔ1 != ""u8 && (len(argsʗ2) > 0 || !Ꮡopts.Value.InputNull)) {
                            return fnameΔ1;
                        }
                        return default;
                    };
                })(iter)),
            gojq.WithInputIter(iter));
        if (err != nil) {
            if (err._<runInternal_typeᴛ1>(out var errΔ7)) {
                ref var name = ref heap<@string>(out var Ꮡname);
                ref var queryΔ1 = ref heap<@string>(out var ᏑqueryΔ1);
                (name, queryΔ1, var errΔ8) = errΔ7.QueryParseError();
                err = new queryParseErrorжerror(Ꮡ(new queryParseError(name, queryΔ1, errΔ8))); goto ᒐdone;
            }
            if (err._<runInternal_typeᴛ2>(out var errΔ9)) {
                ref var fnameΔ2 = ref heap<@string>(out var ᏑfnameΔ2);
                ref var contents = ref heap<@string>(out var Ꮡcontents);
                (fnameΔ2, contents, var errΔ10) = errΔ9.JSONParseError();
                err = new compileErrorжerror(Ꮡ(new compileError(new jsonParseErrorжerror(Ꮡ(new jsonParseError(fnameΔ2, contents, 0, errΔ10)))))); goto ᒐdone;
            }
            err = new compileErrorжerror(Ꮡ(new compileError(err))); goto ᒐdone;
        }
        if (opts.InputNull) {
            iter = newNullInputIter();
        }
        err = cli.process(iter, code);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return Ꮡerr.ValueSlot;
}

internal static (any, error) slurpFile(@string name) {
    GoFrame ᒐ = default;
    try {
        var iter = newSlurpInputIter(
            newFilesInputIter(newJSONInputIter, [name], default));
        var iterʗ1 = iter;
        defer(() => iterʗ1.Close(), ref ᒐ);
        var (val, _) = iter.Next();
        var (err, ok) = val._<error>(ᐧ); if (ok) {
            return (default, err);
        }
        return (val, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static inputIter /*iter*/ createInputIter(this ж<cli> Ꮡcli, slice<@string> args) {
    inputIter iter = default;
    GoFrame ᒐ = default;
    try {
        ref var cli = ref Ꮡcli.DerefOrNull();

        Func<io.Reader, @string, inputIter> newIter = default;
        switch (ᐧ) {
        case {} when cli.inputRaw: {
            if (cli.inputSlurp) {
                newIter = newReadAllIter;
            } else {
                newIter = newRawInputIter;
            }
            break;
        }
        case {} when cli.inputStream: {
            newIter = newStreamInputIter;
            break;
        }
        case {} when cli.inputYAML: {
            newIter = newYAMLInputIter;
            break;
        }
        default: {
            newIter = newJSONInputIter;
            break;
        }}

        if (cli.inputSlurp) {
            defer(() => {
                if (Ꮡcli.Value.inputRaw) {
                    iter = newSlurpRawInputIter(iter);
                } else {
                    iter = newSlurpInputIter(iter);
                }
            }, ref ᒐ);
        }
        if (len(args) == 0) {
            iter = newIter(cli.inStream, "<stdin>"); goto ᒐdone;
        }
        iter = newFilesInputIter(newIter, args, cli.inStream);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return iter;
}

internal static error process(this ref cli cli, inputIter iter, ж<gojq.Code> code) {
    error err = default;
    for (;;) {
        var (v, ok) = iter.Next();
        if (!ok) {
            break;
        }
        {
            var (e, okΔ1) = v._<error>(ᐧ); if (okΔ1) {
                fmt.Fprintf(cli.errStream, "%s: %s\n"u8, name, e);
                err = e;
                continue;
            }
        }
        {
            var e = cli.printValues(code.Run(v, cli.argvalues.ꓸꓸꓸ)); if (e != nil) {
                var (eΔ1, okΔ2) = e._<ж<gojq.HaltError>>(ᐧ); if (okΔ2) {
                    var vΔ1 = eΔ1.Value(); if (vΔ1 != nil) {
                        var (str, okΔ3) = vΔ1._<@string>(ᐧ); if (okΔ3) {
                            cli.errStream.Write(slice<byte>(str));
                        } else {
                            var (bs, _) = gojq.Marshal(vΔ1);
                            cli.errStream.Write(bs);
                            cli.errStream.Write([(byte)'\n']);
                        }
                    }
                    err = new gojq_HaltErrorжerror(eΔ1);
                    break;
                }
                fmt.Fprintf(cli.errStream, "%s: %s\n"u8, name, e);
                err = e;
            }
        }
    }
    if (err != nil) {
        return new emptyErrorжerror(Ꮡ(new emptyError(err)));
    }
    return default;
}

internal static error printValues(this ref cli cli, gojq.Iter iter) {
    var m = cli.createMarshaler();
    for (;;) {
        var (v, ok) = iter.Next();
        if (!ok) {
            break;
        }
        {
            var (err, okΔ1) = v._<error>(ᐧ); if (okΔ1) {
                return err;
            }
        }
        if (cli.outputYAMLSeparator) {
            cli.outStream.Write(slice<byte>("---\n"u8));
        } else {
            cli.outputYAMLSeparator = cli.outputYAML;
        }
        {
            var err = m.marshal(v, cli.outStream); if (err != nil) {
                return err;
            }
        }
        if (cli.exitCodeError != nil) {
            if (v == nil || AreEqual(v, false)) {
                cli.exitCodeError = new exitCodeErrorжerror(Ꮡ(new exitCodeError(exitCodeFalsyErr)));
            } else {
                cli.exitCodeError = new exitCodeErrorжerror(Ꮡ(new exitCodeError(exitCodeOK)));
            }
        }
        if (!cli.outputYAML) {
            if (cli.outputRaw0) {
                cli.outStream.Write([(byte)'\x00']);
            } else if (!cli.outputJoin) {
                cli.outStream.Write([(byte)'\n']);
            }
        }
    }
    return default;
}

internal static marshaler createMarshaler(this ref cli cli) {
    if (cli.outputYAML) {
        return new yamlMarshalerжmarshaler(yamlFormatter(cli.outputIndent));
    }
    nint indent = 2;
    if (cli.outputCompact) {
        indent = -1;
    } else if (cli.outputTab) {
        indent = 1;
    } else if (cli.outputIndent is var i && i != nil) {
        indent = i.Value;
    }
    var f = newEncoder(cli.outputTab, indent);
    if (cli.outputRaw || cli.outputRaw0 || cli.outputJoin) {
        return new rawMarshalerжmarshaler(Ꮡ(new rawMarshaler(new encoderжmarshaler(f), cli.outputRaw0)));
    }
    return new encoderжmarshaler(f);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object debugˢ = (@string)"DEBUG:"u8;

internal static any funcDebug(this ref cli cli, any v, slice<any> _Δp2) {
    {
        var err = newEncoder(false, -1).marshal(slice<any>([debugˢ, v]), cli.errStream); if (err != nil) {
            return err;
        }
    }
    {
        var (_, err) = cli.errStream.Write([(byte)'\n']); if (err != nil) {
            return err;
        }
    }
    return v;
}

internal static any funcStderr(this ref cli cli, any v, slice<any> _) {
    var err = (Ꮡ(new rawMarshaler(m: new encoderжmarshaler(newEncoder(false, -1))))).marshal(v, cli.errStream); if (err != nil) {
        return err;
    }
    return v;
}

} // end cli_package
