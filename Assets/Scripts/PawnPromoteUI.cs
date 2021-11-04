using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnPromoteUI : MonoBehaviour
{
    public Board board;

    //Limits for visualising the pawn promotion popup
    private const float horizontalParent = 14.5f; //Global scale
    private const float verticalParent = 21.7f; //Global scale
    private const float horizontal = 20f; //Local scale
    // private const float vertical = 4.5f; //Local scale

    [SerializeField] private Transform triangle;

    void OnEnable()
    {
        transform.rotation = board.cam.transform.rotation;
        Transform s = board.selectedPiece.transform; //shorthand

		float posX = 0f;
		float posY = 0f;
		float posZ = -2f;

		if (s.position.x > horizontalParent)
		{
			posX = horizontalParent;
		}
		else if (s.position.x < -horizontalParent)
		{
			posX = -horizontalParent;
		}
		else
		{
			posX = s.position.x;
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
			s.position.x,
			board.whiteOnMove ? posY + 4.95f : posY - 4.95f,
			posZ
		);
	}
}
