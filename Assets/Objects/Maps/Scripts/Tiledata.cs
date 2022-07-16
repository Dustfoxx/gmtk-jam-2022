using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu]
public class Tiledata : ScriptableObject
{
    
    public TileBase[] tiles;

    public bool walkable, rollable, wall;

    public GameObject wallObject = null;

    // void Start(){
    //     if(wall){
    //         GameObject newBox = Instantiate(wallObject);
    //         newBox.transform.position = new Vector3(tiles[0].position.x, tiles[0].position.y, tiles[0].position.z);
    //     }
    // }

}
