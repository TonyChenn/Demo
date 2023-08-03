using UnityEngine;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		AssetBundle bundle = AssetBundle.LoadFromFile($"{Application.streamingAssetsPath}/scenes/start.u");
    }
}
