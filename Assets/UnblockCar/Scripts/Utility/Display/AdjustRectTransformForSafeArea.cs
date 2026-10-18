using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustRectTransformForSafeArea : MonoHandler
{

    private RectTransform rectT;
    [HideInInspector]
    public float widthScreenSafe;
    [HideInInspector]
    public float heightScreenSafe;
    [SerializeField]
    private bool adjustForBannerAd;
    [SerializeField]
    private float bannerHeightMobile = 100;
    [SerializeField]
    private float bannerHeightTable = 100;
    [SerializeField]
    private float ratio = 2;


    private float widthTestSafe;

    private float heightTestSafe;

    [SerializeField]
    private Vector2 originOffSetMax;
    [SerializeField]
    private Vector2 originOffSetMin;
    [Header("Mobile Normal")]

    [SerializeField]
    private float addWidthScreenNormal = 0;
    [SerializeField]
    private float addHeightScreenNormal = 0;

    [Header("IPhoneX")]
    [SerializeField]
    private float addWidthScreenRatioLarge2 = 0;
    [SerializeField]
    private float addHeightcreenRatioLarge2 = 0;

    [Header("Tablet")]
    [SerializeField]
    private float addWidthScreenRatioTablet = 0;
    [SerializeField]
    private float addHeightcreenRatioTablet = 0;



    [SerializeField]
    private Vector2 addOffSetMaxTablet;
    [SerializeField]

    private Vector2 addOffSetMinTablet;
 
    private float widthSafe, heightSafe;
    [HideInInspector]
    public int resolution = -1;

    private float bannerHeight = 0;
    private float ratioTablet = 1.5f;

    private Vector2 offSetMax;
    private Vector2 offSetMin;
    private Rect[] cutouts;
    private void Start()
    {

        rectT = GetComponent<RectTransform>();

#if !UNITY_EDITOR
        int max = Mathf.Max(Screen.width, Screen.height);
        int min = Mathf.Min(Screen.width, Screen.height);
        int maxSafe = Mathf.Max((int)Screen.safeArea.width, (int)Screen.safeArea.height);
        heightScreenSafe = widthScreenSafe = max - maxSafe;
       GetResolutionSafe();
        Debug.Log(string.Format("Width : {0} - Height: {1}", Screen.width, Screen.height));
        Debug.Log(string.Format("Safe Area Width : {0} - Height: {1}", Screen.safeArea.width, Screen.safeArea.height));
        Debug.Log(string.Format("Width Screen Safe : {0},Height Screen Safe : {1}   ", widthScreenSafe, heightScreenSafe));
        if (!adjustForBannerAd)
        {
           
            bannerHeight = 0;
        }
#endif

        
    }


    private void Update()
    {
#if !UNITY_EDITOR
        UpdateUI();
#else
        ConvertResolution();
#endif

    }

    private void UpdateUI()
    {

#if UNITY_EDITOR
        int width = (int)GetMainGameViewSize().x;
        int height = (int)GetMainGameViewSize().y;
#else
        int width = Screen.width;
        int height =  Screen.height;
#endif



        if (width > height)
        {
            rectT.offsetMax = new Vector2(-widthSafe / ratio, offSetMax.y);

            rectT.offsetMin = new Vector2(heightSafe / ratio, offSetMin.y + bannerHeight);
            //            Debug.Log("LandSpace");
        }
        else
        {
            rectT.offsetMax = new Vector2(offSetMax.x, -widthSafe / ratio);

            rectT.offsetMin = new Vector2(offSetMin.x, (heightSafe / ratio) + bannerHeight);

        }
    }

    public static Vector2 GetMainGameViewSize()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
        return (Vector2)Res;
    }

    private void ConvertResolution()
    {
        int max = Mathf.Max((int)GetMainGameViewSize().x, (int)GetMainGameViewSize().y);
        int min = Mathf.Min((int)GetMainGameViewSize().x, (int)GetMainGameViewSize().y);
        rectT = GetComponent<RectTransform>();
        cutouts = Screen.cutouts;
        if (max == 2436 && min == 1125)
        {
            if (resolution != 1)
            {
                Start();
                //Iphone X Resolution
                resolution = 1;
                widthScreenSafe = max - 2202;//2202:resolution safe
                heightScreenSafe = widthScreenSafe;
                Debug.Log(string.Format("Width Iphone X : {0} ", widthScreenSafe));
            }
            for (int i = 0; i < cutouts.Length; i++)
            {
                cutouts[i].height = 0;
            }
        }
        else if (max == 1792 && min == 828)
        {
            if (resolution != 2)
            {
                Start();
                //Iphone XR Resolution
                resolution = 2;
                widthScreenSafe = max - 1636;//1636:resolution safe
                heightScreenSafe = widthScreenSafe;
                Debug.Log(string.Format("Width Iphone XR : {0} ", widthScreenSafe));
            }
            for (int i = 0; i < cutouts.Length; i++)
            {
                cutouts[i].height = 0;
            }
        }
        else if (max == 2688 && min == 1242)
        {
            if (resolution != 3)
            {
                Start();
                //Iphone XS Max Resolution
                resolution = 3;
                widthScreenSafe = max - 2454;//2454:resolution safe
                heightScreenSafe = widthScreenSafe;
                Debug.Log(string.Format("Width Iphone XS Max : {0} ", widthScreenSafe));
            }
            for (int i = 0; i < cutouts.Length; i++)
            {
                cutouts[i].height = 0;
            }

        }

        else if (max == 2960 && min == 1440)
        {
            if (resolution != 4)
            {
                Start();
                //Pixel 3XL  Resolution
                resolution = 4;
                widthScreenSafe = max - 2789;//2789:resolution safe
                heightScreenSafe = widthScreenSafe;
                Debug.Log(string.Format("Width Pixel 3XL : {0} ", widthScreenSafe));
            }

        }

        else
        {
#if UNITY_EDITOR
//            Debug.Log(string.Format("Reset Offset Screen"));
#endif
            resolution = -1;
            widthScreenSafe = 0;
            heightScreenSafe = widthScreenSafe;
        }



        GetResolutionSafe();

        if (!adjustForBannerAd)
        {

            bannerHeight = 0;
        }

        UpdateUI();
    }


    private void GetResolutionSafe()
    {

#if !UNITY_EDITOR
     int   max = Mathf.Max(Screen.width, Screen.height);
      int min = Mathf.Min(Screen.width, Screen.height);
#else
        int max = Mathf.Max((int)GetMainGameViewSize().x, (int)GetMainGameViewSize().y);
        int min = Mathf.Min((int)GetMainGameViewSize().x, (int)GetMainGameViewSize().y);
#endif

        float ratioSafe = (float)max / min;
        if (ratioSafe <= ratioTablet)
        {

            bannerHeight = bannerHeightTable;
            widthSafe = widthScreenSafe + addWidthScreenRatioTablet;
            heightSafe = heightScreenSafe + addHeightcreenRatioTablet;


            offSetMax = originOffSetMax + addOffSetMaxTablet;
            offSetMin = originOffSetMin + addOffSetMinTablet;
        }
        else if (ratioSafe >= 2)
        {
            bannerHeight = bannerHeightMobile;
            widthSafe = widthScreenSafe + addWidthScreenRatioLarge2;
            heightSafe = heightScreenSafe + addHeightcreenRatioLarge2;
            offSetMax = originOffSetMax;
            offSetMin = originOffSetMin;

        }
        else
        {
            bannerHeight = bannerHeightMobile;
            widthSafe = widthScreenSafe + addWidthScreenNormal * ((float)max / min);
            heightSafe = heightScreenSafe + addHeightScreenNormal * ((float)max / min);

            offSetMax = originOffSetMax;
            offSetMin = originOffSetMin;
        }
  
        if (cutouts == null) return;
        if (cutouts.Length > 0)
        {
            foreach (var c in cutouts)
            {

                Rect cutout = c;
#if UNITY_ANDROID

                widthSafe += cutout.height;
                heightSafe += cutout.height;
#endif


            }
        }
    }
    public override void GUIEditor()
    {

#if UNITY_EDITOR


        if (GUILayout.Button("Reset Resolution"))
        {
            resolution = -1;
        }

        ConvertResolution();


        base.GUIEditor();
#endif
    }
}
