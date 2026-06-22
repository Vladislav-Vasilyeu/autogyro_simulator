using Assets.Scripts;
using UnityEngine;

public class StarterTrigger : MonoBehaviour, IToggleable
{

    [Header("Animator")]
    public Animator animator;

    [Header("Ссылка на систему систем")]
    public AviationSystems aviationSystems;

    [Header("Инженерное описание кнопки")]
    [TextArea] public string buttonDescription = "Кнопка стартера двигателя";

    public void Toggle()
    {
        if (aviationSystems == null) return;

        if (aviationSystems.TryStartEngine())
        {
            animator.SetTrigger("EngineStart");
            Debug.Log(gameObject.name + " : Тригер вызван");
        }
    }

    
    public string GetDescription()
    {
        return buttonDescription;
    }
}
