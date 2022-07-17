using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOrbitScript : MonoBehaviour
{
	public Vector3 origin;
	public float orbitRadius;
	public float secondsPerFullCycle;
	public float percentOffset;

	float xModifier = 2.0f;

	float time = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		const float wholeLap = Mathf.PI * 2f;
		time += Time.deltaTime;
		var scale = percentOffset + time / secondsPerFullCycle;
        var x = Mathf.Cos(wholeLap * scale);
		var y = Mathf.Sin(wholeLap * scale);
		var direction = new Vector3(x * xModifier, y, 0f);
		transform.position = origin + direction * orbitRadius;
    }
}
