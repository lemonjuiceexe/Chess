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
    public List<Square> legalSquares = new List<Square>();
    List<Square> temp = new List<Square>();
    [SerializeField]
    List<Square> illegalSquares = new List<Square>();

    private float big = 0f;

    private void Start()
    {
        foreach(Transform sq in board.children)
        {
            //If a piece is close to a square - if it's on the square (checking only the x and y, since z is diffrent)
            if((new Vector2(sq.position.x, sq.position.y) - new Vector2(this.transform.position.x, this.transform.position.y)).sqrMagnitude < 0.1f)
            {
                currentSquare = sq.gameObject.GetComponent<Square>();
                currentSquare.currentPiece = this;
                continue;
            }
        }
    }

    private void Update()
    {
        if(big > 0f)
        {
            gameObject.transform.localScale = new Vector2(1.1f, 1.1f);
            big -= Time.deltaTime;
        } 
        else
        {
            gameObject.transform.localScale = new Vector2(1f, 1f);
        }
        currentSquare.occupied = true;

        //If the move just happened
        if(board.whiteOnMove != board.lastFrameWhiteOnMove && !board.disableTurnBoard)
        {
            this.transform.Rotate(0, 0, 180);
        }
    }

    private void OnMouseDown()
    {
        //Taking
        if(currentSquare.legalForSelectedPiece)
        {
            if (currentSquare.MovePiece())
            {
                Destroy(this.gameObject);
            }
            
            return;
        }

        ClearLegal();

        big = 0.1f;

        if (board.selectedPiece == this)
        {
            board.selectedPiece = null;
        }
        else
        {
            board.selectedPiece = this;

            //Actual calculating legal moves
            legalSquares = CalculateLegalMoves(this);

            // pass the legalSquares for board to color instead of doing it yourself
            board.ColorLegal(legalSquares);
        }
    }

    public void ClearLegal()
    {
        legalSquares.Clear();
        temp.Clear();
        board.ClearLegal();
    }

    private List<Square> CalculateLegalMoves(Piece piece)
    {
        switch (piece.type)
        {
            #region King
            case PieceType.King:
                //For all squares
                foreach (Transform sq in board.children)
                {
                    //This magnitude gives every square around (standard king's move)
                    if ((sq.position - piece.transform.position).sqrMagnitude <= 250f)
                    {
                        if (sq.GetComponent<Square>().occupied)
                        {
                            if (sq.GetComponent<Square>().currentPiece.white != piece.white)
                            {
                                piece.legalSquares.Add(sq.gameObject.GetComponent<Square>());
                            }

                            continue;
                        }

                        piece.legalSquares.Add(sq.gameObject.GetComponent<Square>());
                    }
                }

                //TODO: King will be able to take a piece which is defended, as defended piece is not a legal move for the defending one
                //for (int i = (!piece.white ? 0 : 16); i < (!piece.white ? 16 : 32); i++)
                //{
                //    Piece p = board.pieces[i]; //shorthand
                //    foreach (Square iS in p.legalSquares)
                //    {
                //        foreach (Square lS in piece.legalSquares)
                //        {
                //            if (iS == lS)
                //            {
                //                piece.legalSquares.Remove(lS);
                //            }
                //        }
                //    }
                //}

                break;
            #endregion
            #region Queen
            case PieceType.Queen:
                foreach (Transform sq in board.children)
                {
                    //This magnitude gives squares top-left, top-right, bottom-left and bottom-right (relative to current)
                    if ((sq.position - piece.transform.position).sqrMagnitude > 150f && (sq.position - piece.transform.position).sqrMagnitude <= 250f)
                    {
                        if (sq.GetComponent<Square>().occupied)
                        {
                            if (sq.GetComponent<Square>().currentPiece.white != piece.white)
                            {
                                piece.temp.Add(sq.GetComponent<Square>());
                            }

                            continue;
                        }
                        piece.legalSquares.Add(sq.gameObject.GetComponent<Square>());
                    }
                }

                foreach (Square sq in legalSquares)
                {
                    //Four possibilities
                    //indicating squares works properly
                    if (sq.column == piece.currentSquare.column + 1 && sq.row == piece.currentSquare.row + 1)
                    {
                        //Top right
                        if (sq.column != 7 && sq.row != 7)
                        {
                            int i = 1;
                            while (sq.column + i < 8 && sq.row + i < 8)
                            {
                                if (board.squares[Mathf.Abs(sq.row - 7) - i, sq.column + i].occupied)
                                {
                                    if (board.squares[Mathf.Abs(sq.row - 7) - i, sq.column + i].currentPiece.white != piece.white)
                                    {
                                        piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) - i, sq.column + i]);
                                    }

                                    break;
                                }
                                piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) - i, sq.column + i]);
                                i++;
                            }
                        }
                    }
                    else if (sq.column == piece.currentSquare.column + 1 && sq.row == piece.currentSquare.row - 1)
                    {
                        //Bottom right
                        if (sq.column != 7 && sq.row != 0)
                        {
                            int i = 1;
                            while (sq.column + i < 8 && sq.row - i >= 0)
                            {
                                if (board.squares[Mathf.Abs(sq.row - 7) + i, sq.column + i].occupied)
                                {
                                    if (board.squares[Mathf.Abs(sq.row - 7) + i, sq.column + i].currentPiece.white != piece.white)
                                    {
                                        piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) + i, sq.column + i]);
                                    }

                                    break;
                                }
                                piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) + i, sq.column + i]);
                                i++;
                            }
                        }
                    }
                    else if (sq.column == piece.currentSquare.column - 1 && sq.row == piece.currentSquare.row - 1)
                    {
                        //Bottom left
                        if (sq.column != 0 && sq.row != 0)
                        {
                            int i = 1;
                            while (sq.column - i >= 0 && sq.row - i >= 0)
                            {
                                if (board.squares[Mathf.Abs(sq.row - 7) + i, sq.column - i].occupied)
                                {
                                    if (board.squares[Mathf.Abs(sq.row - 7) + i, sq.column - i].currentPiece.white != piece.white)
                                    {
                                        piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) + i, sq.column - i]);
                                    }

                                    break;
                                }
                                piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) + i, sq.column - i]);
                                i++;
                            }
                        }
                    }
                    else if (sq.column == piece.currentSquare.column - 1 && sq.row == piece.currentSquare.row + 1)
                    {
                        //Top left
                        if (sq.column != 0 && sq.row != 7)
                        {
                            int i = 1;
                            while (sq.column - i >= 0 && sq.row + i < 8)
                            {
                                if (board.squares[Mathf.Abs(sq.row - 7) - i, sq.column - i].occupied)
                                {
                                    if (board.squares[Mathf.Abs(sq.row - 7) - i, sq.column - i].currentPiece.white != piece.white)
                                    {
                                        piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) - i, sq.column - i]);
                                    }

                                    break;
                                }
                                piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) - i, sq.column - i]);
                                i++;
                            }
                        }
                    }
                }

                piece.temp.AddRange(legalSquares);
                piece.legalSquares.Clear();

                foreach (Transform sq in board.children)
                {
                    //This magnitude gives squares top, bottom, left and right (relative to current)
                    if ((sq.position - piece.transform.position).sqrMagnitude <= 150f)
                    {
                        if (sq.GetComponent<Square>().occupied)
                        {
                            if (sq.GetComponent<Square>().currentPiece.white != piece.white)
                            {
                                piece.temp.Add(sq.GetComponent<Square>());
                            }

                            continue;
                        }
                        piece.legalSquares.Add(sq.gameObject.GetComponent<Square>());
                    }
                }

                foreach (Square sq in legalSquares)
                {
                    //If legal move is on the edge, it's the last in piece direction anyway, so sort it out here
                    //Four possibilities: piece square is top, left, bottom or right relatively to here
                    if (sq.row == piece.currentSquare.row)
                    {
                        if (sq.column > piece.currentSquare.column && sq.column != 7)
                        {
                            //Case right
                            int i = 1;
                            //Math.Abs is translation from chess notation (squares being counted from bottom-left) to normal notation (from top-left)
                            //Basically while is iterating through legal moves (while condition indicates legal)
                            //Ifs are checking if the sq is occupied, then if its diffrent color than current piece
                            // if yes, then we add it to legal then break
                            // if no, then we just break
                            while (sq.column + i < 8)
                            {
                                if (board.squares[Mathf.Abs(sq.row - 7), sq.column + i].occupied)
                                {
                                    if (board.squares[Mathf.Abs(sq.row - 7), sq.column + i].currentPiece.white != piece.white)
                                    {
                                        piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7), sq.column + i]);
                                    }

                                    break;
                                }
                                piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7), sq.column + i]);
                                i++;
                            }
                        }
                        else if (sq.column != 0)
                        {
                            //Case left
                            int i = -1;
                            while (sq.column + i >= 0)
                            {
                                if (board.squares[Mathf.Abs(sq.row - 7), sq.column + i].occupied)
                                {
                                    if (board.squares[Mathf.Abs(sq.row - 7), sq.column + i].currentPiece.white != piece.white)
                                    {
                                        piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7), sq.column + i]);
                                    }
                                    break;
                                }
                                piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7), sq.column + i]);
                                i--;
                            }
                        }
                    }
                    else
                    {
                        if (sq.row > piece.currentSquare.row && sq.row != 7)
                        {
                            //Case up
                            int i = 1;
                            while (sq.row + i < 8)
                            {
                                if (board.squares[Mathf.Abs(sq.row - 7) - i, sq.column].occupied)
                                {
                                    if (board.squares[Mathf.Abs(sq.row - 7) - i, sq.column].currentPiece.white != piece.white)
                                    {
                                        piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) - i, sq.column]);
                                    }
                                    break;
                                }
                                piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) - i, sq.column]);
                                i++;
                            }
                        }
                        else if (sq.row != 0)
                        {
                            //Case bottom
                            int i = -1;
                            while (sq.row + i >= 0)
                            {
                                if (board.squares[Mathf.Abs(sq.row - 7) - i, sq.column].occupied)
                                {
                                    if (board.squares[Mathf.Abs(sq.row - 7) - i, sq.column].currentPiece.white != piece.white)
                                    {
                                        piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) - i, sq.column]);
                                    }
                                    break;
                                }
                                piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) - i, sq.column]);
                                i--;
                            }
                        }
                    }
                }
                break;
            #endregion
            #region Bishop
            case PieceType.Bishop:
                foreach (Transform sq in board.children)
                {
                    //This magnitude gives squares top-left, top-right, bottom-left and bottom-right (relative to current)
                    if ((sq.position - piece.transform.position).sqrMagnitude > 150f && (sq.position - piece.transform.position).sqrMagnitude <= 250f)
                    {
                        if (sq.GetComponent<Square>().occupied)
                        {
                            if (sq.GetComponent<Square>().currentPiece.white != piece.white)
                            {
                                piece.temp.Add(sq.GetComponent<Square>());
                            }

                            continue;
                        }
                        piece.legalSquares.Add(sq.gameObject.GetComponent<Square>());
                    }
                }

                foreach (Square sq in legalSquares)
                {
                    //Four possibilities
                    //indicating squares works properly
                    if (sq.column == piece.currentSquare.column + 1 && sq.row == piece.currentSquare.row + 1)
                    {
                        //Top right
                        if (sq.column != 7 && sq.row != 7)
                        {
                            int i = 1;
                            while (sq.column + i < 8 && sq.row + i < 8)
                            {
                                if (board.squares[Mathf.Abs(sq.row - 7) - i, sq.column + i].occupied)
                                {
                                    if (board.squares[Mathf.Abs(sq.row - 7) - i, sq.column + i].currentPiece.white != piece.white)
                                    {
                                        piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) - i, sq.column + i]);
                                    }

                                    break;
                                }
                                piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) - i, sq.column + i]);
                                i++;
                            }
                        }
                    }
                    else if (sq.column == piece.currentSquare.column + 1 && sq.row == piece.currentSquare.row - 1)
                    {
                        //Bottom right
                        if (sq.column != 7 && sq.row != 0)
                        {
                            int i = 1;
                            while (sq.column + i < 8 && sq.row - i >= 0)
                            {
                                if (board.squares[Mathf.Abs(sq.row - 7) + i, sq.column + i].occupied)
                                {
                                    if (board.squares[Mathf.Abs(sq.row - 7) + i, sq.column + i].currentPiece.white != piece.white)
                                    {
                                        piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) + i, sq.column + i]);
                                    }

                                    break;
                                }
                                piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) + i, sq.column + i]);
                                i++;
                            }
                        }
                    }
                    else if (sq.column == piece.currentSquare.column - 1 && sq.row == piece.currentSquare.row - 1)
                    {
                        //Bottom left
                        if (sq.column != 0 && sq.row != 0)
                        {
                            int i = 1;
                            while (sq.column - i >= 0 && sq.row - i >= 0)
                            {
                                if (board.squares[Mathf.Abs(sq.row - 7) + i, sq.column - i].occupied)
                                {
                                    if (board.squares[Mathf.Abs(sq.row - 7) + i, sq.column - i].currentPiece.white != piece.white)
                                    {
                                        piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) + i, sq.column - i]);
                                    }

                                    break;
                                }
                                piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) + i, sq.column - i]);
                                i++;
                            }
                        }
                    }
                    else if (sq.column == piece.currentSquare.column - 1 && sq.row == piece.currentSquare.row + 1)
                    {
                        //Top left
                        if (sq.column != 0 && sq.row != 7)
                        {
                            int i = 1;
                            while (sq.column - i >= 0 && sq.row + i < 8)
                            {
                                if (board.squares[Mathf.Abs(sq.row - 7) - i, sq.column - i].occupied)
                                {
                                    if (board.squares[Mathf.Abs(sq.row - 7) - i, sq.column - i].currentPiece.white != piece.white)
                                    {
                                        piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) - i, sq.column - i]);
                                    }

                                    break;
                                }
                                piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) - i, sq.column - i]);
                                i++;
                            }
                        }
                    }
                }
                break;
            #endregion
            #region Knight
            case PieceType.Knight:

                foreach (Transform s in board.children)
                {
                    Square sq = s.gameObject.GetComponent<Square>();

                    if (piece.currentSquare.row + 2 < 8 && sq.row == piece.currentSquare.row + 2)
                    {
                        if (sq.column == piece.currentSquare.column - 1 || sq.column == piece.currentSquare.column + 1)
                        {
                            if (sq.GetComponent<Square>().occupied)
                            {
                                if (sq.GetComponent<Square>().currentPiece.white != piece.white)
                                {
                                    piece.temp.Add(sq.gameObject.GetComponent<Square>());
                                }

                                continue;
                            }

                            piece.temp.Add(sq);
                        }
                    }
                    else if (piece.currentSquare.column + 2 < 8 && sq.column == piece.currentSquare.column + 2)
                    {
                        if (sq.row == piece.currentSquare.row + 1 || sq.row == piece.currentSquare.row - 1)
                        {
                            if (sq.GetComponent<Square>().occupied)
                            {
                                if (sq.GetComponent<Square>().currentPiece.white != piece.white)
                                {
                                    piece.temp.Add(sq.gameObject.GetComponent<Square>());
                                }

                                continue;
                            }

                            piece.temp.Add(sq);
                        }
                    }
                    else if (piece.currentSquare.row - 2 >= 0 && sq.row == piece.currentSquare.row - 2)
                    {
                        if (sq.column == piece.currentSquare.column + 1 || sq.column == piece.currentSquare.column - 1)
                        {
                            if (sq.GetComponent<Square>().occupied)
                            {
                                if (sq.GetComponent<Square>().currentPiece.white != piece.white)
                                {
                                    piece.temp.Add(sq.gameObject.GetComponent<Square>());
                                }

                                continue;
                            }

                            piece.temp.Add(sq);
                        }
                    }
                    else if (piece.currentSquare.column - 2 >= 0 && sq.column == piece.currentSquare.column - 2)
                    {
                        if (sq.row == piece.currentSquare.row + 1 || sq.row == piece.currentSquare.row - 1)
                        {
                            if (sq.GetComponent<Square>().occupied)
                            {
                                if (sq.GetComponent<Square>().currentPiece.white != piece.white)
                                {
                                    piece.temp.Add(sq.gameObject.GetComponent<Square>());
                                }

                                continue;
                            }

                            piece.temp.Add(sq);
                        }
                    }
                }
                break;
            #endregion
            #region Rook
            case PieceType.Rook:
                foreach (Transform sq in board.children)
                {
                    //This magnitude gives squares top, bottom, left and right (relative to current)
                    if ((sq.position - piece.transform.position).sqrMagnitude <= 150f)
                    {
                        if (sq.GetComponent<Square>().occupied)
                        {
                            if (sq.GetComponent<Square>().currentPiece.white != piece.white)
                            {
                                piece.temp.Add(sq.GetComponent<Square>());
                            }

                            continue;
                        }
                        piece.legalSquares.Add(sq.gameObject.GetComponent<Square>());
                    }
                }

                foreach (Square sq in legalSquares)
                {
                    //If legal move is on the edge, it's the last in piece direction anyway, so sort it out here
                    //Four possibilities: piece square is top, left, bottom or right relatively to here
                    if (sq.row == piece.currentSquare.row)
                    {
                        if (sq.column > piece.currentSquare.column && sq.column != 7)
                        {
                            //Case right
                            int i = 1;
                            //Math.Abs is translation from chess notation (squares being counted from bottom-left) to normal notation (from top-left)
                            //Basically while is iterating through legal moves (while condition indicates legal)
                            //Ifs are checking if the sq is occupied, then if its diffrent color than current piece
                            // if yes, then we add it to legal then break
                            // if no, then we just break
                            while (sq.column + i < 8)
                            {
                                if (board.squares[Mathf.Abs(sq.row - 7), sq.column + i].occupied)
                                {
                                    if (board.squares[Mathf.Abs(sq.row - 7), sq.column + i].currentPiece.white != piece.white)
                                    {
                                        piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7), sq.column + i]);
                                    }

                                    break;
                                }
                                piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7), sq.column + i]);
                                i++;
                            }
                        }
                        else if (sq.column != 0)
                        {
                            //Case left
                            int i = -1;
                            while (sq.column + i >= 0)
                            {
                                if (board.squares[Mathf.Abs(sq.row - 7), sq.column + i].occupied)
                                {
                                    if (board.squares[Mathf.Abs(sq.row - 7), sq.column + i].currentPiece.white != piece.white)
                                    {
                                        piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7), sq.column + i]);
                                    }
                                    break;
                                }
                                piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7), sq.column + i]);
                                i--;
                            }
                        }
                    }
                    else
                    {
                        if (sq.row > piece.currentSquare.row && sq.row != 7)
                        {
                            //Case up
                            int i = 1;
                            while (sq.row + i < 8)
                            {
                                if (board.squares[Mathf.Abs(sq.row - 7) - i, sq.column].occupied)
                                {
                                    if (board.squares[Mathf.Abs(sq.row - 7) - i, sq.column].currentPiece.white != piece.white)
                                    {
                                        piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) - i, sq.column]);
                                    }
                                    break;
                                }
                                piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) - i, sq.column]);
                                i++;
                            }
                        }
                        else if (sq.row != 0)
                        {
                            //Case bottom
                            int i = -1;
                            while (sq.row + i >= 0)
                            {
                                if (board.squares[Mathf.Abs(sq.row - 7) - i, sq.column].occupied)
                                {
                                    if (board.squares[Mathf.Abs(sq.row - 7) - i, sq.column].currentPiece.white != piece.white)
                                    {
                                        piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) - i, sq.column]);
                                    }
                                    break;
                                }
                                piece.temp.Add(board.squares[Mathf.Abs(sq.row - 7) - i, sq.column]);
                                i--;
                            }
                        }
                    }
                }

                break;
            #endregion
            #region Pawn
            case PieceType.Pawn:
                int j = 0;
                foreach (Transform sq in board.children)
                {
                    if (piece.white)
                    {
                        //If first move (you can move 2 squares)
                        //If pawn is in second row (can double move), second square !occupied, it's actually the square 2 ahead,                                it's in the same column                                         the square 1 ahead is not occupied
                        //Overall: can move two squares ahead
                        if (piece.currentSquare.row == 1 && !sq.GetComponent<Square>().occupied && sq.GetComponent<Square>().row == piece.currentSquare.row + 2 && sq.GetComponent<Square>().column == piece.currentSquare.column && !board.children[j + 8].GetComponent<Square>().occupied)
                        {
                            piece.legalSquares.Add(sq.GetComponent<Square>());
                        }
                        //Square's one ahead,                                               it's in the same column,                                        is not occupied
                        if (sq.GetComponent<Square>().row == piece.currentSquare.row + 1 && sq.GetComponent<Square>().column == piece.currentSquare.column && !sq.GetComponent<Square>().occupied)
                        {
                            piece.legalSquares.Add(sq.GetComponent<Square>());
                        }

                        //Taking pieces
                        if (sq.GetComponent<Square>().row == piece.currentSquare.row + 1 && (sq.GetComponent<Square>().column == piece.currentSquare.column + 1 || sq.GetComponent<Square>().column == piece.currentSquare.column - 1))
                        {
                            if (sq.GetComponent<Square>().occupied && sq.GetComponent<Square>().currentPiece.white != piece.white)
                            {
                                piece.legalSquares.Add(sq.GetComponent<Square>());
                            }
                        }
                    }
                    else
                    {
                        //Look above basically
                        if (piece.currentSquare.row == 6 && !sq.GetComponent<Square>().occupied && sq.GetComponent<Square>().row == piece.currentSquare.row - 2 && sq.GetComponent<Square>().column == piece.currentSquare.column && !board.children[j - 8].GetComponent<Square>().occupied)
                        {
                            piece.legalSquares.Add(sq.GetComponent<Square>());
                        }
                        if (sq.GetComponent<Square>().row == piece.currentSquare.row - 1 && sq.GetComponent<Square>().column == piece.currentSquare.column && !sq.GetComponent<Square>().occupied)
                        {
                            piece.legalSquares.Add(sq.GetComponent<Square>());
                        }
                        if (sq.GetComponent<Square>().row == piece.currentSquare.row - 1 && (sq.GetComponent<Square>().column == piece.currentSquare.column + 1 || sq.GetComponent<Square>().column == piece.currentSquare.column - 1))
                        {
                            if (sq.GetComponent<Square>().occupied && sq.GetComponent<Square>().currentPiece.white != piece.white)
                            {
                                piece.legalSquares.Add(sq.GetComponent<Square>());
                            }
                        }
                    }

                    j++;
                }

                break;
                #endregion
        }

        foreach (Square sq in temp)
        {
            legalSquares.Add(sq);
        }

        return legalSquares;
    }
}