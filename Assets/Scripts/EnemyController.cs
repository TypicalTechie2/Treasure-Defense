using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;
    [SerializeField] WeaponController weapon;

    private Material enemyMaterial;

    private void Awake()
    {
        enemyMaterial = GetComponent<MeshRenderer>().material;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Bullet")
        {
            currentHealth -= weapon.damagePerHit;

            Debug.Log("Current Health: " + currentHealth);
            StartCoroutine(DetectEnemyHitRoutine());
        }
    }

    private IEnumerator DetectEnemyHitRoutine()
    {
        enemyMaterial.color = Color.red;

        yield return new WaitForSeconds(0.1f);



        if (currentHealth > 0)
        {
            enemyMaterial.color = Color.white;
        }

        else
        {
            enemyMaterial.color = Color.grey;
            gameObject.layer = 10;
            Destroy(gameObject, 2);
        }
    }
}
