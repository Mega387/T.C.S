using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void Quit()
    {
#if UNITY_WEBGL
            Application.Quit();
#else
        Application.Quit();
#endif
    }
}