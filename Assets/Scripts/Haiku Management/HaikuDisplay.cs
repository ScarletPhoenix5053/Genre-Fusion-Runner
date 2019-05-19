using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaikuDisplay : MonoBehaviour
{
    [SerializeField] private Transform inactiveKanaSquares;
    [SerializeField] private List<Transform> kanaLineParents = new List<Transform>(3);
    [SerializeField] private KanaSquare kanaSquareObj;
    
    private KanaSquare[][] kanaSquares;
    private Dictionary<Vector2Int, Kana> kanaPosition = new Dictionary<Vector2Int, Kana>();

    public void ActivateKanaSquare(Kana kana)
    {
        foreach (KeyValuePair<Vector2Int, Kana> kanaPos in kanaPosition)
        {
            if (kana == kanaPos.Value)
            {
                var squarePos = kanaPos.Key;
                kanaSquares[squarePos.x][squarePos.y].SetKana(to: kana);
                kanaPosition.Remove(kanaPos.Key);
                break;
            }
        }
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
        // Clear out lines if they contain anything
        if (kanaSquares != null)
        {
            for (int a = 0; a < kanaSquares.Length; a++)
            {
                for (int b = 0; b < kanaSquares[a].Length; b++)
                {
                    var go = kanaSquares[a][b].gameObject;
                    go.transform.SetParent(inactiveKanaSquares);
                    go.name = "Kana Square (inactive)";
                    go.SetActive(false);
                }
            }
        }
        kanaSquares = new KanaSquare[lineCount][];

        // Reset dictionary
        kanaPosition = new Dictionary<Vector2Int, Kana>();

        int currentKana = 0;
        var allKana = haiku.ToKana();
        for (int l = 0; l < lineCount; l++) 
        {
            kanaSquares[l] = new KanaSquare[haiku.Lines[l].Length];
            for (int k = 0; k < haiku.Lines[l].Length; k++)
            {
                // Initialize kana square
                var newKanaGo = inactiveKanaSquares.GetChild(currentKana).gameObject;
                newKanaGo.SetActive(true);
                newKanaGo.transform.SetParent(kanaLineParents[l]);
                newKanaGo.name = "KanaSquare (" + l + "," + k + ")";

                // Store kana square info
                kanaSquares[l][k] = newKanaGo.GetComponent<KanaSquare>();
                kanaPosition.Add(new Vector2Int(l, k), allKana[currentKana]);

                currentKana++;
            }
        }
    }

    private void Awake()
    {
        InitKanaSquares();
    }

    private const int kanaSquaresPerLine = 12;
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
