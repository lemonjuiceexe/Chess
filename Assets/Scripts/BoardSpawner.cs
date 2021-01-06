using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSpawner : MonoBehaviour
{
    public GameObject board;
    public GameObject bTile;
    public GameObject wTile;

    float x = -35f;
    float y = 35f;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Instantiate(wTile, new Vector3(x, y, 0), Quaternion.identity, board.transform);
                x += 10;
                Instantiate(bTile, new Vector3(x, y, 0), Quaternion.identity, board.transform);
                x += 10;
            }
            x = -35f;
            y -= 10;
            for (int j = 0; j < 4; j++)
            {
                Instantiate(bTile, new Vector3(x, y, 0), Quaternion.identity, board.transform);
                x += 10;
                Instantiate(wTile, new Vector3(x, y, 0), Quaternion.identity, board.transform);
                x += 10;
            }
            x = -35f;
            y -= 10;
        }
    }
}
