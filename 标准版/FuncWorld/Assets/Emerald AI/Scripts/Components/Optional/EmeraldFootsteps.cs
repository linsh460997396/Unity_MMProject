using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using EmeraldAI;

namespace EmeraldAI
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/footsteps-component")]
    public class EmeraldFootsteps : MonoBehaviour
    {
        #region Variables
        public LayerMask DetectableLayers;
        public LayerMask IgnoreLayers;
        public List<Transform> FeetTransforms = new List<Transform>();
        public List<FootstepSurfaceObject> FootstepSurfaces = new List<FootstepSurfaceObject>();
        public bool HideSettingsFoldout;
        public bool FootstepsFoldout;
        public bool SurfaceFoldout;
        EmeraldSystem EmeraldComponent;
        LayerMask InternalIgnoreLayers;
        Transform CurrentFoot;
        Transform LastFoot;
        Transform LastFoot2;
        float TimeStamp;
        #endregion

        void Start()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>();
            Invoke(nameof(SetupIgnoreLayers), 0.1f);
            if (FeetTransforms.Count == 0) Debug.LogError("The '" + gameObject.name + "' does not have any Feet Transforms. Please assign some in order to use the Footsteps Component.");
        }

        /// <summary>
        /// Automatically add an AI's own layer and its LBD's Collider Layer to the IgnoreLayers layermask.
        /// </summary>
        void SetupIgnoreLayers ()
        {
            InternalIgnoreLayers = IgnoreLayers;

            LayerMask LBDLayers = 0;

            //Add the AI's LBD Collider Layer, if the AI is using a LBDComponent.
            if (EmeraldComponent.LBDComponent != null)
            {
                LBDLayers |= (1 << EmeraldComponent.LBDComponent.LBDComponentsLayer);
            }

            //Add the AI's own layer to the 
            InternalIgnoreLayers |= (1 << EmeraldComponent.gameObject.layer);

            for (int i = 0; i < 32; i++)
            {
                if (LBDLayers == (LBDLayers | (1 << i)))
                {
                    InternalIgnoreLayers |= (1 << i);
                }
            }

            //Update the IgnoreLayers with the addition of the automatically included layers.
            IgnoreLayers = InternalIgnoreLayers;
        }

        /// <summary>
        /// A universal function for creating a footstep.
        /// </summary>
        public void Footstep()
        {
            CreateFootstep();
        }

        /// <summary>
        /// The original function used for walk footstep sounds. This is used to allow AI who previously used this event 
        /// to work without having to redo all footstep sounds. Keep in mind, this function will eventually be deprecated in future versions. 
        /// It's best to use the Footstep function, which works universally for both walk and run (as well as any other animations you want footsteps for).
        /// </summary>
        public void WalkFootstepSound()
        {
            CreateFootstep();
        }

        /// <summary>
        /// The original function used for walk footstep sounds. This is used to allow AI who previously used this event 
        /// to work without having to redo all footstep sounds. Keep in mind, this function will eventually be deprecated in future versions. 
        /// It's best to use the Footstep function, which works universally for both walk and run (as well as any other animations you want footsteps for).
        /// </summary>
        public void RunFootstepSound()
        {
            CreateFootstep();
        }

        /// <summary>
        /// Creates a footstep by calculating the lowest footstep at the time a Footstep event is called through an Animation Event.
        /// </summary>
        void CreateFootstep()
        {
            //Return if any of these conditions are met.
            if (EmeraldComponent.AnimationComponent.IsIdling || Time.time < (TimeStamp + 0.25f) || EmeraldComponent.AnimationComponent.BusyBetweenStates || FeetTransforms.Count == 0) return;

            TimeStamp = Time.time; //Time stamp the last footstep and ensure at least 1/4 a second passes before creating a new one (as Animation Events can sometimes fire simultaneously if they are between states).

            //Get the lowest footstep at the time a Footstep event is called through an Animation Event.
            CalculateLowestFoot();

            if (CurrentFoot != null)
            {
                //Fire a raycast down from the lowest foot transform (ignoring any layers from the IgnoreLayers variable).
                RaycastHit hit;
                if (Physics.Raycast(CurrentFoot.position, Vector3.up * -0.25f, out hit, 1f, ~IgnoreLayers))
                {
                    if (hit.collider != null)
                    {
                        //Get the tag of detected raycast collider.
                        var StepData = FootstepSurfaces.Find(step => step.SurfaceType == FootstepSurfaceObject.SurfaceTypes.Tag && hit.collider.CompareTag(step.SurfaceTag));

                        //If the StepData is null, check for a terrain. If a terrain is found, get the most dominant texture for the footstep's position.
                        //Use this texture to determine the Surface Data that should be used for this footstep.
                        if (StepData == null)
                        {
                            Terrain CurrentTerrain = hit.collider.GetComponent<Terrain>();
                            if (CurrentTerrain != null)
                            {
                                Texture CurrentTexture = GetTerrainTexture(transform.position, CurrentTerrain);
                                StepData = FootstepSurfaces.Find(step => step.SurfaceType == FootstepSurfaceObject.SurfaceTypes.Texture && step.SurfaceTextures.Contains(CurrentTexture));
                            }
                        }

                        //Debug Settings Only
                        if (EmeraldComponent.DebuggerComponent != null && EmeraldComponent.DebuggerComponent.DrawFootstepPositions == YesOrNo.Yes)
                        {
                            Debug.DrawLine(CurrentFoot.position, hit.point, Color.yellow, 6);
                            DrawCircle(hit.point + Vector3.up * 0.05f, 0.25f, Color.yellow);
                        }

                        //Play a footstep sound and effect using the current StepData, given they aren'transform null.
                        if (StepData != null)
                        {
                            //Debug Settings Only
                            if (EmeraldComponent.DebuggerComponent != null && EmeraldComponent.DebuggerComponent.DebugLogFootsteps == YesOrNo.Yes)
                            {
                                Debug.Log("The <b><color=green>" + gameObject.name + "</color></b> footstep collided with <b><color=green>" + hit.collider.name + "</color></b> and used the <b><color=green>" + StepData.name + "</color></b> Footstep Surface Object.");
                            }

                            //Step Effects
                            if (StepData.StepEffects.Count > 0)
                            {
                                GameObject StepEffect = StepData.StepEffects[Random.Range(0, StepData.StepEffects.Count)];
                                if (StepEffect != null) EmeraldAI.Utility.EmeraldObjectPool.SpawnEffect(StepEffect, new Vector3(CurrentFoot.position.x, hit.point.y + 0.01f, CurrentFoot.position.z), Quaternion.FromToRotation(Vector3.up, hit.normal), StepData.StepEffectTimeout);
                            }

                            //Footprints
                            if (StepData.Footprints.Count > 0)
                            {
                                GameObject Footprint = StepData.Footprints[Random.Range(0, StepData.Footprints.Count)];
                                if (Footprint != null) EmeraldAI.Utility.EmeraldObjectPool.SpawnEffect(Footprint, new Vector3(CurrentFoot.position.x, hit.point.y + 0.01f, CurrentFoot.position.z), Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation, StepData.FootprintTimeout);
                            }

                            //Step Sounds
                            if (StepData.StepSounds.Count > 0)
                            {
                                AudioClip StepSound = StepData.StepSounds[Random.Range(0, StepData.StepSounds.Count)];
                                if (StepSound != null) EmeraldComponent.SoundComponent.PlayAudioClip(StepSound, StepData.StepVolume);
                            }
                        }
                    }
                }
            }
        }

        Texture GetTerrainTexture(Vector3 Position, Terrain terrain)
        {
            int surfaceIndex = 0;
            surfaceIndex = GetDominateTexture(Position, terrain, terrain.terrainData);
            return terrain.terrainData.terrainLayers[surfaceIndex].diffuseTexture;
        }

        float[] GetTextureBlend(Vector3 Pos, Terrain terrain, TerrainData terrainData)
        {
            int posX = (int)(((Pos.x - terrain.transform.position.x) / terrainData.size.x) * terrainData.alphamapWidth);
            int posZ = (int)(((Pos.z - terrain.transform.position.z) / terrainData.size.z) * terrainData.alphamapHeight);
            float[,,] SplatmapData = terrainData.GetAlphamaps(posX, posZ, 1, 1);
            float[] blend = new float[SplatmapData.GetUpperBound(2) + 1];

            for (int i = 0; i < blend.Length; i++)
            {
                blend[i] = SplatmapData[0, 0, i];
            }

            return blend;
        }

        int GetDominateTexture(Vector3 Pos, Terrain terrain, TerrainData terrainData)
        {
            float[] textureMix = GetTextureBlend(Pos, terrain, terrainData);
            int greatestIndex = 0;
            float maxTextureMix = 0;

            for (int i = 0; i < textureMix.Length; i++)
            {
                if (textureMix[i] > maxTextureMix)
                {
                    greatestIndex = i;
                    maxTextureMix = textureMix[i];
                }
            }

            return greatestIndex;
        }

        /// <summary>
        /// Order the FeetTransforms from the lowest to the highest, assigning the lowest footstep as the CurrentFoot.
        /// </summary>
        void CalculateLowestFoot()
        {
            LastFoot = CurrentFoot;
            CurrentFoot = FeetTransforms.OrderBy(p => p.position.y).First();
            if (LastFoot == CurrentFoot) CurrentFoot = FeetTransforms[FeetTransforms.Count-1];
        }

        /// <summary>
        /// Used to draw a circle of where each footstep collided.
        /// </summary>
        void DrawCircle(Vector3 center, float radius, Color color)
        {
            Vector3 prevPos = center + new Vector3(radius, 0, 0);
            for (int i = 0; i < 30; i++)
            {
                float angle = (float)(i + 1) / 30.0f * Mathf.PI * 2.0f;
                Vector3 newPos = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Debug.DrawLine(prevPos, newPos, color, 6);
                prevPos = newPos;
            }
        }
    }
}