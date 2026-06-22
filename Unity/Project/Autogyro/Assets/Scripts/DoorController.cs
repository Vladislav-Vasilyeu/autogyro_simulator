using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetBool("IsOpen", false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            bool current = anim.GetBool("IsOpen");
            anim.SetBool("IsOpen", !current);
        }
    }
}
