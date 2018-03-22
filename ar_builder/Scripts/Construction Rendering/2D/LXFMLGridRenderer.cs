using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LXFMLGridRenderer : MonoBehaviour
{
	const float kOffsetX = -256.0f;
	const float kOffsetY = -256.0f;

	const float kBrickSize = 32.0f;

	[SerializeField]
	LXFMLBrickRenderer _brickPrefab;

	[SerializeField]
	RectTransform _bricksParent;

	[SerializeField]
	Image _rect;

	[SerializeField]
	ConstructionModel _constructionModel;

	LXFMLConstruction _construction;

	void Start()
	{
		_construction = _constructionModel.Construction;

		var rect = _construction.Grid.GetRect();

		var rt = _rect.GetComponent<RectTransform>();

		rt.sizeDelta = new Vector2(rect.width * kBrickSize, rect.height * kBrickSize);
		rt.anchoredPosition = new Vector2(rect.x * kBrickSize, rect.y * kBrickSize);

		var origin = new Vector3(kOffsetX, kOffsetY, 0.0f);

		var bricks = _construction.GetAllBricks();

		foreach (var brick in bricks)
		{
			var brickImage = Instantiate<LXFMLBrickRenderer>(_brickPrefab);
			var t = brickImage.GetComponent<RectTransform>();
			var coords = (Vector3) _construction.GetBrickCell(brick.id).Coordinates;
			coords *= kBrickSize;
			brickImage.name = "[Brick] " + brick.id;
			brickImage.Render(brick);
			t.SetParent(_bricksParent);
			t.localScale = Vector3.one;
			t.localRotation = Quaternion.identity;
			t.localPosition = origin + coords;
		}
	}
}
