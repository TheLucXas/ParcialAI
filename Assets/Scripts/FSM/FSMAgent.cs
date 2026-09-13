using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FSMAgent : MonoBehaviour
{
    [SerializeField] private PatrolData _patrolData;
    [SerializeField] private AttackData _attackData;
    [SerializeField] private bool _drawGizmos = true;
    [SerializeField] private Color _meleeGizmoColor = Color.red;
    [SerializeField] private Color _rangeGizmoColor = Color.yellow;
    [SerializeField] private Color _radiusGizmoColor = Color.green;
    public float PerceptionRadius => _patrolData.detectionRadius;
    public float AttackCooldownTimer { get; set; } = 0f;
    public Vector3 CurrentVelocity { get; set; } = Vector3.zero;


    private readonly FiniteStateMachine _fsm = new();
    public FiniteStateMachine FSM => _fsm;

    public IdleState Idle { get; private set; }
    public PatrolState Patrol { get; private set; }
    public AttackState Attack { get; private set; }

    private void Start()
    {
        Idle = new(this);
        Patrol = new(_patrolData, this);
        Attack = new(_attackData, this);
        _fsm.AddState(Idle);
        _fsm.AddState(Patrol);
        _fsm.AddState(Attack);
        _fsm.ChangeState(Idle);
    }
    private void Update()
    {
        if (AttackCooldownTimer > 0f)
        {
            AttackCooldownTimer -= Time.deltaTime;
        }

        _fsm.Update();
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

}
