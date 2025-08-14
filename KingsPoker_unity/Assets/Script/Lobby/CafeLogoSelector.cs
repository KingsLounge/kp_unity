using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CafeLogoSelector : MonoBehaviour
{
    public int selected
    {
        get;private set;
    }
    public bool usePhoto
    {
        get;private set;
    }
    public Texture2D photo
    {
        get;private set;
    }

    [SerializeField]
    private CafeLogoData logoList;
    private Sprite[] logos {
        get
        {
            return logoList.sprites;
        }
    }
    [SerializeField]
    private GameObject buttonTemplate;
    [SerializeField]
    private Transform container;
    [SerializeField]
    private Image preview;
    public System.Action<Sprite> onSelected;
#if UNITY_EDITOR
    //유니티 에디터에서는 이미지 업로더가 없으므로 테스트 코드
    public Texture2D testUpload;
    public bool testSuccess = true;
#endif

    private void Awake()
    {
        if(logos != null)
        {
            for (int i = 0; i < logos.Length; i++)
            {
                Sprite sprite = logos[i];
                GameObject go = Instantiate(buttonTemplate);
                go.GetComponentInChildren<Image>().sprite = sprite;
                int idx = i + 1;
                go.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    OnClickSetLogoButton(idx);
                });
                go.transform.SetParent(container);
                go.transform.CleanIdentity();
                go.SetActive(true);
            }
        }
    }

    public async void SetPhoto(string url, string filename)
    {
        photo = await ImageDatabase.LoadImageTexture(url, Application.persistentDataPath + "/cafeLogo", filename);
        usePhoto = true;
        selected = 0;
        Sprite sprite = photo.ToSprite();
        preview.sprite = sprite;
        onSelected.Invoke(sprite);
    }

    public void OnClickImageUploadButton()
    {
#if UNITY_EDITOR
        //유니티 에디터에서는 이미지 업로더가 없으므로 테스트 코드
        usePhoto = testSuccess;
        photo = testUpload;
        if (testSuccess)
        {
            selected = 0;
            Sprite sprite = photo.ToSprite();
            preview.sprite = sprite;
            onSelected.Invoke(sprite);
        }
        return;
#endif
        ImageUploader.Instance.BrowseImage(preview.rectTransform.sizeDelta,(complete,texture)=> {
            usePhoto = complete;
            photo = texture;
            if(complete)
            {
                selected = 0;
                Rect rect = new Rect(0, 0, texture.width, texture.height);
                Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
                sprite.name = texture.name;
                preview.sprite = sprite;
                onSelected.Invoke(sprite);
            }
        });
    }

    public void RandomPick()
    {
        OnClickSetLogoButton(Random.Range(0, logos.Length) + 1);
    }

    public void OnClickSetLogoButton(int idx)
    {
        selected = idx;
        usePhoto = false;
        Sprite sprite = logos[idx - 1];
        preview.sprite = sprite;
        onSelected.Invoke(sprite);
    }
}
