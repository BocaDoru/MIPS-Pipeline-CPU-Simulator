using UnityEngine;
using UnityEditor;
using System.Globalization;

[CustomPropertyDrawer(typeof(HexAttribute))]
public class HexAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.LabelField(position, label.text, "Use [Hex] with int properties only.");
            return;
        }

        EditorGUI.BeginChangeCheck();

        int currentValue = property.intValue;
        string hexString = "0x" + currentValue.ToString("X");
        string newHexString = EditorGUI.TextField(position, label, hexString);

        if (EditorGUI.EndChangeCheck())
        {
            string hexValue = newHexString.ToUpper();

            if (hexValue.StartsWith("0X"))
            {
                hexValue = hexValue.Substring(2);
            }

            if (int.TryParse(hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int newValue))
            {
                property.intValue = newValue;
            }
            else
            {
                Debug.LogWarning("Invalid Hex value entered: " + newHexString);
            }
        }
    }
}