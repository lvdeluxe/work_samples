using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Fusion;

public class ConstructionEditor : MonoBehaviour {

	Vector3 _originalPos;
	bool _validPosition = true;
	bool _newBrickCreated = false;
	bool _isTweening = false;
	bool _doHightlight = true;
	Vector3 _startDragPosition;
	Vector2 _startDragCell;
	Vector2 _cellOffset = new Vector2();

	public TextAsset xml;
	public Material selectedMaterial;
	public Material invalidMaterial;
	
	public RemoveBrickButton removeButton;

	public Transform constructionTarget;

	LegoBrickId selectedPart;

	LegoBrickId draggingScenePart;	
	
	LXFMLCellData _dragCellData;

	private ConstructionEditorUI _modelUI;

	private ConstructionBuilder _builder;

	private Dictionary<int, LegoBrickId> _bricks = new Dictionary<int, LegoBrickId>();
	private Dictionary<int, LegoBrickId> _floatingBricks = new Dictionary<int, LegoBrickId>();

	void Start () {
		_modelUI = GetComponent<ConstructionEditorUI>();

		_builder = ConstructionController.Instance.SetConstruction(constructionTarget);

		if(_builder == null){
			_builder = ConstructionController.Instance.SetConstruction(xml.text, constructionTarget);
		}

		_builder.Init (ConstructionBuilder.BuilderType.Edit);

		foreach(Transform child in constructionTarget.transform){
			LegoBrickId legoBrick = child.GetComponent<LegoBrickId>();
			if(legoBrick != null)
				_bricks.Add(legoBrick.id, legoBrick);
		}
		selectedPart = null;

		UpdateAvailableParts (null);
		UpdateAvailableColors (null);
	}

	public void SaveConstruction(){
//		ConstructionController.Instance.ClearAllUserConstructions();
		ConstructionController.Instance.SaveConstruction(constructionTarget);
		SceneController.Instance.StartLoading("game");
	}

	void UpdateAvailableParts(LegoBrickId selected){

		List<int> list = new List<int> ();
		int selectedDesignId = -1;

		if (selected) {
			for (int i = 0; i < LXFMLHelper.brickSet.Length; i ++) {
				if (LXFMLHelper.GetBrickHeight(LXFMLHelper.brickSet[i]) <= LXFMLHelper.GetBrickHeight(selected.designId) && Mathf.Max(LXFMLHelper.UpperSlots(LXFMLHelper.brickSet[i]),LXFMLHelper.LowerSlots(LXFMLHelper.brickSet[i])) <= Mathf.Max(LXFMLHelper.UpperSlots(selected.designId),LXFMLHelper.LowerSlots(selected.designId))) {
						list.Add (LXFMLHelper.brickSet[i]);
				}
			}
			selectedDesignId = selected.designId;
		} else {
			for (int i = 0; i < LXFMLHelper.brickSet.Length; i ++) {
				list.Add (LXFMLHelper.brickSet[i]);
			}
		}

		_modelUI.UpdatePartsList(list, selected != null, selectedDesignId);

	}
	void UpdateAvailableColors(LegoBrickId selected){
		if (selected) {
			_modelUI.UpdateColorsList(LXFMLHelper.GetColors(selected.designId), _builder.construction.GetBrick(selected.id).materialId);
		}
	}
	
	void Update () {

		if(draggingScenePart){
#if UNITY_EDITOR
			Vector2 mousePos = Input.mousePosition;
#else
			Vector2 mousePos = Input.touchCount > 0 ? Input.GetTouch(0).position : Vector2.zero;
#endif
			Vector2 gridPos = screenPointToGridPosition(mousePos);

			float offsetX = gridPos.x - _startDragCell.x;
			float offsetY = gridPos.y - _startDragCell.y;
			Vector3 pos = new Vector3();
			pos.x = _startDragPosition.x + (offsetX * LXFMLHelper.kBrickSize.x);
			pos.y = _startDragPosition.y + (offsetY * LXFMLHelper.kBrickSize.y);
			draggingScenePart.transform.position = pos;

			TestValidPosition(gridPos);

			if(_validPosition && _floatingBricks.ContainsValue(draggingScenePart)){
				_floatingBricks.Remove(draggingScenePart.id);
			}
		}
	}

	void TestValidPosition(Vector2 gridPos){
		int width = LXFMLHelper.GetBrickWidth(draggingScenePart.designId);
		int heigth = LXFMLHelper.GetBrickHeight(draggingScenePart.designId);
		
		int xStartTest = Mathf.RoundToInt(gridPos.x - _cellOffset.x);
		int xEndTest = Mathf.RoundToInt(gridPos.x - _cellOffset.x + width );
		
		int yStartTest = Mathf.RoundToInt(gridPos.y - _cellOffset.y);
		int yEndTest = Mathf.RoundToInt(gridPos.y - _cellOffset.y + heigth);
		
		_validPosition = true;

		//TEST GRID LIMITS
		if(xStartTest < 0 || xEndTest > LXFMLHelper.kGridSize || yStartTest < 0 || yEndTest > LXFMLHelper.kGridSize )
			_validPosition = false;

		//TEST OCCUPIED CELLS
		if(_validPosition){
			for(int i = xStartTest ; i < xEndTest ; i++){
				for(int j = yStartTest ; j < yEndTest ; j++){
					LXFMLCell cell= _builder.construction.Grid.GetCellAt(i,j);
					if(cell != null && cell.Data != null){
						_validPosition = false;
						break;
					}
				}
			}
		}

		//TEST FLOATING BRICK
		if(_validPosition){
			int numContacts = 0;
			for(int i = xStartTest ; i < xEndTest ; i++){
				LXFMLCell cellBottom = _builder.construction.Grid.GetCellAt(i,yStartTest - 1);
				if(cellBottom != null && cellBottom.Data != null){
					if(cellBottom.Data.Brick.design.type == BrickType.Normal || cellBottom.Data.Brick.design.type == BrickType.SlopeUp || cellBottom.Data.Brick.design.type == BrickType.CurveIn )
						numContacts++;
					else{
						if(cellBottom.Data.IsOrigin)
							numContacts++;
						else{
							if(i == _builder.construction.GetBrickOrigin(cellBottom.Data.Brick.id).x)
								numContacts++;
						}
					}
				}
				LXFMLCell cellTop = _builder.construction.Grid.GetCellAt(i,yEndTest);

				if(cellTop != null && cellTop.Data != null){
					if(cellTop.Data.Brick.design.type == BrickType.Normal || cellTop.Data.Brick.design.type == BrickType.SlopeDown || cellTop.Data.Brick.design.type == BrickType.CurveOut )
						numContacts++;
					else{
						if(cellTop.Data.IsOrigin)
							numContacts++;
					}
				}
			}
			_validPosition = numContacts != 0;
		}
	}
	

	void LateUpdate () {
		if(selectedPart != null && _doHightlight){
			Material currentMat = _validPosition ? selectedMaterial : invalidMaterial;
			Color c = currentMat.GetColor("_TintColor");
			c.a = Time.time%1f;
			if(c.a>0.5f)c.a = 1-c.a;
			currentMat.SetColor("_TintColor",c);
			MeshFilter[] rs = selectedPart.gameObject.GetComponentsInChildren<MeshFilter>();
			foreach(MeshFilter r in rs){
				Matrix4x4 mat = Matrix4x4.TRS(r.transform.position, r.transform.rotation, r.transform.lossyScale * 1.02f);
				Graphics.DrawMesh(r.sharedMesh, mat, currentMat, r.gameObject.layer);
			}
		}
		if(_floatingBricks.Values.Count > 0){
			foreach(LegoBrickId brick in _floatingBricks.Values){
				Color c = invalidMaterial.GetColor("_TintColor");
				c.a = Time.time%1f;
				if(c.a>0.5f)c.a = 1-c.a;
				invalidMaterial.SetColor("_TintColor",c);
				MeshFilter[] rs = brick.gameObject.GetComponentsInChildren<MeshFilter>();
				foreach(MeshFilter r in rs){
					Matrix4x4 mat = Matrix4x4.TRS(r.transform.position, r.transform.rotation, r.transform.lossyScale * 1.02f);
					Graphics.DrawMesh(r.sharedMesh, mat, invalidMaterial, r.gameObject.layer);
				}
			}
		}
	}

	public void CheckSelectedBrick(){
		if(!_isTweening){
#if UNITY_EDITOR
			Vector2 mousePos = Input.mousePosition;
#else
			Vector2 mousePos = Input.touchCount > 0 ? Input.GetTouch(0).position : Vector2.zero;
#endif
			Vector2 gridPos = screenPointToGridPosition(mousePos);

			LXFMLCell cell = _builder.construction.Grid.GetCellAt(gridPos);
			if(cell != null && cell.Data != null){
				selectedPart = draggingScenePart = _bricks[cell.Data.Brick.id];
				_startDragPosition = selectedPart.transform.position;
				_startDragCell = gridPos;
				UpdateAvailableParts (selectedPart);
				UpdateAvailableColors (selectedPart);

				_dragCellData = cell.Data;

				_originalPos = selectedPart.transform.position;

				Vector2 blockOrigin = _builder.construction.GetBrickCell(selectedPart.id).Coordinates;
				if(cell.Data.Brick.isFlipped){
					blockOrigin.x -= cell.Data.Brick.design.width - 1;
				}
				_cellOffset.x = gridPos.x - blockOrigin.x;
				_cellOffset.y = gridPos.y - blockOrigin.y;

				_builder.construction.RemoveBrick(cell.Data.Brick.id);


			}else {
				_modelUI.UpdateColorsList(new int[0], 0);
				selectedPart = null;
				UpdateAvailableParts (null);
				UpdateAvailableColors (null);
			}
		}
	}

	public void ClearAllFloatingBricks(){
		_floatingBricks.Clear();
	}


	void RecurseVisitedBricks(BrickData brick, List<BrickData> visited){
		if(visited.Contains(brick))
			return;
		else
			visited.Add(brick);

		foreach(BrickData connected in brick.ConnectedBricks){
			RecurseVisitedBricks(connected, visited);
		}
	}


	public bool TestAllFloatingBricks(){

		ClearAllFloatingBricks();

		List<BrickData> visited = new List<BrickData>();

		foreach(var brickData in _builder.construction.BottomRow){
			RecurseVisitedBricks(brickData, visited);
		}

		foreach(var brickData in _builder.construction.GetAllBricks()){
			if(!visited.Contains(brickData))
				_floatingBricks.Add(_bricks[brickData.id].id, _bricks[brickData.id]);

		}
		return _floatingBricks.Values.Count == 0;

	}

	public void ReleaseDragPart(){
		if (draggingScenePart) {
			if(removeButton.IsOverButton){
				DestroyPart();
				_dragCellData = null;
				removeButton.IsOverButton = false;
			}else
				UnselectDragBrick ();
		}

	}

	public void Unselect(){
		_modelUI.UpdateColorsList(new int[0], 0);
		_dragCellData = null;
		selectedPart = null;
		draggingScenePart = null;
		_newBrickCreated = false;
	}

	void UnselectDragBrick(){
		if (draggingScenePart) {
			if(_validPosition){
				if(_dragCellData != null){
					Vector3 position = draggingScenePart.transform.position;
					//WEIRD HACK...I HAVE TO OFFSET BY HALF A BRICK TO GET THE CORRECT POSITION
					position.z -= LXFMLHelper.kBrickSize.x / 2f;
					_dragCellData.Brick.position = position;
					_builder.construction.AddBrick(_dragCellData.Brick);
				}else{
					CreateNewBrickData();
				}
				_modelUI.UpdateColorsList(LXFMLHelper.GetColors(draggingScenePart.designId), _builder.construction.GetBrick(draggingScenePart.id).materialId);
				_dragCellData = null;
				_bricks[draggingScenePart.id].transform.parent = constructionTarget;
				draggingScenePart = null;
				_newBrickCreated = false;

			}else{
				StartCoroutine(TweenInvalidBrick(_bricks[draggingScenePart.id].transform));
				draggingScenePart = null;
				_bricks.Remove(-1);
			}
		}				
	}

	IEnumerator TweenInvalidBrick(Transform t){

		Vector3 vel = Vector3.zero;
		_isTweening = true;

		while(!MathUtils.NearEqual(t.position, _originalPos)){
			t.position = Vector3.SmoothDamp(t.position, _originalPos, ref vel, 0.1f);
			yield return 0;
		}

		if(_newBrickCreated){
			Destroy (t.gameObject);
			_dragCellData = null;
			_newBrickCreated = false;
			selectedPart = null;
			UpdateAvailableParts (selectedPart);
			UpdateAvailableColors (selectedPart);
		}else{
			t.position = _originalPos;
			t.parent = constructionTarget;
			if(_dragCellData != null){
				_dragCellData.Brick.position = _originalPos;
				_builder.construction.AddBrick(_dragCellData.Brick);
				_validPosition = true;
			}
		}
		_isTweening = false;
	}

	public void UpdateColorPart(int colorId){
		if(selectedPart){
			var renderer = _bricks[selectedPart.id].gameObject.GetComponentInChildren<MeshRenderer>();
			renderer.sharedMaterial = ConstructionController.Instance.resourcesProvider.GetMaterial(colorId);
			_builder.construction.GetBrick(selectedPart.id).materialId = colorId;
			StartCoroutine(DelayHighlight());
			UpdateAvailableColors(selectedPart);
		}
	}

	IEnumerator DelayHighlight(){
		_doHightlight = false;
		yield return new WaitForSeconds(0.5f);
		_doHightlight = true;
	}

	public void ReplacePart(int designId, bool reverse){
		if(selectedPart != null){

			bool currentReversed = !MathUtils.NearEqual(selectedPart.transform.localRotation.eulerAngles.y, LXFMLHelper.GetDefaultRotation(selectedPart.designId));//(_construction.GetBrick(selectedPart.id));

			int brickId = selectedPart.id;

			//Destroy old one 
			Vector3 oldPosition = selectedPart.transform.position;
			_builder.DestroyBrick(selectedPart);
			_bricks.Remove(brickId);


			//Create new one
			BrickData brickData = new BrickData();

			brickData.design = new BrickDesignData();
			
			brickData.id =  _builder.construction.GetNextBrickId();
			brickData.materialId =  LXFMLHelper.GetColors(designId)[0];
			brickData.design.id = designId;
			brickData.design.width = LXFMLHelper.GetBrickWidth(designId);
			brickData.design.height = LXFMLHelper.GetBrickHeight(designId);
			brickData.design.type = LXFMLHelper.GetBrickType(designId);

			brickData.scale = Vector3.one;

			GameObject newBrick = Instantiate(ConstructionController.Instance.resourcesProvider.GetPrefabForDesign(designId));
			newBrick.transform.SetParent(constructionTarget);
			LegoBrickId newOne = newBrick.GetComponent<LegoBrickId>();
			newOne.id = brickData.id;

			newOne.transform.position = oldPosition;

			Vector3 position = oldPosition;
			//WEIRD HACK...I HAVE TO OFFSET BY HALF A BRICK TO GET THE CORRECT POSITION
			position.z -= LXFMLHelper.kBrickSize.x / 2f;
			
			brickData.position = position;

			float add = reverse?180:0;
			
			if (LXFMLHelper.GetBrickOrientation(newOne.designId) == LXFMLHelper.BrickOrientation.NegativeX) {
				newOne.transform.localEulerAngles = new Vector3 (0, 0f+add, 0);
			} else if (LXFMLHelper.GetBrickOrientation(newOne.designId) == LXFMLHelper.BrickOrientation.PositiveX) {
				newOne.transform.localEulerAngles = new Vector3 (0, 180f+add, 0);
			} else if (LXFMLHelper.GetBrickOrientation(newOne.designId) == LXFMLHelper.BrickOrientation.NegativeZ) {
				newOne.transform.localEulerAngles = new Vector3 (0, 90f+add, 0);
			} else if (LXFMLHelper.GetBrickOrientation(newOne.designId) == LXFMLHelper.BrickOrientation.PositiveZ) {
				newOne.transform.localEulerAngles = new Vector3 (0, 270f+add, 0);
			}


			//TODO : Fix Orientation stuff, it's getting messy...
			if(reverse ){
				if(!currentReversed){
					float xOffset = (brickData.design.width - 1) * LXFMLHelper.kBrickSize.x;
					if(LXFMLHelper.GetBrickOrientation(newOne.designId) == LXFMLHelper.BrickOrientation.PositiveZ)
						newOne.transform.Translate(0f, 0f, xOffset);
					else
						newOne.transform.Translate(-xOffset, 0f, 0f);
					brickData.position = newOne.transform.position;
				}
			}else{
				if (currentReversed){
					float xOffset = (brickData.design.width - 1) * LXFMLHelper.kBrickSize.x;
					if(LXFMLHelper.GetBrickOrientation(newOne.designId) == LXFMLHelper.BrickOrientation.PositiveZ)
						newOne.transform.Translate(0f, 0f, xOffset);
					else
						newOne.transform.Translate(-xOffset, 0f, 0f);
					brickData.position = newOne.transform.position;
				}
			}

			brickData.isFlipped = reverse;
			brickData.rotation = Quaternion.Euler(0f,-newOne.transform.localEulerAngles.y,0f);

			var renderer = newBrick.GetComponentInChildren<MeshRenderer>();
			renderer.sharedMaterial = ConstructionController.Instance.resourcesProvider.GetMaterial(brickData.materialId);
			selectedPart = newOne;

			_bricks.Add(newOne.id, newOne);

			_builder.AddBrick(_bricks[newOne.id].transform, brickData);

			UpdateAvailableParts (selectedPart);
			UpdateAvailableColors (selectedPart);

		}
	}

	public void DestroyPart(){
		if (selectedPart) {
			int partId = selectedPart.id;
			_builder.DestroyBrick(selectedPart);
			_bricks.Remove(partId);
			selectedPart = null;
			UpdateAvailableParts (null);
			UpdateAvailableColors (null);

		}
	}

	public void StopDragPart(){
		UnselectDragBrick();
	}

	void CreateNewBrickData(){

		BrickData brickData = new BrickData();

		brickData.design = new BrickDesignData();

		brickData.id =  _builder.construction.GetNextBrickId();
		brickData.materialId =  LXFMLHelper.GetColors(draggingScenePart.designId)[0];
		brickData.design.id = draggingScenePart.designId;
		brickData.design.width = LXFMLHelper.GetBrickWidth(draggingScenePart.designId);
		brickData.design.height = LXFMLHelper.GetBrickHeight(draggingScenePart.designId);
		brickData.design.type = LXFMLHelper.GetBrickType(draggingScenePart.designId);

		Vector3 position = draggingScenePart.transform.position;
		//WEIRD HACK...I HAVE TO OFFSET BY HALF A BRICK TO GET THE CORRECT POSITION
		position.z -= LXFMLHelper.kBrickSize.x / 2f;

		brickData.position = position;
		brickData.scale = Vector3.one;
		brickData.rotation = Quaternion.Euler(0f,-draggingScenePart.transform.rotation.eulerAngles.y,0f);
		
		brickData.isFlipped = LXFMLHelper.IsBrickFlipped(brickData);

		_builder.AddBrick(_bricks[-1].transform, brickData);

		draggingScenePart.id = brickData.id;



		_bricks.Remove(-1);

		if(_bricks.ContainsKey(draggingScenePart.id))
			_bricks.Remove(draggingScenePart.id);

		_bricks.Add(draggingScenePart.id, draggingScenePart);
	}

	public void StartDragPart(int designId, bool reverse, Vector2 touchPos){
		if(!_isTweening){
			GameObject newBrick = Instantiate(ConstructionController.Instance.resourcesProvider.GetPrefabForDesign(designId));
			newBrick.transform.SetParent(constructionTarget);
			LegoBrickId newOne = newBrick.GetComponent<LegoBrickId>();
			newOne.id = -1;

			Vector2 gridPos = screenPointToGridPosition((Vector3)touchPos);

			newOne.transform.position = new Vector3(0,gridPos.y*LXFMLHelper.kBrickSize.y,gridPos.x*LXFMLHelper.kBrickSize.x);

			_originalPos =  new Vector3(gridPos.x*LXFMLHelper.kBrickSize.x, gridPos.y*LXFMLHelper.kBrickSize.y, 0f);
			
			float add = reverse?180:0;
			
			if (LXFMLHelper.GetBrickOrientation(newOne.designId) == LXFMLHelper.BrickOrientation.NegativeX) {
				newOne.transform.localEulerAngles = new Vector3 (0, 0f+add, 0);
			} else if (LXFMLHelper.GetBrickOrientation(newOne.designId) == LXFMLHelper.BrickOrientation.PositiveX) {
				newOne.transform.localEulerAngles = new Vector3 (0, 180f+add, 0);
			} else if (LXFMLHelper.GetBrickOrientation(newOne.designId) == LXFMLHelper.BrickOrientation.NegativeZ) {
				newOne.transform.localEulerAngles = new Vector3 (0, 90f+add, 0);
			} else if (LXFMLHelper.GetBrickOrientation(newOne.designId) == LXFMLHelper.BrickOrientation.PositiveZ) {
				newOne.transform.localEulerAngles = new Vector3 (0, 270f+add, 0);
			}
					
			var renderer = newBrick.GetComponentInChildren<MeshRenderer>();
			renderer.sharedMaterial = ConstructionController.Instance.resourcesProvider.GetMaterial(LXFMLHelper.GetColors(designId)[0]);

			selectedPart = draggingScenePart = newOne;

			_cellOffset.x = reverse ? LXFMLHelper.GetBrickWidth(newOne.designId) - 1 : 0;
			_cellOffset.y = 0;
			_startDragCell.x = 0f;
			_startDragCell.y = 0f;
			_startDragPosition = Vector3.zero;

			_newBrickCreated = true;

			if(_bricks.ContainsKey(-1))
				_bricks.Remove(-1);

			_bricks.Add(-1,newOne);
		}
	}

	bool IsGridPointValid(Vector2 pt){
		return pt.x >=0f && pt.y >= 0f && pt.x < 16f && pt.y < 16f;
	}

	Vector2 screenPointToGridPosition(Vector3 screenPosition){
		Ray ray = Camera.main.ScreenPointToRay (screenPosition);
		Vector3 pos = constructionTarget.InverseTransformPoint(ray.origin - ray.direction * (ray.origin.z / ray.direction .z));
		return new Vector2(Mathf.RoundToInt(pos.x/LXFMLHelper.kBrickSize.x), Mathf.FloorToInt(pos.y/LXFMLHelper.kBrickSize.y));
	}
}
