using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterEntity
    {
        /// <summary>
        /// Adds a new options to PlayerCharacterEntity for sheafing weapon/s
        /// </summary>
        [Header("Player Sheafing")]
        [SerializeField]
        protected SyncFieldBool isSheathed = new SyncFieldBool();
        private AnimatorCharacterModel animatorCharacterModel;
        IWeaponItem rightHandWeaponItem;
        IWeaponItem leftHandWeaponItem;
        IShieldItem leftHandShieldItem;

        public bool IsSheathed
        {
            get { return isSheathed.Value; }
            set { isSheathed.Value = value; }
        }

        [DevExtMethods("Awake")]
        protected void TidyPlayerSheathAwake()
        {
            onStart += TidyPlayerSheathInit;
            onUpdate += TidyPlayerSheathOnUpdate;
            onEquipWeaponSetChange += TidyUpdatePlayerWeaponItems;
            onSelectableWeaponSetsOperation += TidyUpdatePlayerWeaponItems;
            isSheathed.onChange += TidyOnIsSheathedChange;
        }

        [DevExtMethods("OnDestroy")]
        protected void TidyPlayerSheathOnDestroy()
        {
            onStart -= TidyPlayerSheathInit;
            onUpdate -= TidyPlayerSheathOnUpdate;
            onEquipWeaponSetChange -= TidyUpdatePlayerWeaponItems;
            onSelectableWeaponSetsOperation -= TidyUpdatePlayerWeaponItems;
            isSheathed.onChange -= TidyOnIsSheathedChange;
        }

        /// <summary>
        /// Override to stop actions if sheathed
        /// </summary>
        /// <returns></returns>
        public override bool CanDoActions()
        {
            return !this.IsDead() && !IsAttackingOrUsingSkill && !IsReloading && !IsPlayingActionAnimation() && !IsSheathed;
        }

        /// <summary>
        /// Gather required components and run initial functions
        /// </summary>
        protected void TidyPlayerSheathInit()
        {
            animatorCharacterModel = GetComponent<AnimatorCharacterModel>();
            TidySetupSheathEquipWeapons(GameInstance.PlayingCharacterEntity.EquipWeapons);
            RegisterNetFunction<uint>(Cmd_TidySheathWeapons);
            RegisterNetFunction<uint>(Cmd_TidyUnsheathWeapons);
        }

        /// <summary>
        /// Updates layers on weapon changes only
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        private void TidyUpdatePlayerWeaponItems(byte equipWeaponSet)
        {
            if (!IsOwnerClient)
                return;

            if (!GameInstance.PlayingCharacterEntity)
                return;

            if (IsSheathed)
            {
                CallNetFunction(Cmd_TidySheathWeapons, FunctionReceivers.All, ObjectId);
            }
            else
            {
                CallNetFunction(Cmd_TidyUnsheathWeapons, FunctionReceivers.All, ObjectId);
            }
            Debug.Log("Sheath Wep Set Change: " + isSheathed);
        }

        private void TidyUpdatePlayerWeaponItems(LiteNetLibSyncList.Operation operation, int index)
        {
            if (!IsOwnerClient)
                return;

            if (!GameInstance.PlayingCharacterEntity || !animatorCharacterModel)
                return;

            if (IsSheathed)
            {
                CallNetFunction(Cmd_TidySheathWeapons, FunctionReceivers.All, ObjectId);
            }
            else
            {
                CallNetFunction(Cmd_TidyUnsheathWeapons, FunctionReceivers.All, ObjectId);
            }
            Debug.Log("Sheath Wep Set Change: " + isSheathed);
        }

        /// <summary>
        /// Gets the current players equiped weapon/s data and instatiated gameobject/s
        /// </summary>
        /// <param name="equipWeapons"></param>
        private void TidySetupSheathEquipWeapons(EquipWeapons equipWeapons)
        {
            rightHandWeaponItem = equipWeapons.GetRightHandWeaponItem();
            leftHandWeaponItem = equipWeapons.GetLeftHandWeaponItem();
            leftHandShieldItem = equipWeapons.GetLeftHandShieldItem();
        }

        /// <summary>
        /// Checks for Sheath key / button input
        /// </summary>
        protected void TidyPlayerSheathOnUpdate()
        {
            if (!IsOwnerClient || !animatorCharacterModel)
                return;

            if (InputManager.GetButtonDown("Sheath"))
            {
                if (!IsSheathed)
                {
                    CallNetFunction(Cmd_TidySheathWeapons, FunctionReceivers.All, ObjectId);
                    IsSheathed = true;
                }
                else
                {
                    CallNetFunction(Cmd_TidyUnsheathWeapons, FunctionReceivers.All, ObjectId);
                    IsSheathed = false;
                }
            }
        }

        /// <summary>
        /// Changes the transform of the players equiped weapon/s to the desired sheath position
        /// </summary>
        public void TidySheathWeapons(uint playerObjectId)
        {
            PlayerCharacterEntity player;
            if (!Manager.TryGetEntityByObjectId(playerObjectId, out player))
            {
                Debug.LogError("Sheath No Player Found");
                return;
            }

            TidySetupSheathEquipWeapons(player.EquipWeapons);

            if (player.rightHandWeaponItem != null)
            {
                foreach (EquipmentModel em in player.rightHandWeaponItem.EquipmentModels)
                {
                    Transform tmpWepParent = TidyGetContainerTransform(player.transform, em.sheafSocket);
                    string tmpWepName = em.useInstantiatedObject ? em.model.name : em.model.name + "(Clone)";
                    GameObject tmpWepGo = TidyGetWeaponObject(player.transform, tmpWepName);
                    tmpWepGo.transform.SetParent(tmpWepParent);
                    tmpWepGo.transform.localPosition = em.sheafLocalPosition;
                    tmpWepGo.transform.localEulerAngles = em.sheafLocalEulerAngles;
                }
            }
            if (player.leftHandWeaponItem != null)
            {
                foreach (EquipmentModel em in player.leftHandWeaponItem.EquipmentModels)
                {
                    Transform tmpWepParent = TidyGetContainerTransform(player.transform, em.sheafSocket);
                    string tmpWepName = em.useInstantiatedObject ? em.model.name : em.model.name + "(Clone)";
                    GameObject tmpWepGo = TidyGetWeaponObject(player.transform, tmpWepName);
                    tmpWepGo.transform.SetParent(tmpWepParent);
                    tmpWepGo.transform.localPosition = em.sheafLocalPosition;
                    tmpWepGo.transform.localEulerAngles = em.sheafLocalEulerAngles;
                }
            }
            if (player.leftHandShieldItem != null)
            {
                foreach (EquipmentModel em in player.leftHandShieldItem.EquipmentModels)
                {
                    Transform tmpWepParent = TidyGetContainerTransform(player.transform, em.sheafSocket);
                    string tmpWepName = em.useInstantiatedObject ? em.model.name : em.model.name + "(Clone)";
                    GameObject tmpWepGo = TidyGetWeaponObject(player.transform, tmpWepName);
                    tmpWepGo.transform.SetParent(tmpWepParent);
                    tmpWepGo.transform.localPosition = em.sheafLocalPosition;
                    tmpWepGo.transform.localEulerAngles = em.sheafLocalEulerAngles;
                }
            }
        }

        private void Cmd_TidySheathWeapons(uint playerObjectId)
        {
            TidySheathWeapons(playerObjectId);
        }

        /// <summary>
        /// Changes the transform of the players equiped weapon/s to the desired unsheath position
        /// </summary>
        public void TidyUnsheathWeapons(uint playerObjectId)
        {
            PlayerCharacterEntity player;
            if (!Manager.TryGetEntityByObjectId(playerObjectId, out player))
            {
                Debug.LogError("Sheath No Player Found");
                return;
            }

            TidySetupSheathEquipWeapons(player.EquipWeapons);

            if (rightHandWeaponItem != null)
            {
                foreach (EquipmentModel em in rightHandWeaponItem.EquipmentModels)
                {
                    Transform tmpWepParent = TidyGetContainerTransform(player.transform, em.equipSocket);
                    string tmpWepName = em.useInstantiatedObject ? em.model.name : em.model.name + "(Clone)";
                    GameObject tmpWepGo = TidyGetWeaponObject(player.transform, tmpWepName);
                    tmpWepGo.transform.SetParent(tmpWepParent);
                    tmpWepGo.transform.localPosition = em.localPosition;
                    tmpWepGo.transform.localEulerAngles = em.localEulerAngles;
                }
            }
            if (leftHandWeaponItem != null)
            {
                foreach (EquipmentModel em in leftHandWeaponItem.EquipmentModels)
                {
                    Transform tmpWepParent = TidyGetContainerTransform(player.transform, em.equipSocket);
                    string tmpWepName = em.useInstantiatedObject ? em.model.name : em.model.name + "(Clone)";
                    GameObject tmpWepGo = TidyGetWeaponObject(player.transform, tmpWepName);
                    tmpWepGo.transform.SetParent(tmpWepParent);
                    tmpWepGo.transform.localPosition = em.localPosition;
                    tmpWepGo.transform.localEulerAngles = em.localEulerAngles;
                }
            }
            if (leftHandShieldItem != null)
            {
                foreach (EquipmentModel em in leftHandShieldItem.EquipmentModels)
                {
                    Transform tmpWepParent = TidyGetContainerTransform(player.transform, em.equipSocket);
                    string tmpWepName = em.useInstantiatedObject ? em.model.name : em.model.name + "(Clone)";
                    GameObject tmpWepGo = TidyGetWeaponObject(player.transform, tmpWepName);
                    tmpWepGo.transform.SetParent(tmpWepParent);
                    tmpWepGo.transform.localPosition = em.localPosition;
                    tmpWepGo.transform.localEulerAngles = em.localEulerAngles;
                }
            }
        }

        private void Cmd_TidyUnsheathWeapons(uint playerObjectId)
        {
            TidyUnsheathWeapons(playerObjectId);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="isInitial"></param>
        /// <param name="isOpen"></param>
        private void TidyOnIsSheathedChange(bool isInitial, bool isOpen)
        {
            Debug.Log("Sheath Change");
        }

        /// <summary>
        /// Returns the transform of a defined Equipment Container players AnimatorCharacterModel based on the socketName
        /// </summary>
        /// <param name="socketName"></param>
        /// <returns></returns>
        private Transform TidyGetContainerTransform(Transform player, string socketName)
        {
            if (!animatorCharacterModel)
                return null;

            foreach (EquipmentContainer c in animatorCharacterModel.EquipmentContainers)
            {
                if (c.equipSocket == socketName)
                {
                    return c.transform;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds a child gameobject based on weaponName
        /// </summary>
        /// <param name="weaponName"></param>
        /// <returns></returns>
        private GameObject TidyGetWeaponObject(Transform player, string weaponName)
        {
            foreach (Transform child in player.GetComponentsInChildren<Transform>())
            {
                if (child.name == weaponName)
                {
                    return child.gameObject;
                }
            }

            return null;
        }
    }
}
