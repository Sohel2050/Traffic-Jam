using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

//Link Code 
// http://sbcgamesdev.blogspot.com/2015/07/unity-tutorial-swipe-controlled.html

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Mask))]
[RequireComponent(typeof(ScrollRect))]
public class ScrollSnapRect : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler,IWindowChangeAction {

 
 
    [Tooltip("Threshold time for fast swipe in seconds")]
    public float fastSwipeThresholdTime = 0.3f;
    [Tooltip("Threshold time for fast swipe in (unscaled) pixels")]
    public int fastSwipeThresholdDistance = 100;
  
    [SerializeField]
    private float smoothScroll = .1f;
 
    [SerializeField]
    private ScrollRect scrollRectComponent;
 
    [SerializeField]
    private GridLayoutGroup gridLayoutGroup;

    [SerializeField]
    private List<Image> pageSelectionImages;
    [SerializeField]
    private int totalPage = 3;
    [SerializeField]
    private int startingPage = 0;
    [SerializeField]
    private List<RectTransform> dataObjects = new List<RectTransform>();


    private RectTransform container;
 
    [Header("Debug")]
    [SerializeField]
    private int currentPage;

    // whether lerping is in progress and target lerp position
    private bool _lerp;
    private Vector2 _lerpTo;
 
    // target position of every page
     [SerializeField]
    private List<Vector2> _pagePositions = new List<Vector2>();

    // in draggging, when dragging started and where it started
    private bool _dragging;
    private float _timeStamp;
    private Vector2 _startPosition;

    // for showing small page icons
    private bool _showPageSelection;
    private int _previousPageSelectionIndex;
    // container with Image components - one Image for each page
    

    private Vector2 velocity;
    private RectTransform rectScrollRect;
    public int GetCurrentPage { get { return currentPage; } }
    public int GetTotalPage { get { return totalPage; } }
    public List<RectTransform> GetDataObjects { get { return dataObjects; } }
    //------------------------------------------------------------------------
    public void Initialized(int totalPage)    {
        MultipleWindow.instance.AddWindowChange(this);
        dataObjects.Clear();
        actions.Clear();
        this.totalPage = totalPage;
        container = scrollRectComponent.content;
        for (int i = 0; i < totalPage; i++)
        {
            GameObject newObj = new GameObject("page_"+(i+1));
            newObj.transform.SetParent(container, false);
            RectTransform rectObj = newObj.AddComponent<RectTransform>();
            rectObj.sizeDelta = gridLayoutGroup.cellSize;
            dataObjects.Add(rectObj);
        }
        rectScrollRect = scrollRectComponent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectScrollRect);
        float width = rectScrollRect.rect.width;
        float leftX = (width - gridLayoutGroup.cellSize.x)/2;
        gridLayoutGroup.padding.left = Mathf.FloorToInt(leftX);
        gridLayoutGroup.padding.right = Mathf.FloorToInt(leftX);
        _lerp = false;
        SetPagePositions();
        SetPageSelection(startingPage);
        container = scrollRectComponent.content;
      
    }
   
    //------------------------------------------------------------------------
    void Update() {
        if (rectScrollRect != null)
        {
        
            float leftX = (rectScrollRect.rect.width - gridLayoutGroup.cellSize.x) / 2;
            gridLayoutGroup.padding.left = Mathf.FloorToInt(leftX);
            gridLayoutGroup.padding.right = Mathf.FloorToInt(leftX);
        }

        // if moving to target position
        if (_lerp) {
            // prevent overshooting with values greater than 1
           
            container.anchoredPosition = Vector2.SmoothDamp(container.anchoredPosition, _lerpTo,ref velocity, smoothScroll);
            // time to stop lerping?
            float dst = Vector2.Distance(container.anchoredPosition, _lerpTo);
            if (dst < 1) {
                // snap to target and stop lerping
                container.anchoredPosition = _lerpTo;
                _lerp = false;
                scrollRectComponent.enabled = true;
                // clear also any scrollrect move that may interfere with our lerping
                scrollRectComponent.velocity = Vector2.zero;
            }
         
          
            // switches selection icon exactly to correct page
            if (_showPageSelection) {
                SetPageSelection(GetNearestPage());
            }
        }
    }

    //------------------------------------------------------------------------
    private void SetPagePositions()
    {
        // delete any previous settings
        _pagePositions.Clear();

        // iterate through all container childern and set their positions
        for (int i = 0; i < totalPage; i++)
        {
            RectTransform child = dataObjects[i];
            Vector2 childPosition  = new Vector2(i * (gridLayoutGroup.cellSize.x +gridLayoutGroup.spacing.x), 0f);

            child.anchoredPosition = childPosition;
            
            _pagePositions.Add(-childPosition);
        }
    }

    //------------------------------------------------------------------------
 

    //------------------------------------------------------------------------
    private void LerpToPage(int aPageIndex) {
        aPageIndex = Mathf.Clamp(aPageIndex, 0, _pagePositions.Count - 1);
       
        _lerpTo = _pagePositions[aPageIndex];
        velocity = Vector3.zero;
        _lerp = true;
        currentPage = aPageIndex;
        if (actions[0] != null)
        {
            actions[0]();
        }
    }

  

    //------------------------------------------------------------------------
    private void SetPageSelection(int aPageIndex) {
        // nothing to change
        if (_previousPageSelectionIndex == aPageIndex) {
            return;
        }
        // unselect old
        if (_previousPageSelectionIndex >= 0) {
          
            pageSelectionImages[_previousPageSelectionIndex].SetNativeSize();
        }

        // select new

        pageSelectionImages[aPageIndex].SetNativeSize();

        _previousPageSelectionIndex = aPageIndex;
    }

    //------------------------------------------------------------------------
    private void NextScreen() {
        LerpToPage(currentPage + 1);
    }

    //------------------------------------------------------------------------
    private void PreviousScreen() {
        LerpToPage(currentPage - 1);
    }

    //------------------------------------------------------------------------
    private int GetNearestPage() {
        // based on distance from current position, find nearest page
        Vector2 currentPosition = container.anchoredPosition;
    
        float distance = float.MaxValue;
        int nearestPage = currentPage;
   
        for (int i = 0; i < _pagePositions.Count; i++) {
            float testDist = Vector2.SqrMagnitude(currentPosition - _pagePositions[i]);
            if (testDist < distance) {
                distance = testDist;
                nearestPage = i;
            }
        }

        return nearestPage;
    }   
    //------------------------------------------------------------------------
    public void SetPage(int aPageIndex)
    {
        aPageIndex = Mathf.Clamp(aPageIndex, 0, totalPage - 1);
        container.anchoredPosition = _pagePositions[aPageIndex];
        currentPage = aPageIndex;
        
    }

    //------------------------------------------------------------------------
    public void OnBeginDrag(PointerEventData aEventData) {
        // if currently lerping, then stop it as user is draging
        _lerp = false;
        // not dragging yet
        _dragging = false;
        scrollRectComponent.enabled = true;
    }

    //------------------------------------------------------------------------
    public void OnEndDrag(PointerEventData aEventData) {
        // how much was container's content dragged
        float difference = _startPosition.x - container.anchoredPosition.x;
        // test for fast swipe - swipe that moves only +/-1 item
        if (Time.unscaledTime - _timeStamp < fastSwipeThresholdTime &&
            Mathf.Abs(difference) > fastSwipeThresholdDistance )
             {
            if (difference > 0) {
                NextScreen();
            } else {
                PreviousScreen();
            }
        } else {
            // if not fast time, look to which page we got to
            LerpToPage(GetNearestPage());
        }
        scrollRectComponent.enabled = false;
        _dragging = false;
    }

    //------------------------------------------------------------------------
    public void OnDrag(PointerEventData aEventData) {
        if (!_dragging) {
            // dragging started
            _dragging = true;
            // save time - unscaled so pausing with Time.scale should not affect it
            _timeStamp = Time.unscaledTime;
            // save current position of cointainer
            _startPosition = container.anchoredPosition;
        } else {
            if (_showPageSelection) {
                SetPageSelection(GetNearestPage());
            }
        }
    }


    #region Extension
    private List<UnityAction> actions = new List<UnityAction>();

    public void AddParent(RectTransform obj,int position)
    {
        obj.transform.SetParent(dataObjects[position],false);
        obj.anchorMin = Vector2.zero;
        obj.anchorMax = Vector2.one;
        obj.offsetMin = Vector2.zero;
        obj.offsetMax = Vector2.zero;
    }

    public void AddAction(UnityAction action)
    {
        actions.Add(action);
    }
    #endregion

    #region Interface
    public void WindowChangeUpdate()
    {
        _pagePositions.Clear();

      
        for (int i = 0; i < totalPage; i++)
        {
            RectTransform child = dataObjects[i];
            Vector2 childPosition = new Vector2(i * (gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x), 0f);
            _pagePositions.Add(-childPosition);
        }

        SetPage(currentPage);

    }
    public void WindowChangeStart()
    {
     
      

    }
    #endregion
}
