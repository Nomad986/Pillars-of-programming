using UnityEngine;

public class DataScript : MonoBehaviour
{
    private static DataScript dataInstance;

    private int numberOfEnemies = 10;
    private int numberOfBars = 7;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (dataInstance == null)
        {
            dataInstance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void SetEnemies(int number)
    {
        numberOfEnemies = number;
    }

    public void SetBars(int number)
    {
        numberOfBars = number;
    }

    public int GeEnemies()
    {
        return numberOfEnemies;
    }

    public int GetBars()
    {
        return numberOfBars;
    }
}
