using System.Collections.Generic;

public class AndSplitter : LogicSplitter
{
    private int numberOfSpawnLocations; // Total number of lanes available (valid note indices: 0 to numberOfSpawnLocations - 1)
    
    public AndSplitter(int numberOfSpawnLocations)
    {
        this.numberOfSpawnLocations = numberOfSpawnLocations;
    }
    
    public int[] GetNotesForBeat(int truthValue)
    {
        if (truthValue == 1)
        {
            // For AND operation, if the truth value is 1, spawn notes in all lanes
            int[] allLanes = new int[numberOfSpawnLocations];
            for (int i = 0; i < numberOfSpawnLocations; i++)
            {
                allLanes[i] = i;
            }
            return allLanes;
        }
        // If the truth value is 0, we can spawn notes in any combination of lanes except all lanes, so we pick a random non-empty subset
        
        System.Random rand = new System.Random();
        int subsetSize = rand.Next(1, numberOfSpawnLocations); // Random size from 1 to numberOfSpawnLocations - 1
        HashSet<int> selectedLanes = new HashSet<int>();
        while (selectedLanes.Count < subsetSize)
        {
            selectedLanes.Add(rand.Next(0, numberOfSpawnLocations));
        }
        int[] result = new int[subsetSize];
        selectedLanes.CopyTo(result);
        return result;
    }
    
    public bool EvaluateTruthValue(int[] inputs)
    {
        foreach (var input in inputs)
        {
            if (input == 0)
            {
                return false;
            }
        }
        return true;
    }
}