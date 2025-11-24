using UnityEngine;

public class ObjectController: MonoBehaviour
{
    public float moveSpeed = 5.0f;
    private Vector3 startPosition;

    void Start() {
        startPosition = transform.position;
        Debug.Log("Hello from Start! My name is" + gameObject.name);  
        ChangeColor(Color.pink);      

    }

    void Update() {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        if (transform.position.z > 50.0f) {
            ResetPosition();
        }
    }
    
    private void ResetPosition() {
        transform.position = startPosition;
        Debug.Log("Position has been reset!");
    }

    public void ChangeColor(Color newColor) {
        Renderer  objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null) {
            objectRenderer.material.color = newColor;
        }
    }
}
