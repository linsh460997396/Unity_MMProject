using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NoitaCA
{
    public enum SpellElement
    {
        Fire,
        Frost,
        Poison
    }

    public sealed class SpellController : MonoBehaviour
    {
        [Header("Digging")]
        [SerializeField] private int digRadius = 5;
        [SerializeField] private float digCooldown = 0.12f;
        [SerializeField] private int maxDigRangeCells = 72;

        [Header("Spells")]
        [SerializeField] private int maxSpellRangeCells = 92;
        [SerializeField] private PixelAbility[] startingAbilities;

        private readonly List<PixelProjectile> projectiles = new List<PixelProjectile>(32);
        private readonly List<PixelAbility> abilities = new List<PixelAbility>(8);
        private readonly Dictionary<PixelAbility, Sprite> projectileSprites = new Dictionary<PixelAbility, Sprite>();
        private readonly List<Texture2D> generatedTextures = new List<Texture2D>(8);
        private PixelGrid grid;
        private PixelWorldRenderer worldRenderer;
        private Camera targetCamera;
        private Transform caster;
        private PlayerController casterPlayer;
        private int selectedAbilityIndex;
        private float digTimer;
        private float normalTimer;
        private float specialTimer;
        private float ultimateTimer;

        public PixelAbility SelectedAbility => abilities.Count > 0 ? abilities[Mathf.Clamp(selectedAbilityIndex, 0, abilities.Count - 1)] : null;

        public SpellElement SelectedElement
        {
            get
            {
                PixelAbility selected = SelectedAbility;
                if (selected is FrostPixelAbility)
                {
                    return SpellElement.Frost;
                }

                return selected is PoisonPixelAbility ? SpellElement.Poison : SpellElement.Fire;
            }
        }

        public void Initialize(PixelGrid pixelGrid, PixelWorldRenderer renderer, Camera cameraToUse, Transform casterTransform)
        {
            grid = pixelGrid;
            worldRenderer = renderer;
            targetCamera = cameraToUse;
            caster = casterTransform;
            casterPlayer = casterTransform != null ? casterTransform.GetComponent<PlayerController>() : null;
            BuildAbilityList();
        }

        public bool EquipAbility(PixelAbility ability)
        {
            if (ability == null)
            {
                return false;
            }

            int index = abilities.IndexOf(ability);
            if (index < 0)
            {
                abilities.Add(ability);
                index = abilities.Count - 1;
            }

            selectedAbilityIndex = index;
            EnsureProjectileSprite(ability);
            return true;
        }

        private void Update()
        {
            if (grid == null || worldRenderer == null || caster == null)
            {
                return;
            }

            digTimer = Mathf.Max(0f, digTimer - Time.deltaTime);
            normalTimer = Mathf.Max(0f, normalTimer - Time.deltaTime);
            specialTimer = Mathf.Max(0f, specialTimer - Time.deltaTime);
            ultimateTimer = Mathf.Max(0f, ultimateTimer - Time.deltaTime);

            HandleInput();
            StepProjectiles();
        }

        private void HandleInput()
        {
            if (WasCyclePressed() && abilities.Count > 0)
            {
                selectedAbilityIndex = (selectedAbilityIndex + 1) % abilities.Count;
            }

            if (IsSecondaryButtonPressed() && digTimer <= 0f)
            {
                DigAtPointer();
                digTimer = digCooldown;
            }

            PixelAbility ability = SelectedAbility;
            if (ability == null || !WasPrimaryButtonPressed())
            {
                return;
            }

            PixelAbilityContext context = BuildContext();
            if (IsPowerModifierHeld())
            {
                if (ultimateTimer <= 0f)
                {
                    TriggerCasterAttack();
                    ExecutePlayerSpellWrites(() => ability.CastUltimate(context));
                    ultimateTimer = ability.UltimateCooldown;
                }
            }
            else if (IsControlModifierHeld())
            {
                if (specialTimer <= 0f)
                {
                    TriggerCasterAttack();
                    ExecutePlayerSpellWrites(() => ability.CastSpecial(context));
                    specialTimer = ability.SpecialCooldown;
                }
            }
            else if (normalTimer <= 0f)
            {
                TriggerCasterAttack();
                int beforeCount = projectiles.Count;
                ExecutePlayerSpellWrites(() => ability.CastNormal(context, projectiles));
                for (int i = beforeCount; i < projectiles.Count; i++)
                {
                    AttachProjectileVisual(projectiles[i]);
                }

                normalTimer = ability.NormalCooldown;
            }
        }

        private void DigAtPointer()
        {
            if (!TryGetAimCell(maxDigRangeCells, out Vector2Int cell))
            {
                return;
            }

            int destroyed = grid.DestroyCircle(cell, digRadius);
            if (destroyed > 0)
            {
                grid.SpawnMaterial(cell, MaterialType.Debris, Mathf.Max(3, digRadius), digRadius + 1);
                TriggerCasterAttack();
            }
        }

        private void StepProjectiles()
        {
            PixelAbilityContext context = BuildContext();
            float cellWorldSize = 1f / Mathf.Max(1, worldRenderer.PixelsPerUnit);
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                PixelProjectile projectile = projectiles[i];
                projectile.TimeRemaining -= Time.deltaTime;
                projectile.Velocity += Vector3.up * projectile.Gravity * Time.deltaTime;
                Vector3 delta = projectile.Velocity * Time.deltaTime;
                int steps = Mathf.Max(1, Mathf.CeilToInt(delta.magnitude / (cellWorldSize * 0.45f)));
                bool impacted = false;

                for (int stepIndex = 0; stepIndex < steps; stepIndex++)
                {
                    projectile.Position += delta / steps;
                    Vector2Int cell = worldRenderer.WorldToCell(projectile.Position);

                    if (!grid.InBounds(cell.x, cell.y))
                    {
                        impacted = true;
                        break;
                    }

                    projectile.TrailTimer -= Time.deltaTime;
                    if (projectile.TrailTimer <= 0f && !grid.IsSolid(cell.x, cell.y))
                    {
                        ExecutePlayerSpellWrites(() => projectile.Ability.LeaveProjectileTrail(context, projectile, cell));
                    }

                    if (grid.IsSolid(cell.x, cell.y) || PixelCreatureRegistry.HasCreatureInCircle(cell, 2f))
                    {
                        ExecutePlayerSpellWrites(() => projectile.Ability.ResolveImpact(context, cell));
                        impacted = true;
                        break;
                    }
                }

                if (!impacted && projectile.TimeRemaining <= 0f)
                {
                    Vector2Int impactCell = worldRenderer.WorldToCell(projectile.Position);
                    ExecutePlayerSpellWrites(() => projectile.Ability.ResolveImpact(context, impactCell));
                    impacted = true;
                }

                if (projectile.Visual != null)
                {
                    projectile.Visual.transform.position = projectile.Position;
                }

                if (impacted)
                {
                    DestroyProjectileVisual(projectile);
                    projectiles.RemoveAt(i);
                }
            }
        }

        private void ExecutePlayerSpellWrites(System.Action action)
        {
            if (action == null)
            {
                return;
            }

            if (casterPlayer == null || grid == null)
            {
                action();
                return;
            }

            grid.BeginPlayerSpellWrites();
            try
            {
                action();
            }
            finally
            {
                grid.EndPlayerSpellWrites();
            }
        }

        private PixelAbilityContext BuildContext()
        {
            Vector3 aimWorld = TryGetAimCell(maxSpellRangeCells, out Vector2Int aimCell)
                ? worldRenderer.CellToWorldCenter(aimCell.x, aimCell.y)
                : GetPointerWorldPosition();
            Vector2Int originCell = worldRenderer.WorldToCell(caster.position);
            return new PixelAbilityContext
            {
                Grid = grid,
                Renderer = worldRenderer,
                Camera = targetCamera,
                Caster = caster,
                AimWorld = aimWorld,
                OriginCell = originCell,
                AimCell = aimCell
            };
        }

        private void BuildAbilityList()
        {
            abilities.Clear();
            if (startingAbilities != null)
            {
                for (int i = 0; i < startingAbilities.Length; i++)
                {
                    if (startingAbilities[i] != null && !abilities.Contains(startingAbilities[i]))
                    {
                        abilities.Add(startingAbilities[i]);
                    }
                }
            }

            if (abilities.Count == 0)
            {
                abilities.AddRange(PixelAbilitySet.CreateRuntimeDefaults());
            }

            selectedAbilityIndex = Mathf.Clamp(selectedAbilityIndex, 0, Mathf.Max(0, abilities.Count - 1));
            for (int i = 0; i < abilities.Count; i++)
            {
                EnsureProjectileSprite(abilities[i]);
            }
        }

        private void AttachProjectileVisual(PixelProjectile projectile)
        {
            if (projectile == null || projectile.Ability == null)
            {
                return;
            }

            GameObject projectileObject = new GameObject(projectile.Ability.DisplayName + " Projectile");
            projectileObject.transform.SetParent(transform.parent, true);
            SpriteRenderer renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = EnsureProjectileSprite(projectile.Ability);
            renderer.sortingOrder = 12;
            projectileObject.transform.position = projectile.Position;
            projectile.Visual = projectileObject;
        }

        private Sprite EnsureProjectileSprite(PixelAbility ability)
        {
            if (ability == null)
            {
                return null;
            }

            if (projectileSprites.TryGetValue(ability, out Sprite existing))
            {
                return existing;
            }

            Sprite sprite = ability.CreateProjectileSprite(worldRenderer != null ? worldRenderer.PixelsPerUnit : 16);
            projectileSprites[ability] = sprite;
            if (sprite != null && sprite.texture != null)
            {
                generatedTextures.Add(sprite.texture);
            }

            return sprite;
        }

        private bool TryGetAimCell(int maxRangeCells, out Vector2Int cell)
        {
            cell = worldRenderer.WorldToCell(GetPointerWorldPosition());
            Vector2Int origin = worldRenderer.WorldToCell(caster.position);
            Vector2 delta = cell - origin;
            if (delta.magnitude <= maxRangeCells)
            {
                return true;
            }

            delta = delta.normalized * maxRangeCells;
            cell = new Vector2Int(origin.x + Mathf.RoundToInt(delta.x), origin.y + Mathf.RoundToInt(delta.y));
            return grid.InBounds(cell.x, cell.y);
        }

        private Vector3 GetPointerWorldPosition()
        {
            Camera cameraToUse = targetCamera != null ? targetCamera : Camera.main;
            Vector3 screenPosition = ReadPointerScreenPosition();
            if (cameraToUse == null)
            {
                return caster.position + Vector3.right;
            }

            screenPosition.z = Mathf.Abs(cameraToUse.transform.position.z - worldRenderer.transform.position.z);
            Vector3 worldPosition = cameraToUse.ScreenToWorldPoint(screenPosition);
            worldPosition.z = caster.position.z;
            return worldPosition;
        }

        private void OnDestroy()
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                DestroyProjectileVisual(projectiles[i]);
            }

            foreach (KeyValuePair<PixelAbility, Sprite> pair in projectileSprites)
            {
                if (pair.Value != null)
                {
                    DestroyObject(pair.Value);
                }
            }

            for (int i = 0; i < generatedTextures.Count; i++)
            {
                if (generatedTextures[i] != null)
                {
                    DestroyObject(generatedTextures[i]);
                }
            }

            projectileSprites.Clear();
            generatedTextures.Clear();
        }

        private static void DestroyProjectileVisual(PixelProjectile projectile)
        {
            if (projectile != null && projectile.Visual != null)
            {
                Destroy(projectile.Visual);
                projectile.Visual = null;
            }
        }

        private static Vector3 ReadPointerScreenPosition()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                Vector2 position = Mouse.current.position.ReadValue();
                return new Vector3(position.x, position.y, 0f);
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.mousePosition;
#else
            return Vector3.zero;
#endif
        }

        private static bool WasPrimaryButtonPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.leftButton.wasPressedThisFrame;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetMouseButtonDown(0);
#else
            return false;
#endif
        }

        private static bool IsSecondaryButtonPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.rightButton.isPressed;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetMouseButton(1);
#else
            return false;
#endif
        }

        private static bool WasCyclePressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                return Keyboard.current.qKey.wasPressedThisFrame;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetKeyDown(KeyCode.Q);
#else
            return false;
#endif
        }

        private static bool IsControlModifierHeld()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                return Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.RightControl);
#else
            return false;
#endif
        }

        private static bool IsPowerModifierHeld()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                return Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift);
#else
            return false;
#endif
        }

        private void TriggerCasterAttack()
        {
            if (casterPlayer != null)
            {
                casterPlayer.TriggerAttackAnimation();
            }
        }

        private static void DestroyObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
