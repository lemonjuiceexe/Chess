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
    public float transitionSpeed = 0.1f;
    //If it differs, the move happened in the very previous frame
    public bool lastFrameWhiteOnMove = true;
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
        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                squares[i, j] = children[((i) * 8) + j].GetComponent<Square>();
            }
        }

        disableForcedColorMoves = PlayerPrefs.GetInt("disableForcedColorMoves", 0) == 1;
        disableTurnBoard = PlayerPrefs.GetInt("disableBoardFlip", 0) == 1;
    }

    private void Update()
    {
        //Check after a move detection
        //Just moved
        if(whiteOnMove != lastFrameWhiteOnMove)
        {
            check = IsChecked(!whiteOnMove);
        }
    }

    private void LateUpdate()
    {
        lastFrameWhiteOnMove = whiteOnMove;
    }

    public bool IsChecked(bool color)
    {
        List<Square> ls = new List<Square>();
        //For all pieces of the same color as the piece just moved
        for (int i = (color ? 0 : 16); i < (color ? 16 : 32); i++)
        {
            List<Square> tp = pieces[i].CalculateLegalMoves(false);

            foreach (Square s in tp)
            {
                if (!ls.Contains(s))
                {
                    ls.Add(s);
                }
            }
        }
        //ColorSquares(ls, Color.yellow);
        if (ls.Contains(pieces[lastFrameWhiteOnMove ? 16 : 0].currentSquare))
        {
            Debug.Log("Check!");
            return true;
        }

        //ls.Clear();
        return false;
    }

    public void ColorLegal(List<Square> legalSquares)
    {
        foreach (Square sq in legalSquares)
        {
            sq.legalForSelectedPiece = true;
            if (sq.occupied)
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
        foreach(Square sq in squares)
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