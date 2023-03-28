using Microsoft.AspNetCore.Components.Web;

namespace LogicGame;

/// <summary>
/// Combines a cell and an image index
/// </summary>
public class CellMouseEventArgs : MouseEventArgs
{
    public CellMouseEventArgs(Cell cell, int imageIndex)
    {
        Cell = cell;
        ImageIndex = imageIndex;
    }

    public int ImageIndex { get; set; }
    public Cell Cell { get; }
}


