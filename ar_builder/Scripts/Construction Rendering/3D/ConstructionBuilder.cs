using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion;

public class ConstructionBuilder : MonoBehaviour
{

	public enum ConstructionPivot{
		LowerLeft,
		LowerRight,
		LowerCenter,
		UpperLeft,
		UpperRight,
		UpperCenter,
		MiddleLeft,
		MiddleRight,
		MiddleCenter
	}

	public enum BuilderType{
		Scan,
		Edit,
		UI,
		Turret,
		Building,
		Vehicle
	}

	public enum Alignment{
		Right,
		Left
	}

	List<Transform> _bricks;

	Dictionary<int, Transform> _bricksLookup;

	public int NumBricks{
		get{
			return _bricks.Count;
		}
	}

	public LXFMLConstruction construction;
	public Vector3 pivotOffset = Vector3.zero;

	private bool _isSpinning = false;
	private Vector3 _pivotReferencePosition;
	private float _rotationSpeed;

	private int _numBricksToAnimate;
	private int _numBricksAnimateComplete = 0;

	public void Init(BuilderType type){
		switch(type){
		case BuilderType.Scan:
			ScannerConstruction();
			break;
		case BuilderType.Edit:
			EditorConstruction();
			break;
		case BuilderType.Turret:
			TurretConstruction();
			break;
		case BuilderType.UI:
			UIConstruction();
			break;
		case BuilderType.Building:
			BuildingConstruction();
			break;
		case BuilderType.Vehicle:
			VehicleConstruction();
			break;
		}
	}

	public Vector3 GetLaserPosition(int index){
		var laser = construction.GetLaser(index);
		Transform brickTrans = _bricksLookup[laser.brickId];
		Vector3 forward;
		if(laser.direction < 0){
			forward = brickTrans.right * -1 * (LXFMLHelper.kBrickSize.x / 2f);
		}else{
			forward = brickTrans.right * (((construction.GetBrick(laser.brickId).design.width - 1) * LXFMLHelper.kBrickSize.x) + (LXFMLHelper.kBrickSize.x / 2f));
		}
		Vector3 up = brickTrans.up * (LXFMLHelper.kBrickSize.y / 2f);
		return brickTrans.position + forward + up;
	}

	public void Cleanup(){
		for(int i = 0 ; i < gameObject.transform.childCount ; i++){
			Destroy (gameObject.transform.GetChild(i).gameObject);
		}
         Destroy (this);
	}

	void ScannerConstruction(){
		Build();
//		SetConstructionPivot(ConstructionPivot.MiddleCenter);
//		AddTrailerRenderer();
	}

	void EditorConstruction(){
		Build();
	}

	void TurretConstruction(){
		Build();
		SetConstructionPivot(ConstructionPivot.LowerCenter);
		construction.SetPowerBricks();
	}

	void UIConstruction(){
		Build();
	}

	void BuildingConstruction(){
		Build();
	}

	void VehicleConstruction(){
		Build();
	}

	void Build(){
		IList<BrickData> bricksData = construction.GetAllBricks();
		
		_bricks = new List<Transform>(bricksData.Count);
		_bricksLookup = new Dictionary<int, Transform>();
		
		for (int i = 0; i < bricksData.Count; ++i)
		{
			Transform brick = CreateBrick(bricksData[i], bricksData[i].design.id, transform);
			_bricks.Add(brick);
			_bricksLookup.Add(bricksData[i].id, brick);
		}

		for (int i = 0; i < _bricks.Count; ++i)
		{
			_bricks[i].GetComponent<LegoBrickId>().coords = construction.GetBrickOrigin(_bricks[i].GetComponent<LegoBrickId>().id);
		}
		
		_bricks.Sort(SortUtils.SortByYPosition);
	}

	void AddTrailerRenderer(){
		for (int i = 0; i < _bricks.Count; ++i)
		{
			TrailRenderer trail = _bricks[i].gameObject.AddComponent<TrailRenderer>();
			trail.enabled = false;
			trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			trail.receiveShadows = false;
			trail.autodestruct = false;
			trail.time = 0f;
			trail.startWidth = 1f;
			trail.endWidth = 0f;
			trail.sharedMaterial = Resources.Load<Material>("Materials/BrickTrail");
		}
	}

	public void DestroyBrick(LegoBrickId brick){
		int brickId = brick.id;
		_bricks.Remove(brick.transform);
		_bricksLookup.Remove(brickId);
		construction.RemoveBrick(brickId);
		Destroy (brick.gameObject);
	}

	public void SetConstructionPivot(ConstructionPivot pivot){
		
		Rect bounds = construction.Grid.GetRect();
		
		pivotOffset.x = 0f;
		pivotOffset.y = 0f;
		pivotOffset.z = LXFMLHelper.kBrickSize.x / 2f;
		
		switch(pivot){
		case ConstructionPivot.LowerLeft:
			pivotOffset.x = -((bounds.x * LXFMLHelper.kBrickSize.x) - (LXFMLHelper.kBrickSize.x / 2f));
			pivotOffset.y = -(bounds.y * LXFMLHelper.kBrickSize.y);
			break;
		case ConstructionPivot.LowerRight:
			pivotOffset.x = -((bounds.x * LXFMLHelper.kBrickSize.x) + (bounds.width * LXFMLHelper.kBrickSize.x) - (LXFMLHelper.kBrickSize.x / 2f));
			pivotOffset.y = -(bounds.y * LXFMLHelper.kBrickSize.y);
			break;
		case ConstructionPivot.LowerCenter:
			pivotOffset.x = -((bounds.x * LXFMLHelper.kBrickSize.x) + (bounds.width * LXFMLHelper.kBrickSize.x / 2f) - (LXFMLHelper.kBrickSize.x / 2f));
			pivotOffset.y = -(bounds.y * LXFMLHelper.kBrickSize.y);
			break;
		case ConstructionPivot.UpperLeft:
			pivotOffset.x = -((bounds.x * LXFMLHelper.kBrickSize.x) - (LXFMLHelper.kBrickSize.x / 2f));
			pivotOffset.y = -((bounds.y * LXFMLHelper.kBrickSize.y) + (bounds.height * LXFMLHelper.kBrickSize.y));
			break;
		case ConstructionPivot.UpperRight:
			pivotOffset.x = -((bounds.x * LXFMLHelper.kBrickSize.x) + (bounds.width * LXFMLHelper.kBrickSize.x) - (LXFMLHelper.kBrickSize.x / 2f));
			pivotOffset.y = -((bounds.y * LXFMLHelper.kBrickSize.y) + (bounds.height * LXFMLHelper.kBrickSize.y));
			break;
		case ConstructionPivot.UpperCenter:
			pivotOffset.x = -((bounds.x * LXFMLHelper.kBrickSize.x) + (bounds.width * LXFMLHelper.kBrickSize.x / 2f) - (LXFMLHelper.kBrickSize.x / 2f));
			pivotOffset.y = -((bounds.y * LXFMLHelper.kBrickSize.y) + (bounds.height * LXFMLHelper.kBrickSize.y));
			break;
		case ConstructionPivot.MiddleLeft:
			pivotOffset.x = -((bounds.x * LXFMLHelper.kBrickSize.x) - (LXFMLHelper.kBrickSize.x / 2f));
			pivotOffset.y = -((bounds.y * LXFMLHelper.kBrickSize.y) + (bounds.height * LXFMLHelper.kBrickSize.y / 2f));
			break;
		case ConstructionPivot.MiddleRight:
			pivotOffset.x = -((bounds.x * LXFMLHelper.kBrickSize.x) + (bounds.width * LXFMLHelper.kBrickSize.x) - (LXFMLHelper.kBrickSize.x / 2f));
			pivotOffset.y = -((bounds.y * LXFMLHelper.kBrickSize.y) + (bounds.height * LXFMLHelper.kBrickSize.y / 2f));
			break;
		case ConstructionPivot.MiddleCenter:
			pivotOffset.x = -((bounds.x * LXFMLHelper.kBrickSize.x) + (bounds.width * LXFMLHelper.kBrickSize.x / 2f) - (LXFMLHelper.kBrickSize.x / 2f));
			pivotOffset.y = -((bounds.y * LXFMLHelper.kBrickSize.y) + (bounds.height * LXFMLHelper.kBrickSize.y / 2f));
			break;	
		}
		
		for (int i = 0; i < _bricks.Count; ++i) {
			LegoBrickId legoBrick = _bricks[i].GetComponent<LegoBrickId>();
			BrickData bd = construction.GetBrick(legoBrick.id);
			_bricks[i].localPosition = bd.position;
			_bricks[i].Translate(pivotOffset, Space.World);
		}
	}

	public void StopBricksAnimation(){
		StopAllCoroutines ();
		_numBricksAnimateComplete = _numBricksToAnimate;
		for (int i = 0; i < _bricks.Count; ++i) {
			var renderer = _bricks [i].GetComponentInChildren<MeshRenderer>();
			LegoBrickId legoBrick = _bricks [i].GetComponent<LegoBrickId> ();
			BrickData bd = construction.GetBrick (legoBrick.id);
			renderer.sharedMaterial = ConstructionController.Instance.resourcesProvider.GetMaterial(bd.materialId);
		}
	}

	public int SortByYAscending(Transform brick1, Transform brick2)
	{
		int returnVal =  brick1.position.y.CompareTo(brick2.position.y);
		if(returnVal == 0)
			return brick1.position.x.CompareTo(brick2.position.x);
		return returnVal;
	}

	public IEnumerator AnimateBrickScanRandom(){
		_numBricksToAnimate = _bricks.Count;
		_bricks.Sort (SortByYAscending);
		for (int i = 0; i < _bricks.Count; ++i) {
			_bricks[i].gameObject.SetLayerRecursively (8);
			_bricks[i].gameObject.SetActive (false);
		}

		for (int i = 0; i < _bricks.Count; ++i) {

			yield return new WaitForEndOfFrame ();
			yield return new WaitForEndOfFrame ();
			_bricks[i].gameObject.SetActive (true);
			LegoBrickId legoBrick = _bricks[i].GetComponent<LegoBrickId>();
			BrickData bd = construction.GetBrick(legoBrick.id);
			int[] availColors = LXFMLHelper.GetColors(legoBrick.designId);
			int numIterations = Random.Range(15,20);
			StartCoroutine(AnimateBrickColors(LXFMLHelper.GetColors(legoBrick.designId), bd.materialId, _bricks[i].gameObject, numIterations));

		}
		yield return StartCoroutine ("WaitForAnimComplete");
	}

	private IEnumerator AnimateBrickColors(int[] colors, int actualColor, GameObject brick, int numIterations){
		var renderer = brick.GetComponentInChildren<MeshRenderer>();
		int colorIndex = Random.Range(0,colors.Length);
		renderer.sharedMaterial = ConstructionController.Instance.resourcesProvider.GetMaterial(colors[colorIndex]);
		float randomDelay = Random.Range(0.1f, 0.2f);
		if(numIterations > 0){
			yield return new WaitForSeconds(randomDelay);
			numIterations--;
			StartCoroutine(AnimateBrickColors(colors, actualColor, brick, numIterations));
		}else{
			yield return 0;
			_numBricksAnimateComplete++;
			renderer.sharedMaterial = ConstructionController.Instance.resourcesProvider.GetMaterial(actualColor);
		}
	}

	IEnumerator WaitForAnimComplete(){
		while(_numBricksAnimateComplete != _numBricksToAnimate){
			yield return 0;
		}
	}

	public void AddBrick(Transform brickTransform, BrickData brickData ){
		construction.AddBrick(brickData);
		_bricks.Add(brickTransform);
	}

	Transform CreateBrick(BrickData brick, int designId, Transform container){

		var prefabRef = ConstructionController.Instance.resourcesProvider.GetPrefabForDesign(designId);
		if (prefabRef == null)
		{
			Debug.LogWarning(string.Format("No brick prefab was found for ID {0}. Skipping.", brick.design.id));
			return null;
		}
		
		var prefab = Instantiate<Transform>(prefabRef.transform);
		prefab.SetParent(container, false);
		prefab.localPosition = construction.TransformGridCoordinates(construction.GetBrickOrigin(brick.id));
		prefab.localScale = Vector3.one;
		
		LegoBrickId brickId = prefab.GetComponent<LegoBrickId>();
		if(brickId == null){
			brickId = prefab.gameObject.AddComponent<LegoBrickId>();
			brickId.designId = designId;
			brickId.coords = construction.GetBrickOrigin(brick.id);
		}
		
		brickId.id = brick.id;
		
		// Resetting rotation because it's not 0,0,0 on the prefab for some reason
		prefab.localRotation = Quaternion.identity;
		
		// LXFML inverts y rotation; that's why we rotate around "down"
		prefab.Rotate(Vector3.down, brick.rotation.eulerAngles.y,Space.Self);

		var renderer = prefab.GetComponentInChildren<MeshRenderer>();
		if(renderer != null)
			renderer.sharedMaterial = ConstructionController.Instance.resourcesProvider.GetMaterial(brick.materialId);
		
		return prefab;
	}

	void Update (){
		if (_isSpinning) {
			transform.RotateAround(_pivotReferencePosition,Vector3.up, _rotationSpeed * Time.deltaTime);
		}
	}

	public void StartSpinning(float speed){
		_rotationSpeed = speed;
		StartCoroutine (StartSpinningCoroutine());
	}

	IEnumerator StartSpinningCoroutine(){
		yield return new WaitForSeconds (0.2f);
		_isSpinning = true;
		//TODO : Yark!
		_pivotReferencePosition = new Vector3(0.3f,-4.22f,27.07f);
	}

	private int _decoMaxHeight = 5;
	private int _decoMinHeight = 2;
	private int _extrusionSize = 10;

	private enum WallPosition{
		Left,
		Right
	}

	private void SetWall(string containerName, List<BrickData> edgeBricks, WallPosition position){
		//Create transform to hold the bricks
		var container = new GameObject(containerName).transform;
		container.localScale = new Vector3 (1f,1f,1f);
		
		List<LXFMLCell> cellsForDeco = new List<LXFMLCell>();
		
		//FILTER ALL EDGE BRICKS TO RETRIEVE ELIGIBLE CELLS FOR DECO HOOK ON LOWER PART
		for(int i = 0 ; i < edgeBricks.Count - 1 ; i++){
			
			Vector2 brickOrigin = construction.GetBrickOrigin(edgeBricks[i].id);

			if(LXFMLHelper.IsBrickFlipped(edgeBricks[i])){
				for(int j = (int)brickOrigin.x - LXFMLHelper.GetBrickWidth(edgeBricks[i].design.id) + 1 ; j < (int)brickOrigin.x ; j++){
//					Debug.Log (j);
					if(!LXFMLHelper.IsSlopeUp(edgeBricks[i].design.id) || j == (int)brickOrigin.x){
						cellsForDeco.Add(construction.Grid.GetCellAt((float)j, brickOrigin.y));
					}
				}
			}else{
				for(int j = (int)brickOrigin.x; j < (int)brickOrigin.x  + LXFMLHelper.GetBrickWidth(edgeBricks[i].design.id); j++){
//					Debug.Log (j);
					if(!LXFMLHelper.IsSlopeUp(edgeBricks[i].design.id) || j == (int)brickOrigin.x){
						cellsForDeco.Add(construction.Grid.GetCellAt((float)j, brickOrigin.y));
					}
				}
			}
		}
		
		//FOR EACH ELIGIBLE CELL, FIND BOTTOM CELLS TO EXCLUDE FROM EXTRUSION
		
		Dictionary<LXFMLCell, List<LXFMLCell>> bricksToRemove = new Dictionary<LXFMLCell, List<LXFMLCell>>();
		Dictionary<LXFMLCell, List<int>> availableDecoHeights = new Dictionary<LXFMLCell, List<int>>();
		
		for(int i = 0 ; i < cellsForDeco.Count ; i++){
//			Debug.Log ("Testing " + cellsForDeco[i].Coordinates.ToString());
			int indexX = (int)cellsForDeco[i].Coordinates.x;
			int indexY = (int)cellsForDeco[i].Coordinates.y;
			
			bricksToRemove.Add(cellsForDeco[i], new List<LXFMLCell>());
			availableDecoHeights.Add(cellsForDeco[i], new List<int>());
			
			//if cell is first row, discard
			if(indexY == 0)
				break;
			
			//Stop lookup at index 1 or maxSize
			int endIndex = indexY - _decoMaxHeight < 0 ? 0 : indexY - _decoMaxHeight;
			
			int j = indexY - 1 ;
			
			for(j  = indexY - 1; j >= endIndex ; j--){
				LXFMLCell cell = construction.Grid.GetCellAt(indexX, j);
				bricksToRemove[cellsForDeco[i]].Add(cell);				
			}
			
			for(j  = 0; j < bricksToRemove[cellsForDeco[i]].Count ; j++){
				LXFMLCell currentCell = bricksToRemove[cellsForDeco[i]][j];
				if(j > _decoMinHeight-1 ){
					//DISCARD IF CELL HAS NO STUD TO HOOK THE DECO
					if(currentCell.Data != null && currentCell.Data.Brick != null && edgeBricks.Contains(currentCell.Data.Brick) ){
						BrickData currentBrick = currentCell.Data.Brick;
						int h = LXFMLHelper.GetBrickHeight(currentBrick.design.id);
						if(construction.CellHasUpperStudConnection(currentCell,currentBrick.id))
							availableDecoHeights[cellsForDeco[i]].Add(j);
					}
				}
			}
		}
		
		Dictionary<int, int> xIndexComparer = new Dictionary<int, int>();		
		
		//RETRIEVE LEFT/RIGHT MOST AVAILABLE CELL FOR DECO
		foreach(KeyValuePair<LXFMLCell, List<int>> kvp in availableDecoHeights){
			if(kvp.Value.Count == 0){
				bricksToRemove.Remove(kvp.Key);
			}else{
				if(kvp.Key.Data != null && kvp.Key.Data.Brick != null){
					int id = kvp.Key.Data.Brick.id;
					if(!xIndexComparer.ContainsKey(id)){
						if(position == WallPosition.Left)
							xIndexComparer.Add(id, int.MaxValue);
						else
							xIndexComparer.Add(id, int.MinValue);
					}
					int minY = -1;
					if(position == WallPosition.Left){
						minY = Mathf.Min(xIndexComparer[id], (int)kvp.Key.Coordinates.x);
					}else{
						minY = Mathf.Max(xIndexComparer[id], (int)kvp.Key.Coordinates.x);
					}
					xIndexComparer[id] = minY;
					
					for(int k = 0 ; k < kvp.Value.Count ; k++){
						Debug.Log ("Cell " + kvp.Key.Coordinates.ToString() + " can have " + kvp.Value[k].ToString() + " height deco");
					}
				}
				
			}
		}
		
		Dictionary<LXFMLCell, List<LXFMLCell>> toKeep = new Dictionary<LXFMLCell, List<LXFMLCell>>();
		
		//TRIM CELLS WHEN NOT LEFTMOST OF BRICK
		foreach(KeyValuePair<LXFMLCell, List<LXFMLCell>> kvp in bricksToRemove){
			if(kvp.Key.Data != null && kvp.Key.Data.Brick != null){
				int comparer = kvp.Key.Data.Brick.id;
				foreach(KeyValuePair<int, int> xIndices in xIndexComparer){
					if(comparer == xIndices.Key){
						if(xIndices.Value == (int)kvp.Key.Coordinates.x){
							toKeep.Add(kvp.Key, kvp.Value);
						}
					}
				}
			}
		}
		
		
		//PICK BEST MATCH FOR DECO
		if(toKeep.Count > 0){
			Debug.Log ("Deco avail " + toKeep.Count.ToString());
			//ONLY SUPPORT 1 DECO PER WALL, NEED TO SUPPORT MORE H & V
			int midIndex;
			if(toKeep.Count == 1){
				midIndex = 0;
			}else{
				midIndex = Mathf.RoundToInt((float)toKeep.Count / 2f) - 1;
			}
			
			int brickId = 0;
			
			LXFMLCell cellForDeco = null;
			
			int index = 0;

			foreach(KeyValuePair<LXFMLCell, List<LXFMLCell>> kvp in toKeep){
				if(index == midIndex){
					brickId = kvp.Key.Data.Brick.id;
					cellForDeco = kvp.Key;
				}
				index++;
			}
			
			if(cellForDeco != null){
				GameObject deco = ConstructionController.Instance.resourcesProvider.GetDecorationForHeight(availableDecoHeights[cellForDeco], LXFMLDecoration.DecoPosition.Wall);	

				if(deco != null){

					var decoWidth = deco.GetComponent<LXFMLDecoration>().width;
					var decoHeight = deco.GetComponent<LXFMLDecoration>().height;
					int start = ((_extrusionSize - (int)decoWidth)/2);
					int end = start + (int)decoWidth - 1;
										
					for(int i = 0 ; i < edgeBricks.Count ; i++){
						float posZ = +LXFMLHelper.kBrickSize.x;
						Transform brickT = _bricksLookup[edgeBricks[i].id];
						Vector2 gridPos = construction.GetBrickOrigin(edgeBricks[i].id);
						for(int j = 0 ; j < _extrusionSize ; j++){
							if((j < start || j > end) || (gridPos.y >= cellForDeco.Coordinates.y || gridPos.y < cellForDeco.Coordinates.y - (decoHeight))){
								GameObject copy = Instantiate (brickT.gameObject);
								copy.transform.localPosition = brickT.localPosition;
								copy.transform.localRotation = brickT.localRotation;
								copy.transform.SetParent(container, false);
								copy.transform.Translate(0f,0f,posZ, Space.World);
								extrudedBricksBySize[j].Add(copy);
							}
							posZ += LXFMLHelper.kBrickSize.x;
						}
					}				
					//construction starting position
					Vector3 pivotPoint = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
					int offsetIndex = int.MaxValue;
					
					for(int i = 0 ; i < _bricks.Count ; i++){
						pivotPoint.x = Mathf.Min(pivotPoint.x, _bricks[i].localPosition.x);
						pivotPoint.y = Mathf.Min(pivotPoint.y, _bricks[i].localPosition.y);
						pivotPoint.z = Mathf.Min(pivotPoint.z, _bricks[i].localPosition.z);
						offsetIndex = Mathf.Min(offsetIndex,(int)_bricks[i].GetComponent<LegoBrickId>().coords.x );
					}
				

					GameObject decoInstance = Instantiate(deco);
					decoInstance.transform.SetParent(container);
					
					var pos = pivotPoint;
					pos.x += ((cellForDeco.Coordinates.x - (float)offsetIndex) * LXFMLHelper.kBrickSize.x);
					pos.y += (cellForDeco.Coordinates.y * LXFMLHelper.kBrickSize.y) - (decoHeight * LXFMLHelper.kBrickSize.y);
					pos.z = position == WallPosition.Right ? ((float)start + 1f) * LXFMLHelper.kBrickSize.x : ((float)start + decoWidth) * LXFMLHelper.kBrickSize.x;
					pos.z += pivotPoint.z;


					decoInstance.transform.localPosition = pos;
					float rotY = position == WallPosition.Left ? 90f : 270f;
					decoInstance.transform.Rotate(Vector3.up, rotY);

					buildingDecorations.Add(decoInstance);

				}else{
					ExtrudeNoDeco(edgeBricks, container);
				}
			}
		}else{
			ExtrudeNoDeco(edgeBricks, container);
		}
		container.SetParent(transform,false);
	}

	private void ExtrudeNoDeco(List<BrickData> edgeBricks, Transform container){
		for(int i = 0 ; i < edgeBricks.Count ; i++){
			float posZ = +LXFMLHelper.kBrickSize.x;
			Transform brickT = _bricksLookup[edgeBricks[i].id];
			for(int j = 0 ; j < _extrusionSize ; j++){
				GameObject copy = Instantiate (brickT.gameObject);
				copy.transform.localPosition = brickT.localPosition;
				copy.transform.localRotation = brickT.localRotation;
				copy.transform.SetParent(container, false);
				copy.transform.Translate(0f,0f,posZ, Space.World);
				posZ += LXFMLHelper.kBrickSize.x;
				extrudedBricksBySize[j].Add(copy);
			}
		}
	}

	public int SortByXAscending(LXFMLCell cell1, LXFMLCell cell2)
	{
		int returnVal =  cell1.Coordinates.x.CompareTo(cell2.Coordinates.x);
		if(returnVal == 0)
			return cell1.Coordinates.y.CompareTo(cell2.Coordinates.y);
		return returnVal;
	}

	
	private void SetRoof(){
		var roofContainer = new GameObject("roofContainer").transform;
		roofContainer.localScale = new Vector3 (1f,1f,1f);


		var topEdgeBricks = construction.GetTopEdgeBricks();

		var wallBricks = new List<BrickData>();
		var roofBricks = new List<BrickData>();
		var roofCells = new List<LXFMLCell>();

		wallBricks.AddRange(construction.GetLeftEdgeBricks());
		wallBricks.AddRange(construction.GetRightEdgeBricks());

		for(int i = 0 ; i < topEdgeBricks.Count ; i++){
			List<LXFMLCell> brickCells = construction.GetBrickCells(topEdgeBricks[i].id);
			for(int j = 0 ; j < brickCells.Count ; j++){

				bool hasStud = true;
				LXFMLCell upperCell = construction.Grid.GetCellAt(brickCells[j].Coordinates.x,brickCells[j].Coordinates.y + 1f);
				if(upperCell != null && upperCell.Data != null && upperCell.Data.Brick != null){
					hasStud = false;
				}

				if (construction.CellHasUpperStudConnection(brickCells[j],topEdgeBricks[i].id) && hasStud){
					roofCells.Add(brickCells[j]);
				}
			}
			if(!wallBricks.Contains(topEdgeBricks[i])){
				roofBricks.Add(topEdgeBricks[i]);
			}
		}

		roofCells.Sort(SortByXAscending);

		float lastX = float.MinValue;
		float lastY = float.MinValue;

		int inc = 0;

		//Dictionary<Cell for deco position, maxWidth for deco>
		Dictionary<LXFMLCell, int> decoFootprints = new Dictionary<LXFMLCell, int>();

		LXFMLCell currentCell = roofCells[0];

		for(int i = 0 ; i < roofCells.Count ; i++){

			if((MathUtils.NearEqual(lastX, roofCells[i].Coordinates.x - 1f) && MathUtils.NearEqual(roofCells[i].Coordinates.y , lastY)) || i == 0){
				inc++;
			}else{
				//min roof deco width is 2
				if(inc >= 2){
					decoFootprints.Add(currentCell, inc);
				}
				currentCell = roofCells[i];
				inc = 1;
			}
			lastX = roofCells[i].Coordinates.x;
			lastY = roofCells[i].Coordinates.y;
		}

		currentCell = null;
		int maxY = int.MinValue;

		foreach (KeyValuePair<LXFMLCell, int> kvp in decoFootprints){
			int coordY = (int)kvp.Key.Coordinates.y;
			if(coordY > maxY){
				maxY = coordY;
				currentCell = kvp.Key;
			}
		}

		if(currentCell != null){
			//found a spot for roof deco
			GameObject deco = ConstructionController.Instance.resourcesProvider.GetDecorationForWidth(decoFootprints[currentCell], LXFMLDecoration.DecoPosition.Roof);
			if(deco != null){
				int maxDecoWidth = decoFootprints[currentCell];
				var decoWidth = deco.GetComponent<LXFMLDecoration>().width;
				var decoHeight = deco.GetComponent<LXFMLDecoration>().height;

				GameObject decoInstance = Instantiate(deco);
				decoInstance.transform.SetParent(roofContainer);

				//construction starting position
				Vector3 pivotPoint = new Vector3(float.MaxValue, float.MaxValue, float.MinValue);
				int offsetIndex = int.MaxValue;
				
				for(int i = 0 ; i < _bricks.Count ; i++){
					pivotPoint.x = Mathf.Min(pivotPoint.x, _bricks[i].localPosition.x);
					pivotPoint.y = Mathf.Min(pivotPoint.y, _bricks[i].localPosition.y);
					pivotPoint.z = Mathf.Max(pivotPoint.z, _bricks[i].localPosition.z);
					offsetIndex = Mathf.Min(offsetIndex,(int)_bricks[i].GetComponent<LegoBrickId>().coords.x );
				}

				var pos = pivotPoint;
				pos.x += ((currentCell.Coordinates.x - (float)offsetIndex + ((maxDecoWidth - decoWidth) / 2f)) * LXFMLHelper.kBrickSize.x);
				pos.y += ((currentCell.Coordinates.y + 1f) * LXFMLHelper.kBrickSize.y);
				pos.z += (((_extrusionSize - 2f) / 2f) + 2f) * LXFMLHelper.kBrickSize.x;
				
				decoInstance.transform.localPosition = pos;

				buildingDecorations.Add(decoInstance);
			}
		}

		for(int i = 0 ; i < roofBricks.Count ; i++){
			float posZ = LXFMLHelper.kBrickSize.x;
			Transform brickT = _bricksLookup[roofBricks[i].id];
			for(int j = 0 ; j < _extrusionSize ; j++){

				GameObject copy = Instantiate (brickT.gameObject);
				copy.transform.localPosition = brickT.localPosition;
				copy.transform.localRotation = brickT.localRotation;
				copy.transform.SetParent(roofContainer, false);
				copy.transform.Translate(0f,0f,posZ, Space.World);
				posZ += LXFMLHelper.kBrickSize.x;
				extrudedBricksBySize[j].Add(copy);

				/*
				for(int j = 0 ; j < _extrusionSize ; j++){
							if((j < start || j > end) || (gridPos.y >= cellForDeco.Coordinates.y || gridPos.y < cellForDeco.Coordinates.y - (decoHeight))){
								GameObject copy = Instantiate (brickT.gameObject);
								copy.transform.localPosition = brickT.localPosition;
								copy.transform.localRotation = brickT.localRotation;
								copy.transform.SetParent(container, false);
								copy.transform.Translate(0f,0f,posZ, Space.World);
								extrudedBricksBySize[j].Add(copy);
							}
							posZ += LXFMLHelper.kBrickSize.x;
						}
				 *
				 */
			}
		}
		roofContainer.SetParent(transform, false);
	}	

	public Vector3 GetActualConstructionPivot(){
		Vector3 pivotPoint = new Vector3(float.MaxValue, float.MaxValue, float.MinValue);
		int offsetIndex = int.MaxValue;
		
		for(int i = 0 ; i < _bricks.Count ; i++){
			pivotPoint.x = Mathf.Min(pivotPoint.x, _bricks[i].localPosition.x);
			pivotPoint.y = Mathf.Min(pivotPoint.y, _bricks[i].localPosition.y);
			pivotPoint.z = 0.4f;//Mathf.Max(pivotPoint.z, _bricks[i].localPosition.z);
			offsetIndex = Mathf.Min(offsetIndex,(int)_bricks[i].GetComponent<LegoBrickId>().coords.x );
		}

//		pivotPoint.x -= offsetIndex * LXFMLHelper.kBrickSize.x;
		
		return pivotPoint;
	}
	
	private Transform SetBackFace(){
		var backContainer = new GameObject("backContainer").transform;


		float offsetZ = (_extrusionSize + 1)* LXFMLHelper.kBrickSize.x;

		IList<BrickData> bricksData = construction.GetAllBricks();
		foreach (var brick in bricksData)
		{
			Transform brickT = _bricksLookup[brick.id];
			GameObject copy = Instantiate (brickT.gameObject);
			copy.transform.localPosition = brickT.localPosition;
			copy.transform.localRotation = brickT.localRotation;
			copy.transform.SetParent(backContainer, false);
			copy.transform.Translate(0f,0f,offsetZ, Space.World);
		}
		backContainer.SetParent(transform, false);
		return backContainer;
	}

	public List<GameObject> buildingDecorations;
	private List<List<GameObject>> extrudedBricksBySize;

	public void ShowDecorations(){
		for(int i = 0 ; i < buildingDecorations.Count ; i++){
			buildingDecorations[i].SetActive(true);
		}
	}

	public void EnableBricksByExtrusionIndex(int index){
		if(index == 0)
		{

		}else if(index == _extrusionSize + 1){
			_backContainer.gameObject.SetActive(true);
		}else{
			int extrusionIndex = index - 1;
			for(int j = 0 ; j < extrudedBricksBySize[extrusionIndex].Count ; j++){
				extrudedBricksBySize[extrusionIndex][j].SetActive(true);
			}
		}	
	}

	private Transform _backContainer;

	public void CreateBuilding(){
		buildingDecorations = new List<GameObject>();
		extrudedBricksBySize = new List<List<GameObject>>();
		for(int i = 0 ; i < _extrusionSize ; i++){
			extrudedBricksBySize.Add(new List<GameObject>());
		}
		SetWall("leftContainer", construction.GetLeftEdgeBricks(), WallPosition.Left);
		SetWall("rightContainer", construction.GetRightEdgeBricks(), WallPosition.Right);
		SetRoof();
		_backContainer = SetBackFace();

		_backContainer.gameObject.SetActive(false);

		for(int i = 0 ; i < buildingDecorations.Count ; i++){
			buildingDecorations[i].SetActive(false);
		}

		for(int i = 0 ; i < extrudedBricksBySize.Count ; i++){
			for(int j = 0 ; j < extrudedBricksBySize[i].Count ; j++){
				extrudedBricksBySize[i][j].SetActive(false);
			}
		}
	}
}
