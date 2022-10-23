/* Copyright TidyDev Software Solutions LTD - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Ben Hamlett <ben.hamlett@tidydev.co>, 2022
 * -- Thanks to Shiroyasha
 */

using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UINetworkSceneLoading : MonoBehaviour
    {
        public delegate void OnLoadSceneStartEvent(string sceneName, bool isOnline, float progress);
        public static event OnLoadSceneStartEvent OnLoadSceneStartInit;
    }
}
