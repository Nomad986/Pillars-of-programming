using UnityEngine;
using UnityEngine.UI;

public class GrabButtonScript : MonoBehaviour
{
    [SerializeField] private Transform crosshairTransform;
    private Image image;
    private Text text;

    private Vector3 buttonPosition;

    private void Awake()
    {
        float addX = Screen.width / 64;
        float addY = Screen.height / 36;
        buttonPosition = 
            new Vector3(crosshairTransform.position.x + addX, crosshairTransform.position.y + addY);

        image = GetComponent<Image>();
        text = GetComponentInChildren<Text>();
    }

    private void Update()
    {
        transform.position = buttonPosition;
    }

    public void SetButton(bool active)
    {
        image.enabled = active;
        text.enabled = active;
    }
}
