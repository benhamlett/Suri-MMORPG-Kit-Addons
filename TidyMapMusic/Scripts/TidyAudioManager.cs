/* Copyright TidyDev Software Solutions LTD - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Ben Hamlett <ben.hamlett@tidydev.co>, 2022
 */

using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using MultiplayerARPG;

namespace TidyDev
{
    public class TidyAudioManager : MonoBehaviour
    {
        public static TidyAudioManager Singleton { get; private set; }

        [Header("Components")]
        public AudioSource audioSourceOne;
        public AudioSource audioSourceTwo;
        public AudioMixer audioMixer;

        [Header("Options")]
        public float crossFadeSpeed = 3f;
        public bool randomPlaylist = true;

        private BaseMapInfo currentMap;
        private Coroutine fadeEnumerator;
        private Coroutine playlistEnumerator;
        private string currentMusicId;
        private MapInfoMusic currentMapMusic;
        private AudioSource currentAudioSource;
        private int currentPlaylistIndex = 0;
        private bool transitioning = false;

        /// <summary>
        /// Set up the instance
        /// </summary>
        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            Singleton = this;
        }

        private void Start()
        {
            // Hook into the scene load event (Requires TidyUISceneLoading)
            UINetworkSceneLoading.OnLoadSceneStartInit += GetCurrentMap;

            // Check that we have both audio sources, they are required for crossfading
            if (!audioSourceOne || !audioSourceTwo) this.enabled = false;
        }

        private void LateUpdate()
        {
            // Playlist for map
            if (!currentAudioSource || transitioning) return;
            if(currentAudioSource.isPlaying && currentAudioSource.time >= (currentAudioSource.clip.length - 1f))
            {
                PlayDefaultMapMusic();
            }

        }

        void OnDisable()
        {
            // Scrap the hook :)
            UINetworkSceneLoading.OnLoadSceneStartInit -= GetCurrentMap;
        }

        /// <summary>
        /// Get a reference to the name of the current map
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="isOnline"></param>
        /// <param name="progress"></param>
        private void GetCurrentMap(string sceneName, bool isOnline, float progress)
        {
            currentMap = GetCurrentMapInfo(sceneName);
            PlayDefaultMapMusic();
        }

        #region Functionality
        /// <summary>
        /// Try to play Map music based on its unique id
        /// </summary>
        /// <param name="reference"></param>
        public void PlayMapMusic(string reference)
        {
            if (!currentMap || currentMap.music.Count == 0 || reference == currentMusicId) return;

            MapInfoMusic tmpAudioClip = GetMapMusic(reference);
            if (!tmpAudioClip.audioClip) return;

            StopCoroutine(fadeEnumerator);
            fadeEnumerator = null;
            fadeEnumerator = StartCoroutine(CrossFadeMusic(tmpAudioClip));
            currentMusicId = reference;
            currentMapMusic = tmpAudioClip;
        }

        /// <summary>
        /// On scene change if the Map has a default audio clip it will play 
        /// </summary>
        public void PlayDefaultMapMusic()
        {
            if (!currentMap || currentMap.defaultMusic.Count == 0)
                return;

            if (fadeEnumerator != null)
                StopCoroutine(fadeEnumerator);

            int idx = (randomPlaylist == true ? Random.Range(0, (currentMap.defaultMusic.Count - 1)) : currentPlaylistIndex);

            if (!randomPlaylist) idx = (currentPlaylistIndex +1 > (currentMap.defaultMusic.Count - 1) ? 0 : currentPlaylistIndex +1);

            fadeEnumerator = StartCoroutine(CrossFadeMusic(currentMap.defaultMusic[idx]));
            currentMusicId = currentMap.defaultMusic[idx].uniqueId;
            currentMapMusic = currentMap.defaultMusic[idx];
            currentPlaylistIndex = idx;
        }

        /// <summary>
        /// Fade Map Music Out
        /// </summary>
        public void StopMapMusic()
        {
            fadeEnumerator = StartCoroutine(FadeMusicOut());
            currentMapMusic = null;
            currentMusicId = null;
        }

        /// <summary>
        /// Handles the crossfade between audio sources
        /// </summary>
        /// <param name="audioClip"></param>
        /// <returns></returns>
        private IEnumerator CrossFadeMusic(MapInfoMusic mapMusic)
        {
            transitioning = true;
            // Get exposed audio volume from the Audi Mixer
            float audioSourceOneVol;
            audioMixer.GetFloat("MusicSourceOneVolume", out audioSourceOneVol);
            float audioSourceTwoVol;
            audioMixer.GetFloat("MusicSourceTwoVolume", out audioSourceTwoVol);
            // Decide which audio source is currently playing
            AudioSource tmpCurrentAudioSource = audioSourceOneVol == 0 ? audioSourceOne : audioSourceTwo;
            AudioSource tmpNextAudioSource = audioSourceOneVol == 0 ? audioSourceTwo : audioSourceOne;
            // Setup the next music clip to play
            tmpNextAudioSource.clip = mapMusic.audioClip;
            currentAudioSource = tmpNextAudioSource;
            if (!tmpNextAudioSource.isPlaying)
                tmpNextAudioSource.Play();

            float currentTime = 0;
            float currentVol = Mathf.Pow(10, (tmpCurrentAudioSource == audioSourceOne ? audioSourceOneVol : audioSourceTwoVol) / 20);
            float currentVolTwo = Mathf.Pow(10, (tmpNextAudioSource == audioSourceOne ? audioSourceOneVol : audioSourceTwoVol) / 20);

            while (currentTime < crossFadeSpeed)
            {
                currentTime += Time.deltaTime;
                float tmpNewVolOne = Mathf.Lerp(currentVol, 0, currentTime / crossFadeSpeed);
                audioMixer.SetFloat(tmpCurrentAudioSource == audioSourceOne ? "MusicSourceOneVolume" : "MusicSourceTwoVolume", Mathf.Log10(tmpNewVolOne) * 20);
                float tmpNewVolTwo = Mathf.Lerp(currentVolTwo, mapMusic.audioVolume, currentTime / crossFadeSpeed);
                audioMixer.SetFloat(tmpNextAudioSource == audioSourceOne ? "MusicSourceOneVolume" : "MusicSourceTwoVolume", Mathf.Log10(tmpNewVolTwo) * 20);

                yield return null;
            }

            // Finished
            transitioning = false;
            yield break;
        }
        
        /// <summary>
        /// Handles the fade out of map music
        /// </summary>
        /// <returns></returns>
        private IEnumerator FadeMusicOut()
        {
            // Get exposed audio volume from the Audi Mixer
            float audioSourceOneVol;
            audioMixer.GetFloat("MusicSourceOneVolume", out audioSourceOneVol);
            float audioSourceTwoVol;
            audioMixer.GetFloat("MusicSourceTwoVolume", out audioSourceTwoVol);
            // Decide which audio source is currently playing
            AudioSource tmpCurrentAudioSource = audioSourceOneVol == 0 ? audioSourceOne : audioSourceTwo;
            AudioSource tmpNextAudioSource = audioSourceOneVol == 0 ? audioSourceTwo : audioSourceOne;

            float currentTime = 0;
            float currentVol = Mathf.Pow(10, (tmpCurrentAudioSource == audioSourceOne ? audioSourceOneVol : audioSourceTwoVol) / 20);
            float currentVolTwo = Mathf.Pow(10, (tmpNextAudioSource == audioSourceOne ? audioSourceOneVol : audioSourceTwoVol) / 20);

            while (currentTime < crossFadeSpeed)
            {
                currentTime += Time.deltaTime;
                float tmpNewVolOne = Mathf.Lerp(currentVol, 0, currentTime / crossFadeSpeed);
                audioMixer.SetFloat(tmpCurrentAudioSource == audioSourceOne ? "MusicSourceOneVolume" : "MusicSourceTwoVolume", Mathf.Log10(tmpNewVolOne) * 20);
                float tmpNewVolTwo = Mathf.Lerp(currentVolTwo, 0, currentTime / crossFadeSpeed);
                audioMixer.SetFloat(tmpNextAudioSource == audioSourceOne ? "MusicSourceOneVolume" : "MusicSourceTwoVolume", Mathf.Log10(tmpNewVolTwo) * 20);

                yield return null;
            }
            yield break;
        }
        #endregion

        #region Utils
        /// <summary>
        /// Get the current maps base info from the GameInstance
        /// </summary>
        /// <param name="reference">The Scene Name</param>
        /// <returns></returns>
        private BaseMapInfo GetCurrentMapInfo(string reference)
        {
            if (GameInstance.MapInfos.Count == 0)
                return null;

            foreach (BaseMapInfo mapInfo in GameInstance.MapInfos.Values)
            {
                if (mapInfo != null && !string.IsNullOrEmpty(mapInfo.GetSceneName()) && mapInfo.GetSceneName() == reference)
                    return mapInfo;
            }

            return null;
        }
        
        /// <summary>
        /// Get the audio clip from the referenced music
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        private MapInfoMusic GetMapMusic(string reference)
        {
            foreach (MapInfoMusic m in currentMap.music)
                if (m.uniqueId == reference)
                    return m;

            return null;
        }

        /// <summary>
        /// Check if current music is default music for playlist
        /// </summary>
        /// <param name="mapMusic"></param>
        /// <returns></returns>
        private bool IsDefaultMusic(MapInfoMusic mapMusic)
        {
            foreach (MapInfoMusic m in currentMap.defaultMusic)
                if (m == mapMusic)
                    return true;

            return false;
        }
        #endregion
    }
}