using System.Linq;
using Nonograms.Game;
using Xunit;

namespace Nonograms.Test;

public class Tests
{
    [Theory]
    [InlineData("## ### #", new[] { 2, 3, 1 }, new[] { true, true, true })]
    [InlineData("        ", new[] { 2, 3, 1 }, new[] { false, false, false })]
    [InlineData("   ### #", new[] { 2, 3, 1 }, new[] { false, true, true })]
    [InlineData("##     #", new[] { 2, 3, 1 }, new[] { true, false, true })]
    [InlineData("  ##     # ", new[] { 2, 3, 1 }, new[] { true, false, true })]
    [InlineData("########", new[] { 2, 3, 1 }, new[] { false, false, false })]
    [InlineData("########", new[] { 8 }, new[] { true })]
    [InlineData("## ##", new[] { 2, 2 }, new[] { true, true })]
    [InlineData("#    ", new[] { 2 }, new[] { false })]
    [InlineData("####", new[] { 3 }, new[] { false })]
    [InlineData("##  #  ##", new[] { 2, 1, 2 }, new[] { true, true, true })]
    [InlineData("### #  ##", new[] { 1, 3, 1, 2 }, new[] { false, true, true, true })]
    [InlineData("## ##", new[] { 2, 3 }, new[] { true, false })]
    [InlineData("# #", new[] { 3 }, new[] { false })]
    [InlineData("     ", new[] { 0 }, new[] { true })]
    [InlineData("#", new[] { 1 }, new[] { true })]
    [InlineData("  #", new[] { 1 }, new[] { true })]
    [InlineData("#  #", new[] { 1, 1 }, new[] { true, true })]
    [InlineData("### ##", new[] { 2, 3 }, new[] { false, false })]
    [InlineData("# ### ## ####", new[] { 1, 2, 3, 4 }, new[] { true, false, false, true })]
    [InlineData("# ### # ## ####", new[] { 1, 2, 1, 3, 4 }, new[] { true, false, true, false, true })]
    [InlineData("### ## ###", new[] { 2, 3 }, new[] { false, false })]
    [InlineData("###", new[] { 1, 1, 1 }, new[] { false, false, false })]
    [InlineData("## ##", new[] { 2, 1, 2 }, new[] { true, false, true })]
    [InlineData("## # ### # ##", new[] { 2, 1, 3, 1, 2 }, new[] { true, true, true, true, true })]
    [InlineData("#### ##", new[] { 2, 2, 2 }, new[] { false, false, true })]
    [InlineData("    ##    ##", new[] { 2, 2, 2 }, new[] { true, true, false })]
    [InlineData("  ##   ##  ", new[] { 2, 2, 2 }, new[] { true, true, false })]
    public void Test(string line, int[] hints, bool[] expected)
    {
        var cellStateLine = line.Select(x => x == '#' ? CellContents.Filled : CellContents.Nothing).ToArray();
        var result = HintSatisfactionAnalyser.CheckHintSatisfaction(cellStateLine, hints);
        Assert.Equal(expected, result);
    }
    
    // TODO only satisfy hints where the block is surrounded by crosses
}