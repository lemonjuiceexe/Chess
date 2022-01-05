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

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote))
		{
			if (!consoleOpen)
			{
				ReadSettings();
			}
			consoleOpen = !consoleOpen;
		}
	}

	private void OnGUI()
	{
		if (consoleOpen)
		{
			GUI.Box(new Rect(10, 10, 200, 90), "Debug Menu");
			#region Color
			GUIStyle green = new GUIStyle();
			GUIStyle red = new GUIStyle();
			green.normal.background = MakeTex(Color.green);
			red.normal.background = MakeTex(Color.red);
			#endregion
			//Button for forced moves
			if (GUI.Button(new Rect(12, 30, 160, 20), "Forced color moves"))
			{
				disableForcedColorMoves = !disableForcedColorMoves;
				PushSettingsToBoard();
			}
			//Colorful boxes for forced moves
			if (!board.disableForcedColorMoves)
			{
				GUI.Box(new Rect(185, 30, 20, 20), " ", green);
			}
			else
			{
				GUI.Box(new Rect(185, 30, 20, 20), " ", red);
			}

			//Button for board flip
			if (GUI.Button(new Rect(12, 55, 160, 20), "Board flip"))
			{
				disableBoardFlip = !disableBoardFlip;
				PushSettingsToBoard();
			}
			//Colorful boxes for board flip
			if (!board.disableTurnBoard)
			{
				GUI.Box(new Rect(185, 55, 20, 20), " ", green);
			}
			else
			{
				GUI.Box(new Rect(185, 55, 20, 20), " ", red);
			}
		}
	}

	private void PushSettingsToBoard()
	{
		board.disableForcedColorMoves = this.disableForcedColorMoves;
		board.disableTurnBoard = this.disableBoardFlip;
		PlayerPrefs.SetInt("disableBoardFlip", System.Convert.ToInt32(disableBoardFlip));
		PlayerPrefs.SetInt("disableForcedColorMoves", System.Convert.ToInt32(disableForcedColorMoves));
	}

	private void ReadSettings()
	{
		this.disableBoardFlip = PlayerPrefs.GetInt("disableBoardFlip", 0) == 1;
		this.disableForcedColorMoves = PlayerPrefs.GetInt("disableForcedColorMoves", 0) == 1;
	}

	private Texture2D MakeTex(Color col)
	{
		Color32[] px = new Color32[4];
		for (int i = 0; i < 4; i++)
		{
			px[i] = col;
		}
		Texture2D result = new Texture2D(2, 2);
		result.SetPixels32(px);
		result.Apply();
		return result;
	}
}
