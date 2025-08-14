using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

[System.Serializable]
public class EmoticonInfo
{
    public Emoticon key = Emoticon.none;
    public bool moveable = false;
    public bool includeList = true;
    public Sprite iconTexture;
    public string description = "";
    public string soundEnumKey = "";

    [HideInInspectorWithCondition("moveable",true)]
    public Sprite texture;

    [HideInInspectorWithCondition("moveable", true)]
    public float playTime;

    [HideInInspectorWithCondition("moveable")]
    public SkeletonAnimation spine;

    [HideInInspectorWithCondition("moveable")]
    public string startAnim;

    [HideInInspectorWithCondition("moveable")]
    public string moveAnim;

    [HideInInspectorWithCondition("moveable")]
    public string endAnim;

    [HideInInspectorWithCondition("moveable")]
    public float speedScale = 1f;
}

[CreateAssetMenu(fileName = "EmoticonData", menuName = "Scriptable Object/EmoticonData", order = int.MaxValue)]
public class EmoticonData : ScriptableObject
{
    public List<EmoticonInfo> emoticons = new List<EmoticonInfo>();

    public EmoticonInfo Find(Emoticon emoticon)
    {
        return emoticons.Find(i => i.key == emoticon);
    }

    public EmoticonInfo Find(string emoticon)
    {
        return Find((Emoticon)System.Enum.Parse(typeof(Emoticon), emoticon));
    }
}
