using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f;

    private Vector3 originalPosition;
    public float floatHeight = 1.0f; // Height at which the object floats
    public float floatSpeed = 1.0f; // Speed of floating
    private ParticleSystem particle;

    private void Awake()
    {
        particle = GetComponentInChildren<ParticleSystem>();
    }


    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        FloatPowerUp();
        RotatePowerUp();
    }

    private void FloatPowerUp()
    {
        // Calculate the target position for floating using sine function
        float offsetY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        Vector3 targetPosition = originalPosition + Vector3.up * offsetY;

        targetPosition.y = 2.5f;

        // Move the object towards the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime);
    }

    private void RotatePowerUp()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
