using UnityEngine;
using TMPro;

public class NumberController : MonoBehaviour
{
    public TMP_InputField inputField;
    private int number = 1;
    public int measuresMax;

    void Start()
    {
        // Initialize input field text with the initial number
        inputField.text = number.ToString();
    }

    public void IncreaseNumber()
    {
        
        if(number < measuresMax)
        {
            // Increase the number by 1
            number = number + 1;
            // Update the text in the input field
            inputField.text = number.ToString();
        }
      
    }

    public void DecreaseNumber()
    {
        // Decrease the number by 1
        if(number>1)
        {
            number -= 1;
            inputField.text = number.ToString();
        }
 
       
    }
}
