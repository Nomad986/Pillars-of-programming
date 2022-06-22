using UnityEngine;
using UnityEngine.SceneManagement;


public class PauseMenuScript : MonoBehaviour
{
    [SerializeField] private GameObject player;

    public void Click()
    {
        player.GetComponent<PlayerController>().PauseGameOnClick();
    }

    public void ClickExit()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
