using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileScriptableObject", menuName = "ScriptableObject/Tile1")]
public class TileScriptable : ScriptableObject
{
	public TileBase tile;
	public double encounterChance = .5;
	public int movementCost = 10;
	public EncounterScriptable encounter;
	public TileBase highlightTile;
	public Vector3Int position;
	private MapManager mapManager;

	private void Awake()
	{
		mapManager = FindObjectOfType<MapManager>();
	}

	public void ToggleHighlight()
	{
		Debug.Log("tile: ");
		Debug.Log(mapManager.GetHighlightTileAt(this.position));
		if (mapManager.GetHighlightTileAt(this.position) == highlightTile)
		{
			mapManager.PaintSingleTile(this.position, null, "highlight");
		}
		else
		{
			mapManager.PaintSingleTile(this.position, this.highlightTile, "highlight");
		}
	}

	public void ShowHighlight()
	{
		mapManager.PaintSingleTile(this.position, this.highlightTile, "highlight");
	}

	public void ResetHighlight()
	{
		mapManager.PaintSingleTile(this.position, null, "highlight");
	}

	public void ResetHighlightPath()
	{
		mapManager.PaintSingleTile(this.position, null, "movementHighlight");
	}

	internal void HighlightPath(bool isEnd = false)
	{
		mapManager.PaintValidPath(this.position, isEnd);
	}
}
