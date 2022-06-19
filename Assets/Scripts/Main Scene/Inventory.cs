using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    private float weight;
    private int goldNumber;
    private int silverNumber;
    private int platinumNumber;
    private int copperNumber;

    [SerializeField] private Text goldText;
    [SerializeField] private Text silverText;
    [SerializeField] private Text copperText;
    [SerializeField] private Text platinumText;

    private void Awake()
    {
        weight = 0;

        goldNumber = 0;
        silverNumber = 0;
        platinumNumber = 0;
        copperNumber = 0;

        SetMetalNumber(goldText, goldNumber);
        SetMetalNumber(silverText, silverNumber);
        SetMetalNumber(platinumText, platinumNumber);
        SetMetalNumber(copperText, copperNumber);
    }

    public float GetInventoryWeight()
    { 
        return weight; 
    }

    public void IncreaseCopper()
    {
        copperNumber++;
        weight += 0.5f;
        SetMetalNumber(copperText, copperNumber);
    }

    public bool DecreaseCopper()
    {
        //returns false when there is no more metal in inventory to decrease, otherwise true

        if (copperNumber < 1)
        {
            return false;
        }

        copperNumber--;
        weight -= 0.5f;
        SetMetalNumber(copperText, copperNumber);
        return true;
    }

    public void IncreaseSilver()
    {
        silverNumber++;
        weight += 1f;
        SetMetalNumber(silverText, silverNumber);
    }

    public bool DecreaseSilver()
    {
        //returns false when there is no more metal in inventory to decrease, otherwise true

        if (silverNumber < 1)
        {
            return false;
        }

        silverNumber--;
        weight -= 1f;
        SetMetalNumber(silverText, silverNumber);
        return true;
    }

    public void IncreaseGold()
    {
        goldNumber++;
        weight += 1.5f;
        SetMetalNumber(goldText, goldNumber);
    }

    public bool DecreaseGold()
    {
        //returns false when there is no more metal in inventory to decrease, otherwise true

        if (goldNumber < 1)
        {
            return false;
        }
        
        goldNumber--;
        weight -= 1.5f;
        SetMetalNumber(goldText, goldNumber);
        return true;
    }

    public void IncreasePlatinum()
    {
        platinumNumber++;
        weight += 2.5f;
        SetMetalNumber(platinumText, platinumNumber);
    }

    public bool DecreasePlatinum()
    {
        //returns false when there is no more metal in inventory to decrease, otherwise true

        if (platinumNumber < 1)
        {
            return false;
        }

        platinumNumber--;
        weight -= 2.5f;
        SetMetalNumber(platinumText, platinumNumber);
        return true;
    }

    private void SetMetalNumber(Text metalText, float metalNumber)
    {
        //Set the metal number displayed on inventory UI

        metalText.text = metalNumber.ToString();
    }
}
