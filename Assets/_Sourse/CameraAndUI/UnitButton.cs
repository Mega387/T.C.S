using UnityEngine;
using UnityEngine.UI;

public class UnitButton : MonoBehaviour
{
    public int unitIndex = 0;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    void OnButtonClick()
    {
        if (TrainingHouseUIManager.Instance != null)
        {
            switch (unitIndex)
            {
                case 0:
                    TrainingHouseUIManager.Instance.OpenUnit1();
                    break;
                case 1:
                    TrainingHouseUIManager.Instance.OpenUnit2();
                    break;
                case 2:
                    TrainingHouseUIManager.Instance.OpenUnit3();
                    break;
                case 3:
                    TrainingHouseUIManager.Instance.OpenUnit4();
                    break;
            }
        }
    }
}