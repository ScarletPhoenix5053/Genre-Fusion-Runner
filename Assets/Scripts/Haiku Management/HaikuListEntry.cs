using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HaikuListEntry : MonoBehaviour
{
    [SerializeField] private Text haikuName;
    [SerializeField] private Text[] kanaLines;
    [SerializeField] private Text roumaji;
    [SerializeField] private Text english;

    private Haiku haiku;

    public void Initialize(Haiku haiku)
    {
        this.haiku = haiku;

        var printout = haiku.ToString().Split('\n');

        var kana = printout[1].Split('/');
        for (int i = 0; i < 3; i++)
        {
            kanaLines[i].text = kana[i];
        }

        roumaji.text = printout[2];
        english.text = printout[3];
    }
    public void LoadMyHaikuLevel()
    {
        SessionManager.Instance.BeginStageChange(haiku);
        SessionManager.Instance.ClosePauseMenu();
    }
}