using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereScript : MonoBehaviour
{
	public Camera myCamera;
	public Rigidbody body;
	public float forceModifier;
	public bool isControlled = false;
	public bool isBullied = false;
	public AudioSource[] onCollisionSounds;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
		if(!isControlled) {
			return;
		}

		var v = forceModifier * Time.deltaTime;
		var delta = new Vector3();

		if(Input.GetKey(KeyCode.A)) {
			delta = Vector3.left * v;
		}
		if(Input.GetKey(KeyCode.S)) {
			delta = Vector3.back * v;
		}
		if(Input.GetKey(KeyCode.D)) {
			delta = Vector3.right * v;
		}
		if(Input.GetKey(KeyCode.W)) {
			delta = Vector3.forward * v;
		}

		body.AddForce(delta);
    }

   void OnCollisionEnter(Collision collision)
    {
		if(!isBullied) {
			return;
		}

		if(collision.gameObject.name != "linda") {
			return;
		}

		int i = Random.Range(0, onCollisionSounds.Length);
		var source = onCollisionSounds[i];
		source.Play();
    }
}
