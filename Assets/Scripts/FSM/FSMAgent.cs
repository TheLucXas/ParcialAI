using UnityEngine;
using TMPro;
public class FSMAgent : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private PatrolData _patrolData;
    [SerializeField] private AttackData _attackData;
    [SerializeField] private GatherData _gatherData;

    [Header("Gizmos")]
    [SerializeField] private bool _drawGizmos = true;
    [SerializeField] private Color _meleeGizmoColor = Color.red;
    [SerializeField] private Color _rangeGizmoColor = Color.yellow;
    [SerializeField] private Color _radiusGizmoColor = Color.green;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite[] _refereeSprites;
    [SerializeField] private TextMeshProUGUI _stateText;
    #endregion

    #region Private Fields
    private readonly FiniteStateMachine _fsm = new();
    #endregion

    #region Properties
    public float PerceptionRadius => _patrolData.detectionRadius;
    public float AttackCooldownTimer { get; set; } = 0f;
    public Vector3 CurrentVelocity { get; set; } = Vector3.zero;

    public FiniteStateMachine FSM => _fsm;

    public IdleState Idle { get; private set; }
    public PatrolState Patrol { get; private set; }
    public AttackState Attack { get; private set; }
    public GatherState Gather { get; private set; }
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        Idle = new(this);
        Patrol = new(_patrolData, this);
        Attack = new(_attackData, this);
        Gather = new(_gatherData, this);
        _fsm.AddState(Idle);
        _fsm.AddState(Patrol);
        _fsm.AddState(Attack);
        _fsm.AddState(Gather);
        _fsm.ChangeState(Idle);
        int spriteIndex = Random.Range(0, _refereeSprites.Length);
        if (_refereeSprites != null && _spriteRenderer != null) _spriteRenderer.sprite = _refereeSprites[spriteIndex];
    }
    private void Update()
    {
        if (AttackCooldownTimer > 0f)
        {
            AttackCooldownTimer -= Time.deltaTime;
        }

        _fsm.Update();

        transform.position = Bounds.Instance.CalculateBoundPosition(transform.position);
    }

    private void OnDrawGizmos()
    {
        if (_drawGizmos)
        {
            var pos = new Vector3(transform.position.x, 0f, transform.position.z);
            Gizmos.color = _meleeGizmoColor;
            GizmosUtils.DrawGizmosCircle(pos, Vector3.up, _attackData.MeleeAttackRadius);

            Gizmos.color = _rangeGizmoColor;
            GizmosUtils.DrawGizmosCircle(pos, Vector3.up, _attackData.RangeAttackRadius);

            Gizmos.color = _radiusGizmoColor;
            GizmosUtils.DrawGizmosCircle(pos, Vector3.up, _patrolData.detectionRadius);
        }
    }
    #endregion

    #region Public Methods
    public void UpdateStateText(string newStateName)
    {
        if (_stateText != null)
        {
            _stateText.text = newStateName;
        }
    }
    #endregion
}