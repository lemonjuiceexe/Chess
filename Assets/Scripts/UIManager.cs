using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private bool consoleOpen = false;
    [SerializeField]
    private Board board;
    private bool disableForcedColorMoves = false;
    private bool disableBoardFlip = false; 
    public float timeToBoardPush = 5f;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote))
        {
            consoleOpen = !consoleOpen;
            PushSettingsToBoard();
        }

        if(timeToBoardPush > 0f)
        {
            timeToBoardPush -= Time.deltaTime;
        } 
        else
        {
            PushSettingsToBoard();
            timeToBoardPush = 5f;
        }
    }

    private void OnGUI()
    {
        if(consoleOpen)
        {
            GUI.Box(new Rect(10, 10, 200, 90), "Debug Menu");
            disableForcedColorMoves = GUI.Toggle(new Rect(12, 30, 200, 20), disableForcedColorMoves, "Disable forced color moves");
            disableBoardFlip = GUI.Toggle(new Rect(12, 60, 200, 20), disableBoardFlip, "Disable board flip");
        }
    }

    private void PushSettingsToBoard()
    {
        board.disableForcedColorMoves = this.disableForcedColorMoves;
        board.turnBoard = !this.disableBoardFlip;
    }
}
