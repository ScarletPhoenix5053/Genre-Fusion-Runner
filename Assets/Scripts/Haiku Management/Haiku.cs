using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class Haiku
{
    private readonly HaikuLine[] lines;
    private const int lineCount = 3;
    private const int expectedCsvLines = 4;
    private void ValidateLineIndex(int line)
    {
        if (line <= 0 || line > 3) throw new LineOutOfRangeException();
    }

    public List<Effect> Effects { get; }

    // CONSTRUCTOR
    public Haiku(string csvPath)
    {
        // init stream reader
        var reader = new StreamReader(csvPath);
        var currentLine = "";
        var csvLines = new List<string>();

        // read csv file
        do
        {
            currentLine = reader.ReadLine();
            if (currentLine != null) csvLines.Add(currentLine);
        }
        while (currentLine != null);
        reader.Close();          
        

        // parse into haiku object
        if (csvLines.Count < expectedCsvLines)
        {
            throw new InvalidCsvException("Csv file only contains " + csvLines.Count + " lines." +
                "Expected: " + expectedCsvLines + " lines.");
        }

        var haikuName = csvLines[0];
        var haikuKana = csvLines[1].Split(',');
        var haikuSound = csvLines[2].Split(',');
        var haikuMeaning = csvLines[3].Split(',');
        lines = new HaikuLine[3];
        
        for (int lineN = 0; lineN < haikuKana.Length; lineN++)
        {
            var currentCharLine = haikuKana[lineN].ToCharArray();
            var kanaThisLine = new List<Kana>();
            for (int charN = 0; charN < currentCharLine.Length; charN++)
            {
                var newKana = new Kana(currentCharLine[charN]);
                kanaThisLine.Add(newKana);
            }
            Debug.Assert(haikuKana != null);
            Debug.Assert(haikuMeaning != null);
            lines[lineN] = new HaikuLine(kanaThisLine.ToArray(), haikuMeaning[lineN]);
            lines[lineN].SetLineNumber(to: lineN);
        }
    }
    public Haiku(HaikuLine[] lines, Effect[] effects)
    {
        if (lines.Length != lineCount) throw new LineOutOfRangeException("Haiku must always have 3 lines");
        this.lines = lines;

        Effects = effects == null ?  new List<Effect>() : new List<Effect>(effects);

        for (int l = 0; l < this.lines.Length; l++)
        {
            HaikuLine currentLine = this.lines[l];
            currentLine.SetLineNumber(to: l);
        }
    }
    public Haiku(HaikuLine[] lines) : this(lines, null) { }

    #region Methods
    // KANA
    public int KanaCount()
    {
        var kanaCount = 0;
        foreach (HaikuLine line in lines) { kanaCount += line.KanaCount; }
        return kanaCount;
    }
    public int KanaCount(int line)
    {
        ValidateLineIndex(line);
        return lines[line-1].KanaCount;
    }
    public Kana[] ToKana()
    {
        var haiku = new List<Kana>();
        for (int l = 0; l < lineCount; l++)
        {
            haiku.AddRange(lines[l].ToKana());
        }
        return haiku.ToArray();
    }
    public Kana[] LineToKana(int line)
    {
        ValidateLineIndex(line);
        return lines[line-1].ToKana();
    }
    // STRING
    public override string ToString()
    {
        var haikuString = "";
        for (int i = 0; i < lineCount; i++)
        {
            haikuString += lines[i].ToString();
            if (i != 3) haikuString += ", ";
        }
        return haikuString;
    }
    public string LineToString(int line)
    {
        ValidateLineIndex(line);
        return lines[line-1].ToString();
    }
    // MEANING
    public string ToEnglish()
    {
        var meaning = "";
        for (int i = 0; i < lineCount; i++)
        {
            meaning += lines[i].Meaning;
            if (i != 3) meaning += ", ";
        }
        return meaning;
    }
    public string LineToEnglish(int line)
    {
        ValidateLineIndex(line);
        return lines[line-1].Meaning;
    }
    #endregion
}
public class HaikuLine
{
    private readonly List<Kana> Kana;

    public HaikuLine(Kana[] kana, string meaning)
    {
        Kana = new List<Kana>(kana);
        Meaning = meaning;

        for (int k = 0; k < KanaCount; k++)
        {
            var currentKana = Kana[k];
            currentKana.SetPosInLine(to: k);
        }
    }
    public HaikuLine(Kana[] kana) : this(kana, "") { }

    // KANA
    public int KanaCount => Kana.Count;
    public Kana[] ToKana() => Kana.ToArray();
    public void SetLineNumber(int to)
    {
        var lineNum = to;
        for (int k = 0; k < KanaCount; k++)
        {
            var currentKana = Kana[k];
            currentKana.SetLine(to: lineNum);
        }
    }
    // STRING
    public override string ToString()
    {
        var lineString = "";
        foreach (Kana kana in Kana)
        {
            lineString += kana.ToString() + " ";
        }
        return lineString;
    }
    // MEANING
    public string Meaning { get; }
}
public class Kana
{
    public readonly char Character;
    public readonly string Sound;
    public readonly string Meaning;
    public int Line;
    public int PosInLine;

    public Kana(char character, string sound, string meaning, Vector2Int pos)
    {
        Character = character;
        Sound = sound;
        Meaning = meaning;
        Line = pos.x;
        PosInLine = pos.y;
    }
    public Kana(char character, string sound, string meaning) : this(character, sound, meaning, Vector2Int.zero) { }
    public Kana(char character, string sound) : this(character, sound, "") { }
    public Kana(char character) : this(character, "", "") { }

    public override string ToString() => Character.ToString();

    public void SetLine(int to)
    {
        Line = to;
    }
    public void SetPosInLine(int to)
    {
        PosInLine = to;
    }
    public void SetPos(int line, int posInLine)
    {
        SetLine(to: line);
        SetPosInLine(to: posInLine);
    }
    public void SetPos(Vector2Int newPos)
    {
        SetPos(
            line: newPos.x,
            posInLine: newPos.y
            );
    }
}