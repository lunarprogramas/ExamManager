using System;
using System.Diagnostics;

public class ExamRow
{
    public int Index { get; set; }
    public int Students { get; set; }
}

public class ExamHall
{
    public int MaxStudentsPerRow { get; set; }
    public ExamRow RowA { get; set; }
    public ExamRow RowB { get; set; }
    public ExamRow RowC { get; set; }
    public ExamRow RowD { get; set; }
    public ExamRow RowE { get; set; }
    public ExamRow RowF { get; set; }
}

public class ExamHallManager
{
    private ExamHall hall = new ExamHall
    {
        MaxStudentsPerRow = 6,
        RowA = new ExamRow { Index = 1, Students = 0 },
        RowB = new ExamRow { Index = 2, Students = 0 },
        RowC = new ExamRow { Index = 3, Students = 0 },
        RowD = new ExamRow { Index = 4, Students = 0 },
        RowE = new ExamRow { Index = 5, Students = 0 },
        RowF = new ExamRow { Index = 6, Students = 0 }
    };

    public ExamHall ReturnSetupExamHall()
    {
        return hall;
    }

    public object DetermineCandidateSeat()
    {
        // Try to assign a student to the first available row with space
        var rows = new[] { hall.RowA, hall.RowB, hall.RowC, hall.RowD, hall.RowE, hall.RowF };
        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i].Students < hall.MaxStudentsPerRow)
            {
                rows[i].Students++;
                Debug.WriteLine($"Assigned to Row {ConvertIndexToLetter(rows[i].Index)}, Seat #{rows[i].Students}");
                return ( RowString: ConvertIndexToLetter(rows[i].Index), RowInt: rows[i].Index, Seat: rows[i].Students );
            }
        }

        Console.WriteLine("All rows are full.");
        return null;
    }

    public void ResetExamHall()
    {
        hall.RowA.Students = 0;
        hall.RowB.Students = 0;
        hall.RowC.Students = 0;
        hall.RowD.Students = 0;
        hall.RowE.Students = 0;
        hall.RowF.Students = 0;
    }

    private string ConvertIndexToLetter(int index)
    {
        // Assumes index 1 = A, 2 = B, etc.
        return ((char)('A' + index - 1)).ToString();
    }
}
