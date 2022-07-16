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
}
