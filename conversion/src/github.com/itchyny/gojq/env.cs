namespace go.github.com.itchyny;

using context = context_package;

partial class gojq_package {

partial struct env {
    internal nint pc;
    internal ж<stack> stack;
    internal ж<stack> paths;
    internal ж<scopeStack> scopes;
    internal slice<any> values;
    internal slice<ж<code>> codes;
    internal slice<codeinfo> codeinfos;
    internal slice<fork> forks;
    internal bool backtrack;
    internal nint offset;
    internal nint expdepth;
    internal nint label;
    internal array<any> args = new(32);
    internal context.Context ctx;
}

internal static ж<env> newEnv(context.Context ctx) {
    return Ꮡ(new env(
        stack: newStack(),
        paths: newStack(),
        scopes: newScopeStack(),
        ctx: ctx
    ));
}

partial struct scope {
    internal nint id;
    internal nint offset;
    internal nint pc;
    internal nint saveindex;
    internal nint outerindex;
}

partial struct fork {
    internal nint pc;
    internal nint stackindex;
    internal nint stacklimit;
    internal nint scopeindex;
    internal nint scopelimit;
    internal nint pathindex;
    internal nint pathlimit;
    internal nint offset;
    internal nint expdepth;
}

} // end gojq_package
