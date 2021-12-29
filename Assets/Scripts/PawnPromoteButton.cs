using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnPromoteButton : MonoBehaviour
{
    public Board board;
    [SerializeField] private PieceType type;
    [SerializeField] private GameObject enabler;
    public bool white;
    public float change = 0.2f;
    private bool big = false;
    //Initial scale for buttons
    private float[] initial = new float[3];
    [SerializeField] private Sprite whiteSprite;
    [SerializeField] private Sprite blackSprite;

    void Awake()
    {
        for(int i = 0; i < 3; i++)
        {
            initial[i] = GetComponent<Transform>().localScale[i];
        }
    }
    void OnEnable()
    {
        GetComponent<SpriteRenderer>().sprite = board.selectedPiece.white ? whiteSprite : blackSprite;
    }
    void OnMouseOver()
    {
        if(!big)
        {
            GetComponent<Transform>().localScale = new Vector3(initial[0] + change, initial[1] + change, initial[2]);
            big = true;
        }
    }
    private void OnMouseDown()
    {
        board.selectedPiece.type = type;
        board.selectedPiece.GetComponent<SpriteRenderer>().sprite = board.selectedPiece.white ? whiteSprite : blackSprite;
        enabler.SetActive(false);

        board.AfterMove();
        board.selectedPiece = null;
        board.ClearLegal();
        board.whiteOnMove = !board.whiteOnMove;
    }
    void OnMouseExit()
    {
        if(big)
        {
            GetComponent<Transform>().localScale = new Vector3(initial[0], initial[1], initial[2]);
            big = false;
        }
    }
}
