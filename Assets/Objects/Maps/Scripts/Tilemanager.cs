using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public enum Direction {
	None,
	Up,
	Down,
	Left,
	Right,
};

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
	public string nextSceneIfCompleted = "Level1";
    public Vector3 playerPos = Vector3.zero;
    public Vector3 dicePos = Vector3.zero;
	public Vector2Int exitPos;
	public GameObject key2Switch;
	public AudioSource keyUnlockSfx;
	public AudioSource keyUnlockFailSfx;
    public Dice dice; 
    public Player player; 
	public float switchFadeOutTime = 1f;
	public GameObject exit;
	public TileBase regularTile;
    
    private bool grab = false;

    Vector3 playerOffset = new Vector3(0.5f, 1f, 0.29f);
    Vector3 diceOffset = new Vector3(0.5f, 0.5f, 0.5f);

	int nKeys = 0;

	TileBase[] allTiles;
	BoundsInt bounds;

	public TileBase get(int x, int y) {
		return allTiles[x + y * bounds.size.x];
	}

	public TileBase get(Vector2Int here) {
		return get(here.x, here.y);
	}

	public bool getIsWall(int x, int y) {
		var tile = get(x, y);
        return dataFromTiles[tile].wall;
	}

	public void set(int x, int y, TileBase tile) {
		tileMap.SetTile(new Vector3Int(x, y, 0), tile);
	}

	public int width() {
		return bounds.size.x;
	}

	public int height() {
		return bounds.size.y;
	}

    // Start is called before the first frame update
    void Start()
    {
        bounds = tileMap.cellBounds;
        allTiles = tileMap.GetTilesBlock(bounds);
		createMesh();

        player.transform.position = playerPos + playerOffset;
		dice.transform.position = dicePos + diceOffset;
		//initSwitches();
		exit.transform.position = new Vector3((float)exitPos.x, 0f, (float)exitPos.y)
			+ new Vector3(0.5f, 0.5f, 0.1f);
		exit.SetActive(false);

		for(int x = 0; x < width(); x++) {
			for(int y = 0; y < height(); y++) {
				var tile = get(x, y);
				if(tile.name == "switch-2") {
					nKeys++;
				}
			}
		}
    }


    // Update is called once per frame
    void Update()
    {

		if(player.isMoving() || dice.isMoving()) {
			return;
		}

		Vector3 motion = Vector3.zero;
		Direction dir = Direction.None;

		if (Input.GetKey(KeyCode.W)) {
			motion += Vector3.forward;
			dir = Direction.Up;
		} else if (Input.GetKey(KeyCode.S)) {
			motion += Vector3.back;
			dir = Direction.Down;
		} else if (Input.GetKey(KeyCode.A)) {
			motion += Vector3.left;
			dir = Direction.Left;
		} else if (Input.GetKey(KeyCode.D)) {
			motion += Vector3.right;
			dir = Direction.Right;
		} else if (Input.GetKeyDown(KeyCode.R)) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

        if (Input.GetKey(KeyCode.E)) {
			grab = true;
		} else{
            grab = false;
        }


		var gridWidth = width();
		var gridHeight = height();
		Vector3 proposedPos = player.transform.position + motion;
		//prevent player leaving the grid.
		proposedPos.x = proposedPos.x < 0 ? 0 : proposedPos.x;
		proposedPos.x = proposedPos.x > gridWidth - 1 ? gridWidth - 1 : proposedPos.x;
		proposedPos.z = proposedPos.z < 0 ? 0 : proposedPos.z;
		proposedPos.z = proposedPos.z > gridHeight - 1 ? gridHeight - 1 : proposedPos.z;

		var rpp = proposedPos - playerOffset;
		var rdp = dice.transform.position - diceOffset;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A)) {
            if(grab){
                print(grab);
                print(Mathf.Round(rpp.x - motion.x));
                print(Mathf.Round(rdp.x));
                print(Mathf.Round(rpp.z - motion.z));
                print(Mathf.Round(rdp.z));
            }
		}

        var playerWall = getIsWall((int)proposedPos.x, (int)proposedPos.z);

        bool playerGrabResult = grab && Mathf.Round(rdp.x + motion.x) == Mathf.Round(rpp.x-motion.x) 
        && Mathf.Round(rdp.z + motion.z)  == Mathf.Round(rpp.z - motion.z) && !playerWall;

		if(Mathf.Round(rpp.x) == Mathf.Round(rdp.x) && Mathf.Round(rpp.z) == Mathf.Round(rdp.z) 
        || playerGrabResult) {
			var proposedDicePos = dice.transform.position + motion;
			var proposedDiceCoords = new Vector2Int((int)(proposedDicePos.x), (int)proposedDicePos.z);
            var spacesToWall = 0;
            while (!playerGrabResult && !getIsWall(proposedDiceCoords.x + (int)motion.x*spacesToWall, proposedDiceCoords.y + (int)motion.z*spacesToWall)){
                spacesToWall++;
            }
            if(spacesToWall != 0)
                spacesToWall--;
            dice.spacesToWall = spacesToWall;

			if(getIsWall(proposedDiceCoords.x, proposedDiceCoords.y)) {
				player.stop();
				return;
            } else {
				switch(dir) {
					case Direction.Up:
						dice.goUp();
						break;
					case Direction.Down:
						dice.goDown();
						break;
					case Direction.Left:
						dice.goLeft();
						break;
					case Direction.Right:
						dice.goRight();
						break;
					case Direction.None:
						break;
				}

				checkIfSwitchesWereTriggered(proposedDiceCoords);
				checkIfExitIsReached(proposedDiceCoords);
			}

		}

        if (playerWall)
		{
			player.stop();
		} else {
			switch(dir)
			{
				case Direction.Up:
					player.goUp();
					break;
				case Direction.Down:
					player.goDown();
					break;
				case Direction.Left:
					player.goLeft();
					break;
				case Direction.Right:
					player.goRight();
					break;
				case Direction.None:
					player.stop();
					break;
			}
		}

    }


	void checkIfSwitchesWereTriggered(Vector2Int here) {
		var tile = get(here);
		if(tile.name == "switch-2") {
			tryTriggerSwitch(here, 2);
			return;
		}
	}

	
	void tryTriggerSwitch(Vector2Int here, int key) {
		if(key != dice.bot()) {
			keyUnlockFailSfx.Play();
			return;
		}

		//TODO
		/*
		StartCoroutine(fadeOutSwitch(index));
		*/


		set(here.x, here.y, regularTile);
		keyUnlockSfx.Play();
		nKeys--;

		if(nKeys == 0) {
			exit.SetActive(true);
		}
	}


	void checkIfExitIsReached(Vector2Int coords) {
		if(coords != exitPos) {
			return;
		}

		if(nKeys > 0) {
			return;
		}

		SceneManager.LoadScene(nextSceneIfCompleted);
	}

	void createMesh() {
        for (int x = bounds.size.x-1; x >= 0; x--) {
            for (int y = 0; y < bounds.size.y; y++) {
                var tile = get(x, y);
                if(dataFromTiles[tile].wall)
                {
                    Grid grid = tileMap.transform.parent.GetComponentInParent<Grid>();
                    Vector3 tilePos = grid.GetCellCenterWorld(new Vector3Int(x, y, 0));
                    GameObject newWall = Instantiate(dataFromTiles[tile].wallObject);
                    newWall.transform.position = tilePos;
                }
                else if(tile.name == "Start"){
                    playerPos = new Vector3(x, 0, y);
                }
                else if(tile.name == "End"){
                    exitPos = new Vector2Int(x, y);
                }
                else if(tile.name == "diceStart"){
                    dicePos = new Vector3(x, 0, y);
                }
            }
        }
	}

}
