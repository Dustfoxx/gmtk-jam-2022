using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowScript : MonoBehaviour
{
	public float smoothness = 5f;
	public GameObject target;
	public Vector3 cameraOffset = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var startPosition = transform.position;
		var endPosition = target.transform.position + cameraOffset;

		transform.position = Vector3.Slerp(startPosition, endPosition, smoothness * Time.deltaTime);
    }
}
