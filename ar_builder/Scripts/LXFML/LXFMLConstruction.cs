using UnityEngine;
using System;
using System.Collections.Generic;
using Fusion;

public class LXFMLConstruction
{

	public class ConstructionLaser{
		public int brickId;
		public int power;
		public int direction = 0;
	}

	public static readonly Matrix4x4 kGridToWorldMatrix = Matrix4x4.TRS(Vector3.zero, 
				                                                        Quaternion.identity,
	                                                                    new Vector3(LXFMLHelper.kBrickSize.x, LXFMLHelper.kBrickSize.y, 1.0f));
	
	public int extrusionDepth = 0;

	public Action<ConstructionChangeset> OnChanged;

	readonly LXFMLGrid _grid;
	public LXFMLGrid Grid { get { return _grid; } }

	readonly Dictionary<int, BrickData> _bricks;
	readonly Dictionary<int, LXFMLCell[]> _bricksCellsCache;

	List<ConstructionLaser> _lasers;

	private int _lastBrickId;

	bool _rectIsDirty = true;
	Rect _gridRect;
	Rect Bounds
	{
		get
		{
			if (_rectIsDirty)
			{
				_gridRect = _grid.GetRect();
				_rectIsDirty = false;
			}
			return _gridRect;
		}
	}

	public LXFMLConstruction(IList<BrickData> allBricks){
		_grid = new LXFMLGrid(LXFMLHelper.kGridSize, LXFMLHelper.kGridSize, new LXFMLBottomLeftLinker());
		_bricks = new Dictionary<int, BrickData>();
		_bricksCellsCache = new Dictionary<int, LXFMLCell[]>();
		_gridRect = new Rect();

		for(int i = 0 ;i < allBricks.Count ; i++){
			AddBrick(allBricks[i]);
		}
	}

	public LXFMLConstruction()
	{
		_grid = new LXFMLGrid(LXFMLHelper.kGridSize, LXFMLHelper.kGridSize, new LXFMLBottomLeftLinker());
		_bricks = new Dictionary<int, BrickData>();
		_bricksCellsCache = new Dictionary<int, LXFMLCell[]>();
		_gridRect = new Rect();
	}

	public int GetNextBrickId(){
		return _lastBrickId + 1;
	}

	public void SetPowerBricks(){
		SetLasers();
		SetBombs();
	}

	void SetBombs(){
		//NOTHING FOR NOW
	}

	void RecurseNeighbourPowerBricks(BrickData brick, List<BrickData> visited){
		if(visited.Contains(brick) || !LXFMLHelper.IsPowerBrick(brick.design.id,brick.materialId))
			return;
		else
			visited.Add(brick);
		
		foreach(BrickData connected in brick.NeighbourBricks){
			RecurseNeighbourPowerBricks(connected, visited);
		}
	}

	public bool CellHasLowerStudConnection(LXFMLCell  cell, int brickId){
		bool hasStud = true;
		BrickData brickData = _bricks[brickId];
		int brickHeight = LXFMLHelper.GetBrickHeight(brickData.design.id);
		Vector2 brickOrigin = GetBrickOrigin(brickId);
		
		if(brickHeight == 1){
			if(LXFMLHelper.IsSlopeUp(brickData.design.id)){				
				if(!MathUtils.NearEqual(brickOrigin, cell.Coordinates)){
					hasStud = false;
				}
			}
		}else{
			if(!MathUtils.NearEqual(brickOrigin.y, cell.Coordinates.y)){
				hasStud = false;
			}else{
				if(LXFMLHelper.IsSlopeUp(brickData.design.id)){
					if(!MathUtils.NearEqual(brickOrigin.x, cell.Coordinates.x)){
						hasStud = false;
					}
				}
			}		
		}
		return hasStud;
	}

	public List<LXFMLCell> GetBrickCells(int brickId){
		List<LXFMLCell> cells = new List<LXFMLCell>();
		BrickData brickData = _bricks[brickId];
		int brickWidth = LXFMLHelper.GetBrickWidth(brickData.design.id);
		int brickHeight = LXFMLHelper.GetBrickHeight(brickData.design.id);
		Vector2 brickOrigin = GetBrickOrigin(brickId);

		int originX = !LXFMLHelper.IsBrickFlipped(brickData) ? (int)brickOrigin.x : (int)brickOrigin.x - brickWidth + 1;
		int originY = (int)brickOrigin.y;

//		Debug.Log ("brickId = " + brickId.ToString());
//		Debug.Log (brickOrigin);
//		Debug.Log (originX);

//		Debug.Log("***");

		for(int i = originX ; i < originX +  brickWidth; i++){
			for(int j = originY ; j < originY +  brickHeight; j++){
//				Debug.Log (i.ToString() + " - " + j.ToString());
				cells.Add(_grid.GetCellAt(i,j));
			}
           	
		}
//		Debug.Log("-------------------------");
		return cells;
	}


	public bool CellHasUpperStudConnection(LXFMLCell  cell, int brickId){
		bool hasStud = true;
		BrickData brickData = _bricks[brickId];
		int brickHeight = LXFMLHelper.GetBrickHeight(brickData.design.id);
		Vector2 brickOrigin = GetBrickOrigin(brickId);

		if(brickHeight == 1){
			if(LXFMLHelper.IsSlopeDown(brickData.design.id)){

				if(!MathUtils.NearEqual(brickOrigin, cell.Coordinates)){
					hasStud = false;
				}
			}
		}else{
			float upperY = cell.Coordinates.y + brickHeight - 1;
			if(!MathUtils.NearEqual(upperY, cell.Coordinates.y)){
				hasStud = false;
			}else{
				if(LXFMLHelper.IsSlopeDown(brickData.design.id)){
					if(!MathUtils.NearEqual(cell.Coordinates.x, cell.Coordinates.x))
						hasStud = false;
				}
			}
		}

		return hasStud;
	}

	public List<BrickData> GetNeighbourPowerBricks(BrickData powerBrick){
		
		List<BrickData> visited = new List<BrickData>();
		RecurseNeighbourPowerBricks(powerBrick, visited);
		return visited;
		
	}

	void AddLasers(List<BrickData> bricksList, int direction){
		for(int i = 0 ; i < bricksList.Count ; i++){
			if(LXFMLHelper.IsPowerBrick(bricksList[i].design.id, bricksList[i].materialId)){
				//Check upper and lower bricks
				int numUpperSlots = LXFMLHelper.UpperSlots(bricksList[i].design.id);
				int numLowerSlots = LXFMLHelper.LowerSlots(bricksList[i].design.id);
				Vector2 brickOrigin = GetBrickOrigin(bricksList[i].id);
				
				int inc = 0;
				int j = Mathf.RoundToInt(brickOrigin.x);
				int numUpperBricks = 0;
				
				
				while(inc < numUpperSlots ){
					LXFMLCell upperCell = _grid.GetCellAt(j, (int)brickOrigin.y + bricksList[i].design.height);
					if(upperCell != null && !upperCell.IsEmpty){
						if(upperCell.Data.Brick.design.type == BrickType.Normal || upperCell.Data.Brick.design.type == BrickType.SlopeDown || upperCell.Data.Brick.design.type == BrickType.CurveOut ){
							numUpperBricks++;
						}else{
							if(upperCell.Data.IsOrigin){
								numUpperBricks++;
							}
						}
					}
					j+= bricksList[i].Orientation;
					inc++;
				}
				
				inc = 0;
				j = Mathf.RoundToInt(brickOrigin.x);
				int numLowerBricks = 0;
				
				while(inc < numLowerSlots ){
					LXFMLCell lowerCell = _grid.GetCellAt(j, (int)brickOrigin.y - 1);
					if(lowerCell != null && !lowerCell.IsEmpty){
						if(lowerCell.Data.Brick.design.type == BrickType.Normal || lowerCell.Data.Brick.design.type == BrickType.SlopeUp || lowerCell.Data.Brick.design.type == BrickType.CurveIn ){
							numLowerBricks++;
						}else{
							if(lowerCell.Data.IsOrigin){
								numLowerBricks++;
							}else{
								if(j == GetBrickOrigin(lowerCell.Data.Brick.id).x){
									numLowerBricks++;
								}
							}
						}
					}
					j+= bricksList[i].Orientation;
					inc++;
				}
				
				if(numLowerBricks == numLowerSlots && numUpperBricks == numUpperSlots){
					CreateLaser(bricksList[i], direction);
				}
			}
		}
	}

	void CreateLaser(BrickData laserBrick, int direction){
		var laser = new ConstructionLaser();
		laser.brickId = laserBrick.id;
		laser.direction = direction;
		var neighbours = GetNeighbourPowerBricks(laserBrick);
		int power = 0;
		for(int k = 0 ; k < neighbours.Count ; k++){
			power += neighbours[k].design.width * neighbours[k].design.height;
		}
		laser.power = power / 2;
		_lasers.Add(laser);
	}


	void SetLasers(){
		_lasers = new List<ConstructionLaser>();
		AddLasers(GetLeftEdgeBricks(), -1);
		AddLasers(GetRightEdgeBricks(), 1);
		Debug.Log (_lasers.Count);
	}

	public ConstructionLaser GetLaser(int index){
		return _lasers[index];
	}

	public int NumLasers{
		get{
			return _lasers.Count;
		}
	}

	public void AddBrick(BrickData brick)
	{
		_bricks[brick.id] = brick;
		_bricksCellsCache[brick.id] = new LXFMLCell[brick.design.width * brick.design.height];

		var brickOrigin = GetNormalizedBrickOrigin(brick);

		var normalizedOrigin = new Vector2(brickOrigin.x, brickOrigin.y);
		
		if (brick.isFlipped)
		{
			normalizedOrigin.x = normalizedOrigin.x - (brick.design.width - 1);
		}
		
		LXFMLCell originCell = _grid.GetCellAt(brickOrigin);
		LXFMLCell cell;
		LXFMLCellData cellData;

		int cellCount = 0;

		for (int y = 0; y < brick.design.height; ++y)
		{
			for (int x = 0; x < brick.design.width; ++x)
			{
				cellData = new LXFMLCellData();

				cell = _grid.GetCellAt(normalizedOrigin.x + x, normalizedOrigin.y + y);

				if(normalizedOrigin.x + x >= LXFMLHelper.kGridSize || normalizedOrigin.y + y >= LXFMLHelper.kGridSize){
					Debug.LogWarning ("Invalid position");
				}

				_bricksCellsCache[brick.id][cellCount++] = cell;
				
				if (!cell.IsEmpty)
				{
					//Debug.LogWarningFormat("Overriting cell at {0}.", cell.Coordinates);
				}
				
				cellData.Brick = brick;
				
				//TODO: Is it necessary?
				cellData.IsFull = true;
				
				cell.Data = cellData;
				cell.BrickOrigin = originCell;
			}
		}

		SetConnectedBricks(brick, brickOrigin);

		originCell.Data.IsOrigin = true;

		CommitChange(ConstructionChangesetOperation.Addition, originCell.Data.Brick, originCell.Coordinates);

		_lastBrickId++;
	}

	void ConnectBricks(BrickData b1, BrickData b2){
		b1.ConnectedBricks.Add(b2);
		b2.ConnectedBricks.Add(b1);
		b1.NeighbourBricks.Add(b2);
		b2.NeighbourBricks.Add(b1);
	}

	private void SetConnectedBricks(BrickData brick, Vector2 brickOrigin){
		int numUpperSlots = LXFMLHelper.UpperSlots(brick.design.id);
		int numLowerSlots = LXFMLHelper.LowerSlots(brick.design.id);

		int inc = 0;
		int i = Mathf.RoundToInt(brickOrigin.x);

		
		while(inc < numUpperSlots ){
			LXFMLCell upperCell = _grid.GetCellAt(i, (int)brickOrigin.y + brick.design.height);
			if(upperCell != null && !upperCell.IsEmpty){
				if(upperCell.Data.Brick.design.type == BrickType.Normal || upperCell.Data.Brick.design.type == BrickType.SlopeDown || upperCell.Data.Brick.design.type == BrickType.CurveOut ){
					ConnectBricks(brick, upperCell.Data.Brick);
				}else{
					if(upperCell.Data.IsOrigin){
						ConnectBricks(brick, upperCell.Data.Brick);
					}
				}
			}
			i+= brick.Orientation;
			inc++;
		}		
		
		inc = 0;
		i = Mathf.RoundToInt(brickOrigin.x);
		
		while(inc < numLowerSlots ){
			LXFMLCell lowerCell = _grid.GetCellAt(i, (int)brickOrigin.y - 1);
			if(lowerCell != null && !lowerCell.IsEmpty){
				if(lowerCell.Data.Brick.design.type == BrickType.Normal || lowerCell.Data.Brick.design.type == BrickType.SlopeUp || lowerCell.Data.Brick.design.type == BrickType.CurveIn ){
					ConnectBricks(brick, lowerCell.Data.Brick);
				}else{
					if(lowerCell.Data.IsOrigin){
						ConnectBricks(brick, lowerCell.Data.Brick);
					}else{
						if(i == GetBrickOrigin(lowerCell.Data.Brick.id).x){
							ConnectBricks(brick, lowerCell.Data.Brick);
						}
					}
				}
			}
			i+= brick.Orientation;
			inc++;
		}

		var leftCell = _grid.GetCellAt(brickOrigin.x - 1f, brickOrigin.y);
		var rightCell = _grid.GetCellAt(brickOrigin.x + brick.design.width, brickOrigin.y);

		if(brick.Orientation < 0){
			leftCell = _grid.GetCellAt(brickOrigin.x - brick.design.width, brickOrigin.y);
			rightCell = _grid.GetCellAt(brickOrigin.x + 1, brickOrigin.y);
		}

		if(leftCell != null && !leftCell.IsEmpty){
			brick.NeighbourBricks.Add(leftCell.Data.Brick);
			leftCell.Data.Brick.NeighbourBricks.Add(brick);
		}

		if(rightCell != null && !rightCell.IsEmpty){
			brick.NeighbourBricks.Add(rightCell.Data.Brick);
			rightCell.Data.Brick.NeighbourBricks.Add(brick);
		}
	}

	public List<BrickData> GetTopEdgeBricks(){
		var edgeBricks = new List<BrickData>();
		var rect = _grid.GetRect();
		for(int i  = (int)rect.x ; i < (int)(rect.x + rect.width); i++){
			for(int j  = (int)(rect.y + rect.height - 1) ; j > (int)rect.y ; j--){
				var cellData = _grid.GetCellAt(new Vector2(i,j)).Data;
				if(cellData != null){
					if(!edgeBricks.Contains(cellData.Brick)){
						edgeBricks.Add(cellData.Brick);

					}
					break;
				}
			}
		}
//		edgeBricks.RemoveAt(0);
//		edgeBricks.RemoveAt(edgeBricks.Count - 1);
		return edgeBricks;
	}

	private int SortByGridYPosition(BrickData t1, BrickData t2)
	{		
		var cell1 = GetBrickCell(t1.id);
		var cell2 = GetBrickCell(t2.id);
		return cell2.Coordinates.y.CompareTo(cell1.Coordinates.y);
	}

	public List<BrickData> GetRightEdgeBricks(){
		var edgeBricks = new List<BrickData>();
		for(int i  = 0 ; i < _grid.Height ; i++){
			for(int j  = _grid.Width - 1 ; j >= 0 ; j--){
				var cellData = _grid.GetCellAt(j,i).Data;
				if(cellData != null){
					if(!edgeBricks.Contains(cellData.Brick))
						edgeBricks.Add(cellData.Brick);
					break;
				}
			}
		}

		edgeBricks.Sort(SortByGridYPosition);

		return edgeBricks;
	}

	public List<BrickData> GetLeftEdgeBricks(){
		var edgeBricks = new List<BrickData>();
		for(int i  = 0 ; i < _grid.Height ; i++){
			for(int j  = 0 ; j < _grid.Width ; j++){
				var cellData = _grid.GetCellAt(j,i).Data;
				if(cellData != null){
					if(!edgeBricks.Contains(cellData.Brick))
						edgeBricks.Add(cellData.Brick);
					break;
				}
			}
		}
		edgeBricks.Sort(SortByGridYPosition);
		return edgeBricks;
	}

	public void RemoveBrick(int brickId)
	{
		if (!_bricks.ContainsKey(brickId))
		{
			return;
		}

		var originCell = GetBrickCell(brickId);
		var brick = originCell.Data.Brick;

		foreach (var cell in _bricksCellsCache[brickId])
		{
			cell.Clear();
		}

		foreach(BrickData connected in brick.ConnectedBricks){
			connected.ConnectedBricks.Remove(brick);
		}

		brick.ConnectedBricks.Clear();

		_bricks.Remove(brickId);
		_bricksCellsCache.Remove(brickId);

		CommitChange(ConstructionChangesetOperation.Subtraction, brick, originCell.Coordinates);
	}

	public List<BrickData> BottomRow{
		get{
			List<BrickData> list = new List<BrickData>();
			for(int i = 0 ; i < LXFMLHelper.kGridSize ; i++){
				LXFMLCell cell = _grid.GetCellAt(i , 0);
				if(cell != null && !cell.IsEmpty)
					if(!list.Contains(cell.Data.Brick))
						list.Add(cell.Data.Brick);
			}

			return list;
		}
	}

	public LXFMLCell GetBrickCell(int brickId)
	{
		if (!_bricks.ContainsKey(brickId))
		{
			return null;
		}

		foreach (var cell in _bricksCellsCache[brickId])
		{
			if (cell.Data.IsOrigin)
			{
				return cell;
			}
		}

		return null;
	}

	public Vector2 GetBrickOrigin(int brickId)
	{
		var cell = GetBrickCell (brickId);

		if (cell == null)
		{
			throw new ArgumentException(string.Format("Could not find a brick with id: {0} in the current construction.", brickId));
		}

		return cell.Coordinates;
	}

	public Vector2? TryGetBrickOrigin(int brickId)
	{
		var cell = GetBrickCell (brickId);
		
		if (cell == null)
		{
			return null;
		}
		
		return cell.Coordinates;
	}

	public BrickData GetBrick(int brickId)
	{
		if (!_bricks.ContainsKey(brickId))
		{
			throw new ArgumentException(string.Format("Construction does not contain a brick with the requested ID of: {0}", brickId));
		}

		return _bricks[brickId];
	}

	public BrickData TryGetBrick(int brickId)
	{
		if (!_bricks.ContainsKey(brickId))
		{
			return null;
		}
		
		return _bricks[brickId];
	}

	public IList<BrickData> GetAllBricks()
	{
		return new List<BrickData>(_bricks.Values);
	}

	public Vector3 TransformGridCoordinates(Vector3 coordinates)
	{
		//TODO : Construction pivot point is hard-coded here, need to find a better way
		return kGridToWorldMatrix.MultiplyPoint3x4(coordinates);// - (Vector3)Bounds.max);//new Vector3(Bounds.center.x,  Bounds.min.y, 0f));
	}
	
	public Vector3 TransformGridCoordinates(Vector2 coordinates)
	{
		return TransformGridCoordinates((Vector3) coordinates);
	}
	
	public Vector3 TransformGridCoordinates(float x, float y)
	{
		return TransformGridCoordinates(new Vector3(x, y));
	}

	void CommitChange(ConstructionChangesetOperation operation, BrickData brick, Vector2 coordinates)
	{
		_rectIsDirty = true;

		if (OnChanged != null)
		{
			OnChanged(new ConstructionChangeset(operation, brick, coordinates));
		}
	}

	Vector2 GetNormalizedBrickOrigin(BrickData brick)
	{
		return new Vector2(Mathf.Floor((brick.position.x / LXFMLHelper.kBrickSize.x) + 0.499f),
		                   Mathf.Floor((brick.position.y / LXFMLHelper.kBrickSize.y) + 0.499f));
	}
}

public struct ConstructionChangeset
{
	public ConstructionChangesetOperation operation;
	public BrickData brick;
	public Vector2 coordinates;

	public ConstructionChangeset(ConstructionChangesetOperation operation, BrickData brick, Vector2 coordinates)
	{
		this.operation = operation;
		this.brick = brick;
		this.coordinates = coordinates;
	}
}

public enum ConstructionType{
	Vehicle,
	Building
}

public enum ConstructionChangesetOperation
{
	Addition, Subtraction
}