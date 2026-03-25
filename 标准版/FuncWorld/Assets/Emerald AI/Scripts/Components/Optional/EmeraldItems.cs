using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/items-component")]
    public class EmeraldItems : MonoBehaviour
    {
        #region Items Variables
        public YesOrNo UseDroppableWeapon = YesOrNo.No;

        [SerializeField]
        public List<EquippableWeapons> Type1EquippableWeapons = new List<EquippableWeapons>();
        [SerializeField]
        public List<EquippableWeapons> Type2EquippableWeapons = new List<EquippableWeapons>();
        [System.Serializable]
        public class EquippableWeapons
        {
            public bool HeldToggle;
            public GameObject HeldObject;
            public bool HolsteredToggle;
            public GameObject HolsteredObject;
            public bool DroppableToggle;
            public GameObject DroppableObject;
        }

        public delegate void OnEquipWeaponHandler(string WeaponType);
        public event OnEquipWeaponHandler OnEquipWeapon;
        public delegate void OnUnequipWeaponHandler(string WeaponType);
        public event OnUnequipWeaponHandler OnUnequipWeapon;

        [System.Serializable]
        public class ItemClass
        {
            public int ItemID = 1;
            public GameObject ItemObject;
        }
        [SerializeField]
        public List<ItemClass> ItemList = new List<ItemClass>();
        #endregion

        #region Editor Variables
        public bool HideSettingsFoldout;
        public bool WeaponsFoldout;
        public bool ItemsFoldout;
        #endregion

        void Start()
        {
            InitializeDroppableWeapon();
        }

        /// <summary>
        /// Intializes the AI's droppable weapon to be used when the AI dies.
        /// </summary>
        public void InitializeDroppableWeapon()
        {
            GetComponent<EmeraldHealth>().OnDeath += CreateDroppableWeapon; //Subscribe to the OnDeath event for when dropping an AI's weapon on death, given that it's enabled.
        }

        /// <summary>
        /// Instantiates an AI's Droppable Weapon Object on death. 
        /// </summary>
        public void CreateDroppableWeapon()
        {
            EmeraldSystem EmeraldComponent = GetComponent<EmeraldSystem>();

            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1)
            {
                for (int i = 0; i < Type1EquippableWeapons.Count; i++)
                {
                    if (Type1EquippableWeapons[i].DroppableObject != null && Type1EquippableWeapons[i].DroppableToggle)
                    {
                        if (Type1EquippableWeapons[i].HeldObject != null)
                        {
                            //Instantiate a copy of the DroppableObject and position it to HeldObject
                            var DroppableMeleeWeapon = EmeraldObjectPool.Spawn(Type1EquippableWeapons[i].DroppableObject, Type1EquippableWeapons[i].HeldObject.transform.position, Type1EquippableWeapons[i].HeldObject.transform.rotation);
                            DroppableMeleeWeapon.transform.localScale = Type1EquippableWeapons[i].HeldObject.transform.lossyScale;
                            DroppableMeleeWeapon.transform.SetParent(Type1EquippableWeapons[i].HeldObject.transform.parent);
                            DroppableMeleeWeapon.transform.localPosition = Type1EquippableWeapons[i].HeldObject.transform.localPosition;
                            DroppableMeleeWeapon.gameObject.name = Type1EquippableWeapons[i].HeldObject.gameObject.name + " (Droppable Copy)";
                            DroppableMeleeWeapon.transform.SetParent(EmeraldSystem.ObjectPool.transform);

                            //Check for a collider on the WeaponObject, if there isn'transform one, add one. 
                            if (DroppableMeleeWeapon.GetComponent<Collider>() == null)
                                DroppableMeleeWeapon.AddComponent<BoxCollider>();

                            if (DroppableMeleeWeapon.GetComponent<Rigidbody>() == null)
                                DroppableMeleeWeapon.AddComponent<Rigidbody>();

                            //Apply the AI's current velocity to the weapon object.
                            Rigidbody WeaponRigidbody = DroppableMeleeWeapon.GetComponent<Rigidbody>();
                            WeaponRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

                            //Used to provide the force to the AI's weapon in the opposite direction of the last target to hit the AI.
                            Transform LastAttacker = EmeraldComponent.CombatComponent.LastAttacker;

                            if (EmeraldComponent.CombatTarget != null)
                            {
                                if (LastAttacker != null && LastAttacker != EmeraldComponent.CombatTarget)
                                    WeaponRigidbody.AddForce((LastAttacker.position - transform.position).normalized * EmeraldComponent.CombatComponent.ReceivedRagdollForceAmount * 0.01f, ForceMode.Impulse);
                                else
                                    WeaponRigidbody.AddForce((EmeraldComponent.CombatTarget.position - transform.position).normalized * EmeraldComponent.CombatComponent.ReceivedRagdollForceAmount * 0.01f, ForceMode.Impulse);
                            }

                            //Disable the held object
                            Type1EquippableWeapons[i].HeldObject.SetActive(false);
                        }
                        else
                        {
                            Debug.LogError("The AI '" + gameObject.name + "' does not have a held object for the '" + Type1EquippableWeapons[i].DroppableObject.name + "' Droppable Object (Type 1) with the Items Component, please assign one.");
                        }
                    }
                }

                for (int i = 0; i < Type2EquippableWeapons.Count; i++)
                {
                    if (Type2EquippableWeapons[i].DroppableObject != null && Type2EquippableWeapons[i].DroppableToggle)
                    {
                        if (Type2EquippableWeapons[i].HeldObject != null)
                        {
                            //Instantiate a copy of the DroppableObject and position it to HeldObject
                            var DroppableMeleeWeapon = EmeraldObjectPool.Spawn(Type2EquippableWeapons[i].DroppableObject, Type2EquippableWeapons[i].HeldObject.transform.position, Type2EquippableWeapons[i].HeldObject.transform.rotation);
                            DroppableMeleeWeapon.transform.localScale = Type2EquippableWeapons[i].HeldObject.transform.lossyScale;
                            DroppableMeleeWeapon.transform.SetParent(Type2EquippableWeapons[i].HeldObject.transform.parent);
                            DroppableMeleeWeapon.transform.localPosition = Type2EquippableWeapons[i].HeldObject.transform.localPosition;
                            DroppableMeleeWeapon.gameObject.name = Type2EquippableWeapons[i].HeldObject.gameObject.name + " (Droppable Copy)";
                            DroppableMeleeWeapon.transform.SetParent(EmeraldSystem.ObjectPool.transform);

                            //Check for a collider on the WeaponObject, if there isn'transform one, add one. 
                            if (DroppableMeleeWeapon.GetComponent<Collider>() == null)
                                DroppableMeleeWeapon.AddComponent<BoxCollider>();

                            if (DroppableMeleeWeapon.GetComponent<Rigidbody>() == null)
                                DroppableMeleeWeapon.AddComponent<Rigidbody>();

                            //Apply the AI's current velocity to the weapon object.
                            Rigidbody WeaponRigidbody = DroppableMeleeWeapon.GetComponent<Rigidbody>();
                            WeaponRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

                            //Used to provide the force to the AI's weapon in the opposite direction of the last target to hit the AI.
                            Transform LastAttacker = EmeraldComponent.CombatComponent.LastAttacker;

                            if (EmeraldComponent.CombatTarget != null)
                            {
                                if (LastAttacker != null && LastAttacker != EmeraldComponent.CombatTarget)
                                    WeaponRigidbody.AddForce((LastAttacker.position - transform.position).normalized * -EmeraldComponent.CombatComponent.ReceivedRagdollForceAmount, ForceMode.Impulse);
                                else
                                    WeaponRigidbody.AddForce((EmeraldComponent.CombatTarget.position - transform.position).normalized * -EmeraldComponent.CombatComponent.ReceivedRagdollForceAmount, ForceMode.Impulse);
                            }

                            //Disable the held object
                            Type2EquippableWeapons[i].HeldObject.SetActive(false);
                        }
                        else
                        {
                            Debug.LogError("The AI '" + gameObject.name + "' does not have a held object for the '" + Type2EquippableWeapons[i].DroppableObject.name + "' Droppable Object (Type 2) with the Items Component, please assign one.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Enables the AI's weapon object and plays the AI's equip sound effect, if one is applied.
        /// </summary>
        public void EquipWeapon(string WeaponTypeToEnable)
        {
            EmeraldSystem EmeraldComponent = GetComponent<EmeraldSystem>();

            if (WeaponTypeToEnable == "Weapon Type 1")
            {
                for (int i = 0; i < Type1EquippableWeapons.Count; i++)
                {
                    if (!Type1EquippableWeapons[i].HolsteredToggle) return;

                    if (Type1EquippableWeapons[i].HeldObject != null) Type1EquippableWeapons[i].HeldObject.SetActive(true); //Enable the held weapon
                    if (Type1EquippableWeapons[i].HolsteredObject != null) Type1EquippableWeapons[i].HolsteredObject.SetActive(false); //Disable the holstered weapon
                }

                OnEquipWeapon?.Invoke(WeaponTypeToEnable); //Invoke the OnEquipWeapon event.
            }
            else if (WeaponTypeToEnable == "Weapon Type 2")
            {
                for (int i = 0; i < Type2EquippableWeapons.Count; i++)
                {
                    if (!Type2EquippableWeapons[i].HolsteredToggle) return;

                    if (Type2EquippableWeapons[i].HeldObject != null) Type2EquippableWeapons[i].HeldObject.SetActive(true); //Enable the held weapon
                    if (Type2EquippableWeapons[i].HolsteredObject != null) Type2EquippableWeapons[i].HolsteredObject.SetActive(false); //Disable the holstered weapon
                }

                OnEquipWeapon?.Invoke(WeaponTypeToEnable); //Invoke the OnEquipWeapon event.
            }
            else
            {
                Debug.Log("This string withing the EquipWeapon Animation Event is blank or incorrect. Ensure that it's either Weapon Type 1 or Weapon Type 2.");
            }
        }

        /// <summary>
        /// Disables the AI's weapon object and plays the AI's unequip sound effect, if one is applied.
        /// </summary>
        public void UnequipWeapon(string WeaponTypeToDisable)
        {
            EmeraldSystem EmeraldComponent = GetComponent<EmeraldSystem>();

            if (WeaponTypeToDisable == "Weapon Type 1")
            {
                for (int i = 0; i < Type1EquippableWeapons.Count; i++)
                {
                    if (!Type1EquippableWeapons[i].HolsteredToggle) return;

                    if (Type1EquippableWeapons[i].HeldObject != null) Type1EquippableWeapons[i].HeldObject.SetActive(false); //Disable the held weapon
                    if (Type1EquippableWeapons[i].HolsteredObject != null) Type1EquippableWeapons[i].HolsteredObject.SetActive(true); //Enable the holstered weapon
                }

                OnUnequipWeapon?.Invoke(WeaponTypeToDisable); //Invoke the OnUnequipWeapon event.
            }
            else if (WeaponTypeToDisable == "Weapon Type 2")
            {
                for (int i = 0; i < Type2EquippableWeapons.Count; i++)
                {
                    if (!Type2EquippableWeapons[i].HolsteredToggle) return;

                    if (Type2EquippableWeapons[i].HeldObject != null) Type2EquippableWeapons[i].HeldObject.SetActive(false); //Disable the held weapon
                    if (Type2EquippableWeapons[i].HolsteredObject != null) Type2EquippableWeapons[i].HolsteredObject.SetActive(true); //Enable the holstered weapon
                }

                OnUnequipWeapon?.Invoke(WeaponTypeToDisable); //Invoke the OnUnequipWeapon event.
            }
            else
            {
                Debug.Log("This string withing the UnequipWeapon Animation Event is blank or incorrect. Ensure that it's either Type 2 or Type 1.");
            }
        }

        /// <summary>
        /// Enables an item from your AI's Item list using the Item ID.
        /// </summary>
        public void EnableItem(int ItemID)
        {
            //Look through each item in the ItemList for the appropriate ID.
            //Once found, enable the item of the same index as the found ID.
            for (int i = 0; i < ItemList.Count; i++)
            {
                if (ItemList[i].ItemID == ItemID)
                {
                    if (ItemList[i].ItemObject != null)
                    {
                        ItemList[i].ItemObject.SetActive(true);
                    }
                }
            }
        }

        /// <summary>
        /// Disables an item from your AI's Item list using the Item ID.
        /// </summary>
        public void DisableItem(int ItemID)
        {
            //Look through each item in the ItemList for the appropriate ID.
            //Once found, enable the item of the same index as the found ID.
            for (int i = 0; i < ItemList.Count; i++)
            {
                if (ItemList[i].ItemID == ItemID)
                {
                    if (ItemList[i].ItemObject != null)
                    {
                        ItemList[i].ItemObject.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// Disables all items from your AI's Item list.
        /// </summary>
        public void DisableAllItems()
        {
            //Disable all of an AI's items
            for (int i = 0; i < ItemList.Count; i++)
            {
                if (ItemList[i].ItemObject != null)
                {
                    ItemList[i].ItemObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Reset any equipped items to their defaults.
        /// </summary>
        public void ResetSettings()
        {
            EmeraldSystem EmeraldComponent = GetComponent<EmeraldSystem>();

            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1)
            {
                for (int i = 0; i < Type1EquippableWeapons.Count; i++)
                {
                    if (EmeraldComponent.AnimationComponent.m_AnimationProfile.Type1Animations.PullOutWeapon.AnimationClip != null && EmeraldComponent.AnimationComponent.m_AnimationProfile.Type1Animations.PutAwayWeapon.AnimationClip != null)
                    {
                        if (Type1EquippableWeapons[i].HeldObject != null && Type1EquippableWeapons[i].HolsteredObject != null)
                        {
                            Type1EquippableWeapons[i].HeldObject.SetActive(false); //Disable the held weapon
                            Type1EquippableWeapons[i].HolsteredObject.SetActive(true); //Enable the holstered weapon
                        }
                        else
                        {
                            if (Type1EquippableWeapons[i].HeldObject) Type1EquippableWeapons[i].HeldObject.SetActive(true); //Enable the held weapon
                        }
                    }
                    else
                    {
                        if (Type1EquippableWeapons[i].HeldObject) Type1EquippableWeapons[i].HeldObject.SetActive(true); //Enable the held weapon
                    }
                }
            }
            else if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2)
            {
                for (int i = 0; i < Type2EquippableWeapons.Count; i++)
                {
                    if (EmeraldComponent.AnimationComponent.m_AnimationProfile.Type2Animations.PullOutWeapon.AnimationClip != null && EmeraldComponent.AnimationComponent.m_AnimationProfile.Type2Animations.PutAwayWeapon.AnimationClip != null)
                    {
                        if (Type2EquippableWeapons[i].HeldObject != null && Type2EquippableWeapons[i].HolsteredObject != null)
                        {
                            Type2EquippableWeapons[i].HeldObject.SetActive(false); //Disable the held weapon
                            Type2EquippableWeapons[i].HolsteredObject.SetActive(true); //Enable the holstered weapon
                        }
                        else
                        {
                            if (Type2EquippableWeapons[i].HeldObject) Type2EquippableWeapons[i].HeldObject.SetActive(true); //Enable the held weapon
                        }
                    }
                    else
                    {
                        if (Type2EquippableWeapons[i].HeldObject) Type2EquippableWeapons[i].HeldObject.SetActive(true); //Enable the held weapon
                    }
                }
            }
        }

        public void EnableWeaponCollider(string Name)
        {
            EmeraldSystem EmeraldComponent = GetComponent<EmeraldSystem>();

            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1)
            {
                for (int i = 0; i < Type1EquippableWeapons.Count; i++)
                {
                    if (Type1EquippableWeapons[i].HeldObject.GetComponent<EmeraldWeaponCollision>() != null)
                    {
                        Type1EquippableWeapons[i].HeldObject.GetComponent<EmeraldWeaponCollision>().EnableWeaponCollider(Name);
                    }
                }
            }
            else if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2)
            {
                for (int i = 0; i < Type2EquippableWeapons.Count; i++)
                {
                    if (Type2EquippableWeapons[i].HeldObject.GetComponent<EmeraldWeaponCollision>() != null)
                    {
                        Type2EquippableWeapons[i].HeldObject.GetComponent<EmeraldWeaponCollision>().EnableWeaponCollider(Name);
                    }
                }
            }
        }

        public void DisableWeaponCollider(string Name)
        {
            EmeraldSystem EmeraldComponent = GetComponent<EmeraldSystem>();

            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1)
            {
                for (int i = 0; i < Type1EquippableWeapons.Count; i++)
                {
                    if (Type1EquippableWeapons[i].HeldObject.GetComponent<EmeraldWeaponCollision>() != null)
                    {
                        Type1EquippableWeapons[i].HeldObject.GetComponent<EmeraldWeaponCollision>().DisableWeaponCollider(Name);
                    }
                }
            }
            else if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2)
            {
                for (int i = 0; i < Type2EquippableWeapons.Count; i++)
                {
                    if (Type2EquippableWeapons[i].HeldObject.GetComponent<EmeraldWeaponCollision>() != null)
                    {
                        Type2EquippableWeapons[i].HeldObject.GetComponent<EmeraldWeaponCollision>().DisableWeaponCollider(Name);
                    }
                }
            }
        }
    }
}