using UnityEngine;
using System.Collections;

public class LXFMLBuildingConstruction : LXFMLConstruction {

	public BuildingTypes buildingType;

	public LXFMLBuildingConstruction(BuildingTypes pType):base(){
		buildingType = pType;
		extrusionDepth = GetExtrusionByType();
	}

	private int GetExtrusionByType(){
		switch(buildingType){
			default:
			case BuildingTypes.AbstractType1:
				return 8;
			case BuildingTypes.AbstractType2:
				return 10;
			case BuildingTypes.AbstractType3:
				return 12;
		}
	}
}

public enum BuildingTypes{
	//WE DONT KNOW THE TYPES YET, SO JUST SOME ABSTRACT STUFF FOR NOW
	AbstractType1,
	AbstractType2,
	AbstractType3
}
