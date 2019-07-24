using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GenerateStyle { 磚塊, 牆壁 }

public class MazeGenerator : MonoBehaviour
{
    [Header("生成方式")]
    public GenerateStyle generateStyle;
    [Header("固定出現的阻擋器")]
    public GameObject cornerBox;
    [Header("動態出現的阻擋器")]
    public GameObject dynamicBox;

    [Header("動態牆(直)")]
    public GameObject dynamicWallTall;
    [Header("動態牆(橫)")]
    public GameObject dynamicWallWide;
    [Header("靜態牆(直)")]
    public GameObject staticWallTall;
    [Header("靜態牆(橫)")]
    public GameObject staticWallWide;

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
                    //connect中，每個space所有的List最開始會拿到一個內含一個成員的List，該成員為自身spaceId
                    List<int> initialList = new List<int>();
                    initialList.Add(spaceCellCount);
                    connectList.Add(initialList);
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

                //連結空間的特別方式
                int startSpaceSet;
                int targetSpaceSet;

                //首先檢查開始空間的List，預設每個space應該都至少有一個成員。如果該List為空，表示該空間已經被整合到其他空間
                if (connectList[connectSpace0].Count != 0)
                {
                    //開始空間中還存有空間，即此空間尚未被整合
                    if(connectList[connectSpace1].Count != 0)
                    {
                        //如果目標空間也尚未被整合，那就將目標空間內的空間全部整合至開始空間中
                        for(int i=0; i< connectList[connectSpace1].Count; i++)
                        {
                            connectList[connectSpace0].Add(connectList[connectSpace1][i]);
                        }
                        //完了後，清掉目標空間內的空間
                        connectList[connectSpace1].Clear();
                    }
                    else
                    {
                        //目標空間已經被整合，那就要檢查被整合至哪一個空間內
                        for(targetSpaceSet=0; targetSpaceSet< connectSpace1; targetSpaceSet++)
                        {
                            if( connectList[targetSpaceSet].Contains(connectSpace1) )
                            {
                                //找到目標空間所在的List了，和connect0比較大小
                                if( connectSpace0 < targetSpaceSet )
                                {
                                    //targetSpaceSet的內容要被整合進connect0中
                                    for( int i=0; i<connectList[targetSpaceSet].Count; i++  )
                                    {
                                        connectList[connectSpace0].Add(connectList[targetSpaceSet][i]);
                                    }
                                    //完了後，清掉targetSpaceSet內的空間
                                    connectList[targetSpaceSet].Clear();
                                }
                                else
                                {
                                    //connect0的內容要被整合進targetSpaceSet中
                                    for (int i = 0; i < connectList[connectSpace0].Count; i++)
                                    {
                                        connectList[targetSpaceSet].Add(connectList[connectSpace0][i]);
                                    }
                                    //完了後，清掉targetSpaceSet內的空間
                                    connectList[connectSpace0].Clear();
                                }

                                //無論結果如何，結束就跳出迴圈
                                break;
                            }
                        }
                    }
                }
                else
                {
                    //開始空間中已經沒有空間，此空間已經被整合進其他的空間內了
                    //在這情況下，要從頭找尋開始空間被整合進哪一個空間內
                    for(startSpaceSet=0; startSpaceSet<connectSpace0; startSpaceSet++)
                    {
                        if( connectList[startSpaceSet].Contains(connectSpace0) )
                        {
                            //找到connect0被整合進的空間了，跳出
                            break;
                        }
                    }

                    if(connectList[connectSpace1].Count!=0)
                    {
                        //目標空間未被整合
                        for(int i=0; i<connectList[connectSpace1].Count; i++)
                        {
                            connectList[startSpaceSet].Add(connectList[connectSpace1][i]);
                        }
                        connectList[connectSpace1].Clear();
                    }
                    else
                    {
                        //目標空間已經被整合，那就要檢查被整合至哪一個空間內
                        for (targetSpaceSet = 0; targetSpaceSet < connectSpace1; targetSpaceSet++)
                        {
                            if (connectList[targetSpaceSet].Contains(connectSpace1))
                            {
                                //找到目標空間所在的List了，和startSpaceSet比較大小
                                if (startSpaceSet < targetSpaceSet)
                                {
                                    //targetSpaceSet的內容要被整合進startSpaceSet中
                                    for (int i = 0; i < connectList[targetSpaceSet].Count; i++)
                                    {
                                        connectList[startSpaceSet].Add(connectList[targetSpaceSet][i]);
                                    }
                                    //完了後，清掉targetSpaceSet內的空間
                                    connectList[targetSpaceSet].Clear();
                                }
                                else
                                {
                                    //startSpaceSet的內容要被整合進targetSpaceSet中
                                    for (int i = 0; i < connectList[startSpaceSet].Count; i++)
                                    {
                                        connectList[targetSpaceSet].Add(connectList[startSpaceSet][i]);
                                    }
                                    //完了後，清掉targetSpaceSet內的空間
                                    connectList[startSpaceSet].Clear();
                                }

                                //無論結果如何，結束就跳出迴圈
                                break;
                            }
                        }
                    }
                }
            }

            //connectList[connectSpace0].Add(connectSpace1);

            wallList.Remove(wallList[drawWallId]);
        }

        #endregion

        #region 迷宮生成
        //正中央為(0,0)，左上角座標為( -0.75F*mazeWidth , 0.75F*mazeHeight )
        switch (generateStyle)
        {
            case GenerateStyle.磚塊:
                for (int i = 0; i < actualMazeHeight; i++)
                {
                    for (int j = 0; j < actualMazeWidth; j++)
                    {
                        if (generateCell[i, j].wallId == -1 && generateCell[i, j].spaceId == -1)
                        {
                            Instantiate(cornerBox, new Vector2(-0.75F * mazeWidth + 0.75F * j, 0.75F * mazeHeight - 0.75F * i), Quaternion.identity);
                        }
                        else if (generateCell[i, j].wallId != -1 && !generateCell[i, j].wallOpen)
                        {
                            Instantiate(dynamicBox, new Vector2(-0.75F * mazeWidth + 0.75F * j, 0.75F * mazeHeight - 0.75F * i), Quaternion.identity);
                        }
                    }
                }
                break;
            case GenerateStyle.牆壁:
                for (int i = 0; i < actualMazeHeight; i++)
                {
                    for (int j = 0; j < actualMazeWidth; j++)
                    {
                        if( i%2==j%2 )
                        {

                        }
                        else if( i==0 || i==actualMazeHeight-1 )
                        {
                            Instantiate(staticWallWide, new Vector2(-0.57F * mazeWidth + 0.57F * j, 0.57F * mazeHeight - 0.57F * i), Quaternion.Euler(0,0,90));
                        }
                        else if( j==0 || j==actualMazeWidth-1)
                        {
                            Instantiate(staticWallTall, new Vector2(-0.57F * mazeWidth + 0.57F * j, 0.57F * mazeHeight - 0.57F * i), Quaternion.identity);
                        }
                        else if(generateCell[i, j].wallId != -1 && !generateCell[i, j].wallOpen)
                        {
                            Instantiate(i % 2 == 0 ? dynamicWallWide : dynamicWallTall, new Vector2(-0.57F * mazeWidth + 0.57F * j, 0.57F * mazeHeight - 0.57F * i), i%2==0 ? Quaternion.Euler(0, 0, 90) : Quaternion.identity);
                        }
                    }
                }
                break;
        }



        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        //按空白鍵重生
        if(Input.GetKeyDown(KeyCode.Space))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(Application.loadedLevel);
        }
    }

    //巡覽
    public bool ExploreTo(int startSpaceId, int targetSpaceId, int pastStartSpaceId = -1)
    {
        bool spaceFound = false;

        //特別巡覽方式
        if (connectList[startSpaceId].Count != 0)
        {
            return connectList[startSpaceId].Contains(targetSpaceId);
        }
        else
        {
            for(int startSpaceSet = 0; startSpaceSet < startSpaceId; startSpaceSet++)
            {
                if( connectList[startSpaceSet].Contains(startSpaceId) )
                {
                    return connectList[startSpaceSet].Contains(targetSpaceId);
                }
            }
        }


        //if(startSpaceId < targetSpaceId)
        //{
        //    //根據connentList的格式
        //    for (int i = 0; i < connectList[startSpaceId].Count; i++)
        //    {
        //        if (connectList[startSpaceId][i] == targetSpaceId)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return ExploreTo(connectList[startSpaceId][i], targetSpaceId);
        //        }
        //    }
        //}
        //else
        //{
        //    //逆轉格式
        //    //比如說9x9中10要找0路過9的情況， 這時startSpaceId=10, targetSpaceId=0。
        //    //但是connectList[10]中不會有9所以要找connectList[i=0~9]中有沒有存在通往10的List
        //    for (int i = 0; i < connectList[targetSpaceId].Count; i++)
        //    {
        //        if (connectList[targetSpaceId][i] == startSpaceId)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return ExploreTo(connectList[startSpaceId][i], targetSpaceId);
        //        }
        //    }
        //}


        ////向下巡覽
        ////不能向下巡覽的條件為：前一個ID來自其下方(direction為UP)，或該空間已經處於最下面一列(startSpaceId/mazeWidth的結果會等於mazeHeight-1)
        //if( direction!=ExploreDirection.Up && startSpaceId / mazeWidth < mazeHeight - 1)
        //{
        //    //往下巡覽的方法是當前ID + 迷宮寬度
        //    if( connectList[startSpaceId].Contains(startSpaceId+mazeWidth) )
        //    {
        //        //若此值為真，表示和下方空間是通路，此時檢查巡覽到的空間是否為目標空間
        //        if(startSpaceId + mazeWidth == targetSpaceId)
        //        {
        //            //若為目標空間，就回傳true
        //            return true;
        //        }
        //        else
        //        {
        //            //若不是目標空間，則繼續做巡覽。以下方空間為新的起點繼續做巡覽，方向為下
        //            return ExploreTo(startSpaceId + mazeWidth, targetSpaceId, ExploreDirection.Down);
        //        }
        //    }
        //}

        ////向上巡覽
        ////不能向上巡覽的條件為：前一個ID來自其上方(direction為DOWN)，或該空間已經處於最上面一列(startSpaceId/mazeWidth的結果會等於0)
        //if (direction != ExploreDirection.Down && startSpaceId / mazeWidth > 0)
        //{
        //    //往上巡覽的方法是當前ID - 迷宮寬度，依照儲存的邏輯，大數字會存於小數字的路徑，因此此處會反轉索引和值的位置
        //    if (connectList[startSpaceId - mazeWidth].Contains(startSpaceId))
        //    {
        //        //若此值為真，表示和上方空間是通路，此時檢查巡覽到的空間是否為目標空間
        //        if (startSpaceId - mazeWidth == targetSpaceId)
        //        {
        //            //若為目標空間，就回傳true
        //            return true;
        //        }
        //        else
        //        {
        //            //若不是目標空間，則繼續做巡覽。以上方空間為新的起點繼續做巡覽，方向為上
        //            return ExploreTo(startSpaceId - mazeWidth, targetSpaceId, ExploreDirection.Up);
        //        }
        //    }
        //}

        ////向左巡覽
        ////不能向左巡覽的條件為：前一個ID來自其左方(direction為RIGHT)，或該空間已經處於最左面一欄(startSpaceId%mazeWidth的結果會等於0)
        //if (direction != ExploreDirection.Right && startSpaceId % mazeWidth > 0)
        //{
        //    //往左巡覽的方法是當前ID - 1，依照儲存的邏輯，大數字會存於小數字的路徑，因此此處會反轉索引和值的位置
        //    if (connectList[startSpaceId - 1].Contains(startSpaceId))
        //    {
        //        //若此值為真，表示和左方空間是通路，此時檢查巡覽到的空間是否為目標空間
        //        if (startSpaceId - 1 == targetSpaceId)
        //        {
        //            //若為目標空間，就回傳true
        //            return true;
        //        }
        //        else
        //        {
        //            //若不是目標空間，則繼續做巡覽。以左方空間為新的起點繼續做巡覽，方向為左
        //            return ExploreTo(startSpaceId - 1, targetSpaceId, ExploreDirection.Left);
        //        }
        //    }
        //}

        ////向右巡覽
        ////不能向右巡覽的條件為：前一個ID來自其右方(direction為LEFT)，或該空間已經處於最右面一欄(startSpaceId%mazeWidth的結果會等於mazeWidth-1)
        //if (direction != ExploreDirection.Left && startSpaceId % mazeWidth < mazeWidth - 1)
        //{
        //    //往右巡覽的方法是當前ID + 1
        //    if (connectList[startSpaceId].Contains(startSpaceId + 1))
        //    {
        //        //若此值為真，表示和右方空間是通路，此時檢查巡覽到的空間是否為目標空間
        //        if (startSpaceId + 1 == targetSpaceId)
        //        {
        //            //若為目標空間，就回傳true
        //            return true;
        //        }
        //        else
        //        {
        //            //若不是目標空間，則繼續做巡覽。以右方空間為新的起點繼續做巡覽，方向為右
        //            return ExploreTo(startSpaceId + 1, targetSpaceId, ExploreDirection.Right);
        //        }
        //    }
        //}

        //必須要有機會和比鄰空間形成通路才有機會更改spaceFound的值。
        //如果已經到達死路，即再也沒有其他可行路線時會傳回false

        return spaceFound;
    }
}
