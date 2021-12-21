using UnityEngine;

public class TextureScaleFollower : MonoBehaviour
{
    Renderer rend;
    float startTiling;
    [SerializeField]
    bool useX = true;

    float TextureScale {
        get {
            return useX ? rend.material.mainTextureScale.x : rend.material.mainTextureScale.y;
        }
        set {
            Vector2 newTiling;
            if (useX)
                newTiling = rend.material.mainTextureScale.SetX(value);
            else
                newTiling = rend.material.mainTextureScale.SetY(value);
            rend.material.mainTextureScale = newTiling;
        }
    }


    private void Awake()
    {
        rend = GetComponent<Renderer>();
        startTiling = useX ? rend.material.mainTextureScale.x : rend.material.mainTextureScale.y;
    }

    private void Start()
    {
        GetComponentInParent<RoomLengthChanger>().Changed += UpdateTextureSize;
    }


    void UpdateTextureSize(RoomLengthChanger scaleChanger)
    {
        TextureScale = startTiling * scaleChanger.ScaleFactor;
    }
}
