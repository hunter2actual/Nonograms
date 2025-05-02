using System;
using System.Collections.Generic;

namespace Nonograms.FileSystem;

public class PuzzleStateManager(Configuration configuration)
{
    public void InitialisePack(string packName, List<LoadedPuzzle> loadedPuzzlePack)
    {
        if (!configuration.AllPuzzlesState.TryGetValue(packName, out var puzzles))
        {
            return;
        }
        
        for (int i = 0; i < loadedPuzzlePack.Count; i++)
        {
            var loadedPuzzleKey = packName + loadedPuzzlePack[i].title;
            if (!configuration.AllPuzzlesState.ContainsKey(loadedPuzzleKey))
            {
                configuration.AllPuzzlesState[loadedPuzzleKey] = new PuzzleState
                {
                    Pack = packName,
                    Title = loadedPuzzlePack[i].title,
                    Completed = false,
                    BoardState = new bool[,] { }
                };
            }
        }
        configuration.Save();
    }

    public PuzzleState GetPuzzleState(string pack, string title)
    {
        var key = pack + title;
        if (!configuration.AllPuzzlesState.ContainsKey(key))
        {
            configuration.AllPuzzlesState[key] = new PuzzleState
            {
                Pack = pack,
                Title = title,
                Completed = false,
                BoardState = new bool[,] { }
            };
        }
        
        return configuration.AllPuzzlesState[pack + title];
    }

    public void SetCurrentPuzzleState(PuzzleState puzzleState)
    {
        configuration.CurrentPuzzleStateKey = puzzleState.PackTitleKey;
        if (configuration.AllPuzzlesState is not null)
        {
            configuration.AllPuzzlesState[puzzleState.PackTitleKey] = puzzleState;
        }
        configuration.Save();
    }

    public PuzzleState GetCurrentPuzzleState()
    {
        if (configuration.CurrentPuzzleStateKey is null)
            throw new NullReferenceException("Current puzzle state has not been set");
        return configuration.AllPuzzlesState[configuration.CurrentPuzzleStateKey];
    }

    public void CompletedCurrentPuzzle()
    {
        if (configuration.CurrentPuzzleStateKey is null) return;
        
        configuration.AllPuzzlesState[configuration.CurrentPuzzleStateKey].Completed = true;
        configuration.CurrentPuzzleStateKey = null;
        configuration.Save();
    }

    public void ClearCurrentPuzzleState()
    {
        configuration.CurrentPuzzleStateKey = null;
        // TODO use on "reset puzzle" functionality?
        configuration.Save();
    }
}