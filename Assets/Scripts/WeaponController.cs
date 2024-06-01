using System.Collections;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public TrailRenderer bulletTrail;
    public Transform bulletPos;
    public GameObject bullet;
    public float bulletSpeedRate;
    public float bulletSpeed = 50f;
    public AudioSource bulletAudio;
    public AudioClip fireSound;

    public void UseBullet(Vector3 direction)
    {
        StartCoroutine(ShootBullet(direction));
    }

    IEnumerator ShootBullet(Vector3 direction)
    {
        GameObject spawnedBullet = Instantiate(bullet, bulletPos.position, Quaternion.LookRotation(direction));

        bulletAudio.PlayOneShot(fireSound, 0.2f);

        Rigidbody bulletRB = spawnedBullet.GetComponent<Rigidbody>();
        bulletRB.velocity = direction * bulletSpeed;

        yield return null;
    }
}
