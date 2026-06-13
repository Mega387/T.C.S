using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bootstrapper : MonoBehaviour
{
    void Start()
    {

        ResoursUI.population = 20;
        ResoursUI.wooden = 160;//150
        ResoursUI.eat = 160;
        ResoursUI.stone = 0;
        ResoursUI.ironIngot = 0;
        ResoursUI.ironOre = 0;
    }
}
