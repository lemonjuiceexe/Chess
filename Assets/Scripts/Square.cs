using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    //Column, row
    public int column;
    public int row;

    public bool occupied = false;
    public Piece currentPiece;

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
}