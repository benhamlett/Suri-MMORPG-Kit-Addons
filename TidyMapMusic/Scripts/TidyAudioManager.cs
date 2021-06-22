using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MultiplayerARPG
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

        private BaseMapInfo currentMap;
        private Coroutine enumerator;
        private string currentMusicId;

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
            if (!currentMap || currentMap.music.Count == 0 || reference == currentMusicId)
                return;

            AudioClip tmpAudioClip = GetMapMusic(reference);
            if (!tmpAudioClip)
                return;

            if (enumerator != null)
                StopCoroutine(enumerator);

            enumerator = StartCoroutine(CrossFadeMusic(tmpAudioClip));
            currentMusicId = reference;
        }

        /// <summary>
        /// On scene change if the Map has a default audio clip it will play 
        /// </summary>
        public void PlayDefaultMapMusic()
        {
            if (!currentMap)
                return;

            if (enumerator != null)
                StopCoroutine(enumerator);

            enumerator = StartCoroutine(CrossFadeMusic(currentMap.defaultMusic));
        }

        /// <summary>
        /// Handles the crossfade between audio sources
        /// </summary>
        /// <param name="audioClip"></param>
        /// <returns></returns>
        private IEnumerator CrossFadeMusic(AudioClip audioClip)
        {
            // Get exposed audio volume from the Audi Mixer
            float audioSourceOneVol;
            audioMixer.GetFloat("MusicSourceOneVolume", out audioSourceOneVol);
            float audioSourceTwoVol;
            audioMixer.GetFloat("MusicSourceTwoVolume", out audioSourceTwoVol);
            // Decide which audio source is currently playing
            AudioSource tmpCurrentAudioSource = audioSourceOneVol == 0 ? audioSourceOne : audioSourceTwo;
            AudioSource tmpNextAudioSource = audioSourceOneVol == 0 ? audioSourceTwo : audioSourceOne;
            // Setup the next music clip to play
            tmpNextAudioSource.clip = audioClip;
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
                float tmpNewVolTwo = Mathf.Lerp(currentVolTwo, 1, currentTime / crossFadeSpeed);
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
        private AudioClip GetMapMusic(string reference)
        {
            foreach (MapInfoMusic m in currentMap.music)
                if (m.uniqueId == reference)
                    return m.audioClip;

            return null;
        }
        #endregion
    }
}