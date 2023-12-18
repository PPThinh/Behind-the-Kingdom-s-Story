using GameCreator.Runtime.Quests;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace KingdomProject.Manager
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance;
        [SerializeField] private GameObject _loadingCanvas;
        [SerializeField] private Image _progressBar;
        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }

        public async void LoadScene(string sceneName)
        {
            var scene = SceneManager.LoadSceneAsync(sceneName);
            scene.allowSceneActivation = false;

            _loadingCanvas.SetActive(true);

            do
            {
                await System.Threading.Tasks.Task.Delay(100);
                _progressBar.fillAmount = scene.progress;
            }
            while (scene.progress < 0.9f);

            scene.allowSceneActivation = true;

        }
    }
}
