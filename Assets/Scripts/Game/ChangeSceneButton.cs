using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KingdomProject.Manager;

namespace KingdomProject
{
    public class ChangeSceneButton : MonoBehaviour
    {
        string sceneName = "1_Main_Menu";
        Button btn;
        public void ChangeScene()
        {
            LevelManager.Instance.LoadScene(sceneName);
        }

        public void Start()
        {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(ChangeScene);
        }
    }
}
