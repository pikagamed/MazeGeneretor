using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("固定出現的阻擋器")]
    public GameObject cornerBox;
    [Header("動態出現的阻擋器")]
    public GameObject dynamicBox;
    [Header("迷宮長"), Range(9,25)]
    public int mazeHeight = 15;
    int actualMazeHeight;
    [Header("迷宮寬"), Range(9, 25)]
    public int mazeWidth = 15;
    int actualMazeWidth;
    Boxcell[,] generateCell;
    int spaceCellCount;
    List<Boxcell> wallList;
    List<List<int>> connectList = new List<List<int>>();   //巡迴導覽座標，這個List紀錄能巡覽到哪個Cell格。只要巡覽到該Cell，就認為這兩個Cell是連接的。


    // Start is called before the first frame update
    void Start()
    {
        #region 基礎牆壁宣告
        //迷宮實際上長寬為 給定數字×2+1.
        //X邊界編號為mazeWidth×2、Y邊界編號為mazeHeight×2
        actualMazeHeight = mazeHeight * 2 + 1;
        actualMazeWidth = mazeWidth * 2 + 1;

        spaceCellCount = 0;
        wallList = new List<Boxcell>();
        generateCell = new Boxcell[actualMazeHeight, actualMazeWidth];

        for(int i=0; i< actualMazeHeight; i++)
        {
            for(int j=0; j< actualMazeWidth; j++)
            {
                //如果i和j都是偶數，或者i=0或邊界值，或者j=0或邊界值，此BOX為不可動的牆壁
                //如果i是奇數，j是偶數，表示該BOX為連接左右空間之牆壁。 連接的空間為 [ (i/2)*mazeWidth+j/2-1, (i/2)*mazeWidth+j/2 ]
                //如果i是偶數，j是奇數，表示該BOX為連接上下空間之牆壁。 連接的空間為 [ (i/2-1)*mazeWidth+j/2, (i/2)*mazeWidth+j/2 ]
                //如果i和j都是奇數，則為空間

                if(  (i%2==0 && j%2==0) || (i==0) || (i== actualMazeHeight-1) || (j==0) || (j== actualMazeWidth-1) )
                {
                    generateCell[i, j] = new Boxcell( i , j , -1, -1, -1, -1 );
                }
                else if( i%2==1 && j%2==0 )
                {
                    generateCell[i, j] = new Boxcell(i, j, wallList.Count, (i / 2) * mazeWidth + j / 2 - 1, (i / 2) * mazeWidth + j / 2, -1);
                    wallList.Add(generateCell[i, j]);
                }
                else if (i % 2 == 0 && j % 2 == 1)
                {
                    generateCell[i, j] = new Boxcell(i, j, wallList.Count, (i / 2 - 1) * mazeWidth + j / 2, (i / 2) * mazeWidth + j / 2, -1);
                    wallList.Add(generateCell[i, j]);
                }
                else
                {
                    generateCell[i, j] = new Boxcell(i, j, -1, -1, -1, spaceCellCount);
                    connectList.Add(new List<int>());
                    spaceCellCount++;
                }
            }
        }
        #endregion

        #region 隨機打掉牆壁
        while (wallList.Count > 0)
        {
            int drawWallId = Random.Range(0, wallList.Count);
            int connectSpace0 = wallList[drawWallId].connectSpace[0];
            int connectSpace1 = wallList[drawWallId].connectSpace[1];
            //巡覽從connectSpace0到connectSpace1，如果是連接在一起的，不打掉牆壁，反之，打掉牆壁

            if (!ExploreTo (connectSpace0, connectSpace1))
            {
                wallList[drawWallId].wallOpen = true;
                connectList[connectSpace0].Add(connectSpace1);
            }

            wallList.Remove(wallList[drawWallId]);
        }

        #endregion


        #region 迷宮生成
        //正中央為(0,0)，左上角座標為( -0.75F*mazeWidth , 0.75F*mazeHeight )
        for (int i=0; i<actualMazeHeight; i++)
        {
            for(int j=0; j<actualMazeWidth; j++)
            {
                if(generateCell[i, j].wallId == -1 && generateCell[i, j].spaceId==-1)
                {
                    Instantiate(cornerBox , new Vector2( -0.75F*mazeWidth + 0.75F*j, 0.75F*mazeHeight - 0.75F*i ), Quaternion.identity);
                }
                else if (generateCell[i, j].wallId != -1 && !generateCell[i,j].wallOpen)
                {
                    Instantiate(dynamicBox, new Vector2(-0.75F * mazeWidth + 0.75F * j, 0.75F * mazeHeight - 0.75F * i), Quaternion.identity);
                }
            }
        }

        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //巡覽
    public bool ExploreTo(int startSpaceId, int targetSpaceId)
    {
        bool spaceFound = false;

        for (int i = 0; i < connectList[startSpaceId].Count; i++)
        {
            //如果已經有找到，不要再浪費時間在迴圈中
            if (spaceFound) return spaceFound;

            if (connectList[startSpaceId][i] == targetSpaceId)
            {
                spaceFound = true;
            }
            else
            {
                spaceFound = ExploreTo(connectList[startSpaceId][i], targetSpaceId);
                if (spaceFound) return spaceFound;
                spaceFound = ExploreTo(targetSpaceId, connectList[startSpaceId][i]);
                if (spaceFound) return spaceFound;
            }
        }

        return spaceFound;
    }
}
