using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Utility;

namespace Nonograms.FileSystem;

public class PuzzleLoader(string directoryName)
{
    private List<LoadedPuzzle> _loadedPuzzles;
    private string _loadedFile;
    
    public Dictionary<string, string> GetFileNames()
    {
        var path = Path.Combine(directoryName, "Puzzles");
        return Directory.GetFiles(path).ToDictionary(
            x => x,
            x =>
            {
                var fileName = Path.GetFileNameWithoutExtension(x);
                return fileName[3..].Replace('_', ' ').Trim();
            });
    }
    
    public List<LoadedPuzzle> GetPuzzles(string file)
    {
        if (_loadedFile != file) // caching
        {
            _loadedFile = file;
            _loadedPuzzles = ParsePuzzles(File.ReadAllLines(_loadedFile));
        }

        return _loadedPuzzles;
    }

    private List<LoadedPuzzle> ParsePuzzles(string[] lines)
    {
        var result = new List<LoadedPuzzle>();
        string? currentPuzzleTitle = null;
        var currentPuzzleData = new List<bool[]>();
        foreach (var line in lines)
        {
            if (line.Contains('#') || line.IsNullOrWhitespace()) // puzzle data
            {
                currentPuzzleData.Add(line.Select(x => x == '#').ToArray());
            }
            else // puzzle name
            {
                if (currentPuzzleTitle is not null)
                {
                    result.Add(new LoadedPuzzle
                    {
                        title = currentPuzzleTitle,
                        data = CleanPuzzleData(currentPuzzleData)
                    });
                }
                
                currentPuzzleTitle = line.Trim();
                currentPuzzleData = new List<bool[]>();
            }
        }
        result.Add(new LoadedPuzzle
        {
            title = currentPuzzleTitle,
            data = CleanPuzzleData(currentPuzzleData)
        });

        return result;
    }

    private bool[,] CleanPuzzleData(List<bool[]> input)
    {
        var width = input.Select(line => line.Length).Max();
        var height = input.Count;
        var result = new bool[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                result[x, y] = GetBoolAt(input[y], x);
            }
        }

        return result;
    }
    
    bool GetBoolAt(bool[] array, int index) // Defaults to false if index out of bounds
    {
        return index >= 0 && index < array.Length && array[index];
    }
}