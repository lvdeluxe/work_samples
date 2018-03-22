using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class BrickData
{
	public int id;
	public bool isFlipped;
	public int Orientation{
		get{
			return isFlipped ? -1 : 1 ;
		}
	}

	public HashSet<BrickData> ConnectedBricks {get; private set;}
	public HashSet<BrickData> NeighbourBricks {get; private set;}

	public int materialId;

	public Vector3 position;
	public Vector3 scale;
	public Quaternion rotation;

	public BrickDesignData design;

	public BrickData()
	{
		ConnectedBricks = new HashSet<BrickData>();
		NeighbourBricks = new HashSet<BrickData>();
	}

}

[Serializable]
public struct BrickDesignData
{
	public int id;
	public int width;
	public int height;
	public BrickType type;
}

[Serializable]
public enum BrickType
{
	Normal,
	SlopeUp, SlopeDown,
	CurveIn, CurveOut
}