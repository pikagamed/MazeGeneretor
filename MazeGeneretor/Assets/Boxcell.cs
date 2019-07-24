using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Boxcell
{
    #region 欄位宣告
    protected int _xPosition;
    protected int _yPosition;
    private int _wallId;     //方格所在位置的x座標、y座標，不同為奇數或偶數時，為牆壁。 這個牆壁是可能被破壞的。如果不是牆壁，值為-1
    private int[] _connectSpace = new int[2];   //該牆壁連接的兩個方格。如果該牆壁為邊界，兩值均為-1
    private bool _wallOpen; //這個牆壁是否會出現。如果此值為true，表示在生成牆壁時，這個BOX的位置是空的。
                                            //不能被破壞的牆壁為：x座標、y座標均為偶數；x座標或y座標為0或座標最大值(迷宮邊界)
    private int _spaceId;  //方格所在位置的x座標、y座標均為奇數時，為空間。是玩家可以自由走動的地方。如果不是空間，值為-1
    #endregion

    #region 屬性設定
    public int xPosition { get => _xPosition; }
    public int yPosition { get => _yPosition; }
    public int wallId { get => _wallId; }
    public int[] connectSpace { get => _connectSpace; }
    public bool wallOpen { get => _wallOpen; set => _wallOpen = value; }
    public int spaceId { get => _spaceId; }
    #endregion

    #region 建構函式
    public Boxcell(int xPos, int yPos, int wallNum, int connectSpace0, int connectSpace1, int spaceNum )
    {
        _xPosition = xPos;
        _yPosition = yPos;
        _wallId = wallNum;
        _connectSpace[0] = connectSpace0;
        _connectSpace[1] = connectSpace1;
        _wallOpen = false;
        _spaceId = spaceNum;
    }
    #endregion

    #region 演算函式

    #endregion
}
