/* Copyright TidyDev Software Solutions LTD - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Ben Hamlett <ben.hamlett@tidydev.co>, 2022
 */

using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class MapInfoMusic
    {
        public string uniqueId; // Used to reference the audio clip to play
        public AudioClip audioClip;
        [Range(0, 1)]
        public float audioVolume = 0;
    }

    public partial class BaseMapInfo
    {
        [Header("Tidy Map Music")]
        public List<MapInfoMusic> defaultMusic = new List<MapInfoMusic>();
        public List<MapInfoMusic> music = new List<MapInfoMusic>();
    }
}