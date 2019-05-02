using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Haiku
{
    private readonly HaikuLine[] lines;
    private const int lineCount = 3;
    private void ValidateLineIndex(int line)
    {
        if (line <= 0 || line > 3) throw new LineOutOfRangeException();
    }

    public List<Effect> Effects { get; }

    // CONSTRUCTOR
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