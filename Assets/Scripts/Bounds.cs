using UnityEngine;

public class Bounds : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField]
    private float _height = 34f;
    [SerializeField]
    private float _width = 52.2f;
    [SerializeField] private GameObject _backgroundField;
    [SerializeField] private bool _drawGizmos;
    [SerializeField] private Color _gizmosColor = Color.white;
    #endregion

    #region Properties
    public static Bounds Instance { get; private set; }
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnValidate()
    {
        if (_backgroundField != null)
        {
            _backgroundField.transform.localScale = new Vector3(_width, _height, 1f);
        }
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;

        Gizmos.color = _gizmosColor;
        Gizmos.DrawWireCube(transform.position, new Vector3(_width, 0f, _height));
    }
    #endregion

    #region Public Methods
    public Vector3 CalculateBoundPosition(Vector3 position)
    {
        Vector3 newPosition = position;

        if (position.x > _width / 2f) newPosition.x = -_width / 2f;
        if (position.x < -_width / 2f) newPosition.x = _width / 2f;
        if (position.z > _height / 2f) newPosition.z = -_height / 2f;
        if (position.z < -_height / 2f) newPosition.z = _height / 2f;
        newPosition.y = 0f;

        return newPosition;
    }

    public Vector3 GetRandomPointInBounds()
    {
        float x = Random.Range(-_width / 2f, _width / 2f);
        float z = Random.Range(-_height / 2f, _height / 2f);
        return new Vector3(x, 0f, z);
    }

    public bool IsOutOfBounds(Vector3 position)
    {
        return position.x > _width / 2f || position.x < -_width / 2f ||
               position.z > _height / 2f || position.z < -_height / 2f;
    }
    #endregion
}