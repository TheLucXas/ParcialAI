using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class FSMAgent : MonoBehaviour
{
    [SerializeField] private PatrolData _patrolData;

    private readonly FiniteStateMachine _fsm = new();
    public FiniteStateMachine FSM => _fsm;

    public IdleState Idle { get; private set; }
    public PatrolState Patrol { get; private set; }
    public AttackState Attack { get; private set; }

    private void Start()
    {
        Idle = new(this);
        Patrol = new(_patrolData, this);
        _fsm.AddState(Idle);
        _fsm.AddState(Patrol);
        _fsm.AddState(Attack);
        _fsm.ChangeState(Idle);
    }
    private void Update()
    {
        _fsm.Update();
    }


}
