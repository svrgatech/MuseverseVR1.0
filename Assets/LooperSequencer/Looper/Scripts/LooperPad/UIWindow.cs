using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIWindow : MonoBehaviour
{
    private RectTransform windowRectTransform;
    private bool isOpen = false; // Track the current state of the window

    private Vector3 initialScale;

    // Start is called before the first frame update
    void Start()
    {
        initialScale = transform.localScale;
        windowRectTransform = GetComponent<RectTransform>();
        //// Initial scale set to zero
        //windowRectTransform.localScale = Vector3.zero;

        //CloseWindow();
    }

    // Open the window with a tween animation
    public void OpenWindow(float duration = 1f)
    {
        // Tween the window's scale from zero to one
        windowRectTransform.DOScale(initialScale, duration).SetEase(Ease.OutBack);
        isOpen = true;
    }

    // Close the window with a tween animation
    public void CloseWindow(float duration = 1f)
    {
        // Tween the window's scale from one to zero
        windowRectTransform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack);
        isOpen = false;
    }

    // Toggle the window between open and closed states
    public void ToggleWindow(float duration = 0.2f)
    {
        if (isOpen)
        {
            CloseWindow(duration);
        }
        else
        {
            OpenWindow(duration);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // You can add any additional logic or functionality here if needed
    }
}
