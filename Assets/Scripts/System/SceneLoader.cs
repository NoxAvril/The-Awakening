using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadCharacterSelect()
    {
        SceneManager.LoadScene("CharacterSelect");
    }
}
