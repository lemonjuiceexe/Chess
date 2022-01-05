using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
	//2D array of squares, actually representing a board (8x8)
	public Square[,] squares = new Square[8, 8];
	[Header("Pieces and squares arrays")]
	public Transform[] children;
	public Piece[] pieces;

	[Header("Gameobjects references")]
	public Camera cam;

	[Header("Game management")]
	public Piece selectedPiece;
	public bool whiteOnMove = true;
	public float transitionSpeed;
	public bool check = false;

	[Header("Colours")]
	public Color legalColor;
	public Color legalTakeColor;
	public Color illegalKingMoveColor;

	//debug stuff
	[Header("Debug variables")]
	public bool disableForcedColorMoves;
	public bool disableTurnBoard;

	private void Start()
	{
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				squares[i, j] = children[((i) * 8) + j].GetComponent<Square>();
			}
		}

		disableForcedColorMoves = PlayerPrefs.GetInt("disableForcedColorMoves", 0) == 1;
		disableTurnBoard = PlayerPrefs.GetInt("disableBoardFlip", 0) == 1;
	}

	public void AfterMove()
	{
		check = IsChecking(!whiteOnMove);
		
		if (!disableTurnBoard)
		{
			cam.transform.Rotate(0, 0, 180);
			foreach (Piece p in pieces)
			{
				try
				{
					p.transform.Rotate(0, 0, 180);
				}
				catch { continue; }
			}
		}

		foreach (Transform ob in children)
		{
			Square s = ob.GetComponent<Square>();
			s.epCountdown += (s.epable ? 1 : 0) * (s.epCountdown < 2 ? 1 : 0);

			if (s.epCountdown >= 2)
			{
				s.epCountdown = 0;
				s.epable = false;
				s.epPawn = null;
			}
		}
	}

	public bool IsChecking(bool color)
	{
		List<Square> ls = new List<Square>();
		//For all pieces of the same color as color
		for (int i = (color ? 0 : 16); i < (color ? 16 : 32); i++)
		{
			if (pieces[i] == null) { continue; }
			List<Square> tp = pieces[i].CalculateLegalMoves(false);

			foreach (Square s in tp)
			{
				if (!ls.Contains(s))
				{
					ls.Add(s);
				}
			}
		}

		if (ls.Contains(pieces[color ? 16 : 0].currentSquare))
		{
			return true;
		}

		return false;
	}

	public void ColorLegal(List<Square> legalSquares)
	{
		foreach (Square sq in legalSquares)
		{
			sq.legalForSelectedPiece = true;
			if (sq.occupied || (sq.epable && (int)selectedPiece.type == 5))
			{
				sq.GetComponent<SpriteRenderer>().color = legalTakeColor;
			}
			else
			{
				sq.GetComponent<SpriteRenderer>().color = legalColor;
			}

		}
	}
	public void ColorSquares(List<Square> squares, Color c)
	{
		foreach (Square sq in squares)
		{
			sq.GetComponent<SpriteRenderer>().color = c;
		}
	}
	public void ClearLegal()
	{
		foreach (Square sq in squares)
		{
			sq.legalForSelectedPiece = false;

			sq.GetComponent<SpriteRenderer>().color = Color.white;
		}
	}
}