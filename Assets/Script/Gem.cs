
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
        transform.Rotate(
            Vector3.up * rotateSpeed * Time.deltaTime
        );

        float offset =
            Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        transform.position =
            startPosition + Vector3.up * offset;
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            collected = true;

            // เพิ่มคะแนน
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CollectGem();
            }

            Destroy(gameObject);
        }
    }
}
