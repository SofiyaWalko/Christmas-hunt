// using UnityEngine;

// public class BouncePad : MonoBehaviour
// {
//     [SerializeField] private float bounceForce = 20f;

//     private void OnCollisionEnter(Collision collision)
//     {
//         if (!collision.collider.CompareTag("Player")) return;

//         PlayerController pc = collision.collider.GetComponent<PlayerController>();
//         if (pc == null)
//             pc = collision.collider.GetComponentInParent<PlayerController>();

//         if (pc != null)
//         {
//             float v = Mathf.Sqrt(bounceForce * -2f * pc.gravity);
//             pc.SetVerticalVelocity(v);
//         }
//     }
// }
