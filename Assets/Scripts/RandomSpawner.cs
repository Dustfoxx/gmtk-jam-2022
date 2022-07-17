using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [SerializeField]
    public GameObject[] environmentObjects;

    // Start is called before the first frame update
    void Start()
    {
        var amount = Random.Range(2, 5);
        for(int i = 0; i < amount; i++){
            GameObject tempControl = Instantiate(environmentObjects[Random.Range(0, environmentObjects.Length - 1)]);

            float xval = (float)Random.Range(0, 99)/100f - 0.5f;
            float yval = (float)Random.Range(0, 99)/100f - 0.5f;

            print(xval + " " + yval);

            tempControl.transform.position = transform.position + Vector3.up/2 + new Vector3(xval, 0, yval);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
