using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    
    //2D array of squares, actually representing a board (8x8)
    public Square[,] squares = new Square[8, 8];
    [Header("Pieces and squares arrays")]
    public Transform[] children;
    public Transform[] pieces;

    [Header("Gameobjects references")]
    public Camera cam;

    [Header("Game management")]
    public Piece selectedPiece;
    public bool whiteOnMove = true;
    public float transitionSpeed = 0.1f;
    //If it differs, the move happened in the very previous frame
    public bool lastFrameWhiteOnMove = true;

    [Header("Colours")]
    public Color legalColor;
    public Color legalTakeColor;

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

    private void LateUpdate()
    {
        lastFrameWhiteOnMove = whiteOnMove;
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

    public void ClearLegal()
    {
        foreach (Square sq in squares)
        {
            sq.legalForSelectedPiece = false;

            sq.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}