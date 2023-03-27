namespace LogicGame;

public class Hint
{
    public HintType Type { get; set; }
    public string HintText { get; set; } = string.Empty;
    public List<Cell> Cells { get; set; } = new List<Cell>();

    public bool ContainsCells(List<Cell> cells)
    {
        foreach (var cell in cells)
        {
            if (!Cells.Contains(cell)) 
                return false;
        }
        return true;
    }
}
