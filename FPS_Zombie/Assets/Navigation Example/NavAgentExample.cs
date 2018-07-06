using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentExample : MonoBehaviour
{
    public AIWaypointNetwork waypointNetwork;
    public int currentIndex = 0;
    public bool hasPath = false;
    public bool pathPending = false;
    public NavMeshPathStatus pathStatus = NavMeshPathStatus.PathInvalid;
    public AnimationCurve jumpCurve = new AnimationCurve();

    private NavMeshAgent navAgent;

	void Start ()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.SetDestination(waypointNetwork.waypoints[currentIndex].position);

        SetNextDestination(false);

    }
	
    void SetNextDestination (bool increment)
    {
        int incStep = increment ? 1 : 0;
        Transform nextWaypointTransform = null;

        int nextWaypoint = (currentIndex + incStep >= waypointNetwork.waypoints.Count) ? 0 : currentIndex + incStep;
        nextWaypointTransform = waypointNetwork.waypoints[nextWaypoint];

        if(nextWaypointTransform != null)
        {
            currentIndex = nextWaypoint;
            navAgent.SetDestination(nextWaypointTransform.position);
            return;
        }

        currentIndex++;
    }

	void Update ()
    {
        hasPath = navAgent.hasPath;
        pathPending = navAgent.pathPending;
        pathStatus = navAgent.pathStatus;

        if(navAgent.isOnOffMeshLink)
        {
            StartCoroutine(Jump(1.0f));
            return;
        }

        if((navAgent.remainingDistance <= navAgent.stoppingDistance && !pathPending) || pathStatus == NavMeshPathStatus.PathInvalid || pathStatus == NavMeshPathStatus.PathPartial)
        {
            SetNextDestination(true);
        }
        else if (navAgent.isPathStale)
        {
            SetNextDestination(false);
        }	
	}

    IEnumerator Jump(float duration)
    {
        OffMeshLinkData meshLinkData = navAgent.currentOffMeshLinkData;
        Vector3 startPos             = navAgent.transform.position;
        Vector3 endPos               = meshLinkData.endPos + (navAgent.baseOffset * Vector3.up);
        float time                   = 0.0f;

        while(time <= duration)
        {
            float t = time / duration;
            navAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + (jumpCurve.Evaluate(t) * Vector3.up);
            time += Time.deltaTime;
            yield return null;
        }

        navAgent.CompleteOffMeshLink();
    }
}
