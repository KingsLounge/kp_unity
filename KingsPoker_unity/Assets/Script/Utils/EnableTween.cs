using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using UnityEngine.Events;

public class EnableTween : MonoBehaviour
{
    [SerializeReference]
    private List<TweenAni> tweenAnimations = new List<TweenAni>();
    private int currentAnimationIndex = 0;
    
    public void OnEnable()
    {
        PlayAni();
    }

    public void PlayAni()
    {
        if (tweenAnimations.Count == 0) return;
        
        currentAnimationIndex = 0;
        PlayNextAnimation();
    }

    private void PlayNextAnimation()
    {
        if (currentAnimationIndex >= tweenAnimations.Count) return;

        var currentAni = tweenAnimations[currentAnimationIndex];
        
        // 원래 endAction을 임시 저장
        UnityEvent originalEndAction = currentAni.endAction ?? new UnityEvent();
        
        // 새로운 endAction 생성 및 기존 이벤트 복사
        currentAni.endAction = new UnityEvent();
        for (int i = 0; i < originalEndAction.GetPersistentEventCount(); i++)
        {
            UnityEngine.Object target = originalEndAction.GetPersistentTarget(i);
            string methodName = originalEndAction.GetPersistentMethodName(i);
            UnityAction action = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), target, methodName);
            currentAni.endAction.AddListener(action);
        }
        
        // 다음 애니메이션 실행을 위한 콜백 추가
        currentAni.endAction.AddListener(() => {
            currentAnimationIndex++;
            PlayNextAnimation();
        });
        
        currentAni.Play();
    }

    [ContextMenu("Add Scale Animation")]
    private void AddScaleAnimation()
    {
        if (tweenAnimations == null)
            tweenAnimations = new List<TweenAni>();
        tweenAnimations.Add(new ScaleAni());
    }

    [ContextMenu("Add Rect Move Animation")]
    private void AddRectMoveAnimation()
    {
        if (tweenAnimations == null)
            tweenAnimations = new List<TweenAni>();
        tweenAnimations.Add(new RectMoveAni());
    }

    [System.Serializable]
    public class TweenAni
    {
        public UnityEvent startAction;
        public UnityEvent endAction;
        
        public float duration;
        public float delay; 
        public virtual void Kill(){}
        public virtual void Play(){}
        
    }

    [System.Serializable]
    public class ScaleAni:TweenAni
    {
        public RectTransform startPosition;
        public RectTransform target;
        public Vector3 from;
        public Vector3 to;
        public TweenerCore<Vector3, Vector3, VectorOptions> scaleCore;
        public Ease ease;
        public override void Kill()
        {
            scaleCore.Kill();
        }
        public override void Play()
        {
            target.sizeDelta = startPosition.sizeDelta;
            target.localScale = from;
            target.position = startPosition.position;
            scaleCore = target.DOScale(to, duration).SetEase(ease).SetDelay(delay).OnStart(() => startAction?.Invoke()).OnComplete(() => endAction?.Invoke());
        }
       
    }

    [System.Serializable]
    public class RectMoveAni:TweenAni
    {
        public TweenerCore<Vector3, Vector3, VectorOptions> moveCore;
        public TweenerCore<Quaternion, Quaternion, NoOptions> rotateCore;
        public TweenerCore<Vector2, Vector2, VectorOptions> sizeCore;
        public Vector3 rotationVector;

        public override void Kill()
        {
            moveCore.Kill();
            rotateCore.Kill();
            sizeCore.Kill();
        }
        public RectTransform from;
        public RectTransform to;
        public RectTransform target;
        public Ease moveEase;
        public Ease sizeEase;
        public Ease rotationEase;
        
        


        public override void Play()
        {
            target.sizeDelta = from.sizeDelta;
            target.rotation = from.rotation;
            target.position = from.position;
            var cardParent = target.parent;
            
            moveCore = DOTween.To(() => cardParent.InverseTransformPoint(from.position), (value) => target.localPosition = value, cardParent.InverseTransformPoint(to.position), duration).SetEase(moveEase).SetDelay(delay).OnStart(() => { target.gameObject.SetActive(true); target.SetAsLastSibling(); startAction?.Invoke(); });
            //cardTransform.DOMove(targetRect.position, duration).SetEase(moveEase).SetDelay(delay);




            var targetSaver = to.GetComponent<CardRotationSaver>();
            var targetRotVector = targetSaver ? targetSaver.rotation : to.rotation.eulerAngles;
            var startSaver = from.GetComponent<CardRotationSaver>();
            var startRotVector = startSaver ? startSaver.rotation : from.rotation.eulerAngles;


            //cardAni.rotateCore = DOTween.To(()=> cardAni.rotationVector, value => { cardTransform.rotation = Quaternion.Euler(value); cardAni.rotationVector = value; Debug.Log($"card rotation {value}"); }, targetRect.rotation.eulerAngles, duration);
            //DOTween.To(() => startRotVector.z, (value) => { cardAni.rotationVector.z = value; cardTransform.rotation = Quaternion.Euler(cardAni.rotationVector); }, targetRotVector.z, duration).SetEase(ZrotationEase).SetDelay(delay);
            //DOTween.To(() => startRotVector.x, (value) => { cardAni.rotationVector.x = value; cardTransform.rotation = Quaternion.Euler(cardAni.rotationVector); }, targetRotVector.x, duration).SetEase(XrotationEase).SetDelay(delay);
            //DOTween.To(() => startRotVector.y, (value) => { cardAni.rotationVector.y = value; cardTransform.rotation = Quaternion.Euler(cardAni.rotationVector); }, targetRotVector.y, duration).SetEase(YrotationEase).SetDelay(delay);
            rotateCore = target.DORotateQuaternion(to.rotation, duration).SetEase(rotationEase).SetDelay(delay);
            var targetSize = to.sizeDelta;
            targetSize.x *= (to.lossyScale.x / from.lossyScale.x);
            targetSize.y *= (to.lossyScale.y / from.lossyScale.y);

            sizeCore = target.DOSizeDelta(targetSize, duration).SetEase(sizeEase).SetDelay(delay);

            var obj = target.gameObject;
            moveCore.onPlay = () => { obj.SetActive(true); 
                //SoundManager.Instance.PlayEffectSound(Sound_TableEnum.SFX_CARD_THROW);
            };
            //cardAni.moveCore.onComplete = action;
            moveCore.onKill = () => endAction?.Invoke();

            //cardAni.moveCore.onComplete += ()=> tweenAnis.Remove(cardAni);
            
            moveCore.onComplete += () => Kill();    
        }
    }
}
