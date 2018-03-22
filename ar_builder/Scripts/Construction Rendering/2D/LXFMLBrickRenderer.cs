using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LXFMLBrickRenderer : MonoBehaviour
{
	[SerializeField]
	Image _fullImage;

	[SerializeField]
	Image _slopeImage;

	GameObject _fullGO;
	GameObject _slopeGO;

	RectTransform _fullTransform;
	RectTransform _slopeTransform;

	void Awake()
	{
		_fullGO = _fullImage.gameObject;
		_slopeGO = _slopeImage.gameObject;

		_fullTransform = _fullGO.GetComponent<RectTransform>();
		_slopeTransform = _slopeGO.GetComponent<RectTransform>();

//		Test ();
	}

	public void Render(BrickData brick)
	{
		Reset();

		if (brick.design.type == BrickType.Normal)
		{
			RenderNormal(brick);
		}
		else
		{
			RenderSlope(brick);
		}

		_fullImage.color = LXFMLHelper.GetBrickColor(brick.materialId);
		_slopeImage.color = LXFMLHelper.GetBrickColor(brick.materialId);
	}

	void RenderNormal (BrickData brick)
	{
		_fullGO.SetActive(true);

		var scaleX = (float) brick.design.width;
		var scaleY = (float) brick.design.height;

		if (brick.isFlipped)
		{
			scaleX *= -1.0f;
			_fullTransform.localPosition = Vector3.right * _fullTransform.rect.width;
		}

		_fullTransform.localScale = new Vector3(scaleX, scaleY, 1.0f);
	}

	void RenderSlope (BrickData brick)
	{
		_fullGO.SetActive(true);
		_slopeGO.SetActive(true);
		
		_fullTransform.localScale = new Vector3(1.0f, (float) brick.design.height, 1.0f);

		var scaleX = (float) brick.design.width - 1.0f;
		var scaleY = (float) brick.design.height;
		
		if (brick.isFlipped)
		{
			scaleX *= -1.0f;
		}
		else
		{
			_slopeTransform.localPosition = Vector3.right * _slopeTransform.rect.width;
		}

		if (brick.design.type == BrickType.SlopeUp || brick.design.type == BrickType.CurveIn)
		{
			var pos = _slopeTransform.localPosition;
			_slopeTransform.localPosition = new Vector3(pos.x, _slopeTransform.rect.width * scaleY, pos.z);
			scaleY *= -1.0f;
		}
		
		_slopeTransform.localScale = new Vector3(scaleX, scaleY, 1.0f);
	}

	void Reset()
	{
		_fullGO.SetActive(false);
		_slopeGO.SetActive(false);
	}

	void Test()
	{
		var brick = new BrickData();
		brick.design = new BrickDesignData();
		
		brick.id =  -1;
		brick.materialId =  102;
		brick.design.id = 60481;
		brick.design.width = LXFMLHelper.GetBrickWidth(brick.design.id);
		brick.design.height = LXFMLHelper.GetBrickHeight(brick.design.id);
		brick.design.type = LXFMLHelper.GetBrickType(brick.design.id);

		brick.position = Vector3.zero;
		brick.scale = Vector3.one;
		brick.rotation = Quaternion.identity;
		
		brick.isFlipped = true;

		Render(brick);
	}
}
