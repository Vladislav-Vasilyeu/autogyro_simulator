using UnityEngine;

public class HighlightElement : MonoBehaviour
{
    private MeshRenderer[] renderers;
    private Material[][] originalMaterials; 

    [Header("Материал подсветки")]
    
    public Material highlightMaterial;

    void Awake()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();

        // Запоминаем исходные материалы, чтобы вернуть их позже
        originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                // Сохраняем копию массива материалов конкретного рендерера
                originalMaterials[i] = renderers[i].sharedMaterials;
            }
        }
    }

    public void SetHighlight(bool state)
    {
        if (renderers == null || renderers.Length == 0) return;
        if (highlightMaterial == null)
        {
            Debug.LogError($"[HighlightElement] На объекте {gameObject.name} не назначен Highlight Material в инспекторе!");
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            if (state)
            {
                // Включаем подсветку: временно заменяем ВСЕ материалы объекта на наш материал подсветки
                Material[] highlightArray = new Material[renderers[i].sharedMaterials.Length];
                for (int j = 0; j < highlightArray.Length; j++)
                {
                    highlightArray[j] = highlightMaterial;
                }
                renderers[i].materials = highlightArray;
            }
            else
            {
                // Выключаем подсветку: возвращаем родные максовские материалы из памяти
                renderers[i].materials = originalMaterials[i];
            }
        }
    }
}