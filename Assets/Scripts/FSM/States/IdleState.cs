using UnityEngine;

public class IdleState : State
{
    #region Private Fields
    private readonly FSMAgent _agent;
    readonly float _timeToChangeToPatrol = 2f;
    private float _timer = 0f;
    #endregion

    #region Constructor
    public IdleState(FSMAgent agent) => _agent = agent;
    #endregion

    #region State Overrides
    public override void Enter()
    {
        _timer = 0f;
        _agent.UpdateStateText("Idle");
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _timeToChangeToPatrol)
        {
            _agent.FSM.ChangeState(_agent.Patrol);
        }
    }

    public override void Exit()
    {
    }
    #endregion
}