using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResoursUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI populationText;
    [SerializeField] private TextMeshProUGUI woodenText;
    [SerializeField] private TextMeshProUGUI eatText;
    [SerializeField] private TextMeshProUGUI stoneText;
    [SerializeField] private TextMeshProUGUI ironIngotText;
    [SerializeField] private TextMeshProUGUI ironOreText;
    public static float population = 20;
    public static float wooden = 200;
    public static float eat = 200;
    public static float stone = 0;
    public static float ironIngot = 0;
    public static float ironOre = 0;
    void Start()
    {
        
    }

    void Update()
    {
        populationText.text = $"{population}";
        woodenText.text = $"{wooden}";
        eatText.text = $"{eat}";
        stoneText.text = $"{stone}";
        ironIngotText.text = $"{ironIngot}";
        ironOreText.text = $"{ironOre}";
    }
}
