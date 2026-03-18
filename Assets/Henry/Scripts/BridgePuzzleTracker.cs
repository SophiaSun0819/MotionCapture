using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BridgePuzzleTracker : MonoBehaviour
{
    [Header("Setup")]
    public List<SnapPlank> planks = new List<SnapPlank>();
    public int planksNeededToComplete = 7;

    [Header("Milestone Events")]
    public UnityEvent onFirstPlankPlaced;
    public UnityEvent onSecondPlankPlaced;
    public UnityEvent onBridgeComplete;

    private int placedCount = 0;

    private bool firstEventFired = false;
    private bool secondEventFired = false;
    private bool completeEventFired = false;

    private readonly HashSet<SnapPlank> countedPlanks = new HashSet<SnapPlank>();

    private void Update()
    {
        for (int i = 0; i < planks.Count; i++)
        {
            SnapPlank plank = planks[i];
            if (plank == null) continue;

            if (plank.IsSnapped && !countedPlanks.Contains(plank))
            {
                countedPlanks.Add(plank);
                placedCount++;

                if (!firstEventFired && placedCount >= 1)
                {
                    firstEventFired = true;
                    onFirstPlankPlaced?.Invoke();
                }

                if (!secondEventFired && placedCount >= 2)
                {
                    secondEventFired = true;
                    onSecondPlankPlaced?.Invoke();
                }

                if (!completeEventFired && placedCount >= planksNeededToComplete)
                {
                    completeEventFired = true;
                    onBridgeComplete?.Invoke();
                }
            }
        }
    }

    public int GetPlacedCount()
    {
        return placedCount;
    }

    public bool IsBridgeComplete()
    {
        return completeEventFired;
    }
}