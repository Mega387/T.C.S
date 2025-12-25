using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    public int buildingIndex;
    private BuildManager buildingManager;

    void Start()
    {
        buildingManager = FindObjectOfType<BuildManager>();
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    public void OnButtonClick()
    {
        buildingManager.SelectBuilding(buildingIndex);
    }
}