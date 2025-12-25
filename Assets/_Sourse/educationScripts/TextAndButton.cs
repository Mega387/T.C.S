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
    [SerializeField] GameObject BuildTextWoodenEnter;
    [SerializeField] GameObject BuildTextWoodenArea;
    [Header("------------------------")]
    [SerializeField] GameObject BuildingTextPlants;
    [SerializeField] GameObject BuildingAreaPlants;
    [Header("------------------------")]
    [SerializeField] GameObject BuildingCaveText;
    [SerializeField] GameObject BuildingCaveArea;
    [SerializeField] GameObject TextFinish;

    private int count = 0;
    private int countAfter = 0;
    private bool uiIsAllow = false;
    private bool isFinish = false;
    private bool PressButton = false;
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
        if (Input.GetKeyDown(KeyCode.Return) && isFinish == false && PressButton == true || Input.GetKeyDown(KeyCode.KeypadEnter) && isFinish == false && PressButton == true)
        {
            countAfter++;
            UpdateUIafterButton();

        }
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
                BuildTextWoodenEnter.SetActive(true);//fffff
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
        BuildTextWoodenArea.SetActive(false);
        BuildingTextPlants.SetActive(false);
        BuildingAreaPlants.SetActive(false);
        BuildingCaveText.SetActive(false);
        BuildingCaveArea.SetActive(false);
        TextFinish.SetActive(false);
    }
}
