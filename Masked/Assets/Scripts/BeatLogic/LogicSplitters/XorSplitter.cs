using System.Collections.Generic;

public class XorSplitter : LogicSplitter
{
    private int numberOfSpawnLocations; // Total number of lanes available (valid note indices: 0 to numberOfSpawnLocations - 1)
    
    public XorSplitter(int numberOfSpawnLocations)
    {
        this.numberOfSpawnLocations = numberOfSpawnLocations;
    }
    
    public int[] GetNotesForBeat(int truthValue)
    {
        if (truthValue == 0)
        {
            // For XOR operation, if the truth value is 0, spawn notes in an even number of lanes
            System.Random rand = new System.Random();
            int subsetSize = rand.Next(0, numberOfSpawnLocations / 2 + 1) * 2; // Random even size
            HashSet<int> selectedLanes = new HashSet<int>();
            while (selectedLanes.Count < subsetSize)
            {
                selectedLanes.Add(rand.Next(0, numberOfSpawnLocations));
            }
            int[] result = new int[subsetSize];
            selectedLanes.CopyTo(result);
            return result;
        }
        else
        {
            // If the truth value is 1, spawn notes in an odd number of lanes
            System.Random rand = new System.Random();
            int subsetSize = rand.Next(0, (numberOfSpawnLocations + 1) / 2) * 2 + 1; // Random odd size
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
    
    public bool EvaluateTruthValue(int[] inputs)
    {
        int oneCount = 0;
        foreach (var input in inputs)
        {
            if (input == 1)
            {
                oneCount++;
            }
        }
        return oneCount % 2 == 1;
    }
}