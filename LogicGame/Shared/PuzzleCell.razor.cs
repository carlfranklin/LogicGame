using Microsoft.AspNetCore.Components;

namespace LogicGame.Shared;

public partial class PuzzleCell: ComponentBase
{
    [Parameter]
    public Cell Cell { get; set; }

    [Parameter]
    public List<List<Cell>> SortedCells { get; set; } = new List<List<Cell>>();

    [Parameter]
    public List<List<Cell>> Cells { get; set; } = new List<List<Cell>>();

    [Parameter]
    public EventCallback<string> ImageHintChanged { get; set; }

    [Parameter]
    public EventCallback<Cell> CellChanged { get; set; }

    [Parameter]
    public string BigCellWidth { get; set; }

    [Parameter]
    public string SmallCellWidth { get; set; }

    public async Task ShowImageName(string imageName)
    {
        await ImageHintChanged.InvokeAsync(imageName);
    }

    protected string ImageName(Cell cell, int column)
    {
        // find this cell's parent list in Sorted Cells
        var imagesRows = (from x in SortedCells where x.Contains(cell) select x).ToList();
        
        // Get the row index
        var rowIndex = SortedCells.IndexOf(imagesRows.First());

        // Get the cells in the un-sorted list at this index
        var cells = Cells[rowIndex];

        // use the column to retrieve the original image file name
        string result = $"images/{cells[column].FileName}";

        return result;
    }

    protected async Task ShowOrHide(CustomMouseEventArgs args)
    {
        var cell = args.Cell;

        // are we de-selecting a possibility?
        if (cell.Possibilities[args.ImageIndex])
        {
            // Make sure this is NOT the solution image
            string imageName = ImageName(cell, args.ImageIndex);
            if (imageName.Contains(cell.FileName))
                return;
        }

        cell.Possibilities[args.ImageIndex] = !cell.Possibilities[args.ImageIndex];
        await CellChanged.InvokeAsync(cell);
    }

    protected async Task ShowImage(Cell cell, int index)
    {
        if (cell.Index == index)
        {
            cell.Solve();
            await CellChanged.InvokeAsync(cell);
        }
    }
}
