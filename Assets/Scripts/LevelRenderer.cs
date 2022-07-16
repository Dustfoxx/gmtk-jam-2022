using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public enum Direction {
	None,
	Up,
	Down,
	Left,
	Right,
};

public class Switch {
	public Switch(Vector2Int p, int k) {
		pos = p;
		key = k;
		triggered = false;
	}

	public GameObject obj;
	public Vector2Int pos;
	public int key;
	public bool triggered;
};

public class LevelRenderer : MonoBehaviour
{

	int[,] grid = new int[,]{
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 1,},
		{1, 0, 0, 0, 0, 0, 0, 0, 0, 1,},
		{1, 0, 0, 0, 0, 0, 0, 0, 0, 1,},
		{1, 0, 0, 0, 0, 0, 0, 0, 0, 1,},
		{1, 0, 0, 0, 0, 0, 0, 0, 0, 1,},
		{1, 0, 0, 0, 0, 0, 1, 1, 0, 1,},
		{1, 0, 0, 0, 0, 0, 0, 0, 0, 1,},
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 1,},
	};

	Switch[] switches = new Switch[]{
		new Switch(new Vector2Int(5, 2), 2),
	};


	public string nextSceneIfCompleted = "Level1";
    public Vector3 playerPos = Vector3.zero;
    public Vector3 dicePos = Vector3.zero;
	public Vector2Int exitPos = new Vector2Int(8, 6);

	public GameObject key2Switch;
	public AudioSource keyUnlockSfx;
	public AudioSource keyUnlockFailSfx;

    public Dice dice; 
    public Player player; 
	public float switchFadeOutTime = 1f;
	public GameObject exit;
    Vector3 playerOffset = new Vector3(0.5f, 1f, 0.29f);
    Vector3 diceOffset = new Vector3(0.5f, 0.5f, 0.5f);
    Texture2D tex;

	void initSwitches() {
		for(int i = 0; i < switches.Length; i++) {
			var p = switches[i].pos;
			var pos = new Vector3((float)p.x + 0.5f, 0.05f, (float)p.y + 0.5f);
			switch(switches[i].key) {
				case 2:
					switches[i].obj = Instantiate(key2Switch, pos, key2Switch.transform.rotation);
					switches[i].obj.transform.localScale *= 3f;
					break;
				default:
					print(switches[i].key);
					Assert.IsTrue(false);
					break;
			}
		}

	}

	void checkIfSwitchesWereTriggered(Vector2Int here) {
		for(int i = 0; i < switches.Length; i++) {
			var there = switches[i].pos;
			//print(there + " compare " + here);
			if(there == here) {
				tryTriggerSwitch(i);
				break;
			} else {
				//print("No trigger");
			}
		}
	}

	void tryTriggerSwitch(int index) {
		if(switches[index].triggered) {
			return;
		}

		if(switches[index].key != dice.bot()) {
			keyUnlockFailSfx.Play();
			//print("Unlock failed!");
			return;
		}

		switches[index].triggered = true;
		keyUnlockSfx.Play();
		StartCoroutine(fadeOutSwitch(index));


		//print("Unlocked! top was " + dice.top() + " bot was " + dice.bot());
		
		for(int i = 0; i < switches.Length; i++) {
			if(!switches[i].triggered) {
				return;
			}
		}

		exit.SetActive(true);
	}

	IEnumerator fadeOutSwitch(int index) {
		var obj = switches[index].obj;
		var sprite = obj.GetComponent<SpriteRenderer>();

		var t = switchFadeOutTime;
		while(t > 0) {
			var delta = Time.deltaTime;
			t -= delta;
			var s = t / switchFadeOutTime;
			var color = sprite.color;
			color.a = s;
			sprite.color = color;
			yield return null;
		}
		Destroy(obj);
	}

    // Start is called before the first frame update
    void Start()
    {
		var gridWidth = grid.GetLength(1);
		var gridHeight = grid.GetLength(0);
        tex = new Texture2D(gridWidth, gridHeight, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Point; //makes texture crisp, not blurry
        
        this.transform.position = new Vector3(gridWidth / 2, 0, gridHeight / 2); //shift by half
        this.transform.localScale = new Vector3(-gridWidth / 10f, 1f, -gridHeight / 10f); //Unity Planes are 10 units wide.
        
        MeshRenderer renderer = this.GetComponent<MeshRenderer>(); 
        renderer.material.mainTexture = tex;
        
        player.transform.position = playerPos + playerOffset;
		dice.transform.position = dicePos + diceOffset;
        
        //Only needs to be called once in Start() or Awake(), if grid doesn't change.
        RenderGridToTexture(); 
		initSwitches();
		exit.transform.position = new Vector3((float)exitPos.x, 0f, (float)exitPos.y)
			+ new Vector3(0.5f, 0.5f, 0.1f);
		exit.SetActive(false);
    }

    void RenderGridToTexture()
    {
		var gridWidth = grid.GetLength(1);
		var gridHeight = grid.GetLength(0);
        for (int z = 0; z < gridHeight; z++) {
            for (int x = 0; x < gridWidth; x++) {
                tex.SetPixel(x, z, 
                    grid[z, x] == 1 ? Color.red : Color.black );
			}
		}
        tex.Apply();    
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

		var gridWidth = grid.GetLength(1);
		var gridHeight = grid.GetLength(0);
		Vector3 proposedPos = player.transform.position + motion;
		//prevent player leaving the grid.
		proposedPos.x = proposedPos.x < 0 ? 0 : proposedPos.x;
		proposedPos.x = proposedPos.x > gridWidth - 1 ? gridWidth - 1 : proposedPos.x;
		proposedPos.z = proposedPos.z < 0 ? 0 : proposedPos.z;
		proposedPos.z = proposedPos.z > gridHeight - 1 ? gridHeight - 1 : proposedPos.z;

		var rpp = proposedPos - playerOffset;
		var rdp = dice.transform.position - diceOffset;

		if(Mathf.Round(rpp.x) == Mathf.Round(rdp.x) && Mathf.Round(rpp.z) == Mathf.Round(rdp.z)) {
			var proposedDicePos = dice.transform.position + motion;
			var proposedDiceCoords = new Vector2Int((int)(proposedDicePos.x), (int)proposedDicePos.z);
			if(grid[proposedDiceCoords.y, proposedDiceCoords.x] == 1) {
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

		if (grid[(int)proposedPos.z, (int)proposedPos.x] == 1)
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

	void checkIfExitIsReached(Vector2Int coords) {
		if(coords != exitPos) {
			return;
		}

		for(int i = 0; i < switches.Length; i++) {
			if(!switches[i].triggered) {
				return;
			}
		}

		SceneManager.LoadScene(nextSceneIfCompleted);
	}
}
