using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    //Column, row
    public int column;
    public int row;

    public bool occupied = false;
    public bool legalForSelectedPiece = false;
    public Piece currentPiece;

    public Board board;

    bool transitioning = false;
    Transform startPos;
    Transform endPos;
    private float startTime;
    private float dist;
    GameObject transPiece;

    private void Start()
    {

        //Applying column and row number 
        switch (transform.position.x)
        {
            case -35f:
                column = 0;
                break;
            case -25f:
                column = 1;
                break;
            case -15f:
                column = 2;
                break;
            case -5f:
                column = 3;
                break;
            case 5f:
                column = 4;
                break;
            case 15f:
                column = 5;
                break;
            case 25f:
                column = 6;
                break;
            case 35f:
                column = 7;
                break;
            default:
                Debug.LogError("Square's x position is not correct");
                break;
        }

        switch (transform.position.y)
        {
            case -35f:
                row = 0;
                break;
            case -25f:
                row = 1;
                break;
            case -15f:
                row = 2;
                break;
            case -5f:
                row = 3;
                break;
            case 5f:
                row = 4;
                break;
            case 15f:
                row = 5;
                break;
            case 25f:
                row = 6;
                break;
            case 35f:
                row = 7;
                break;
            default:
                Debug.LogError("Square's y position is not correct");
                break;
        }
    }

    private void FixedUpdate()
    {
        if(transitioning)
        {
            float distCovered = (Time.time - startTime) * board.transitionSpeed; // calculates covered distance
            float distFraction = distCovered / dist; // calculates what fraction of the distance it covered
            transPiece.transform.position = Vector3.Lerp(startPos.position, endPos.position, distFraction); // moves the piece using fancy math stuff
            // if the piece is near its destination, then stop moving, we dont need it
            if (Vector3.Distance(transPiece.transform.position, this.transform.position) < 0.01f)
            {
                transitioning = false;
                transPiece.transform.position = new Vector3(Mathf.Round(transPiece.transform.position.x), Mathf.Round(transPiece.transform.position.y), 0f);
            }
        }
    }

    private void OnMouseDown()
    {
        if (legalForSelectedPiece)
        {
            MovePiece();
        }
    }

    //Return value indicates if piece was actually moved
    public bool MovePiece()
    {
        //If selected piece's on move (or if testing is disabled)
        if(board.whiteOnMove == board.selectedPiece.white || board.disableForcedColorMoves)
        {
            if (!board.selectedPiece.hasMoved)
            {
                board.selectedPiece.hasMoved = true;
            }

            board.selectedPiece.currentSquare.currentPiece = null;
            board.selectedPiece.currentSquare.occupied = false;

            // old teleporting, without transition
            //board.selectedPiece.transform.position = this.transform.position;

            #region Transition
            //basically assigns every needed variable
            startPos = board.selectedPiece.transform; //sets start and end positions
            endPos = this.transform;
            transPiece = board.selectedPiece.gameObject; // keeps the piece in memory since its erased from selectedPiece now
            startTime = Time.time; // time when we started moving
            dist = Vector3.Distance(startPos.position, endPos.position); //calculates transition distance
                                                                         // sets info that we can now move the piece
            transitioning = true;
            #endregion

            //Board rotation
            if (!board.disableTurnBoard)
            {
                board.cam.transform.Rotate(0, 0, 180);
            }

            this.occupied = true;
            board.selectedPiece.currentSquare = this;
            this.currentPiece = board.selectedPiece;

            board.selectedPiece = null;

            board.ClearLegal();

            board.whiteOnMove = !board.whiteOnMove;

            return true;
        }
        else
        {
            board.ClearLegal();
            board.selectedPiece = null;

            return false;
        }
    }
}