namespace LogicGame;

public class Hint
{
    public HintType Type { get; set; }
    public string HintText { get; set; } = string.Empty;
    public List<Cell> Cells { get; set; } = new List<Cell>();
    public bool Checked { get; set; } = false;
    public bool ContainsCells(List<Cell> cells)
    {
        foreach (var cell in cells)
        {
            var match = (from x in Cells where x.Name == cell.Name select x).FirstOrDefault();
            if (match != null)
                return true;
        }
        return false;
    }
}
