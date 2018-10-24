using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GalleryTileBehaviour : MonoBehaviour, IPointerClickHandler {
    public int imageId;

    public void OnPointerClick(PointerEventData eventData) {
        Debug.Log("Open gallery preview no : " + imageId);
        GameObject.Find("Core Controller").GetComponent<CoreController>().openGalleryPreview(imageId);
    }
}
