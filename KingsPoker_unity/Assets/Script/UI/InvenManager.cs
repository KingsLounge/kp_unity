using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.U2D;

public class InvenManager : MonoBehaviour
{
    private List<JObject> invenList;
    public GameObject invenSlotPrefab;
    public Transform contantContainer;
    public SpriteAtlas shopAtlas;
    public Animator ani;
    private static InvenManager instance;
    public static InvenManager Instance
    {
        get{return instance;}
    }
    private void Awake() {
        instance = this;
    }
    public void Init()
    {
        
        for(int i = 0; i<invenList.Count; i++)
        {
            InvenSlot slot;
            Console.Log(invenList[i]["ref_idx"]);
            var refitemIdx = (int)invenList[i]["ref_idx"];
            
            var jShopData = IAPManager.Instance.GetDicData(refitemIdx);
            
            if(jShopData == null)
            {
                Console.Error(string.Format("존재하지 않는 아이템 코드 : {0}",refitemIdx));
                continue;
            }

            if(i<contantContainer.childCount)
            {
                slot = contantContainer.GetChild(i).GetComponent<InvenSlot>();
            }
            else
            {
                slot = Instantiate(invenSlotPrefab, contantContainer).GetComponent<InvenSlot>();
            }
            if(slot != null)
            {
                var sp = shopAtlas.GetSprite(jShopData["img"].ToString());
                if(sp == null)
                {
                    Console.Error(string.Format("다음 이미지를 찾을 수 없습니다.: {0}",jShopData["img"].ToString()));
                }
                else
                {
                    slot.itemImage.sprite = sp;
                }
                
                slot.itemNameText.text = jShopData["name"].ToString();
                slot.refitemIdx = refitemIdx;
            }
            else
            {
                Console.Error(contantContainer.GetChild(i).name + "에 InvenSlot컴포넌트가 존재하지 않습니다.");
            }
            
        }
        Console.Log(string.Format("{0} : {1}",invenList.Count, contantContainer.childCount));
        var num = contantContainer.childCount - invenList.Count;
        for(int i = num-1; i >= 0; i--)
        {
            Destroy(contantContainer.GetChild(invenList.Count+i).gameObject);
        }
        // while(invenList.Count < contantContainer.childCount)
        // {
        //     Destroy(contantContainer.GetChild(contantContainer.childCount-1).gameObject);
        //     Console.Log(string.Format("{0} : {1}",invenList.Count, contantContainer.childCount));
        // }
        Console.Log(string.Format("{0} : {1}",invenList.Count, contantContainer.childCount));
        LoadingCircle.Instance.StopSpin();
        gameObject.SetActive(true);
    }
    
    public void OnClickCloseButton()
    {
        ani.Play("ShopCloseAni");
    }
    public void CloseWindow()
    {
        gameObject.SetActive(false);
    }
}
