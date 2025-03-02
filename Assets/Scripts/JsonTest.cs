using UnityEngine;
using Newtonsoft.Json;

public class JsonTest : MonoBehaviour
{
    void Start()
    {
        var json = JsonConvert.SerializeObject(new { message = "Hello, Unity!" });
        Debug.Log(json);
    }
}