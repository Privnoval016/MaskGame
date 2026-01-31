public interface LogicSplitter
{
    /** <summary>
     * Given a truth value, returns the array of note lane indices that should be spawned for that beat.
     * If there are multiple note combinations that could result in the same truth value, return a random valid combination.
     * </summary>
     *
     * <param name="truthValue">The truth value for the current beat. Can be 0 or 1.</param>
     * <returns>An array of note lane indices to spawn for the given truth value.</returns>
     */
    public int[] GetNotesForBeat(int truthValue);
    
    /** <summary>
     * Evaluates the truth value based on the provided array.
     * </summary>
     *
     * <param name="inputs">An array representing the inputs (e.g., note parities) for evaluation.</param>
     * <returns>The evaluated truth value (true or false).</returns>
     */
    public bool EvaluateTruthValue(int[] inputs);
}