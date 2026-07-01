using UnityEngine;

namespace NoitaCA
{
    [CreateAssetMenu(menuName = "NoitaCA/Pixel Equipment", fileName = "Pixel Equipment")]
    public sealed class PixelEquipmentDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Pixel Staff";
        [SerializeField] private PixelAbility ability;
        [SerializeField] private Color32 pickupColor = new Color32(236, 198, 92, 255);
        [SerializeField] private float pickupRadius = 0.55f;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public PixelAbility Ability => ability;
        public Color32 PickupColor => pickupColor;
        public float PickupRadius => Mathf.Max(0.05f, pickupRadius);

        public static PixelEquipmentDefinition CreateRuntime(string equipmentName, PixelAbility equipmentAbility)
        {
            PixelEquipmentDefinition definition = CreateInstance<PixelEquipmentDefinition>();
            definition.name = equipmentName;
            definition.displayName = equipmentName;
            definition.ability = equipmentAbility;
            definition.pickupColor = equipmentAbility != null ? equipmentAbility.PrimaryColor : new Color32(236, 198, 92, 255);
            definition.hideFlags = HideFlags.HideAndDontSave;
            return definition;
        }
    }
}
