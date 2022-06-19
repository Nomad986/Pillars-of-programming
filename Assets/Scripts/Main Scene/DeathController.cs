using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DeathController : MonoBehaviour
{
    public void ReturnToMenu(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(0);
        }
    }
}
