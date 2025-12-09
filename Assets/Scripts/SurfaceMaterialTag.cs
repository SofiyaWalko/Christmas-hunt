using UnityEngine;

public class SurfaceMaterialTag : MonoBehaviour
{
    public SurfaceMaterial surfaceMaterial;

    private void OnValidate()
    {
        if (surfaceMaterial == null)
        {
            gameObject.name = $"{gameObject.name} (No Material)";
        }
    }
}
