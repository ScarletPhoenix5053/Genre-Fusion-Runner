using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaikuList : MonoBehaviour
{
    [SerializeField] private HaikuDatabase haikuDatabase;
    [SerializeField] private Transform listContent;
    [Header("Prefab")]
    [SerializeField] private GameObject haikuListEntryPrefab;

    private void Start()
    {
        haikuDatabase.GenerateHaiku();
        for (int i = 0; i < haikuDatabase.Haiku.Count; i++)
        {
            var newEntry = Instantiate(haikuListEntryPrefab, listContent);

            newEntry.GetComponent<HaikuListEntry>().Initialize(haikuDatabase.Haiku[i]);
        }
    }
}
