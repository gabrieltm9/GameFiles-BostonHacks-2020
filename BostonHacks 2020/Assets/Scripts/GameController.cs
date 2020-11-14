using GoogleSheetsToUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [HideInInspector]
    public string associatedSheet = "1r7oUGjUpapdhlDBwxJnwajmHPzfIwgjMKl4rvHHPgLc";
    [HideInInspector]
    public string associatedWorksheet = "Data";

    // Start is called before the first frame update
    void Start()
    {
        SpreadsheetManager.Read(new GSTU_Search(associatedSheet, associatedWorksheet), UpdateData);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateData(GstuSpreadSheet ss)
    {
        Debug.Log(ss["B2"].value);
    }
}