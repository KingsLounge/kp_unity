using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chip : MonoBehaviour
{

    private Rigidbody2D rigid;
    private BoxCollider2D boxCol;
    private CircleCollider2D circleCol;
    private Image image;
    private RectTransform rect;
    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        boxCol = GetComponent<BoxCollider2D>();
        circleCol = GetComponent<CircleCollider2D>();
        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        
    }

    public void Throw(Vector3 targetPos) {
        Vector3 pos = targetPos - transform.position;
        rigid.velocity = pos.normalized * pos.magnitude * rigid.drag;
        StartCoroutine(ColliderDisable(3.0f * Random.Range(0.5f,1f)));
    }

    private IEnumerator ColliderDisable(float delay) {
        yield return new WaitForSeconds(delay);
        this.boxCol.enabled = false;
        this.circleCol.enabled = false;
    }

    public void SetChip(Sprite texture,bool isCircle) {
        circleCol.enabled = isCircle;
        boxCol.enabled = !isCircle;
        image.sprite = texture;
        image.SetNativeSize();
    }
}
