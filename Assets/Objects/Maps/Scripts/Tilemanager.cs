using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tilemanager : MonoBehaviour
{

    [SerializeField]
    private List<Tiledata> tileDatas;

    private Dictionary<TileBase, Tiledata> dataFromTiles;
    
    private void Awake(){
        dataFromTiles = new Dictionary<TileBase, Tiledata>();

        foreach (var tileData in tileDatas){
            foreach(var tile in tileData.tiles){
                dataFromTiles.Add(tile, tileData);
            }
        }
    }

    public int[,] tileMatrix; 
    public Tilemap tileMap = null;

    // Start is called before the first frame update
    void Start()
    {
        BoundsInt bounds = tileMap.cellBounds;
        tileMatrix = new int[bounds.size.y, bounds.size.x];

        TileBase[] allTiles = tileMap.GetTilesBlock(bounds);

        for (int x = bounds.size.x-1; x >= 0; x--) {
            for (int y = 0; y < bounds.size.y; y++) {
                TileBase tile = allTiles[x + y * bounds.size.x];
                
                if(dataFromTiles[tile].wall)
                {
                    Grid grid = tileMap.transform.parent.GetComponentInParent<Grid>();
                    Vector3 tilePos = grid.GetCellCenterWorld(new Vector3Int(x, y, 0));
                    GameObject newWall = Instantiate(dataFromTiles[tile].wallObject);
                    newWall.transform.position = tilePos;
                }
                if (tile != null) {
                    if(tile.name == "Red")
                    {
                        tileMatrix[y, x] = 1;
                    }
                } else {
                    Debug.Log("Error: No tile found");
                }
            }
        }
        string printString = "";
        for (int i = 0; i < tileMatrix.GetLength(0); i++)
        {
            printString += "{";
            for (int p = 0; p < tileMatrix.GetLength(1); p++)
            {
                printString += tileMatrix[i, p] + ", ";
            }
            printString += "}";
        }
        Debug.Log(printString);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            BoundsInt bounds = tileMap.cellBounds;
        tileMatrix = new int[bounds.size.y, bounds.size.x];

        TileBase[] allTiles = tileMap.GetTilesBlock(bounds);

        tileMap.SetTile(Vector3Int.up, allTiles[0 + 0 * bounds.size.x]);
        tileMap.SetTile(Vector3Int.zero, allTiles[0 + 0 * bounds.size.x]);
        Debug.Log("Runs");
        }
    }
}