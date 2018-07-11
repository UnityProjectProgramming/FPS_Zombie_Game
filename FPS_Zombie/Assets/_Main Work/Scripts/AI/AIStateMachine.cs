using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIStateType  { None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead };
public enum AITargetType { None, Waypoint, Visual_Player, Visual_Light, Visual_Food, Audio };
public enum AITriggerEventType { Enter, Stay, Exit};
/// <summary>
/// Describe a potential target to the AI System.
/// </summary>
public struct AITarget
{
    //TODO make private and apply setters and getters
    public AITargetType type { get; private set; }
    public Collider     collider { get; private set; }
    public Vector3      position { get; private set; }      // current position in the world
    public float        distance { get; set; }      // distance from player
    public float        time { get; private set; }          // tie since the target was last ping'd

    public void Set(AITargetType t, Collider c, Vector3 p, float d)
    {
        type = t;
        collider = c;
        position = p;
        distance = d;
        time = Time.time;
    }

    public void Clear()
    {
        type = AITargetType.None;
        collider = null;
        position = Vector3.zero;
        distance = Mathf.Infinity;
        time = 0.0f;
    }
}

public abstract class AIStateMachine : MonoBehaviour
{
    // Public 
    public AITarget VisualThreat = new AITarget();
    public AITarget AudioThreat = new AITarget();

    // Protected
    protected AIState _currentState = null;
    protected Dictionary<AIStateType, AIState> _states = new Dictionary<AIStateType, AIState>();
    protected AITarget _target = new AITarget();

    [SerializeField] protected AIStateType _currentStateType = AIStateType.Idle;
    [SerializeField] protected SphereCollider _targetTrigger = null;
    [SerializeField] protected SphereCollider _sensorTrigger = null;

    [SerializeField] [Range(0, 20)] protected float _stoppingDistance = 1.0f;

    //Component Cache
    protected Animator _animator = null;
    protected NavMeshAgent _navAgent = null;
    protected Collider _collider = null;
    protected Transform _transform = null;

    // Public properties
    public Animator Animator { get { return _animator; } }
    public NavMeshAgent NavAgent { get { return _navAgent; } }

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();
        _transform = transform;
    }

    protected virtual void Start()
    {
        AIState[] states = GetComponents<AIState>();
        foreach (AIState state in states)
        {
            if(state != null && !_states.ContainsKey(state.GetStateType()))
            {
                _states[state.GetStateType()] = state; // getting the state type is the key.
                state.SetStateMachine(this);
            }
        }

        if(_states.ContainsKey(_currentStateType))
        {
            _currentState = _states[_currentStateType];
            _currentState.OnEnterState();
        }
        else
        {
            _currentState = null;
        }
    }
	
    protected virtual void  FixedUpdate()
    {
        VisualThreat.Clear();
        AudioThreat.Clear();

        if(_target.type != AITargetType.None)
        {
            _target.distance = Vector3.Distance(_transform.position, _target.position);
        }
    }

    /// <summary>
    /// called by unity each frame, Gives the current state a chance to 
    /// update itself.
    /// </summary>
    protected virtual void Update()
    {
        if (!_currentState) { return; }
        AIStateType newStateType = _currentState.OnUpdate();

        if(_currentStateType != newStateType)
        {
            AIState newState = null;
            // this functions (TryGetValue) first checks just like the containKey does
            // to see if this key exist in the dicenory and if it does
            // it will automatically populate the out parameter ( newState )
            // with the actual AIState Reference
            if(_states.TryGetValue(newStateType, out newState))
            {
                _currentState.OneExitState();
                newState.OnEnterState();
                _currentState = newState;
            }
            else if (_states.TryGetValue(AIStateType.Idle, out newState))
            {
                _currentState.OneExitState();
                newState.OnEnterState();
                _currentState = newState;
            }

            _currentStateType = newStateType;
        }
    }

    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d)
    {
        _target.Set(t, c, p, d);

        if(_targetTrigger)
        {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d, float s)
    {
        _target.Set(t, c, p, d);

        if (_targetTrigger)
        {
            _targetTrigger.radius = s;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    public void SetTarget(AITarget t)
    {
        _target = t; 

        if (_targetTrigger)
        {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    public void ClearTarget()
    {
        _target.Clear();
        if(_targetTrigger)
        {
            _targetTrigger.enabled = false;
        }
    }
}
