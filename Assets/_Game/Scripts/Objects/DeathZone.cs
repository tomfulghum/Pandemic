using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.CompareTag("Player")) {
            AreaTransitionManager.Instance.Setback(other.gameObject, other.GetComponent<PlayerRespawnTracker>().GetRespawnPoint(), () => {
                other.GetComponent<PlayerHook>().Reset();
            });
        }
    }
}
