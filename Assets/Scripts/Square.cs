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
	//En passant 'able
	public bool epable;
	int epCountdown = 0;

	public Board board;

	bool transitioning = false;
	bool rookTransitioning = false;
	Transform startPos;
	Transform rookStartPos;
	Transform endPos;
	Transform rookEndPos;
	private float startTime;
	private float rookStartTime;
	private float dist;
	private float rookDist;
	GameObject transPiece;
	GameObject transRook;

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

	private void Update()
	{
		if(board.lastFrameWhiteOnMove != board.whiteOnMove && epable && epCountdown < 1)
		{
			GetComponent<SpriteRenderer>().color = Color.blue;
			epCountdown++;
		}
		if(epCountdown >= 1)
		{
			epCountdown = 0;
			epable = false;
		}
	}

	private void FixedUpdate()
	{
		if (transitioning)
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
		if(rookTransitioning)
		{
			float rookDistCovered = (Time.time - rookStartTime) * board.transitionSpeed;
			float rookDistFraction = rookDistCovered / rookDist;
			transRook.transform.position = Vector3.Lerp(rookStartPos.position, rookEndPos.position, rookDistFraction);

			if(Vector3.Distance(transRook.transform.position, rookEndPos.position) < 0.01f)
			{
				rookTransitioning = false;
				transRook.transform.position = new Vector3(Mathf.Round(transRook.transform.position.x), Mathf.Round(transRook.transform.position.y), 0f);
			}
		}
	}

	private void OnMouseDown()
	{
		//Normal move
		if (legalForSelectedPiece)
		{
			MovePiece();
		}
		//Castle
		else if (board.selectedPiece && board.selectedPiece.castleMove.Contains(this))
		{
			Piece castleRook;
			Square rookDest;
			//White kingside
			if (column == 6 && row == 0)
			{
				castleRook = board.pieces[6];
				rookDest = board.children[61].GetComponent<Square>();
			}
			//Black kingside
			else if (column == 6 && row == 7)
			{
				castleRook = board.pieces[22];
				rookDest = board.children[5].GetComponent<Square>();
			}
			//White queenside
			else if (column == 2 && row == 0)
			{
				castleRook = board.pieces[7];
				rookDest = board.children[59].GetComponent<Square>();
			}
			//Black queenside
			else if(column == 2 && row == 7)
			{
				castleRook = board.pieces[23];
				rookDest = board.children[3].GetComponent<Square>();
			}
			else
			{
				Debug.LogError("Castlemove doesn't correspond to any of the four castling options");
				return;
			}

			//King move
			MovePiece();

			//Manual move of the rook
			castleRook.hasMoved = true;
			castleRook.currentSquare.occupied = false;
			castleRook.currentSquare.currentPiece = null;
			
			#region Transition
			//basically assigns every needed variable
			rookStartPos = castleRook.currentSquare.transform; //sets start and end positions
			rookEndPos = rookDest.transform;
			transRook = castleRook.gameObject; // keeps the piece in memory since its erased from selectedPiece now
			rookStartTime = Time.time; // time when we started moving
			rookDist = Vector3.Distance(rookStartPos.position, rookEndPos.position); //calculates transition distance
																		 // sets info that we can now move the piece
			rookTransitioning = true;
			#endregion
			
			//castleRook.transform.position = rookDest.transform.position;

			rookDest.occupied = true;
			castleRook.currentSquare = rookDest;
			rookDest.currentPiece = castleRook;
		}
	}

	//Return value indicates if piece was actually moved
	public bool MovePiece()
	{
		//If selected piece's on move (or if testing is disabled)
		if (board.whiteOnMove == board.selectedPiece.white || board.disableForcedColorMoves)
		{
			int srcRow = board.selectedPiece.currentSquare.row;

			if (!board.selectedPiece.hasMoved)
			{
				board.selectedPiece.hasMoved = true;
			}

			board.selectedPiece.currentSquare.currentPiece = null;
			board.selectedPiece.currentSquare.occupied = false;

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

			//Check detection, board and pieces rotation
			board.AfterMove();

			this.occupied = true;
			board.selectedPiece.currentSquare = this;
			this.currentPiece = board.selectedPiece;

			//Check if that was a double pawn move
			if((int)board.selectedPiece.type == 5 && Mathf.Abs(this.row - srcRow) == 2)
			{
				int indx = 0;
				Debug.Log(this);
				Debug.Log("----------");
				for(int i = 0; i < 64; i++)
				{
					if(board.children[i] == this.transform)
					{
						indx = i;
					}
				}
				int offs = board.whiteOnMove ? 1 : -1;
				Square e = board.children[indx + (8 * offs)].GetComponent<Square>();
				e.epable = true;
			}

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

	public bool IsSquareAttacked(bool attackingColor) 
	{
		for (int i = (attackingColor ? 0 : 16); i < (attackingColor ? 16 : 32); i++)
		{
			Piece p = board.pieces[i]; //shorthand

			if (p == null) continue;

			List<Square> tempT = p.CalculateLegalMoves(false);
			foreach (Square s in tempT)
			{
				if (s == this)
				{
					return true;
				}
			}
		}

		return false;
	}
}