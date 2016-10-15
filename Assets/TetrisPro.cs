using UnityEngine;
using System;
using System.Collections;

public class TetrisPro : MonoBehaviour
{
	private const int MAX_Y = 20;
	private const int MAX_X = 10;
	private const int INIT_Y = 0;
	private const int INIT_X = 3;

	private int SCORE_STAGE = 1000;
	private float REACT_TIME = 0.5f;
	private float DAS_TIME = 0.15f;
	private float DAS_REPE = 0.016f;
	private float LINE_TIME = 0.3f;
	private float FALL_TIME = 1.0f;
	
	public GUISkin myskin = null;
	public GUISkin[] blockskin = null;
	public GUISkin backgroundskin = null;
	public GUISkin ghostskin = null;
	public GUISkin menuskin = null;


	private int width=0, height=0;

	private int[,] stateArray = new int[MAX_Y, MAX_X];
	private int[,] gfxArray = new int[MAX_Y, MAX_X];

	float lastFallTime = 0.0f;

	float startAnimationTime = 0.0f;
	float startGroundTime = 0.0f;

	float startDasTime = 0.0f;
	int dasDirection = 0;
	int dasStatus = 0;

	int irsDirection = 0;

	bool hideCurBlock = false;

	int stage = 0;
	int[] score = {0,0};
	bool noCurClock = false;
	bool isFirstGrounded = false;
	int countedFill = 0;
	private int[] filledYs = new int[MAX_Y];
	TetrisBlock curBlock;
	TetrisBlock nextBlock;
	TetrisBlock ghostBlock;

	const bool USE_FIXED = true;
	const int LAST_TIME_COUNT = 11;
	const int EXP_FPS = 120;
	float[] lastTime = new float[LAST_TIME_COUNT];
	int lastTimePtr = 0;
	float lastFpsTime = 0.0f;
	int countFps = 0;
	int curFps = 0;
	float firstTime = 0.0f;
	int firstFrame = 0;
	int frameCount = 0;

	enum Buttons{
		LEFT, RIGHT, DOWN, UP, ROTL, ROTR, ROT2,
		size
	};

	static string[] BUTTON_NAMES=new string[(int)Buttons.size]{
		"Left","Right","Down","Up","RotL","RotR","Rot180"
	};

	bool[] myInput=new bool[(int)Buttons.size];
	bool[] myLastInput=new bool[(int)Buttons.size];

	float myTime()
	{
		if(USE_FIXED){
			return Time.fixedTime;
		}else{
			return Time.time;
		}
	}

	void myButtonUpate()
	{
		for(int i=0;i<(int)Buttons.size;i++){
			myLastInput[i] = myInput[i];
			myInput[i]=Input.GetButton(BUTTON_NAMES[(int)i]);
		}
	}

	bool myTrigger(Buttons button)
	{
		return myLastInput[(int)button]==false && myInput[(int)button]==true;
	}

	bool myButton(Buttons button)
	{
		return myInput[(int)button];
	}

	void OnGUI ()
	{
		width = Screen.width;
		height = Screen.height;

		int cs = height/MAX_Y;
		int offsetY= (height-cs*MAX_Y)/2;
		int offsetX= (width-cs*MAX_X)/2;
		int tmpY;
		int tmpX;

		GUI.skin=menuskin;
		GUI.Label (new Rect (offsetX - 100, offsetY + 30, 100, 30), "SCORE");
		GUI.Label (new Rect (offsetX - 100, offsetY + 50, 100, 30), this.getScore().ToString ());
		GUI.Label (new Rect (offsetX + MAX_X * cs + 32, offsetY + 30, 100, 30), "NEXT");

		GUI.Label (new Rect (offsetX - 100, offsetY + 100, 100, 30), "iFPS: " + this.curFps.ToString ());
		/*
		GUI.Label (new Rect (offsetX - 100, offsetY + 120, 100, 30), "" + (frameCount - firstFrame) / (myTime() - firstTime));
		GUI.Label (new Rect (offsetX - 100, offsetY + 140, 100, 30), "" + 
			1.0 / (
			(lastTime [(lastTimePtr + LAST_TIME_COUNT - 1) % LAST_TIME_COUNT] - lastTime [lastTimePtr]) / (LAST_TIME_COUNT - 1.0) +
			(1.0 / EXP_FPS)
		    )
		);

		for(int i=0;i<lastTime.GetLength (0);i++)
			GUI.Label (new Rect (offsetX - 100, offsetY + 180+20*i, 100, 30), this.lastTime[i].ToString ());

		*/
		GUI.skin=backgroundskin;
		GUI.Box(new Rect (offsetX, offsetY, MAX_X * cs, MAX_Y * cs), "");
		for (int Y = 0; Y<MAX_Y; Y++) {
			for (int X=0; X<MAX_X; X++) {
				if (gfxArray [Y, X] >= 0) {
					GUI.skin = blockskin[gfxArray [Y, X]];
					GUI.Button (new Rect (offsetX + X * cs, offsetY + Y * cs, cs, cs), "");
				}
			}
		}
		GUI.skin=backgroundskin;
		GUI.Box(new Rect (offsetX + (MAX_X + 1)*cs, offsetY + 2 * cs, 4 * cs, 4 * cs), "");
		GUI.skin = blockskin[this.nextBlock.T];
		for (int i = 0; i < 4; i++) {
			tmpY = 3 + this.nextBlock.listY [i];
			tmpX = MAX_X + 1 + this.nextBlock.listX [i];
			GUI.Button (new Rect (offsetX + tmpX * cs, offsetY + tmpY * cs, cs, cs), "");
		}

		if(noCurClock){
			
		} else {
			this.ghostBlock.Y = this.GetGroundedY (this.curBlock.Y, this.curBlock.X);
			this.ghostBlock.X = this.curBlock.X;
			this.ghostBlock.listY = this.curBlock.listY;
			this.ghostBlock.listX = this.curBlock.listX;

			if(!this.hideCurBlock)
			{
				GUI.skin = ghostskin;
				for (int i=0; i < this.ghostBlock.listY.GetLength(0); i++) {
					tmpY = this.ghostBlock.Y + this.ghostBlock.listY [i];
					tmpX = this.ghostBlock.X + this.ghostBlock.listX [i];
					if (this.InField (tmpY, tmpX)) {
						GUI.Button (new Rect (offsetX + tmpX * cs, offsetY + tmpY * cs, cs, cs), "");
					}
				}

				GUI.skin = blockskin[this.curBlock.T ];
				for (int i=0; i < this.curBlock.listY.GetLength(0); i++) {
					tmpY = this.curBlock.Y + this.curBlock.listY [i];
					tmpX = this.curBlock.X + this.curBlock.listX [i];
					if (this.InField (tmpY, tmpX)) {
						GUI.Button (new Rect (offsetX + tmpX * cs, offsetY + tmpY * cs, cs, cs), "");
					}
				}
			}
		}
	}

	void genGfx()
	{
		for (int Y = 0; Y<MAX_Y; Y++) {
			for (int X=0; X<MAX_X; X++) {
				gfxArray[Y,X]=-1;
			}
		}
		for (int Y = 0; Y<MAX_Y; Y++) {
			for (int X=0; X<MAX_X; X++) {
				if (stateArray [Y, X] >=0) {
					gfxArray[Y,X] = stateArray [Y, X];
				}
			}
		}
	}
	// Use this for initialization
	void Start ()
	{
		//Time.fixedDeltaTime=0.005f; //will implement later
		if(USE_FIXED){
			frameCount =0;
			Time.fixedDeltaTime=1.0f/(float)EXP_FPS;
		}
		resetStateArray ();
		
		this.ghostBlock = new TetrisBlock (INIT_Y, INIT_X);
		
		this.nextBlock = new TetrisBlock (INIT_Y, INIT_X);
		GenNewBlock ();
		lastFallTime = myTime();
	}
	
	void updateStateByBlock (TetrisBlock tmpBlock, int stateVal)
	{
		int tmpY;
		int tmpX;
		// Debug.Log ("center Y:" + tmpBlock.Y + " center X:" + tmpBlock.X);
		for (int i=0; i < tmpBlock.listY.GetLength(0); i++) {
			tmpY = tmpBlock.Y + tmpBlock.listY [i];
			tmpX = tmpBlock.X + tmpBlock.listX [i];
			if (this.InField (tmpY, tmpX)) {
				stateArray [tmpY, tmpX] = stateVal;
			}
		}
	}

	void drawByBlock (TetrisBlock tmpBlock, int stateVal)
	{

	}
	// Update is called once per frame
	void FixedUpdate ()
	{
		if(USE_FIXED)
			realUpdate();
	}

	void Update()
	{
		if(!USE_FIXED)
			realUpdate();
	}

	void realUpdate()
	{
		FpsManager();
		myButtonUpate();

		if (noCurClock == false) {
			TetrisMove ();
		} else {
			WaitingMove ();
		}

		StageManager ();

		genGfx();

		frameCount++;
	}

	void FpsManager()
	{
		if(frameCount ==0){
			lastFpsTime = myTime();
			countFps = 0;
			firstFrame = frameCount;
			firstTime=myTime();
		}else{
			lastTime[lastTimePtr++] = (myTime()-firstTime)-(float)((frameCount-firstFrame)/(double)EXP_FPS);
			lastTimePtr%=LAST_TIME_COUNT;
		}

		countFps++;
		if(myTime()-lastFpsTime>=1.0f)
		{
			lastFpsTime+=1.0f;
			curFps=countFps;
			countFps=0;
		}
	}

	void TetrisMove ()
	{
		this.resetFilledYs ();
		bool isHarddrop = false;
		bool reactTimeReset = false;

		if (myTrigger (Buttons.ROTR)) {
			reactTimeReset |= this.Rotate (this.curBlock.Y, this.curBlock.X, 1);
		} 
		if (myTrigger (Buttons.ROTL)) {
			reactTimeReset |= this.Rotate (this.curBlock.Y, this.curBlock.X, -1);
		}
		if (myTrigger (Buttons.ROT2)) {
			reactTimeReset |= this.Rotate (this.curBlock.Y, this.curBlock.X, 2);
		}
		if (myTrigger (Buttons.LEFT)) {
			startDasTime=myTime();
			dasDirection=-1;
			dasStatus=0;
			if(!IsCollision (curBlock.Y,curBlock.X-1,curBlock)) {
				curBlock.X-=1;
				reactTimeReset = true;
			}
		}
		if (myTrigger (Buttons.RIGHT)) {
			startDasTime=myTime();
			dasDirection=1;
			dasStatus=0;
			if(!IsCollision (curBlock.Y,curBlock.X+1,curBlock)) {
				curBlock.X+=1;
				reactTimeReset = true;
			}		
		}

		if (!myButton (Buttons.LEFT) && dasDirection == -1)
			dasDirection = 0;
		if (!myButton (Buttons.RIGHT) && dasDirection == 1)
			dasDirection = 0;
		if (dasDirection != 0) {
			if (dasStatus == 0) {
				if (myTime() - startDasTime - DAS_TIME >= 0) {
					if (!IsCollision (curBlock.Y, curBlock.X + dasDirection, curBlock)) {
						curBlock.X += dasDirection;
						reactTimeReset = true;
						startDasTime = myTime();
						dasStatus = 1;
					}
				}
			} else {
				if (myTime() - startDasTime - DAS_REPE >= 0) {
					if (!IsCollision (curBlock.Y, curBlock.X + dasDirection, curBlock)) {
						curBlock.X += dasDirection;
						reactTimeReset = true;
						startDasTime = myTime();
					}
				}
			}
		}

		if (myTrigger (Buttons.DOWN)) {
			//toY = curBlock.Y + 1;
			this.curBlock.Y = this.GetGroundedY (this.curBlock.Y, this.curBlock.X);
			// Debug.Log ("down:" + isCollision);
		}
		if (myTrigger (Buttons.UP)) {
			this.addScore(this.GetGroundedY (this.curBlock.Y, this.curBlock.X) - this.curBlock.Y);
			this.curBlock.Y = this.GetGroundedY (this.curBlock.Y, this.curBlock.X);
			isHarddrop = true;
		}

		if (IsGrounded ()) {
			//add immediate react response
			if (this.isFirstGrounded == false || (this.isFirstGrounded && reactTimeReset) ) {
				this.isFirstGrounded = true;
				this.startGroundTime = myTime();
			}
			if (isHarddrop || myTime() - this.startGroundTime - REACT_TIME >= 0) {
				updateStateByBlock(this.curBlock, this.curBlock.T);
				this.countedFill = this.checkFilledLines ();
				if (this.countedFill > 0) {
					for (int Y = 0; Y<MAX_Y; Y++) {
						if(filledYs[Y]==1){
							for (int X=0; X<MAX_X; X++) {
								stateArray[Y,X]=-1;
							}
						}
					}
					this.addScore(this.countedFill * this.countedFill * 100);
					this.noCurClock = true;
					startAnimationTime = myTime();
				} else {
					this.countedFill = 0;

					GenNewBlock ();
				}
			}
			
		} else {
			if (this.FALL_TIME!=0.0f){
				this.isFirstGrounded = false;
				//auto down set download speed

				if ((myTime() - lastFallTime - FALL_TIME) >= 0) {
					this.curBlock.Y += 1;
					lastFallTime = myTime();
				}
			}else{
				this.curBlock.Y = this.GetGroundedY (this.curBlock.Y, this.curBlock.X);
				if(reactTimeReset)
					startGroundTime = myTime();
			}
		}
	}

	void GenNewBlock ()
	{
		this.curBlock = this.nextBlock;
		this.nextBlock = new TetrisBlock (INIT_Y, INIT_X);
		this.isFirstGrounded = false;
		checkGameOver ();
		if (this.FALL_TIME==0.0f) {
			this.curBlock.Y = this.GetGroundedY (this.curBlock.Y, this.curBlock.X);
			this.isFirstGrounded = true;
			this.startGroundTime = myTime();
		}
	}
	
	void checkGameOver ()
	{
		if (IsGameOver ()) {
			Application.LoadLevel ("StartMenu");
		}
	}

	bool IsGameOver ()
	{
		// when is grounded, check is it over game.
		bool gameOver = false;
		int tmpY = -1;
		int tmpX = -1;
		for (int i = 0; i < 4; i++) {
			tmpY = this.curBlock.Y + this.curBlock.listY [i];
			tmpX = this.curBlock.X + this.curBlock.listX [i];
			if (stateArray [tmpY, tmpX] >=0) {
				gameOver = true;
			}
		}
		return gameOver;
	}

	void WaitingMove ()
	{
		if (myTrigger (Buttons.ROTR)) {
			irsDirection=1;
		} 
		if (myTrigger (Buttons.ROTL)) {
			irsDirection=-1;
		}
		if (myTrigger (Buttons.ROT2)) {
			irsDirection=2;
		}

		if (myTrigger (Buttons.LEFT)) {
			startDasTime=myTime();
			dasDirection=-1;
			dasStatus=0;
		}
		if (myTrigger (Buttons.RIGHT)) {
			startDasTime=myTime();
			dasDirection=1;
			dasStatus=0;
		}
		
		if (!myButton (Buttons.LEFT) && dasDirection == -1)
			dasDirection = 0;
		if (!myButton (Buttons.RIGHT) && dasDirection == 1)
			dasDirection = 0;
		if (dasDirection != 0) {
			if (dasStatus == 0) {
				if (myTime() - startDasTime - DAS_TIME >= 0) {
					startDasTime = myTime();
					dasStatus = 1;
				}
			} else {
				if (myTime() - startDasTime - DAS_REPE >= 0) {
					startDasTime = myTime();
				}
			}
		}

		float animationTime = myTime() - this.startAnimationTime;
		//stop blink
		if (animationTime >= LINE_TIME) {
			Cascade ();
			if (checkFilledLines () == 0) {
				GenNewBlock ();
				if(irsDirection!=0){
					Rotate(this.curBlock.Y,this.curBlock.X,irsDirection);
					irsDirection=0;
				}
				this.noCurClock = false;
				this.countedFill = 0;
			} else {
				this.countedFill += this.checkFilledLines ();
				this.addScore(this.countedFill * this.countedFill * 100);
				this.noCurClock = true;
				startAnimationTime = myTime();
			}
		}
		
	}

	void Cascade ()
	{
		int offset = 0;
		for (int Y = MAX_Y - 1; Y >= 0; Y --) {
			if (filledYs [Y] == 1) {
				CascadeMoveDown (Y, offset);
				offset ++;
			}
		}
	}

	void CascadeMoveDown (int delY, int offset)
	{
		for (int Y = delY + offset; Y >= 1; Y --) {
			for (int X = 0; X < MAX_X; X ++) {
				stateArray [Y, X] = stateArray [Y - 1, X];
			}
		}
		for (int X = 0; X < MAX_X; X ++) {
			stateArray [0, X] = -1;
		}
	}

	void StageManager ()
	{
		this.stage = (int)(this.getScore() / SCORE_STAGE);
		if(stage<10){
			this.FALL_TIME = 1.0f / ((float)Math.Pow(this.stage + 1, 2));
		}else if(stage<25){
			this.FALL_TIME=0.0f;
			this.REACT_TIME=0.5f-0.016666666666666f*(stage-10);
		}else{
			this.FALL_TIME=0.0f;
			this.REACT_TIME=0.5f;
			this.hideCurBlock=true;
		}
	}

	bool IsFillingLine (int Y)
	{
		bool flag = true;
		for (int X = 0; X < MAX_X; X++) {
			if (stateArray [Y, X] <0) {
				flag = false;
			}
		}
		return flag;
	}
	
	bool Rotate (int Y, int X, int direction)
	{
		int [] originalListY = new int[4];
		int [] originalListX = new int[4];
		int originalR=this.curBlock.R;
		//backup original position
		for (int i = 0; i < 4; i++) {
			originalListY [i] = this.curBlock.listY [i];
			originalListX [i] = this.curBlock.listX [i];
		}
		this.curBlock.R=(this.curBlock.R + direction +4)%4;
		this.curBlock.LoadList(this.curBlock.T,this.curBlock.R );

		int [,,] wktable = null;
		if(this.curBlock.T==0){ //I
			switch(direction){
			case 1:wktable=Rotator.WKTableSRSI_R;break;
			case -1:wktable=Rotator.WKTableSRSI_L;break;
			case 2:wktable=Rotator.WKTableSRSI_2;break;
			default:wktable=Rotator.WKTableSRSI_R;break;
			}
		}else{
			switch(direction){
			case 1:wktable=Rotator.WKTableSRSX_R;break;
			case -1:wktable=Rotator.WKTableSRSX_L;break;
			case 2:wktable=Rotator.WKTableSRSX_2;break;
			default:wktable=Rotator.WKTableSRSX_R;break;
			}
		}
		for(int i=0;i<wktable.GetLength(1);i++){
			if(!IsCollision (
				this.curBlock.Y+wktable[originalR,i,1],
				this.curBlock.X+wktable[originalR,i,0],
				this.curBlock
				)){
				this.curBlock.Y+=wktable[originalR,i,1];
				this.curBlock.X+=wktable[originalR,i,0];
				return true;
			}
		}

		this.curBlock.R=originalR;
		for (int i = 0; i < 4; i++) {
			this.curBlock.listY [i] = originalListY [i];
			this.curBlock.listX [i] = originalListX [i];
		}
		return false;
	}

	bool InField (int Y, int X)
	{
		return (Y < MAX_Y && Y >= 0 && X >= 0 && X < MAX_X);
	}

	bool IsCollision (int Y, int X, TetrisBlock tmpBlock)
	{
		int tmpY;
		int tmpX;
		bool result = false;
		for (int i = 0; i < 4; i++) {
			tmpY = Y + tmpBlock.listY [i];
			tmpX = X + tmpBlock.listX [i];
			if (!this.InField (tmpY, tmpX) || (stateArray [tmpY, tmpX] >=0)) {
				result = true;
			}
		}
		return result;
	}

	bool IsGrounded ()
	{
		return this.IsCollision (this.curBlock.Y + 1, this.curBlock.X, this.curBlock);
	}

	int GetGroundedY (int curY, int curX)
	{
		for (int Y = curY; Y < MAX_Y; Y ++) {
			if (this.IsCollision (Y + 1, curX, this.curBlock))
				return Y;
		}
		return MAX_Y - 1;
	}

	void resetFilledYs ()
	{
		for (int i = 0; i < filledYs.GetLength(0); i++) {
			filledYs [i] = -1;
		}
	}

	void resetStateArray ()
	{
		for (int Y = 0; Y < MAX_Y; Y ++) {
			for (int X = 0; X <MAX_X; X ++) {	
				stateArray [Y, X] = -1;
			}
		}
	}

	int checkFilledLines ()
	{
		int countLines = 0;
		for (int Y = 0; Y < MAX_Y; Y ++) {
			if (IsFillingLine (Y)) {
				filledYs [Y] = 1;
				countLines ++;
			}
		}
		return countLines;
	}

	void addScore(int s)
	{
		score[0]+=s*461550377;
		score[1]+=s*2489371;
	}

	int getScore()
	{
		if ((((score[0]*104729)^(score[1]*1624997395))&0x7fffffff) != 0)
			return 233333333;
		else
			return (((score[0]*104729)&(score[1]*1624997395))&0x7fffffff);
	}
}

public class TetrisBlock
{
	//type rot cell
	static int[,,] blockListY = new int[,,]{
		{{1,1,1,1},{0,1,2,3},{2,2,2,2},{3,2,1,0}},	// I
		{{0,1,1,1},{2,2,1,0},{2,1,1,1},{0,0,1,2}},	// L
		{{0,0,1,1},{0,1,1,0},{1,1,0,0},{1,0,0,1}},	// O
		{{0,0,1,1},{0,1,1,2},{2,2,1,1},{2,1,1,0}},	// Z
		{{0,1,1,1},{1,0,1,2},{2,1,1,1},{1,2,1,0}},	// T
		{{0,1,1,1},{0,0,1,2},{2,1,1,1},{2,2,1,0}},	// J
		{{0,0,1,1},{2,1,1,0},{2,2,1,1},{0,1,1,2}},	// S
	};
	static int[,,] blockListX = new int[,,]{
		{{0,1,2,3},{2,2,2,2},{3,2,1,0},{1,1,1,1}},	// I
		{{2,2,1,0},{2,1,1,1},{0,0,1,2},{0,1,1,1}},	// L
		{{1,2,2,1},{2,2,1,1},{2,1,1,2},{1,1,2,2}},	// O
		{{0,1,1,2},{2,2,1,1},{2,1,1,0},{0,0,1,1}},	// Z
		{{1,0,1,2},{2,1,1,1},{1,2,1,0},{0,1,1,1}},	// T
		{{0,0,1,2},{2,1,1,1},{2,2,1,0},{0,1,1,1}},	// J
		{{2,1,1,0},{2,2,1,1},{0,1,1,2},{0,0,1,1}},	// S
	};
	public int[] listY = new int[4];
	public int[] listX = new int[4];
	public int Y;
	public int X;
	public int R;
	public int T;
	
	public TetrisBlock (int Y1, int X1)
	{
		this.Y = Y1;
		this.X = X1;
		this.R = 0;
		this.T = (int)(UnityEngine.Random.value * blockListY.GetLength (0));
		//type = 0;
		LoadList(T,R);
	}

	public void LoadList (int type, int rot)
	{
		for (int i = 0; i < blockListY.GetLength(2); i++) {
			this.listY [i] = blockListY [type, rot, i]; 
			this.listX [i] = blockListX [type, rot, i]; 
		}
	}
}

public class Rotator
{
	public static int[,,] WKTableSRSI_R = new int[,,]
	{
		{{ 0, 0},{-2, 0},{+1, 0},{-2,+1},{+1,-2}},
		{{ 0, 0},{-1, 0},{+2, 0},{-1,-2},{+2,+1}},
		{{ 0, 0},{+2, 0},{-1, 0},{+2,-1},{-1,+2}},
		{{ 0, 0},{+1, 0},{-2, 0},{+1,+2},{-2,-1}}
	};
	public static int[,,] WKTableSRSI_L = new int[,,]
	{
		{{ 0, 0},{-1, 0},{+2, 0},{-1,-2},{+2,+1}},
		{{ 0, 0},{+2, 0},{-1, 0},{+2,-1},{-1,+2}},
		{{ 0, 0},{+1, 0},{-2, 0},{+1,+2},{-2,-1}},
		{{ 0, 0},{-2, 0},{+1, 0},{-2,+1},{+1,-2}}
	};
	public static int[,,] WKTableSRSI_2 = new int[,,] 
	{
		{{ 0, 0},{-1, 0},{-2, 0},{+1, 0},{+2, 0},{ 0,+1}},
		{{ 0, 0},{ 0,+1},{ 0,+2},{ 0,-1},{ 0,-2},{-1, 0}},
		{{ 0, 0},{+1, 0},{+2, 0},{-1, 0},{-2, 0},{ 0,-1}},
		{{ 0, 0},{ 0,+1},{ 0,+2},{ 0,-1},{ 0,-2},{+1, 0}}
	};
	public static int[,,] WKTableSRSX_R = new int[,,]
	{
		{{ 0, 0},{-1, 0},{-1,-1},{ 0,+2},{-1,+2}},
		{{ 0, 0},{+1, 0},{+1,+1},{ 0,-2},{+1,-2}},
		{{ 0, 0},{+1, 0},{+1,-1},{ 0,+2},{+1,+2}},
		{{ 0, 0},{-1, 0},{-1,+1},{ 0,-2},{-1,-2}}
	};
	public static int[,,] WKTableSRSX_L = new int[,,]
	{
		{{ 0, 0},{+1, 0},{+1,-1},{ 0,+2},{+1,+2}},
		{{ 0, 0},{+1, 0},{+1,+1},{ 0,-2},{+1,-2}},
		{{ 0, 0},{-1, 0},{-1,-1},{ 0,+2},{-1,+2}},
		{{ 0, 0},{-1, 0},{-1,+1},{ 0,-2},{-1,-2}}
	};
	public static int[,,] WKTableSRSX_2 = new int[,,]
	{
		{{ 0, 0},{+1, 0},{+2, 0},{+1,+1},{+2,+1},{-1, 0},{-2, 0},{-1,+1},{-2,+1},{ 0,-1},{+3, 0},{-3, 0}},
		{{ 0, 0},{ 0,+1},{ 0,+2},{-1,+1},{-1,+2},{ 0,-1},{ 0,-2},{-1,-1},{-1,-2},{+1, 0},{ 0,+3},{ 0,-3}},
		{{ 0, 0},{-1, 0},{-2, 0},{-1,-1},{-2,-1},{+1, 0},{+2, 0},{+1,-1},{+2,-1},{ 0,+1},{-3, 0},{+3, 0}},
		{{ 0, 0},{ 0,+1},{ 0,+2},{+1,+1},{+1,+2},{ 0,-1},{ 0,-2},{+1,-1},{+1,-2},{-1, 0},{ 0,+3},{ 0,-3}}
	};

}