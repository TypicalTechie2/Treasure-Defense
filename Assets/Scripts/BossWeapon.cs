using System.Collections;
using System.Collections.Generic;
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
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        lavaNavMesh.destination = player.position;
        FreezeRotation();
    }

    private void FreezeRotation()
    {
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("PowerUp"))
        {
            Destroy(gameObject);
        }
    }
}
