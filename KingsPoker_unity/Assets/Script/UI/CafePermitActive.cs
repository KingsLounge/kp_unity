using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CafePermitActive : MonoBehaviour
{
    [System.Serializable]
    public struct PermitActiveData
    {
        public CAFE_MEMBER_PERMIT permit;
        public bool active;
    }
    private static List<CafePermitActive> list = new List<CafePermitActive>();

    public bool defaultActive = false;
    public List<PermitActiveData> permitDatas = new List<PermitActiveData>();

    public void Awake()
    {
        list.Add(this);
    }

    public void OnDestroy()
    {
        list.Remove(this);
    }

    public static void EnterCafe(int cafeIdx)
    {
        CAFE_MEMBER_PERMIT permit = (CAFE_MEMBER_PERMIT)Cafe.instance.GetCafeList(cafeIdx).cafeMembers[0].permit;
    }
}
