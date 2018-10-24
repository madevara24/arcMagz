using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class ImageTargetBehaviour : MonoBehaviour, ITrackableEventHandler {
    public enum ImageTargetType {PanoramicImage, InteractiveImage, Interactive3D, InteractiveVideo, Hyperlink, InteractiveCLoud}
    [SerializeField] ImageTargetType markerType = ImageTargetType.InteractiveImage;
    private TrackableBehaviour mTrackableBehaviour;
    private void Start() {
        mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (mTrackableBehaviour) {
            mTrackableBehaviour.RegisterTrackableEventHandler(this);
        }
    }
    //TRACKABLE DISPATCH EVENT #########################################################################################################
    public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus) {
        if (newStatus == TrackableBehaviour.Status.DETECTED || newStatus == TrackableBehaviour.Status.TRACKED || newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED) {
            onMarkerDetected();
        }
        else if (newStatus == TrackableBehaviour.Status.NO_POSE) {
            onMarkerLost();
        }
    }

    private void onMarkerLost() {
        switch (markerType) {
            case ImageTargetType.InteractiveCLoud:
            EventManager.TriggerEvent(ObjectCloudCloudBehaviour.EVENT_INTERACTIVE_CLOUD_LOST);
            break;
        case ImageTargetType.PanoramicImage:
                EventManager.TriggerEvent(CoreController.EVENT_PARONAMIC_IMAGE_LOST);
                //DO SOMETHING
                break;
            case ImageTargetType.InteractiveImage:
                //DO SOMETHING
                break;
            case ImageTargetType.Interactive3D:
                GameObject.Find("Core Controller").GetComponent<CoreController>().isInteractive3dMarkerDetected = false;
                //DO SOMETHING
                break;
            case ImageTargetType.InteractiveVideo:
                EventManager.TriggerEvent(CoreController.EVENT_INTERACTIVE_VIDEO_LOST);
                break;
            case ImageTargetType.Hyperlink:
                EventManager.TriggerEvent(CoreController.EVENT_HYPERLINK_IMAGE_LOST);
                break;
            default:
                break;
        }
    }

    private void onMarkerDetected() {
        switch (markerType) {
            case ImageTargetType.InteractiveCLoud:
                EventManager.TriggerEvent(ObjectCloudCloudBehaviour.EVENT_INTERACTIVE_CLOUD_DETECTED);
                break;
            case ImageTargetType.PanoramicImage:
                EventManager.TriggerEvent(CoreController.EVENT_PARONAMIC_IMAGE_DETECTED);
                //DO SOMETHING
                break;
            case ImageTargetType.InteractiveImage:
                //DO SOMETHING
                break;
            case ImageTargetType.Interactive3D:
                GameObject.Find("Core Controller").GetComponent<CoreController>().isInteractive3dMarkerDetected = true;
                EventManager.TriggerEvent(CoreController.EVENT_INTERACTIVE_3D_DETECTED);
                //DO SOMETHING
                break;
            case ImageTargetType.InteractiveVideo:
                EventManager.TriggerEvent(CoreController.EVENT_INTERACTIVE_VIDEO_DETECTED);
                break;
            case ImageTargetType.Hyperlink:
                EventManager.TriggerEvent(CoreController.EVENT_HYPERLINK_IMAGE_DETECTED);
                break;
            default:
                break;
        }
    }

    private void OnMouseDown() {
        switch (markerType) {
            case ImageTargetType.PanoramicImage:
                break;
            case ImageTargetType.InteractiveImage:
                //EventManager.TriggerEvent(CoreController.EVENT_INTERACTIVE_IMAGE_OPEN_GALLERY);
                //DO SOMETHING
                break;
            case ImageTargetType.Interactive3D:
                //DO SOMETHING
                break;
            case ImageTargetType.InteractiveVideo:
                Debug.Log("TapMarkerVideo");
                EventManager.TriggerEvent(CoreController.EVENT_INTERACTIVE_VIDEO_TAP);
                break;
            case ImageTargetType.Hyperlink:
                EventManager.TriggerEvent(CoreController.EVENT_HYPERLINK_TAP);
                break;
            default:
                break;
        }
    }
}
