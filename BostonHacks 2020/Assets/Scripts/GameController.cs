using GoogleSheetsToUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using Michsky.UI.ModernUIPack;

public class GameController : MonoBehaviour
{
    public string associatedSheet = "1j3CFtSOcuSWDcjHorCrlwONceXIb1S8Efu1naGzJe3U";
    public string associatedWorksheet = "Sheet1";

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

    public NotificationManager pollutionNotif;

    public Image fakeCursor;

    //World
    public GameObject housePrefab;
    public Transform housesParent;
    public Transform houseSpawnpointsParent;

    public Transform indoorsParent;
    public Transform indoorsSpawnpoint;

    public Transform interiorLightsParent;
    public Transform interiorWaterObjectsParent;
    public bool hasToggledInteriorProps;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.X))
        {
            data[currentUserDataIndex].totalScore += 5;
            UpdateUserScore(4, data[currentUserDataIndex].totalScore);
        }
    }

    public void ChangePollutionVals(int type, int change) //Types: 1 = energy, 2 = water, 3 = waste (total gets calculated automatically)
    {
        UserData ud = data[currentUserDataIndex];

        switch (type)
        {
            case 1:
                ud.energyScore += change;
                UpdateUserScore(type, ud.energyScore);
                RunPollutionNotif("Energy usage lowered!");
                break;
            case 2:
                ud.waterScore += change;
                UpdateUserScore(type, ud.waterScore);
                RunPollutionNotif("Water usage lowered!");
                break;
            case 3:
                ud.wasteScore += change;
                UpdateUserScore(type, ud.wasteScore);
                RunPollutionNotif("Waste usage lowered!");
                break;
        }

        UpdateHouseData(housesParent.Find(fbm.user.UserId).GetComponent<HouseController>());

        ud.totalScore += change;
        UpdateUserScore(4, ud.totalScore);
    }

    public void RunPollutionNotif(string description = "", string title = "")
    {
        if (description.Length > 0)
            pollutionNotif.description = description;
        if (title.Length > 0)
            pollutionNotif.title = title;

        pollutionNotif.UpdateUI();

        pollutionNotif.OpenNotification();
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
            int.TryParse(ss.rows[i][3].value, out userData.energyScore);
            int.TryParse(ss.rows[i][4].value, out userData.waterScore);
            int.TryParse(ss.rows[i][5].value, out userData.wasteScore);
            int.TryParse(ss.rows[i][6].value, out userData.totalScore);

            data.Add(userData);

            if(userData.id == fbm.user.UserId)
            {
                currentUserDataIndex = data.Count - 1;

                if(!hasToggledInteriorProps)
                {
                    //Lights
                    List<int> childLightsActivated = new List<int>();
                    for (int k = 0; k < userData.energyScore; k++)
                    {
                        int temp = Random.Range(0, interiorLightsParent.transform.childCount);
                        if (!childLightsActivated.Contains(temp))
                        {
                            childLightsActivated.Add(temp);
                            interiorLightsParent.GetChild(temp).GetComponent<LightController>().SetLightOn();
                        }
                    }

                    //Water
                    List<int> childWaterObjectsEnabled = new List<int>();
                    for (int k = 0; k < Mathf.Min(interiorWaterObjectsParent.childCount, userData.waterScore); k++)
                    {
                        int temp = Random.Range(0, interiorWaterObjectsParent.transform.childCount);
                        if (!childWaterObjectsEnabled.Contains(temp))
                        {
                            childWaterObjectsEnabled.Add(temp);
                            interiorWaterObjectsParent.GetChild(temp).GetComponent<WaterObjectController>().SetWaterOn();
                        }
                    }

                    hasToggledInteriorProps = true;
                }
            }
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

        hc.name = ud.id;
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

        GetSpreadsheetData();
        StartCoroutine(GetSpreadsheetDataTimer());

        loginUI.SetActive(false);
        menuCamera.gameObject.SetActive(false);

        player.SetActive(true);
        world.SetActive(true);

        gameStarted = true;
    }

    public void GoIndoors()
    {
        Debug.Log("Entering indoors...");

        housesParent.gameObject.SetActive(false);
        indoorsParent.gameObject.SetActive(true);

        FirstPersonController fpc = player.GetComponent<FirstPersonController>();
        fpc.enabled = false;

        player.transform.position = indoorsSpawnpoint.position;
        player.transform.localEulerAngles = new Vector3(0, 90, 0);
        fpc.m_MouseLook.m_CharacterTargetRot = player.transform.rotation;

        fakeCursor.enabled = false;

        StartCoroutine(EnableFirstPersonController());
    }

    public void GoOutdoors()
    {
        Debug.Log("Entering outdoors...");

        housesParent.gameObject.SetActive(true);
        indoorsParent.gameObject.SetActive(false);

        FirstPersonController fpc = player.GetComponent<FirstPersonController>();
        fpc.enabled = false;

        HouseController hc = housesParent.Find(fbm.user.UserId).GetComponent<HouseController>();
        player.transform.position = hc.exitSpawnpoint.position;
        player.transform.rotation = hc.exitSpawnpoint.rotation;
        fpc.m_MouseLook.m_CharacterTargetRot = player.transform.rotation;

        fakeCursor.enabled = false;

        StartCoroutine(EnableFirstPersonController());
    }

    IEnumerator EnableFirstPersonController()
    {
        yield return new WaitForFixedUpdate();
        player.GetComponent<FirstPersonController>().enabled = true;
    }
}