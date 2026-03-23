using System.Collections.Generic;
using UnityEngine;

public class StonePuzzleTracker : MonoBehaviour
{
    public List<MovableStone> stones = new List<MovableStone>();
    public int stonesNeededToComplete = 3;

    private readonly HashSet<MovableStone> clearedStones = new HashSet<MovableStone>();

    public int GetClearedCount()
    {
        return clearedStones.Count;
    }

    public bool IsComplete()
    {
        return clearedStones.Count >= stonesNeededToComplete;
    }

    private void OnEnable()
    {
        for (int i = 0; i < stones.Count; i++)
        {
            if (stones[i] != null)
                stones[i].OnCleared += HandleStoneCleared;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < stones.Count; i++)
        {
            if (stones[i] != null)
                stones[i].OnCleared -= HandleStoneCleared;
        }
    }

    private void Start()
    {
        // Catch anything already cleared before play state logic starts
        for (int i = 0; i < stones.Count; i++)
        {
            if (stones[i] != null && stones[i].IsCleared)
                clearedStones.Add(stones[i]);
        }
    }

    private void HandleStoneCleared(MovableStone stone)
    {
        if (stone == null) return;
        clearedStones.Add(stone);
    }
}