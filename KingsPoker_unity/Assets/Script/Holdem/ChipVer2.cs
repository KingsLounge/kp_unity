using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class ChipVer2 : MonoBehaviour
{
    public PokerChipText chipText;
    
    public BettingManager bettingManager;
    public Player player;
    
    [SerializeField]
    private ChipDataList chipData;
    [SerializeField]
    private Transform chipContainer;
    [SerializeField]
    private GameObject chipObj;
    [SerializeField]
    private float spacing = 10f;
    [SerializeField]
    private Transform disableParent;
    [SerializeField]
    private Transform chipContainerParnet;

    private List<GameObject> chipObjs = new List<GameObject>();
    private List<Transform> chipContainers = new List<Transform>();

    private ObjectPool2<GameObject> chipPool = new ObjectPool2<GameObject>();

    private UnityAction callback = null;

    private bool isInit = false;
    public long chip
    {
        get; private set;
    }
    // Start is called before the first frame update
    private void Init()
    {
        chipPool.generator = () => {
            return Instantiate(chipObj, disableParent);  
        };
        chipPool.activator = (obj) =>
        {
            obj.SetActive(true);
            chipObjs.Add(obj);
        };
        chipPool.deactivator = (obj) => {
            obj.SetActive(false);
            chipObjs.Remove(obj);
            obj.transform.SetParent(disableParent);
        };
        chipPool.remover = (obj) => { Destroy(obj); };
        isInit = true;
    }

    public string GetMoneyString(long chip)
    {
        if (bettingManager != null)
            return bettingManager.GetMoneyString(chip);
        else if (player != null)
            return player.GetMoneyString(chip);
        else
            return string.Format("{0:#,##0.##}", chip);
    }

    public void SetChip(long chip)
    {
        if(!isInit)
        {
            Init();
        }
        this.chip = chip;
        chipText.SetChip(chip);//.text = GetMoneyString(chip);
        ChipObjsSet();
        gameObject.SetActive(chip > 0);
    }

    public void ChipObjsSet()
    {
        ChipReset();
        var chipData = this.chipData.MoneyToChipOfCount(chip);
        var middle = (chipData.Count-1) / 2f;
        for(int i = 0; i < chipData.Count; i++)
        {
            var data = chipData[i];
            var sp = data.data.sprite;
            Transform container = null;
            if(i<chipContainers.Count)
            {
                container = chipContainers[i];
            }
            else
            {
                container = Instantiate(chipContainer, chipContainerParnet);
                chipContainers.Add(container);
            }
            var pos = container.transform.localPosition;
            pos.x = (i - middle) * spacing;
            container.transform.localPosition = pos;
            for(int j = 0; j < data.count; j++)
            {
                var chip = chipPool.GetObject();
                chip.transform.SetParent(container);
                chip.GetComponentInChildren<Image>().sprite = sp;
            }
        }
        
    }
    public void ChipReset()
    {
       while(chipObjs.Count > 0)
        {
            chipPool.ReturnObject(chipObjs[0]);
        }
    }

    public void MoveChip(Vector3 targetPos, float time, UnityAction callback = null)
    {

        if (gameObject.activeSelf)
        {
            transform.DOMove(targetPos,time).onComplete = ()=>
            {
                transform.localPosition = Vector3.zero;
                ChipReset();
                gameObject.SetActive(false);
                callback?.Invoke();
            };
            //MoveChipCo(targetPos, time, callback);
        }
        else
            callback?.Invoke();
    }
    public IEnumerator MoveChipCo(Vector3 targetPos, float time, UnityAction callback = null)
    {
        var from = transform.parent.position;
        var timer = 0f;
        while(timer<time)
        {
            yield return 0;
            timer += Time.deltaTime;
            Vector3 pos = Vector3.Lerp(from, targetPos, timer/time);
            transform.position = pos;

        }
        transform.localPosition = Vector3.zero;
        gameObject.SetActive(false);
        callback?.Invoke();
    }
}
