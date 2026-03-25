using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Footstep Surface Object", menuName = "Emerald AI/Footstep Surface Object")]
public class FootstepSurfaceObject : ScriptableObject
{
    public bool HideSettingsFoldout;
    public bool SurfaceSettingsFoldout;

    public enum SurfaceTypes { Tag = 1, Texture = 2 };
    [Tooltip("Controls how the information will be received for a footstep.")] 
    public SurfaceTypes SurfaceType = SurfaceTypes.Tag;
    [Space(5)]
    [Tooltip("The terrain textures that need to be detected for this Foostep Surface Object.\n\nNote: This must be received from a Unity Terrain.")]
    public List <Texture> SurfaceTextures = new List<Texture>();
    [Space(5)]
    [Tooltip("The tag that needs to be detected for this Foostep Surface Object.\n\nNote: This can be any gameobject, excluding a Unity Terrain.")]
    [Tag] [SerializeField] public string SurfaceTag = "Untagged";
    [Space(10)]
    [Tooltip("Controls the volume of the footsteps for this Foostep Surface Object.")]
    [Range(0, 1)] public float StepVolume = 1;
    [Space(10)]
    [Tooltip("A list of footstep sounds that will randomly play when this Footstep Surface Object is used.")]
    public List<AudioClip> StepSounds = new List<AudioClip>();
    [Space(10)]
    [Tooltip("Controls the how long (in seconds) it takes for the Step Effects to despawn.")]
    [Range(0.5f, 6)] public float StepEffectTimeout = 2;
    [Tooltip("A list of footstep effects that will randomly be picked when this Footstep Surface Object is used.\n\nNote: You can leave this setting blank if you do not want to use a Step Effect.")]
    public List<GameObject> StepEffects = new List<GameObject>();
    [Space(10)]
    [Tooltip("Controls the how long (in seconds) it takes for the Footprint to despawn.")]
    [Range(1f, 30)] public float FootprintTimeout = 10;
    [Tooltip("A list of footprints that will randomly be picked (as well as positioned and aligned to the detected surface) when this Footstep Surface Object is used.\n\nNote: You can leave this setting blank if you do not want to use a Footprint.")]
    public List<GameObject> Footprints = new List<GameObject>();
}
