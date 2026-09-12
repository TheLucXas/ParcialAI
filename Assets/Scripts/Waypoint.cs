using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [SerializeField] private bool _drawGizmos = true;
    [SerializeField] private bool _hideGizmosOnPlay = true;

    void Update()
    {
        OnDrawGizmos();
    }

    void Start()
    {
        if (_hideGizmosOnPlay) _drawGizmos = false;
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(1f, 0f, 1f));
    }

}
