using go;
using static go.builtin;

// Prints the first go.sort frame above a sort.Slice comparator, as the converted runtime resolves it (runtime.Caller):
// "<file>.go:<line>" when go.sort's .pdb is beside the application, "none" when it is not. The file:line of a std frame
// comes from the PACKAGE's symbol file and nowhere else, so this is what a NuGet consumer's traceback can show.
public static class Program
{
    public static int Main()
    {
        slice<nint> s = new nint[] { 3, 1, 2 }.slice();
        string found = "none";

        sort_package.Slice(s, (nint i, nint j) =>
        {
            for (nint skip = 1; skip < 8 && found == "none"; skip++)
            {
                (uintptr _, @string file, nint line, bool ok) = runtime_package.Caller(skip);
                string name = System.IO.Path.GetFileName(file.ToString());

                if (ok && line > 0 && (name == "zsortfunc.go" || name == "slice.go"))
                    found = $"{name}:{line}";
            }

            return s[i] < s[j];
        });

        System.Console.WriteLine("PACKAGE-SYMBOLS: " + found);
        return 0;
    }
}
