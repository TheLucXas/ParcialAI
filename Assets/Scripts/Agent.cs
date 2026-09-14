using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    private enum SteeringModes { Seek, Flee, Arrive, Pursuit, Evade, Flocking }

    [Header("Stats")]
    [SerializeField] private float _maxHealth = 2.0f;
    [SerializeField] private float _attackDamage = 0.25f;
    [SerializeField] private float _attackCooldown = 0.5f;
    [SerializeField] private float _respawnTime = 6.0f;
    private float _currentHealth;
    private bool _isDead = false;
    [SerializeField]
    private float _maxSpeed = 5f;
    [SerializeField]
    private float _maxForce = 10f;
    [SerializeField]
    private float _viewRadius = 5f;
    [SerializeField]
    private SteeringModes _currentMode;
    [SerializeField]
    private float _arriveRadius = 3f;
    private LayerMask _originalMask;
    [SerializeField] private Color _gizmosFlockingColor = Color.purple;
    [SerializeField] private LayerMask _rewardLayer;
    [SerializeField] private LayerMask _hunterLayer;

    [Header("Evade Stats")]
    [SerializeField] private float _evadeSpeed = 8f;
    [SerializeField] private float _evadeForce = 15f;

    [Header("Flocking values")]
    [SerializeField]
    private float _separationRadius = 2f;
    [SerializeField, Range(0f, 3f)]
    private float _separationWeight = 1f;
    [SerializeField, Range(0f, 3f)]
    private float _cohesionWeight = 1f;
    [SerializeField, Range(0f, 3f)]
    private float _alignmentWeight = 1f;

    [Header("References")]
    [SerializeField]
    private Transform target;
    [SerializeField]
    private Agent targetAgent;

    private static readonly List<Agent> _allAgents = new();
    private readonly Collider[] _detectionBuffer = new Collider[1];
    private readonly Collider[] _hunterBuffer = new Collider[1];

    [Header("Visual Feedback")]
    [SerializeField] private MeshRenderer _meshRenderer;
    private Color _originalColor;
    private Coroutine _damageFlashCoroutine;

    private float _timer;
    private Vector3 _velocity;
    public Vector3 Velocity => _velocity;
    public bool IsDead => _isDead;
    private int _originalLayer;

    private void Awake()
    {
        _allAgents.Add(this);
        _currentHealth = _maxHealth;
        _originalLayer = gameObject.layer;
        if (_meshRenderer != null) _originalColor = _meshRenderer.material.color;
    }

    private void Start()
    {
        Vector3 randomVector = new(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        _velocity += randomVector.normalized * _maxSpeed;
        _timer = _attackCooldown;
    }

    private void Update()
    {
        if (_isDead) return;

        Vector3 steeringForce = Vector3.zero;

        int hunterHitCount = Physics.OverlapSphereNonAlloc(transform.position, _viewRadius, _hunterBuffer, _hunterLayer);
        int rewardHitCount = Physics.OverlapSphereNonAlloc(transform.position, _viewRadius, _detectionBuffer, _rewardLayer);

        if (hunterHitCount > 0 && _hunterBuffer[0].TryGetComponent<FSMAgent>(out var hunter))
        {
            steeringForce += SteeringUtils.CalculateEvade(
                transform.position,
                _velocity,
                _evadeSpeed,
                _evadeForce,
                hunter.transform.position,
                hunter.CurrentVelocity
            );
        }
        else if (rewardHitCount > 0)
        {
            Vector3 ballPosition = _detectionBuffer[0].transform.position;
            Vector3 flatBallPos = new Vector3(ballPosition.x, 0f, ballPosition.z);

            steeringForce += CalculateArrive(flatBallPos);
            steeringForce += CalculateSeparation(_allAgents, _separationRadius) * _separationWeight;

            if (Vector3.Distance(transform.position, ballPosition) <= _arriveRadius)
            {
                _velocity = Vector3.zero;
                _timer += Time.deltaTime;

                if (_timer >= _attackCooldown)
                {
                    _timer = 0f;
                    if (_detectionBuffer[0].TryGetComponent<Ball>(out var ball)) ball.TakeDamage(_attackDamage);
                }
                return;
            }
        }
        else
        {
            steeringForce += CalculateFlocking();
        }

        _velocity += steeringForce;
        _velocity.y = 0f;

        transform.position += _velocity * Time.deltaTime;

        if (_velocity.sqrMagnitude > 0.0001f)
        {
            transform.forward = _velocity.normalized;
        }

        transform.position = Bounds.Instance.CalculateBoundPosition(transform.position);
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;

        if (_meshRenderer != null)
        {
            if (_damageFlashCoroutine != null) StopCoroutine(_damageFlashCoroutine);
            _damageFlashCoroutine = StartCoroutine(DamageFlash());
        }

        if (_currentHealth <= 0f)
        {
            _currentHealth = 0f;
            _isDead = true;
            _velocity = Vector3.zero;
            gameObject.layer = LayerMask.NameToLayer("Gather");

            if (_meshRenderer != null) _meshRenderer.material.color = Color.red;
        }
    }

    public void Collect()
    {
        AgentVisibility(false);
        StartCoroutine(WaitToRespawn());
    }

    private IEnumerator WaitToRespawn()
    {
        yield return new WaitForSeconds(_respawnTime);
        Respawn();
    }

    private IEnumerator DamageFlash()
    {
        _meshRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.125f);
        _meshRenderer.material.color = _originalColor;
        yield return new WaitForSeconds(0.125f);

        _meshRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.125f);
        _meshRenderer.material.color = _originalColor;
        yield return new WaitForSeconds(0.125f);

        _meshRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.125f);

        if (!_isDead) _meshRenderer.material.color = _originalColor;
    }

    public void Respawn()
    {
        transform.position = Bounds.Instance.GetRandomPointInBounds();
        _currentHealth = _maxHealth;
        _isDead = false;

        if (_meshRenderer != null) _meshRenderer.material.color = _originalColor;

        AgentVisibility(true);
    }

    public void AgentVisibility(bool visible)
    {
        gameObject.layer = visible ? _originalLayer : 0;

        if (_meshRenderer != null) _meshRenderer.enabled = visible;
    }

    #region Steering Behaviors

    #region Flocking
    private Vector3 CalculateFlocking()
    {
        return CalculateSeparation(_allAgents, _separationRadius) * _separationWeight
             + CalculateAlignment(_allAgents, _viewRadius) * _alignmentWeight
             + CalculateCohesion(_allAgents, _viewRadius) * _cohesionWeight;
    }

    private Vector3 CalculateCohesion(IEnumerable<Agent> agents, float radius)
    {
        Vector3 desiredPosition = default;
        int count = 0;

        foreach (Agent item in agents)
        {
            if (item == this || item.IsDead) continue;
            if (InRange(item.transform.position, radius))
            {
                desiredPosition += item.transform.position;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        desiredPosition /= count;

        return CalculateSeek(desiredPosition);
    }

    private Vector3 CalculateSeparation(IEnumerable<Agent> agents, float radius)
    {
        Vector3 desired = default;
        int count = 0;

        foreach (Agent item in agents)
        {
            if (item == this || item.IsDead) continue;
            if (InRange(item.transform.position, radius))
            {
                desired += (item.transform.position - transform.position);
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        desired /= count;

        return SteeringUtils.CalculateSteering(_velocity, -desired.normalized * _maxSpeed, _maxForce);
    }

    private Vector3 CalculateAlignment(List<Agent> agents, float radius)
    {
        Vector3 desired = default;
        int count = 0;

        foreach (Agent item in agents)
        {
            if (item == this || item.IsDead) continue;
            if (InRange(item.transform.position, radius))
            {
                desired += item.Velocity;
                count++;
            }
        }
        if (count == 0) return Vector3.zero;
        desired /= count;

        return SteeringUtils.CalculateSteering(_velocity, desired.normalized * _maxSpeed, _maxForce);
    }

    private bool InRange(Vector3 position, float radius) => (position - transform.position).sqrMagnitude <= radius * radius;

    #endregion

    private Vector3 CalculatePursuit(Agent target) =>
        SteeringUtils.CalculatePursuit(transform.position, _velocity, _maxSpeed, _maxForce, target.transform.position, target.Velocity);
    private Vector3 CalculateEvade(Agent target) =>
         SteeringUtils.CalculateEvade(transform.position, _velocity, _maxSpeed, _maxForce, target.transform.position, target.Velocity);

    private Vector3 CalculateSeek(Vector3 targetPosition) =>
        SteeringUtils.CalculateSeek(transform.position, _velocity, targetPosition, _maxSpeed, _maxForce);

    private Vector3 CalculateFlee(Vector3 targetPosition) =>
        SteeringUtils.CalculateFlee(transform.position, _velocity, targetPosition, _maxSpeed, _maxForce);

    private Vector3 CalculateArrive(Vector3 targetPosition) =>
        SteeringUtils.CalculateArrive(transform.position, _velocity, targetPosition, _maxSpeed, _maxForce, _arriveRadius);

    #endregion

    #region Steering
    private Vector3 GetCurrentSteeringMode() => GetSteering(_currentMode, target.position);
    private Vector3 GetSteering(SteeringModes mode, Vector3 targetPosition)
    {
        return mode switch
        {
            SteeringModes.Seek => CalculateSeek(targetPosition),
            SteeringModes.Flee => CalculateFlee(targetPosition),
            SteeringModes.Arrive => CalculateArrive(targetPosition),
            _ => Vector3.zero,
        };
    }
    #endregion

    private void OnDestroy()
    {
        _allAgents.Remove(this);
    }

    private void OnDrawGizmos()
    {
        Vector3 pos2D = new Vector3(transform.position.x, 0f, transform.position.z);

        Gizmos.color = Color.white;
        GizmosUtils.DrawGizmosCircle(pos2D, Vector3.up, _viewRadius);

        Gizmos.color = _gizmosFlockingColor;
        GizmosUtils.DrawGizmosCircle(pos2D, Vector3.up, _separationRadius);
    }
}