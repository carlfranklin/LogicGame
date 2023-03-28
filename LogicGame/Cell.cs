namespace LogicGame;

public class Cell
{
    /// <summary>
    /// The name that gets displayed when you hover over a picture in a cell
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The filename minus the path of the image file associated with this cell
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// The Index (0, 1, 2, or 3) of the image before shuffling
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 4 booleans in an array that represent whether the possible solutions
    /// are being shown. If they are all false, the cell is considered solved
    /// </summary>
    public bool[] Possibilities { get; set; }

    /// <summary>
    /// Returns true if all the possibilities are false
    /// </summary>
    /// <returns></returns>
    public bool IsSolved()
    {
        return Possibilities[0] == false
            && Possibilities[1] == false
            && Possibilities[2] == false
            && Possibilities[3] == false;
    }

    /// <summary>
    /// Sets all possibilities to false
    /// </summary>
    public void Solve()
    {
        Possibilities[0] = false;
        Possibilities[1] = false;
        Possibilities[2] = false;
        Possibilities[3] = false;
    }

    /// <summary>
    /// Constructor takes the name, filename, and index.
    /// It also initializes the possibilities
    /// </summary>
    /// <param name="name"></param>
    /// <param name="fileName"></param>
    /// <param name="index"></param>
    public Cell(string name, string fileName, int index)
    {
        Name = name;
        FileName = fileName;
        Index = index;
        Possibilities = new bool[] { true, true, true, true };
    }
}
