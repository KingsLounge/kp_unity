using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipContainerManager : MonoBehaviour
{
    private static ChipContainerManager instance;
    public static ChipContainerManager Instance {
        get {
            return instance;
        }
    }
    private RectTransform tr;
    public GameObject chipPrefab;
    public ChipDataList chipData;
    public float movePerSecond = 0.6f;
    
    // Start is called before the first frame update
    void Awake()
    {
        ChipContainerManager.instance = this;
        tr = GetComponent<RectTransform>();
    }
    
    public void Throw(Vector3 originPos,long money) {
        if (money == 0)
            return;
        List<ChipData> datas = chipData.MoneyToChip(money);
        for(int i = 0; i < datas.Count; i++) {
            GameObject go = Instantiate(chipPrefab, transform);//transform.AddChild(chipPrefab);
            go.transform.localScale = chipPrefab.transform.localScale;
            go.transform.position = originPos;
            Vector3 target = transform.position;
            target.x += tr.sizeDelta.x * Random.Range(-0.5f,0.5f);
            target.y += tr.sizeDelta.y * Random.Range(-0.5f,0.5f);
            Chip chip = go.GetComponent<Chip>();
            chip.SetChip(datas[i].sprite,datas[i].isCircle);
            chip.Throw(target);
        }
    }

    public void Clear() {
        //transform.DestroyChildren();
        for(int i = transform.childCount-1; i >= 0 ; i--)
        {
            var child = transform.GetChild(i).gameObject;
            Destroy(child);
        }    
    }

    public void MoveAndClear(Vector3 position) {
        StartCoroutine(MoveAndClearCoroutine(position));
    }
    
    private IEnumerator MoveAndClearCoroutine(Vector3 position) {
        Vector3 originPos = transform.position;
        float dis = 0;
        while(true) {
            dis += Time.deltaTime / movePerSecond;
            if(dis >= 1f)
                dis = 1f;
            transform.position = Vector3.Lerp(originPos,position,dis);
            float scale = 0.3f + (0.6f * (1f - dis));
            transform.localScale = new Vector2(scale, scale);
            if(dis >= 1f)
                break;
            yield return null;
        }
        Clear();
        transform.position = originPos;
        transform.localScale = new Vector2(1f,1f);
    }
}
