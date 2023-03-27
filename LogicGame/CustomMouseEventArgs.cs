using Microsoft.AspNetCore.Components.Web;

namespace LogicGame;

public class CustomMouseEventArgs : MouseEventArgs
{

    public CustomMouseEventArgs(Cell cell, int imageIndex)
    {
        Cell = cell;
        ImageIndex = imageIndex;
    }

    public int ImageIndex { get; set; }
    public Cell Cell { get; }
}


