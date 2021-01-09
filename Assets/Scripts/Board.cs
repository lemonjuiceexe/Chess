using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    //2D array of squares, actually representing a board (8x8)
    public Square[,] squares = new Square[8, 8];
    public Transform[] children;

    public Camera cam;

    public Piece selectedPiece;
    public bool turnBoard = true;
    public bool whiteOnMove = true;
    //If it differs, the move happened in the very previous frame
    public bool lastFrameWhiteOnMove = true;

    public Color legalColor;
    public Color legalTakeColor;

    public float transitionSpeed = 0.1f; //set it in board, so we dont have to change in each and every square individually

    private void Start()
    {
        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                squares[i, j] = children[((i) * 8) + j].GetComponent<Square>();
            }
        }
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