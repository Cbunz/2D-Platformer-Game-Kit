using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerCharacter))]
public class PlayerCharacterEditor : Editor
{
    SerializedProperty spriteRendererProperty;
    SerializedProperty damageableProperty;
    SerializedProperty meleeDamagerProperty;
    SerializedProperty facingLeftBulletSpawnPointProperty;
    SerializedProperty facingRightBulletSpawnPointProperty;
    SerializedProperty bulletPoolProperty;
    SerializedProperty cameraFollowTargetProperty;
    SerializedProperty boyProperty;

    SerializedProperty maxSpeedProperty;
    SerializedProperty groundAccelerationProperty;
    SerializedProperty groundDecelerationProperty;
    SerializedProperty pushingSpeedProportionProperty;

    SerializedProperty airborneAccelProportionProperty;
    SerializedProperty airborneDecelProportionProperty;
    SerializedProperty gravityProperty;
    SerializedProperty jumpSpeedProperty;
    SerializedProperty jumpAbortSpeedReductionProperty;

    SerializedProperty wallSlideOnProperty;
    SerializedProperty wallSlideUpGravityProperty;
    SerializedProperty wallSlideDownGravityProperty;
    SerializedProperty wallSlideJumpXProperty;
    SerializedProperty wallSlideJumpYProperty;
    SerializedProperty wallSlideAirborneDecelProportionProperty;
    SerializedProperty wallSlideTimeoutDurationProperty;
    SerializedProperty canWallSlideUpProperty;
    SerializedProperty wallSlideUpSpeedProperty;

    SerializedProperty hurtJumpAngleProperty;
    SerializedProperty hurtJumpSpeedProperty;
    SerializedProperty flickeringDurationProperty;

    SerializedProperty meleeAttackOnProperty;
    SerializedProperty meleeAttackDashSpeedProperty;
    SerializedProperty dashWhileAirborneProperty;
    SerializedProperty meleeDamagerDelayProperty;

    SerializedProperty rangedAttackOnProperty;
    SerializedProperty shotsPerSecondProperty;
    SerializedProperty bulletSpeedProperty;
    SerializedProperty holdingGunTimeoutDurationProperty;
    SerializedProperty rightBulletSpawnPointAnimatedProperty;

    SerializedProperty footstepAudioPlayerProperty;
    SerializedProperty landingAudioPlayerProperty;
    SerializedProperty hurtAudioPlayerProperty;
    SerializedProperty meleeAttackAudioPlayerProperty;
    SerializedProperty rangedAttackAudioPlayerProperty;

    SerializedProperty cameraHorizontalFacingOffsetProperty;
    SerializedProperty cameraHorizontalSpeedOffsetProperty;
    SerializedProperty cameraVerticalInputOffsetProperty;
    SerializedProperty maxHorizontalDeltaDampTimeProperty;
    SerializedProperty maxVerticalDeltaDampTimeProperty;
    SerializedProperty verticalCameraOffsetDelayProperty;

    SerializedProperty triggerTagProperty;
    SerializedProperty spriteOriginallyFacesLeftProperty;

    bool referencesFoldout;
    bool movementSettingsFoldout;
    bool airborneSettingsFoldout;
    bool wallSlideSettingsFoldout;
    bool hurtSettingsFoldout;
    bool meleeSettingsFoldout;
    bool rangedSettingsFoldout;
    bool audioSettingsFoldout;
    bool cameraFollowSettingsFoldout;
    bool miscSettingsFoldout;

    readonly GUIContent spriteRendererContent = new GUIContent("Sprite Renderer");
    readonly GUIContent damageableContent = new GUIContent("Damageable");
    readonly GUIContent meleeDamagerContent = new GUIContent("Melee Damager");
    readonly GUIContent facingLeftBulletSpawnPointContent = new GUIContent("Facing Left Bullet Spawn Point");
    readonly GUIContent facingRightBulletSpawnPointContent = new GUIContent("Facing Right Bullet Spawn Point");
    readonly GUIContent bulletPoolContent = new GUIContent("Bullet Pool");
    readonly GUIContent cameraFollowTargetContent = new GUIContent("Camera Follow Target");
    readonly GUIContent boyContent = new GUIContent("Boy");

    readonly GUIContent maxSpeedContent = new GUIContent("Max Speed");
    readonly GUIContent groundAccelerationContent = new GUIContent("Ground Acceleration");
    readonly GUIContent groundDecelerationContent = new GUIContent("Ground Deceleration");
    readonly GUIContent pushingSpeedProportionContent = new GUIContent("Pushing Speed Proportion");

    readonly GUIContent airborneAccelProportionContent = new GUIContent("Airborne Accel Proportion");
    readonly GUIContent airborneDecelProportionContent = new GUIContent("Airborne Decel Proportion");
    readonly GUIContent gravityContent = new GUIContent("Gravity");
    readonly GUIContent jumpSpeedContent = new GUIContent("Jump Speed");
    readonly GUIContent jumpAbortSpeedReductionContent = new GUIContent("Jump Abort Speed Reduction");

    readonly GUIContent wallSlideOnContent = new GUIContent("Wall Slide On");
    readonly GUIContent wallSlideUpGravityContent = new GUIContent("Slide Up Gravity");
    readonly GUIContent wallSlideDownGravityContent = new GUIContent("Slide Down Gravity");
    readonly GUIContent wallSlideJumpXContent = new GUIContent("Slide Jump X Vector");
    readonly GUIContent wallSlideJumpYContent = new GUIContent("Slide Jump Y Vector");
    readonly GUIContent wallSlideAirborneDecelProportionContent = new GUIContent("Jump Decel Proportion");
    readonly GUIContent wallSlideTimeoutDurationContent = new GUIContent("Slide Timeout Duration");
    readonly GUIContent canWallSlideUpContent = new GUIContent("Can Wall Slide Up");
    readonly GUIContent wallSlideUpSpeedContent = new GUIContent("Slide Up Speed");

    readonly GUIContent hurtJumpAngleContent = new GUIContent("Hurt Jump Angle");
    readonly GUIContent hurtJumpSpeedContent = new GUIContent("Hurt Jump Speed");
    readonly GUIContent flickeringDurationContent = new GUIContent("Flicking Duration", "When the player is hurt, she becomes invulnerable for a short time and the SpriteRenderer flickers on and off to indicate this.  This field is the duration in seconds the SpriteRenderer stays either on or off whilst flickering.  To adjust the duration of invulnerability see the Damageable component.");

    readonly GUIContent meleeAttackOnContent = new GUIContent("Melee Attack On");
    readonly GUIContent meleeAttackDashSpeedContent = new GUIContent("Dash Speed");
    readonly GUIContent dashWhileAirborneContent = new GUIContent("Dash While Airborne");
    readonly GUIContent meleeDamagerDelayContent = new GUIContent("Melee Damager Delay");

    readonly GUIContent rangedAttackOnContent = new GUIContent("Ranged Attack On");
    readonly GUIContent shotsPerSecondContent = new GUIContent("Shots Per Second");
    readonly GUIContent bulletSpeedContent = new GUIContent("Bullet Speed");
    readonly GUIContent holdingGunTimeoutDurationContent = new GUIContent("Holding Gun Timeout Duration");
    readonly GUIContent rightBulletSpawnPointAnimatedContent = new GUIContent("Right Bullet Spawn Point Animated");

    readonly GUIContent footstepPlayerContent = new GUIContent("Footstep Audio Player");
    readonly GUIContent landingAudioPlayerContent = new GUIContent("Landing Audio Player");
    readonly GUIContent hurtAudioPlayerContent = new GUIContent("Hurt Audio Player");
    readonly GUIContent meleeAttackAudioPlayerContent = new GUIContent("Melee Attack Audio Player");
    readonly GUIContent rangedAttackAudioPlayerContent = new GUIContent("Ranged Attack Audio Player");

    readonly GUIContent cameraHorizontalFacingOffsetContent = new GUIContent("Camera Horizontal Facing Offset");
    readonly GUIContent cameraHorizontalSpeedOffsetContent = new GUIContent("Camera Horizontal Speed Offset");
    readonly GUIContent cameraVerticalInputOffsetContent = new GUIContent("Camera Vertical Input Offset");
    readonly GUIContent maxHorizontalDeltaDampTimeContent = new GUIContent("Max Horizontal Delta Damp Time");
    readonly GUIContent maxVerticalDeltaDampTimeContent = new GUIContent("Max Vertical Delta Damp Time");
    readonly GUIContent verticalCameraOffsetDelayContent = new GUIContent("Vertical Camera Offset Delay");

    readonly GUIContent triggerTagContent = new GUIContent("Trigger Tag");
    readonly GUIContent spriteOriginallyFacesLeftContent = new GUIContent("Sprite Originally Faces Left");

    readonly GUIContent referencesContent = new GUIContent("References");
    readonly GUIContent movementSettingsContent = new GUIContent("Movement Settings");
    readonly GUIContent airborneSettingsContent = new GUIContent("Airborne Settings");
    readonly GUIContent wallSlideSettingsContent = new GUIContent("Wall Slide Settings");
    readonly GUIContent hurtSettingsContent = new GUIContent("Hurt Settings");
    readonly GUIContent meleeSettingsContent = new GUIContent("Melee Settings");
    readonly GUIContent rangedSettingsContent = new GUIContent("Ranged Settings");
    readonly GUIContent audioSettingsContent = new GUIContent("Audio Settings");
    readonly GUIContent cameraFollowSettingsContent = new GUIContent("Camera Follow Settings");
    readonly GUIContent miscSettingsContent = new GUIContent("Misc Settings");

    void OnEnable()
    {
        spriteRendererProperty = serializedObject.FindProperty("spriteRenderer");
        damageableProperty = serializedObject.FindProperty("damageable");
        meleeDamagerProperty = serializedObject.FindProperty("meleeDamager");
        facingLeftBulletSpawnPointProperty = serializedObject.FindProperty("facingLeftBulletSpawnPoint");
        facingRightBulletSpawnPointProperty = serializedObject.FindProperty("facingRightBulletSpawnPoint");
        bulletPoolProperty = serializedObject.FindProperty("bulletPool");
        cameraFollowTargetProperty = serializedObject.FindProperty("cameraFollowTarget");
        boyProperty = serializedObject.FindProperty("boy");

        maxSpeedProperty = serializedObject.FindProperty("maxSpeed");
        groundAccelerationProperty = serializedObject.FindProperty("groundAcceleration");
        groundDecelerationProperty = serializedObject.FindProperty("groundDeceleration");
        pushingSpeedProportionProperty = serializedObject.FindProperty("pushingSpeedProportion");

        airborneAccelProportionProperty = serializedObject.FindProperty("airborneAccelProportion");
        airborneDecelProportionProperty = serializedObject.FindProperty("airborneDecelProportion");
        gravityProperty = serializedObject.FindProperty("gravity");
        jumpSpeedProperty = serializedObject.FindProperty("jumpSpeed");
        jumpAbortSpeedReductionProperty = serializedObject.FindProperty("jumpAbortSpeedReduction");

        wallSlideOnProperty = serializedObject.FindProperty("wallSlideOn");
        wallSlideUpGravityProperty = serializedObject.FindProperty("wallSlideUpGravity");
        wallSlideDownGravityProperty = serializedObject.FindProperty("wallSlideDownGravity");
        wallSlideJumpXProperty = serializedObject.FindProperty("wallSlideJumpX");
        wallSlideJumpYProperty = serializedObject.FindProperty("wallSlideJumpY");
        wallSlideAirborneDecelProportionProperty = serializedObject.FindProperty("wallSlideAirborneDecelProportion");
        wallSlideTimeoutDurationProperty = serializedObject.FindProperty("wallSlideTimeoutDuration");
        canWallSlideUpProperty = serializedObject.FindProperty("canWallSlideUp");
        wallSlideUpSpeedProperty = serializedObject.FindProperty("wallSlideUpSpeed");

        hurtJumpAngleProperty = serializedObject.FindProperty("hurtJumpAngle");
        hurtJumpSpeedProperty = serializedObject.FindProperty("hurtJumpSpeed");
        flickeringDurationProperty = serializedObject.FindProperty("flickeringDuration");

        meleeAttackOnProperty = serializedObject.FindProperty("meleeAttackOn");
        meleeAttackDashSpeedProperty = serializedObject.FindProperty("meleeAttackDashSpeed");
        dashWhileAirborneProperty = serializedObject.FindProperty("dashWhileAirborne");
        meleeDamagerDelayProperty = serializedObject.FindProperty("meleeDamagerDelay");

        rangedAttackOnProperty = serializedObject.FindProperty("rangedAttackOn");
        shotsPerSecondProperty = serializedObject.FindProperty("shotsPerSecond");
        bulletSpeedProperty = serializedObject.FindProperty("bulletSpeed");
        holdingGunTimeoutDurationProperty = serializedObject.FindProperty("holdingGunTimeoutDuration");
        rightBulletSpawnPointAnimatedProperty = serializedObject.FindProperty("rightBulletSpawnPointAnimated");

        footstepAudioPlayerProperty = serializedObject.FindProperty("footstepAudioPlayer");
        landingAudioPlayerProperty = serializedObject.FindProperty("landingAudioPlayer");
        hurtAudioPlayerProperty = serializedObject.FindProperty("hurtAudioPlayer");
        meleeAttackAudioPlayerProperty = serializedObject.FindProperty("meleeAttackAudioPlayer");
        rangedAttackAudioPlayerProperty = serializedObject.FindProperty("rangedAttackAudioPlayer");

        cameraHorizontalFacingOffsetProperty = serializedObject.FindProperty("cameraHorizontalFacingOffset");
        cameraHorizontalSpeedOffsetProperty = serializedObject.FindProperty("cameraHorizontalSpeedOffset");
        cameraVerticalInputOffsetProperty = serializedObject.FindProperty("cameraVerticalInputOffset");
        maxHorizontalDeltaDampTimeProperty = serializedObject.FindProperty("maxHorizontalDeltaDampTime");
        maxVerticalDeltaDampTimeProperty = serializedObject.FindProperty("maxVerticalDeltaDampTime");
        verticalCameraOffsetDelayProperty = serializedObject.FindProperty("verticalCameraOffsetDelay");

        triggerTagProperty = serializedObject.FindProperty("triggerTag");
        spriteOriginallyFacesLeftProperty = serializedObject.FindProperty("spriteOriginallyFacesLeft");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.indentLevel++;

        referencesFoldout = EditorGUILayout.Foldout(referencesFoldout, referencesContent);

        if (referencesFoldout)
        {
            EditorGUILayout.PropertyField(spriteRendererProperty, spriteRendererContent);
            EditorGUILayout.PropertyField(damageableProperty, damageableContent);
            EditorGUILayout.PropertyField(meleeDamagerProperty, meleeDamagerContent);
            EditorGUILayout.PropertyField(facingLeftBulletSpawnPointProperty, facingLeftBulletSpawnPointContent);
            EditorGUILayout.PropertyField(facingRightBulletSpawnPointProperty, facingRightBulletSpawnPointContent);
            EditorGUILayout.PropertyField(bulletPoolProperty, bulletPoolContent);
            EditorGUILayout.PropertyField(cameraFollowTargetProperty, cameraFollowTargetContent);
            EditorGUILayout.PropertyField(boyProperty, boyContent);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.indentLevel++;

        movementSettingsFoldout = EditorGUILayout.Foldout(movementSettingsFoldout, movementSettingsContent);

        if (movementSettingsFoldout)
        {
            EditorGUILayout.PropertyField(maxSpeedProperty, maxSpeedContent);
            EditorGUILayout.PropertyField(groundAccelerationProperty, groundAccelerationContent);
            EditorGUILayout.PropertyField(groundDecelerationProperty, groundDecelerationContent);
            EditorGUILayout.PropertyField(pushingSpeedProportionProperty, pushingSpeedProportionContent);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.indentLevel++;

        airborneSettingsFoldout = EditorGUILayout.Foldout(airborneSettingsFoldout, airborneSettingsContent);

        if (airborneSettingsFoldout)
        {
            EditorGUILayout.PropertyField(airborneAccelProportionProperty, airborneAccelProportionContent);
            EditorGUILayout.PropertyField(airborneDecelProportionProperty, airborneDecelProportionContent);
            EditorGUILayout.PropertyField(gravityProperty, gravityContent);
            EditorGUILayout.PropertyField(jumpSpeedProperty, jumpSpeedContent);
            EditorGUILayout.PropertyField(jumpAbortSpeedReductionProperty, jumpAbortSpeedReductionContent);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.indentLevel++;

        wallSlideSettingsFoldout = EditorGUILayout.Foldout(wallSlideSettingsFoldout, wallSlideSettingsContent);

        if (wallSlideSettingsFoldout)
        {
            EditorGUILayout.PropertyField(wallSlideOnProperty, wallSlideOnContent);
            EditorGUILayout.PropertyField(wallSlideUpGravityProperty, wallSlideUpGravityContent);
            EditorGUILayout.PropertyField(wallSlideDownGravityProperty, wallSlideDownGravityContent);
            EditorGUILayout.PropertyField(wallSlideJumpXProperty, wallSlideJumpXContent);
            EditorGUILayout.PropertyField(wallSlideJumpYProperty, wallSlideJumpYContent);
            EditorGUILayout.PropertyField(wallSlideAirborneDecelProportionProperty, wallSlideAirborneDecelProportionContent);
            EditorGUILayout.PropertyField(wallSlideTimeoutDurationProperty, wallSlideTimeoutDurationContent);
            EditorGUILayout.PropertyField(canWallSlideUpProperty, canWallSlideUpContent);
            if (canWallSlideUpProperty.boolValue == true)
            {
                EditorGUILayout.PropertyField(wallSlideUpSpeedProperty, wallSlideUpSpeedContent);
            }
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.indentLevel++;

        hurtSettingsFoldout = EditorGUILayout.Foldout(hurtSettingsFoldout, hurtSettingsContent);

        if (hurtSettingsFoldout)
        {
            EditorGUILayout.PropertyField(hurtJumpAngleProperty, hurtJumpAngleContent);
            EditorGUILayout.PropertyField(hurtJumpSpeedProperty, hurtJumpSpeedContent);
            EditorGUILayout.PropertyField(flickeringDurationProperty, flickeringDurationContent);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.indentLevel++;

        meleeSettingsFoldout = EditorGUILayout.Foldout(meleeSettingsFoldout, meleeSettingsContent);

        if (meleeSettingsFoldout)
        {
            EditorGUILayout.PropertyField(meleeAttackOnProperty, meleeAttackOnContent);
            EditorGUILayout.PropertyField(meleeAttackDashSpeedProperty, meleeAttackDashSpeedContent);
            EditorGUILayout.PropertyField(dashWhileAirborneProperty, dashWhileAirborneContent);
            EditorGUILayout.PropertyField(meleeDamagerDelayProperty, meleeDamagerDelayContent);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.indentLevel++;

        rangedSettingsFoldout = EditorGUILayout.Foldout(rangedSettingsFoldout, rangedSettingsContent);

        if (rangedSettingsFoldout)
        {
            EditorGUILayout.PropertyField(rangedAttackOnProperty, rangedAttackOnContent);
            EditorGUILayout.PropertyField(shotsPerSecondProperty, shotsPerSecondContent);
            EditorGUILayout.PropertyField(bulletSpeedProperty, bulletSpeedContent);
            EditorGUILayout.PropertyField(holdingGunTimeoutDurationProperty, holdingGunTimeoutDurationContent);
            EditorGUILayout.PropertyField(rightBulletSpawnPointAnimatedProperty, rightBulletSpawnPointAnimatedContent);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.indentLevel++;

        audioSettingsFoldout = EditorGUILayout.Foldout(audioSettingsFoldout, audioSettingsContent);

        if (audioSettingsFoldout)
        {
            EditorGUILayout.PropertyField(footstepAudioPlayerProperty, footstepPlayerContent);
            EditorGUILayout.PropertyField(landingAudioPlayerProperty, landingAudioPlayerContent);
            EditorGUILayout.PropertyField(hurtAudioPlayerProperty, hurtAudioPlayerContent);
            EditorGUILayout.PropertyField(meleeAttackAudioPlayerProperty, meleeAttackAudioPlayerContent);
            EditorGUILayout.PropertyField(rangedAttackAudioPlayerProperty, rangedAttackAudioPlayerContent);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.indentLevel++;

        cameraFollowSettingsFoldout = EditorGUILayout.Foldout(cameraFollowSettingsFoldout, cameraFollowSettingsContent);

        if (cameraFollowSettingsFoldout)
        {
            EditorGUILayout.PropertyField(cameraHorizontalFacingOffsetProperty, cameraHorizontalFacingOffsetContent);
            EditorGUILayout.PropertyField(cameraHorizontalSpeedOffsetProperty, cameraHorizontalSpeedOffsetContent);
            EditorGUILayout.PropertyField(cameraVerticalInputOffsetProperty, cameraVerticalInputOffsetContent);
            EditorGUILayout.PropertyField(maxHorizontalDeltaDampTimeProperty, maxHorizontalDeltaDampTimeContent);
            EditorGUILayout.PropertyField(maxVerticalDeltaDampTimeProperty, maxVerticalDeltaDampTimeContent);
            EditorGUILayout.PropertyField(verticalCameraOffsetDelayProperty, verticalCameraOffsetDelayContent);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.indentLevel++;

        miscSettingsFoldout = EditorGUILayout.Foldout(miscSettingsFoldout, miscSettingsContent);

        if (miscSettingsFoldout)
        {
            EditorGUILayout.PropertyField(triggerTagProperty, triggerTagContent);
            EditorGUILayout.PropertyField(spriteOriginallyFacesLeftProperty, spriteOriginallyFacesLeftContent);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
