[System.Serializable]
public class SaveData {
    public int rows;
    public int cols;
    public int[] faces; // sprite indices for each slot
    public bool[] matched;
    public int score;
    public int streak;
    public int seed;
}