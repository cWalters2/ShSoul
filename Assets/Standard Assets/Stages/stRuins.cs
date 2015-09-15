using UnityEngine;
using System.Collections;

public class stRuins : Stage {

	public TextAsset Terrain;
	protected void Start(){
		startPos[0] = new SPoint(-24*SCALE_FAC, 20*SCALE_FAC);
		startPos[1] = new SPoint(-20*SCALE_FAC, 20*SCALE_FAC);
		startPos[2] = new  SPoint(20, 0);
		startPos[3] = new SPoint(20, 0);

		LoadStage ();
		numPlr = 2;
		for (int i=0; i<numPlr; i++) {
		//	player[i].fHelper.stats.id.num = i + 1;
		//	player[i].SetPos(startPos[i]);
		}
		matchModeOn=GameObject.FindGameObjectWithTag("MatchHelper").GetComponent<MatchHelper>().isMatch;
	}
	// Update is called once per frame

	public bool LoadStage(){

		tBox = new TerrainBox ();
		tBox.LoadCollisionBoxes(Terrain);
		uBound.x = 250;
		uBound.y = 300;
		lBound.x = -300;
		lBound.y = -75;
		return true;
	}
}
