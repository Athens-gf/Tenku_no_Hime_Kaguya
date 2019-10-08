using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Text))]
public class RotateText : UIBehaviour, IMeshModifier
{
    private Text textComponent;
    private string text = null;
    private char[] characters;

    // 回転させない文字群
    [SerializeField] private List<char> _nonRotatableCharacters;
    [SerializeField] static int ShiftChar = 0;
    [SerializeField] private char[] _shiftCharacters = new char[ShiftChar];
    [SerializeField] private float[] _shiftXPixels = new float[ShiftChar];
    [SerializeField] private float[] _shiftYPixels = new float[ShiftChar];

    void Update()
    {
        if (textComponent == null)
            textComponent = GetComponent<Text>();
        else if (textComponent.text != "" && textComponent.text != text)
        {
            text = textComponent.text;
            GetComponent<Graphic>()?.SetVerticesDirty();
        }
    }

    new void OnValidate()
    {
        textComponent = GetComponent<Text>();
        if (textComponent?.text != "" && textComponent.text != text)
        {
            text = textComponent.text;
            GetComponent<Graphic>()?.SetVerticesDirty();
        }
    }

    public void ModifyMesh(Mesh mesh) { }
    public void ModifyMesh(VertexHelper verts)
    {
        if (!IsActive()) return;

        List<UIVertex> vertexList = new List<UIVertex>();
        verts.GetUIVertexStream(vertexList);

        ModifyVertices(vertexList);

        verts.Clear();
        verts.AddUIVertexTriangleStream(vertexList);
    }

    void ModifyVertices(List<UIVertex> vertexList)
    {
        if (textComponent?.text != null && textComponent.text != "")
        {
            characters = textComponent.text.ToCharArray();
            if (characters.Length == 0)
                return;

            for (int i = 0, vertexListCount = vertexList.Count; i < vertexListCount; i += 6)
            {
                int index = i / 6;
                //文字の回転の制御
                if (!IsNonrotatableCharactor(characters[index]))
                {
                    var center = Vector2.Lerp(vertexList[i].position, vertexList[i + 3].position, 0.5f);
                    for (int r = 0; r < 6; r++)
                    {
                        var element = vertexList[i + r];
                        var pos = element.position - (Vector3)center;
                        var newPos = new Vector2(
                                        pos.x * Mathf.Cos(90 * Mathf.Deg2Rad) - pos.y * Mathf.Sin(90 * Mathf.Deg2Rad),
                                        pos.x * Mathf.Sin(90 * Mathf.Deg2Rad) + pos.y * Mathf.Cos(90 * Mathf.Deg2Rad)
                                    );
                        element.position = (Vector3)(newPos + center);
                        vertexList[i + r] = element;
                    }
                }
                //文字の位置の制御
                float[] shiftPixel = GetPixelShiftCharactor(characters[index]);
                if (shiftPixel[0] != 0 || shiftPixel[1] != 0)
                {
                    var center = Vector2.Lerp(vertexList[i].position, vertexList[i + 3].position, 0.5f);
                    for (int r = 0; r < 6; r++)
                    {
                        var element = vertexList[i + r];
                        Debug.Log("before：" + element.position.x + "," + element.position.y);
                        var pos = element.position - (Vector3)center;
                        var newPos = new Vector2(
                                        pos.x + shiftPixel[0],
                                        pos.y + shiftPixel[1]
                                    );
                        element.position = (Vector3)(newPos + center);
                        Debug.Log("after：" + element.position.x + "," + element.position.y);
                        vertexList[i + r] = element;
                    }
                }
            }
        }
    }

    bool IsNonrotatableCharactor(char character) => _nonRotatableCharacters.Any(x => x == character);

    float[] GetPixelShiftCharactor(char character)
    {
        int index = System.Array.IndexOf(_shiftCharacters, character);
        float[] pixel = new float[2];
        if (0 <= index && index < _shiftXPixels.Length && index < _shiftYPixels.Length)
        {
            pixel[0] = _shiftXPixels[index];
            pixel[1] = _shiftYPixels[index];
        }
        return pixel;
    }
}