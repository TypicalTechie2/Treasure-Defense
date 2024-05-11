using UnityEngine;
using UnityEngine.AI;

public class BossWeapon : MonoBehaviour
{
    private NavMeshAgent lavaNavMesh;
    private Transform player;
    private Rigidbody rb;
    public int damagePerHit = 5;

    private void Awake()
    {
        lavaNavMesh = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player").GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        lavaNavMesh.destination = player.position;
        FreezeRotation();
    }

    // Method to freeze rotation of the rigidbody
    private void FreezeRotation()
    {
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
    }

    // OnTriggerEnter is called when the Collider other enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("PowerUp"))
        {
            Destroy(gameObject);
        }
    }
}
