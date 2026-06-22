using Assets.Scripts;
using UnityEngine;

public class CockpitSwitch : MonoBehaviour, IToggleable
{
    [Header("Animator")]
    public Animator animator;

    [Header("Состояние")]
    public bool isOn = false;

    [Header("Инженерное описание тумблера")]
    [TextArea] public string switchDescription = "Описание тумблера";

    public void Toggle()
    {
        isOn = !isOn;
        animator.SetBool("IsOn", isOn);
        Debug.Log(gameObject.name + " : " + isOn);
    }

    public string GetDescription()
    {
        // Возвращаем описание + текущий статус для наглядности
        string status = isOn ? "<color=#00875A>ВКЛ</color>" : "<color=#DE350B>ВЫКЛ</color>";
        return $"{switchDescription}\nСтатус: {status}";
    }
}
