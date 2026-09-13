using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public event Action<Ball> OnDestroyed;
    [SerializeField] private bool _drawGizmos = true;
    [SerializeField] private Color _gizmosColor = Color.pink;

    private void OnDestroy()
    {
        OnDestroyed?.Invoke(this);
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;

        Gizmos.color = _gizmosColor;
        var pos = new Vector3(transform.position.x, 0f, transform.position.z);
        GizmosUtils.DrawGizmosCircle(pos, Vector3.up, 1f);
    }

}
