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
    private Dictionary<int, List<Vector2Int>> affectedTiles = new Dictionary<int, List<Vector2Int>>();

	private GameObject[][] spawnedWalls;


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
    public AudioSource[] flipSfx;
    int lastflip = 0;
	public AudioSource[] noiseSfx;
    int lastNoise = 0;
    public AudioSource[] barkSfx;
    int lastBark = 0;
	public AudioSource[] rollSfx;
    int lastRoll = 0;
	public AudioSource[] footstepSfx;
    int lastFootstep = 0;
    public Dice dice; 
    public Player player; 
	public float switchFadeOutTime = 1f;
	public GameObject exit;
	public TileBase regularTile;
    public TileBase wallTile;
    

    private float t = 0f;
    private float footstepTime = 0f;
    private int noiseInterval = 5;
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

    void playSoundFromArr(AudioSource[] noises, int lastPlayed){
        int choice = 0;
        do{
            choice = (int)Random.Range(0, noises.Length - 1);
        }while(choice == lastPlayed);

        noises[choice].Play();
    }

    // Start is called before the first frame update
    void Start()
    {
        bounds = tileMap.cellBounds;
		spawnedWalls = new GameObject[bounds.size.x][];
		for(int i = 0; i < bounds.size.x; i++)
		{
			spawnedWalls[i] = new GameObject[bounds.size.y];
		}
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

		resetSpecialTiles();
    }


    // Update is called once per frame
    void Update()
    {

        if(player.isMoving()){
            var footDelta = Time.deltaTime;
            footstepTime += footDelta;
            if(footstepTime > 1){
                playSoundFromArr(footstepSfx, lastFootstep);
                footstepTime = 0f;
            }
        }
		if(player.isMoving() || dice.isMoving()) {
			return;
		}

        var delta = Time.deltaTime;
        t += delta;
        var s = t / noiseInterval;
        if(Random.Range(0, 500) < s*100){
            playSoundFromArr(noiseSfx, lastNoise);
            t = 0f;
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
                playSoundFromArr(rollSfx, lastRoll);
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
		if(dataFromTiles[tile].stageSwitch) {
			tryTriggerSwitch(here, dataFromTiles[tile].functionNumber, tile);
			return;
		}
	}

	
	void tryTriggerSwitch(Vector2Int here, int key, TileBase tile) {
		if(key == 2){
            keyUnlock(here, key);
        }
        else if (key == 7){
            revealHiddenWalls(here, tile);
        }
	}


    void keyUnlock(Vector2Int here, int key){
        if(key != dice.top()) {
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

    void revealHiddenWalls(Vector2Int here, TileBase tile){
        set(here.x, here.y, regularTile);
        List<Vector2Int> tiles = affectedTiles[dataFromTiles[tile].id];
        for(int i = 0; i < tiles.Count; i++){
            set(tiles[i].x, tiles[i].y, wallTile);
        }
        print("arrived");
        createMesh();
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
        allTiles = tileMap.GetTilesBlock(bounds);
        for (int x = bounds.size.x-1; x >= 0; x--) {
            for (int y = 0; y < bounds.size.y; y++) {
                var tile = get(x, y);
                if(dataFromTiles[tile].wall)
                {
					if(!spawnedWalls[x][y])
					{
                    	Grid grid = tileMap.transform.parent.GetComponentInParent<Grid>();
                    	Vector3 tilePos = grid.GetCellCenterWorld(new Vector3Int(x, y, 0));
                    	GameObject newWall = Instantiate(dataFromTiles[tile].wallObject);
                    	newWall.transform.position = tilePos;
						spawnedWalls[x][y] = newWall;
					}
                }
                if(tile.name == "Start"){
                    playerPos = new Vector3(x, 0, y);
                }
                if(tile.name == "End"){
                    exitPos = new Vector2Int(x, y);
                }
                if(tile.name == "diceStart"){
                    dicePos = new Vector3(x, 0, y);
                }
                if(dataFromTiles[tile].interactable){
                    if(affectedTiles.ContainsKey(dataFromTiles[tile].id)){
                        affectedTiles[dataFromTiles[tile].id].Add(new Vector2Int(x, y));
                    } else {
                        List<Vector2Int> tempList = new List<Vector2Int>();
                        tempList.Add(new Vector2Int(x, y));
                        affectedTiles.Add(dataFromTiles[tile].id, tempList);
                        print(dataFromTiles[tile].id);
                    }
                }
				if (!dataFromTiles[tile].wall && spawnedWalls[x][y] != null){
					Destroy(spawnedWalls[x][y]);
				}
            }
        }
	}
    

	void resetSpecialTiles() {
		for(int x = 0; x < width(); x++) {
			for(int y = 0; y < height(); y++) {
				var tile = get(x, y);
				if(tile.name == "Start"
					||tile.name == "End"
					|| tile.name == "diceStart") {
					set(x, y, regularTile);
				}
			}
		}
	}
}
