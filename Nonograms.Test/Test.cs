using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Nonograms.Test;

public class Tests
{
    [Theory]
    [InlineData("## ### #", new[] { 2, 3, 1 }, new[] { true, true, true })]
    [InlineData("        ", new[] { 2, 3, 1 }, new[] { false, false, false })]
    [InlineData("   ### #", new[] { 2, 3, 1 }, new[] { false, true, true })]
    [InlineData("##     #", new[] { 2, 3, 1 }, new[] { true, false, true })]
    [InlineData("########", new[] { 2, 3, 1 }, new[] { false, false, false })]
    [InlineData("## ##", new[] { 2, 2 }, new[] { true, true })]
    [InlineData("#    ", new[] { 2 }, new[] { false })]
    [InlineData("####", new[] { 3 }, new[] { false })]
    [InlineData("##  #  ##", new[] { 2, 1, 2 }, new[] { true, true, true })]
    [InlineData("### #  ##", new[] { 3, 1, 2 }, new[] { true, true, true })]
    [InlineData("## ##", new[] { 2, 3 }, new[] { true, false })]
    [InlineData("# #", new[] { 3 }, new[] { false })]
    [InlineData("     ", new int[0], new bool[0])]
    [InlineData("#", new[] { 1 }, new[] { true })]
    [InlineData("#  #", new[] { 1, 1 }, new[] { true, true })]
    [InlineData("### ##", new[] { 2, 3 }, new[] { false, false })]
    [InlineData("###", new[] { 1, 1, 1 }, new[] { false, false, false })]
    [InlineData("## ##", new[] { 2, 1, 2 }, new[] { true, false, true })]
    [InlineData("## # ### # ##", new[] { 2, 1, 3, 1, 2 }, new[] { true, true, true, true, true })]
    public void Test(string line, int[] hints, bool[] expected)
    {
        var cellStateLine = line.Select(x => x == '#' ? CellState.Filled : CellState.Nothing).ToArray();
        var result = CheckHintSatisfactionAdvanced(cellStateLine, hints);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Checks which hints in a Nonogram line are satisfied and should be greyed out.
    /// </summary>
    /// <param name="line">The current state of a row or column</param>
    /// <param name="hints">The numerical hints for the line (consecutive filled block lengths)</param>
    /// <returns>Array of booleans indicating which hints are satisfied</returns>
    private bool[] CheckHintSatisfactionAdvanced(CellState[] line, int[] hints)
    {
        if (hints == null || hints.Length == 0)
            return Array.Empty<bool>();

        bool[] satisfied = new bool[hints.Length];
        List<int> blocks = GetFilledBlocks(line);

        // Quick check: If no blocks, nothing is satisfied
        if (blocks.Count == 0)
            return satisfied;

        // Quick check: If there are more blocks than hints, none can be definitively satisfied
        if (blocks.Count > hints.Length)
            return satisfied;

        // Special case: If the line is completely filled, check against the expected pattern
        if (line.All(cell => cell == CellState.Filled))
        {
            // Calculate the expected length of a fully filled line for these hints
            int expectedLength = hints.Sum() + hints.Length - 1;
            if (line.Length != expectedLength)
                return satisfied; // Invalid filling pattern
        }

        // The key insight: For nonograms, blocks must appear in the same order as hints
        // We need to determine if each block can be unambiguously matched to a specific hint

        // Step 1: Try to match blocks with hints in order
        // This approach assumes that blocks must appear in the same order as the hints
        TryOrderedMatching(blocks, hints, satisfied);

        // Step 2: If the above didn't match everything, try to match by uniqueness
        // This handles cases where there are unique block sizes that can only match specific hints
        TryUniqueMatching(blocks, hints, satisfied);

        return satisfied;
    }

    /// <summary>
    /// Tries to match blocks with hints assuming they appear in the same order
    /// </summary>
    private void TryOrderedMatching(List<int> blocks, int[] hints, bool[] satisfied)
    {
        // If we have exactly the same number of blocks as hints,
        // check if each block can only match one hint in order
        if (blocks.Count == hints.Length)
        {
            bool allMatch = true;
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i] != hints[i])
                {
                    allMatch = false;
                    break;
                }
            }

            if (allMatch)
            {
                // All blocks match all hints in order
                Array.Fill(satisfied, true);
                return;
            }
        }

        // Otherwise, try prefix matching
        int matchedBlocks = 0;

        // Match blocks from the start until we find a mismatch
        while (matchedBlocks < blocks.Count && matchedBlocks < hints.Length)
        {
            if (blocks[matchedBlocks] == hints[matchedBlocks])
            {
                satisfied[matchedBlocks] = true;
                matchedBlocks++;
            }
            else
            {
                break;
            }
        }

        // Match blocks from the end until we find a mismatch
        int blocksFromEnd = 0;
        while (blocksFromEnd < blocks.Count && blocksFromEnd < hints.Length &&
               blocks.Count - 1 - blocksFromEnd > matchedBlocks &&
               hints.Length - 1 - blocksFromEnd > matchedBlocks)
        {
            if (blocks[blocks.Count - 1 - blocksFromEnd] == hints[hints.Length - 1 - blocksFromEnd])
            {
                satisfied[hints.Length - 1 - blocksFromEnd] = true;
                blocksFromEnd++;
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// Tries to match blocks with hints based on uniqueness of sizes
    /// </summary>
    private void TryUniqueMatching(List<int> blocks, int[] hints, bool[] satisfied)
    {
        // Group blocks by size
        var blockGroups = blocks.GroupBy(size => size)
            .ToDictionary(g => g.Key, g => g.Count());

        // Group hints by size
        var hintGroups = new Dictionary<int, List<int>>();
        for (int i = 0; i < hints.Length; i++)
        {
            if (!hintGroups.ContainsKey(hints[i]))
                hintGroups[hints[i]] = new List<int>();
            hintGroups[hints[i]].Add(i);
        }

        // Look for blocks with unique sizes that can only match specific hints
        foreach (var blockGroup in blockGroups)
        {
            int size = blockGroup.Key;
            int count = blockGroup.Value;

            // If we have exactly the same number of blocks of a size as hints of that size
            if (hintGroups.ContainsKey(size) && hintGroups[size].Count == count)
            {
                // Mark all hints of this size as satisfied
                foreach (int hintIndex in hintGroups[size])
                {
                    satisfied[hintIndex] = true;
                }
            }
        }
    }

    /// <summary>
    /// Extracts lengths of continuous filled blocks from a line
    /// </summary>
    private List<int> GetFilledBlocks(CellState[] line)
    {
        List<int> blocks = new List<int>();
        int currentBlockLength = 0;

        foreach (var cell in line)
        {
            if (cell == CellState.Filled)
            {
                currentBlockLength++;
            }
            else if (currentBlockLength > 0)
            {
                blocks.Add(currentBlockLength);
                currentBlockLength = 0;
            }
        }

        // Don't forget to add the last block if the line ends with filled cells
        if (currentBlockLength > 0)
            blocks.Add(currentBlockLength);

        return blocks;
    }
}