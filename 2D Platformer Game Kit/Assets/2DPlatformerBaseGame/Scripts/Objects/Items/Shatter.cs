using System;
using UnityEngine;


[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class Shatter : MonoBehaviour
{

    //Enumerated Types.
    public enum ColliderType
    {
        None,
        Circle,
        Box,
        Polygon
    };

    //Classes;
    [Serializable]
    public class ShatterDetails
    {
        //Properties.
        public int horizontalCuts = 8, verticalCuts = 8;
        public bool randomizeAtRunTime = false;
        public int randomSeed = 0;
        public float randomness = 0.5f;
        public float zigzagFrequency = 0f;
        public float zigzagAmplitude = 0f;
        public float timeToDisappear = 0f;
        public ColliderType colliderType;
        public Vector2 explodeFrom = new Vector2(0.5f, 0.5f);
        public Vector2 explosionForce = new Vector2(0, 5);
    }

    //Properties.
    public ShatterDetails shatterDetails;

    //Variables.
    Vector3[] originalShatterPieceLocations;
    Quaternion[] originalShatterPieceRotations;
    Transform shatterGameObjectTransform = null;
    Color[] sourcePixels;
    bool error = false;

    //Start.
    void Start() {

        //Constants.
        const float pentagonAngle = 1.2566370614359172953850573533118f;
        Color transparentColour = new Color(0, 0, 0, 0);

        //Local variables.
        int shatterGameObjectIndex = 0;
        int[,] horizontalCutTop, verticalCutLeft;

        //Get the sprite renderer and texture sizes.
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null) {
            error = true;
            return;
        }
        int textureWidth = (int) spriteRenderer.sprite.rect.width;
        int textureHeight = (int) spriteRenderer.sprite.rect.height;

        //Get the pixels of the sprite's texture.
        int spriteWidth = (int) spriteRenderer.sprite.rect.width;
        int spriteHeight = (int) spriteRenderer.sprite.rect.height;
        try {
            sourcePixels = spriteRenderer.sprite.texture.GetPixels((int) spriteRenderer.sprite.rect.xMin, (int) spriteRenderer.sprite.rect.yMin, spriteWidth, spriteHeight);
        }
        catch {
            Debug.LogError("Sprite Shatter (" + name + "): There was an error reading the texture data. Please ensure a valid sprite is selected on the " +
                    "sprite renderer and the \"Read/Write\" texture flag is set.");
            error = true;
            return;
        }

        //Randomize the random seed if required at runtime.
        if (shatterDetails.randomizeAtRunTime)
            shatterDetails.randomSeed = new System.Random().Next();

        //Make the cuts.
        makeCuts(spriteRenderer.sprite, shatterDetails.horizontalCuts, shatterDetails.verticalCuts, shatterDetails.randomSeed, shatterDetails.randomness,
                shatterDetails.zigzagFrequency, shatterDetails.zigzagAmplitude, out horizontalCutTop, out verticalCutLeft);

        //Loop over the cuts.
        for (int i = 0; i <= shatterDetails.horizontalCuts; i++)
            for (int j = 0; j <= shatterDetails.verticalCuts; j++) {

                //Calculate the size of this "shatter" sprite by seeing which pixels are contained within the current shape defined by the cuts.
                int minHorizontalCutTop = int.MaxValue;
                int maxHorizontalCutTop = -1;
                int minVerticalCutLeft = int.MaxValue;
                int maxVerticalCutLeft = -1;
                for (int k = 0; k < textureWidth; k++)
                    for (int l = 0; l < textureHeight; l++)
                        if ((j == 0 || k >= verticalCutLeft[j - 1, l]) && (j == shatterDetails.verticalCuts || k < verticalCutLeft[j, l]) &&
                                (i == 0 || l >= horizontalCutTop[i - 1, k]) && (i == shatterDetails.horizontalCuts || l < horizontalCutTop[i, k])) {
                            if (k < minVerticalCutLeft)
                                minVerticalCutLeft = k;
                            if (k + 1 > maxVerticalCutLeft)
                                maxVerticalCutLeft = k + 1;
                            if (l < minHorizontalCutTop)
                                minHorizontalCutTop = l;
                            if (l + 1 > maxHorizontalCutTop)
                                maxHorizontalCutTop = l + 1;
                        }

                //If this "shatter" shape is at least one pixel in each dimension, create the sprite.
                if (maxVerticalCutLeft - minVerticalCutLeft > 0 && maxHorizontalCutTop - minHorizontalCutTop > 0) {

                    //Create the new texture.
                    Texture2D shatterTexture = new Texture2D(maxVerticalCutLeft - minVerticalCutLeft, maxHorizontalCutTop - minHorizontalCutTop,
                            TextureFormat.RGBA32, false);
                    shatterTexture.anisoLevel = spriteRenderer.sprite.texture.anisoLevel;
                    shatterTexture.filterMode = spriteRenderer.sprite.texture.filterMode;
                    shatterTexture.wrapMode = spriteRenderer.sprite.texture.wrapMode;

                    //Set the texture pixels from the original sprite's texture.
                    int textureMinX = int.MaxValue, textureMaxX = int.MinValue, textureMinY = int.MaxValue, textureMaxY = int.MinValue;
                    bool allTransparent = true;
                    Color[] shatterTexturePixels = new Color[shatterTexture.width * shatterTexture.height];
                    for (int k = minVerticalCutLeft; k < maxVerticalCutLeft; k++)
                        for (int l = minHorizontalCutTop; l < maxHorizontalCutTop; l++)
                            if ((j == 0 || k >= verticalCutLeft[j - 1, l]) && (j == shatterDetails.verticalCuts || k < verticalCutLeft[j, l]) &&
                                    (i == 0 || l >= horizontalCutTop[i - 1, k]) && (i == shatterDetails.horizontalCuts || l < horizontalCutTop[i, k])) {
                                Color sourcePixel = sourcePixels[(l * textureWidth) + k];
                                if (sourcePixel.a > 0.001f) {
                                    if (k - minVerticalCutLeft < textureMinX)
                                        textureMinX = k - minVerticalCutLeft;
                                    if (k - minVerticalCutLeft > textureMaxX)
                                        textureMaxX = k - minVerticalCutLeft;
                                    if (l - minHorizontalCutTop < textureMinY)
                                        textureMinY = l - minHorizontalCutTop;
                                    if (l - minHorizontalCutTop > textureMaxY)
                                        textureMaxY = l - minHorizontalCutTop;
                                    allTransparent = false;
                                }
                                shatterTexturePixels[((l - minHorizontalCutTop) * shatterTexture.width) + k - minVerticalCutLeft] = sourcePixel;
                            }
                            else
                                shatterTexturePixels[((l - minHorizontalCutTop) * shatterTexture.width) + k - minVerticalCutLeft] = transparentColour;
                    shatterTexture.SetPixels(shatterTexturePixels);

                    //Create the game object as long as there was at least one pixel that wasn't transparent.
                    if (!allTransparent) {

                        //Create the game object and position it relative to the original game object.
                        GameObject spriteShatterGameObject = new GameObject("Sprite Shatter " + (shatterGameObjectIndex + 1));
                        spriteShatterGameObject.layer = gameObject.layer;
                        Vector3 oldGameObjectPosition = transform.position;
                        Quaternion oldGameObjectRotation = transform.rotation;
                        Vector3 oldGameObjectScale = transform.localScale;
                        transform.position = Vector3.zero;
                        transform.rotation = Quaternion.identity;
                        transform.localScale = Vector3.one;
                        spriteShatterGameObject.transform.localPosition = new Vector3(spriteRenderer.bounds.center.x -
                                spriteRenderer.bounds.extents.x + ((float) (minVerticalCutLeft * spriteWidth) / textureWidth /
                                spriteRenderer.sprite.pixelsPerUnit), spriteRenderer.bounds.center.y - spriteRenderer.bounds.extents.y +
                                ((float) (minHorizontalCutTop * spriteHeight) / textureHeight / spriteRenderer.sprite.pixelsPerUnit),
                                transform.position.z);
                        transform.position = oldGameObjectPosition;
                        transform.rotation = oldGameObjectRotation;
                        transform.localScale = oldGameObjectScale;

                        //Create a sprite renderer and attach a sprite to it that points to the newly-created texture.
                        SpriteRenderer shatterSpriteRenderer = spriteShatterGameObject.AddComponent<SpriteRenderer>();
                        shatterSpriteRenderer.color = spriteRenderer.color;
                        shatterSpriteRenderer.sharedMaterial = spriteRenderer.sharedMaterial;
                        shatterSpriteRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
                        shatterSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder;
                        shatterSpriteRenderer.sprite = Sprite.Create(shatterTexture, new Rect(0, 0, shatterTexture.width, shatterTexture.height),
                                Vector2.zero, ((float) textureWidth / spriteWidth) * spriteRenderer.sprite.pixelsPerUnit);
                        shatterSpriteRenderer.sprite.texture.Apply();

                        //Add a rigid body to the shatter game object to ensure it reacts to gravity.
                        spriteShatterGameObject.AddComponent<Rigidbody2D>();

                        //Create the necessary collider.
                        bool colliderCouldNotBeCreated = false;
                        if (shatterDetails.colliderType == ColliderType.Polygon) {

                            //For polygon colliders, create one but detect whether there weren't enough pixels to generate a proper collider. In the
                            //case the default "pentagon" is created, so detect this and cancel the creation of this shatter game object.
                            PolygonCollider2D polygonCollider = spriteShatterGameObject.AddComponent<PolygonCollider2D>();
                            if (polygonCollider.points.Length == 5) {
                                colliderCouldNotBeCreated = true;
                                Vector2 centre = Vector2.zero;
                                for (int k = 0; k < 5; k++)
                                    centre += polygonCollider.points[k];
                                centre /= 5;
                                for (int k = 0; k < 4; k++) {
                                    float x1 = polygonCollider.points[k].x - centre.x;
                                    float y1 = polygonCollider.points[k].y - centre.y;
                                    float x2 = polygonCollider.points[k + 1].x - centre.x;
                                    float y2 = polygonCollider.points[k + 1].y - centre.y;
                                    if (Math.Abs(Math.Acos(((x1 * x2) + (y1 * y2)) / (Math.Sqrt(Math.Pow(x1, 2) + Math.Pow(y1, 2)) *
                                            Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2)))) - pentagonAngle) > 0.001f) {
                                        colliderCouldNotBeCreated = false;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (shatterDetails.colliderType == ColliderType.Box) {
                            BoxCollider2D boxCollider = spriteShatterGameObject.AddComponent<BoxCollider2D>();
                            boxCollider.offset = new Vector2(
                                    (textureMinX + textureMaxX + 1) / (spriteRenderer.sprite.pixelsPerUnit * 2),
                                    (textureMinY + textureMaxY + 1) / (spriteRenderer.sprite.pixelsPerUnit * 2));
                            boxCollider.size = new Vector2(
                                    (textureMaxX - textureMinX + 1) / spriteRenderer.sprite.pixelsPerUnit,
                                    (textureMaxY - textureMinY + 1) / spriteRenderer.sprite.pixelsPerUnit);
                            colliderCouldNotBeCreated = boxCollider.shapeCount == 0;
                        }
                        else if (shatterDetails.colliderType == ColliderType.Circle) {
                            CircleCollider2D circleCollider = spriteShatterGameObject.AddComponent<CircleCollider2D>();
                            circleCollider.offset = new Vector2(
                                    (textureMinX + textureMaxX + 1) / (spriteRenderer.sprite.pixelsPerUnit * 2),
                                    (textureMinY + textureMaxY + 1) / (spriteRenderer.sprite.pixelsPerUnit * 2));
                            circleCollider.radius = ((textureMaxX - textureMinX + 1) + (textureMaxY - textureMinY + 1)) /
                                    (spriteRenderer.sprite.pixelsPerUnit * 4);
                            colliderCouldNotBeCreated = circleCollider.shapeCount == 0;
                        }

                        //If the collider failed to be created, destroy the game object.
                        if (colliderCouldNotBeCreated)
                            Destroy(spriteShatterGameObject);
                        else {

                            //If this is the first shatter object being created, create the parent object that goes directly underneath the main game
                            //object.
                            if (shatterGameObjectIndex == 0) {

                                //Create the parent object, getting the next free name.
                                int nextFreeSpriteShatterParentNumber = 0;
                                string spriteShatterParentName = name + " - Sprite Shatter";
                                Transform[] childTransforms = GetComponentsInChildren<Transform>(true);
                                while (transformArrayContainsGameObject(childTransforms, spriteShatterParentName)) {
                                    nextFreeSpriteShatterParentNumber++;
                                    spriteShatterParentName = name + " - Sprite Shatter (" + (nextFreeSpriteShatterParentNumber + 1) + ")";
                                }
                                GameObject shatterGameObject = new GameObject(spriteShatterParentName);
                                shatterGameObject.layer = gameObject.layer;
                                shatterGameObjectTransform = shatterGameObject.transform;
                                shatterGameObjectTransform.SetParent(transform);
                                shatterGameObjectTransform.localPosition = Vector3.zero;
                                shatterGameObjectTransform.localRotation = Quaternion.identity;
                                shatterGameObjectTransform.localScale = Vector3.one;
                                shatterGameObject.SetActive(false);
                            }

                            //Move onto the next game object index.
                            shatterGameObjectIndex++;

                            //Parent the shatter object to the main parent, and sort out positions and orientations.
                            Vector3 originalPosition = spriteShatterGameObject.transform.localPosition;
                            spriteShatterGameObject.transform.SetParent(shatterGameObjectTransform);
                            spriteShatterGameObject.transform.localPosition = originalPosition;
                            spriteShatterGameObject.transform.localRotation = Quaternion.identity;
                            spriteShatterGameObject.transform.localScale = Vector3.one;
                        }
                    }
                }
            }

        //If no shattered pieces were created (probably because the image was entirely transparent or the pieces were too small), display an error.
        if (shatterGameObjectTransform == null) {
            Debug.LogError("Sprite Shatter (" + name + "): No shattered pieces were created. This is probably because the pieces are too small for the " +
                    "sprite, or the sprite has too much transparency.");
            error = true;
        }

        //Get the original position and rotation of the shatter objects so they can be reset.
        else {
            originalShatterPieceLocations = new Vector3[shatterGameObjectTransform.childCount];
            originalShatterPieceRotations = new Quaternion[shatterGameObjectTransform.childCount];
            for (int i = 0; i < shatterGameObjectTransform.childCount; i++) {
                originalShatterPieceLocations[i] = shatterGameObjectTransform.GetChild(i).transform.localPosition;
                originalShatterPieceRotations[i] = shatterGameObjectTransform.GetChild(i).transform.localRotation;
            }
        }
    }

    //Populate the "cut" arrays to determine the positions of the cuts in the texture. This can be called either for the preview or for when performing the
    //actual cutting.
    public void makeCuts(Sprite sprite, int horizontalCuts, int verticalCuts, int randomSeed, float randomness, float zigzagFrequency,
            float zigzagAmplitude, out int[,] horizontalCutTop, out int[,] verticalCutLeft) {

        //Get the texture sizes.
        int textureWidth = (int) sprite.rect.width;
        int textureHeight = (int) sprite.rect.height;

        //Set the random offsets to the X/Y positions of each end of the cuts across/down the texture.
        int[,] randomTopOffsets = new int[horizontalCuts, 2];
        int[,] randomLeftOffsets = new int[verticalCuts, 2];
        System.Random random = new System.Random(randomSeed);
        for (int i = 0; i < horizontalCuts; i++)
            for (int j = 0; j < 2; j++)
                randomTopOffsets[i, j] = random.Next((int) ((float) -((textureHeight / horizontalCuts / 2) - 1) * randomness),
                        (int) ((float) ((textureHeight / horizontalCuts / 2) - 1) * randomness));
        for (int i = 0; i < verticalCuts; i++)
            for (int j = 0; j < 2; j++)
                randomLeftOffsets[i, j] = random.Next((int) ((float) -((textureWidth / verticalCuts / 2) - 1) * randomness),
                        (int) ((float) ((textureWidth / verticalCuts / 2) - 1) * randomness));

        //Initialise the cut arrays.
        horizontalCutTop = new int[horizontalCuts, textureWidth];
        verticalCutLeft = new int[verticalCuts, textureHeight];

        //Convert the zigzag float values into numbers of pixels.
        int zigzagFrequencyPixels = zigzagFrequency < 0.0001 ? 0 : ((int) (((Mathf.Max(textureWidth, textureHeight)) * (1 - (zigzagFrequency * 0.975f))) /
                4) * 4);
        int zigzagAmplitudePixels = (int) (Mathf.Min((float) textureHeight / (verticalCuts + 1), (float) textureWidth / (horizontalCuts + 1)) *
                zigzagAmplitude);

        //Loop over the horizontal and vertical cuts and populate the arrays.
        for (int k = 0; k < 2; k++)
            for (int i = 0; i < (k == 0 ? horizontalCuts : verticalCuts); i++) {
                float zigzag = 0;
                for (int j = 0; j < (k == 0 ? textureWidth : textureHeight); j++) {
                    if (zigzagFrequencyPixels > 0) {
                        if (j % zigzagFrequencyPixels * 2 < zigzagFrequencyPixels / 2 || j % zigzagFrequencyPixels * 2 >= (zigzagFrequencyPixels * 3) / 2)
                            zigzag = zigzag + (1 / (float) zigzagFrequencyPixels);
                        else
                            zigzag = zigzag - (1 / (float) zigzagFrequencyPixels);
                    }
                    if (k == 0)
                        horizontalCutTop[i, j] = Math.Min(Math.Max((((i + 1) * textureHeight) / (horizontalCuts + 1)) +
                                (int) transition(0, textureWidth, j, false, false, randomTopOffsets[i, 1] - randomTopOffsets[i, 0]) +
                                randomTopOffsets[i, 0] + (int) (zigzag * zigzagAmplitudePixels), 0), textureHeight - 1);
                    else
                        verticalCutLeft[i, j] = Math.Min(Math.Max((((i + 1) * textureWidth) / (verticalCuts + 1)) +
                                (int) transition(0, textureHeight, j, false, false, randomLeftOffsets[i, 1] - randomLeftOffsets[i, 0]) +
                                randomLeftOffsets[i, 0] + (int) (zigzag * zigzagAmplitudePixels), 0), textureWidth - 1);
                }
            }
    }

    //Transition between two values.
    static float transition(int minValue, int maxValue, int currentValue, bool useTrigonometryAtStart, bool useTrigonometryAtEnd, float scale) {
        if (minValue == maxValue)
            return 0;
        if (minValue > maxValue) {
            minValue = -minValue;
            maxValue = -maxValue;
            currentValue = -currentValue;
        }
        if (currentValue > maxValue)
            currentValue = maxValue;
        if (currentValue < minValue)
            currentValue = minValue;
        if (useTrigonometryAtStart && useTrigonometryAtEnd)
            return (float) ((Mathf.Sin((((float) (currentValue - minValue) / (maxValue - minValue)) * Mathf.PI) - (Mathf.PI / 2)) / 2) + 0.5) * scale;
        else if (!useTrigonometryAtStart && !useTrigonometryAtEnd)
            return ((currentValue - minValue) / (float) (maxValue - minValue)) * scale;
        else {
            float multiplier = Mathf.Clamp((((currentValue - minValue) / (float) (maxValue - minValue)) * 2) - 0.5f, 0, 1) * scale;
            float trigonometryAmount = (float) ((Mathf.Sin((((currentValue - minValue) / (float) (maxValue - minValue)) * Mathf.PI) -
                    (Mathf.PI / 2)) / 2) + 0.5);
            float nonTrigonometryAmount = (currentValue - minValue) / (float) (maxValue - minValue);
            if (useTrigonometryAtEnd)
                return (trigonometryAmount * multiplier) + (nonTrigonometryAmount * (scale - multiplier));
            else
                return (trigonometryAmount * (scale - multiplier)) + (nonTrigonometryAmount * multiplier);
        }
    }

    //Returns whether a transform array contains a transform associated with a game object with a given name
    static bool transformArrayContainsGameObject(Transform[] transformArray, string gameObjectName) {
        for (int i = 0; i < transformArray.Length; i++)
            if (transformArray[i].gameObject.name == gameObjectName)
                return true;
        return false;
    }

    /// <summary>
    /// Shatter a sprite for which a "Sprite Shatter" has already been initialised. This method hides the original sprite by disabling its sprite renderer
    /// (if you want to disable other components of the original game object, such as a collider, you must do this manually) and activates its "shatter"
    /// pieces, applying any specified forces to them. The sprite will then shatter as the pieces react to gravity. To reset the sprite back to its original
    /// state and to hide the shatter pieces, call "reset()".
    /// </summary>
    public void shatter()
    {
        if (error)
            return;
        SpriteRenderer parentSpriteRenderer = null;
        if (shatterGameObjectTransform != null)
            parentSpriteRenderer = shatterGameObjectTransform.parent.gameObject.GetComponent<SpriteRenderer>();
        if (shatterGameObjectTransform == null || parentSpriteRenderer == null)
            Debug.LogError("Sprite Shatter (" + name + "): The Sprite Shatter game object or its Sprite Renderer could not be found. Please initialise " +
                    "the \"Sprite Shatter\" component again.");
        else
        {

            //Disable the sprite renderer of the original object to hide it, and activate the shatter pieces game objects.
            parentSpriteRenderer.enabled = false;
            shatterGameObjectTransform.gameObject.SetActive(true);

            //Apply forces to the pieces.
            if (Math.Abs(shatterDetails.explosionForce.x) > 0.0001f || Math.Abs(shatterDetails.explosionForce.y) > 0.0001f) {
                Vector2 explosionSource = new Vector2(
                        (parentSpriteRenderer.bounds.extents.x * shatterDetails.explodeFrom.x * 2) + parentSpriteRenderer.bounds.center.x -
                            parentSpriteRenderer.bounds.extents.x,
                        (parentSpriteRenderer.bounds.extents.y * shatterDetails.explodeFrom.y * 2) + parentSpriteRenderer.bounds.center.y -
                            parentSpriteRenderer.bounds.extents.y);
                for (int i = 0; i < shatterGameObjectTransform.childCount; i++) {
                    Transform shatterObject = shatterGameObjectTransform.GetChild(i);
                    shatterObject.gameObject.layer = 18;
                    shatterObject.gameObject.AddComponent<ShatterPieces>();
                    shatterObject.gameObject.GetComponent<ShatterPieces>().timeToDisappear = shatterDetails.timeToDisappear;
                    Vector2 normalizedVectorFromExplosionSource = (new Vector2(shatterObject.position.x - explosionSource.x, shatterObject.position.y -
                            explosionSource.y)).normalized;
                    if (shatterObject.GetComponent<Rigidbody2D>() != null)
                        shatterObject.GetComponent<Rigidbody2D>().velocity = new Vector2(normalizedVectorFromExplosionSource.x *
                                shatterDetails.explosionForce.x, normalizedVectorFromExplosionSource.y * shatterDetails.explosionForce.y);
                }
            }
        }
    }

    /// <summary>
    /// Resets a previously-shattered sprite. Reactivates the main sprite and deactivates the "shatter" pieces, placing them back at their original
    /// positions with their original rotations, ready for another shatter.
    /// </summary>
    public void reset() {
        if (error)
            return;
        SpriteRenderer parentSpriteRenderer = null;
        if (shatterGameObjectTransform != null)
            parentSpriteRenderer = shatterGameObjectTransform.parent.gameObject.GetComponent<SpriteRenderer>();
        if (shatterGameObjectTransform == null || parentSpriteRenderer == null)
            Debug.LogError("Sprite Shatter (" + name + "): The Sprite Shatter game object or its Sprite Renderer could not be found. Please initialise " +
                    "the \"Sprite Shatter\" component again.");
        else {
            parentSpriteRenderer.enabled = true;
            shatterGameObjectTransform.gameObject.SetActive(false);

            //Restore the original positions of the "shatter" pieces.
            for (int i = 0; i < shatterGameObjectTransform.childCount; i++) {
                shatterGameObjectTransform.GetChild(i).transform.localPosition = originalShatterPieceLocations[i];
                shatterGameObjectTransform.GetChild(i).transform.localRotation = originalShatterPieceRotations[i];
            }
        }
    }
}