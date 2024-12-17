using JrEncoder;

namespace BuilderTests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ParityTest()
    {
        string comp =
            "55, 55, 27, 80, 80, 07, 52, 9B, 80, 2A, 52, 80, 80, 80, 80, 80, E3, 80, 80, 31, D5, 31, D5, 31, D5, 31, D5, 31, D5, 31, D5, 31, D5, 80, 80, 31, D5, 00, 55, 55, 27, 31, 64, 54, E5, 73, F4, 20, 4C, E9, 6E, E5, 20, 31, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 55, 55, 27, 52, 64, 54, E5, 73, F4, 20, 4C, E9, 6E, E5, 20, 32, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 55, 55, 27, E3, 64, 54, E5, 73, F4, 20, 4C, E9, 6E, E5, 20, B3, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F, 7F";
        
        OMCW omcw = new(false, false, false, false, false, true, true, true, 10, 3);
        
        DataFrame[] testPage = new PageBuilder(10)
            .setOMCW(omcw)
            .setPageAttributes(new PageAttributes(false, false, false, false, false, false))
            .setLineAttributes(
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue),
                new TextLineAttributes(false, false, false, true, Color.Blue)
            )
            .addLine(1, "Test Line 1")
            .addLine(2, "Test Line 2")
            .addLine(3, "Test Line 3")

            .setAddress(new Address(1, 2, 3, 4))
            .build();
        
        List<byte> outputData = new();
        foreach (DataFrame frame in testPage)
            outputData.AddRange(frame.getFrame());
        
        string output = outputData.Select(b => b.ToString("X2")).Aggregate((a, b) => $"{a}, {b}");
        
        Assert.That(output.Equals(comp));
    }
}