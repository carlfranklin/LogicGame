using Microsoft.AspNetCore.Components;

namespace LogicGame.Shared;

/// <summary>
/// PuzzleCell represents the UI around a single cell
/// </summary>
public partial class PuzzleCell: ComponentBase
{
    /// <summary>
    /// The cell being represented
    /// </summary>
    [Parameter]
    public Cell Cell { get; set; }

    /// <summary>
    /// The sorted cells provided by the application
    /// </summary>
    [Parameter]
    public List<List<Cell>> SortedCells { get; set; } 
        = new List<List<Cell>>();

    /// <summary>
    /// The list of unsorted cells provided by the application
    /// </summary>
    [Parameter]
    public List<List<Cell>> Cells { get; set; } 
        = new List<List<Cell>>();

    /// <summary>
    /// Event fired when the user hovers over a cell image
    /// </summary>
    [Parameter]
    public EventCallback<string> ImageHintChanged { get; set; }

    /// <summary>
    /// Event fired when a cell changes so the app can update the UI
    /// </summary>
    [Parameter]
    public EventCallback<Cell> CellChanged { get; set; }

    /// <summary>
    /// Cell Size (square) provided by the application
    /// </summary>
    [Parameter]
    public string BigCellWidth { get; set; }

    /// <summary>
    /// Interior (small) cell size provided by the application
    /// </summary>
    [Parameter]
    public string SmallCellWidth { get; set; }

    /// <summary>
    /// Helper method to raise ImageHintChanged event
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="index">If -1, we can use the cell name, 
    /// otherwwise we use the name of one of the small images
    /// </param>
    /// <returns></returns>
    protected async Task ShowImageName(Cell cell, int index = -1)
    {
        if (index == -1)
            // Use the cell name
            await ImageHintChanged.InvokeAsync(cell.Name);
        else
        {
            // Get the name of the small image by index
            var name = ImageName(cell, index);
            await ImageHintChanged.InvokeAsync(name);
        }
    }

    /// <summary>
    /// Helper method to get the image name given the index
    /// which is 0, 1, 2, or 3
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    protected string ImageName(Cell cell, int index)
    {
        // find this cell's parent list in Sorted Cells
        var imagesRows = (from x in SortedCells 
                          where x.Contains(cell) select x).ToList();

        // Get the row index
        var rowIndex = SortedCells.IndexOf(imagesRows.First());

        // Get the cells in the un-sorted list at this index
        var cells = Cells[rowIndex];

        // use the column to retrieve the original image file name
        string result = cells[index].Name;

        return result;
    }

    /// <summary>
    /// Helper name to get the filename of a smaller image
    /// given the index which is 0, 1, 2, or 3
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    protected string ImageFileName(Cell cell, int index)
    {
        // find this cell's parent list in Sorted Cells
        var imagesRows = (from x in SortedCells 
                          where x.Contains(cell) select x).ToList();
        
        // Get the row index
        var rowIndex = SortedCells.IndexOf(imagesRows.First());

        // Get the cells in the un-sorted list at this index
        var cells = Cells[rowIndex];

        // use the column to retrieve the original image file name
        string result = $"images/{cells[index].FileName}";

        return result;
    }

    /// <summary>
    /// Helper method to show or hide one of the smaller images
    /// using a custom CellMouseEventArgs class, which contains
    /// both a cell and a small image index
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    protected async Task ShowOrHide(CellMouseEventArgs args)
    {
        var cell = args.Cell;

        // are we de-selecting a possibility?
        if (cell.Possibilities[args.ImageIndex])
        {
            // Make sure this is NOT the solution image
            string imageName = ImageFileName(cell, args.ImageIndex);
            if (imageName.Contains(cell.FileName))
                return;
        }

        // Switch
        cell.Possibilities[args.ImageIndex] = 
            !cell.Possibilities[args.ImageIndex];

        // Notify the UI
        await CellChanged.InvokeAsync(cell);
    }

    /// <summary>
    /// Helper method to solve a cell and update the UI
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    protected async Task SolveCellAndNotify(Cell cell, int index)
    {
        // Only solve if the index matches the correct answer
        if (cell.Index == index)
        {
            cell.Solve();
            await CellChanged.InvokeAsync(cell);
        }
    }
}
