using UnityEngine;

public class GatherState : State
{
    #region Private Fields
    private GatherData _data;
    private FSMAgent _agent;
    private Transform _target;
    private Agent _targetAgent;
    private bool _isGathering;
    private float _timer;
    #endregion

    #region Constructor
    public GatherState(GatherData data, FSMAgent agent)
    {
        _data = data;
        _agent = agent;
    }
    #endregion

    #region Public Methods
    public void SetTarget(Transform target) => _target = target;
    #endregion

    #region State Overrides
    public override void Enter()
    {
        _agent.UpdateStateText("Gather");
        if (_target != null) _targetAgent = _target.GetComponentInParent<Agent>();
        _isGathering = false;
        _timer = 0;
    }

    public override void Update()
    {
        if (_target == null || _targetAgent == null)
        {
            _agent.FSM.ChangeState(_agent.Patrol);
            return;
        }

        if (!_targetAgent.IsDead)
        {
            _agent.FSM.ChangeState(_agent.Patrol);
            return;
        }

        float dist = Vector3.Distance(_agent.transform.position, _target.position);
        if (dist > _agent.PerceptionRadius + 1.5f)
        {
            _agent.FSM.ChangeState(_agent.Patrol);
            return;
        }

        if (_isGathering && _timer < _data.GatherTime)
        {
            _timer += Time.deltaTime;
            return;
        }
        else if (_timer >= _data.GatherTime)
        {
            _targetAgent.Collect();
            _agent.FSM.ChangeState(_agent.Patrol);
            return;
        }

        Vector3 agentPos2D = new Vector3(_agent.transform.position.x, 0f, _agent.transform.position.z);
        Vector3 targetPos2D = new Vector3(_target.position.x, 0f, _target.position.z);

        if (Vector3.Distance(agentPos2D, targetPos2D) <= _data.GatherCheckDistance && !_isGathering)
        {
            _isGathering = true;
            _agent.CurrentVelocity = Vector3.zero;
            return;
        }

        _agent.CurrentVelocity += SteeringUtils.CalculateArrive(
            agentPos2D,
            _agent.CurrentVelocity,
            targetPos2D,
            _data.Speed,
            _data.SteerForce,
            _data.ArriveRadius
        );

        _agent.CurrentVelocity = new Vector3(_agent.CurrentVelocity.x, 0f, _agent.CurrentVelocity.z);

        _agent.transform.position += _agent.CurrentVelocity * Time.deltaTime;

        if (_agent.CurrentVelocity.sqrMagnitude > 0.0001f)
        {
            _agent.transform.forward = _agent.CurrentVelocity.normalized;
        }
    }

    public override void Exit()
    {
        _target = null;
        _targetAgent = null;
        _isGathering = false;
    }
    #endregion
}

[System.Serializable]
public class GatherData
{
    [Header("Timing")]
    public float GatherTime = 3f;

    [Header("Movement")]
    public float Speed = 13f;
    public float SteerForce = 10f;
    public float ArriveRadius = 1.75f;
    public float GatherCheckDistance = 1.25f;
}