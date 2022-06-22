using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private DataScript dataScript;
    [SerializeField] private Slider barSlider;
    [SerializeField] private Slider enemySlider;

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
