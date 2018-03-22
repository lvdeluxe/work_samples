using UnityEngine;

public class LXFMLCell
{
	public LXFMLCell NeighbourTopLeft		{ get; set; }
	public LXFMLCell NeighbourTop			{ get; set; }
	public LXFMLCell NeighbourTopRight		{ get; set; }
	public LXFMLCell NeighbourLeft			{ get; set; }
	public LXFMLCell NeighbourRight			{ get; set; }
	public LXFMLCell NeighbourBottomLeft	{ get; set; }
	public LXFMLCell NeighbourBottom		{ get; set; }
	public LXFMLCell NeighbourBottomRight	{ get; set; }

	public LXFMLCell BrickOrigin			{ get; set; }

	public LXFMLCellData Data { get; set; }
	public bool IsEmpty { get { return Data == null; } }

	Vector2 _coordinates;
	public Vector2 Coordinates { get { return _coordinates; } }

	public LXFMLCell(Vector2 coordinates)
	{
		_coordinates = coordinates;
	}

	public void Clear()
	{
		BrickOrigin = null;
		Data = null;
	}

}

