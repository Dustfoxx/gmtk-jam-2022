using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu]
public class Tiledata : ScriptableObject
{
    
    public TileBase[] tiles;

    public bool walkable, rollable, wall, stageSwitch, interactable;

    public int functionNumber, id;

    public (int, int) coordinates = (0, 0);

    public GameObject wallObject = null;
}
