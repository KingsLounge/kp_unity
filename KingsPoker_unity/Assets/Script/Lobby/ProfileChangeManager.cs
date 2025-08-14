using System.Collections;
using System.Collections.Generic;
using Constant;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class ProfileChangeManager : MonoBehaviour
{
    [SerializeField]
    private GameObject buttonTemplate;

    [SerializeField]
    private Transform container;

    [SerializeField]
    private SpriteAtlas characterAtlas;

    private List<GameObject> slots = new List<GameObject>();

    public void Awake()
    {
        init();
        InfoManager.AddValueChangeLisener(OnValueChange);
    }

    public void OnValueChange(string dataName)
    {
        switch (dataName)
        {
            case "myItems":
                init();
                break;
        }
    }

    private async void init()
    {
        var table = TableDataManager.characterTable;
        var inven = InfoManager.MyItems;
        int count = 0;
        if (GameConfig.USE_PHOTO_URL && !string.IsNullOrEmpty(MyStatus.photoURL))
        {
            GameObject go = null;
            if (count < slots.Count)
            {
                go = slots[count];
            }
            else
            {
                go = Instantiate(buttonTemplate);
                slots.Add(go);
                go.transform.SetParent(container);
                go.transform.CleanIdentity();
                go.SetActive(true);
            }

            Texture2D texture = await ImageDatabase.LoadImageTexture(
                MyStatus.photoURL,
                Application.persistentDataPath + "/profileImg",
                MyStatus.gid
            );
            if (texture)
            {
                Rect rect = new Rect(0, 0, texture.width, texture.height);
                Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
                sprite.name = texture.name;
                go.GetComponentInChildren<Image>().sprite = sprite;
                go.GetComponentInChildren<Button>().onClick.AddListener(OnClickUrlProfileButton);
            }
            count++;
        }
        foreach (var data in table)
        {
            GameObject go = null;
            if (count < slots.Count)
            {
                go = slots[count];
            }
            else
            {
                go = Instantiate(buttonTemplate);
                slots.Add(go);
                go.transform.SetParent(container);
                go.transform.CleanIdentity();
                go.SetActive(true);
            }

            var value = data.Value;
            Sprite sprite = characterAtlas.GetSprite(value.ValueOrDefault("2d_index", ""));
            bool has = false;
            var refIdx = value.ValueOrDefault("item_id", 0);
            for (int j = 0; j < inven.Count; j++)
            {
                if (refIdx == inven[j]["ref_idx"].ToObject<int>())
                {
                    has = true;
                    break;
                }
            }

            go.GetComponentInChildren<Image>().sprite = sprite;
            var button = go.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                OnClickProfileChangeButton(data.Value.ValueOrDefault("item_id", 0));
            });
            button.interactable = has;

            count++;
        }
    }

    public void OnClickUrlProfileButton()
    {
        StartCoroutine(
            SetUseUrlProfile(
                true,
                async () =>
                {
                    Texture2D texture = await ImageDatabase.LoadImageTexture(
                        MyStatus.photoURL,
                        Application.persistentDataPath + "/profileImg",
                        MyStatus.gid
                    );
                    LobbyManager.Instance.SetProfile(texture);
                }
            )
        );
    }

    public void OnClickProfileChangeButton(int idx)
    {
        PublisherApiManager.Instance.EquipItemAPI(
            (int)idx,
            (System.Action<long>)(
                async (statusCode) =>
                {
                    if (statusCode == 200)
                    {
                        if (!MyStatus.usePhotoURL)
                        {
                            Sprite sprite = null;
                            var table = TableDataManager.characterTable;
                            foreach (var data in table)
                            {
                                if (data.Value.ValueOrDefault((string)"item_id", (int)0) == idx)
                                    sprite = characterAtlas.GetSprite(
                                        (string)
                                            data.Value.ValueOrDefault(
                                                (string)"2d_index",
                                                (string)""
                                            )
                                    );
                            }
                            LobbyManager.Instance.SetProfile((Sprite)sprite);
                        }
                        // await UniTask.WaitForSeconds(0.1f);
                        // InfoManager.Instance.GetMyItems();
                    }
                    else { }
                }
            )
        );
        // StartCoroutine(
        //     SetUseUrlProfile(
        //         false,
        //         () =>
        //         {
        //             PublisherApiManager.Instance.EquipItemAPI(
        //                 (int)idx,
        //                 (System.Action<long>)(
        //                     async (statusCode) =>
        //                     {
        //                         if (statusCode == 200)
        //                         {
        //                             if (!MyStatus.usePhotoURL)
        //                             {
        //                                 Sprite sprite = null;
        //                                 var table = TableDataManager.characterTable;
        //                                 foreach (var data in table)
        //                                 {
        //                                     if (
        //                                         data.Value.ValueOrDefault((string)"item_id", (int)0)
        //                                         == idx
        //                                     )
        //                                         sprite = characterAtlas.GetSprite(
        //                                             (string)
        //                                                 data.Value.ValueOrDefault(
        //                                                     (string)"2d_index",
        //                                                     (string)""
        //                                                 )
        //                                         );
        //                                 }
        //                                 LobbyManager.Instance.SetProfile((Sprite)sprite);
        //                             }
        //                             // await UniTask.WaitForSeconds(0.1f);
        //                             // InfoManager.Instance.GetMyItems();
        //                         }
        //                         else { }
        //                     }
        //                 )
        //             );
        //         }
        //     )
        //);
    }

    private IEnumerator SetUseUrlProfile(
        bool use,
        System.Action success = null,
        System.Action fail = null
    )
    {
        Packet packet = new Packet((int)CPProtocol.CP_USER_INFO_OPTION_UPDATE);
        JObject updateData = new JObject();
        updateData.Add("usePhoto", use);
        packet.Add("updateData", updateData);
        WebSocketManager.defaultCli.Send(packet.ToJson());
        WaitForPCProtocol wait = new WaitForPCProtocol(
            (p, c) =>
            {
                return p == PCProtocol.PC_USER_INFO_OPTION_UPDATE
                    || p == PCProtocol.PC_USER_INFO_OPTION_UPDATE_FAIL;
            }
        );
        yield return wait;
        if (wait.Result.p == (int)PCProtocol.PC_USER_INFO_OPTION_UPDATE)
        {
            MyStatus.usePhotoURL = (wait.Result.c["option"] as JObject).ValueOrDefault(
                "usePhoto",
                false
            );
            success?.Invoke();
        }
        else
            fail?.Invoke();
    }
}
