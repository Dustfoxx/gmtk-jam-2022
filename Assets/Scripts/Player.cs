using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

	public float walkingTime = 0.3f;

	Animator animator;
	bool isAnimating = false;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public bool isMoving() {
		return isAnimating;
	}

	IEnumerator animate(Vector3 desiredTranslation) {
		var startPosition = transform.position;
		var endPosition = transform.position + desiredTranslation;

		float t = 0f;

		if(desiredTranslation.z > 0) {
			animator.SetFloat("yDirection", 1f);
			animator.SetFloat("xDirection", 0f);
		} else if(desiredTranslation.z < 0) {
			animator.SetFloat("yDirection", -1f);
			animator.SetFloat("xDirection", 0f);
		} else {
			animator.SetFloat("yDirection", 0f);
		}

		if(desiredTranslation.x > 0) {
			animator.SetFloat("xDirection", 1f);
		} else if(desiredTranslation.x < 0) {
			animator.SetFloat("xDirection", -1f);
		} else {
			animator.SetFloat("xDirection", 0f);
		}

		animator.SetBool("isWalking", true);
		isAnimating = true;
		while(t < walkingTime) {
			var delta = Time.deltaTime;
			t += delta;
			var s = t / walkingTime;
			var positionThisFrame = Vector3.Slerp(startPosition, endPosition, s);
			transform.position = positionThisFrame;
			yield return null;
		}
		transform.position = endPosition;
		isAnimating = false;
		animator.SetBool("isWalking", false);
	}

	public void goUp() {
		StartCoroutine(animate(Vector3.forward));
	}

	public void goDown() {
		StartCoroutine(animate(Vector3.back));
	}

	public void goLeft() {
		StartCoroutine(animate(Vector3.left));
	}

	public void goRight() {
		StartCoroutine(animate(Vector3.right));
	}

	public void stop() {
		animator.gameObject.SetActive(false);
		animator.gameObject.SetActive(true);
	}
}
