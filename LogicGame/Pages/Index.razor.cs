﻿using LogicGame.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Security.Cryptography;

namespace LogicGame.Pages;

public partial class Index : ComponentBase
{
    protected string Message { get; set; } = "Carl Franklin's Logic Puzzle";
    protected string ImageHint { get; set; } = "Hover over an image to see a description";
    protected List<List<Cell>> SortedCells { get; set; } = new List<List<Cell>>();
    protected List<List<Cell>> Cells { get; set; } = new List<List<Cell>>();
    protected List<Hint> Hints { get; set; } = new List<Hint>();
    protected List<Hint> FilteredHints => Hints.Where(i => i.HintText.ToLower().Contains(HintFilter.ToLower())).ToList();

    protected string BigCellWidth = "100px";
    protected string SmallCellWidth = "50px";
    protected string HintFilter = string.Empty;
    protected string CanvasDisplay { get; set; } = "none";

    [CascadingParameter]
    public IModalService Modal { get; set; } = default!;

    [Inject]
    public IJSRuntime jSRuntime { get; set; }

    protected async Task ClearHintFilter()
    {
        HintFilter = "";
        await jSRuntime.InvokeVoidAsync("SetFocus", "hintFilter");
    }

    protected async Task ShowHelp()
    {
        ModalOptions options = new ModalOptions()
        {
            DisableBackgroundCancel = true,
            Size = ModalSize.Large,
            Position = ModalPosition.Middle
        };
        var modal = Modal.Show<HelpModal>("Logic Puzzle Help", options);
        var result = await modal.Result;
    }

    protected async Task OnCellChanged(Cell cell)
    {
        await UpdateGameState();
    }

    protected void OnImageHintChanged(string imageHint)
    {
        ImageHint = imageHint;
    }

    private Hint GenerateThisNotThatClue()
    {
        // This but not that clue
        var hint = new Hint();
        hint.Type = HintType.ThisNotThat;

        var row1 = GetRandom();
        var row2 = GetRandom(new int[] { row1 });
        var row3 = GetRandom(new int[] { row1, row2 });

        var col1 = GetRandom();
        var col2 = GetRandom(new int[] { col1 });

        var imageRow1 = SortedCells[row1 - 1];
        var imageRow2 = SortedCells[row2 - 1];
        var imageRow3 = SortedCells[row3 - 1];

        var tempList = new List<Cell>();
        tempList.Add(imageRow2[col1 - 1]);
        tempList.Add(imageRow3[col2 - 1]);
        var rnd = new Random(tempList.GetHashCode());
        var shortList = tempList.OrderBy(item => rnd.Next()).ToList();

        hint.Cells.Add(imageRow1[col1 - 1]);
        hint.Cells.Add(shortList.First());
        hint.Cells.Add(shortList.Last());

        var match = (from x in Hints
                     where x.Type == HintType.ThisNotThat
                     && x.ContainsCells(hint.Cells)
                     select x).FirstOrDefault();

        if (match != null) return null;

        var firstItem = hint.Cells[0].Name;

        firstItem = firstItem.Substring(0, 1).ToUpper() + firstItem.Substring(1);

        hint.HintText = $"{firstItem} " +
            $"is EITHER in the same column as {shortList.First().Name} " +
            $"or  {shortList.Last().Name} " +
            $"but not BOTH";

        return hint;
    }

    private Hint GenerateBetweenClue()
    {
        // Between Clue
        var hint = new Hint();
        hint.Type = HintType.Between;

        var row1 = GetRandom();
        var row2 = GetRandom(new int[] { row1 });
        var row3 = GetRandom(new int[] { row1, row2 });

        var col1 = RandomNumberGenerator.GetInt32(1, SortedCells.Count - 1);
        var col2 = col1 + 1;
        var col3 = col1 + 2;

        var imageRow1 = SortedCells[row1 - 1];
        var imageRow2 = SortedCells[row2 - 1];
        var imageRow3 = SortedCells[row3 - 1];

        hint.Cells.Add(imageRow1[col2 - 1]);
        hint.Cells.Add(imageRow2[col1 - 1]);
        hint.Cells.Add(imageRow3[col3 - 1]);

        var match = (from x in Hints
                     where x.Type == HintType.Between
                     && x.ContainsCells(hint.Cells)
                     select x).FirstOrDefault();

        if (match != null) return null;

        hint.HintText = $"The column with {hint.Cells[0].Name} " +
            $"is between the column with {hint.Cells[1].Name} " +
            $"and the column with {hint.Cells[2].Name}"; ;

        return hint;
    }

    private Hint GenerateNextToClue()
    {
        // Next-To Clue
        var hint = new Hint();
        hint.Type = HintType.NextTo;

        var row1 = GetRandom();
        var row2 = GetRandom(new int[] { row1 });

        var col1 = RandomNumberGenerator.GetInt32(1, SortedCells.Count);
        int col2 = col1 + 1;

        var imageRow1 = SortedCells[row1 - 1];
        var imageRow2 = SortedCells[row2 - 1];

        hint.Cells.Add(imageRow1[col1 - 1]);
        hint.Cells.Add(imageRow2[col2 - 1]);

        var match = (from x in Hints
                     where x.Type == HintType.NextTo
                     && x.ContainsCells(hint.Cells)
                     select x).FirstOrDefault();

        if (match != null) return null;

        hint.HintText = $"The column with {hint.Cells[0].Name} " +
            $"is next to the column with {hint.Cells[1].Name}"; ;

        return hint;
    }

    private Hint GenerateSameColumnClue()
    {
        // Same Column Clue
        var hint = new Hint();
        hint.Type = HintType.SameColumn;

        var row1 = GetRandom();
        var row2 = GetRandom(new int[] { row1 });

        var col1 = GetRandom();

        var imageRow1 = SortedCells[row1 - 1];
        var imageRow2 = SortedCells[row2 - 1];

        hint.Cells.Add(imageRow1[col1 - 1]);
        hint.Cells.Add(imageRow2[col1 - 1]);

        var match = (from x in Hints
                     where x.Type == HintType.SameColumn
                     && x.ContainsCells(hint.Cells)
                     select x).FirstOrDefault();

        if (match != null) return null;

        var firstItem = hint.Cells[0].Name;
        firstItem = firstItem.Substring(0, 1).ToUpper() + firstItem.Substring(1);

        hint.HintText = $"{firstItem} " +
            $"is in the same column as {hint.Cells[1].Name}"; ;

        return hint;
    }

    protected int GetRandom(int[] notThese = null)
    {
        int result = RandomNumberGenerator.GetInt32(1, SortedCells.Count + 1); ;
        if (notThese == null)
            return result;

        while (notThese.Contains(result))
            result = RandomNumberGenerator.GetInt32(1, SortedCells.Count + 1);

        return result;
    }

    private async Task UpdateGameState()
    {
        foreach (var cellsList in SortedCells)
        {
            // get the row index
            var rowIndex = SortedCells.IndexOf(cellsList);
            // get all the solved cells in this row
            var solved = (from x in cellsList where x.IsSolved() == true select x).ToList();
            // get all the unsolved cells in this row
            var notSolved = (from x in cellsList where x.IsSolved() == false select x).ToList();

            foreach (var solvedCell in solved)
            {
                // get the original image index (0, 1, 2, or 3)
                var index = solvedCell.Index;

                foreach (var notSolvedCell in notSolved)
                {
                    notSolvedCell.Possibilities[index] = false;
                }
            }

            // Check again
            notSolved = (from x in cellsList where x.IsSolved() == false select x).ToList();
            if (notSolved.Count == 1)
            {
                // only one cell left unsolved
                notSolved.First().Solve();
            }
            else
            {
                // find any cell with the only possibility
                var count0 = (from x in notSolved where x.Possibilities[0] == true select x).ToList();
                if (count0.Count == 1)
                {
                    count0.First().Solve();
                }

                var count1 = (from x in notSolved where x.Possibilities[1] == true select x).ToList();
                if (count1.Count == 1)
                {
                    count1.First().Solve();
                }

                var count2 = (from x in notSolved where x.Possibilities[2] == true select x).ToList();
                if (count2.Count == 1)
                {
                    count2.First().Solve();
                }

                var count3 = (from x in notSolved where x.Possibilities[3] == true select x).ToList();
                if (count3.Count == 1)
                {
                    count3.First().Solve();
                }
            }
        }

        if (IsPuzleSolved())
        {
            Message = "Congratulations!! You did it!";
            CanvasDisplay = "block";
            await jSRuntime.InvokeVoidAsync("ExplodeConfetti");
        }
    }

    private bool IsPuzleSolved()
    {
        foreach (var imageRow in SortedCells)
        {
            var notSolved = (from x in imageRow where x.IsSolved() == false select x).ToList();
            if (notSolved.Count != 0)
                return false;
        }
        return true;
    }

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

    protected async Task ShuffleCells()
    {
        // Copy Cells
        SortedCells = Cells.ToArray().ToList();

        var obj1 = new object();
        var obj2 = new object();
        var obj3 = new object();
        var obj4 = new object();
        var rnd = new Random(obj1.GetHashCode());
        SortedCells[0] = SortedCells[0].OrderBy(item => rnd.Next()).ToList();
        rnd = new Random(obj2.GetHashCode());
        SortedCells[1] = SortedCells[1].OrderBy(item => rnd.Next()).ToList();
        rnd = new Random(obj3.GetHashCode());
        SortedCells[2] = SortedCells[2].OrderBy(item => rnd.Next()).ToList();
        rnd = new Random(obj4.GetHashCode());
        SortedCells[3] = SortedCells[3].OrderBy(item => rnd.Next()).ToList();

        // pick 3 random cells to reveal
        var x1 = GetRandom();
        var x2 = GetRandom(new int[] { x1 });
        var x3 = GetRandom(new int[] { x1, x2 });

        var y1 = GetRandom();
        var y2 = GetRandom(new int[] { y1 });
        var y3 = GetRandom(new int[] { y1, y2 });

        var imageRow1 = SortedCells[y1 - 1];
        var imageRow2 = SortedCells[y2 - 1];
        var imageRow3 = SortedCells[y3 - 1];

        imageRow1[x1 - 1].Possibilities = new bool[] { false, false, false, false };
        imageRow2[x2 - 1].Possibilities = new bool[] { false, false, false, false };
        imageRow3[x3 - 1].Possibilities = new bool[] { false, false, false, false };

        await UpdateGameState();

        // Generate Hints
        GenerateHints();
    }

    protected override async Task OnAfterRenderAsync (bool firstRender)
    {
        if (firstRender)
        {
            await jSRuntime.InvokeVoidAsync("SetFocus", "hintFilter");
        }
    }

    protected override async Task OnInitializedAsync()
    {
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

        await ShuffleCells();
    }
}
