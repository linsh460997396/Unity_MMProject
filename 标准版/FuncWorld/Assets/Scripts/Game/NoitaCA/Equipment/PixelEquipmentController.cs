using System.Collections.Generic;
using UnityEngine;

namespace NoitaCA
{
    public sealed class PixelEquipmentController : MonoBehaviour
    {
        private readonly List<PixelEquipmentDefinition> equipment = new List<PixelEquipmentDefinition>(8);
        private SpellController spellController;
        private PixelEquipmentDefinition equipped;

        public PixelEquipmentDefinition Equipped => equipped;
        public IReadOnlyList<PixelEquipmentDefinition> Equipment => equipment;

        public void Initialize(SpellController controller)
        {
            spellController = controller;
        }

        public bool Equip(PixelEquipmentDefinition definition)
        {
            if (definition == null || definition.Ability == null)
            {
                return false;
            }

            if (!equipment.Contains(definition))
            {
                equipment.Add(definition);
            }

            equipped = definition;
            if (spellController == null)
            {
                spellController = GetComponent<SpellController>();
            }

            return spellController != null && spellController.EquipAbility(definition.Ability);
        }
    }
}
