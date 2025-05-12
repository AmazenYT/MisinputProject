using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionOnTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger has the "Player" tag
        if (other.CompareTag("Player"))
        {
            // Load the EndScene
            SceneManager.LoadScene("EndScene");
        }
    }
}
