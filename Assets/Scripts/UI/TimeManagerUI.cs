using UnityEngine;
using UnityEngine.UI;

public class TimeManagerUI : MonoBehaviour
{
    [SerializeField]private TimeManager timeManager;
    public Text dayTimeText;

    public void Update()
    {
        string dayTime = timeManager.Hours.ToString() + ": " + timeManager.Minutes.ToString();
        dayTimeText.text = dayTime;
    }

}