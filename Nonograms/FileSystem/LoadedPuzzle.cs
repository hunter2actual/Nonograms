namespace Nonograms.FileSystem;

public record LoadedPuzzle
{
    public string title;
    public bool[,] data;
}