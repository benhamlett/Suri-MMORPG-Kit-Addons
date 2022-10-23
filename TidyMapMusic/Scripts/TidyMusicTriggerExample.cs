/* Copyright TidyDev Software Solutions LTD - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Ben Hamlett <ben.hamlett@tidydev.co>, 2022
 */

using UnityEngine;

namespace TidyDev
{
    public class TidyMusicTriggerExample : MonoBehaviour
    {
        public string uniqueId;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "PlayerTag")
            {
                if (uniqueId.Length > 0)
                    TidyAudioManager.Singleton.PlayMapMusic(uniqueId);
                else
                    TidyAudioManager.Singleton.StopMapMusic();
            }
        }
    }
}