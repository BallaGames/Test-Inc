using UnityEditor.UI;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Movement SFX")]
    [SerializeField] private AK.Wwise.Event footstep;
    //Event for jump
    //Event for slide
    //Event for Ladder
    //

    [Header("Input/UX SFX")]
    [SerializeField] private AK.Wwise.Event interact_failed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayInteractFailed()
    {
        interact_failed.Post(gameObject);
    }
    public void PlayFootstep()
    {
        footstep.Post(gameObject);
    }
}
