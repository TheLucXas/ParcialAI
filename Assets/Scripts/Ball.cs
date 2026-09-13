using Mono.Cecil.Cil;
using System;
using System.Collections;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public event Action<Ball> OnDestroyed;
    [SerializeField] private float _life = 1f;
    [SerializeField] private bool _drawGizmos = true;
    [SerializeField] private Color _gizmosColor = Color.pink;
    [SerializeField] private float _invincibilityTime = 0.25f;
    private bool _isInvincible = false;

    private void OnDestroy()
    {
        OnDestroyed?.Invoke(this);
    }

    private void Update()
    {
        
    }

    public void TakeDamage(float amount)
    {
        if (_isInvincible) return;

        if (_life <= 0)
        {
            Destroy(gameObject);
            return;
        }

        _life -= amount;
        StartCoroutine(InvincibilityTime());
    }

    IEnumerator InvincibilityTime()
    {
        _isInvincible = true;
        yield return new WaitForSeconds(_invincibilityTime);
        _isInvincible = false;
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;

        Gizmos.color = _gizmosColor;
        var pos = new Vector3(transform.position.x, 0f, transform.position.z);
        GizmosUtils.DrawGizmosCircle(pos, Vector3.up, 1f);
    }

}
