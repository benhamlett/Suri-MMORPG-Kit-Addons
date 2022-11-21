/* Copyright TidyDev Software Solutions LTD - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Ben Hamlett <ben.hamlett@tidydev.co>, 2022
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class UILoadingScene
    {
        public MapInfo map;
        public string[] tips;
        public Sprite[] mapArt;
    }

    public partial class UISceneLoading
    {
        [Header("References")]
        public TextWrapper tipTextWrapper;

        [Header("Extra Options")]
        public List<UILoadingScene> uiLoadingScenes = new List<UILoadingScene>();

        private Image loadingImage;

        private void Start()
        {
            SceneManager.activeSceneChanged += ChangedActiveScene;
        }

        void OnDisable()
        {
            SceneManager.activeSceneChanged -= ChangedActiveScene;
        }

        /// <summary>
        /// Attempts to display a art and tips for the map that is being loaded
        /// </summary>
        /// <param name="current"></param>
        /// <param name="next"></param>
        private void ChangedActiveScene(Scene current, Scene next)
        {
            if (tipTextWrapper) tipTextWrapper.text = "";

            UILoadingScene map = GetMapInfo(next.name);
            if (map != null)
            {
                if (map.mapArt.Length > 0)
                {
                    int _rand = Random.Range(0, (map.mapArt.Length - 1));
                    rootObject.GetComponent<Image>().sprite = map.mapArt[_rand];
                }
                if (tipTextWrapper && map.tips.Length > 0) tipTextWrapper.text = map.tips[Random.Range(0, (map.tips.Length - 1))];
            }
        }

        /// <summary>
        /// Attempts to get the MapInfo data based on the name
        /// </summary>
        /// <param name="mapName"></param>
        /// <returns></returns>
        private UILoadingScene GetMapInfo(string mapName)
        {
            foreach(UILoadingScene s in uiLoadingScenes)
            {
                if (s.map.GetSceneName() == mapName)
                    return s;
            }

            return null;
        }
    }
}