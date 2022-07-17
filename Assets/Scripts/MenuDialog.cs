using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuDialog : MonoBehaviour
{

	public string startGameScene;
	public GameObject border;
	public AudioSource moveCursorSfx;
	public AudioSource selectSfx;
	public Vector3 menuDelta;
	public GameObject gradient;
	public float gradientInOutTime = 1f;
	float time = 0f;

	SpriteRenderer gradientSprite;

	const int start = 0;
	const int howToPlay = 1;
	int selected = start;
	bool locked = false;

    // Start is called before the first frame update
    void Start()
    {
        gradientSprite = gradient.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
		if(locked) {
			return;
		}

		time += Time.deltaTime;

		if(time < gradientInOutTime) {
			float scale = 1f - (time / gradientInOutTime);
			var color = gradientSprite.color;
			color.a = scale;
			gradientSprite.color = color;
			return;
		}

		var moving = Input.GetKeyDown(KeyCode.W) ||Input.GetKeyDown(KeyCode.S);
		if(moving) {
			if(selected == start) {
				selected = howToPlay;
				border.transform.position -= menuDelta;
			} else {
				selected = start;
				border.transform.position += menuDelta;
			}

			moveCursorSfx.Play();

			/*
			if(Input.GetKeyDown(KeyCode.W)) {

			} else if(Input.GetKeyDown(KeyCode.S)) {

			}
			*/
		}

		var yes = Input.GetKeyDown(KeyCode.Space) ||Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E);
		if(yes) {
			selectSfx.Play();
			switch(selected) {
				case start:
					StartCoroutine(update2ElectricBoogaloo());
					break;
				case howToPlay:
					break;
			}
		}
    }

	IEnumerator update2ElectricBoogaloo() {
		locked = true;
		time = 0f;
		while(time < gradientInOutTime) {
			time += Time.deltaTime;
			var scale = time / gradientInOutTime;
			var color = gradientSprite.color;
			color.a = scale;
			gradientSprite.color = color;
			yield return null;
		};
		
		SceneManager.LoadScene(startGameScene);
	}
}
