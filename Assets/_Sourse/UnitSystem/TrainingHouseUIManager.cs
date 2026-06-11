using UnityEngine;

public class TrainingHouseUIManager : MonoBehaviour
{
    public static TrainingHouseUIManager Instance;

    public GameObject unitSelectionMenu;
    public GameObject unit1Menu;
    public GameObject unit2Menu;
    public GameObject unit3Menu;
    public GameObject unit4Menu;

    private trainingHouseUnits currentManager;
    private int selectedUnitIndex = -1;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        CloseAllMenus();
    }

    public void OpenUnitSelectionMenu(trainingHouseUnits manager)
    {
        currentManager = manager;
        unitSelectionMenu.SetActive(true);
    }

    public void OpenUnit1()
    {
        Debug.Log("OpenUnit1 вызван");
        selectedUnitIndex = 0;
        unitSelectionMenu.SetActive(false);
        unit1Menu.SetActive(true);
    }

    public void OpenUnit2()
    {
        selectedUnitIndex = 1;
        unitSelectionMenu.SetActive(false);
        unit2Menu.SetActive(true);
    }

    public void OpenUnit3()
    {
        selectedUnitIndex = 2;
        unitSelectionMenu.SetActive(false);
        unit3Menu.SetActive(true);
    }

    public void OpenUnit4()
    {
        selectedUnitIndex = 3;
        unitSelectionMenu.SetActive(false);
        unit4Menu.SetActive(true);
    }

    public void StartProduction()
    {
        if (currentManager != null && selectedUnitIndex >= 0)
        {
            currentManager.StartProductionOnHouse(selectedUnitIndex);
            CloseAllMenus();
        }
    }

    public void BackToSelection()
    {
        unit1Menu.SetActive(false);
        unit2Menu.SetActive(false);
        unit3Menu.SetActive(false);
        unit4Menu.SetActive(false);
        unitSelectionMenu.SetActive(true);
    }

    public void CloseAllMenus()
    {
        unitSelectionMenu.SetActive(false);
        unit1Menu.SetActive(false);
        unit2Menu.SetActive(false);
        unit3Menu.SetActive(false);
        unit4Menu.SetActive(false);

        if (currentManager != null)
            currentManager.ClearCurrentSelection();

        currentManager = null;
        selectedUnitIndex = -1;
    }
}