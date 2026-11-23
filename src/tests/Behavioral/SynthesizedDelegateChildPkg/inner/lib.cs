namespace go.SynthesizedDelegateChildPkg;

partial class inner_package {

partial struct Record {
    public @string Name;
    public nint Hits;
}

public delegate (ж<Record>, error) Loader(map<@string, ж<Record>> cache, @string name, Func<@string, (@string, error)> lookup);

} // end inner_package
