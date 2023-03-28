﻿using LogicGame.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Security.Cryptography;

namespace LogicGame.Pages;

public partial class Index : ComponentBase
{
    /// <summary>
    /// The message shown at the top of the page
    /// </summary>
    protected string Message { get; set; } = "Carl Franklin's Logic Puzzle";

    /// <summary>
    /// The image hint shown when you hover over an image in a cell
    /// </summary>
    protected string ImageHint { get; set; } = "Hover over an image to see a description";

    /// <summary>
    /// The sorted or shuffled cells
    /// </summary>
    protected List<List<Cell>> SortedCells { get; set; } = new List<List<Cell>>();

    /// <summary>
    /// The un-sorted cells in the original order
    /// </summary>
    protected List<List<Cell>> Cells { get; set; } = new List<List<Cell>>();

    /// <summary>
    /// Hints, generated by the app code
    /// </summary>
    protected List<Hint> Hints { get; set; } = new List<Hint>();

    /// <summary>
    /// Bound to an input on the main screen to filter the list of hints
    /// </summary>
    protected string HintFilter = string.Empty;

    /// <summary>
    /// Filtered hints, bound to the HintFilter string
    /// </summary>
    protected List<Hint> FilteredHints => Hints.Where(i => i.HintText.ToLower().Contains(HintFilter.ToLower())).ToList();

    /// <summary>
    /// Cell size (big and small). You can control the size of the UI with these
    /// </summary>
    protected string BigCellWidth = "101px";
    protected string SmallCellWidth = "50px";

    /// <summary>
    /// Used to show and hide the canvas for the confetti
    /// </summary>
    protected string CanvasDisplay { get; set; } = "none";

    /// <summary>
    /// Modal for showing the help screen
    /// </summary>
    [CascadingParameter]
    public IModalService Modal { get; set; } = default!;

    /// <summary>
    /// Used to call JavaScript
    /// </summary>
    [Inject]
    public IJSRuntime jSRuntime { get; set; }

    /// <summary>
    /// When the x button is selected, we clear the hint filter
    /// and set focus to the hintFilter input
    /// </summary>
    /// <returns></returns>
    protected async Task ClearHintFilter()
    {
        HintFilter = "";
        await jSRuntime.InvokeVoidAsync("SetFocus", "hintFilter");
    }

    /// <summary>
    /// Shows the Help Modal
    /// </summary>
    /// <returns></returns>
    protected async Task ShowHelp()
    {
        ModalOptions options = new ModalOptions()
        {
            DisableBackgroundCancel = true,
            Size = ModalSize.Large,
            Position = ModalPosition.Middle
        };
        // Show the modal
        var modal = Modal.Show<HelpModal>("Logic Puzzle Help", options);
        // wait for it to close
        await modal.Result;
    }

    /// <summary>
    /// Event handler in which we calll UpdateGameState to ensure
    /// the state of every cell is good.
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    protected async Task OnCellChanged(Cell cell)
    {
        await UpdateGameState();
    }

    /// <summary>
    /// Event handler to show the image hint when the user 
    /// mouses over a picture in a cell
    /// </summary>
    /// <param name="imageHint"></param>
    protected void OnImageHintChanged(string imageHint)
    {
        // If the puzzle is solved, DO NOT show the ImageHint,
        // because it says "refresh the browser to play again"
        if (!IsPuzleSolved())
            ImageHint = imageHint;
    }

    /// <summary>
    /// One of four methods that generate clues.
    /// SameColumn says "X is in the same column as Y"
    /// </summary>
    /// <returns></returns>
    private Hint GenerateSameColumnClue()
    {
        // Same Column Clue
        var hint = new Hint();
        hint.Type = HintType.SameColumn;

        // pick 2 random rows (Y)
        var row1 = GetRandom();
        var row2 = GetRandom(new int[] { row1 });

        // pick 1 random column (X)
        var col1 = GetRandom();

        // These are the List<Cell> for each row
        var imageRow1 = SortedCells[row1 - 1];
        var imageRow2 = SortedCells[row2 - 1];

        // Add these cells to the hint.Cells list
        hint.Cells.Add(imageRow1[col1 - 1]);
        hint.Cells.Add(imageRow2[col1 - 1]);

        // This will return not null if we already have a hint
        // in the Hints collection of this type using the same
        // cells.
        var match = (from x in Hints
                     where x.Type == HintType.SameColumn
                     && x.ContainsCells(hint.Cells)
                     select x).FirstOrDefault();

        if (match != null)
            // Another hint with these cells already exists. Bail
            return null;

        // Construct the hint text
        var firstItem = hint.Cells[0].Name;
        // Capitalize the first cell name because it starts the sentence.
        firstItem = firstItem.Substring(0, 1).ToUpper() + firstItem.Substring(1);
        // Put it together
        hint.HintText = $"{firstItem} " +
            $"is in the same column as {hint.Cells[1].Name}"; ;

        return hint;
    }


    /// <summary>
    /// One of four methods that generate clues.
    /// NextTo says "The column with X is next to the column with Y"
    /// </summary>
    /// <returns></returns>
    private Hint GenerateNextToClue()
    {
        // Next-To Clue
        var hint = new Hint();
        hint.Type = HintType.NextTo;

        // pick 2 random rows (Y)
        var row1 = GetRandom();
        var row2 = GetRandom(new int[] { row1 });

        // randomly pick 2 columns (X) next to each other
        var col1 = RandomNumberGenerator.GetInt32(1, SortedCells.Count);
        int col2 = col1 + 1;

        // These are the List<Cell> for each row
        var imageRow1 = SortedCells[row1 - 1];
        var imageRow2 = SortedCells[row2 - 1];

        // Add these cells to the hint.Cells list
        hint.Cells.Add(imageRow1[col1 - 1]);
        hint.Cells.Add(imageRow2[col2 - 1]);

        // This will return not null if we already have a hint
        // in the Hints collection of this type using the same
        // cells.
        var match = (from x in Hints
                     where x.Type == HintType.NextTo
                     && x.ContainsCells(hint.Cells)
                     select x).FirstOrDefault();

        if (match != null)
            // Another hint with these cells already exists. Bail
            return null;

        // Construct the hint text
        hint.HintText = $"The column with {hint.Cells[0].Name} " +
            $"is next to the column with {hint.Cells[1].Name}"; ;

        return hint;
    }

    /// <summary>
    /// One of four methods that generate clues.
    /// Between says "The column with X is between the column 
    /// with Y and the column with Z"
    /// </summary>
    /// <returns></returns>
    private Hint GenerateBetweenClue()
    {
        // Between Clue
        var hint = new Hint();
        hint.Type = HintType.Between;

        // pick 3 random rows (Y)
        var row1 = GetRandom();
        var row2 = GetRandom(new int[] { row1 });
        var row3 = GetRandom(new int[] { row1, row2 });

        // randomly pick 3 columns (X) next to each other
        var col1 = RandomNumberGenerator.GetInt32(1, SortedCells.Count - 1);
        var col2 = col1 + 1;
        var col3 = col1 + 2;

        // These are the List<Cell> for each row
        var imageRow1 = SortedCells[row1 - 1];
        var imageRow2 = SortedCells[row2 - 1];
        var imageRow3 = SortedCells[row3 - 1];

        // Add these cells to the hint.Cells list
        hint.Cells.Add(imageRow1[col2 - 1]);
        hint.Cells.Add(imageRow2[col1 - 1]);
        hint.Cells.Add(imageRow3[col3 - 1]);

        // This will return not null if we already have a hint
        // in the Hints collection of this type using the same
        // cells.
        var match = (from x in Hints
                     where x.Type == HintType.Between
                     && x.ContainsCells(hint.Cells)
                     select x).FirstOrDefault();

        if (match != null)
            // Another hint with these cells already exists. Bail
            return null;

        // Construct the hint text
        hint.HintText = $"The column with {hint.Cells[0].Name} " +
            $"is between the column with {hint.Cells[1].Name} " +
            $"and the column with {hint.Cells[2].Name}"; ;

        return hint;
    }

    /// <summary>
    /// One of four methods that generate clues.
    /// ThisNotThat is a clue that says "X is either in the 
    /// same column as Y or Z but not both."
    /// </summary>
    /// <returns></returns>
    private Hint GenerateThisNotThatClue()
    {
        // This but not that clue
        var hint = new Hint();
        hint.Type = HintType.ThisNotThat;

        // pick 3 random rows (Y)
        var row1 = GetRandom();
        var row2 = GetRandom(new int[] { row1 });
        var row3 = GetRandom(new int[] { row1, row2 });

        // pick 2 random columns (X)
        var col1 = GetRandom();
        var col2 = GetRandom(new int[] { col1 });

        // These are the List<Cell> for each row
        var imageRow1 = SortedCells[row1 - 1];
        var imageRow2 = SortedCells[row2 - 1];
        var imageRow3 = SortedCells[row3 - 1];

        // Because I need to randomize the list of 
        // cells (otherwise the first cell would
        // always be the solution) I create a temporary
        // list, whcih is then randomized into shortList.
        var tempList = new List<Cell>();
        tempList.Add(imageRow2[col1 - 1]);
        tempList.Add(imageRow3[col2 - 1]);
        var rnd = new Random(tempList.GetHashCode());
        var shortList = tempList.OrderBy(item => rnd.Next()).ToList();

        // Add these cells to the hint.Cells list
        hint.Cells.Add(imageRow1[col1 - 1]);
        hint.Cells.Add(shortList.First());
        hint.Cells.Add(shortList.Last());

        // This will return not null if we already have a hint
        // in the Hints collection of this type using the same
        // cells.
        var match = (from x in Hints
                     where x.Type == HintType.ThisNotThat
                     && x.ContainsCells(hint.Cells)
                     select x).FirstOrDefault();

        if (match != null)
            // Another hint with these cells already exists. Bail
            return null;

        // Construct the hint text
        var firstItem = hint.Cells[0].Name;
        // Capitalize the first cell name because it starts the sentence.
        firstItem = firstItem.Substring(0, 1).ToUpper() + firstItem.Substring(1);
        // Put it together
        hint.HintText = $"{firstItem} " +
            $"is EITHER in the same column as {shortList.First().Name} " +
            $"or  {shortList.Last().Name} " +
            $"but not BOTH";

        return hint;
    }

    /// <summary>
    /// returns a random int, excluding any ints that you pass in
    /// </summary>
    /// <param name="notThese">a list of ints (or null) 
    /// that we should not return</param>
    /// <returns></returns>
    protected int GetRandom(int[] notThese = null)
    {
        // Get a random number with 1 as the lower bound and
        // SortedCells count (4) as the upper bound
        int result = RandomNumberGenerator.GetInt32(1, SortedCells.Count + 1);

        if (notThese == null)
            // No constraints. Return the number
            return result;

        // Keep generating until we get a number not in the specified array
        while (notThese.Contains(result))
            result = RandomNumberGenerator.GetInt32(1, SortedCells.Count + 1);

        // Done!
        return result;
    }

    /// <summary>
    /// Checks to make sure the state of the cells is correct
    /// </summary>
    /// <returns></returns>
    private async Task UpdateGameState()
    {
        foreach (var cellsList in SortedCells)
        {
            // get the row index
            var rowIndex = SortedCells.IndexOf(cellsList);

            // get all the solved cells in this row
            var solved = (from x in cellsList
                          where x.IsSolved() == true
                          select x).ToList();

            // get all the unsolved cells in this row
            var notSolved = (from x in cellsList
                             where x.IsSolved() == false
                             select x).ToList();

            // Remove the solved image from all the unsolved
            // cells in this row
            foreach (var solvedCell in solved)
            {
                // get the original image index (0, 1, 2, or 3)
                var index = solvedCell.Index;

                foreach (var notSolvedCell in notSolved)
                {
                    notSolvedCell.Possibilities[index] = false;
                }
            }

            while (true)
            {
                bool changed = false;

                // Check again
                notSolved = (from x in cellsList where x.IsSolved() == false select x).ToList();
                if (notSolved.Count == 1)
                {
                    // only one cell left unsolved
                    notSolved.First().Solve();
                    changed = true;
                }
                else
                {
                    // find any cell with the only possibility
                    var count0 = (from x in notSolved where x.Possibilities[0] == true select x).ToList();
                    if (count0.Count == 1)
                    {
                        count0.First().Solve();
                        changed = true;
                    }

                    var count1 = (from x in notSolved where x.Possibilities[1] == true select x).ToList();
                    if (count1.Count == 1)
                    {
                        count1.First().Solve();
                        changed = true;
                    }

                    var count2 = (from x in notSolved where x.Possibilities[2] == true select x).ToList();
                    if (count2.Count == 1)
                    {
                        count2.First().Solve();
                        changed = true;
                    }

                    var count3 = (from x in notSolved where x.Possibilities[3] == true select x).ToList();
                    if (count3.Count == 1)
                    {
                        count3.First().Solve();
                        changed = true;
                    }
                }
                if (!changed) break;
            }
        }

        // Is the puzzle solved?
        if (IsPuzleSolved())
        {
            // We're done!
            Message = "Congratulations!! You did it!";
            CanvasDisplay = "block";
            ImageHint = "Refresh the browser to play again.";
            await jSRuntime.InvokeVoidAsync("ExplodeConfetti");
        }
    }

    /// <summary>
    /// Returns true if the puzzle has been solved
    /// </summary>
    /// <returns></returns>
    private bool IsPuzleSolved()
    {
        foreach (var imageRow in SortedCells)
        {
            var notSolved = (from x in imageRow 
                             where x.IsSolved() == false 
                             select x).ToList();
            if (notSolved.Count != 0)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Generate all the hints
    /// </summary>
    protected void GenerateHints()
    {
        while (true)
        {
            var hint1 = GenerateSameColumnClue();
            if (hint1 != null) { Hints.Add(hint1); }
            if (Hints.Count == 20) break;

            var hint2 = GenerateNextToClue();
            if (hint2 != null) { Hints.Add(hint2); }
            if (Hints.Count == 20) break;

            var hint3 = GenerateBetweenClue();
            if (hint3 != null) { Hints.Add(hint3); }
            if (Hints.Count == 20) break;

            var hint4 = GenerateThisNotThatClue();
            if (hint4 != null) { Hints.Add(hint4); }
            if (Hints.Count == 20) break;

            if (hint1 == null && hint2 == null && hint3 == null && hint4 == null)
                break;
        }
        var rnd = new Random(Hints.GetHashCode());
        Hints = Hints.OrderBy(item => rnd.Next()).ToList();
    }

    /// <summary>
    /// Randomize the cells
    /// </summary>
    /// <returns></returns>
    protected async Task ShuffleCells()
    {
        // Copy Cells
        SortedCells = Cells.ToArray().ToList();

        // objects to use and random seeds
        var obj1 = new object();
        var obj2 = new object();
        var obj3 = new object();
        var obj4 = new object();

        // shuffle
        var rnd = new Random(obj1.GetHashCode());
        SortedCells[0] = SortedCells[0].OrderBy(item => rnd.Next()).ToList();
        rnd = new Random(obj2.GetHashCode());
        SortedCells[1] = SortedCells[1].OrderBy(item => rnd.Next()).ToList();
        rnd = new Random(obj3.GetHashCode());
        SortedCells[2] = SortedCells[2].OrderBy(item => rnd.Next()).ToList();
        rnd = new Random(obj4.GetHashCode());
        SortedCells[3] = SortedCells[3].OrderBy(item => rnd.Next()).ToList();

        // pick 3 random cells to reveal by x and y
        var x1 = GetRandom();
        var x2 = GetRandom(new int[] { x1 });
        var x3 = GetRandom(new int[] { x1, x2 });
        var y1 = GetRandom();
        var y2 = GetRandom(new int[] { y1 });
        var y3 = GetRandom(new int[] { y1, y2 });

        // pull out lists of cells by the row
        var imageRow1 = SortedCells[y1 - 1];
        var imageRow2 = SortedCells[y2 - 1];
        var imageRow3 = SortedCells[y3 - 1];

        // solve the cells
        imageRow1[x1 - 1].Solve();
        imageRow2[x2 - 1].Solve();
        imageRow3[x3 - 1].Solve();

        // Make sure the cells are accurate
        await UpdateGameState();

        // Generate Hints
        GenerateHints();
    }

    /// <summary>
    /// After the first render, set focus to the hintFilter <input> tag
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await jSRuntime.InvokeVoidAsync("SetFocus", "hintFilter");
        }
    }

    protected override async Task OnInitializedAsync()
    {
        // Create the cells.
        // Cells is a list of a list of cells.
        // The outside list is rows.
        // The inside list is columns.

        var row1 = new List<Cell>
        {
            new Cell("Carl", "carl.jpg", 0),
            new Cell("Scott", "scott.jpg", 1),
            new Cell("Richard", "richard.jpg", 2),
            new Cell("Patrick", "patrick.jpg", 3)
        };
        Cells.Add(row1);

        var row2 = new List<Cell>
        {
            new Cell("the wine", "wine.jpg", 0),
            new Cell("the beer", "beer.jpg", 1),
            new Cell("the coffee", "coffee.jpg", 2),
            new Cell("the tea", "tea.jpg", 3)
        };
        Cells.Add(row2);

        var row3 = new List<Cell>
        {
            new Cell("the puppy", "puppy.jpg", 0),
            new Cell("the kitten", "kitten.jpg", 1),
            new Cell("the monkey", "monkey.jpg", 2),
            new Cell("the bear", "bear.jpg", 3)
        };
        Cells.Add(row3);

        var row4 = new List<Cell>
        {
            new Cell("the burger", "burger.jpg", 0),
            new Cell("the taco", "taco.jpg", 1),
            new Cell("the pizza", "pizza.jpg", 2),
            new Cell("the sushi", "sushi.jpg", 3)
        };
        Cells.Add(row4);

        // Randomize
        await ShuffleCells();
    }
}
