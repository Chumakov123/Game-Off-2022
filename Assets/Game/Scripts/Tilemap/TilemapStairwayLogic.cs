using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapStairwayLogic : MonoBehaviour
{
    //��������� PlatformEffector2D ������ ������ Physics2D.IgnoreLayerCollision. ����� ��� ���������, � ���������� ��������� ����� ����� ���� useColliderMask
    void Start()
    {
        PlatformEffector2D platformEffector = GetComponent<PlatformEffector2D>();
        platformEffector.useColliderMask = false;
    }
}
