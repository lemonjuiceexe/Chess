using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnPromoteUI : MonoBehaviour
{
    public Board board;

    //Limits for visualising the pawn promotion popup
    private const float horizontalParent = 14.5f; //Global scale
	private const float verticalParent = 21.7f; //Global scale

	[SerializeField] private Transform triangle;
	[SerializeField] private GameObject enabler;
	public void ShowUI(Vector3 pos)
    {
		enabler.SetActive(true);

		transform.rotation = board.cam.transform.rotation;

		float posX = 0f;
		float posY = 0f;
		float posZ = -2f;

		if (pos.x > horizontalParent)
		{
			posX = horizontalParent;
		}
		else if (pos.x < -horizontalParent)
		{
			posX = -horizontalParent;
		}
		else
		{
			posX = pos.x;
		}

		if (board.whiteOnMove)
		{
			posY = verticalParent;
		}
		else
		{
			posY = -verticalParent; ;
		}

		transform.position = new Vector3(
			posX,
			posY,
			posZ
		);
		triangle.position = new Vector3(
			pos.x,
			board.whiteOnMove ? posY + 4.95f : posY - 4.95f,
			posZ
		);
	}
}
