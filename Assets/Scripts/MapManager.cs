using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
public class MapManager : MonoBehaviour
{
	[SerializeField]
	private Tilemap map;

	[SerializeField]
	private Tilemap highlight;

	[SerializeField]
	private Tilemap movementHighlight;

	[SerializeField]
	private List<TileScriptable> tileDatas;

	[SerializeField]
	private TileBase movementArrow;

	[SerializeField]
	private TileBase movementEndArrow;

	[SerializeField]
	private MovementSystem movementSystem;

	private Dictionary<TileBase, TileScriptable> dataFromTiles;
	private Dictionary<Vector3Int, TileScriptable> mapTileData;
	private void Awake()
	{
		dataFromTiles = new Dictionary<TileBase, TileScriptable>();
		mapTileData = new Dictionary<Vector3Int, TileScriptable>();
		foreach (var tileData in tileDatas)
		{
			dataFromTiles.Add(tileData.tile, tileData);
		}
		foreach (var pos in map.cellBounds.allPositionsWithin)
		{
			Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
			if (map.HasTile(localPlace))
			{
				TileBase tile = map.GetTile(localPlace);
				TileScriptable tileScript = Object.Instantiate(dataFromTiles[tile]) as TileScriptable;
				tileScript.position = localPlace;
				mapTileData.Add(tileScript.position, tileScript);
			}
		}
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector3Int gridPosition = map.WorldToCell(mousePosition);
			TileBase clickedTile = map.GetTile(gridPosition);

			double encounterChance = dataFromTiles[clickedTile].encounterChance;
			EncounterScriptable encounterObject = dataFromTiles[clickedTile].encounter;

			print($"Position {gridPosition}");
		}
	}

	private void Paint(Vector3Int position, TileBase tile, string layer)
	{
		switch (layer)
		{
			case "base":
				break;
			case "highlight":
				highlight.SetTile(position, tile);
				break;
			case "movementHighlight":
				movementHighlight.SetTile(position, tile);
				break;
			default:
				map.SetTile(position, tile);
				break;
		}
	}

	public void PaintSingleTile(Vector3Int position, TileBase tile, string layer = "base")
	{
		Paint(position, tile, layer);
	}

	public void PaintValidPath(Vector3Int position, bool isEnd = false)
	{
		this.PaintSingleTile(position, (isEnd ? movementEndArrow : movementArrow), "movementHighlight");
	}

	public void PaintTiles(List<Vector3Int> positions, TileBase tile, string layer = "base")
	{
		foreach (var pos in positions)
		{
			Paint(pos, tile, layer);
		}
	}

	public void ClearTiles(string layer)
	{
		switch (layer)
		{
			case "base":
				map.ClearAllTiles();
				break;
			case "highlight":
				highlight.ClearAllTiles();
				break;
			default:
				break;
		}
	}

	public Vector3 GetPlayerTileCenterPosition(Vector3 worldPosition)
	{
		Vector3Int gridPosition = map.WorldToCell(worldPosition);
		return map.GetCellCenterWorld(gridPosition);
	}

	public Vector3 GetTileCenter(Vector3Int worldPosition)
	{
		return map.GetCellCenterWorld(worldPosition);
	}

	public Vector3Int GetPlayerTilePosition(Vector3 worldPosition)
	{
		return map.WorldToCell(worldPosition);
	}

	public List<TileScriptable> GetSurroundingTiles(Vector3 worldPosition, int maxRange)
	{
		Vector3Int gridPosition = map.WorldToCell(worldPosition);
		//List<Vector3Int> surroundingTiles = FindAllTileNeighbors(worldPosition, maxRange);
		GraphSearch.BFSResult bfsResult = GraphSearch.BFSGetRange(this, gridPosition, 30);
		List<Vector3Int> surroundingTiles = new List<Vector3Int>(bfsResult.GetRangePositions());
		//TileBase tile = map.GetTile(gridPosition);
		List<TileScriptable> tiles = new List<TileScriptable>();
		// Get surrounding Tile encounter Data
		foreach (var sTilePos in surroundingTiles)
		{
			//print(mapTileData[sTilePos]);
			//TileScriptable tileScript = mapTileData[sTilePos];
			tiles.Add(mapTileData[sTilePos]);
		}

		return tiles;
	}

	public TileBase GetHighlightTileAt(Vector3Int position)
	{
		return highlight.GetTile(position);
	}

	public TileBase GetHighlightTileAt(Vector2 position)
	{
		Vector3Int gridPosition = highlight.WorldToCell(position);
		return highlight.GetTile(gridPosition);
	}

	public TileScriptable GetTileAt(Vector3Int position)
	{
		return mapTileData[position];
	}

	public List<Vector3Int> FindAllTileNeighbors(Vector3Int gameOjectPosition)
	{
		//var gridPosition = map.WorldToCell(gameOjectPosition);

		if (!map.HasTile(gameOjectPosition))
		{
			Debug.LogWarning($"The position {gameOjectPosition} does not exist in the map!");
			return new List<Vector3Int>();
		}

		var sTilesPos = new List<Vector3Int>();

		//Vector3Int[] neighbourPositions = gridPosition.x % 2 == 0 ? neighbourPositionsEven : neighbourPositionsOdd;

		foreach (var neighbourPosition in Direction.GetDirectionList(gameOjectPosition.y))
		{
			var position = (gameOjectPosition + neighbourPosition);

			if (map.HasTile(position))
			{
				//var neighbour = map.GetTile(position);
				sTilesPos.Add(position);
			}
		}
		return sTilesPos;

		// or using Linq you could also write it as
		//return (from neighbourPosition in neighbourPositions select gridPosition + neighbourPosition into position where map.HasTile(position) select map.GetTile(position)).ToList();
	}

	public void ShowPaths(Party party)
	{
		movementSystem.ShowRange(party, this);
	}
	public void HidePaths(Party party)
	{
		movementSystem.HideRange(this);
	}
	public void SelectPath(Vector2 mousePosition)
	{
		Vector3Int gridPosition = highlight.WorldToCell(mousePosition);
		movementSystem.ShowPath(gridPosition, this);
	}
	public void MoveParty(Party party)
	{
		movementSystem.MoveParty(party, this);
	}
}

public static class Direction
{
	public static List<Vector3Int> directionsOffsetOdd = new List<Vector3Int> {
		new Vector3Int(-1, 1, 0), //N1
		new Vector3Int(0, 1, 0), //N2
		new Vector3Int(1, 0, 0), //E
		new Vector3Int(0, -1, 0), //S2
		new Vector3Int(-1, -1, 0), //S1
		new Vector3Int(-1, 0, 0), //W
	};

	public static List<Vector3Int> directionsOffsetEven = new List<Vector3Int> {
		new Vector3Int(0, 1, 0), //N1
		new Vector3Int(1, 1, 0), //N2
		new Vector3Int(1, 0, 0), //E
		new Vector3Int(1, -1, 0), //S2
		new Vector3Int(0, -1, 0), //S1
		new Vector3Int(-1, 0, 0), //W
	};

	public static List<Vector3Int> GetDirectionList(int y)
		=> y % 2 == 0 ? directionsOffsetOdd : directionsOffsetEven;
}
