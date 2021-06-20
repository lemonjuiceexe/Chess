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

	//bool selected = false;
	public Square currentSquare;
	[SerializeField] List<Square> illegalSquares = new List<Square>();

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

		ClearLegal();

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

			// pass the legalSquares for board to color instead of doing it yourself
			board.ColorLegal(legalSquares);
		}
	}

	public void ClearLegal()
	{
		board.ClearLegal();
	}

	public void DestroyPiece()
	{
		//TODO: Add some safeguards, maybe some checks before doing the Destroy
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

	public List<Square> CalculateLegalMoves(bool calcFullKing = true)
	{
		List<Square> temp = new List<Square>();
		List<Square> legalSquares = new List<Square>();

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
									if (board.selectedPiece == this)
									{
										//s.gameObject.GetComponent<SpriteRenderer>().color = board.illegalKingMoveColor;
									}
									legalSquares.Remove(s);
									illegalSquares.Add(s);
								}
							}
						}
						tempT.Clear();
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
                    if (s && !s.occupied)
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
					if (s && ((s.occupied && s.currentPiece.white != white) || !calcFullKing))
                    {
                        legalSquares.Add(s);
                    }
					nC += 2 * offset;
					s = SquareExists(nR, nC);
					if (s && ((s.occupied && s.currentPiece.white != white) || !calcFullKing))
					{
                        legalSquares.Add(s);
					}

					//            Square sq = IsThisSquareCool(nR, nC, out bool st, calcFullKing);
					////sq.GetComponent<SpriteRenderer>().color = Color.magenta;
					//            if (sq)
					//            {
					//	legalSquares.Add(sq);
					//            }
					////Double first move
					//nR += (1 * offset);
					//if (!hasMoved && !st)
					//            {
					//	Square d = IsThisSquareCool(nR, nC, out _, calcFullKing);
					//                if (d)
					//                {
					//		legalSquares.Add(d);
					//                }
					//            }
					////Taking
					//nR -= (1 * offset);
					//nC -= (1 * offset);
					//Square t = IsThisSquareCool(nR, nC, out _, calcFullKing);
					//            if (t && ((t.occupied && t.currentPiece.white != white) || !calcFullKing))
					//            {
					//	legalSquares.Add(t);
					//            }
					//nC += (2 * offset);
					//Square l = IsThisSquareCool(nR, nC, out _, calcFullKing);
					//if (l && ((l.occupied && l.currentPiece.white != white) || !calcFullKing))
					//{
					//	legalSquares.Add(l);
					//}


					/*
					int j = 0;
					foreach (Transform sq in board.children)
					{
						Square s = sq.GetComponent<Square>();
						if (this.white)
						{
							//If first move (you can move 2 squares)
							//If pawn is in second row (can double move), second square !occupied, it's actually the square 2 ahead,                                it's in the same column                                         the square 1 ahead is not occupied
							//Overall: can move two squares ahead
							if (calcFullKing) // also check if we arent calling it from enemy king, in which case dont calc moves directly ahead
							{
								if (this.currentSquare.row == 1 && !s.occupied && s.row == this.currentSquare.row + 2 && s.column == this.currentSquare.column && !board.children[j + 8].GetComponent<Square>().occupied)
								{
									legalSquares.Add(s);
								}
								//Square's one ahead,                                               it's in the same column,                                        is not occupied
								if (s.row == this.currentSquare.row + 1 && s.column == this.currentSquare.column && !s.occupied)
								{
									legalSquares.Add(s);
								}
							}

							//Taking pieces
							if (s.row == this.currentSquare.row + 1 && (s.column == this.currentSquare.column + 1 || s.column == this.currentSquare.column - 1))
							{
								if (s.occupied && s.currentPiece.white != this.white || !calcFullKing)
								{
									legalSquares.Add(s);
								}
							}
						}
						else
						{
							if (calcFullKing) // also check if we arent calling it from enemy king, in which case dont calc moves directly ahead
							{
								//Look above basically
								if (this.currentSquare.row == 6 && !s.occupied && s.row == this.currentSquare.row - 2 && s.column == this.currentSquare.column && !board.children[j - 8].GetComponent<Square>().occupied)
								{
									legalSquares.Add(s);
								}
								if (s.row == this.currentSquare.row - 1 && s.column == this.currentSquare.column && !s.occupied)
								{
									legalSquares.Add(s);
								}
							}
							if (s.row == this.currentSquare.row - 1 && (s.column == this.currentSquare.column + 1 || s.column == this.currentSquare.column - 1))
							{
								if (s.occupied && s.currentPiece.white != this.white || !calcFullKing)
								{
									legalSquares.Add(s);
								}
							}
						}

						j++;
					}
					*/
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
		if (board.check && calcFullKing)
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