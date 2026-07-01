using UnityEngine;

namespace NoitaCA
{
    public static class PixelAbilitySet
    {
        public static PixelAbility[] CreateRuntimeDefaults()
        {
            return new PixelAbility[]
            {
                Create<FirePixelAbility>("Fire Staff"),
                Create<FrostPixelAbility>("Frost Staff"),
                Create<LightningPixelAbility>("Lightning Staff"),
                Create<PoisonPixelAbility>("Poison Staff"),
                Create<ExplosionPixelAbility>("Explosion Staff")
            };
        }

        private static T Create<T>(string abilityName) where T : PixelAbility
        {
            T ability = ScriptableObject.CreateInstance<T>();
            ability.name = abilityName;
            ability.UseRuntimeDefaults();
            ability.hideFlags = HideFlags.HideAndDontSave;
            return ability;
        }
    }
}
