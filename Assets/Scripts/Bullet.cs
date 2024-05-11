using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damagePerHit;

    // Trigger collision handle between wallm chestm or Boss object.
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Chest") || other.gameObject.CompareTag("Boss"))
        {
            Destroy(gameObject);
        }
    }
}
