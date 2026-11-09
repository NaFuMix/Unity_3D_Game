
using UnityEngine;

public class Gem : MonoBehaviour
{
    public float rotateSpeed = 90f;
    public float floatSpeed = 2f;
    public float floatHeight = 0.15f;

    private Vector3 startPosition;
    private bool collected = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Rotate
        transform.Rotate(
            Vector3.up * rotateSpeed * Time.deltaTime
        );

        // Floating animation
        float offset = Mathf.Sin(
            Time.time * floatSpeed
        ) * floatHeight;

        transform.position = startPosition
            + Vector3.up * offset;
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            collected = true;

            GameManager.Instance.CollectGem();

            Destroy(gameObject);
        }
    }
}
