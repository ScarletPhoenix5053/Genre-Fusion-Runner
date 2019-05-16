using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaikuDisplay : MonoBehaviour
{
    [SerializeField] private Transform inactiveKanaSquares;
    [SerializeField] private List<Transform> kanaLineParents = new List<Transform>(3);
    [SerializeField] private KanaSquare kanaSquareObj;
    
    private KanaSquare[][] kanaSquares;
    private Dictionary<Kana, Vector2Int> kanaPosition = new Dictionary<Kana, Vector2Int>();

    public void ActivateKanaSquare(Kana kana)
    {
        var squarePos = kanaPosition[kana];
        kanaSquares[squarePos.x][squarePos.y].SetKana(to: kana);
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
                kanaPosition.Add(allKana[currentKana], new Vector2Int(l, k));

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
