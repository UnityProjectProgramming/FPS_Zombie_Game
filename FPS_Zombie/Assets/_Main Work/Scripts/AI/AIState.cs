using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState : MonoBehaviour
{
    public void SetStateMachine(AIStateMachine stateMachine) { _stateMachine = stateMachine; }

    /// <summary>
    /// default handlres they are called by the AI state machine when informations needs to be handeld.
    /// </summary>
    /// <returns></returns>
    public virtual void OnEnterState()        { }
    public virtual void OneExitState()        { }
    public virtual void OnAnimatorUpdated()   { }
    public virtual void OnAnimatorIKUpdated() { }
    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other) { }
    public virtual void OnDestinationReached(bool isReached) { }


    // Abstract methods
    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();


    protected AIStateMachine _stateMachine;
}
