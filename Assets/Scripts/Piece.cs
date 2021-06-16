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

        // TODO: Move this, this just seems like a janky way to try to fix some problems
        if (!currentSquare.occupied)
        {
            currentSquare.occupied = true;
        }

        // TODO: Move this too, also seems like a bad way
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

    private Square isThisSquareCool(int newRow, int newCol, out bool stuck, bool calcFullKing)
    {
        stuck = true;
        if (newRow > 7 || newCol > 7 || newRow < 0 || newCol < 0)
        {
            // TODO: Can't find an elegant way to incorporate this into the while loop!
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

    public List<Square> CalculateLegalMoves(bool calcFullKing = true)
    {
        List<Square> temp = new List<Square>();
        List<Square> legalSquares = new List<Square>();

        if(currentSquare == null)
        {
            return legalSquares;
        }
        
        switch (this.type)
        {
            #region King
            case PieceType.King:
                //For all squares
                foreach (Square sq in board.squares)
                {
                    // An ugly way, but gets every square around the piece
                    if ((sq.row == currentSquare.row + 1 && sq.column == currentSquare.column - 1) ||
                        (sq.row == currentSquare.row - 1 && sq.column == currentSquare.column - 1) ||
                        (sq.row == currentSquare.row - 1 && sq.column == currentSquare.column + 1) ||
                        (sq.row == currentSquare.row + 1 && sq.column == currentSquare.column + 1) ||
                        (sq.row == currentSquare.row     && sq.column == currentSquare.column - 1) ||
                        (sq.row == currentSquare.row - 1 && sq.column == currentSquare.column)     ||
                        (sq.row == currentSquare.row     && sq.column == currentSquare.column + 1) ||
                        (sq.row == currentSquare.row + 1 && sq.column == currentSquare.column))
                    {
                        if (sq.occupied)
                        {
                            if (sq.currentPiece.white != this.white || !calcFullKing)
                            {
                                temp.Add(sq);
                            }
                            else
                            {
                                continue;
                            }
                        }

                        temp.Add(sq);
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
                        foreach(Square s in tempT)
                        {
                            if (temp.Contains(s))
                            {
                                if(board.selectedPiece == this)
                                {
                                    //s.gameObject.GetComponent<SpriteRenderer>().color = board.illegalKingMoveColor;
                                }
                                temp.Remove(s);
                                illegalSquares.Add(s);
                            }
                        }
                    }
                    tempT.Clear();
                }

                legalSquares.Clear();

                break;
            #endregion
            #region Queen
            case PieceType.Queen:

                // The bishop 
                int newRow = currentSquare.row;
                int newCol = currentSquare.column;
                while (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
                {
                    newRow++;
                    newCol++;

                    bool stuck;
                    Square s = isThisSquareCool(newRow, newCol, out stuck, calcFullKing);
                    if (s) 
                        legalSquares.Add(s);
                    else
                        break;
                    if (stuck)
                        break;
                }

                newRow = currentSquare.row;
                newCol = currentSquare.column;
                while (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
                {
                    newRow--;
                    newCol++;

                    bool stuck;
                    Square s = isThisSquareCool(newRow, newCol, out stuck, calcFullKing);
                    if (s)
                        legalSquares.Add(s);
                    else
                        break;
                    if (stuck)
                        break;
                }

                newRow = currentSquare.row;
                newCol = currentSquare.column;
                while (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
                {
                    newRow++;
                    newCol--;

                    bool stuck;
                    Square s = isThisSquareCool(newRow, newCol, out stuck, calcFullKing);
                    if (s)
                        legalSquares.Add(s);
                    else
                        break;
                    if (stuck)
                        break;
                }

                newRow = currentSquare.row;
                newCol = currentSquare.column;
                while (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
                {
                    newRow--;
                    newCol--;

                    bool stuck;
                    Square s = isThisSquareCool(newRow, newCol, out stuck, calcFullKing);
                    if (s)
                        legalSquares.Add(s);
                    else
                        break;
                    if (stuck)
                        break;
                }

                // The rook
                /*foreach (Transform sq in board.children)
                {
                    Square s = sq.GetComponent<Square>();
                    if ((s.row == currentSquare.row     && s.column == currentSquare.column - 1) ||
                        (s.row == currentSquare.row - 1 && s.column == currentSquare.column    ) ||
                        (s.row == currentSquare.row     && s.column == currentSquare.column + 1) ||
                        (s.row == currentSquare.row + 1 && s.column == currentSquare.column))
                    {
                        if (s.occupied)
                        {
                            if (s.currentPiece.white != this.white || !calcFullKing)
                            {
                                temp.Add(s);
                            }

                            continue;
                        }
                        legalSquares.Add(s);
                    }
                }

                foreach (Square sq in legalSquares)
                {
                    //If legal move is on the edge, it's the last in piece direction anyway, so sort it out here
                    //Four possibilities: piece square is top, left, bottom or right relatively to here
                    if (sq.row == this.currentSquare.row)
                    {
                        if (sq.column > this.currentSquare.column && sq.column != 7)
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
                                Square s = board.squares[Mathf.Abs(sq.row - 7), sq.column + i];
                                if (s.occupied)
                                {
                                    if (s.currentPiece.white != this.white || !calcFullKing)
                                    {
                                        temp.Add(s);
                                    }

                                    break;
                                }
                                temp.Add(s);
                                i++;
                            }
                        }
                        else if (sq.column != 0)
                        {
                            //Case left
                            int i = -1;
                            while (sq.column + i >= 0)
                            {
                                Square s = board.squares[Mathf.Abs(sq.row - 7), sq.column + i];
                                if (s.occupied)
                                {
                                    if (s.currentPiece.white != this.white || !calcFullKing)
                                    {
                                        temp.Add(s);
                                    }
                                    break;
                                }
                                temp.Add(s);
                                i--;
                            }
                        }
                    }
                    else
                    {
                        if (sq.row > this.currentSquare.row && sq.row != 7)
                        {
                            //Case up
                            int i = 1;
                            while (sq.row + i < 8)
                            {
                                Square s = board.squares[Mathf.Abs(sq.row - 7) - i, sq.column];
                                if (s.occupied)
                                {
                                    if (s.currentPiece.white != this.white || !calcFullKing)
                                    {
                                        temp.Add(s);
                                    }
                                    break;
                                }
                                temp.Add(s);
                                i++;
                            }
                        }
                        else if (sq.row != 0)
                        {
                            //Case bottom
                            int i = -1;
                            while (sq.row + i >= 0)
                            {
                                Square s = board.squares[Mathf.Abs(sq.row - 7) - i, sq.column];
                                if (s.occupied)
                                {
                                    if (s.currentPiece.white != this.white || !calcFullKing)
                                    {
                                        temp.Add(s);
                                    }
                                    break;
                                }
                                temp.Add(s);
                                i--;
                            }
                        }
                    }
                }
                */
                break;
            #endregion
            #region Bishop
            case PieceType.Bishop:
                foreach (Transform sq in board.children)
                {
                    Square s = sq.GetComponent<Square>();
                    if ((s.row == currentSquare.row + 1 && s.column == currentSquare.column - 1) ||
                        (s.row == currentSquare.row - 1 && s.column == currentSquare.column - 1) || 
                        (s.row == currentSquare.row - 1 && s.column == currentSquare.column + 1) || 
                        (s.row == currentSquare.row + 1 && s.column == currentSquare.column + 1))
                    {
                        if (s.occupied)
                        {
                            if (s.currentPiece.white != this.white || !calcFullKing)
                            {
                                temp.Add(s);
                            }

                            continue;
                        }
                        legalSquares.Add(s);
                    }
                }

                foreach (Square sq in legalSquares)
                {
                    //Four possibilities
                    //indicating squares works properly
                    if (sq.column == this.currentSquare.column + 1 && sq.row == this.currentSquare.row + 1)
                    {
                        //Top right
                        if (sq.column != 7 && sq.row != 7)
                        {
                            int i = 1;
                            while (sq.column + i < 8 && sq.row + i < 8)
                            {
                                Square s = board.squares[Mathf.Abs(sq.row - 7) - i, sq.column + i];
                                if (s.occupied)
                                {
                                    if (s.currentPiece.white != this.white || !calcFullKing)
                                    {
                                        temp.Add(s);
                                    }

                                    break;
                                }
                                temp.Add(s);
                                i++;
                            }
                        }
                    }
                    else if (sq.column == this.currentSquare.column + 1 && sq.row == this.currentSquare.row - 1)
                    {
                        //Bottom right
                        if (sq.column != 7 && sq.row != 0)
                        {
                            int i = 1;
                            while (sq.column + i < 8 && sq.row - i >= 0)
                            {
                                Square s = board.squares[Mathf.Abs(sq.row - 7) + i, sq.column + i];
                                if (s.occupied)
                                {
                                    if (s.currentPiece.white != this.white || !calcFullKing)
                                    {
                                        temp.Add(s);
                                    }

                                    break;
                                }
                                temp.Add(s);
                                i++;
                            }
                        }
                    }
                    else if (sq.column == this.currentSquare.column - 1 && sq.row == this.currentSquare.row - 1)
                    {
                        //Bottom left
                        if (sq.column != 0 && sq.row != 0)
                        {
                            int i = 1;
                            while (sq.column - i >= 0 && sq.row - i >= 0)
                            {
                                Square s = board.squares[Mathf.Abs(sq.row - 7) + i, sq.column - i];
                                if (s.occupied)
                                {
                                    if (s.currentPiece.white != this.white || !calcFullKing)
                                    {
                                        temp.Add(s);
                                    }

                                    break;
                                }
                                temp.Add(s);
                                i++;
                            }
                        }
                    }
                    else if (sq.column == this.currentSquare.column - 1 && sq.row == this.currentSquare.row + 1)
                    {
                        //Top left
                        if (sq.column != 0 && sq.row != 7)
                        {
                            int i = 1;
                            while (sq.column - i >= 0 && sq.row + i < 8)
                            {
                                Square s = board.squares[Mathf.Abs(sq.row - 7) - i, sq.column - i];
                                if (s.occupied)
                                {
                                    if (s.currentPiece.white != this.white || !calcFullKing)
                                    {
                                        temp.Add(s);
                                    }

                                    break;
                                }
                                temp.Add(s);
                                i++;
                            }
                        }
                    }
                }
                break;
            #endregion
            #region Knight
            case PieceType.Knight:
                foreach (Transform sq in board.children)
                {
                    Square s = sq.gameObject.GetComponent<Square>();

                    if (this.currentSquare.row + 2 < 8 && s.row == this.currentSquare.row + 2)
                    {
                        if (s.column == this.currentSquare.column - 1 || s.column == this.currentSquare.column + 1)
                        {
                            if (s.occupied)
                            {
                                if (s.currentPiece.white != this.white || !calcFullKing)
                                {
                                    temp.Add(s);
                                }

                                continue;
                            }

                            temp.Add(s);
                        }
                    }
                    else if (this.currentSquare.column + 2 < 8 && s.column == this.currentSquare.column + 2)
                    {
                        if (s.row == this.currentSquare.row + 1 || s.row == this.currentSquare.row - 1)
                        {
                            if (s.occupied)
                            {
                                if (s.currentPiece.white != this.white || !calcFullKing)
                                {
                                    temp.Add(s);
                                }

                                continue;
                            }

                            temp.Add(s);
                        }
                    }
                    else if (this.currentSquare.row - 2 >= 0 && s.row == this.currentSquare.row - 2)
                    {
                        if (s.column == this.currentSquare.column + 1 || s.column == this.currentSquare.column - 1)
                        {
                            if (s.occupied)
                            {
                                if (s.currentPiece.white != this.white || !calcFullKing)
                                {
                                    temp.Add(s);
                                }

                                continue;
                            }

                            temp.Add(s);
                        }
                    }
                    else if (this.currentSquare.column - 2 >= 0 && s.column == this.currentSquare.column - 2)
                    {
                        if (s.row == this.currentSquare.row + 1 || s.row == this.currentSquare.row - 1)
                        {
                            if (s.occupied)
                            {
                                if (s.currentPiece.white != this.white || !calcFullKing)
                                {
                                    temp.Add(s);
                                }

                                continue;
                            }

                            temp.Add(s);
                        }
                    }
                }
                break;
            #endregion
            #region Rook
            case PieceType.Rook:
                foreach (Transform sq in board.children)
                {
                    Square s = sq.GetComponent<Square>();
                    if ((s.row == currentSquare.row     && s.column == currentSquare.column - 1) ||
                        (s.row == currentSquare.row - 1 && s.column == currentSquare.column    ) ||
                        (s.row == currentSquare.row     && s.column == currentSquare.column + 1) ||
                        (s.row == currentSquare.row + 1 && s.column == currentSquare.column))
                    {
                        if (s.occupied)
                        {
                            if (s.currentPiece.white != this.white || !calcFullKing)
                            {
                                temp.Add(s);
                            }

                            continue;
                        }
                        legalSquares.Add(s);
                    }
                }

                foreach (Square sq in legalSquares)
                {
                    //If legal move is on the edge, it's the last in piece direction anyway, so sort it out here
                    //Four possibilities: piece square is top, left, bottom or right relatively to here
                    if (sq.row == this.currentSquare.row)
                    {
                        if (sq.column > this.currentSquare.column && sq.column != 7)
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
                                Square s = board.squares[Mathf.Abs(sq.row - 7), sq.column + i];
                                if (s.occupied)
                                {
                                    if (s.currentPiece.white != this.white || !calcFullKing)
                                    {
                                        temp.Add(s);
                                    }

                                    break;
                                }
                                temp.Add(s);
                                i++;
                            }
                        }
                        else if (sq.column != 0)
                        {
                            //Case left
                            int i = -1;
                            while (sq.column + i >= 0)
                            {
                                Square s = board.squares[Mathf.Abs(sq.row - 7), sq.column + i];
                                if (s.occupied)
                                {
                                    if (s.currentPiece.white != this.white || !calcFullKing)
                                    {
                                        temp.Add(s);
                                    }
                                    break;
                                }
                                temp.Add(s);
                                i--;
                            }
                        }
                    }
                    else
                    {
                        if (sq.row > this.currentSquare.row && sq.row != 7)
                        {
                            //Case up
                            int i = 1;
                            while (sq.row + i < 8)
                            {
                                Square s = board.squares[Mathf.Abs(sq.row - 7) - i, sq.column];
                                if (s.occupied)
                                {
                                    if (s.currentPiece.white != this.white || !calcFullKing)
                                    {
                                        temp.Add(s);
                                    }
                                    break;
                                }
                                temp.Add(s);
                                i++;
                            }
                        }
                        else if (sq.row != 0)
                        {
                            //Case bottom
                            int i = -1;
                            while (sq.row + i >= 0)
                            {
                                Square s = board.squares[Mathf.Abs(sq.row - 7) - i, sq.column];
                                if (s.occupied)
                                {
                                    if (s.currentPiece.white != this.white || !calcFullKing)
                                    {
                                        temp.Add(s);
                                    }
                                    break;
                                }
                                temp.Add(s);
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
                if(originalPieceOnNewSquare != null) 
                {
                    originalPieceOnNewSquare.currentSquare = null; 
                }
                newSquare.currentPiece = this;
                newSquare.occupied = true;
                this.currentSquare = newSquare;

                if(!board.IsChecking(!board.whiteOnMove))
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

        Debug.Log(legalSquares.Count);

        return legalSquares;
    }
}