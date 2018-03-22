using UnityEngine;
using System.Text;

public interface ILXFMLCellsLinker
{
	void Link(LXFMLCell[] cells, LXFMLGrid grid);
	void Dump(LXFMLGrid grid);
}

/// <summary>
/// Linking strategy for an LXFMLGrid.
/// [0,0] Is at the bottom left
/// x goes up left to right
/// y goes up bottom to top
/// </summary>
public class LXFMLBottomLeftLinker : LXFMLLinkerBase
{
	override public void Link (LXFMLCell[] cells, LXFMLGrid grid)
	{
		LXFMLCell cell;
		for (int i = 0, n = cells.Length; i < n; ++i)
		{
			cell = cells[i];
			
			cell.NeighbourTopLeft 		= grid.GetCellAt(new Vector2(cell.Coordinates.x - 1, cell.Coordinates.y + 1));
			cell.NeighbourTop			= grid.GetCellAt(new Vector2(cell.Coordinates.x, 	 cell.Coordinates.y + 1));
			cell.NeighbourTopRight 		= grid.GetCellAt(new Vector2(cell.Coordinates.x + 1, cell.Coordinates.y + 1));
			cell.NeighbourLeft	 		= grid.GetCellAt(new Vector2(cell.Coordinates.x - 1, cell.Coordinates.y));
			cell.NeighbourRight 		= grid.GetCellAt(new Vector2(cell.Coordinates.x + 1, cell.Coordinates.y));
			cell.NeighbourBottomLeft 	= grid.GetCellAt(new Vector2(cell.Coordinates.x - 1, cell.Coordinates.y - 1));
			cell.NeighbourBottom 		= grid.GetCellAt(new Vector2(cell.Coordinates.x, 	 cell.Coordinates.y - 1));
			cell.NeighbourBottomRight	= grid.GetCellAt(new Vector2(cell.Coordinates.x + 1, cell.Coordinates.y - 1));
		}
	}

	override public void Dump(LXFMLGrid grid)
	{
		for (int y = grid.Height-1; y >= 0; --y)
		{
			DumpLine(grid, y);
		}
	}
}


/// <summary>
/// Linking strategy for an LXFMLGrid.
/// [0,0] Is at the top left
/// x goes up left to right
/// y goes up top to bottom
/// </summary>
public class LXFMLTopLeftLinker : LXFMLLinkerBase
{
	override public void Link (LXFMLCell[] cells, LXFMLGrid grid)
	{
		LXFMLCell cell;
		for (int i = 0, n = cells.Length; i < n; ++i)
		{
			cell = cells[i];
			
			cell.NeighbourTopLeft 		= grid.GetCellAt(new Vector2(cell.Coordinates.x - 1, cell.Coordinates.y - 1));
			cell.NeighbourTop			= grid.GetCellAt(new Vector2(cell.Coordinates.x, 	 cell.Coordinates.y - 1));
			cell.NeighbourTopRight 		= grid.GetCellAt(new Vector2(cell.Coordinates.x + 1, cell.Coordinates.y - 1));
			cell.NeighbourLeft	 		= grid.GetCellAt(new Vector2(cell.Coordinates.x - 1, cell.Coordinates.y));
			cell.NeighbourRight 		= grid.GetCellAt(new Vector2(cell.Coordinates.x + 1, cell.Coordinates.y));
			cell.NeighbourBottomLeft 	= grid.GetCellAt(new Vector2(cell.Coordinates.x - 1, cell.Coordinates.y + 1));
			cell.NeighbourBottom 		= grid.GetCellAt(new Vector2(cell.Coordinates.x, 	 cell.Coordinates.y + 1));
			cell.NeighbourBottomRight	= grid.GetCellAt(new Vector2(cell.Coordinates.x + 1, cell.Coordinates.y + 1));
		}
	}

	override public void Dump(LXFMLGrid grid)
	{
		for (int y = 0; y < grid.Height; ++y)
		{
			DumpLine(grid, y);
		}
	}
}

public abstract class LXFMLLinkerBase : ILXFMLCellsLinker
{
	abstract public void Link (LXFMLCell[] cells, LXFMLGrid grid);
	abstract public void Dump(LXFMLGrid grid);

	protected void DumpLine(LXFMLGrid grid, int row)
	{
		var s = new StringBuilder();
		
		s.Append("<size=22><b>");
		
		for (int x = 0; x < grid.Width; ++x)
		{
			var cell = grid.GetCellAt(x, row);
			
			if (cell.IsEmpty)
			{
				s.Append("<color=grey>[O]</color>");
			}
			else
			{
				Color32 color = (Color32) LXFMLHelper.GetBrickColor(cell.Data.Brick.materialId);
				var colorString = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
				
				var str = string.Format("<color=#{0}>[O]</color>", colorString);
				
				s.Append(str);
			}
		}
		
		s.Append("</b></size>");
		
		Debug.Log(s.ToString());
	}
}