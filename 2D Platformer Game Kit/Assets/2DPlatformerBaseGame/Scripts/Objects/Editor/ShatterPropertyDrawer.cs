using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Shatter.ShatterDetails))]
public class SpriteShatterPropertyDrawer : PropertyDrawer {

    //Get property height.
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        SpriteRenderer spriteRenderer = ((Shatter) property.serializedObject.targetObject).gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return base.GetPropertyHeight(property, label) * 2;
        TextureImporter textureImporter = (TextureImporter) AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(spriteRenderer.sprite));
        if (textureImporter == null)
            return base.GetPropertyHeight(property, label) * 2;
        bool textureIsReadable = ((TextureImporter) AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(spriteRenderer.sprite))).isReadable;
        property.Next(true);
        int horizontalCuts = property.intValue;
        property.Next(true);
        int verticalCuts = property.intValue;
        property.Next(true);
        bool randomizeAtRuntime = property.boolValue;
        return (base.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing) * ((textureIsReadable ? 17 : 23) +
                (randomizeAtRuntime ? 0 : 1) + (horizontalCuts == 0 && verticalCuts == 0 ? 1 : 0)) +
                (textureIsReadable ? (int) (((float) (int) spriteRenderer.sprite.rect.height / (float) (int) spriteRenderer.sprite.rect.width) *
                Screen.width * 0.75f) : 0);
    }

    //On GUI.
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        
        //Set the row height and initial position.
        float rowHeight = base.GetPropertyHeight(property, label);
        position = new Rect(position.xMin, position.yMin, position.width, rowHeight);
        rowHeight += EditorGUIUtility.standardVerticalSpacing;

        //If the sprite doesn't have its "readable" flag set, warn the user and given them the option to set it.
        SpriteRenderer spriteRenderer = ((Shatter) property.serializedObject.targetObject).gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null) {
            position.height *= 2;
            EditorGUI.HelpBox(position, "Sprite Renderer does not have a valid sprite!", MessageType.Error);
            return;
        }
        string assetPath = AssetDatabase.GetAssetPath(spriteRenderer.sprite);
        TextureImporter textureImporter = (TextureImporter) AssetImporter.GetAtPath(assetPath);
        if (textureImporter == null) {
            position.height *= 2;
            EditorGUI.HelpBox(position, "Sprite Renderer does not have a valid sprite!", MessageType.Error);
            return;
        }
        if (!textureImporter.isReadable) {
            addRow(ref position, rowHeight);
            position.height *= 2;
            EditorGUI.HelpBox(position, "Texture \"Read/Write\" flag not set!", MessageType.Error);
            position.height /= 2;
            addRow(ref position, rowHeight * 2);
            if (GUI.Button(position, new GUIContent("Set Texture Read/Write Flag", "Sprite Shatter requires that the sprite's texture has its \"Read/Write\"" +
                    "flag set. This is to ensure that the pixels can be read from the texture in order to shatter it. The flag is not currently set on the " +
                    "texture associated with this sprite, but you can click the button below to set it."))) {
                textureImporter.isReadable = true;
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                EditorUtility.DisplayDialog("Texture Read/Write Flag Set", "The \"Read/Write\" flag has been set on the texture associated with this sprite.",
                        "OK");
            }
            addRow(ref position, rowHeight);
        }
        
        //Display the property fields.
        addRow(ref position, rowHeight);
        EditorGUI.LabelField(position, "Create Shatter Objects", EditorStyles.boldLabel);
        addRow(ref position, rowHeight);
        property.Next(true);
        EditorGUI.BeginProperty(position, label, property);
        property.intValue = EditorGUI.IntSlider(position, new GUIContent("Horizontal Cuts", "The number of cuts to make going horizontally across the " +
                "sprite. More cuts will shatter the sprite into smaller pieces, but will require more game objects and may affect performance."),
                property.intValue, 0, 32);
        EditorGUI.EndProperty();
        int horizontalCuts = property.intValue;
        addRow(ref position, rowHeight);
        property.Next(false);
        EditorGUI.BeginProperty(position, label, property);
        property.intValue = EditorGUI.IntSlider(position, new GUIContent("Vertical Cuts", "The number of cuts to make going vertically down the sprite. More " +
                "cuts will shatter the sprite into smaller pieces, but will require more game objects and may affect performance."), property.intValue, 0, 32);
        EditorGUI.EndProperty();
        int verticalCuts = property.intValue;
        addRow(ref position, rowHeight);
        if (horizontalCuts == 0 && verticalCuts == 0) {
            EditorGUI.HelpBox(position, "The sprite will not shatter without any cuts!", MessageType.Warning);
            addRow(ref position, rowHeight);
        }
        property.Next(false);
        EditorGUI.BeginProperty(position, label, property);
        property.boolValue = EditorGUI.Toggle(position, new GUIContent("Randomize at Runtime", "Generate a completely random seed at runtime, meaning the " +
                "layout of the shattered sprite pieces will be different every time the game is run. This only has any effect if \"Randomness\" is set to " +
                "greater than zero."), property.boolValue);
        EditorGUI.EndProperty();
        bool randomizeAtRuntime = property.boolValue;
        addRow(ref position, rowHeight);
        property.Next(false);
        if (!randomizeAtRuntime) {
            EditorGUI.BeginProperty(position, label, property);
            property.intValue = EditorGUI.IntField(position, new GUIContent("Random Seed", "The seed value to use when generating random numbers. The same " +
                    "seed will always generate cuts in the same way, which is useful if you want multiple objects to shatter in an identical way. Otherwise " +
                    "you can select \"Randomize at Runtime\" below to generate a different random set of cuts each time the game runs. The random seed only " +
                    "has any effect if \"Randomness\" is set to greater than zero."), property.intValue);
            EditorGUI.EndProperty();
            addRow(ref position, rowHeight);
        }
        int randomSeed = property.intValue;
        property.Next(false);
        EditorGUI.BeginProperty(position, label, property);
        property.floatValue = EditorGUI.Slider(position, new GUIContent("Randomness", "The amount of randomness to apply to the cuts. A value of zero will " +
                "result in perfectly straight cuts, whereas a value of one will result in cuts that potentially could be almost touching each other."),
                property.floatValue, 0, 1);
        EditorGUI.EndProperty();
        float randomness = property.floatValue;
        addRow(ref position, rowHeight);
        property.Next(false);
        EditorGUI.BeginProperty(position, label, property);
        property.floatValue = EditorGUI.Slider(position, new GUIContent("Zigzag Frequency", "This property, along with \"Zigzag Amplitude\" allows for a " +
                "zigzag shape to be added to the cuts to make more jagged edges and more random-looking pieces. \"Zigzag Frequency\" specifies the number of " +
                "jagged edges across or down the cuts, but will have no effect if \"Zigzag Amplitude\" is zero."), property.floatValue, 0, 1);
        EditorGUI.EndProperty();
        float zigzagFrequency = property.floatValue;
        addRow(ref position, rowHeight);
        property.Next(false);
        EditorGUI.BeginProperty(position, label, property);
        property.floatValue = EditorGUI.Slider(position, new GUIContent("Zigzag Amplitude", "This property, along with \"Zigzag Amplitude\" allows for a " +
                "zigzag shape to be added to the cuts to make more jagged edges and more random-looking pieces. \"Zigzag Frequency\" specifies the number of " +
                "jagged edges across or down the cuts, but will have no effect if \"Zigzag Amplitude\" is zero."), property.floatValue, 0, 1);
        EditorGUI.EndProperty();
        float zigzagAmplitude = property.floatValue;



        addRow(ref position, rowHeight);
        property.Next(false);
        EditorGUI.BeginProperty(position, label, property);
        property.floatValue = EditorGUI.Slider(position, new GUIContent("Time to Disappear", "This property determines how long the shatter pieces will wait before they disappear."), property.floatValue, 0, 1000);
        EditorGUI.EndProperty();
        float timeToDisappear = property.floatValue;

        ((Shatter)property.serializedObject.targetObject).shatterDetails.timeToDisappear = timeToDisappear;

        addRow(ref position, rowHeight);
        property.Next(false);
        EditorGUI.BeginProperty(position, label, property);
        property.intValue = EditorGUI.IntPopup(position, new GUIContent("Collider Types", "The collider type to add to each of the shattered pieces of the " +
                "sprite. Having no collider is the most efficient way of shattering a sprite, but the shattered pieces will not collide with anything. A " +
                "polygon collider is the least-efficient collider, but produces the most accurate collisions."), property.intValue, new GUIContent[] {
                new GUIContent("None", "Do not apply any any colliders to the shattered pieces."),
                new GUIContent("Circle", "Apply circle colliders to the shattered pieces."),
                new GUIContent("Box", "Apply box colliders to the shattered pieces."),
                new GUIContent("Polygon", "Apply polygon colliders to the shattered pieces that match the shape each piece.") },
                new int[] { 0, 1, 2, 3 });
        EditorGUI.EndProperty();
        addRow(ref position, rowHeight);
        addRow(ref position, rowHeight);
        EditorGUI.LabelField(position, "Sprite Shattering Properties", EditorStyles.boldLabel);
        addRow(ref position, rowHeight);
        property.Next(false);
        EditorGUI.LabelField(position, new GUIContent("Explode From", "The position from which to apply the explosion force when exploding the shattered " +
                "pieces of the sprite. The X co-ordinate should be 0 for the left edge of the sprite and 1 for the right edge, and the Y co-ordinate should " +
                "be 0 for bottom of the sprite and 1 for the top, although the values can be outside this range. For example to make the sprite explode up " +
                "from the bottom, co-ordinates of (0.5, 0) could be used."));
        addRow(ref position, rowHeight);
        EditorGUI.BeginProperty(position, label, property);
        property.vector2Value = EditorGUI.Vector2Field(position, "", property.vector2Value);
        EditorGUI.EndProperty();
        addRow(ref position, rowHeight);
        property.Next(false);
        EditorGUI.LabelField(position, new GUIContent("Explosion Force", "The force to apply in the X and Y direction when exploding the shattered pieces of " +
                "the sprite. A force of zero will make the sprite just collapse in its current position"));
        addRow(ref position, rowHeight);
        EditorGUI.BeginProperty(position, label, property);
        property.vector2Value = EditorGUI.Vector2Field(position, "", property.vector2Value);
        EditorGUI.EndProperty();
        addRow(ref position, rowHeight);

        //Display a preview texture with the cuts made. First display a "Preview" header.
        addRow(ref position, rowHeight);
        EditorGUI.LabelField(position, "Preview", EditorStyles.boldLabel);
        addRow(ref position, rowHeight);
        if (!textureImporter.isReadable) {
            position.height *= 2;
            EditorGUI.HelpBox(position, "Texture \"Read/Write\" flag not set!", MessageType.Error);
        }
        else {
            int textureWidth = (int) spriteRenderer.sprite.rect.width;
            int textureHeight = (int) spriteRenderer.sprite.rect.height;

            //Calculate the texture width and height from the window width.
            int previewTextureWidth = (int) (position.width * 0.75f);
            int previewTextureHeight = (int) ((textureHeight / (float) textureWidth) * position.width * 0.75f);
            Color[] sourcePixels = spriteRenderer.sprite.texture.GetPixels((int) spriteRenderer.sprite.rect.xMin, (int) spriteRenderer.sprite.rect.yMin,
                    (int) spriteRenderer.sprite.rect.width, (int) spriteRenderer.sprite.rect.height);

            //If the preview texture is at least two pixels wide...
            if (previewTextureWidth > 1) {

                //Call the method that makes the cuts as these will be drawn onto the texture.
                int[,] horizontalCutTop, verticalCutLeft;
                ((Shatter) property.serializedObject.targetObject).makeCuts(spriteRenderer.sprite, horizontalCuts, verticalCuts, randomSeed, randomness,
                        zigzagFrequency, zigzagAmplitude, out horizontalCutTop, out verticalCutLeft);

                //Create the preview texture and set its pixels from the main texture.
                Texture2D previewTexture = new Texture2D(previewTextureWidth, previewTextureHeight, TextureFormat.RGBA32, false);
                previewTexture.hideFlags = HideFlags.HideAndDontSave;
                Color[] previewTexturePixels = new Color[previewTextureWidth * previewTextureHeight];
                for (int i = 0; i < previewTextureWidth; i++)
                    for (int j = 0; j < previewTextureHeight; j++)
                        previewTexturePixels[(j * previewTextureWidth) + i] = sourcePixels[(((j * textureHeight) / previewTextureHeight) * textureWidth) +
                                ((i * textureWidth) / previewTextureWidth)];

                //Loop over the cuts and draw them on the texture (white with a black outline to cover all texture colours).
                Color[] originalPreviewTexturePixels = (Color[]) previewTexturePixels.Clone();
                for (int l = 0; l < 2; l++) {
                    for (int i = 0; i < horizontalCuts; i++)
                        for (float j = 0; j < textureWidth; j += textureWidth / position.width) {
                            int previewTextureX = (int) ((j * previewTextureWidth) / textureWidth);
                            int previewTextureY = (horizontalCutTop[i, (int) j] * previewTextureHeight) / textureHeight;
                            if (originalPreviewTexturePixels[(previewTextureY * previewTextureWidth) + previewTextureX].a > 0.001f) {
                                for (int m = l == 0 ? -1 : 0; m <= (l == 0 ? 1 : 0); m++)
                                    for (int k = l == 0 ? -1 : 0; k <= (l == 0 ? 1 : 0); k++) {
                                        if (previewTextureX + m >= 0 && previewTextureX + m < previewTextureWidth && previewTextureY + k >= 0 &&
                                                previewTextureY + k < previewTextureHeight)
                                            previewTexturePixels[((previewTextureY + k) * previewTextureWidth) + previewTextureX + m] = l == 0 ? Color.black :
                                                    Color.white;
                                    }
                            }
                        }
                    for (int i = 0; i < verticalCuts; i++)
                        for (float j = 0; j < textureHeight; j += textureHeight / position.width) {
                            int previewTextureX = (verticalCutLeft[i, (int) j] * previewTextureWidth) / textureWidth;
                            int previewTextureY = (int) ((j * previewTextureHeight) / textureHeight);
                            if (originalPreviewTexturePixels[(previewTextureY * previewTextureWidth) + previewTextureX].a > 0.001f) {
                                for (int m = l == 0 ? -1 : 0; m <= (l == 0 ? 1 : 0); m++)
                                    for (int k = l == 0 ? -1 : 0; k <= (l == 0 ? 1 : 0); k++)
                                        if (previewTextureX + m >= 0 && previewTextureX + m < previewTextureWidth && previewTextureY + k >= 0 &&
                                                previewTextureY + k < previewTextureHeight)
                                            previewTexturePixels[((previewTextureY + k) * previewTextureWidth) + previewTextureX + m] = l == 0 ? Color.black :
                                                    Color.white;
                            }
                        }
                }
                previewTexture.SetPixels(previewTexturePixels);
                previewTexture.Apply();

                //Draw the texture in the editor window.
                position.xMin += position.width * 0.125f;
                position.xMax -= position.width * 0.125f;
                position.height = previewTextureHeight;
                GUIStyle style = new GUIStyle();
                style.normal.background = previewTexture;
                EditorGUI.LabelField(position, GUIContent.none, style);
            }
        }
    }

    //Add a row to the inspector.
    void addRow(ref Rect position, float rowHeight) {
        position = new Rect(position.xMin, position.yMin + rowHeight, position.width, position.height);
    }
}
