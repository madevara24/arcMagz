using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ObjectCloudCloudBehaviour : MonoBehaviour {

    [SerializeField] TouchDetector touchDetector;
    [SerializeField] Camera arCamera;

    #region Enum Event
    public const string EVENT_INTERACTIVE_CLOUD_OPEN_INTERFACE = "EVENT_INTERACTIVE_CLOUD_OPEN_INTERFACE";
    public const string EVENT_INTERACTIVE_CLOUD_DETECTED = "EVENT_INTERACTIVE_CLOUD_DETECTED";
    public const string EVENT_INTERACTIVE_CLOUD_LOST = "EVENT_INTERACTIVE_CLOUD_LOST";
    #endregion
        
    #region Interactive Cloud Variables
    [SerializeField] GameObject canvasInteractiveCloud;
    [SerializeField] Image interactiveCloudBackgroundImage;
    [SerializeField] Image imgBottomBar, imgBottomBarDonut, imgBottomBarIcon, imgBottomBarNext, imgBottomBarPrev;
    [SerializeField] Image imgArrowPartInspection, imgArrowCarPaint, imgArrowTransform, imgArrowAnimation;
    [SerializeField] Image imgAnimIconBonnet, imgAnimIconDoor, imgAnimIconRoof;
    [SerializeField] Sprite spriteAnimIconOn, spriteAnimIconOff, spriteIconCarPaint, spriteIconPartInspection;
    [SerializeField] Button btnPartInspection, btnCarPaint, btnTransform, btnAnimation;
    [SerializeField] Button btnAnimRoof, btnAnimDoor, btnAnimBonnet;
    [SerializeField] Button btnBottomBarNext, btnBottomBarPrev;
    [SerializeField] Text txtInspectPartName;

    [SerializeField] GameObject interactiveCloudObject, doorLF, doorRF, doorLR, doorRR, bonnetR, bonnetL, rotateGuideObject;

    [SerializeField] List<Sprite> listOfInteractiveCloudPaintWidgetSprite;
    [SerializeField] List<Image> listOfInteractiveCloudPaintWidgetImage;
    [SerializeField] List<Material> listOfInteractiveCloudMaterials;
    [SerializeField] List<GameObject> listOfPaintedCarParts;
    [SerializeField] List<Vector3> listOfInspectPartRotationEuler;
    [SerializeField] List<string> listofInspectPartName;

    public enum InteractiveCloudViewMode { Animation, CarPaint, Transform, PartInspection, Default }
    private InteractiveCloudViewMode interactiveCloudViewMode = InteractiveCloudViewMode.Default;
    private bool isDoorClosed = true, isBonnetClosed = true, isRoofClosed = true, isAnimationFinished = true;
    private int triggerRoofOpenHash = Animator.StringToHash("triggerRoofOpen"), triggerRoofCloseHash = Animator.StringToHash("triggerRoofClose");
    private Vector3 interactiveCloudStartScale, interactiveCloudDefaultScale, interactiveCloudStartEuler;
    
    private int interactiveCloudPaintId = 4, interactiveCloudInspetPartId = 0;

    public bool isInteractiveCloudMarkerDetected = false;
    #endregion
    
    // Use this for initialization
    void Start () {
        EventManager.StartListening(EVENT_INTERACTIVE_CLOUD_DETECTED, onDetectInteractiveCloud);
        EventManager.StartListening(EVENT_INTERACTIVE_CLOUD_LOST, onLostInteractiveCloud);
        EventManager.StartListening(EVENT_INTERACTIVE_CLOUD_OPEN_INTERFACE, openInteractiveCloudInterface);
    }
	
	// Update is called once per frame
	void Update () {

	}

    public void openInteractiveCloudInterface() {
        canvasInteractiveCloud.SetActive(true);
    }
    
    #region Interactive Cloud Functions
    private void onDetectInteractiveCloud() {
        isInteractiveCloudMarkerDetected = true;
        interactiveCloudDefaultScale = interactiveCloudStartScale = interactiveCloudObject.transform.localScale;
    }

    private void onLostInteractiveCloud() {
        isInteractiveCloudMarkerDetected = false;
    }

    private void updateInteractiveCloud() {
        if (interactiveCloudViewMode == InteractiveCloudViewMode.Transform) {
            interactiveCloudCheckSwipe();
        }
    }

    private void interactiveCloudCheckSwipe() {
        if (touchDetector.SwipeUp) {
            if (interactiveCloudStartScale.x * (1 + (Mathf.Abs(touchDetector.SwipeDelta.y) / 640)) < 2) {
                interactiveCloudObject.transform.localScale = interactiveCloudStartScale * (1 + (Mathf.Abs(touchDetector.SwipeDelta.y) / 640));
            }
        }
        if (touchDetector.SwipeDown) {
            if (interactiveCloudStartScale.x / (1 + (Mathf.Abs(touchDetector.SwipeDelta.y) / 640)) > 0.25f) {
                interactiveCloudObject.transform.localScale = interactiveCloudStartScale / (1 + (Mathf.Abs(touchDetector.SwipeDelta.y) / 640));
            }
        }
        if (touchDetector.SwipeRight) {
            interactiveCloudObject.transform.localRotation = Quaternion.Euler(new Vector3(interactiveCloudStartEuler.x, interactiveCloudStartEuler.y - (touchDetector.SwipeDelta.x / 2), interactiveCloudStartEuler.z));
        }
        if (touchDetector.SwipeLeft) {
            interactiveCloudObject.transform.localRotation = Quaternion.Euler(new Vector3(interactiveCloudStartEuler.x, interactiveCloudStartEuler.y - (touchDetector.SwipeDelta.x / 2), interactiveCloudStartEuler.z));
        }
        if (!touchDetector.IsDraging) {
            interactiveCloudStartScale = interactiveCloudObject.transform.localScale;
            interactiveCloudStartEuler = interactiveCloudObject.transform.localRotation.eulerAngles;
        }
    }

    private void interactiveCloudChangeTexture() {
        for (int i = 0; i < listOfPaintedCarParts.Count; i++) {
            listOfPaintedCarParts[i].GetComponent<Renderer>().material = listOfInteractiveCloudMaterials[interactiveCloudPaintId];
        }
    }

    public void onPressPartInspectionButton() {
        Debug.Log("Press Part Inspect");
        if (interactiveCloudViewMode == InteractiveCloudViewMode.Default) {
            imgBottomBarIcon.GetComponent<Image>().sprite = spriteIconPartInspection;
            changeInteractiveCloudViewMode(InteractiveCloudViewMode.PartInspection);
            imgArrowPartInspection.color = Color.cyan;
            imgArrowPartInspection.transform.localEulerAngles = new Vector3(0, 0, 180);
            interactiveCloudInspectPart(0);
            showInteractiveCloudBottomBar(true);
            showInteractiveCloudInspectPartName(true);
        }
        else {
            changeInteractiveCloudViewMode(InteractiveCloudViewMode.Default);
            imgArrowPartInspection.color = Color.white;
            imgArrowPartInspection.transform.localEulerAngles = Vector3.zero;
            showInteractiveCloudBottomBar(false);
            showInteractiveCloudInspectPartName(false);
        }
    }

    public void onPressCarPaintButton() {
        Debug.Log("Press Car Paint");
        if (interactiveCloudViewMode == InteractiveCloudViewMode.Default) {
            imgBottomBarIcon.GetComponent<Image>().sprite = spriteIconCarPaint;
            changeInteractiveCloudViewMode(InteractiveCloudViewMode.CarPaint);
            imgArrowCarPaint.color = Color.cyan;
            imgArrowCarPaint.transform.localEulerAngles = new Vector3(0, 0, 180);
            StartCoroutine(interactiveCloudSlideButton(btnCarPaint, 475, 0.3f, 0.3f));
            interactiveCloudUpdatePaintWidgetImage();
            showInteractiveCloudPaintWidgetImage(true);
            showInteractiveCloudBottomBar(true);
        }
        else {
            changeInteractiveCloudViewMode(InteractiveCloudViewMode.Default);
            imgArrowCarPaint.color = Color.white;
            imgArrowCarPaint.transform.localEulerAngles = Vector3.zero;
            StartCoroutine(interactiveCloudSlideButton(btnCarPaint, 377, 0.3f));
            showInteractiveCloudPaintWidgetImage(false);
            showInteractiveCloudBottomBar(false);
        }
    }

    public void onPressTransformButton() {
        Debug.Log("Press Transform");
        if (interactiveCloudViewMode == InteractiveCloudViewMode.Default) {
            changeInteractiveCloudViewMode(InteractiveCloudViewMode.Transform);
            imgArrowTransform.color = Color.cyan;
            imgArrowTransform.transform.localEulerAngles = new Vector3(0, 0, 180);
            StartCoroutine(interactiveCloudSlideButton(btnTransform, 475, 0.3f, 0.3f));
        }
        else {
            changeInteractiveCloudViewMode(InteractiveCloudViewMode.Default);
            imgArrowTransform.color = Color.white;
            imgArrowTransform.transform.localEulerAngles = Vector3.zero;
            StartCoroutine(interactiveCloudSlideButton(btnTransform, 279, 0.3f));
        }
    }

    public void onPressAnimationButton() {
        Debug.Log("Press Animation");
        if (interactiveCloudViewMode == InteractiveCloudViewMode.Default) {
            changeInteractiveCloudViewMode(InteractiveCloudViewMode.Animation);
            imgArrowAnimation.color = Color.cyan;
            imgArrowAnimation.transform.localEulerAngles = new Vector3(0, 0, 180);
            StartCoroutine(interactiveCloudSlideButton(btnAnimation, 475, 0.3f, 0.3f));
            StartCoroutine(showInteractiveCloudAnimButton(true));
        }
        else {
            changeInteractiveCloudViewMode(InteractiveCloudViewMode.Default, 0.6f);
            imgArrowAnimation.color = Color.white;
            imgArrowAnimation.transform.localEulerAngles = Vector3.zero;
            StartCoroutine(interactiveCloudSlideButton(btnAnimation, 181, 0.3f, 0.3f));
            StartCoroutine(showInteractiveCloudAnimButton(false));
        }
    }

    public void onPressInteractiveCloudNextPartButton() {
        Debug.Log("Next");
        if (interactiveCloudViewMode == InteractiveCloudViewMode.CarPaint) {
            changeInteractiveCloudPaintId(interactiveCloudPaintId + 1);
        }
        else if (interactiveCloudViewMode == InteractiveCloudViewMode.PartInspection) {
            changeInteractiveCloudPartInspectionId(true);
        }
    }

    public void onPressInteractiveCloudPrevPartButton() {
        Debug.Log("Prev");
        if (interactiveCloudViewMode == InteractiveCloudViewMode.CarPaint) {
            changeInteractiveCloudPaintId(interactiveCloudPaintId - 1);
        }
        else if (interactiveCloudViewMode == InteractiveCloudViewMode.PartInspection) {
            changeInteractiveCloudPartInspectionId(false);
        }
    }

    public void onPressAnimRoofButton() {
        interactiveCloudAnimRoof();
    }

    public void onPressAnimDoorButton() {
        if (isAnimationFinished) {
            isAnimationFinished = false;
            StartCoroutine(interactiveCloudAnimDoor());
            StartCoroutine(waitAnimationFinish(2.5f));
        }
    }

    public void onPressAnimBonnetButton() {
        if (isAnimationFinished) {
            isAnimationFinished = false;
            interactiveCloudAnimBonnet();
            StartCoroutine(waitAnimationFinish(2.5f));
        }
    }

    private void changeInteractiveCloudPartInspectionId(bool _next) {
        if (_next)
            interactiveCloudInspetPartId++;
        else
            interactiveCloudInspetPartId--;

        if (interactiveCloudInspetPartId < 0)
            interactiveCloudInspetPartId = listOfInspectPartRotationEuler.Count - 1;
        if (interactiveCloudInspetPartId > listOfInspectPartRotationEuler.Count - 1)
            interactiveCloudInspetPartId = 0;

        interactiveCloudInspectPart(interactiveCloudInspetPartId);
    }

    public void onPressCarPaintWidgetImage(int _id) {
        changeInteractiveCloudPaintId(_id + interactiveCloudPaintId - 3);
    }

    private void changeInteractiveCloudPaintId(int _id) {
        interactiveCloudPaintId = _id;

        if (interactiveCloudPaintId >= listOfInteractiveCloudMaterials.Count)
            interactiveCloudPaintId = interactiveCloudPaintId - listOfInteractiveCloudMaterials.Count;

        if (interactiveCloudPaintId < 0)
            interactiveCloudPaintId = interactiveCloudPaintId + listOfInteractiveCloudMaterials.Count - 1;

        interactiveCloudChangeTexture();
        interactiveCloudUpdatePaintWidgetImage();
    }

    private void showInteractiveCloudBottomBar(bool _show) {
        imgBottomBar.gameObject.SetActive(_show);
        btnBottomBarNext.gameObject.SetActive(_show);
        btnBottomBarPrev.gameObject.SetActive(_show);
    }

    private void showInteractiveCloudInspectPartName(bool _show) {
        txtInspectPartName.gameObject.SetActive(_show);
    }

    private void showInteractiveCloudPaintWidgetImage(bool _show) {
        for (int i = 0; i < listOfInteractiveCloudPaintWidgetImage.Count; i++) {
            listOfInteractiveCloudPaintWidgetImage[i].gameObject.SetActive(_show);
        }
    }

    private IEnumerator showInteractiveCloudAnimButton(bool _show) {
        if (_show) {
            yield return new WaitForSeconds(0.5f);
            btnAnimBonnet.gameObject.SetActive(_show);
            btnAnimDoor.gameObject.SetActive(_show);
            btnAnimRoof.gameObject.SetActive(_show);
            btnAnimBonnet.GetComponent<Image>().DOFade(0.3f, 0.3f);
            btnAnimDoor.GetComponent<Image>().DOFade(0.3f, 0.3f);
            btnAnimRoof.GetComponent<Image>().DOFade(0.3f, 0.3f);
        }
        else {
            yield return new WaitForSeconds(0f);
            btnAnimBonnet.GetComponent<Image>().DOFade(0, 0.2f);
            btnAnimDoor.GetComponent<Image>().DOFade(0, 0.2f);
            btnAnimRoof.GetComponent<Image>().DOFade(0, 0.2f);
            StartCoroutine(interactiveCloudActivateButton(btnAnimBonnet, false, 0.3f));
            StartCoroutine(interactiveCloudActivateButton(btnAnimDoor, false, 0.3f));
            StartCoroutine(interactiveCloudActivateButton(btnAnimRoof, false, 0.3f));
        }
    }

    private void interactiveCloudUpdatePaintWidgetImage() {
        int paintId = interactiveCloudPaintId - 3;
        Debug.Log("Paint ID : " + interactiveCloudPaintId);
        for (int i = 0; i < listOfInteractiveCloudPaintWidgetImage.Count; i++) {
            if (paintId + i < 0) {
                changeInteractiveCloudWidgetCarPaintImageSprite(listOfInteractiveCloudPaintWidgetImage[i], paintId + i + listOfInteractiveCloudMaterials.Count);
            }
            else if (paintId + i > listOfInteractiveCloudMaterials.Count - 1) {
                changeInteractiveCloudWidgetCarPaintImageSprite(listOfInteractiveCloudPaintWidgetImage[i], paintId + i - listOfInteractiveCloudMaterials.Count);
            }
            else {
                changeInteractiveCloudWidgetCarPaintImageSprite(listOfInteractiveCloudPaintWidgetImage[i], paintId + i);
            }
        }
    }

    private void changeInteractiveCloudWidgetCarPaintImageSprite(Image _image, int _id) {
        Debug.Log("Sprite ID : " + _id);
        if (_id < 3)
            _image.sprite = listOfInteractiveCloudPaintWidgetSprite[_id];
        else
            _image.sprite = listOfInteractiveCloudPaintWidgetSprite[3];

        switch (_id) {
            case 3: _image.color = Color.black; break;
            case 4: _image.color = Color.gray; break;
            case 6: _image.color = Color.red; break;
            case 7: _image.color = new Color(1, .5f, 0); break;
            case 8: _image.color = Color.yellow; break;
            case 9: _image.color = new Color(.5f, 1, 0); break;
            case 10: _image.color = Color.green; break;
            case 11: _image.color = new Color(0, 1, .5f); break;
            case 12: _image.color = Color.cyan; break;
            case 13: _image.color = new Color(0, .5f, 1); break;
            case 14: _image.color = Color.blue; break;
            case 15: _image.color = new Color(.5f, 0, 1); break;
            case 16: _image.color = Color.magenta; break;
            case 17: _image.color = new Color(1, 0, .5f); break;
            default: _image.color = Color.white; break;
        }
    }

    private void changeInteractiveCloudViewMode(InteractiveCloudViewMode _newMode, float _wait = 0.3f) {
        if (_newMode == InteractiveCloudViewMode.Default) {
            //interactiveCloudBackgroundImage.gameObject.SetActive(true);
            interactiveCloudBackgroundImage.GetComponent<Image>().DOFade(0.5f, 0.3f);
        }
        else {
            interactiveCloudBackgroundImage.GetComponent<Image>().DOFade(0f, 0.3f);
        }
        //interactiveCloudBackgroundImage.gameObject.SetActive(false);

        interactiveCloudViewMode = _newMode;
        interactiveCloudSlideOtherButton(_wait);
    }

    private IEnumerator interactiveCloudSlideButton(Button _button, float _endYPos, float _duration, float _wait = 0f) {
        yield return new WaitForSeconds(_wait);
        _button.transform.DOLocalMoveY(_endYPos, _duration);
    }

    private IEnumerator interactiveCloudFadeButton(Button _button, bool _fadeIn, float _duration, float _wait = 0f) {
        yield return new WaitForSeconds(_wait);
        if (_fadeIn) {
            _button.gameObject.SetActive(true);
            _button.GetComponent<Image>().DOFade(0.5f, _duration);
        }
        else {
            _button.GetComponent<Image>().DOFade(0f, _duration);
            StartCoroutine(interactiveCloudActivateButton(_button, false, _duration));
        }
    }

    private IEnumerator interactiveCloudActivateButton(Button _button, bool _active, float _wait = 0f) {
        yield return new WaitForSeconds(_wait);
        _button.gameObject.SetActive(_active);
    }

    private void interactiveCloudSlideOtherButton(float _wait = 0.3f) {
        switch (interactiveCloudViewMode) {
            case InteractiveCloudViewMode.Animation:
                StartCoroutine(interactiveCloudFadeButton(btnPartInspection, false, 0.3f));
                StartCoroutine(interactiveCloudFadeButton(btnCarPaint, false, 0.3f));
                StartCoroutine(interactiveCloudFadeButton(btnTransform, false, 0.3f));
                StartCoroutine(interactiveCloudFadeBottomBar(false));
                break;
            case InteractiveCloudViewMode.CarPaint:
                StartCoroutine(interactiveCloudFadeButton(btnPartInspection, false, 0.3f));
                StartCoroutine(interactiveCloudFadeButton(btnTransform, false, 0.3f));
                StartCoroutine(interactiveCloudFadeButton(btnAnimation, false, 0.3f));
                StartCoroutine(interactiveCloudFadeBottomBar(true));
                break;
            case InteractiveCloudViewMode.Transform:
                StartCoroutine(interactiveCloudFadeButton(btnPartInspection, false, 0.3f));
                StartCoroutine(interactiveCloudFadeButton(btnCarPaint, false, 0.3f));
                StartCoroutine(interactiveCloudFadeButton(btnAnimation, false, 0.3f));
                StartCoroutine(interactiveCloudFadeBottomBar(false));
                break;
            case InteractiveCloudViewMode.PartInspection:
                StartCoroutine(interactiveCloudFadeButton(btnTransform, false, 0.3f));
                StartCoroutine(interactiveCloudFadeButton(btnCarPaint, false, 0.3f));
                StartCoroutine(interactiveCloudFadeButton(btnAnimation, false, 0.3f));
                StartCoroutine(interactiveCloudFadeBottomBar(true));
                break;
            case InteractiveCloudViewMode.Default:
                StartCoroutine(interactiveCloudFadeButton(btnPartInspection, true, 0.3f, _wait));
                StartCoroutine(interactiveCloudFadeButton(btnTransform, true, 0.3f, _wait));
                StartCoroutine(interactiveCloudFadeButton(btnCarPaint, true, 0.3f, _wait));
                StartCoroutine(interactiveCloudFadeButton(btnAnimation, true, 0.3f, _wait));
                StartCoroutine(interactiveCloudFadeBottomBar(false));
                break;
            default:
                StartCoroutine(interactiveCloudFadeButton(btnPartInspection, true, 0.3f, _wait));
                StartCoroutine(interactiveCloudFadeButton(btnTransform, true, 0.3f, _wait));
                StartCoroutine(interactiveCloudFadeButton(btnCarPaint, true, 0.3f, _wait));
                StartCoroutine(interactiveCloudFadeButton(btnAnimation, true, 0.3f, _wait));
                StartCoroutine(interactiveCloudFadeBottomBar(false));
                break;
        }
    }

    private IEnumerator interactiveCloudFadeBottomBar(bool _show) {
        if (_show) {
            imgBottomBar.gameObject.SetActive(true);
            imgBottomBar.DOFade(1f, 0.3f);
            imgBottomBarDonut.DOFade(1f, 0.3f);
            imgBottomBarIcon.DOFade(1f, 0.3f);
            imgBottomBarNext.DOFade(1f, 0.3f);
            imgBottomBarPrev.DOFade(1f, 0.3f);
        }
        else {
            imgBottomBar.DOFade(0f, 0.3f);
            imgBottomBarNext.DOFade(0f, 0.3f);
            imgBottomBarPrev.DOFade(0f, 0.3f);
            imgBottomBarDonut.DOFade(0f, 0.3f);
            imgBottomBarIcon.DOFade(1f, 0.3f);
            yield return new WaitForSeconds(0.3f);
            imgBottomBar.gameObject.SetActive(false);
        }
    }

    private void interactiveCloudAnimRoof() {
        if (isRoofClosed) {
            imgAnimIconRoof.sprite = spriteAnimIconOn;
            interactiveCloudObject.GetComponent<Animator>().SetTrigger(triggerRoofOpenHash);
            isRoofClosed = false;
        }
        else {
            imgAnimIconRoof.sprite = spriteAnimIconOff;
            interactiveCloudObject.GetComponent<Animator>().SetTrigger(triggerRoofCloseHash);
            isRoofClosed = true;
        }
    }

    private IEnumerator waitAnimationFinish(float _duration) {
        yield return new WaitForSeconds(_duration);
        isAnimationFinished = true;
    }

    private IEnumerator interactiveCloudAnimDoor() {
        if (isDoorClosed) {
            imgAnimIconDoor.sprite = spriteAnimIconOn;
            isDoorClosed = false;
            translateDoor(doorLF, -0.0005f, 0.2f);
            translateDoor(doorLR, -0.0005f, 0.2f);
            translateDoor(doorRF, 0.0005f, 0.2f);
            translateDoor(doorRR, 0.0005f, 0.2f);
            yield return new WaitForSeconds(0.1f);
            rotateDoor(doorLF, 60, 2.5f);
            rotateDoor(doorLR, -66, 2.5f);
            rotateDoor(doorRF, -60, 2.5f);
            rotateDoor(doorRR, 65, 2.5f);
        }
        else {
            imgAnimIconDoor.sprite = spriteAnimIconOff;
            isDoorClosed = true;
            rotateDoor(doorLF, 0, 2.5f);
            rotateDoor(doorLR, 0, 2.5f);
            rotateDoor(doorRF, 0, 2.5f);
            rotateDoor(doorRR, 0, 2.5f);
            yield return new WaitForSeconds(2f);
            translateDoor(doorLF, 0.0005f, 0.2f);
            translateDoor(doorLR, 0.0005f, 0.2f);
            translateDoor(doorRF, -0.0005f, 0.2f);
            translateDoor(doorRR, -0.0005f, 0.2f);
        }
    }

    private void translateDoor(GameObject _door, float _translate, float _duration) {
        _door.transform.DOLocalMoveX(_door.transform.localPosition.x + _translate, _duration);
    }

    private void rotateDoor(GameObject _door, float _zRot, float _duration) {
        _door.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(298.3f, 159.9f, -347.9f + _zRot)), _duration);
    }

    private void interactiveCloudAnimBonnet() {
        if (isBonnetClosed) {
            imgAnimIconBonnet.sprite = spriteAnimIconOn;
            isBonnetClosed = false;
            bonnetL.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(-19.6f, -268.2f, -24.7f)), 2.5f);
            bonnetR.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(-31.7f, -111.7f, -332.8f)), 2.5f);
        }
        else {
            imgAnimIconBonnet.sprite = spriteAnimIconOff;
            isBonnetClosed = true;
            bonnetL.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(294f, -202.5f, 284.2f)), 2.5f);
            bonnetR.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(-66f, -202.5f, -255.8f)), 2.5f);
        }
    }

    private void interactiveCloudInspectPart(int _id) {
        if (_id == 0) {
            rotateGuideObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
        else {
            rotateGuideObject.transform.LookAt(new Vector3(arCamera.transform.position.x, 0, arCamera.transform.position.z));
            rotateGuideObject.transform.localRotation = Quaternion.Euler(new Vector3(0, rotateGuideObject.transform.localRotation.eulerAngles.y + listOfInspectPartRotationEuler[_id].y, rotateGuideObject.transform.localRotation.eulerAngles.z));
        }
        interactiveCloudObject.transform.DOLocalRotateQuaternion(Quaternion.Euler(rotateGuideObject.transform.localRotation.eulerAngles), 2.5f);
        txtInspectPartName.text = listofInspectPartName[_id];
    }
    #endregion
}
