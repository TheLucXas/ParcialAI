using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public event Action<Ball> OnDestroyed;

    private void OnDestroy()
    {
        OnDestroyed?.Invoke(this);
    }
}
