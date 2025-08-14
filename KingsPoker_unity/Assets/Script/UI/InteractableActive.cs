using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


[ExecuteAlways]
public class InteractableActive : MonoBehaviour
{
    private Button button;
    private bool prevInteractable;
    public List<GameObject> childs = new List<GameObject>();
    public void Awake() {
        button = GetComponent<Button>();
        prevInteractable = !button.interactable;
    }
    public void Update() {
        if(prevInteractable != button.interactable) {
            for(int i = 0; i < childs.Count; i ++) {
                childs[i].SetActive(button.interactable);
            }
            prevInteractable = button.interactable;
        }
    }
}
