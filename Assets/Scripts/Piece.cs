using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
	public enum PieceType
	{
		King,
		Queen,
		Bishop,
		Knight,
		Rook,
		Pawn
	}
	//0 - black, 1 - white
	public bool white;
	public bool hasMoved = false;

	public Board board;

	public PieceType type;

	public Square currentSquare;
	[SerializeField] List<Square> illegalSquares = new List<Square>();
	public List<Square> castleMove = new List<Square>();

	private float big = 0f;

	private void Start()
	{
		foreach (Transform sq in board.children)
		{
			//If a piece is close to a square - if it's on the square (checking only the x and y, since z is diffrent)
			if ((new Vector2(sq.position.x, sq.position.y) - new Vector2(this.transform.position.x, this.transform.position.y)).sqrMagnitude < 0.1f)
			{
				currentSquare = sq.gameObject.GetComponent<Square>();
				currentSquare.currentPiece = this;
				continue;
			}
		}
	}

	private void Update()
	{
		if (big > 0f)
		{
			transform.localScale = new Vector2(1.1f, 1.1f);
			big -= Time.deltaTime;
		}
		else
		{
			transform.localScale = new Vector2(1f, 1f);
		}

		// TODO: Move this, this just seems like a janky way to try to fix some problems
		if (!currentSquare.occupied)
		{
			currentSquare.occupied = true;
		}
	}

	private void OnMouseDown()
	{
		//Taking
		if (currentSquare.legalForSelectedPiece)
		{
			if (currentSquare.MovePiece())
			{
				DestroyPiece();
			}
			return;
		}

		board.ClearLegal();

		big = 0.1f;

		// if the piece is already selected, deselect it
		if (board.selectedPiece == this)
		{
			board.selectedPiece = null;
		}
		// if it isn't, select it and calculate legal moves
		else
		{
			board.selectedPiece = this;

			//Actual calculating legal moves
			List<Square> legalSquares = CalculateLegalMoves();

			// pass the legalSquares to board to color instead of doing it yourself
			board.ColorLegal(legalSquares);
			board.ColorSquares(castleMove, Color.blue);
		}
	}

	public void DestroyPiece(bool ep = false)
	{
		//TODO: Add some safeguards, maybe some checks before doing the Destroy
		for(int i = 0; i < board.pieces.Length; i++)
		{
			if(board.pieces[i] == GetComponent<Piece>())
			{
				board.pieces[i] = null;
			}
		}
		//because of en passant the current square not always will be occupied after the move
		if(ep)
		{
			this.currentSquare.occupied = false;
			this.currentSquare.currentPiece = null;
			this.currentSquare = null;
		}
		
		Destroy(this.gameObject);
	}

	private Square IsThisSquareCool(int newRow, int newCol, out bool stuck, bool calcFullKing)
	{
		stuck = true;
		if (newRow > 7 || newCol > 7 || newRow < 0 || newCol < 0)
		{
			return null;
		}

		Square s = board.squares[Mathf.Abs(newRow - 7), newCol];

		if (s.occupied)
		{
			if (s.currentPiece.white != this.white || !calcFullKing)
			{
				return s;
			}
			else
			{
				// we hit a piece we can't take, so we end the loop (can't take any pieces after it either)
				return null;
			}
		}
		else
		{
			stuck = false;
			return s;
		}
	}
	private Square SquareExists(int r, int l)
	{
		try { Square res = board.squares[Mathf.Abs(r - 7), l]; return res; }
		catch { return null; }
	}

	private bool IsCastlingLegal(bool kingside, out Square mvsq)
	{
		mvsq = null;
		if(type != PieceType.King)
		{
			Debug.LogError("Method `IsCastlingLegal` should only be called on king");
		}
		if (hasMoved)
		{
			return false;
		}
		//a is index of rook in the board.pieces; b, c, d are indexes of squares which king goes through 
		int a, b, c, d = 0;

		if (white)
		{
			if (kingside) 
			{
				//White kingside rook
				a = 6;
				b = 61;
				c = 62;
			}
			else 
			{
				a = 7;
				b = 59;
				c = 58;
				d = 57;
			}
		}
		else
		{
			if (kingside) 
			{
				a = 22;
				b = 5;
				c = 6;
			}
			else 
			{
				a = 23;
				b = 3;
				c = 2;
				d = 1;
			}
		}

		Piece rook = board.pieces[a];
		Square p1 = board.children[b].GetComponent<Square>();
		Square p2 = board.children[c].GetComponent<Square>();
		bool temp = true;
		if(d != 0)
		{
			Square p3 = board.children[d].GetComponent<Square>();
			temp = !p3.occupied;
		}
		
		if((!rook.hasMoved &&
				!p1.occupied && !p2.occupied &&
				!p1.IsSquareAttacked(!white) && !p2.IsSquareAttacked(!white) &&
				(!board.check || (board.check && !board.IsChecking(!white))) &&
				temp))
		{
			mvsq = p2;
		}
		return (!rook.hasMoved &&
				!p1.occupied && !p2.occupied &&
				!p1.IsSquareAttacked(!white) && !p2.IsSquareAttacked(!white) &&
				(!board.check || (board.check && !board.IsChecking(!white))) &&
				temp);
	}

	public List<Square> CalculateLegalMoves(bool calcFullKing = true)
	{
		List<Square> temp = new List<Square>();
		List<Square> legalSquares = new List<Square>();

		castleMove.Clear();

		if (currentSquare == null)
		{
			return legalSquares;
		}

		switch (this.type)
		{
			#region King
			case PieceType.King:
				{
					for (int x = -1; x < 2; x++)
					{
						for (int y = -1; y < 2; y++)
						{
							Square s = IsThisSquareCool(currentSquare.row + x, currentSquare.column + y, out _, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
						}
					}

					List<Square> tempT = new List<Square>();

					if (calcFullKing)
					{
						//For all enemy pieces
						for (int i = (!this.white ? 0 : 16); i < (!this.white ? 16 : 32); i++)
						{
							Piece p = board.pieces[i]; //shorthand

							if (p == null) continue;

							tempT = p.CalculateLegalMoves(false);
							foreach (Square s in tempT)
							{
								if (legalSquares.Contains(s))
								{
									legalSquares.Remove(s);
									illegalSquares.Add(s);
								}
							}
						}
						tempT.Clear();


						//Castling
						if (IsCastlingLegal(true, out Square castlemove))
						{
							castleMove.Add(castlemove);
						}
						if (IsCastlingLegal(false, out castlemove))
						{
							castleMove.Add(castlemove);
						}
					}
				}
				break;
			#endregion
			#region Queen
			case PieceType.Queen:
				{
					bool[] directions = new bool[8];
					// the 8 cardinal directions - first four for up, down, left, right, then for the other ones
					// we set them to true if we find something - edge of the board, another figure, so that we don't iterate through them anymore
					for (int i = 1; i < 8; i++)
					{
						if (!directions[0])
						{
							int newRow = currentSquare.row + i;
							int newCol = currentSquare.column;

							Square s = IsThisSquareCool(newRow, newCol, out bool stuck, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
							if (stuck)
							{
								directions[0] = true;
							}
						}
						if (!directions[1])
						{
							int newRow = currentSquare.row;
							int newCol = currentSquare.column + i;

							Square s = IsThisSquareCool(newRow, newCol, out bool stuck, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
							if (stuck)
							{
								directions[1] = true;
							}
						}
						if (!directions[2])
						{
							int newRow = currentSquare.row - i;
							int newCol = currentSquare.column;

							Square s = IsThisSquareCool(newRow, newCol, out bool stuck, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
							if (stuck)
							{
								directions[2] = true;
							}
						}
						if (!directions[3])
						{
							int newRow = currentSquare.row;
							int newCol = currentSquare.column - i;

							Square s = IsThisSquareCool(newRow, newCol, out bool stuck, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
							if (stuck)
							{
								directions[3] = true;
							}
						}
						if (!directions[4])
						{
							int newRow = currentSquare.row + i;
							int newCol = currentSquare.column + i;

							Square s = IsThisSquareCool(newRow, newCol, out bool stuck, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
							if (stuck)
							{
								directions[4] = true;
							}
						}
						if (!directions[5])
						{
							int newRow = currentSquare.row + i;
							int newCol = currentSquare.column - i;

							Square s = IsThisSquareCool(newRow, newCol, out bool stuck, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
							if (stuck)
							{
								directions[5] = true;
							}
						}
						if (!directions[6])
						{
							int newRow = currentSquare.row - i;
							int newCol = currentSquare.column + i;

							Square s = IsThisSquareCool(newRow, newCol, out bool stuck, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
							if (stuck)
							{
								directions[6] = true;
							}
						}
						if (!directions[7])
						{
							int newRow = currentSquare.row - i;
							int newCol = currentSquare.column - i;

							Square s = IsThisSquareCool(newRow, newCol, out bool stuck, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
							if (stuck)
							{
								directions[7] = true;
							}
						}
					}
				}
				break;
			#endregion
			#region Bishop
			case PieceType.Bishop:
				{
					bool[] directions = new bool[4];
					// here we just use the second half of the queen code
					for (int i = 1; i < 8; i++)
					{
						if (!directions[0])
						{
							int newRow = currentSquare.row + i;
							int newCol = currentSquare.column + i;

							Square s = IsThisSquareCool(newRow, newCol, out bool stuck, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
							if (stuck)
							{
								directions[0] = true;
							}
						}
						if (!directions[1])
						{
							int newRow = currentSquare.row + i;
							int newCol = currentSquare.column - i;

							Square s = IsThisSquareCool(newRow, newCol, out bool stuck, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
							if (stuck)
							{
								directions[1] = true;
							}
						}
						if (!directions[2])
						{
							int newRow = currentSquare.row - i;
							int newCol = currentSquare.column + i;

							Square s = IsThisSquareCool(newRow, newCol, out bool stuck, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
							if (stuck)
							{
								directions[2] = true;
							}
						}
						if (!directions[3])
						{
							int newRow = currentSquare.row - i;
							int newCol = currentSquare.column - i;

							Square s = IsThisSquareCool(newRow, newCol, out bool stuck, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
							if (stuck)
							{
								directions[3] = true;
							}
						}
					}
				}
				break;
			#endregion
			#region Knight
			case PieceType.Knight:
				{
					for (int x = -2; x <= 2; x++)
					{
						for (int y = -2; y <= 2; y++)
						{
							if (Mathf.Abs(x) + Mathf.Abs(y) != 3) continue;
							Square s = IsThisSquareCool(currentSquare.row + x, currentSquare.column + y, out _, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
						}
					}
				}
				break;
			#endregion
			#region Rook
			case PieceType.Rook:
				{
					bool[] directions = new bool[4];
					for (int i = 1; i < 8; i++)
					{
						if (!directions[0])
						{
							int newRow = currentSquare.row + i;
							int newCol = currentSquare.column;

							Square s = IsThisSquareCool(newRow, newCol, out bool stuck, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
							if (stuck)
							{
								directions[0] = true;
							}
						}
						if (!directions[1])
						{
							int newRow = currentSquare.row;
							int newCol = currentSquare.column + i;

							Square s = IsThisSquareCool(newRow, newCol, out bool stuck, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
							if (stuck)
							{
								directions[1] = true;
							}
						}
						if (!directions[2])
						{
							int newRow = currentSquare.row - i;
							int newCol = currentSquare.column;

							Square s = IsThisSquareCool(newRow, newCol, out bool stuck, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
							if (stuck)
							{
								directions[2] = true;
							}
						}
						if (!directions[3])
						{
							int newRow = currentSquare.row;
							int newCol = currentSquare.column - i;

							Square s = IsThisSquareCool(newRow, newCol, out bool stuck, calcFullKing);
							if (s)
							{
								legalSquares.Add(s);
							}
							if (stuck)
							{
								directions[3] = true;
							}
						}
					}
				}
				break;
			#endregion
			#region Pawn
			case PieceType.Pawn:
				{
					int offset = white ? 1 : -1;
					int nR = currentSquare.row + (1 * offset);
					int nC = currentSquare.column;
					//Move forward
					Square s = SquareExists(nR, nC);
					if (s && !s.occupied && calcFullKing)
					{
						legalSquares.Add(s);
						//Double first move
						nR += 1 * offset;
						if (!hasMoved && !SquareExists(nR, nC).occupied)
						{
							legalSquares.Add(SquareExists(nR, nC));
						}
					}
					else
					{
						nR += 1 * offset;
					}
					//Take
					nR -= 1 * offset;
					nC -= 1 * offset;
					s = SquareExists(nR, nC);
					if (s && (((s.occupied && s.currentPiece.white != white) || (s.epable && s.epPawn.white != white)) || !calcFullKing))
					{
						legalSquares.Add(s);
					}
					nC += 2 * offset;
					s = SquareExists(nR, nC);
					if (s && (((s.occupied && s.currentPiece.white != white) || (s.epable && s.epPawn.white != white)) || !calcFullKing))
					{
						legalSquares.Add(s);
					}
				}
				break;
				#endregion
		}
		foreach (Square sq in temp)
		{
			legalSquares.Add(sq);
		}

		temp.Clear();

		//Simulating moves to check if they break the check (therefore, if they're legal when the king is checked)
		if (calcFullKing)
		{
			foreach (Square newSquare in legalSquares)
			{
				//temporary variables to not lose any data
				Square originalSquare = this.currentSquare;
				bool newSquareOriginallyOccupied = newSquare.occupied;
				Piece originalPieceOnNewSquare = newSquare.currentPiece;

				originalSquare.occupied = false;
				originalSquare.currentPiece = null;
				if (originalPieceOnNewSquare != null)
				{
					originalPieceOnNewSquare.currentSquare = null;
				}
				newSquare.currentPiece = this;
				newSquare.occupied = true;
				this.currentSquare = newSquare;

				if (!board.IsChecking(!board.whiteOnMove))
				{
					temp.Add(newSquare);
				}

				if (originalPieceOnNewSquare != null)
				{
					originalPieceOnNewSquare.currentSquare = newSquare;
				}
				originalSquare.occupied = true;
				originalSquare.currentPiece = this;

				newSquare.currentPiece = originalPieceOnNewSquare;
				newSquare.occupied = newSquareOriginallyOccupied;
				this.currentSquare = originalSquare;

			}
			legalSquares.Clear();
		}

		foreach (Square sq in temp)
		{
			legalSquares.Add(sq);
		}
		temp.Clear();

		return legalSquares;
	}
}