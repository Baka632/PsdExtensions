namespace PsdExtensions.CSharp.Test;

public class TestForPsdPropertyHelper
{
    [Fact]
    public void TestForManaged()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "test.psd");
        (double x, double y, ResolutionUnit unit, short realLayerCount) = PsdPropertyHelper.GetPsdProperties(path);
        Assert.Equal(300, x);
        Assert.Equal(300, y);
        Assert.Equal(ResolutionUnit.Inch, unit);
        Assert.Equal(2, realLayerCount);
    }
}
