using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
	public float howMuchItMoves;

	public int[] diceValues = new int[6];

	int currentTop;
	int currentBot;

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

	void goUp() {
		var d0 = diceValues[0];
		var d3 = diceValues[3];
		var d5 = diceValues[5];
		var d2 = diceValues[2];

		diceValues[0] = d3;
		diceValues[3] = d5;
		diceValues[5] = d2;
		diceValues[2] = d0;

		transform.Rotate(90f, 0f, 0f, Space.World);
	}

	void goDown() {
		var d0 = diceValues[0];
		var d3 = diceValues[3];
		var d5 = diceValues[5];
		var d2 = diceValues[2];

		diceValues[0] = d2;
		diceValues[3] = d0;
		diceValues[5] = d3;
		diceValues[2] = d5;

		transform.Rotate(-90f, 0f, 0f, Space.World);
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

	void goLeft() {
		var d0 = diceValues[0];
		var d1 = diceValues[1];
		var d4 = diceValues[4];
		var d5 = diceValues[5];

		diceValues[0] = d4;
		diceValues[1] = d0;
		diceValues[4] = d5;
		diceValues[5] = d1;

		transform.Rotate(0f, 0f, 90f, Space.World);
	}

	void goRight() {
		var d0 = diceValues[0];
		var d1 = diceValues[1];
		var d4 = diceValues[4];
		var d5 = diceValues[5];

		diceValues[0] = d1;
		diceValues[1] = d5;
		diceValues[4] = d0;
		diceValues[5] = d4;

		transform.Rotate(0f, 0f, -90f, Space.World);
	}

    // Start is called before the first frame update
    void Start()
    {
		updateTopAndBot();
    }

	void updateTopAndBot() {
        currentTop = diceValues[0];
        currentBot = diceValues[5];
	}

    // Update is called once per frame
    void Update()
    {
		// left
		if(Input.GetKeyDown(KeyCode.A)) {
			transform.position += Vector3.left * howMuchItMoves;
			goLeft();
		}

		// right
		if(Input.GetKeyDown(KeyCode.D)) {
			transform.position += Vector3.right * howMuchItMoves;
			goRight();
		}

		// up
		if(Input.GetKeyDown(KeyCode.W)) {
			transform.position += Vector3.forward * howMuchItMoves;
			goUp();
		}

		// down
		if(Input.GetKeyDown(KeyCode.S)) {
			transform.position += Vector3.back * howMuchItMoves;
			goDown();
		}

		updateTopAndBot();

		//print("Top is now: " + currentTop);
		//print("Bot is now: " + currentBot);
    }
}
