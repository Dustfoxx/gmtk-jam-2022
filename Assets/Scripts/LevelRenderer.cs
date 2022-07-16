using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelRenderer : MonoBehaviour
{

	/*
	public class Cell {
		public bool collides;
	};
	*/

	int[,] grid = new int[,]{
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 1,},
		{1, 0, 0, 0, 0, 0, 0, 0, 0, 1,},
		{1, 0, 0, 0, 0, 0, 0, 0, 0, 1,},
		{1, 0, 0, 0, 0, 1, 1, 0, 0, 1,},
		{1, 0, 0, 0, 0, 1, 1, 0, 0, 1,},
		{1, 0, 0, 0, 0, 0, 0, 0, 0, 1,},
		{1, 0, 0, 0, 0, 0, 0, 0, 0, 1,},
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 1,},
	};

	enum Direction {
		None,
		Up,
		Down,
		Left,
		Right,
	};

    public Vector3 playerPos = Vector3.zero;

    public Dice player; 
    Vector3 playerOffset = new Vector3(0.5f, 0.5f, 0.5f);
    Texture2D tex;

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
        
        //Only needs to be called once in Start() or Awake(), if grid doesn't change.
        RenderGridToTexture(); 
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
		if(player.isMoving()) {
			return;
		}

		Vector3 motion = Vector3.zero;
		Direction dir = Direction.None;

		if (Input.GetKeyDown(KeyCode.W)) {
			motion += Vector3.forward;
			dir = Direction.Up;
		} else if (Input.GetKeyDown(KeyCode.S)) {
			motion += Vector3.back;
			dir = Direction.Down;
		} else if (Input.GetKeyDown(KeyCode.A)) {
			motion += Vector3.left;
			dir = Direction.Left;
		} else if (Input.GetKeyDown(KeyCode.D)) {
			motion += Vector3.right;
			dir = Direction.Right;
		}

		var gridWidth = grid.GetLength(1);
		var gridHeight = grid.GetLength(0);
		Vector3 proposedPos = player.transform.position + motion;
		//prevent player leaving the grid.
		proposedPos.x = proposedPos.x < 0 ? 0 : proposedPos.x;
		proposedPos.x = proposedPos.x > gridWidth - 1 ? gridWidth - 1 : proposedPos.x;
		proposedPos.z = proposedPos.z < 0 ? 0 : proposedPos.z;
		proposedPos.z = proposedPos.z > gridHeight - 1 ? gridHeight - 1 : proposedPos.z;

		if (grid[(int)proposedPos.z, (int)proposedPos.x] == 1)
		{
			proposedPos = playerPos; //reset the proposed to the current position (stay there).
			
			Debug.Log("Invalid attempted move from " + playerPos +
											  " to " + proposedPos);
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
			}
		}
    }
}
