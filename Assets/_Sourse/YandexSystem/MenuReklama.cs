using UnityEngine;
using YG;

public class MenuReklama : MonoBehaviour
{
    private static bool isFirstLaunch = true;

    void Start()
    {
        Debug.Log("MenuReklama: Start вызван");

        if (isFirstLaunch)
        {
            Debug.Log("MenuReklama: Первый запуск меню, реклама НЕ будет показана");
            isFirstLaunch = false;
        }
        else
        {
            Debug.Log("MenuReklama: Повторный заход в меню, реклама БУДЕТ показана");
            ShowAd();
        }
    }

    private void ShowAd()
    {
        Debug.Log("MenuReklama: ShowAd вызван");

        if (YG2.isSDKEnabled)
        {
            Debug.Log("MenuReklama: SDK готов, показываем рекламу");
            YG2.InterstitialAdvShow();
            Debug.Log("MenuReklama: YG2.InterstitialAdvShow() выполнен");
        }
        else
        {
            Debug.LogWarning("MenuReklama: SDK НЕ готов, реклама не будет показана");
        }
    }
}