using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWindowManager : MonoBehaviour
{

    [SerializeField]
    public UIWindow[] uIWindows;

    public int presentOpenWindow = 999;

    public void ClosePresentOpenWindow()
    {
        uIWindows[presentOpenWindow].CloseWindow();
    }

    public void OpenWindow(int index)
    {
        uIWindows[index].OpenWindow();
    }

    public void ToggleWindow(int index)
    {
        if (index != presentOpenWindow && presentOpenWindow!=999)
            ClosePresentOpenWindow();

        uIWindows[index].ToggleWindow();
    }

    public void CloseAllWindows()
    {
        for(int i=0; i<uIWindows.Length; i++)
        {
            uIWindows[i].CloseWindow();
        }
    }

}
