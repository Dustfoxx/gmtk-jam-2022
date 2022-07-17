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

    public Tilemap tileMap = null;
	public string nextSceneIfCompleted = "Level1";
    public Vector3 playerPos = Vector3.zero;
    public Vector3 dicePos = Vector3.zero;
	public Vector2Int exitPos;
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
	public Flip flip;
	public CameraFollowScript camera;
	public GameObject unrollableBump;
    

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

	public bool getIsRollable(int x, int y) {
		var tile = get(x, y);
        return dataFromTiles[tile].rollable;
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
		dataFromTiles = new Dictionary<TileBase, Tiledata>();

        foreach (var tileData in tileDatas){
            foreach(var tile in tileData.tiles){
                dataFromTiles.Add(tile, tileData);
            }
        }

		flipSprite = flip.GetComponent<SpriteRenderer>();
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
				var isKey = false
					||tile.name == "switch-1"
					||tile.name == "switch-2"
					||tile.name == "switch-3"
					||tile.name == "switch-4"
					||tile.name == "switch-5"
					||tile.name == "switch-6";
				if(isKey) {
					nKeys++;
				}
			}
		}

		resetSpecialTiles();
		camera.target = flip.gameObject;
		flip.transform.position = exit.transform.position;
    }


    // Update is called once per frame
    void Update()
    {
		if(doingFlipAnimation) {
			doFlipAnimation();
			return;
		}
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

        var playerWall = getIsWall((int)proposedPos.x, (int)proposedPos.z);

        bool playerGrabResult = grab && Mathf.Round(rdp.x + motion.x) == Mathf.Round(rpp.x-motion.x) 
        && Mathf.Round(rdp.z + motion.z)  == Mathf.Round(rpp.z - motion.z) && !playerWall;
		bool didPush = false;

		if(Mathf.Round(rpp.x) == Mathf.Round(rdp.x) && Mathf.Round(rpp.z) == Mathf.Round(rdp.z) 
        || playerGrabResult) {
			var proposedDicePos = dice.transform.position + motion;
			var proposedDiceCoords = new Vector2Int((int)(proposedDicePos.x), (int)proposedDicePos.z);
            var spacesToWall = 0;
            while (!playerGrabResult && !getIsWall(proposedDiceCoords.x + (int)motion.x*spacesToWall, proposedDiceCoords.y + (int)motion.z*spacesToWall)
			&& getIsRollable(proposedDiceCoords.x + (int)motion.x*spacesToWall, proposedDiceCoords.y + (int)motion.z*spacesToWall)){
                spacesToWall++;
            }
            if(spacesToWall != 0)
                spacesToWall--;
            dice.spacesToWall = spacesToWall;

			if(getIsWall(proposedDiceCoords.x, proposedDiceCoords.y)
				|| !getIsRollable(proposedDiceCoords.x, proposedDiceCoords.y)) {
				player.stop();
				return;
            } else {
				didPush = true;
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
			if(!didPush) {
				var playerHill = !getIsRollable((int)proposedPos.x, (int)proposedPos.z);
				switch(dir)
				{
					case Direction.Up:
						player.goUp(playerHill);
						break;
					case Direction.Down:
						player.goDown(playerHill);
						break;
					case Direction.Left:
						player.goLeft(playerHill);
						break;
					case Direction.Right:
						player.goRight(playerHill);
						break;
					case Direction.None:
						player.stop();
						break;
				}
			} else {
				switch(dir) {
					case Direction.Up:
						player.goUp(false);
						break;
					case Direction.Down:
						player.goDown(false);
						break;
					case Direction.Left:
						player.goLeft(false);
						//player.pushLeft();
						break;
					case Direction.Right:
						player.goRight(false);
						//player.pushRight();
						break;
					case Direction.None:
						player.stop();
						break;
				}
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
		switch(key) {
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
				keyUnlock(here, key, tile);
				break;
			case 7:
				revealHiddenWalls(here, tile);
				break;
		}
	}

	HashSet<Vector2Int> unlockedKeys = new HashSet<Vector2Int>();

    void keyUnlock(Vector2Int here, int key, TileBase tile){
		if(unlockedKeys.Contains(here)) {
			return;
		}

        if(key != dice.top()) {
			keyUnlockFailSfx.Play();
			return;
		}

		//TODO
		/*
		StartCoroutine(fadeOutSwitch(index));
		*/

		unlockedKeys.Add(here);

		revealHiddenWalls(here, tile);
		//var tile = get(here);
		//dataFromTiles[tile].stageSwitch = false;
		set(here.x, here.y, regularTile);
		keyUnlockSfx.Play();
		nKeys--;

		if(nKeys == 0) {
			exit.SetActive(true);
		}
    }

    void revealHiddenWalls(Vector2Int here, TileBase tile){
        set(here.x, here.y, regularTile);
		var id = dataFromTiles[tile].id;
		if(!affectedTiles.ContainsKey(id)) {
			return;
		}
        List<Vector2Int> tiles = affectedTiles[id];
        for(int i = 0; i < tiles.Count; i++){
            set(tiles[i].x, tiles[i].y, regularTile);
        }
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
				var data = dataFromTiles[tile];
                if(data.wall)
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
				if(!data.rollable) {
                    Grid grid = tileMap.transform.parent.GetComponentInParent<Grid>();
                    Vector3 tilePos = grid.GetCellCenterWorld(new Vector3Int(x, y, 0));
					tilePos.y = tilePos.y - 0.45f;
					GameObject newBump = Instantiate(unrollableBump);
					newBump.transform.position = tilePos;
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


	bool doingFlipAnimation = true;
	float flipTime = 0f;
	SpriteRenderer flipSprite;
	void doFlipAnimation() {
		if(Input.GetKey(KeyCode.E)) {
			endFlipAnimation();
			return;
		}

		const float beginJumpKeyFrame = 1.0f;
		const float endJumpKeyFrame = 2.0f;

		const float turnLeftKeyFrame = 3.0f;
		const float turnRightKeyFrame = 4.0f;
		const float turnLeftAgainKeyFrame = 5.0f;

		const float teleportKeyFrame = 6.0f;
		const float raiseKeyFrame = 6.5f;
		const float endKeyFrame = 7.0f;

		flipTime += Time.deltaTime;
		if(flipTime > beginJumpKeyFrame) {
			flip.makeNoise(1);
			const float flipJumpPeak = 0.2f;
			const float cycle = Mathf.PI * 2f;
			const float nJumps = 3f;
			var position = flip.transform.position;
			var step = (flipTime - beginJumpKeyFrame);
			var scale = 1f + Mathf.Sin((Mathf.PI * -0.5f) + (cycle * (step * step)) * nJumps);
			var y = flipJumpPeak * scale;
			position.y = exit.transform.position.y + y;
			flip.transform.position = position;
		}
		if(flipTime > endJumpKeyFrame) {
			flip.transform.position = exit.transform.position;
		}
		if(flipTime > turnLeftKeyFrame) {
			flipSprite.flipX = false;
		}
		if(flipTime > turnRightKeyFrame) {
			flip.makeNoise(2);
			flipSprite.flipX = true;
		}
		if(flipTime > turnLeftAgainKeyFrame) {
			flipSprite.flipX = false;
		}
		if(flipTime > teleportKeyFrame) {
			flip.makeNoise(3);
			var s = 1f - (flipTime - teleportKeyFrame) / (endKeyFrame - teleportKeyFrame);
			var scale = flip.transform.localScale;
			scale.x = s;
			flip.transform.localScale = scale;
		}
		if(flipTime > raiseKeyFrame) {
			var s = (flipTime - raiseKeyFrame) / (endKeyFrame - raiseKeyFrame);
			var position = flip.transform.position;
			position.y = exit.transform.position.y + s;
			flip.transform.position = position;
		}
		if(flipTime > endKeyFrame) {
			endFlipAnimation();
		}
	}
	
	void endFlipAnimation() {
		
		playSoundFromArr(flipSfx, lastflip);
		doingFlipAnimation = false;
		camera.target = player.gameObject;
		flip.gameObject.SetActive(false);
	}

	void OnDestroy(){
		for(int i = 0; i < bounds.size.x; i++)
		{
			for(int p = 0; p < spawnedWalls[i].Length; p++)
			{
				Destroy(spawnedWalls[i][p]);
			} 
		}
	}
}
