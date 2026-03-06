using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextAndButton : MonoBehaviour
{
    [Header("------------------------")]
    [SerializeField] GameObject StartText;
    [SerializeField] GameObject ResourseUI;
    [SerializeField] GameObject ResourseText;
    [SerializeField] GameObject ResourseTextTear2;
    [SerializeField] GameObject ResourseTextTear3;
    [SerializeField] GameObject ResourseTextTear4;
    [Header("------------------------")]
    [SerializeField] GameObject BuildText;
    [SerializeField] GameObject BuildButton;
    [SerializeField] GameObject BuildButtonText;
    [SerializeField] GameObject BuildTextHouse;
    [SerializeField] GameObject BuildTextPlants;
    [SerializeField] GameObject BuildTextWooden;
    [SerializeField] GameObject BuildTextCave;
    [Header("------------------------")]
    [SerializeField] GameObject UImenuBuildReal;
    [SerializeField] GameObject UImenuBuilddontReal;
    [Header("------------------------")]
    [SerializeField] GameObject BuildTextWoodenEnter;
    [SerializeField] GameObject BuildTextWoodeninfo;
    [SerializeField] GameObject BuildTextWoodenArea;
    [SerializeField] GameObject BuildTextWoodennext;
    [Header("------------------------")]
    [SerializeField] GameObject BuildingTextPlants;
    [SerializeField] GameObject BuildingAreaPlants;
    [SerializeField] GameObject BuildingAreaPlantsnext;
    [Header("------------------------")]
    [SerializeField] GameObject BuildingCaveText;
    [SerializeField] GameObject BuildingCaveArea;
    [SerializeField] GameObject TextFinish;

    private int count = 0;
    private int countAfter = 0;
    private int countAfterAfter = 0;
    private int countAfterAfterAfter = 0;
    private int countAfterAfterAfterFinal = 0;
    private bool uiIsAllow = false;
    private bool isFinish = false;
    private bool PressButton = false;
    private bool isfinishEnUIbuildmenu = false;
    private bool isPressWoodenBuild = false;
    private bool isPressPlantsBuild = false;
    private bool isPressCaveBuild = false;
    void Start()
    {
        ResetSetActive();
        UpdateUI();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && isFinish == false && PressButton == false || Input.GetKeyDown(KeyCode.KeypadEnter) && isFinish == false && PressButton == false)
        {
            count++;
            UpdateUI();

        }
        if (Input.GetKeyDown(KeyCode.Return) && isFinish == false && PressButton == true && isPressWoodenBuild == false && isPressPlantsBuild == false || Input.GetKeyDown(KeyCode.KeypadEnter) && isFinish == false && isPressWoodenBuild == false && PressButton == true && isPressPlantsBuild == false)
        {
            countAfter++;
            Debug.Log(countAfter);
            UpdateUIafterButton();

        }
        if (Input.GetKeyDown(KeyCode.Return) && isFinish == false && PressButton == true && isPressWoodenBuild == true && isPressPlantsBuild == false || Input.GetKeyDown(KeyCode.KeypadEnter) && isFinish == false && isPressWoodenBuild == true && PressButton == true && isPressPlantsBuild == false)
        {
            countAfterAfter++;
            Debug.Log("Переход выполнен");
            UpdateUIafterInfoBuild();

        }
        if (Input.GetKeyDown(KeyCode.Return) && isFinish == false && PressButton == true && isPressWoodenBuild == true && isPressPlantsBuild == true || Input.GetKeyDown(KeyCode.KeypadEnter) && isFinish == false && isPressWoodenBuild == true && PressButton == true && isPressPlantsBuild == true)
        {
            countAfterAfterAfter++;
            Debug.Log("2Переход выполнен");
            UpdateUIafterInfoBuildPlants();

        }
        if (Input.GetKeyDown(KeyCode.Return) && isFinish == false && PressButton == true && isPressWoodenBuild == true && isPressPlantsBuild == true && isPressCaveBuild == true || Input.GetKeyDown(KeyCode.KeypadEnter) && isFinish == false && isPressWoodenBuild == true && PressButton == true && isPressPlantsBuild == true && isPressCaveBuild == true)
        {
            countAfterAfterAfterFinal++;
            Debug.Log("3Переход выполнен");
            UpdateUIafterInfoBuildPlants();

        }
    }
    public void PressCaveBuild()
    {
        isPressCaveBuild = true;
        UpdateUIafterInfoBuildCave();
    }
    public void PressPlantsBuild()
    {
        isPressPlantsBuild = true;
        UpdateUIafterInfoBuildPlants();
    }
    public void PressWoodenBuild()
    {
        isPressWoodenBuild = true;
        UpdateUIafterInfoBuild();
    }
    public void PressButtonBuild()
    {
        PressButton = true;
        UpdateUIafterButton();
    }
    public void PressButtonBuildArea()
    {
        BuildTextWoodenArea.SetActive(true);
    }
    void UpdateUIafterInfoBuildCave()
    {
        // 4 цикл после нажатия по кнопке строить феомы
        ResetSetActive();
        switch (countAfterAfterAfterFinal)
        {
            case 0:
                BuildingCaveText.SetActive(true);
                BuildingCaveArea.SetActive(true);
                break;
            case 1:
                TextFinish.SetActive(true);
                break;
            case 2:
                //выход из сцены
                break;
        }

    }
    void UpdateUIafterInfoBuildPlants()
    {
        // 4 цикл после нажатия по кнопке строить феомы
        ResetSetActive();
        switch (countAfterAfterAfter)
        {
            case 0:
                BuildingTextPlants.SetActive(true);
                BuildingAreaPlants.SetActive(true);
                break;
            case 1:
                BuildingAreaPlantsnext.SetActive(true);
                break;
        }

    }
    void UpdateUIafterInfoBuild()
    {
        // 3 цикл после нажатия по кнопке строить лесорубов
        ResetSetActive();
        switch (countAfterAfter)
        {
            case 0:
                BuildTextWoodeninfo.SetActive(true);
                BuildTextWoodenArea.SetActive(true);
                break;
            case 1:
                BuildTextWoodennext.SetActive(true);
                break;
        }

    }
    void UpdateUIafterButton()
    {
        ResetSetActive();
        switch (countAfter)
        {
            case 0:
                BuildButtonText.SetActive(true);
                break;
            case 1:
                BuildTextHouse.SetActive(true);
                break;
            case 2:
                BuildTextPlants.SetActive(true);
                break;
            case 3:
                BuildTextWooden.SetActive(true);
                break;
            case 4:
                BuildTextCave.SetActive(true);
                break;
            case 5:
                BuildTextWoodenEnter.SetActive(true);
                UImenuBuildReal.SetActive(true);
                UImenuBuilddontReal.SetActive(false);
                isfinishEnUIbuildmenu = true; //ffffffffffffffffffffff
                break;
        }
    }
    void UpdateUI()
    {
        ResetSetActive();
        switch (count)
        {
            case 0:
                StartText.SetActive(true);
                break;
            case 1:
                ResourseUI.SetActive(true);
                ResourseText.SetActive(true);
                uiIsAllow = true;
                break;
            case 2:
                ResourseTextTear2.SetActive(true);
                break;
            case 3:
                ResourseTextTear3.SetActive(true);
                break;
            case 4:
                ResourseTextTear4.SetActive(true);
                break;
            case 5:
                BuildText.SetActive(true);
                BuildButton.SetActive(true);
                break;
        }
    }
    void ResetSetActive()
    {
         StartText.SetActive(false);
        ResourseUI.SetActive(uiIsAllow);
        ResourseText.SetActive(false);
        ResourseTextTear2.SetActive(false);
        ResourseTextTear3.SetActive(false);
        ResourseTextTear4.SetActive(false);
        BuildText.SetActive(false);
        BuildButtonText.SetActive(false);
        BuildTextHouse.SetActive(false);
        BuildTextPlants.SetActive(false);
        BuildTextWooden.SetActive(false);
        BuildTextCave.SetActive(false);
        BuildTextWoodenEnter.SetActive(false);
        BuildTextWoodeninfo.SetActive(false);
        BuildTextWoodenArea.SetActive(false);
        BuildTextWoodennext.SetActive(false);
        BuildingTextPlants.SetActive(false);
        BuildingAreaPlants.SetActive(false);
        BuildingAreaPlantsnext.SetActive(false);
        BuildingCaveText.SetActive(false);
        BuildingCaveArea.SetActive(false);
        TextFinish.SetActive(false);

        UImenuBuildReal.SetActive(isfinishEnUIbuildmenu);
        UImenuBuilddontReal.SetActive(!isfinishEnUIbuildmenu);
    }
}
