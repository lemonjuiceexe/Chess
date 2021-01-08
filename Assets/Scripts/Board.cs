using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    //2D array of squares, actually representing a board (8x8)
    public Square[,] squares = new Square[8, 8];
    public Transform[] children;

    public Piece selectedPiece;

    public Color legalColor;

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

    public void ColorLegal(List<Square> legalSquares)
    {
        foreach (Square sq in legalSquares)
        {
            sq.legalForSelectedPiece = true;

            sq.GetComponent<SpriteRenderer>().color = legalColor;
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