using UnityEngine;

/* Created by Farzad Hemmati
 * Maintains the desired aspect ratio when entering fullscreen.
 * In WebGL: on small screens while not in fullscreen, scales down the image within the fixed canvas so the whole game is still visible.
 *  This allows the game to be published with a larger canvas to show a higher resolution of the game while still being compatible with smaller screens.
 * Place on the scene's camera. 
 * Uncheck resizable window. */

public class CameraFixer : MonoBehaviour
{
    private Camera cam;
    private static readonly float antecedent = 9, consequent = 16;
    private static readonly (float antecedent, float consequent, float ratio) aspectRatio = (antecedent, consequent, antecedent / consequent);
    private static bool lastKnownFSState;

    private void Awake() => cam = GetComponent<Camera>();

    void Start()
    {
        lastKnownFSState = Screen.fullScreen;
        FixScreenSize();
    }

    private void Update()
    {
        //or call SetAspectRatio() dirrectly when entering fullscreen
        if (lastKnownFSState != Screen.fullScreen)
        {
            lastKnownFSState = Screen.fullScreen;
            if (Screen.fullScreen)
                SetAspectRatio();
        }
    }

    public void FixScreenSize()
    {
#if UNITY_WEBGL
        if (Screen.fullScreen)
            SetAspectRatio();
        else
            SetSmallSize();
#endif
#if UNITY_STANDALONE
        SetAspectRatio();
#endif 
    }

    public void SetAspectRatio()
    {
        //current viewport height should be scaled by this amount
        float scaledHeight = (float)Screen.width / Screen.height / targetAspect;

        Rect rect = cam.rect;
        if (scaledHeight != 1f)
        {
            if (scaledHeight < 1f)
            {
                //scaled height is less than current height, add letterbox
                rect.width = 1.0f;
                rect.height = scaledHeight;
                rect.x = 0;
                rect.y = (1.0f - scaledHeight) / 2.0f;
            }
            else
            {
                //called height is more than current height, add pillarbox
                float scalewidth = 1.0f / scaledHeight;
                rect.width = scalewidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scalewidth) / 2.0f;
                rect.y = 0;
            }
            cam.rect = rect;
        }
    }

#if UNITY_WEBGL
    void SetSmallSize()
    {
        //size of the game when published on the web
        int webCanvasWidth = 1280; 
        int webCanvasHight = 720;

        if ((Screen.currentResolution.width < 1920 || Screen.currentResolution.height < 1080) && !Screen.fullScreen)
        {
            //user's computer screen is smaller than 1920x1080, so our default WebGL viewport of 1280x720 would be large or too large
            //scale down the image within the 1280x720 box, bottom left alligned

            Rect rect = cam.rect;
            rect.x = rect.y = 0;
            float usableX = Screen.currentResolution.width * 0.99f; //room for right scroll bar
            float usableY = Screen.currentResolution.height * 0.85f; //room for internet browser top and task bar bottom

            rect.width = usableX / webCanvasWidth;
            rect.height = usableY / webCanvasHight;
            if (usableX > usableY / consequent * antecedent) //screen too long for 16:9
                rect.width = usableY / consequent * antecedent / webCanvasWidth;
            else if (usableX < usableY / consequent * antecedent) //screen too tall for 16:9
                rect.height = usableX / antecedent * consequent / webCanvasHight;
            cam.rect = rect;
        }
    }
#endif

}
