using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [SerializeField] private bool _drawGizmos = true;

    void Update()
    {
        OnDrawGizmos();
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(1f, 0f, 1f));
    }

}
