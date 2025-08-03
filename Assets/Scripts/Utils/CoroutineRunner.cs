using UnityEngine;
using System.Collections;

public class CoroutineRunner : GenericSingleton<CoroutineRunner>
{
    public Coroutine Run(IEnumerator routine)
    {
        return StartCoroutine(routine);
    }

    public void Stop(Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }
}
