using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flip : MonoBehaviour
{
	
	bool noiseA = false;
	bool noiseB = false;
	bool noiseC = false;
	public AudioSource[] flipNoises;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void makeNoise(int j) {
		switch(j) {
			case 1:
				if(noiseA) {
					return;
				}
				noiseA = true;
				break;
			case 2:
				if(noiseB) {
					return;
				}
				noiseB = true;
				break;
			case 3:
				if(noiseC) {
					return;
				}
				noiseC = true;
				break;
		}

		if(flipNoises.Length == 0){
			print("BRUH FLIP");
			return;
		}

		var i = Random.Range(0, flipNoises.Length);
		print("Playing " + i);
		flipNoises[i].Play();
	}
}
