using UnityEngine;

[CreateAssetMenu(fileName = "SurfaceMaterial", menuName = "Audio/Surface Material")]
public class SurfaceMaterial : ScriptableObject
{
    [Header("Material Settings")]
    public string materialName = "Default";
    public PhysicsMaterial physicMaterial;

    [Header("Sound Clips")]
    public AudioClip[] footstepClips = new AudioClip[1];
    public AudioClip jumpClip;

    [Header("Sound Settings")]
    [Range(0f, 2f)]
    public float footstepPitchMin = 0.9f;
    [Range(0f, 2f)]
    public float footstepPitchMax = 1.1f;
    [Range(0f, 1f)]
    public float volumeMultiplier = 1f;

    public AudioClip GetRandomFootstepClip()
    {
        if (footstepClips.Length == 0)
            return null;
        
        return footstepClips[Random.Range(0, footstepClips.Length)];
    }
}
