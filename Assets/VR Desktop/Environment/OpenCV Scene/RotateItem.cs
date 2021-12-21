using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ItemCarousel))]
public class RotateItem : MonoBehaviour {

    [SerializeField]
    float speed = 10;

    ItemCarousel carousel;


    private void Awake()
    {
        carousel = GetComponent<ItemCarousel>();
    }

    private void Update()
    {
        if (carousel.CurrentItem)
            carousel.CurrentItem.Rotate(Vector3.up * Time.deltaTime * speed);
    }


    public void SetSpeedFromField(InputField field)
    {
        speed = float.Parse(field.text);
    }
}
