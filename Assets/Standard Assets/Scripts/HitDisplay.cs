
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HitDisplay  : MonoBehaviour  {
	SPoint[] lastHit;
	GameObject hitB;
	GameObject attB;
	SPoint[] attHit;
	SPoint[] pBounds;
	int attLen;
	int aBoxLen;
	SPoint hDim;
	SPoint hLoc;
	SPoint gOri;
	float gIntr;
	public Material attMat;
	public Material boxMat;
	public Mesh hitMesh;
	public Mesh boxMesh;
	public float zDepth;

	// Use this for initialization
	public void Start () {
		hDim = new SPoint(20.0f, 20.0f);
		hLoc = new SPoint(-10.0f, -10.0f);
		zDepth = 200.0f;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void PostHit(SPoint[] a, int numArr, SPoint[] b, int numBArr, SPoint[] p, SPoint gO, float gI){
		return;
		if(hitB==null){
			hitB = new GameObject ();
			hitB.name = "debugHit";
			hitB.AddComponent<MeshFilter>();
			hitB.AddComponent<MeshRenderer>();
			hitB.GetComponent<MeshRenderer>().material = boxMat;

		}
		if(attB==null){
			attB = new GameObject ();
			attB.name = "attHit";
			attB.AddComponent<MeshFilter>();
			attB.AddComponent<MeshRenderer>();
			attB.GetComponent<MeshRenderer>().material = attMat;
            
        }

		lastHit = new SPoint[numArr];
		pBounds = new SPoint[4];
		attLen = numArr;
		if (numBArr != 0) {
			attHit = new SPoint[numBArr];

		}else
			attHit = null;
		aBoxLen = numBArr;
		for(int i=0;i<numArr;i++){
			lastHit[i]=new SPoint();
			lastHit[i].x = a[i].x*hDim.x;
			lastHit[i].y = a[i].y*hDim.y;
		}
		for(int i=0;i<numBArr;i++){
			attHit[i]=new SPoint();
			attHit[i].x = b[i].x*hDim.x;
			attHit[i].y = b[i].y*hDim.y;
		}
		for(int i=0;i<4;i++)
			pBounds[i] = new  SPoint(p[i].x*hDim.x, p[i].y*hDim.y);
		gOri = new SPoint(gO.x*hDim.x, gO.y*hDim.y);
		gIntr =  gI*5*hDim.x;

		RenderHit ();
	}
	
	public bool RenderHit(){
		SPoint tLoc = new SPoint(hLoc.x,hLoc.y); 
		if (hitMesh == null)
			hitMesh = new Mesh ();
		if (boxMesh == null)
			boxMesh = new Mesh ();
        DrawConsoleHit(pBounds, hDim, "HitBox", "PlrHitBox", 4, tLoc, boxMesh);
		DrawConsoleHit(lastHit, hDim, "AttkBox", "PlrAttBox", attLen, tLoc, hitMesh);
		DrawConsoleGrid(gOri, gIntr, hDim, tLoc);
		MeshFilter mf = hitB.GetComponent<MeshFilter>();
		mf.mesh = hitMesh;
		MeshFilter mfb = attB.GetComponent<MeshFilter>();
		mfb.mesh = boxMesh;
        return true;
	}
	public void DrawConsoleHit(SPoint[] v, SPoint dim, string mat, string moName,  int endN, SPoint p, Mesh mesh){
		//Overloaded function; endInd is the index to stop at
		//v is the vertex list to draw at in local space
		//p is the world space offset
		//function to draw a hitbox, using the OGRE libraries
		float winHgt = 1;//vp->getActualHeight();
		float winWid = 1;//vp->getActualWidth();
		float vScl = 1;
		if(winWid>winHgt)
			vScl = winWid/winHgt;
		dim.y=dim.y*vScl;
		p.y = 1-p.y - dim.y;



		Vector3[] vertices = new Vector3[endN];
		Color32[] col = new Color32[endN];
		
		//std::stringstream hbName;
		 
		Material fMat;


		if(moName.CompareTo("PlrAttBox")==0)
			fMat=attMat;
		else
			fMat=boxMat;
		for (int i = 0; i < endN; i++) {
			vertices [i] = new Vector3 (-(v[i].x + p.x), v[i].y*vScl + p.y, zDepth);
			col [i] = new Color32 (0, 250, 20, 150);
		}
		// define start and end SPoint	

		int tTot = (endN - 2) * 3;
		int[] tri = new int[tTot];
		for (int i = 0; i<(tTot/3);i++) {//triangle fan
			tri[i*3]=0;
			tri[(i*3)+1]=i+1;
			tri[(i*3)+2]=i+2;
		}

		mesh.vertices=vertices;
		mesh.triangles = tri;
		//mesh.colors32 = col;

		// add ManualObject to the RootScoaeneNode (so it will be visible)

	}

	public void DrawConsoleGrid(SPoint gOr, float gIn, SPoint dim, SPoint p){
		/*double winHgt = vp->getActualHeight();
		double winWid = vp->getActualWidth();
		double vScl = 1;
		if(winWid>winHgt)
			vScl = winWid/winHgt;
		dim.y=dim.y*vScl;
		p.y = 1-p.y - dim.y;
		if(mSceneMgr->hasManualObject("GridOr"))
			mSceneMgr->destroyManualObject("GridOr");
		Ogre::ManualObject* tri = mSceneMgr->createManualObject("GridOr");	
		tri->setRenderQueueGroup(Ogre::RENDER_QUEUE_OVERLAY);
		tri->setUseIdentityProjection(true);
		tri->setUseIdentityView(true);
		tri->setQueryFlags(0);
		if(abs(gOr.x)<1){
			tri->estimateVertexCount(4);
			tri->begin("GridOrigin", Ogre::RenderOperation::OT_LINE_LIST);
			tri->position(gOr.x + p.x, 0*vScl + p.y, 0); 
			tri->position(gOr.x + p.x, 1*vScl + p.y, 0); 
			tri->end();
		}
		if(abs(gOr.y)<1){
			tri->begin("GridOrigin", Ogre::RenderOperation::OT_LINE_LIST);
			tri->position(0 + p.x, gOr.y*vScl + p.y, 0); 
			tri->position(1 + p.x, gOr.y*vScl + p.y, 0); 
			tri->end();
		}
		if((abs(gOr.y)<1)||(abs(gOr.y)<1))
			hitBounds->attachObject(tri);
		if(mSceneMgr->hasManualObject("GridLi"))
			mSceneMgr->destroyManualObject("GridLi");
		Ogre::ManualObject* line = mSceneMgr->createManualObject("GridLi");
		line->setUseIdentityProjection(true);
		line->setUseIdentityView(true);
		line->setQueryFlags(0);
		line->begin("GridLines", Ogre::RenderOperation::OT_LINE_LIST);
		double xPos = gOr.x;
		int num = 1;
		
		while(gOr.x+num*gIn<1){
			line->position(gOr.x+num*gIn + p.x, 0*vScl + p.y, 0); 
			line->position(gOr.x+num*gIn + p.x, 1*vScl + p.y, 0); 
			num++;
		}
		num=1;
		while(gOr.y+num*gIn<0)
			num++;//set to proper value
		while(gOr.y+num*gIn<1){
			line->position(0 + p.x, (gOr.y+num*gIn)*vScl + p.y, 0); 
			line->position(1 + p.x, (gOr.y+num*gIn)*vScl + p.y, 0); 
			num++;
		}
		num = -1;
		while(gOr.x+num*gIn>0){
			line->position(gOr.x+num*gIn + p.x, 0*vScl + p.y, 0); 
			line->position(gOr.x+num*gIn + p.x, 1*vScl + p.y, 0); 
			num--;
		}
		num = -1;
		while(gOr.y+num*gIn>1)
			num--;//set to proper value
		while(gOr.y+num*gIn>0){
			line->position(0 + p.x, (gOr.y+num*gIn)*vScl + p.y, 0); 
			line->position(1 + p.x, (gOr.y+num*gIn)*vScl + p.y, 0); 
			num--;	
		}
		line->end();
		// add ManualObject to the RootSceneNode (so it will be visible)
		hitBounds->attachObject(line);
		// add ManualObject to the RootScoaeneNode (so it will be visible)
		return S_OK;*/
	}

}
