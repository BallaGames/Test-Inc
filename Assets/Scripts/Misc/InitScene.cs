using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class loads from the "init scene" to the first "gameplay" scene. It exists mostly to force the transition to the menu.
/// </summary>
public class InitScene : MonoBehaviour
{
    public SceneReference menuScene;
    private void Start()
    {
        SceneManager.LoadScene(menuScene.Name);
    }
}
