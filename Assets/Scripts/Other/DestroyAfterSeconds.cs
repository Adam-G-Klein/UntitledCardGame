using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour
{
    public int seconds = 6;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DestroyAfterSecondsCoroutine());
    }

    private IEnumerator DestroyAfterSecondsCoroutine() {
        yield return new WaitForSeconds(seconds);
        Destroy(this.gameObject);
    }
}
