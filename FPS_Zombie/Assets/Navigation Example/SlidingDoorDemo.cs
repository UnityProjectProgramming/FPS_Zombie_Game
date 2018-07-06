using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorState {Opened, Animating, Closed};

public class SlidingDoorDemo : MonoBehaviour
{
    public float slidingDistance = 4.0f;
    public float duration = 2.0f;
    public AnimationCurve slideCurve = new AnimationCurve();

    private Transform doorTransform;
    private Vector3 openPos;
    private Vector3 closePos;
    private DoorState doorState = DoorState.Closed;

	// Use this for initialization
	void Start ()
    {
        doorTransform = transform;
        closePos = transform.position;
        openPos = transform.position + (doorTransform.right * slidingDistance);

	}
	
	// Update is called once per frame
	void Update ()
    {
		if(doorState != DoorState.Animating && Input.GetKeyDown(KeyCode.Space))
        {
            DoorState currentDoorState = doorState == DoorState.Opened ? DoorState.Closed : DoorState.Opened;
            StartCoroutine(AnimateDoor(currentDoorState));
        }
	}

    IEnumerator AnimateDoor(DoorState currentState)
    {
        doorState = DoorState.Animating;
        float time = 0.0f;
        Vector3 startPos = currentState == DoorState.Opened ? closePos : openPos;
        Vector3 endPos = currentState == DoorState.Opened ? openPos : closePos;

        while(time <= duration)
        {
            float t = time / duration;
            doorTransform.position = Vector3.Lerp(startPos, endPos, slideCurve.Evaluate(t));
            time += Time.deltaTime;
            yield return null;
        }
        doorTransform.position = endPos;
        doorState = currentState;
    }
}
