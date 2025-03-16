using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour
{
    public float destroyTime = 1.5f;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}
