using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugGui : MonoBehaviour {

    [SerializeField] GameObject doorLF, doorLR, doorRF, doorRR;

    private void OnGUI() {
        GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = 30;
        //GUI.Box(new Rect(100, 50, 400, 50), string.Format("{0:0.000}", bonnetL.transform.rotation.eulerAngles));
        //GUI.Box(new Rect(100, 100, 400, 50), string.Format("{0:0.000}", bonnetR.transform.rotation.eulerAngles));
        //GUI.Box(new Rect(100, 150, 400, 50), string.Format("{0:0.000}", canvasPreview.activeSelf ? previewStartPos + touchDetector.SwipeDelta : Vector2.zero));
    }

    private void Start() {
        Debug.Log(doorLF.transform.localPosition + "\n" + doorLR.transform.localPosition + "\n" + doorRF.transform.localPosition + "\n" + doorRR.transform.localPosition);
    }
}
