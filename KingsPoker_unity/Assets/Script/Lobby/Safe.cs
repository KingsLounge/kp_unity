using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Safe : MonoBehaviour
{
    public Sprite chipOnSprite;
    public Sprite chipOffSprite;
    public Sprite goldOnSprite;
    public Sprite goldOffSprite;
    public Sprite tabOnSprite;
    public Sprite tabOffSprite;
    public Sprite safeOnImage;
    public Sprite safeOffImage;
    public Sprite inputOn;
    public Sprite inputOff;
    public Sprite outputOn;
    public Sprite outputOff;
    public Text currentChipText;
    public Text safeChipText;
    public Text currentGoldText;
    public Text safeGoldText;
    public Text inputText;
    public Image chipImage;
    public Image goldImage;
    public Image chipArrowImage;
    public Image goldArrowImage;
    public Image goldTabImage;
    public Image chipTabImage;
    public Image chipSafeImage;
    public Image goldSafeImage;
    public GameObject inputButton;
    public GameObject outputButton;
    private bool isChip = true;
    private bool input = true;

    private string type;
    private long point;
    private long changePoint;

    // public void TestIn()
    // {
    //     SafeIn("gold", 100000);
    // }

    // public void TestOut()
    // {
    //     SafeOut("gold", 100000);
    // }
    private void OnEnable() {
        SetImage();
        SetText();
    }
    public void SafeIn()
    {
        changePoint = point;
        PublisherApiManager.Instance.RequestSafeIn(type, changePoint, SafeInCallback);
        point = 0;
        SetInputText();
    }

    public void SafeOut()
    {
        changePoint = point;
        PublisherApiManager.Instance.RequestSafeOut(type, changePoint, SafeOutCallBack);
        point = 0;
        SetInputText();
    }

    public void SafeInCallback(long statusCode)
    {
        if(statusCode == 200)
        {
            Console.Log(string.Format("{0} 입금 완료 : {1}", type, point));
            if(type == "silver")
            {
                MyStatus.zc -= changePoint;
                MyStatus.safeSc += changePoint;
            }
            else if(type == "gold")
            {
                MyStatus.dc -= changePoint;
                MyStatus.safeGc += changePoint;
            }
            SetText();
        }
        else
        {
            Console.Error("입금 실패");
        }
    }

    public void SafeOutCallBack(long statusCode)
    {
        if (statusCode == 200)
        {
            Console.Log(string.Format("{0} 출금 완료 : {1}", type, point));
            if(type == "silver")
            {
                MyStatus.zc += changePoint;
                MyStatus.safeSc -= changePoint;
            }
            else if(type == "gold")
            {
                MyStatus.dc += changePoint;
                MyStatus.safeGc -= changePoint;
            }
            changePoint = 0;
            SetText();
        }
        else
        {
            Console.Error("출금 실패");
        }
    }

    public void Input()
    {
        input = true;
        SetImage();
    }

    public void Output()
    {
        input = false;
        SetImage();
    }

    public void OnChip()
    {
        isChip = true;
        SetImage();
    }

    public void OnGold()
    {
        isChip = false;
        SetImage();
    }

    public void InputChip()
    {
        isChip = true;
        input = true;
        SetImage();
    }
    public void OutputChip()
    {
        isChip = true;
        input = false;
        SetImage();
    }

    public void InputGold()
    {
        isChip = false;
        input = true;
        SetImage();
    }

    public void OutPutGold()
    {
        isChip = false;
        input = false;
        SetImage();
    }

    public void SetText()
    {
        currentChipText.text = MoneyToString.Converting(MyStatus.zc);
        currentGoldText.text = MoneyToString.Converting(MyStatus.dc);
        safeChipText.text = MoneyToString.Converting(MyStatus.safeSc);
        safeGoldText.text = MoneyToString.Converting(MyStatus.safeGc);
        LobbyManager.Instance.UpdateChipText();
        LobbyManager.Instance.SetSafeText();
    }

    public void SetImage()
    {
        chipSafeImage.sprite = isChip ? safeOnImage : safeOffImage;
        goldSafeImage.sprite = isChip ? safeOffImage : safeOnImage;
        chipTabImage.sprite = isChip ? tabOnSprite : tabOffSprite;
        goldTabImage.sprite = isChip ? tabOffSprite : tabOnSprite;
        chipArrowImage.sprite = isChip ? (input? inputOn : outputOn):(input?inputOff:outputOff);
        goldArrowImage.sprite = isChip ? (input? inputOff : outputOff):(input?inputOn:outputOn);
        chipImage.sprite = isChip ? chipOnSprite : chipOffSprite;
        goldImage.sprite = isChip ? goldOffSprite : goldOnSprite;
        currentChipText.color = isChip?Color.white:Color.gray;
        safeChipText.color = isChip?Color.white:Color.gray;
        currentGoldText.color = isChip?Color.gray:Color.white;
        safeGoldText.color = isChip?Color.gray:Color.white;
        inputButton.SetActive(input);
        outputButton.SetActive(!input);
        point = 0;
        type = isChip? "silver" : "gold";
        SetInputText();
    }

    public void SetInputText()
    {
        inputText.text = MoneyToString.Converting(point);
    }

    public void NumberButton(int num)
    {
        point = point*10+num;
        var max = isChip?(input? MyStatus.zc:MyStatus.safeSc):(input?MyStatus.dc:MyStatus.safeGc);
        point = point>max?max:point;
        SetInputText();
    }

    public void ZeroZeroButton()
    {
        point *= 100;
        var max = isChip?(input? MyStatus.zc:MyStatus.safeSc):(input?MyStatus.dc:MyStatus.safeGc);
        point = point>max?max:point;
        SetInputText();
    }

    public void BackButton()
    {
        point /= 10;
        SetInputText();
    }

    public void ResetButton()
    {
        point = 0;
        SetInputText();
    }
    public void MaxButton()
    {
        point = isChip?(input? MyStatus.zc:MyStatus.safeSc):(input?MyStatus.dc:MyStatus.safeGc);
        SetInputText();
    }
    

   
}
