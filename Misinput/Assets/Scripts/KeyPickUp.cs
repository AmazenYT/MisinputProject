using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public string keyToUnlock; // "W", "A", "S", "D", "Space"

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovementTutorial player = other.GetComponentInParent<PlayerMovementTutorial>();
        if (player != null)
        {
            player.UnlockAbility(keyToUnlock);
            Destroy(gameObject);
        }
    }
}
