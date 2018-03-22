using UnityEngine;
using System.Text;

public class LXFMLGrid
{
	readonly int _width;
	readonly int _height;
	readonly int _count;

	public int Width { get { return _width; } }
	public int Height { get { return _height; } }
	public int Count { get { return _count; } }
	
	LXFMLCell[] _cells;
	ILXFMLCellsLinker _linker;
	
	public LXFMLGrid(int width, int height, ILXFMLCellsLinker linkingStrategy = null)
	{
		_width = width;
		_height = height;
		_count = width * height;

		if (linkingStrategy == null)
		{
			// Default to linking from bottom left
			linkingStrategy = new LXFMLBottomLeftLinker();
		}

		_linker = linkingStrategy;

		Initialize();
	}

	public LXFMLCell GetCellAt(Vector2 coordinates)
	{
		return GetCellAt(coordinates.x, coordinates.y);
	}

	public LXFMLCell GetCellAt(float x, float y)
	{
		return GetCellAt(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
	}

	public LXFMLCell GetCellAt(int x, int y)
	{
		if (x < 0 || x >= _width ||
		    y < 0 || y >= _height)
		{
			return null;
		}

		return _cells[(int)(y * _width + x)];
	}

	public Rect GetRect()
	{
		int x, y;

		int firstColumnWithContent = -1;
		int lastColumnWithContent = -1;
		int firstRowWithContent = -1;
		int lastRowWithContent = -1;

		bool hasData = false;

		for (x = 0; x < _width; ++x)
		{
			for (y = 0; y < _height; ++y)
			{
				if (!GetCellAt(x, y).IsEmpty)
				{
					hasData = true;

					// Columns
					if (firstColumnWithContent == -1)
					{
						firstColumnWithContent = x;
					}

					lastColumnWithContent = x;

					//Rows
					if (firstRowWithContent == -1 || y < firstRowWithContent)
					{
						firstRowWithContent = y;
					}

					if (y > lastRowWithContent)
					{
						lastRowWithContent = y;
					}
				}
			}
		}

		if (hasData)
		{
			return new Rect(firstColumnWithContent, 
			                firstRowWithContent, 
			                lastColumnWithContent - firstColumnWithContent + 1,
			                lastRowWithContent - firstRowWithContent + 1);
		}
		else
		{
			return new Rect(0.0f, 0.0f, 0.0f, 0.0f);
		}
	}

	public void Dump()
	{
		_linker.Dump(this);
	}
	
	void Initialize()
	{
		InitializeCells();
		_linker.Link(_cells, this);
	}
	
	void InitializeCells()
	{
		_cells = new LXFMLCell[_count];
		for (int i = 0; i < _count; ++i)
		{
			_cells[i] = new LXFMLCell(new Vector2(i % _width, (int) i / _width));
		}
	}
}

