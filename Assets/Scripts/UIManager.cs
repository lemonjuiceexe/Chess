using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private bool consoleOpen = false;
    [SerializeField]
    private Board board;
    private bool disableForcedColorMoves = false;
    public float timeToBoardPush = 10f;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote))
        {
            consoleOpen = !consoleOpen;
        }

        if(timeToBoardPush > 0f)
        {
            timeToBoardPush -= Time.deltaTime;
        } 
        else
        {
            board.disableForcedColorMoves = this.disableForcedColorMoves;
        }
    }

    private void OnGUI()
    {
        if(consoleOpen)
        {
            GUI.Box(new Rect(10, 10, 200, 90), "Debug Menu");
            disableForcedColorMoves = GUI.Toggle(new Rect(12, 30, 200, 20), disableForcedColorMoves, "Disable forced color moves");
        }
    }
}
