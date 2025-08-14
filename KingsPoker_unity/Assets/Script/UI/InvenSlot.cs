using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
public class InvenSlot : MonoBehaviour
{
    public Image itemImage;
    public Text itemNameText;
    public int refitemIdx;
    public int itemCount;
    public void OnClickUseButton()
    {
        PublisherApiManager.Instance.UseItemApi(refitemIdx,UseItemCallback);
    }
    public void UseItemCallback(long statusCode, JObject jData) 
    {
        switch(statusCode)
        {
            case 200:
                Console.Log(jData.ToString());
                InfoManager.Instance.GetMyItems();
            break;
        }
    }
}
