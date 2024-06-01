using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CounterController : MonoBehaviour
{
    [SerializeField]
    private int counter = 0;
    public float delay = 1f; // Delay between increments/decrements
    public TMP_InputField inputField;
    public int maxCount = 100; // Maximum count value
    public int minCount = 0; // Minimum count value

    private void Start()
    {
        counter = minCount;
        UpdateCounterDisplay();
    }

    private IEnumerator ChangeCounter(bool increase)
    {
        // Check and update the counter appropriately
        if (increase && counter < maxCount)
        {
            counter++;
            UpdateCounterDisplay();
        }
        else if (!increase && counter > minCount)
        {
            counter--;
            UpdateCounterDisplay();
        }

        // Continue updating if the button is still held down and within the limits
        while (increase ? isIncreasing : isDecreasing)
        {
            yield return new WaitForSeconds(delay);

            if (increase && counter < maxCount)
            {
                counter++;
            }
            else if (!increase && counter > minCount)
            {
                counter--;
            }
            else
            {
                // Exit loop if max or min is reached
                break;
            }

            UpdateCounterDisplay();
        }
    }

    public void CountUp()
    {
        if (!isIncreasing)
        {
            isIncreasing = true;
            StartCoroutine(ChangeCounter(true));
        }
    }

    public void CountDown()
    {
        if (!isDecreasing)
        {
            isDecreasing = true;
            StartCoroutine(ChangeCounter(false));
        }
    }

    public void StopCounting()
    {
        isIncreasing = false;
        isDecreasing = false;
        StopAllCoroutines();

        StartCoroutine(ResumeLoopers());
    }

    private void UpdateCounterDisplay()
    {
        if (inputField != null)
        {
            inputField.text = counter.ToString();
        }
    }

    private bool isIncreasing = false;
    private bool isDecreasing = false;


    IEnumerator ResumeLoopers()
    {
        yield return new WaitForSeconds(0.5f);
        Station.instance.PlayLoopersFromPlayState();
    }
}
