using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject prefabToDuplicate; // Dragged image prefab
    private GameObject draggedImage; // Instantiated image
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvas = FindObjectOfType<Canvas>();
        prefabToDuplicate = this.gameObject;
    }

    // Called when the pointer is pressed down on the object
    public void OnPointerDown(PointerEventData eventData)
    {
        
      
    }

    // Called on the start of the drag
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Instantiate the duplicate image when drag begins

        draggedImage = Instantiate(prefabToDuplicate, Input.mousePosition, Quaternion.identity);
        draggedImage.transform.SetParent(canvas.transform, false);
        rectTransform = draggedImage.GetComponent<RectTransform>();
        canvasGroup = draggedImage.GetComponent<CanvasGroup>();

        // Make the image transparent while dragging
        canvasGroup.alpha = .6f;
        // Make the image non-interactable while dragging
        canvasGroup.blocksRaycasts = false;

        //SET THE AUDIO SAMPLE PRESENTLY SELECTED IN THE SAMPLE PACK MANAGER
        StartCoroutine(FindObjectOfType<SamplePackManager>().LoadAudioClipFromFile(GetComponent<AudioSampleMetadata>().path, false));

    }

    // Called while dragging
    public void OnDrag(PointerEventData eventData)
    {
        // Update the position of the dragged image
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    // Called on the end of the drag
    public void OnEndDrag(PointerEventData eventData)
    {
        // Make the image fully visible and interactable again
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        GameObject dropTarget = eventData.pointerCurrentRaycast.gameObject;
        if (dropTarget != null && (dropTarget.GetComponent<Button>() != null || dropTarget.GetComponent<EventTrigger>() != null))
        {
            if(dropTarget.name == "PlayButton")
            {
                Looper looper = dropTarget.GetComponentInParent<Looper>();
                looper.SetAudioClip(FindObjectOfType<SamplePackManager>().presentSelectedSample);
                Debug.Log("Dropped on " + looper.name);
            }
           
        }



        Destroy(draggedImage.gameObject);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Instantiate the duplicate image when clicked
       
    }


}
