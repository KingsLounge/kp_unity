using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;


public class CardAnimationManager : MonoBehaviour
{

    private List<CardAni> tweenAnis = new List<CardAni>();

    public Ease moveEase = Ease.OutExpo;
    public Ease ZrotationEase = Ease.OutExpo;
    public Ease rotationEase = Ease.OutExpo;
    public Ease XrotationEase = Ease.InQuad;
    public Ease YrotationEase = Ease.InQuad;
    public Ease sizeEase = Ease.InQuad;

    public void CardMove(RectTransform cardTransform, RectTransform startRect, RectTransform targetRect, float delay, float duration = 0.5f, TweenCallback endAction = null, TweenCallback startAction = null)
    {

        //cardTransform.SetPositionAndRotation(startRect.position, startRect.rotation);
        cardTransform.sizeDelta = startRect.sizeDelta;
        var cardParent = cardTransform.parent;
        var cardAni = new CardAni();
        cardAni.moveCore = DOTween.To(() => cardParent.InverseTransformPoint(startRect.position), (value) => cardTransform.localPosition = value, cardParent.InverseTransformPoint(targetRect.position), duration).SetEase(moveEase).SetDelay(delay).OnStart(() => { cardTransform.gameObject.SetActive(true); cardTransform.SetAsLastSibling(); startAction?.Invoke(); });
        //cardTransform.DOMove(targetRect.position, duration).SetEase(moveEase).SetDelay(delay);




        var targetSaver = targetRect.GetComponent<CardRotationSaver>();
        var targetRotVector = targetSaver ? targetSaver.rotation : targetRect.rotation.eulerAngles;
        var startSaver = startRect.GetComponent<CardRotationSaver>();
        var startRotVector = startSaver ? startSaver.rotation : startRect.rotation.eulerAngles;


        //cardAni.rotateCore = DOTween.To(()=> cardAni.rotationVector, value => { cardTransform.rotation = Quaternion.Euler(value); cardAni.rotationVector = value; Debug.Log($"card rotation {value}"); }, targetRect.rotation.eulerAngles, duration);
        //DOTween.To(() => startRotVector.z, (value) => { cardAni.rotationVector.z = value; cardTransform.rotation = Quaternion.Euler(cardAni.rotationVector); }, targetRotVector.z, duration).SetEase(ZrotationEase).SetDelay(delay);
        //DOTween.To(() => startRotVector.x, (value) => { cardAni.rotationVector.x = value; cardTransform.rotation = Quaternion.Euler(cardAni.rotationVector); }, targetRotVector.x, duration).SetEase(XrotationEase).SetDelay(delay);
        //DOTween.To(() => startRotVector.y, (value) => { cardAni.rotationVector.y = value; cardTransform.rotation = Quaternion.Euler(cardAni.rotationVector); }, targetRotVector.y, duration).SetEase(YrotationEase).SetDelay(delay);
        cardAni.rotateCore = cardTransform.DORotateQuaternion(targetRect.rotation, duration).SetEase(rotationEase).SetDelay(delay);
        var targetSize = targetRect.sizeDelta;
        targetSize.x *= (targetRect.lossyScale.x / startRect.lossyScale.x);
        targetSize.y *= (targetRect.lossyScale.y / startRect.lossyScale.y);

        cardAni.sizeCore = cardTransform.DOSizeDelta(targetSize, duration).SetEase(sizeEase).SetDelay(delay);

        var obj = cardTransform.gameObject;
        cardAni.moveCore.onPlay = () => { obj.SetActive(true); 
            //SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_CARD_THROW);
         };
        //cardAni.moveCore.onComplete = action;
        cardAni.moveCore.onKill = endAction;

        //cardAni.moveCore.onComplete += ()=> tweenAnis.Remove(cardAni);
        cardAni.moveCore.onKill += () => tweenAnis.Remove(cardAni);

        cardAni.moveCore.onComplete += () => cardAni.Kill();

        tweenAnis.Add(cardAni);
    }

    public void KillAnimations()
    {
        if (tweenAnis.Count > 0)
        {
            while(tweenAnis.Count > 0)
            {
                tweenAnis[0].Kill();
            }
        }
    }

    public struct CardAni
    {
        public TweenerCore<Vector3, Vector3, VectorOptions> moveCore;
        public TweenerCore<Quaternion, Quaternion, NoOptions> rotateCore;
        public TweenerCore<Vector2, Vector2, VectorOptions> sizeCore;
        public Vector3 rotationVector;

        public void Kill()
        {
            moveCore.Kill();
            rotateCore.Kill();
            sizeCore.Kill();
        }
    }
}
