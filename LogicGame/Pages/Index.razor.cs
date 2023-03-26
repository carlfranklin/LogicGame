using Microsoft.AspNetCore.Components;
using System.Security.Cryptography;

namespace LogicGame.Pages;

public partial class Index : ComponentBase
{
    public bool ShowCompleteGrid { get; set; } = true;
    public string Message { get; set; } = "";

    public List<List<Cell>> ImageNames { get; set; } = new List<List<Cell>>();

    public void ShowImageName(string imageName)
    {
        Message = imageName;
    }

    public void GenerateThisNotThatClue()
    {
        // This but not that clue

        var row1 = GetRandom();
        var row2 = GetRandom(new int[] { row1 });
        var row3 = GetRandom(new int[] { row1, row2 });

        var col1 = GetRandom();
        var col2 = GetRandom(new int[] { col1 });

        var imageRow1 = ImageNames[row1 - 1];
        var imageRow2 = ImageNames[row2 - 1];
        var imageRow3 = ImageNames[row3 - 1];

        var tempList = new List<Cell>();
        tempList.Add(imageRow2[col1 - 1]);
        tempList.Add(imageRow3[col2 - 1]);
        var rnd = new Random(tempList.GetHashCode());
        var shortList = tempList.OrderBy(item => rnd.Next()).ToList();

        var firstItem = imageRow1[col1 - 1].Name;
        firstItem = firstItem.Substring(0, 1).ToUpper() + firstItem.Substring(1);

        Message = $"{firstItem} " +
            $"is EITHER in the same column as {shortList.First().Name} " +
            $"or  {shortList.Last().Name} " +
            $"but not BOTH";
    }

    public void GenerateBetweenClue()
    {
        // Between Clue

        var row1 = GetRandom();
        var row2 = GetRandom(new int[] { row1 });
        var row3 = GetRandom(new int[] { row1, row2 });

        var col1 = RandomNumberGenerator.GetInt32(1, ImageNames.Count - 1);
        var col2 = col1 + 1;
        var col3 = col1 + 2;

        var imageRow1 = ImageNames[row1 - 1];
        var imageRow2 = ImageNames[row2 - 1];
        var imageRow3 = ImageNames[row3 - 1];

        Message = $"The column with {imageRow1[col2 - 1].Name} " +
            $"is between the column with {imageRow2[col1 - 1].Name} " +
            $"and the column with {imageRow3[col3 - 1].Name}";

    }

    public void GenerateNextToClue()
    {
        // Next-To Clue
        var row1 = GetRandom();
        var row2 = GetRandom(new int[] { row1 });

        var col1 = RandomNumberGenerator.GetInt32(1, ImageNames.Count);
        int col2 = col1 + 1;
        
        var imageRow1 = ImageNames[row1 - 1];
        var imageRow2 = ImageNames[row2 - 1];

        Message = $"The column with {imageRow1[col1 - 1].Name} " +
            $"is next to the column with {imageRow2[col2 - 1].Name}";

    }

    public void GenerateSameColumnClue()
    {
        // Same Column Clue
        var row1 = GetRandom();
        var row2 = GetRandom(new int[] { row1 });

        var col1 = GetRandom();

        var imageRow1 = ImageNames[row1 - 1];
        var imageRow2 = ImageNames[row2 - 1];

        var firstItem = imageRow1[col1 - 1].Name;
        firstItem = firstItem.Substring(0, 1).ToUpper() + firstItem.Substring(1);

        Message = $"{firstItem} " +
            $"is in the same column as {imageRow2[col1 - 1].Name}";

    }

    public int GetRandom(int[] notThese = null)
    {
        int result = RandomNumberGenerator.GetInt32(1, ImageNames.Count + 1); ;
        if (notThese == null)
            return result;

        while (notThese.Contains(result))
            result = RandomNumberGenerator.GetInt32(1, ImageNames.Count + 1);

        return result;
    }

    public void ShuffleImages()
    {
        var obj1 = new object();
        var obj2 = new object();
        var obj3 = new object();
        var obj4 = new object();
        var rnd = new Random(obj1.GetHashCode());
        ImageNames[0] = ImageNames[0].OrderBy(item => rnd.Next()).ToList();
        rnd = new Random(obj2.GetHashCode());
        ImageNames[1] = ImageNames[1].OrderBy(item => rnd.Next()).ToList();
        rnd = new Random(obj3.GetHashCode());
        ImageNames[2] = ImageNames[2].OrderBy(item => rnd.Next()).ToList();
        rnd = new Random(obj4.GetHashCode());
        ImageNames[3] = ImageNames[3].OrderBy(item => rnd.Next()).ToList();
    }

    protected override void OnInitialized()
    {
        var row1 = new List<Cell>();
        row1.Add(new Cell("the lady", "person1.png"));
        row1.Add(new Cell("the clown", "person2.png"));
        row1.Add(new Cell("the baby", "person3.png"));
        row1.Add(new Cell("the goober guy", "person4.png"));
        ImageNames.Add(row1);

        var row2 = new List<Cell>();
        row2.Add(new Cell("the red house", "house1.png"));
        row2.Add(new Cell("the yellow house", "house2.png"));
        row2.Add(new Cell("the white house", "house3.png"));
        row2.Add(new Cell("the blue house", "house4.png"));
        ImageNames.Add(row2);

        var row3 = new List<Cell>();
        row3.Add(new Cell("the stop sign", "sign1.png"));
        row3.Add(new Cell("the RR sign", "sign2.png"));
        row3.Add(new Cell("the speed limit sign", "sign3.png"));
        row3.Add(new Cell("the hospital sign", "sign4.png"));
        ImageNames.Add(row3);

        var row4 = new List<Cell>();
        row4.Add(new Cell("the Israeli flag", "flag1.png"));
        row4.Add(new Cell("the UK flag", "flag2.png"));
        row4.Add(new Cell("the Japanese flag", "flag3.png"));
        row4.Add(new Cell("the German flag", "flag4.png"));
        ImageNames.Add(row4);


        ShuffleImages();
    }
}
