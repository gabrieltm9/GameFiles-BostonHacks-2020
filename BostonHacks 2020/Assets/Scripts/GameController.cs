using GoogleSheetsToUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    public int maxScore = 30;

    //Data
    public GstuSpreadSheet spreadsheet;
    public List<UserData> data;
    public int currentUserDataIndex;

    public int worldScore;
    public int totalWorldScore;
    public Slider worldScoreSlider;

    //UI
    public GameObject loginUI;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    //World
    public GameObject housePrefab;
    public Transform housesParent;
    public Transform houseSpawnpointsParent;

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

        CalculateWorldScore();

        //Update existing houses
        for (int i = 0; i < housesParent.childCount; i++)
            UpdateHouseData(housesParent.GetChild(i).GetComponent<HouseController>());

        //Spawn new houses
        while(housesParent.childCount < data.Count)
        {
            HouseController hc = SpawnHouse(housesParent.childCount).GetComponent<HouseController>();
            hc.userDataID = data[housesParent.childCount - 1].id;
            UpdateHouseData(hc);
        }
    }

    void UpdateHouseData(HouseController hc)
    {
        UserData ud = data.Find(v => v.id == hc.userDataID);

        hc.nameText.text = ud.name;
        hc.scoreText.text = ud.totalScore + "/" + maxScore;

        if (ud.waterScore > 8)
            hc.waterLeak.Play();
        else
            hc.waterLeak.Stop();

        if (ud.wasteScore > 5)
            hc.smokeStack1.Play();
        if (ud.wasteScore > 10)
            hc.smokeStack2.Play();
        if(ud.wasteScore < 5)
        {
            hc.smokeStack1.Stop();
            hc.smokeStack2.Stop();
        }

        //if(ud.energyScore > 8)
    }

    void CalculateWorldScore()
    {
        worldScore = 0;
        totalWorldScore = data.Count * maxScore;

        foreach (UserData ud in data)
            worldScore += ud.totalScore;

        float temp = (float) worldScore / totalWorldScore;
        worldScoreSlider.value = 1 - temp;
    }

    public void UpdateUserScore(int scoreType, int newScore) //Score types: 1 = energy, 2 = water, 3 = waste, 4 = total
    {
        spreadsheet.rows[fbm.user.UserId][scoreType + 2].UpdateCellValue(associatedSheet, associatedWorksheet, "" + newScore);
    }

    GameObject SpawnHouse(int spawnpointIndex)
    {
        return Instantiate(housePrefab, houseSpawnpointsParent.GetChild(spawnpointIndex).transform.position, houseSpawnpointsParent.GetChild(spawnpointIndex).transform.rotation, housesParent);
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