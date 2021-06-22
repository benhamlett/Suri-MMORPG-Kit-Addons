using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class MapInfoMusic
    {
        public string uniqueId; // Used to reference the audio clip to play
        public AudioClip audioClip;
    }

    public partial class BaseMapInfo
    {
        [Header("Tidy Map Music")]
        public AudioClip defaultMusic;
        public List<MapInfoMusic> music = new List<MapInfoMusic>();
    }
}