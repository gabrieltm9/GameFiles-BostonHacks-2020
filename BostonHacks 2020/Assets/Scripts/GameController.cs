using GoogleSheetsToUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    [HideInInspector]
    public string associatedSheet = "1r7oUGjUpapdhlDBwxJnwajmHPzfIwgjMKl4rvHHPgLc";
    [HideInInspector]
    public string associatedWorksheet = "Data";

    public FirebaseManager fbm;

    public GameObject player;
    public GameObject world;
    public Camera menuCamera;

    public bool gameStarted;
    public bool refreshSpreadsheet = true;

    //Data
    public GstuSpreadSheet spreadsheet;
    public List<UserData> data;
    public int currentUserDataIndex;

    //UI
    public GameObject loginUI;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.X))
        {
            data[currentUserDataIndex].totalScore += 5;
            UpdateUserScore(4, data[currentUserDataIndex].totalScore);
        }
    }

    void GetSpreadsheetData()
    {
        Debug.Log("Getting spreadsheet data...");
        SpreadsheetManager.Read(new GSTU_Search(associatedSheet, associatedWorksheet), ReceiveData);
    }

    IEnumerator GetSpreadsheetDataTimer()
    {
        yield return new WaitForSeconds(5);
        if(refreshSpreadsheet)
        {
            GetSpreadsheetData();
            StartCoroutine(GetSpreadsheetDataTimer());
        }
    }

    void ReceiveData(GstuSpreadSheet ss)
    {
        spreadsheet = ss;
        data = new List<UserData>();

        for(int i = 2; i <= ss.rows.primaryDictionary.Count; i++)
        {
            UserData userData = new UserData();
            userData.id = ss.rows[i][0].value;
            userData.email = ss.rows[i][1].value;
            userData.name = ss.rows[i][2].value;
            userData.energyScore = int.Parse(ss.rows[i][3].value);
            userData.waterScore = int.Parse(ss.rows[i][4].value);
            userData.wasteScore = int.Parse(ss.rows[i][5].value);
            userData.totalScore = int.Parse(ss.rows[i][6].value);

            data.Add(userData);

            if(userData.id == fbm.user.UserId)
                currentUserDataIndex = data.Count - 1;
        }
    }

    public void UpdateUserScore(int scoreType, int newScore) //Score types: 1 = energy, 2 = water, 3 = waste, 4 = total
    {
        spreadsheet.rows[fbm.user.UserId][scoreType + 2].UpdateCellValue(associatedSheet, associatedWorksheet, "" + newScore);
    }

    public void StartGame()
    {
        if (gameStarted)
            return;

        gameStarted = true;

        GetSpreadsheetData();
        StartCoroutine(GetSpreadsheetDataTimer());

        loginUI.SetActive(false);
        menuCamera.gameObject.SetActive(false);

        player.SetActive(true);
        world.SetActive(true);
    }
}