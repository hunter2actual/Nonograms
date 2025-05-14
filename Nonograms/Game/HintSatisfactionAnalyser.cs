using System;
using System.Collections.Generic;

namespace Nonograms.Game;

// by Claude
public static class HintSatisfactionAnalyser
{
    /// Checks which hints in a Nonogram line are satisfied and should be greyed out.
    /// </summary>
    /// <param name="line">The current state of a row or column</param>
    /// <param name="hints">The numerical hints for the line (consecutive filled block lengths)</param>
    /// <returns>Array of booleans indicating which hints are satisfied</returns>
    public static bool[] CheckHintSatisfaction(CellContents[] line, int[] hints)
    {
        if (hints.Length == 0)
            return [];

        bool[] satisfied = new bool[hints.Length];
        List<int> blocks = GetFilledBlocks(line);

        if (blocks.Count == 0 || blocks.Count > hints.Length)
            return satisfied;

        // Perfect match case
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
                Array.Fill(satisfied, true);
                return satisfied;
            }
        }

        // Check for direct matches of blocks to hints by position
        // First from the beginning
        int j = 0;
        while (j < blocks.Count && j < hints.Length)
        {
            if (blocks[j] == hints[j])
            {
                satisfied[j] = true;
                j++;
            }
            else
            {
                break;
            }
        }

        // Then from the end
        int k = 0;
        while (k < blocks.Count && k < hints.Length &&
               blocks.Count - 1 - k >= j &&
               hints.Length - 1 - k >= j)
        {
            int blockIdx = blocks.Count - 1 - k;
            int hintIdx = hints.Length - 1 - k;

            if (blocks[blockIdx] == hints[hintIdx])
            {
                satisfied[hintIdx] = true;
                k++;
            }
            else
            {
                break;
            }
        }

        // Match by uniqueness for remaining blocks and hints
        //MatchUniqueBlocks(blocks, hints, satisfied, j, hints.Length - k);

        return satisfied;
    }
    
    // private static bool[] MatchUniqueBlocks(List<int> blocks, int[] hints, bool[] satisfied, int startHintIdx, int endHintIdx)
    // {
    //     List<int> unmatchedBlockValues = new List<int>();
    //     List<int> unmatchedBlockIndices = new List<int>();
    //
    //     for (int i = 0; i < blocks.Count; i++)
    //     {
    //         // Skip blocks that are likely already matched from start/end matching
    //         if (i < startHintIdx || i >= blocks.Count - (hints.Length - endHintIdx))
    //             continue;
    //
    //         unmatchedBlockValues.Add(blocks[i]);
    //         unmatchedBlockIndices.Add(i);
    //     }
    //
    //     // Create a list of unmatched hints and their indices
    //     List<int> unmatchedHintValues = [];
    //     List<int> unmatchedHintIndices = [];
    //
    //     for (int i = startHintIdx; i < endHintIdx; i++)
    //     {
    //         if (!satisfied[i])
    //         {
    //             unmatchedHintValues.Add(hints[i]);
    //             unmatchedHintIndices.Add(i);
    //         }
    //     }
    //
    //     // Find unique block values that match unique hint values
    //     for (int i = 0; i < unmatchedBlockValues.Count; i++)
    //     {
    //         int blockValue = unmatchedBlockValues[i];
    //         int blockIndex = unmatchedBlockIndices[i];
    //
    //         // Count how many times this value appears in unmatched blocks
    //         int blockValueCount = unmatchedBlockValues.Count(b => b == blockValue);
    //
    //         // Count how many times this value appears in unmatched hints
    //         int hintValueCount = unmatchedHintValues.Count(h => h == blockValue);
    //
    //         // If there's exactly one block and one hint with this value, it's a match
    //         if (blockValueCount == 1 && hintValueCount == 1)
    //         {
    //             int hintArrayIndex = unmatchedHintIndices[unmatchedHintValues.IndexOf(blockValue)];
    //
    //             // Check if the relative ordering is consistent
    //             bool orderingConsistent = true;
    //
    //             // The block at blockIndex should correspond to the hint at hintArrayIndex
    //             // Check other unique blocks to ensure they have the correct relative ordering
    //             for (int j = 0; j < unmatchedBlockValues.Count; j++)
    //             {
    //                 if (i == j) continue;
    //
    //                 int otherBlockValue = unmatchedBlockValues[j];
    //                 int otherBlockIndex = unmatchedBlockIndices[j];
    //
    //                 // Check if other block also has a unique matching hint
    //                 int otherBlockValueCount = unmatchedBlockValues.Count(b => b == otherBlockValue);
    //                 int otherHintValueCount = unmatchedHintValues.Count(h => h == otherBlockValue);
    //
    //                 if (otherBlockValueCount == 1 && otherHintValueCount == 1)
    //                 {
    //                     int otherHintArrayIndex = unmatchedHintIndices[unmatchedHintValues.IndexOf(otherBlockValue)];
    //
    //                     // Check ordering: if block i is before block j, hint i should be before hint j
    //                     if ((blockIndex < otherBlockIndex && hintArrayIndex > otherHintArrayIndex) ||
    //                         (blockIndex > otherBlockIndex && hintArrayIndex < otherHintArrayIndex))
    //                     {
    //                         orderingConsistent = false;
    //                         break;
    //                     }
    //                 }
    //             }
    //
    //             if (orderingConsistent)
    //             {
    //                 satisfied[hintArrayIndex] = true;
    //             }
    //         }
    //     }
    //
    //     return satisfied;
    // }

    private static List<int> GetFilledBlocks(CellContents[] line)
    {
        List<int> blocks = new List<int>();
        int currentBlockLength = 0;

        foreach (var cell in line)
        {
            if (cell == CellContents.Filled)
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