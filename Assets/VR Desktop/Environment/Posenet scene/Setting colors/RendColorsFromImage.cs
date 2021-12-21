using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RendColorsFromImage : MonoBehaviour
{
    [SerializeField]
    Renderer[] paintRenderers;
    [SerializeField]
    RendererToFilter[] renderersWithSpecificMaterialIndexes;

    public void ColorFromImage(Image image)
    {
        foreach (Renderer rend in paintRenderers)
        {
            List<RendererToFilter> rendsToFilter = new List<RendererToFilter>();
            if (renderersWithSpecificMaterialIndexes.Length > 0)
                foreach (RendererToFilter rendToFilter in renderersWithSpecificMaterialIndexes)
                    if (rend == rendToFilter.rend)
                        rendsToFilter.Add(rendToFilter);
            if (rendsToFilter.Count < 1)
                foreach (Material mat in rend.materials)
                    mat.color = image.color;
            else
                // Only change the color of the specified indexes.
                for (int i = 0; i < rend.materials.Length; i++)
                {
                    foreach (RendererToFilter rendToFilter in rendsToFilter)
                        foreach (int index in rendToFilter.indexesToUse)
                            if (i == index)
                                rend.materials[i].color = image.color;
                }
        }
    }

    [System.Serializable]
    struct RendererToFilter
    {
        public Renderer rend;
        public int[] indexesToUse;
    }
}