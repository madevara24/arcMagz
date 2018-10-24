using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Video;

public class CoreController : MonoBehaviour {

    #region Core Variables
    [SerializeField] TouchDetector touchDetector;
    [SerializeField] Camera arCamera;

    [SerializeField] List<Sprite> listOfGallerySprite;
    #endregion

    #region Gallery Overview Variables
    [SerializeField] GameObject canvasOverview;
    [SerializeField] GameObject scrollView;
    [SerializeField] Image prefabImage;
    [SerializeField] RawImage prefabRawImage;

    private List<RawImage> listOfGalleryRawImage = new List<RawImage>();
    private List<Image> listOfGalleryImage = new List<Image>();
    #endregion

    #region Gallery Preview Variables
    [SerializeField] GameObject canvasPreview;
    [SerializeField] Image imgPreviewLeft;
    [SerializeField] Image imgPreviewMid;
    [SerializeField] Image imgPreviewRight;

    private float imgPreviewLeftDefaultXPos, imgPreviewMidDefaultXPos, imgPreviewRightDefaultXPos;
    private Vector3 previewStartScale;
    private Vector2 previewStartPos;

    public enum ZoomState { FitToScreen, ZoomIn, FitToHeight }
    [SerializeField] ZoomState galleryPreviewZoomState = ZoomState.FitToScreen;

    private int previewedImageId = 0;
    private bool isCanSlide = true, isFinishSlide = true;
    #endregion

    #region Interactive 3D Variables
    [SerializeField] GameObject canvasInteractive3D;
    [SerializeField] Image interactive3dBackgroundImage;
    [SerializeField] Image imgBottomBar, imgBottomBarDonut, imgBottomBarIcon, imgBottomBarNext, imgBottomBarPrev;
    [SerializeField] Image imgArrowPartInspection, imgArrowCarPaint, imgArrowTransform, imgArrowAnimation;
    [SerializeField] Image imgAnimIconBonnet, imgAnimIconDoor, imgAnimIconRoof;
    [SerializeField] Sprite spriteAnimIconOn, spriteAnimIconOff, spriteIconCarPaint, spriteIconPartInspection;
    [SerializeField] Button btnPartInspection, btnCarPaint, btnTransform, btnAnimation;
    [SerializeField] Button btnAnimRoof, btnAnimDoor, btnAnimBonnet;
    [SerializeField] Button btnBottomBarNext, btnBottomBarPrev;
    [SerializeField] Text txtInspectPartName;

    [SerializeField] GameObject interactive3dObject, doorLF, doorRF, doorLR, doorRR, bonnetR, bonnetL, rotateGuideObject;

    [SerializeField] List<Sprite> listOfInteractive3dPaintWidgetSprite;
    [SerializeField] List<Image> listOfInteractive3dPaintWidgetImage;
    [SerializeField] List<Material> listOfInteractive3dMaterials;
    [SerializeField] List<GameObject> listOfPaintedCarParts;
    [SerializeField] List<Vector3> listOfInspectPartRotationEuler;
    [SerializeField] List<string> listofInspectPartName;

    public enum Interactive3DViewMode { Animation, CarPaint, Transform, PartInspection, Default }
    private Interactive3DViewMode interactive3DViewMode = Interactive3DViewMode.Default;
    private bool isDoorClosed = true, isBonnetClosed = true, isRoofClosed = true, isAnimationFinished = true;
    private int triggerRoofOpenHash = Animator.StringToHash("triggerRoofOpen"), triggerRoofCloseHash = Animator.StringToHash("triggerRoofClose");
    private Vector3 interactive3dStartScale, interactive3dDefaultScale, interactive3dStartEuler;

    private Interactive3DObjectBehaviour get3dObjctBehaviour() {
        return interactive3dObject.GetComponent<Interactive3DObjectBehaviour>();
    }

    private int interactive3dPaintId = 4, interactive3dInspetPartId = 0;

    public bool isInteractive3dMarkerDetected = false;
    #endregion

    #region Interactive Video Variables
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] AudioSource audioVideo;
    [SerializeField] Image progress;
    [SerializeField] GameObject Play, Pause, UIvideo, MuteButton, unMuteButton, MarkerVideo;
    private static float timerUI = 3.0f;
    #endregion

    #region Hyperlink
    [SerializeField] GameObject BGlink, Logo;
    #endregion

    #region Panoramic Image Variabales
    public float scrollSpeed = 0.05f;
    [SerializeField] Renderer panoramicRenderer;
    [SerializeField] Material panoramicMaterial;
    private float panoramicMoveSpeed = 0f;
    private bool panoramicDetected, isPanoramicMovingRight = true, isPanoramicMoving = false;
    #endregion

    #region Event Enum Region
    public const string EVENT_INTERACTIVE_IMAGE_OPEN_GALLERY = "EVENT_INTERACTIVE_IMAGE_OPEN_GALLERY";
    public const string EVENT_INTERACTIVE_3D_OPEN_INTERFACE = "EVENT_INTERACTIVE_3D_OPEN_INTERFACE";
    public const string EVENT_INTERACTIVE_3D_DETECTED = "EVENT_INTERACTIVE_3D_DETECTED";
    public const string EVENT_INTERACTIVE_3D_LOST = "EVENT_INTERACTIVE_3D_LOST";";
    public const string EVENT_INTERACTIVE_VIDEO_DETECTED = "EVENT_INTERACTIVE_VIDEO_DETECTED";
    public const string EVENT_INTERACTIVE_VIDEO_TAP = "EVENT_INTERACTIVE_VIDEO_TAP";
    public const string EVENT_INTERACTIVE_VIDEO_LOST = "EVENT_INTERACTIVE_VIDEO_LOST";
    public const string EVENT_PARONAMIC_IMAGE_DETECTED = "EVENT_PARONAMIC_IMAGE_DETECTED";
    public const string EVENT_PARONAMIC_IMAGE_LOST = "EVENT_PARONAMIC_IMAGE_LOST";
    public const string EVENT_HYPERLINK_IMAGE_DETECTED = "EVENT_HYPERLINK_IMAGE_DETECTED";
    public const string EVENT_HYPERLINK_IMAGE_LOST = "EVENT_HYPERLINK_IMAGE_LOST";
    public const string EVENT_HYPERLINK_TAP = "EVENT_HYPERLINK_TAP";
    public const string EVENT_DOUBLE_TAP = "EVENT_DOUBLE_TAP";
    #endregion

    void Start () {
        DOTween.Init(true, true, LogBehaviour.Default);
        EventManager.StartListening(EVENT_INTERACTIVE_IMAGE_OPEN_GALLERY, openGalleryOverview);
        EventManager.StartListening(EVENT_INTERACTIVE_3D_OPEN_INTERFACE, openInteractive3DInterface);
        EventManager.StartListening(EVENT_INTERACTIVE_3D_DETECTED, onDetectInteractive3D);
        EventManager.StartListening(EVENT_INTERACTIVE_3D_LOST, onDetectInteractive3D);
        EventManager.StartListening(EVENT_INTERACTIVE_VIDEO_DETECTED, onDetectVideoInteractive);
        EventManager.StartListening(EVENT_INTERACTIVE_VIDEO_LOST, onLostVideoInteractive);
        EventManager.StartListening(EVENT_DOUBLE_TAP, onDoubleTapDetected);
        EventManager.StartListening(EVENT_INTERACTIVE_VIDEO_TAP, ActiveUI);
        EventManager.StartListening(EVENT_PARONAMIC_IMAGE_DETECTED, onDetectPanoramic);
        EventManager.StartListening(EVENT_PARONAMIC_IMAGE_LOST, onLostPanoramic);
        EventManager.StartListening(EVENT_HYPERLINK_IMAGE_DETECTED, onDetectHyperlink);
        EventManager.StartListening(EVENT_HYPERLINK_IMAGE_LOST, onLostHyperlink);
        EventManager.StartListening(EVENT_HYPERLINK_TAP, onTapHyperlink);
        //openGalleryOverview();
    }

    // Update is called once per frame
    void Update () {
        debuggingKey();
        if(canvasPreview.activeSelf)
            updateGalleryPreview();
        if (UIvideo.activeSelf)
            UpdateVideoPlayer();
        
        if (panoramicDetected) {
            //updateTexturePosition();
            updatePanoramicPosition();
        }
        if (isInteractive3dMarkerDetected)
            updateInteractive3D();
        if (Input.GetKeyDown(KeyCode.Escape)) {
            onPressBackButton();
        }
    }

    private void debuggingKey() {
        if (Input.GetKeyDown(KeyCode.L)) {
            Debug.Log("L");
            EventManager.TriggerEvent(EVENT_INTERACTIVE_IMAGE_OPEN_GALLERY);
        }
        if (Input.GetKeyDown(KeyCode.K)) {
            Debug.Log("K");
            EventManager.TriggerEvent(EVENT_DOUBLE_TAP);
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            Debug.Log(imgBottomBarDonut.GetComponentInChildren<Image>(false).sprite.name);
            //Debug.Log("Local Euler\nLF : " + doorLF.transform.localRotation.eulerAngles + "\nLR : " + doorLR.transform.localRotation.eulerAngles + "\nRF : " + doorRF.transform.localRotation.eulerAngles + "\nRR : " + doorRR.transform.localRotation.eulerAngles);
        }
        if (Input.GetKeyDown(KeyCode.O)) {
            Debug.Log("Local Rotation : " + bonnetL.transform.localRotation.ToString() + "; " + bonnetR.transform.localRotation.ToString() + "\nEuler : " + bonnetL.transform.localRotation.eulerAngles.ToString() + "; " + bonnetR.transform.localRotation.eulerAngles.ToString());
        }
    }

    private void OnGUI() {
        GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = 40;
        //GUI.Box(new Rect(50, 100, 700, 50), string.Format("{0} {1} {2}", touchDetector.SwipeDelta, interactive3dStartScale, interactive3dObject.transform.localScale));
        //GUI.Box(new Rect(50, 150, 700, 50), string.Format("{0} {1}", bonnetL.transform.localRotation.eulerAngles, bonnetR.transform.localRotation.eulerAngles));
        //GUI.Box(new Rect(100, 200, 600, 50), string.Format("{0} {1}", interactive3dStartEuler.y + touchDetector.SwipeDelta.x, interactive3dStartEuler.y - touchDetector.SwipeDelta.x));
        //GUI.Box(new Rect(100, 100, 600, 50), string.Format("{0:0.000} {1} {2}", imgPreviewMid.transform.localPosition.y + touchDetector.SwipeDelta.y, -30 + (200 * (imgPreviewMid.transform.localScale.y - 3)), -85 - (200 * (imgPreviewMid.transform.localScale.y - 3))));
    }

    #region Core Functions
    public void onPressBackButton() {
        if (canvasInteractive3D.activeSelf)
            canvasInteractive3D.SetActive(false);
        if (canvasOverview.activeSelf)
            canvasOverview.SetActive(false);
        if (canvasPreview.activeSelf) {
            canvasPreview.SetActive(false);
            canvasOverview.SetActive(true);
        }
    }

    private void onDoubleTapDetected() {
        if (canvasPreview.activeSelf)
            galleryPreviewDoubleTap();
    }
    public void openGalleryOverview() {
        canvasOverview.SetActive(true);
        initGalleryOverview();
    }

    public void openGalleryPreview(int _imageId) {
        canvasPreview.SetActive(true);
        canvasOverview.SetActive(false);
        initGalleryPreview(_imageId);
    }

    public void openInteractive3DInterface() {
        canvasInteractive3D.SetActive(true);
    }
    #endregion

    #region Gallery Overview Functions
    public void initGalleryOverview() {
        //canvasOverview.SetActive(true);
        /*
        initScrollViewContent();
        initTiledImages();*/
    }

    private void closeGalleryOverview() {
        canvasOverview.SetActive(false);
    }

    public void onPressGalleryOverviewBackButton() {
        closeGalleryOverview();
    }
    /*
    private void initScrollViewContent() {
        float imageHeight = canvasOverview.GetComponent<RectTransform>().rect.width * 9 / 64;
        scrollView.GetComponent<ScrollRect>().content.sizeDelta = new Vector2(canvasOverview.GetComponent<RectTransform>().rect.width * 9 / 64, listOfGallerySprite.Count / 2 * imageHeight);
        Debug.Log("Image height : " + imageHeight + "\nScroll Height : " + (listOfGallerySprite.Count / 2 * imageHeight) + "\nScreen height : " + Screen.height);
    }

    private void initTiledImages() {
        Debug.Log("Screen width " + canvasOverview.GetComponent<RectTransform>().rect.width);
        float imageHeight = canvasOverview.GetComponent<RectTransform>().rect.width / 4;
        float imageWidth = imageHeight / 9 * 16;
        for (int i = 0; i < listOfGallerySprite.Count / 4; i++) {
            for (int j = 0; j < 4; j++) {
                attachImage(getImagePosition(i, j, imageHeight, imageHeight), imageWidth, imageHeight, (i * 4) + j);
                Debug.Log("ID : " + ((i * 4) + j));
            }
        }
    }

    private Vector2 getImagePosition(float _verticalId, float _horizontalId, float _width, float _height) {
        Vector2 position;
        position.x = (_horizontalId + 0.5f) * _width;
        position.y = ((_verticalId + 0.5f) * -_height);
        return position;
    }

    private void attachImage(Vector2 _pos, float _width, float _height, int _imageId) {
        RawImage newRawImage = Instantiate(prefabRawImage) as RawImage;
        Vector3 positon = new Vector3(_pos.x, _pos.y, 0);
        newRawImage.rectTransform.SetParent(scrollView.GetComponent<ScrollRect>().content.transform, false);
        newRawImage.rectTransform.anchorMax = new Vector2(0, 1);
        newRawImage.rectTransform.anchorMin = new Vector2(0, 1);
        newRawImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        newRawImage.rectTransform.anchoredPosition = _pos;
        newRawImage.rectTransform.sizeDelta = new Vector2(_height, _height);
        Image newImage = Instantiate(prefabImage) as Image;
        newImage.transform.SetParent(newRawImage.transform);
        newImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        newImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        newImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        newImage.rectTransform.anchoredPosition = Vector2.zero;
        newImage.rectTransform.sizeDelta = new Vector2(_width, _height);
        newImage.sprite = listOfGallerySprite[_imageId];
        //newImage.preserveAspect = true;
        newImage.transform.localScale = Vector3.one;
        newImage.gameObject.AddComponent<GalleryTileBehaviour>();
        newImage.GetComponent<GalleryTileBehaviour>().imageId = _imageId;
        listOfGalleryRawImage.Add(newRawImage);
        listOfGalleryImage.Add(newImage);
    }*/
    #endregion

    #region Gallery Preview Functions
    public void initGalleryPreview(int _idImage) {
        changeGalleryPreviewZoomState(ZoomState.FitToScreen);
        previewedImageId = _idImage;
        loadImageSprite();
        initPreviewImageDefaultPos();
    }

    private void closeGalleryPreview() {
        canvasOverview.SetActive(true);
        canvasPreview.SetActive(false);
    }

    public void onPressGalleryPreviewBackButton() {
        closeGalleryPreview();
    }

    private void initPreviewImageDefaultPos() {
        imgPreviewLeftDefaultXPos = imgPreviewLeft.transform.localPosition.x;
        imgPreviewMidDefaultXPos = imgPreviewMid.transform.localPosition.x;
        imgPreviewRightDefaultXPos = imgPreviewRight.transform.localPosition.x;
    }

    private float getTouchDetectorPinchDelta() {
        return touchDetector.PinchCurrentMag / touchDetector.PinchStartMag;
    }

    private float getScaleMultiplier(float _dif = 1) {
        float scale = getTouchDetectorPinchDelta();
        if (scale < 1)
            return scale;
        else
            return 1 + (scale / _dif);
    }

    private void updateGalleryPreview() {
        if (touchDetector.IsPinching && isFinishSlide) {
            Debug.Log("Pinch");
            if (imgPreviewMid.transform.localScale.x > 1)
                changeGalleryPreviewZoomState(ZoomState.ZoomIn);

            imgPreviewMid.transform.localScale = previewStartScale * getScaleMultiplier(15);


            if (getTouchDetectorPinchDelta() < 0.8f && galleryPreviewZoomState == ZoomState.FitToScreen) {
                //onPressBackButton();
            }
        }
        if (!touchDetector.IsDraging) {
            if (imgPreviewMid.transform.localScale.x > 5)
                imgPreviewMid.transform.localScale = new Vector3(5, 5, 5);
            if (imgPreviewMid.transform.localScale.x < 1)
                imgPreviewMid.transform.localScale = Vector3.one;

            previewStartScale = imgPreviewMid.transform.localScale;
        }
        if (galleryPreviewZoomState == ZoomState.FitToScreen)
            updateGalleryPreviewSlider();
        else
            updateGalleryPreviewNavigation();
    }

    private void updateGalleryPreviewNavigation() {
        if (touchDetector.IsDraging) {
            if(Mathf.Abs(previewStartPos.x + touchDetector.SwipeDelta.x) < (imgPreviewMid.transform.localScale.x - 1) * 360) {
                if (imgPreviewMid.transform.localScale.x >= 3f) {
                    if (imgPreviewMid.transform.localPosition.y + touchDetector.SwipeDelta.y < -30 + (200 * (imgPreviewMid.transform.localScale.y - 3)) &&
                        imgPreviewMid.transform.localPosition.y + touchDetector.SwipeDelta.y > -85 - (200 * (imgPreviewMid.transform.localScale.y - 3))) {
                        imgPreviewMid.transform.localPosition = previewStartPos + touchDetector.SwipeDelta;
                    }
                    else {
                        imgPreviewMid.transform.localPosition = new Vector3(previewStartPos.x + touchDetector.SwipeDelta.x, imgPreviewMid.transform.localPosition.y);
                    }
                }
                else {
                    imgPreviewMid.transform.localPosition = new Vector3(previewStartPos.x + touchDetector.SwipeDelta.x, 0);
                }
            }
        }
        else {
            previewStartPos = imgPreviewMid.transform.localPosition;
        }
    }

    private void updateGalleryPreviewSlider() {
        if (touchDetector.IsDraging) {
            if(touchDetector.SwipeRight || touchDetector.SwipeLeft) {
                if (isCanSlide) {
                    isFinishSlide = false;
                    imgPreviewLeft.transform.localPosition = new Vector3(imgPreviewLeftDefaultXPos + touchDetector.SwipeDelta.x, imgPreviewLeft.transform.localPosition.y);
                    imgPreviewMid.transform.localPosition = new Vector3(imgPreviewMidDefaultXPos + touchDetector.SwipeDelta.x, imgPreviewMid.transform.localPosition.y);
                    imgPreviewRight.transform.localPosition = new Vector3(imgPreviewRightDefaultXPos + touchDetector.SwipeDelta.x, imgPreviewRight.transform.localPosition.y);
                }
            }
        }
        else {
            if(isCanSlide && !isFinishSlide) {
                Debug.Log("Trigger Slide");
                isCanSlide = false;
                slidePreviewImages();
                StartCoroutine(waitSlideFinish());
            }
        }
    }

    private IEnumerator waitSlideFinish() {
        yield return new WaitForSeconds(0.3f);
        Debug.Log("Reposition Image");
        isCanSlide = true;
        isFinishSlide = true;
        repositionImage();
        loadImageSprite();
    }

    private void slidePreviewImages() {
        Debug.Log("Img Prev Mid local pos : " + imgPreviewMid.transform.localPosition);
        if(imgPreviewMid.transform.localPosition.x > 160) {
            imgPreviewLeft.transform.DOLocalMoveX(imgPreviewMidDefaultXPos, 0.3f, true);
            imgPreviewMid.transform.DOLocalMoveX(imgPreviewRightDefaultXPos, 0.3f, true);
            imgPreviewRight.transform.DOLocalMoveX(imgPreviewRightDefaultXPos, 0.3f, true);
            changeObjectSpriteId(false);
        }
        else if (imgPreviewMid.transform.localPosition.x < -160) {
            imgPreviewLeft.transform.DOLocalMoveX(imgPreviewLeftDefaultXPos, 0.3f, true);
            imgPreviewMid.transform.DOLocalMoveX(imgPreviewLeftDefaultXPos, 0.3f, true);
            imgPreviewRight.transform.DOLocalMoveX(imgPreviewMidDefaultXPos, 0.3f, true);
            changeObjectSpriteId(true);
        }
        else {
            imgPreviewLeft.transform.DOLocalMoveX(imgPreviewLeftDefaultXPos, 0.3f, true);
            imgPreviewMid.transform.DOLocalMoveX(imgPreviewMidDefaultXPos, 0.3f, true);
            imgPreviewRight.transform.DOLocalMoveX(imgPreviewRightDefaultXPos, 0.3f, true);
        }
    }

    private void repositionImage() {
        imgPreviewLeft.transform.localPosition = new Vector3(imgPreviewLeftDefaultXPos, 0);
        imgPreviewMid.transform.localPosition = new Vector3(imgPreviewMidDefaultXPos, 0);
        imgPreviewRight.transform.localPosition = new Vector3(imgPreviewRightDefaultXPos, 0);
    }

    private void loadImageSprite() {
        imgPreviewMid.sprite = listOfGallerySprite[previewedImageId];
        if (previewedImageId == 0)
            imgPreviewLeft.sprite = listOfGallerySprite[listOfGallerySprite.Count - 1];
        else
            imgPreviewLeft.sprite = listOfGallerySprite[previewedImageId - 1];
        if (previewedImageId == listOfGallerySprite.Count - 1)
            imgPreviewRight.sprite = listOfGallerySprite[0];
        else
            imgPreviewRight.sprite = listOfGallerySprite[previewedImageId + 1];
    }

    private void changeObjectSpriteId(bool _next) {
        if (_next) {
            if (previewedImageId < listOfGallerySprite.Count - 1)
                previewedImageId++;
            else
                previewedImageId = 0;
        }
        else {
            if (previewedImageId > 0)
                previewedImageId--;
            else
                previewedImageId = listOfGallerySprite.Count - 1;
        }
    }

    private void galleryPreviewDoubleTap() {
        Debug.Log("Double Tap");
        switch (galleryPreviewZoomState) {
            case ZoomState.FitToScreen:
                changeGalleryPreviewZoomState(ZoomState.FitToHeight);
                break;
            case ZoomState.ZoomIn:
                changeGalleryPreviewZoomState(ZoomState.FitToScreen);
                break;
            case ZoomState.FitToHeight:
                changeGalleryPreviewZoomState(ZoomState.FitToScreen);
                break;
            default:
                changeGalleryPreviewZoomState(ZoomState.FitToScreen);
                break;
        }
    }

    private void adjustPreviewedImageZoom() {
        switch (galleryPreviewZoomState) {
            case ZoomState.FitToScreen:
                imgPreviewMid.transform.DOScale(Vector3.one, 0.3f);
                imgPreviewMid.transform.DOLocalMove(Vector3.zero, 0.3f);
                break;
            case ZoomState.FitToHeight:
                imgPreviewMid.transform.DOScale(new Vector3(3f, 3f), 0.3f);
                imgPreviewMid.transform.DOLocalMove(new Vector3(0, -55), 0.3f);
                break;
            default:
                break;
        }
    }

    private void changeGalleryPreviewZoomState(ZoomState _newState) {
        galleryPreviewZoomState = _newState;
        adjustPreviewedImageZoom();
    }
    #endregion

    #region Interactive 3D Functions
    private void onDetectInteractive3D() {
        interactive3dDefaultScale = interactive3dStartScale = interactive3dObject.transform.localScale;
        isInteractive3dMarkerDetected = true;
    }

    private void onLostInteractive3d() {
        isInteractive3dMarkerDetected = false;
    }

    private void updateInteractive3D() {
        if(interactive3DViewMode == Interactive3DViewMode.Transform) {
            interactive3DCheckSwipe();
        }
    }

    private void interactive3DCheckSwipe() {
        if (touchDetector.SwipeUp) {
            if (interactive3dStartScale.x * (1 + (Mathf.Abs(touchDetector.SwipeDelta.y) / 640)) < 2) {
                interactive3dObject.transform.localScale = interactive3dStartScale * (1 + (Mathf.Abs(touchDetector.SwipeDelta.y) / 640));
            }
        }
        if (touchDetector.SwipeDown) {
            if (interactive3dStartScale.x / (1 + (Mathf.Abs(touchDetector.SwipeDelta.y) / 640)) > 0.25f) {
                interactive3dObject.transform.localScale = interactive3dStartScale / (1 + (Mathf.Abs(touchDetector.SwipeDelta.y) / 640));
            }
        }
        if (touchDetector.SwipeRight) {
            interactive3dObject.transform.localRotation = Quaternion.Euler(new Vector3(interactive3dStartEuler.x, interactive3dStartEuler.y - (touchDetector.SwipeDelta.x / 2), interactive3dStartEuler.z));
        }
        if (touchDetector.SwipeLeft) {
            interactive3dObject.transform.localRotation = Quaternion.Euler(new Vector3(interactive3dStartEuler.x, interactive3dStartEuler.y - (touchDetector.SwipeDelta.x / 2), interactive3dStartEuler.z));
        }
        if (!touchDetector.IsDraging) {
            interactive3dStartScale = interactive3dObject.transform.localScale;
            interactive3dStartEuler = interactive3dObject.transform.localRotation.eulerAngles;
        }
    }

    private void interactive3DChangeTexture() {
        for (int i = 0; i < listOfPaintedCarParts.Count; i++) {
            listOfPaintedCarParts[i].GetComponent<Renderer>().material = listOfInteractive3dMaterials[interactive3dPaintId];
        }
    }

    public void onPressPartInspectionButton() {
        Debug.Log("Press Part Inspect");
        if (interactive3DViewMode == Interactive3DViewMode.Default) {
            imgBottomBarIcon.GetComponent<Image>().sprite = spriteIconPartInspection;
            changeInteractive3DViewMode(Interactive3DViewMode.PartInspection);
            imgArrowPartInspection.color = Color.cyan;
            imgArrowPartInspection.transform.localEulerAngles = new Vector3(0, 0, 180);
            interactive3dInspectPart(0);
            showInteractive3dBottomBar(true);
            showInteractive3dInspectPartName(true);
        }
        else {
            changeInteractive3DViewMode(Interactive3DViewMode.Default);
            imgArrowPartInspection.color = Color.white;
            imgArrowPartInspection.transform.localEulerAngles = Vector3.zero;
            showInteractive3dBottomBar(false);
            showInteractive3dInspectPartName(false);
        }   
    }

    public void onPressCarPaintButton() {
        Debug.Log("Press Car Paint");
        if (interactive3DViewMode == Interactive3DViewMode.Default) {
            imgBottomBarIcon.GetComponent<Image>().sprite = spriteIconCarPaint;
            changeInteractive3DViewMode(Interactive3DViewMode.CarPaint);
            imgArrowCarPaint.color = Color.cyan;
            imgArrowCarPaint.transform.localEulerAngles = new Vector3(0, 0, 180);
            StartCoroutine(interactive3dSlideButton(btnCarPaint, 475, 0.3f, 0.3f));
            interactive3dUpdatePaintWidgetImage();
            showInteractive3dPaintWidgetImage(true);
            showInteractive3dBottomBar(true);
        }
        else {
            changeInteractive3DViewMode(Interactive3DViewMode.Default);
            imgArrowCarPaint.color = Color.white;
            imgArrowCarPaint.transform.localEulerAngles = Vector3.zero;
            StartCoroutine(interactive3dSlideButton(btnCarPaint, 377, 0.3f));
            showInteractive3dPaintWidgetImage(false);
            showInteractive3dBottomBar(false);
        }
    }

    public void onPressTransformButton() {
        Debug.Log("Press Transform");
        if (interactive3DViewMode == Interactive3DViewMode.Default) {
            changeInteractive3DViewMode(Interactive3DViewMode.Transform);
            imgArrowTransform.color = Color.cyan;
            imgArrowTransform.transform.localEulerAngles = new Vector3(0, 0, 180);
            StartCoroutine(interactive3dSlideButton(btnTransform, 475, 0.3f, 0.3f));
        }
        else {
            changeInteractive3DViewMode(Interactive3DViewMode.Default);
            imgArrowTransform.color = Color.white;
            imgArrowTransform.transform.localEulerAngles = Vector3.zero;
            StartCoroutine(interactive3dSlideButton(btnTransform, 279, 0.3f));
        }
    }

    public void onPressAnimationButton() {
        Debug.Log("Press Animation");
        if (interactive3DViewMode == Interactive3DViewMode.Default) {
            changeInteractive3DViewMode(Interactive3DViewMode.Animation);
            imgArrowAnimation.color = Color.cyan;
            imgArrowAnimation.transform.localEulerAngles = new Vector3(0, 0, 180);
            StartCoroutine(interactive3dSlideButton(btnAnimation, 475, 0.3f, 0.3f));
            StartCoroutine(showInteractive3dAnimButton(true));
        }
        else {
            changeInteractive3DViewMode(Interactive3DViewMode.Default, 0.6f);
            imgArrowAnimation.color = Color.white;
            imgArrowAnimation.transform.localEulerAngles = Vector3.zero;
            StartCoroutine(interactive3dSlideButton(btnAnimation, 181, 0.3f, 0.3f));
            StartCoroutine(showInteractive3dAnimButton(false));
        }
    }

    public void onPressInteractive3DNextPartButton() {
        Debug.Log("Next");
        if(interactive3DViewMode == Interactive3DViewMode.CarPaint) {
            changeInteractive3dPaintId(interactive3dPaintId + 1);
        }else if(interactive3DViewMode == Interactive3DViewMode.PartInspection) {
            changeInteractive3dPartInspectionId(true);
        }
    }

    public void onPressInteractive3DPrevPartButton() {
        Debug.Log("Prev");
        if (interactive3DViewMode == Interactive3DViewMode.CarPaint) {
            changeInteractive3dPaintId(interactive3dPaintId - 1);
        }
        else if (interactive3DViewMode == Interactive3DViewMode.PartInspection) {
            changeInteractive3dPartInspectionId(false);
        }
    }

    public void onPressAnimRoofButton() {
        interactive3dAnimRoof();
    }

    public void onPressAnimDoorButton() {
        if (isAnimationFinished) {
            isAnimationFinished = false;
            StartCoroutine(interactive3dAnimDoor());
            StartCoroutine(waitAnimationFinish(2.5f));
        }
    }

    public void onPressAnimBonnetButton() {
        if (isAnimationFinished) {
            isAnimationFinished = false;
            interactive3dAnimBonnet();
            StartCoroutine(waitAnimationFinish(2.5f));
        }
    }

    private void changeInteractive3dPartInspectionId(bool _next) {
        if (_next)
            interactive3dInspetPartId++;
        else
            interactive3dInspetPartId--;

        if (interactive3dInspetPartId < 0)
            interactive3dInspetPartId = listOfInspectPartRotationEuler.Count - 1;
        if (interactive3dInspetPartId > listOfInspectPartRotationEuler.Count - 1)
            interactive3dInspetPartId = 0;

        interactive3dInspectPart(interactive3dInspetPartId);
    }

    public void onPressCarPaintWidgetImage(int _id) {
        changeInteractive3dPaintId(_id + interactive3dPaintId - 3);
    }

    private void changeInteractive3dPaintId(int _id) {
        interactive3dPaintId = _id;

        if (interactive3dPaintId >= listOfInteractive3dMaterials.Count)
            interactive3dPaintId = interactive3dPaintId - listOfInteractive3dMaterials.Count;

        if (interactive3dPaintId < 0)
            interactive3dPaintId = interactive3dPaintId + listOfInteractive3dMaterials.Count - 1;

        interactive3DChangeTexture();
        interactive3dUpdatePaintWidgetImage();
    }

    private void showInteractive3dBottomBar(bool _show) {
        imgBottomBar.gameObject.SetActive(_show);
        btnBottomBarNext.gameObject.SetActive(_show);
        btnBottomBarPrev.gameObject.SetActive(_show);
    }

    private void showInteractive3dInspectPartName(bool _show) {
        txtInspectPartName.gameObject.SetActive(_show);
    }

    private void showInteractive3dPaintWidgetImage(bool _show) {
        for (int i = 0; i < listOfInteractive3dPaintWidgetImage.Count; i++) {
            listOfInteractive3dPaintWidgetImage[i].gameObject.SetActive(_show);
        }
    }

    private IEnumerator showInteractive3dAnimButton(bool _show) {
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
            StartCoroutine(interactive3dActivateButton(btnAnimBonnet, false, 0.3f));
            StartCoroutine(interactive3dActivateButton(btnAnimDoor, false, 0.3f));
            StartCoroutine(interactive3dActivateButton(btnAnimRoof, false, 0.3f));
        }
    }

    private void interactive3dUpdatePaintWidgetImage() {
        int paintId = interactive3dPaintId - 3;
        Debug.Log("Paint ID : " + interactive3dPaintId);
        for (int i = 0; i < listOfInteractive3dPaintWidgetImage.Count; i++) {
            if (paintId + i < 0) {
                changeInteractive3dWidgetCarPaintImageSprite(listOfInteractive3dPaintWidgetImage[i], paintId + i + listOfInteractive3dMaterials.Count);
            }
            else if (paintId + i > listOfInteractive3dMaterials.Count - 1) {
                changeInteractive3dWidgetCarPaintImageSprite(listOfInteractive3dPaintWidgetImage[i], paintId + i - listOfInteractive3dMaterials.Count);
            }
            else {
                changeInteractive3dWidgetCarPaintImageSprite(listOfInteractive3dPaintWidgetImage[i], paintId + i);
            }
        }
    }

    private void changeInteractive3dWidgetCarPaintImageSprite(Image _image, int _id) {
        Debug.Log("Sprite ID : " + _id);
        if (_id < 3)
            _image.sprite = listOfInteractive3dPaintWidgetSprite[_id];
        else
            _image.sprite = listOfInteractive3dPaintWidgetSprite[3];

        switch (_id) {
            case 3:_image.color = Color.black;break;
            case 4:_image.color = Color.gray;break;
            case 6:_image.color = Color.red;break;
            case 7:_image.color = new Color(1, .5f, 0);break;
            case 8:_image.color = Color.yellow;break;
            case 9:_image.color = new Color(.5f, 1, 0);break;
            case 10:_image.color = Color.green;break;
            case 11:_image.color = new Color(0, 1, .5f);break;
            case 12:_image.color = Color.cyan;break;
            case 13:_image.color = new Color(0, .5f, 1);break;
            case 14:_image.color = Color.blue;break;
            case 15:_image.color = new Color(.5f, 0, 1);break;
            case 16:_image.color = Color.magenta;break;
            case 17:_image.color = new Color(1, 0, .5f);break;
            default:_image.color = Color.white;break;
        }
    }

    private void changeInteractive3DViewMode(Interactive3DViewMode _newMode, float _wait = 0.3f) {
        if (_newMode == Interactive3DViewMode.Default) {
            //interactive3dBackgroundImage.gameObject.SetActive(true);
            interactive3dBackgroundImage.GetComponent<Image>().DOFade(0.5f, 0.3f);
        }
        else {
            interactive3dBackgroundImage.GetComponent<Image>().DOFade(0f, 0.3f);
        }
            //interactive3dBackgroundImage.gameObject.SetActive(false);

        interactive3DViewMode = _newMode;
        interactive3dSlideOtherButton(_wait);
    }

    private IEnumerator interactive3dSlideButton(Button _button, float _endYPos, float _duration, float _wait = 0f) {
        yield return new WaitForSeconds(_wait);
        _button.transform.DOLocalMoveY(_endYPos, _duration);
    }

    private IEnumerator interactive3dFadeButton(Button _button, bool _fadeIn, float _duration, float _wait = 0f) {
        yield return new WaitForSeconds(_wait);
        if (_fadeIn) {
            _button.gameObject.SetActive(true);
            _button.GetComponent<Image>().DOFade(0.5f, _duration);
        }
        else {
            _button.GetComponent<Image>().DOFade(0f, _duration);
            StartCoroutine(interactive3dActivateButton(_button, false, _duration));
        }
    }

    private IEnumerator interactive3dActivateButton(Button _button, bool _active, float _wait = 0f) {
        yield return new WaitForSeconds(_wait);
        _button.gameObject.SetActive(_active);
    }

    private void interactive3dSlideOtherButton(float _wait = 0.3f) {
        switch (interactive3DViewMode) {
            case Interactive3DViewMode.Animation:
                StartCoroutine(interactive3dFadeButton(btnPartInspection, false, 0.3f));
                StartCoroutine(interactive3dFadeButton(btnCarPaint, false, 0.3f));
                StartCoroutine(interactive3dFadeButton(btnTransform, false, 0.3f));
                StartCoroutine(interactive3dFadeBottomBar(false));
                break;
            case Interactive3DViewMode.CarPaint:
                StartCoroutine(interactive3dFadeButton(btnPartInspection, false, 0.3f));
                StartCoroutine(interactive3dFadeButton(btnTransform, false, 0.3f));
                StartCoroutine(interactive3dFadeButton(btnAnimation, false, 0.3f));
                StartCoroutine(interactive3dFadeBottomBar(true));
                break;
            case Interactive3DViewMode.Transform:
                StartCoroutine(interactive3dFadeButton(btnPartInspection, false, 0.3f));
                StartCoroutine(interactive3dFadeButton(btnCarPaint, false, 0.3f));
                StartCoroutine(interactive3dFadeButton(btnAnimation, false, 0.3f));
                StartCoroutine(interactive3dFadeBottomBar(false));
                break;
            case Interactive3DViewMode.PartInspection:
                StartCoroutine(interactive3dFadeButton(btnTransform, false, 0.3f));
                StartCoroutine(interactive3dFadeButton(btnCarPaint, false, 0.3f));
                StartCoroutine(interactive3dFadeButton(btnAnimation, false, 0.3f));
                StartCoroutine(interactive3dFadeBottomBar(true));
                break;
            case Interactive3DViewMode.Default:
                StartCoroutine(interactive3dFadeButton(btnPartInspection, true, 0.3f, _wait));
                StartCoroutine(interactive3dFadeButton(btnTransform, true, 0.3f, _wait));
                StartCoroutine(interactive3dFadeButton(btnCarPaint, true, 0.3f, _wait));
                StartCoroutine(interactive3dFadeButton(btnAnimation, true, 0.3f, _wait));
                StartCoroutine(interactive3dFadeBottomBar(false));
                break;
            default:
                StartCoroutine(interactive3dFadeButton(btnPartInspection, true, 0.3f, _wait));
                StartCoroutine(interactive3dFadeButton(btnTransform, true, 0.3f, _wait));
                StartCoroutine(interactive3dFadeButton(btnCarPaint, true, 0.3f, _wait));
                StartCoroutine(interactive3dFadeButton(btnAnimation, true, 0.3f, _wait));
                StartCoroutine(interactive3dFadeBottomBar(false));
                break;
        }
    }

    private IEnumerator interactive3dFadeBottomBar(bool _show) {
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

    private void interactive3dAnimRoof() {
        if (isRoofClosed) {
            imgAnimIconRoof.sprite = spriteAnimIconOn;
            interactive3dObject.GetComponent<Animator>().SetTrigger(triggerRoofOpenHash);
            isRoofClosed = false;
        }
        else {
            imgAnimIconRoof.sprite = spriteAnimIconOff;
            interactive3dObject.GetComponent<Animator>().SetTrigger(triggerRoofCloseHash);
            isRoofClosed = true;
        }
    }
    
    private IEnumerator waitAnimationFinish(float _duration) {
        yield return new WaitForSeconds(_duration);
        isAnimationFinished = true;
    }

    private IEnumerator interactive3dAnimDoor() {
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

    private void interactive3dAnimBonnet() {
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

    private void interactive3dInspectPart(int _id) {
        if (_id == 0) {
            rotateGuideObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
        else {
            rotateGuideObject.transform.LookAt(new Vector3(arCamera.transform.position.x, 0, arCamera.transform.position.z));
            rotateGuideObject.transform.localRotation = Quaternion.Euler(new Vector3(0, rotateGuideObject.transform.localRotation.eulerAngles.y + listOfInspectPartRotationEuler[_id].y, rotateGuideObject.transform.localRotation.eulerAngles.z));
        }
        interactive3dObject.transform.DOLocalRotateQuaternion(Quaternion.Euler(rotateGuideObject.transform.localRotation.eulerAngles), 2.5f);
        txtInspectPartName.text = listofInspectPartName[_id];
    }
    #endregion

    #region Interactive Video Functions
    public void onDetectVideoInteractive() {
        videoPlayer.Play();
        Debug.Log("Play " + videoPlayer.isPlaying);
    }
    public void onLostVideoInteractive() {
        videoPlayer.Pause();
        UIvideo.SetActive(false);
    }
    private void UpdateVideoPlayer() {

        if (videoPlayer.isPlaying) {
            Play.SetActive(false);
            Pause.SetActive(true);
        }
        else {
            Play.SetActive(true);
            Pause.SetActive(false);
        }

        if (videoPlayer.frameCount > 0) {
            progress.fillAmount = (float)videoPlayer.frame / (float)videoPlayer.frameCount;
        }

        if(audioVideo.mute) {
            MuteButton.SetActive(false);
            unMuteButton.SetActive(true);
        }
        else {
            MuteButton.SetActive(true);
            unMuteButton.SetActive(false);
        }

        if(UIvideo.active) {
            timerUI -= Time.deltaTime;
            if(timerUI <= 0) {
                UIvideo.SetActive(false);
                timerUI = 3.0f;
            }
        }
        if(!UIvideo.active) {
            MarkerVideo.GetComponent<BoxCollider>().enabled = true;
            timerUI = 3.0f;
        }

    }

    public void ActiveUI() {
        MarkerVideo.GetComponent<BoxCollider>().enabled = false;
        UIvideo.active = !UIvideo.active;
    }

    public void ForwardTime() {
        videoPlayer.time = videoPlayer.time + 5;
    }

    public void BackwardTime() {
        videoPlayer.time = videoPlayer.time - 5;
    }

    public void StartTimerUI() {
        timerUI = 3.0f;
    }


    /*void IDragHandler.OnDrag(PointerEventData eventData) {
        TrySkip(eventData);
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
        TrySkip(eventData);
    }
    private void TrySkip(PointerEventData eventData) {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(progress.rectTransform, eventData.position, null, out localPoint)) {
            float pct = Mathf.InverseLerp(progress.rectTransform.rect.xMin, progress.rectTransform.rect.xMax, localPoint.x);
            SkipToPercent(pct);
        }
    }

    private void SkipToPercent(float pct) {
        var frame = videoPlayer.frameCount * pct;
        videoPlayer.frame = (long)frame;
    }*/

    #endregion

    #region Hyperlink Functions
    public void onDetectHyperlink() {
        BGlink.GetComponent<Image>().DOFade(0.5f, 0.5f);
        Invoke("FadeLogo", 1);

    }

    public void onLostHyperlink() {
        BGlink.GetComponent<Image>().DOFade(0, 0);
        Logo.GetComponent<Image>().DOFade(0, 0);
    }

    public void FadeLogo() {
        Logo.GetComponent<Image>().DOFade(1f, 0.5f);
    }

    public void onTapHyperlink() {
        Application.OpenURL("https://www.instagram.com/hello.continuum/");
    }
    #endregion

    #region Panoramic Image Functions
    private void updateTexturePosition() {
        float offset = Time.time * scrollSpeed;
        panoramicRenderer.material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
    }

    private void updatePanoramicPosition() {
        if (panoramicMoveSpeed != 0)
            //Debug.Log("Is Moving " + isPanoramicMoving + "\nIs Right " + isPanoramicMovingRight + "\nSpeed " + panoramicMoveSpeed.ToString("0.00000") + "\nOffset " + panoramicMaterial.mainTextureOffset.x.ToString("0.00000"));
        if (isPanoramicMoving) {
            if (isPanoramicMovingRight) {
                if (panoramicMaterial.mainTextureOffset.x > 0.856f) {
                    if (isPanoramicMovingRight)
                        isPanoramicMovingRight = false;
                    if (isPanoramicMoving) {
                        panoramicMoveSpeed = 0;
                        isPanoramicMoving = false;
                        StartCoroutine(commencePanoramicMove());
                    }
                }
                else {
                    panoramicMaterial.mainTextureOffset += new Vector2(panoramicMoveSpeed * Time.deltaTime, 0);
                    if (panoramicMoveSpeed == 0.086f && panoramicMaterial.mainTextureOffset.x > 0.743) {
                        Debug.Log("Deaccelerate");
                        DOTween.To(() => panoramicMoveSpeed, x => panoramicMoveSpeed = x, 0, 4);
                    }
                }
            }
            else {
                if (panoramicMaterial.mainTextureOffset.x < 0) {
                    if (!isPanoramicMovingRight)
                        isPanoramicMovingRight = true;
                    if (isPanoramicMoving) {
                        panoramicMoveSpeed = 0;
                        isPanoramicMoving = false;
                        StartCoroutine(commencePanoramicMove());
                    }
                }
                else {
                    panoramicMaterial.mainTextureOffset -= new Vector2(panoramicMoveSpeed * Time.deltaTime, 0);
                    if (panoramicMoveSpeed == 0.086f && panoramicMaterial.mainTextureOffset.x < 0.11) {
                        Debug.Log("Deaccelerate");
                        DOTween.To(() => panoramicMoveSpeed, x => panoramicMoveSpeed = x, 0, 4);
                    }
                }
            }
        }
        else {

        }
    }

    private IEnumerator commencePanoramicMove() {
        yield return new WaitForSeconds(3f);
        isPanoramicMoving = true;
        DOTween.To(() => panoramicMoveSpeed, x => panoramicMoveSpeed = x, 0.086f, 4);
    }

    private void onDetectPanoramic() {
        Debug.Log("Mat offset " + panoramicMaterial.mainTextureOffset);
        if (!panoramicDetected) {
            panoramicDetected = true;
            StartCoroutine(startPanoramicMove());
        }
    }
    private void onLostPanoramic() {
        if (panoramicDetected) {
            panoramicDetected = false;
            DOTween.Kill(panoramicMoveSpeed);
            panoramicMoveSpeed = 0;
            panoramicMaterial.SetTextureOffset("_MainTex", new Vector2(0.048f, 0));
        }
    }

    private IEnumerator startPanoramicMove() {
        yield return new WaitForSeconds(3);
        isPanoramicMoving = true;
        DOTween.To(() => panoramicMoveSpeed, x => panoramicMoveSpeed = x, 0.086f, 4);
    }
    #endregion
}

