namespace LogicGame;

public class Cell
{
    public string Name { get; set; }
    public string FileName { get; set; }

    public Cell(string name, string fileName)
    {
        Name = name;
        FileName = fileName;
    }
}
