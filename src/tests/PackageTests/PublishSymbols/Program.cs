// Prints a frame that lives in the consumer's OWN referenced library. Its file:line is read from that library's .pdb,
// which a single-file publish leaves loose beside the executable; when the symbol file is missing the frame reads ":0".
public static class Program
{
    public static int Main()
    {
        System.Console.WriteLine("PUBLISH-SYMBOLS: " + PublishSymbolsLib.Where.Frame());
        return 0;
    }
}
