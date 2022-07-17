using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [SerializeField]
    public GameObject[] environmentObjects;
    public GameObject[] rareObjects;

    private List<GameObject> objectList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        var amount = Random.Range(1, 3);
        for(int i = 0; i < amount; i++){
            GameObject tempControl;
            if(Random.Range(1, 100) > 95){
                tempControl = Instantiate(rareObjects[Random.Range(0, rareObjects.Length - 1)]);
            }
            else
            {
                tempControl = Instantiate(environmentObjects[Random.Range(0, environmentObjects.Length - 1)]);
            }
            

            float xval = (float)Random.Range(0, 99)/100f - 0.5f;
            float yval = (float)Random.Range(0, 99)/100f - 0.5f;

            float scale = (float)Random.Range(0, 40)/100f;

            tempControl.transform.localScale = tempControl.transform.localScale - Vector3.one/5 + new Vector3(scale, scale, scale);
            tempControl.transform.position = transform.position + Vector3.up/2 + new Vector3(xval, 0, yval);

            objectList.Add(tempControl);
        }
    }

    void OnDestroy(){
        for(int i = 0; i < objectList.Count; i++){
            Destroy(objectList[i]);
        }
    }
}
