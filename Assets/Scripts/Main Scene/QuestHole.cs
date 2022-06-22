using UnityEngine;

public class QuestHole : MonoBehaviour
{
    private DataScript data;
    private int barsToCollect;
    [SerializeField] private GameObject player;

    private void Awake()
    {
        data = FindObjectOfType<DataScript>();

        if (data == null)
        {
            barsToCollect = 7;
        }
        else
        {
            barsToCollect = data.GetBars();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Metal"))
        {
            barsToCollect--;
            Destroy(other.gameObject);
            if (barsToCollect == 0)
            {
                player.GetComponent<PlayerController>().Win();
            }
        }
    }

    public int GetRemainingBars()
    {
        return barsToCollect;
    }

}
