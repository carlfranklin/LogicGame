namespace LogicGame;

public class Cell
{
    public string Name { get; set; }
    public string FileName { get; set; }
    public int Index { get; set; }

    public bool[] Possibilities { get; set; }

    public bool IsSolved()
    {
        return Possibilities[0] == false
            && Possibilities[1] == false
            && Possibilities[2] == false
            && Possibilities[3] == false;
    }

    public void Solve()
    {
        Possibilities[0] = false;
        Possibilities[1] = false;
        Possibilities[2] = false;
        Possibilities[3] = false;
    }

    public Cell(string name, string fileName, int index)
    {
        Name = name;
        FileName = fileName;
        Index = index;
        Possibilities = new bool[] { true, true, true, true };
    }
}
