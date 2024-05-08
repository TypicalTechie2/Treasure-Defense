using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform playerTransform;
    public Vector3 playerOffset;
    public float xBoundary = 45;
    public float positiveZBoundary = 40;
    public float negativeZBoundary = -80;
    private PlayerController player;

    private void Awake()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (player.isGameActive)
        {
            transform.position = playerTransform.position + playerOffset;
            CameraBoundary();
        }
    }

    void CameraBoundary()
    {
        float clampedX = Mathf.Clamp(transform.position.x, -xBoundary, xBoundary);
        float clampedZ = Mathf.Clamp(transform.position.z, negativeZBoundary, positiveZBoundary);

        transform.position = new Vector3(clampedX, transform.position.y, clampedZ);
    }
}
