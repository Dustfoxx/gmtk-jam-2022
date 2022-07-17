using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

	public float walkingTime = 0.3f;

	Animator animator;
	bool isAnimating = false;
	bool isOnHill = false;
	SpriteRenderer sprite;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
		sprite = GetComponent<SpriteRenderer>();
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

		if(desiredTranslation.z > 0f) {
			animator.SetFloat("yDirection", 1f);
		} else if(desiredTranslation.z < 0f) {
			animator.SetFloat("yDirection", -1f);
		} else {
			animator.SetFloat("yDirection", 0f);
		}

		if(desiredTranslation.x > 0f) {
			animator.SetFloat("xDirection", 1f);
			sprite.flipX = false;
		} else if(desiredTranslation.x < 0f) {
			animator.SetFloat("xDirection", 1f);
			sprite.flipX = true;
		} else {
			animator.SetFloat("xDirection", 0f);
			sprite.flipX = false;
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

	Vector3 hillDiff(bool hill) {
		const float d = 0.2f;
		if(hill && isOnHill || !hill && !isOnHill) {
			return Vector3.zero;
		} else if(!hill && isOnHill) {
			return Vector3.down * d;
		}
		return Vector3.up * d;
	}

	public void goUp(bool hill) {
		StartCoroutine(animate(Vector3.forward + hillDiff(hill)));
		isOnHill = hill;
	}

	public void goDown(bool hill) {
		StartCoroutine(animate(Vector3.back + hillDiff(hill)));
		isOnHill = hill;
	}

	public void goLeft(bool hill) {
		StartCoroutine(animate(Vector3.left + hillDiff(hill)));
		isOnHill = hill;
	}

	public void goRight(bool hill) {
		StartCoroutine(animate(Vector3.right + hillDiff(hill)));
		isOnHill = hill;
	}

	IEnumerator push(float x, float y) {
		const float pushTime = 0.5f;
		float t = 0f;
		isAnimating = true;
		animator.SetBool("isPushing", true);

		if(y > 0f) {
			animator.SetFloat("yDirection", 1f);
		} else if(y < 0f) {
			animator.SetFloat("yDirection", -1f);
		} else {
			animator.SetFloat("yDirection", 0f);
		}

		if(x > 0f) {
			animator.SetFloat("xDirection", 1f);
			sprite.flipX = false;
		} else if(x < 0f) {
			animator.SetFloat("xDirection", 1f);
			sprite.flipX = true;
		} else {
			animator.SetFloat("xDirection", 0f);
			sprite.flipX = false;
		}
		while(t < pushTime) {
			t += Time.deltaTime;

			yield return null;
		}
		isAnimating = false;
		animator.SetBool("isPushing", false);
	}

	public void pushRight() {

	}

	public void pushLeft() {

	}

	public void stop() {
		animator.gameObject.SetActive(false);
		animator.gameObject.SetActive(true);
	}
}
