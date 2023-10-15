using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

using System.Linq;
using System.Collections.Generic;
using TMPro;

public class ScrollDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string charLoc = "scroll";
    private Transform parentAfterDrag;
    private Vector3 offset;
    private Transform originalParent;
    private GridLayoutGroup gridLayoutGroup;
    private int originalIndex;
    public bool isDragging = false;
    public WordPair wordPair;
    private RectTransform scrollViewRect;
    public RectTransform objectRectTransform;
    public TextMeshProUGUI textMeshPro;
    private CombinationRuleLoader ruleLoader;
    private bool collisionProcessed = false;
    private TextMeshProUGUI textField;
    private GameUpdater gameUpdater;
    public AudioClip pop;
    public AudioSource audioSource;
    public GameObject ScrollChar;
    public Transform contentParent;

    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = pop;

        if (charLoc == "scroll") 
        {
            gridLayoutGroup = GetComponentInParent<GridLayoutGroup>();
            originalParent = transform.parent;
            scrollViewRect = GetComponentInParent<ScrollRect>().viewport.GetComponent<RectTransform>();
            objectRectTransform = GetComponent<RectTransform>();
        }
    }

    private void Awake()
    {
        ruleLoader = GameObject.FindObjectOfType<CombinationRuleLoader>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        if (charLoc == "scroll") 
        {
            originalIndex = transform.GetSiblingIndex(); 

            parentAfterDrag = transform.parent;
            transform.SetParent(transform.root);
            transform.SetAsLastSibling();
        }
        
        offset = transform.position - Input.mousePosition;
        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            textField = GameObject.Find("HoverText").GetComponent<TextMeshProUGUI>();
            textField.text = wordPair.word;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            textField.text = "";
        }
        if (charLoc == "scroll") {
            if (!IsObjectInsideRectTransform(scrollViewRect, objectRectTransform))
            {
                CreateCloneAtCursor();
            }

            transform.SetParent(parentAfterDrag);
            transform.SetParent(originalParent);

            if (gridLayoutGroup != null)
            {
                transform.SetSiblingIndex(originalIndex);
            }         
        }

        else if (charLoc == "canvas"){
            if (IsInsideScrollViewLeftEdge())
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        GameObject obj1 = gameObject;
        GameObject obj2 = other.gameObject;

        if (
            !obj1.GetComponent<ScrollDrag>().collisionProcessed
            && !obj2.GetComponent<ScrollDrag>().collisionProcessed
            && charLoc == "canvas" 
            && !obj1.GetComponent<ScrollDrag>().isDragging 
            && !obj2.GetComponent<ScrollDrag>().isDragging
           )
        { 
            string selectedWord1 = obj1.GetComponent<ScrollDrag>().wordPair.word;
            string selectedWord2 = obj2.GetComponent<ScrollDrag>().wordPair.word;

            // Check if a combination rule matches the selected words of both objects
            foreach (CombinationRule rule in ruleLoader.combinationRules)
            {
                if (selectedWord1 == rule.word1 && selectedWord2 == rule.word2)
                { 
                    obj1.GetComponent<ScrollDrag>().collisionProcessed = true;
                    obj2.GetComponent<ScrollDrag>().collisionProcessed = true;

                    Destroy(obj1);
                    Destroy(obj2);
                    
                    // Combine the objects and create a new one based on the resultPrefab
                    GameObject resultObject = CreateCloneAtCursor();
                    resultObject.GetComponent<ScrollDrag>().wordPair = rule.wordPair;
                    resultObject.GetComponent<TextMeshProUGUI>().text = rule.wordPair.wordZh;
                    resultObject.GetComponent<TextMeshProUGUI>().enabled = true;
                    resultObject.GetComponent<BoxCollider2D>().enabled = true;
                    resultObject.GetComponent<ScrollDrag>().enabled = true;
                    resultObject.GetComponent<AudioSource>().enabled = true;

                    resultObject.GetComponent<ScrollDrag>().audioSource.Play();

                    ScrollDragInitializer scrollDragInitializer = GetComponentInParent<Canvas>().GetComponentInChildren<ScrollDragInitializer>();
                    if (SceneManager.GetActiveScene().name == "Game")
                    {
                        gameUpdater = GetComponentInParent<Canvas>().GetComponent<GameUpdater>();
                        gameUpdater.CheckAttempt(rule.wordPair);
                    }
                    List<WordPair> wordPairs = scrollDragInitializer.parameterList;

                    if (!wordPairs.Any(pair => pair.word == rule.wordPair.word))
                    {
                        wordPairs.Add(rule.wordPair);
                    }

                    scrollDragInitializer.UpdateScrollDragsFromParameterList(true);

                    BoxCollider2D boxCollider = resultObject.GetComponent<BoxCollider2D>();
                    boxCollider.size = new Vector2(rule.wordPair.charLen * 30, boxCollider.size.y);
                    boxCollider.offset = new Vector2(rule.wordPair.charLen * 15, boxCollider.offset.y);
                }
            }

            ScrollDrag[] scrollDrags = GetComponentInParent<Canvas>().GetComponentsInChildren<ScrollDrag>();
            foreach (var drag in scrollDrags)
            {
                drag.GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 0, 255);
            }
        }
    }

    private bool IsInsideScrollViewLeftEdge()
    {
        RectTransform canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        ScrollRect scrollRect = GetComponentInParent<Canvas>().GetComponentInChildren<ScrollRect>();
        RectTransform objectRect = GetComponent<RectTransform>();

        Vector3 objectCenter = objectRect.TransformPoint(objectRect.rect.center);
        Vector3 scrollLeftEdge = scrollRect.content.TransformPoint(new Vector3(scrollRect.content.rect.xMin, 0, 0));

        // Convert world positions to screen positions
        Vector2 objectCenterScreen = RectTransformUtility.WorldToScreenPoint(null, objectCenter);
        Vector2 scrollLeftEdgeScreen = RectTransformUtility.WorldToScreenPoint(null, scrollLeftEdge);

        // Check if the object's center is to the left of the scroll view's left edge
        return objectCenterScreen.x > scrollLeftEdgeScreen.x;
    }

    private GameObject CreateCloneAtCursor()
    {
        GameObject clone = Instantiate(ScrollChar, contentParent);
        clone.transform.SetParent(transform.parent, false);
        clone.transform.position = Input.mousePosition + offset;
        clone.transform.SetAsLastSibling();
        clone.GetComponent<ScrollDrag>().charLoc = "canvas";

        return clone;
    }

    private bool IsObjectInsideRectTransform(RectTransform parentRect, RectTransform childRect)
    {
        if (parentRect == null || childRect == null)
        {
            return false;
        }

        Rect parentRectInWorld = GetWorldRect(parentRect);
        Rect childRectInWorld = GetWorldRect(childRect);

        return parentRectInWorld.Contains(childRectInWorld.min) || parentRectInWorld.Contains(childRectInWorld.max);
    }

    private Rect GetWorldRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        Vector3 min = corners[0];
        Vector3 max = corners[2];

        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }
}
