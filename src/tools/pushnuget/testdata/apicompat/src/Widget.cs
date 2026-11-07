namespace Fixture;

public class Widget
{
    VISIBILITY static readonly Widget Instance = new Widget();

    public int Size => 1;
#if ADDED
    public int Weight => 2;
#endif
}
