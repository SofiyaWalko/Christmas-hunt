using UnityEngine;

public class SurfaceDetector : MonoBehaviour
{
    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float rayDistance = 1f;

    private SurfaceMaterial _currentMaterial;
    private SurfaceMaterial _defaultMaterial;
    private Transform _rayPoint;

    private void Start()
    {
        _rayPoint = transform;
        
        // Если нет материала по умолчанию, создаём его
        if (_defaultMaterial == null)
        {
            _defaultMaterial = ScriptableObject.CreateInstance<SurfaceMaterial>();
            _defaultMaterial.materialName = "Default";
        }

        _currentMaterial = _defaultMaterial;
    }

    private void Update()
    {
        DetectSurface();
    }

    private void DetectSurface()
    {
        if (Physics.Raycast(_rayPoint.position, Vector3.down, out RaycastHit hit, rayDistance, groundLayer))
        {
            // Ищем компонент SurfaceMaterialTag на столкнувшемся объекте
            SurfaceMaterialTag materialTag = hit.collider.GetComponent<SurfaceMaterialTag>();
            
            if (materialTag != null && materialTag.surfaceMaterial != null)
            {
                if (_currentMaterial != materialTag.surfaceMaterial)
                {
                    _currentMaterial = materialTag.surfaceMaterial;
                    OnSurfaceChanged();
                }
            }
            else
            {
                if (_currentMaterial != _defaultMaterial)
                {
                    _currentMaterial = _defaultMaterial;
                    OnSurfaceChanged();
                }
            }
        }
    }

    private void OnSurfaceChanged()
    {
        // Уведомляем PlayerSoundController об изменении материала
        PlayerSoundController soundController = GetComponent<PlayerSoundController>();
        if (soundController != null)
        {
            soundController.SetCurrentMaterial(_currentMaterial);
        }
    }

    public SurfaceMaterial GetCurrentMaterial()
    {
        return _currentMaterial;
    }
}
