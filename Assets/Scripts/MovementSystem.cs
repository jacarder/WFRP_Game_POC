using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GraphSearch;

public class MovementSystem : MonoBehaviour
{
	private BFSResult movementRange = new BFSResult();
	private List<Vector3Int> currentPath = new List<Vector3Int>();

	public void HideRange(MapManager mapManager)
	{
		foreach (Vector3Int tilePosition in movementRange.GetRangePositions())
		{
			mapManager.GetTileAt(tilePosition).ResetHighlight();
			mapManager.GetTileAt(tilePosition).ResetHighlightPath();
		}
		movementRange = new BFSResult();
	}

	public void ShowRange(Party party, MapManager mapManager)
	{
		CalculateRange(party, mapManager);
		foreach (Vector3Int tilePosition in movementRange.GetRangePositions())
		{
			mapManager.GetTileAt(tilePosition).ShowHighlight();
		}
	}

	public void CalculateRange(Party party, MapManager mapManager)
	{
		movementRange = GraphSearch.BFSGetRange(mapManager, mapManager.GetPlayerTilePosition(party.transform.position), party.movementPoints);
	}

	public void ShowPath(Vector3Int selectedTilePosition, MapManager mapManager)
	{
		if (movementRange.GetRangePositions().Contains(selectedTilePosition))
		{
			foreach (Vector3Int tilePosition in currentPath)
			{
				mapManager.GetTileAt(tilePosition).ResetHighlightPath();
			}
			currentPath = movementRange.GetPathTo(selectedTilePosition);
			foreach (Vector3Int tilePosition in currentPath)
			{
				mapManager.GetTileAt(tilePosition).HighlightPath(tilePosition == currentPath.Last());
			}
		}
	}

	public void MoveParty(Party party, MapManager mapManager)
	{
		Debug.Log($"Moving party");
		party.MoveThroughPath(currentPath.Select(pos => mapManager.GetTileCenter(pos)).ToList());
	}

	public bool IsTileInRange(Vector3Int tilePosition)
	{
		return movementRange.IsHexPositionInRange(tilePosition);
	}
}
