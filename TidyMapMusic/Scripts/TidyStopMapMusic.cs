/* Copyright TidyDev Software Solutions LTD - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Ben Hamlett <ben.hamlett@tidydev.co>, 2022
 */

using UnityEngine;

namespace TidyDev
{
    public class TidyStopMapMusic : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            TidyAudioManager.Singleton.StopMapMusic();
        }
    }
}