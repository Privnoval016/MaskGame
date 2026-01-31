using System.Collections.Generic;

public class OrSplitter : LogicSplitter
{
    private int numberOfSpawnLocations; // Total number of lanes available (valid note indices: 0 to numberOfSpawnLocations - 1)
    
    public OrSplitter(int numberOfSpawnLocations)
    {
        this.numberOfSpawnLocations = numberOfSpawnLocations;
    }
    
    public int[] GetNotesForBeat(int truthValue)
    {
        if (truthValue == 0)
        {
            // For OR operation, if the truth value is 0, no notes should be spawned
            return new int[0];
        }
        // If the truth value is 1, we can spawn notes in any non-empty combination of lanes
        System.Random rand = new System.Random();
        int subsetSize = rand.Next(1, numberOfSpawnLocations + 1); // Random size from 1 to numberOfSpawnLocations
        HashSet<int> selectedLanes = new HashSet<int>();
        while (selectedLanes.Count < subsetSize)
        {
            selectedLanes.Add(rand.Next(0, numberOfSpawnLocations));
        }
        int[] result = new int[subsetSize];
        selectedLanes.CopyTo(result);
        return result;
    }
}