using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
	public int[] diceValues = new int[6];

	public float rotationTime = 1f;

	bool isAnimating = false;

	int currentTop;
	int currentBot;

	public int top() {
		return currentTop;
	}

	public int bot() {
		return currentBot;
	}

	/*
	// regular
	  1
	2 4 5
	  6
	  3

	// indicies
	  0
	1 3 4
	  5
	  2
	*/

	public bool isMoving() {
		return isAnimating;
	}

	IEnumerator animate(Vector3 desiredRotation, Vector3 desiredTranslation) {
		var startRotation = transform.rotation;
		transform.Rotate(desiredRotation, Space.World);
		var endRotation = transform.rotation;
		transform.Rotate(-desiredRotation, Space.World);

		var startPosition = transform.position;
		var endPosition = transform.position + desiredTranslation;

		float t = 0f;

		isAnimating = true;
		while(t < rotationTime) {
			var delta = Time.deltaTime;
			t += delta;
			var s = t / rotationTime;
			var rotationThisFrame = Quaternion.Slerp(startRotation, endRotation, s);
			var positionThisFrame = Vector3.Slerp(startPosition, endPosition, s);

			var yAxisOffset = Mathf.Sin(s * Mathf.PI) * 0.2f;

			transform.rotation = rotationThisFrame;
			transform.position = positionThisFrame;
			transform.position += Vector3.up * yAxisOffset;
			yield return null;
		}
		transform.position = endPosition;
		isAnimating = false;
	}

	public void goUp() {
		var d0 = diceValues[0];
		var d3 = diceValues[3];
		var d5 = diceValues[5];
		var d2 = diceValues[2];

		diceValues[0] = d3;
		diceValues[3] = d5;
		diceValues[5] = d2;
		diceValues[2] = d0;
		updateTopAndBot();

		StartCoroutine(animate(
			new Vector3(90f, 0f, 0f),
			Vector3.forward
		));
	}

	public void goDown() {
		var d0 = diceValues[0];
		var d3 = diceValues[3];
		var d5 = diceValues[5];
		var d2 = diceValues[2];

		diceValues[0] = d2;
		diceValues[3] = d0;
		diceValues[5] = d3;
		diceValues[2] = d5;
		updateTopAndBot();

		StartCoroutine(animate(
			new Vector3(-90f, 0f, 0f),
			Vector3.back
		));
	}

	/*
	// regular
	  1
	2 4 5 3
	  6

	// indicies
	  0
	1 3 4 2
	  5
	*/

	public void goLeft() {
		var d0 = diceValues[0];
		var d1 = diceValues[1];
		var d4 = diceValues[4];
		var d5 = diceValues[5];

		diceValues[0] = d4;
		diceValues[1] = d0;
		diceValues[4] = d5;
		diceValues[5] = d1;
		updateTopAndBot();

		StartCoroutine(animate(
			new Vector3(0f, 0f, 90f),
			Vector3.left
		));
	}

	public void goRight() {
		var d0 = diceValues[0];
		var d1 = diceValues[1];
		var d4 = diceValues[4];
		var d5 = diceValues[5];

		diceValues[0] = d1;
		diceValues[1] = d5;
		diceValues[4] = d0;
		diceValues[5] = d4;
		updateTopAndBot();

		StartCoroutine(animate(
			new Vector3(0f, 0f, -90f),
			Vector3.right
		));
	}

    // Start is called before the first frame update
    void Start()
    {
		updateTopAndBot();
    }

	void updateTopAndBot() {
        currentTop = diceValues[0];
        currentBot = diceValues[5];
		//print("Top is now: " + currentTop);
		//print("Bot is now: " + currentBot);
	}

    // Update is called once per frame
    void Update()
    {
    }
}
