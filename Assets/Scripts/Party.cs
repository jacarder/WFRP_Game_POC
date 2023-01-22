using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Party : MonoBehaviour
{
	private MapManager mapManager;
	private Vector2 position;
	private bool hasMoved;
	private bool isSelected = false;

	private float movementDuration = 0.5f, rotationDuration = 0.3f;
	public int movementPoints = 20;

	[SerializeField]
	private EncounterScriptable encounter;

	private Queue<Vector3> pathPositions = new Queue<Vector3>();

	public event Action<Party> MovementFinished;

	public Animator movementAnimator;

	private void Awake()
	{
		movementAnimator = GetComponent<Animator>();
		movementAnimator.enabled = false;
		mapManager = FindObjectOfType<MapManager>();
		Vector3 gridPosition = mapManager.GetPlayerTileCenterPosition(transform.position);
		transform.position = gridPosition;
	}

	private void OnMouseDown()
	{

	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

			if (hit)
			{
				if (hit.collider.gameObject == this.gameObject)
				{
					isSelected = true;

					// List<TileScriptable> tiles = mapManager.GetSurroundingTiles(transform.position, 2);
					// List<Vector3Int> tilePositions = new List<Vector3Int>();
					// foreach (var tile in tiles)
					// {
					// 	//print($"{tile.position} {tile.encounter?.type}");
					// 	tile.ToggleHighlight();
					// 	//tilePositions.Add(tile.position);
					// }
					// //mapManager.PaintTiles(tilePositions);
					mapManager.ShowPaths(this);
				}
				else
				{
					//  Click other object
					isSelected = false;
					//mapManager.ClearTiles("highlight");
				}
			}
			else
			{
				// Click outside of any object
				if (mapManager.GetHighlightTileAt(mousePos) == null)
				{
					isSelected = false;
					//mapManager.ClearTiles("highlight");
					mapManager.HidePaths(this);
				}
				else //	Clicked on highlight
				{
					mapManager.SelectPath(mousePos);
				}
			}
		}
		//	Move test
		if (Input.GetKeyUp("space"))
		{
			mapManager.MoveParty(this);
		}
	}

	public void MoveThroughPath(List<Vector3> currentPath)
	{
		pathPositions = new Queue<Vector3>(currentPath);
		Vector3 firstTarget = pathPositions.Dequeue();
		StartCoroutine(MovementCoroutine(firstTarget));
	}

	// private IEnumerator RotationCoroutine(Vector3 endPosition, float rotationDuration)
	// {
	// 	Quaternion startRotation = transform.rotation;
	// 	endPosition.z = transform.position.z;
	// 	Vector3 direction = endPosition - transform.position;
	// 	Quaternion endRotation = Quaternion.LookRotation(direction, Vector3.up);
	// 	if (Mathf.Approximately(Mathf.Abs(Quaternion.Dot(startRotation, endRotation)), 1.0f) == false)
	// 	{
	// 		float timeElapsed = 0;
	// 		while (timeElapsed < rotationDuration)
	// 		{
	// 			timeElapsed += Time.deltaTime;
	// 			float lerpStep = timeElapsed / rotationDuration;
	// 			transform.rotation = Quaternion.Lerp(startRotation, endRotation, lerpStep);
	// 			yield return null;
	// 		}
	// 		transform.rotation = endRotation;
	// 	}
	// 	StartCoroutine(MovementCoroutine(endPosition));
	// }

	private IEnumerator MovementCoroutine(Vector3 endPosition)
	{
		Vector3 startPosition = transform.position;
		//endPosition.y = startPosition.y;
		float timeElapsed = 0;

		while (timeElapsed < movementDuration)
		{
			movementAnimator.Play("TokenMove");
			movementAnimator.enabled = true;
			timeElapsed += Time.deltaTime;
			float lerpStep = timeElapsed / movementDuration;
			transform.position = Vector3.Lerp(mapManager.GetPlayerTileCenterPosition(startPosition), mapManager.GetPlayerTileCenterPosition(endPosition), lerpStep);
			yield return null;
		}

		transform.position = endPosition;
		Debug.Log("Transform.position " + transform.position);

		if (pathPositions.Count > 0)
		{
			Debug.Log("Selecting the next position!");
			StartCoroutine(MovementCoroutine(pathPositions.Dequeue()));
		}
		else
		{
			Vector3 gridPosition = mapManager.GetPlayerTilePosition(transform.position);
			Debug.Log("Movement finished! " + transform.position + " Grid pos: " + gridPosition);
			movementAnimator.enabled = false;
			MovementFinished?.Invoke(this);
		}
	}
}
