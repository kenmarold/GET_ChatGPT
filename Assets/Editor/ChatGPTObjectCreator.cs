using UnityEngine;
using UnityEditor;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

public class ChatGPTObjectCreator : EditorWindow
{
    private string prompt = "Create a cube named AI_Object";
    private string apiKey = "YOUR_OPENAI_API_KEY_HERE";
    private string apiEndpoint = "https://api.openai.com/v1/chat/completions";
    private bool isGenerating = false;

    [MenuItem("Tools/ChatGPT Object Creator")]
    public static void ShowWindow()
    {
        GetWindow<ChatGPTObjectCreator>("ChatGPT Object Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("ChatGPT Object Generator", EditorStyles.boldLabel);
        prompt = EditorGUILayout.TextField("Prompt:", prompt);

        if (GUILayout.Button("Generate Object"))
        {
            GenerateObjectFromPrompt();
        }
    }

    private async void GenerateObjectFromPrompt()
    {
        isGenerating = true;
        EditorUtility.DisplayProgressBar("Generating Object", "Waiting for ChatGPT response...", 0.5f);

        string response = await GetChatGPTResponse(prompt);
        ProcessResponse(response);
        
        isGenerating = false;
        EditorUtility.ClearProgressBar();
    }

    private async Task<string> GetChatGPTResponse(string userPrompt)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);

            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = "You are a Unity object generator. Respond with object creation instructions." },
                    new { role = "user", content = userPrompt }
                },
                max_tokens = 100
            };

            string jsonRequest = JsonConvert.SerializeObject(requestBody);
            HttpContent content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(apiEndpoint, content);

            string jsonResponse = await response.Content.ReadAsStringAsync();
            Debug.Log("ChatGPT API Response: " + jsonResponse); // Log response

            if (response.IsSuccessStatusCode)
            {
                return jsonResponse;
            }
            else
            {
                Debug.LogError("API Error: " + response.ReasonPhrase);
                return "";
            }
        }
    }

    private void ProcessResponse(string jsonResponse)
    {
        if (string.IsNullOrEmpty(jsonResponse)) return;
        
        Debug.Log("Processing Response: " + jsonResponse); // Log processing

        dynamic responseObject = JsonConvert.DeserializeObject(jsonResponse);
        string chatGPTText = responseObject.choices[0].message.content;

        GameObject newObject = null;
        
        if (chatGPTText.Contains("cube"))
        {
            newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newObject.name = "AI_Cube";
        }
        else if (chatGPTText.Contains("sphere"))
        {
            newObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            newObject.name = "AI_Sphere";
        }
        else
        {
            Debug.LogWarning("ChatGPT response did not contain a recognizable object creation command.");
            newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newObject.name = "Test_Cube";
        }
        
        if (newObject != null)
        {
            newObject.transform.position = Vector3.zero;
            AssignColorFromText(newObject, chatGPTText);
        }
    }

    private void AssignColorFromText(GameObject obj, string text)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null) return;
        
        Color objectColor = Color.white; // Default color
        string colorName = ExtractColorFromText(text);
        
        if (ColorUtility.TryParseHtmlString(colorName, out Color parsedColor))
        {
            objectColor = parsedColor;
        }
        else
        {
            Debug.LogWarning("Could not parse color: " + colorName);
        }
        
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (material == null)
        {
            material = new Material(Shader.Find("Standard")); // Fallback
        }
        material.color = objectColor;
        renderer.material = material;
    }

    private string ExtractColorFromText(string text)
    {
        string[] colors = { "red", "green", "blue", "yellow", "cyan", "magenta", "black", "white", "gray", "orange", "purple", "pink" };
        foreach (string color in colors)
        {
            if (text.ToLower().Contains(color))
            {
                return color;
            }
        }
        return "white"; // Default if no color is found
    }
}