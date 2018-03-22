using UnityEngine;
using System.Collections.Generic;
using System;
using Fusion;

public static class LXFMLHelper
{

	public static Vector2 kBrickSize  = new Vector2(0.8f,0.96f);
	public const int kGridSize = 18;
	public const int kBrickMaxSize = 6;

	public static int[] brickSet = new int[]{3005,3062,3004,2877,4216,30136,98283,60592,60593,3010,60594,60623,60596,3009,33243,3040,3665,60477,60481};

	public enum BrickOrientation{
		PositiveX,
		PositiveZ,
		NegativeX,
		NegativeZ,
	}

	public static BrickOrientation GetBrickOrientation(int designId){
		switch(designId){
		default:
		case 3005:
			return BrickOrientation.NegativeZ;
		case 3062:
			return BrickOrientation.NegativeZ;
		case 3004:
			return BrickOrientation.NegativeX;
		case 2877:	
			return BrickOrientation.NegativeX;	
		case 4216:
			return BrickOrientation.NegativeX;
		case 30136:
			return BrickOrientation.NegativeX;
		case 98283:
			return BrickOrientation.NegativeX;
		case 60592:		
			return BrickOrientation.NegativeX;
		case 60593:
			return BrickOrientation.NegativeX;
		case 3010:		
			return BrickOrientation.NegativeX;
		case 60594:
			return BrickOrientation.NegativeX;
		case 60623:
			return BrickOrientation.NegativeX;
		case 60596:
			return BrickOrientation.NegativeX;
		case 3009:
			return BrickOrientation.NegativeX;
		case 33243:
			return BrickOrientation.NegativeX;
		case 3040:
			return BrickOrientation.PositiveZ;
		case 3665:
			return BrickOrientation.PositiveZ;
		case 60477:
			return BrickOrientation.PositiveZ;
		case 60481:
			return BrickOrientation.PositiveZ;
		}
	}

	public static bool IsPowerBrick(int designId, int matId){
//		return designId == 3004 && matId == 21;
		return matId == 21;
	}
	
	public static float GetDefaultRotation(int designId){
		switch(designId){
		default:
		case 3005:
		case 3062:
		case 3004:
		case 2877:		
		case 3069:		
		case 4216:
		case 30136:
		case 98283:
		case 60592:		
		case 60593:
		case 63864:		
		case 3010:		
		case 60594:
		case 60623:
		case 60596:
		case 3009:
		case 33243:
			return 0f;
		case 3040:
		case 3665:
		case 60477:
		case 60481:
			return 270f;
		}
	}

	public static int LowerSlots(int designId){
		switch(designId){
		default:
		case 60623:
			return 0;
		case 3665:
		case 3062:
		case 3005:
			return 1;
		case 3040:
		case 60481:
		case 3004:
		case 2877:	
		case 4216:
		case 30136:
		case 98283:
		case 60592:	
		case 60593:
			return 2;
		case 33243:
			return 3;
		case 60477:
		case 3010:
		case 60594:
		case 60596:
			return 4;
		case 3009:
			return 6;			
		}
	}

	public static int UpperSlots(int designId){
		switch(designId){
		default:
		case 60623:
			return 0;
		case 3040:
		case 33243:
		case 3062:
		case 3005:
		case 60477:
		case 60481:
			return 1;
		case 3004:
		case 2877:	
		case 4216:
		case 30136:
		case 98283:
		case 60592:	
		case 60593:
		case 3665:
			return 2;		
		case 3010:	
		case 60594:
		case 60596:
			return 4;		
		case 3009:
			return 6;
		
		}
	}

	public static bool IsBrickFlipped(BrickData brick)
	{
		var rotation = brick.rotation.eulerAngles;
		
		if (MathUtils.NearEqual(rotation.y, 0.0f) || MathUtils.NearEqual(rotation.y, 90.0f))
		{
			return false;
		}
		
		if (MathUtils.NearEqual(rotation.y, 180.0f) || MathUtils.NearEqual(rotation.y, 270.0f))
		{
			return true;
		}
		
		throw new Exception(string.Format("Brick [{0}] has an invalid Y rotation of {1}.", brick.id, rotation.y));
	}

	public static int GetBrickWidth(int designId)
	{
		switch(designId)
		{
			case 3005:
			case 3062:
				return 1;
				
			case 3004:
			case 2877:
			case 3040:
			case 3069:
			case 3665:
			case 4216:
			case 30136:
			case 98283:
			case 60592:
			case 60481:
			case 60593:
				return 2;
				
			case 63864:
			case 33243:
				return 3;
				
			case 3010:
			case 60477:
			case 60594:
			case 60623:
			case 60596:
				return 4;
				
			case 3009:
				return 6;
		}
		
		return 0;
	}

	public static bool IsSpecial2By1Brick(int designId){
		return designId == 2877 || designId == 4216 || designId == 30136 || designId == 98283;
	}
	
	public static bool Is2By1Brick(int designId){
		return designId == 3004 || IsSpecial2By1Brick(designId);
	}
	
	public static int[] Get2X1Colors(){
		return new int[]{5, 21, 26, 102, 330, 24, 135, 192, 194};
	}


	public static int GetModelByColor(int colorId){
		switch(colorId){
		case 24:
			return 2877;
		case 135:
			return 4216;
		case 192:
			return 30136;
		case 194:
			return 98283;
		default:
		case 5:
		case 21:
		case 26:
		case 102:
		case 330:
			return 3004;
		}
	}

	public static int[] GetColors(int designId, int exclude){
		int[] colorsTmp = GetColors(designId);
		return colorsTmp;
//		int[] colors = new int[colorsTmp.Length - 1];
//		int inc = 0;
//		for(int i = 0 ; i < colorsTmp.Length ; i++){
//			if(colorsTmp[i] != exclude){
//				colors[inc] = colorsTmp[i];
//				inc++;
//			}
//		}
//		return colors;
	}

	public static int[] GetColors(int designId){
		switch(designId){
		default:
		case 60592:
		case 60593:
		case 60594:
		case 60596:
			return new int[]{1};
		case 60623:
		case 2877:
			return new int[]{24};
		case 33243:
			return new int[]{26};
		case 3005:
		case 3004:
		case 3010:
		case 3040:
			return new int[]{5, 21, 26, 102, 330};
		case 3009:
			return new int[]{5, 21, 26, 330};
		case 3062:
			return new int[]{192};
		case 3665:
			return new int[]{5, 21, 26, 102};
		case 4216:
			return new int[]{135};
		case 30136:
			return new int[]{192};
		case 60477:
			return new int[]{21, 26};
		case 60481:
			return new int[]{21, 26, 330};
		case 98283:
			return new int[]{194};
		}
	}
	
	public static int GetBrickHeight(int designId)
	{
		switch(designId)
		{
			case 2877:
			case 3004:
			case 3005:
			case 3009:
			case 3010:
			case 3040:
			case 3062:
			case 3069:
			case 3665:
			case 4216:
			case 30136:		
			case 60477:
			case 63864:
			case 98283:
				return 1;
				
			case 60592:
			case 60481:
			case 33243:
				return 2;
				
			case 60594:
			case 60593:
				return 3;
				
			case 60623:
			case 60596:
				return 6;
		}

		return 0;
	}

	public static Color GetBrickColor(int materialId)
	{
		switch(materialId)
		{
			default:
			case 1:
				return new Color(0.95f,0.95f,0.95f);
			case 5:
				return new Color(0.69f,0.62f,0.43f);
			case 21:
				return new Color(0.7f,0f,0f);
			case 24:
				return new Color(0.98f,0.78f,0.04f);
			case 26:
				return new Color(0f,0f,0f);
			case 102:
				return new Color(0.45f,0.58f,0.78f);
			case 135:
				return new Color(0.44f,0.5f,0.6f);
			case 141:
				return new Color(0f,0.27f,0.1f);
			case 192:
				return new Color(0.37f,0.19f,0.03f);
			case 194:
				return new Color(0.59f,0.59f,0.59f);
			case 330:
				return new Color(0.46f,0.46f,0.3f);
		}
	}

	public static BrickType GetBrickType(int designId)
	{
		if (IsSlopeDown(designId))
			return BrickType.SlopeDown;

		if (IsSlopeUp(designId))
			return BrickType.SlopeUp;

		if (IsCurvedIn(designId))
			return BrickType.CurveIn;

		if (IsCurvedOut(designId))
			return BrickType.CurveOut;

		return BrickType.Normal;
	}

	public static List<int> GetStraightBricksForLength(int len){
		var list = new List<int>();
		int len_tmp = len;
		while(len_tmp != 0){
			if(len_tmp >= 6){
				list.Add(3009);
				len_tmp-= 6;
			}else if(len_tmp >= 4){
				list.Add(3010);
				len_tmp-= 4;
			}else if(len_tmp >= 2){
				list.Add(3004);
				len_tmp-= 2;
			}else{
				list.Add(3005);
				len_tmp-= 1;
			}
		}

		return list;
	}
	
	public static bool IsSlopeDown(int designId)
	{
		return designId == 3040 || designId == 60481 || designId == 60477;
	}
	
	public static bool IsSlopeUp(int designId)
	{
		return designId == 3665;
	}
	
	public static bool IsSlope(int designId)
	{
		return IsSlopeDown(designId) || IsSlopeUp(designId);
	}

	// Can that even happen?
	public static bool IsCurvedIn(int designId)
	{
		//TODO: Implement
		return false;
	}

	public static bool IsCurvedOut(int designId)
	{
		return designId == 33243;
	}

	public static bool IsCurved(int designId)
	{
		return IsCurvedIn(designId) || IsCurvedOut(designId);
	}

	public static Matrix4x4 GetBrickMatrix(string values)
	{
		string[] split = values.Split(',');
		
		Matrix4x4 matrix = new Matrix4x4();
		
		matrix.m00 = float.Parse(split[0]);
		matrix.m01 = float.Parse(split[3]);
		matrix.m02 = float.Parse(split[6]);
		matrix.m03 = float.Parse(split[9]);
		
		matrix.m10 = float.Parse(split[1]);
		matrix.m11 = float.Parse(split[4]);
		matrix.m12 = float.Parse(split[7]);
		matrix.m13 = float.Parse(split[10]);
		
		matrix.m20 = float.Parse(split[2]);
		matrix.m21 = float.Parse(split[5]);
		matrix.m22 = float.Parse(split[8]);
		matrix.m23 = float.Parse(split[11]);
		
		matrix.m30 = 0f;
		matrix.m31 = 0f;
		matrix.m32 = 0f;
		matrix.m33 = 0f;
		
		return matrix;
	}
	
	public static Vector3 GetBrickPosition(Matrix4x4 matrix)
	{
		var x = matrix.m03;
		var y = matrix.m13;
		var z = -matrix.m23;
		
		return new Vector3(x, y, z);
	}
	
	public static Vector3 GetBrickScale(Matrix4x4 m)
	{
		var x = Mathf.Sqrt(m.m00 * m.m00 + m.m01 * m.m01 + m.m02 * m.m02);
		var y = Mathf.Sqrt(m.m10 * m.m10 + m.m11 * m.m11 + m.m12 * m.m12);
		var z = Mathf.Sqrt(m.m20 * m.m20 + m.m21 * m.m21 + m.m22 * m.m22);
		
		return new Vector3(x, y, z);
	}
	
	public static Quaternion GetBrickRotation(Matrix4x4 matrix)
	{
		return Quaternion.LookRotation(matrix.GetColumn(2),matrix.GetColumn(1));
	}
}

