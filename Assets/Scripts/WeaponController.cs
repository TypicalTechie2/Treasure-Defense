using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public TrailRenderer bulletTrail;
    public Transform bulletPos;
    public GameObject bullet;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UseBullet()
    {
        StartCoroutine(ShootBullet());
    }

    IEnumerator ShootBullet()
    {
        GameObject spawnedBullet = Instantiate(bullet, bulletPos.position, bullet.transform.rotation);
        Rigidbody bulletRB = spawnedBullet.GetComponent<Rigidbody>();
        bulletRB.velocity = bulletPos.forward * 50;

        yield return null;
    }
}
