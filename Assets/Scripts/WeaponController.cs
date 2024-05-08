using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public TrailRenderer bulletTrail;
    public Transform bulletPos;
    public GameObject bullet;
    public float bulletSpeedRate;
    public float bulletSpeed = 50f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UseBullet(Vector3 direction)
    {
        StartCoroutine(ShootBullet(direction));
    }

    IEnumerator ShootBullet(Vector3 direction)
    {
        GameObject spawnedBullet = Instantiate(bullet, bulletPos.position, Quaternion.LookRotation(direction));
        Rigidbody bulletRB = spawnedBullet.GetComponent<Rigidbody>();
        bulletRB.velocity = direction * bulletSpeed;

        yield return null;
    }
}
