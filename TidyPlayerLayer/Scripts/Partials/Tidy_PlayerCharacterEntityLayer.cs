/* Copyright TidyDev Software Solutions LTD - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Ben Hamlett <ben.hamlett@tidydev.co>, 2022
 */

using UnityEngine;

namespace MultiplayerARPG
{
    public partial class GameInstance : MonoBehaviour
    {
        /// <summary>
        /// Adds a new option to the GameInstance component to set unique player layer
        /// </summary>
        [Header("Local Player")]
        public UnityLayer playerLayer;
    }

    public partial class PlayerCharacterEntity
    {
        [DevExtMethods("Awake")]
        protected void TidyPlayerAwake()
        {
            onStart += UpdatePlayerLayers;
            onSelectableWeaponSetsOperation += UpdatePlayerWeaponLayers;
            onEquipItemsOperation += UpdatePlayerArmorLayers;
            
        }

        [DevExtMethods("OnDestroy")]
        protected void TidyPlayerOnDestroy()
        {
            onStart -= UpdatePlayerLayers;
            onSelectableWeaponSetsOperation -= UpdatePlayerWeaponLayers;
            onEquipItemsOperation -= UpdatePlayerArmorLayers;
        }

        /// <summary>
        /// Itterated through all child objects of the player
        /// if the object is on the characterLayer then sets it to the new playerLayer
        /// Usefull for cameras culling ONLY the local player etc
        /// </summary>
        private void UpdatePlayerLayers()
        {
            if (!IsOwnerClient)
                return;

            gameObject.tag = CurrentGameInstance.playerTag;
            //gameObject.layer = CurrentGameInstance.playerLayer;

            // Set child object layer if layer is Character
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                if(child.GetComponent<Renderer>())
                {
                    child.gameObject.layer = CurrentGameInstance.playerLayer;
                }
                /*if (child.gameObject.layer == CurrentGameInstance.characterLayer)
                {
                    child.gameObject.layer = CurrentGameInstance.playerLayer;
                }*/
            }
        }

        /// <summary>
        /// Updates layers on weapon changes only
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        private void UpdatePlayerWeaponLayers(LiteNetLibManager.LiteNetLibSyncList.Operation operation, int index)
        {
            Debug.Log("Weapon Change");
            UpdatePlayerLayers();
        }

        /// <summary>
        /// Updates layers on armor changes only
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        private void UpdatePlayerArmorLayers(LiteNetLibManager.LiteNetLibSyncList.Operation operation, int index)
        {
            Debug.Log("Armor Change");
            UpdatePlayerLayers();
        }
    }
}