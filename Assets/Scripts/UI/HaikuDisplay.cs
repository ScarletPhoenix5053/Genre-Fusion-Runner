using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaikuDisplay : MonoBehaviour
{
    [SerializeField] private Transform inactiveKanaSquares;
    [SerializeField] private List<Transform> kanaLineParents = new List<Transform>(3);
    [SerializeField] private KanaSquare kanaSquareObj;
    
    private KanaSquare[][] kanaSquares;

    public void SetKanaSquare(int squareIndex, Kana to)
    {
        throw new System.NotImplementedException();
        //kanaSquareParent.GetChild(squareIndex).GetComponent<KanaSquare>().SetKana(to);
    } 
    public void SetKanaSquare(int lineIndex, int squareIndex, Kana to)
    {
        var newKana = to;
        /*
        Debug.Assert(kanaSquares != null);  
        Debug.Assert(kanaSquares[lineIndex] != null);
        Debug.Assert(kanaSquares[lineIndex][squareIndex] != null);
        */
        kanaSquares[lineIndex][squareIndex].SetKana(to: newKana);
        Debug.Log("Setting value of: ", kanaSquares[lineIndex][squareIndex]);
    }


    private const int lineCount = 3;
    public void ResetDisplay()
    {
        Debug.Log("Resetting");
        foreach (KanaSquare[] kanaSquareArray in kanaSquares)
        {
            foreach (KanaSquare kanaSquare in kanaSquareArray)
            {
                kanaSquare.ResetKana();
            }
        }
    }
    public void InitDisplay(Haiku haiku)
    {
        int currentKana = 0;
        kanaSquares = new KanaSquare[lineCount][];
        for (int l = 0; l < lineCount; l++)
        {
            kanaSquares[l] = new KanaSquare[haiku.KanaCount(l+1)];
            for (int k = 0; k < haiku.KanaCount(l+1); k++)
            {
                var newKanaGo = inactiveKanaSquares.GetChild(currentKana).gameObject;
                newKanaGo.SetActive(true);
                newKanaGo.transform.SetParent(kanaLineParents[l]);
                newKanaGo.name = "KanaSquare (" + l + "," + k + ")";
                kanaSquares[l][k] = newKanaGo.GetComponent<KanaSquare>();
                currentKana++;
            }
        }
    }

    private void Awake()
    {
        InitKanaSquares();
    }

    private const int kanaSquaresPerLine = 8;
    private void InitKanaSquares()
    {
        if (kanaLineParents.Count != lineCount) throw new LineOutOfRangeException("Must have exactly threee transforms in kanaLineParents array.");
        for (int l = 0; l < lineCount; l++)
        {
            for (int k = 0; k < kanaSquaresPerLine; k++)
            {
                var newKanaSquare = Instantiate(kanaSquareObj, inactiveKanaSquares);
                newKanaSquare.gameObject.SetActive(false);
                newKanaSquare.transform.SetParent(inactiveKanaSquares);
            }
        }
    }
}
