using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : InputComponent, IDataPersister
{
    public static PlayerInput Instance
    {
        get { return instance; }
    }

    protected static PlayerInput instance;

    public bool HaveControl { get { return haveControl; } }

    public InputButton Pause = new InputButton(KeyCode.Escape, XboxControllerButtons.Menu);
    public InputButton Interact = new InputButton(KeyCode.E, XboxControllerButtons.Y);
    public InputButton MeleeAttack = new InputButton(KeyCode.K, XboxControllerButtons.X);
    public InputButton RangedAttack = new InputButton(KeyCode.O, XboxControllerButtons.B);
    public InputButton Jump = new InputButton(KeyCode.Space, XboxControllerButtons.A);
    public InputButton Boost = new InputButton(KeyCode.Mouse0, XboxControllerButtons.RightBumper);
    public InputAxis Horizontal = new InputAxis(KeyCode.D, KeyCode.A, XboxControllerAxes.LeftstickHorizontal);
    public InputAxis Vertical = new InputAxis(KeyCode.W, KeyCode.S, XboxControllerAxes.LeftstickVertical);
    

    // [HideInInspector]
    public DataSettings dataSettings;

    protected bool haveControl = true;

    protected bool debugMenuIsOpen = false;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            throw new UnityException("There cannot be more than one PlayerInput script.  The instances are " + instance.name + " and " + name + ".");
    }

    void OnEnable()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            throw new UnityException("There cannot be more than one PlayerInput script.  The instances are " + instance.name + " and " + name + ".");

        PersistentDataManager.RegisterPersister(this);
    }

    void OnDisable()
    {
        PersistentDataManager.UnregisterPersister(this);

        instance = null;
    }

    protected override void GetInputs(bool fixedUpdateHappened)
    {
        Pause.Get(fixedUpdateHappened, inputType);
        Interact.Get(fixedUpdateHappened, inputType);
        MeleeAttack.Get(fixedUpdateHappened, inputType);
        RangedAttack.Get(fixedUpdateHappened, inputType);
        Jump.Get(fixedUpdateHappened, inputType);
        Boost.Get(fixedUpdateHappened, inputType);
        Horizontal.Get(inputType);
        Vertical.Get(inputType);

        if (Input.GetKeyDown(KeyCode.F12))
        {
            debugMenuIsOpen = !debugMenuIsOpen;
        }
    }

    public override void GainControl()
    {
        haveControl = true;

        GainControl(Pause);
        GainControl(Interact);
        GainControl(MeleeAttack);
        GainControl(RangedAttack);
        GainControl(Jump);
        GainControl(Boost);
        GainControl(Horizontal);
        GainControl(Vertical);
    }

    public override void ReleaseControl(bool resetValues = true)
    {
        haveControl = false;

        ReleaseControl(Pause, resetValues);
        ReleaseControl(Interact, resetValues);
        ReleaseControl(MeleeAttack, resetValues);
        ReleaseControl(RangedAttack, resetValues);
        ReleaseControl(Jump, resetValues);
        ReleaseControl(Boost, resetValues);
        ReleaseControl(Horizontal, resetValues);
        ReleaseControl(Vertical, resetValues);
    }

    public void DisableMeleeAttacking()
    {
        MeleeAttack.Disable();
    }

    public void EnableMeleeAttacking()
    {
        MeleeAttack.Enable();
    }

    public void DisableRangedAttacking()
    {
        RangedAttack.Disable();
    }

    public void EnableRangedAttacking()
    {
        RangedAttack.Enable();
    }

    public DataSettings GetDataSettings()
    {
        return dataSettings;
    }

    public void SetDataSettings(string dataTag, DataSettings.PersistenceType persistenceType)
    {
        dataSettings.dataTag = dataTag;
        dataSettings.persistenceType = persistenceType;
    }

    public Data SaveData()
    {
        return new Data<bool, bool>(MeleeAttack.Enabled, RangedAttack.Enabled);
    }

    public void LoadData(Data data)
    {
        Data<bool, bool> playerInputData = (Data<bool, bool>)data;

        if (playerInputData.value0)
            MeleeAttack.Enable();
        else
            MeleeAttack.Disable();

        if (playerInputData.value1)
            RangedAttack.Enable();
        else
            RangedAttack.Disable();
    }

    void OnGUI()
    {
        if (debugMenuIsOpen)
        {
            const float height = 100;

            GUILayout.BeginArea(new Rect(30, Screen.height - height, 200, height));

            GUILayout.BeginVertical("box");
            GUILayout.Label("Press F12 to close");

            bool meleeAttackEnabled = GUILayout.Toggle(MeleeAttack.Enabled, "Enable Melee Attack");
            bool rangeAttackEnabled = GUILayout.Toggle(RangedAttack.Enabled, "Enable Range Attack");

            if (meleeAttackEnabled != MeleeAttack.Enabled)
            {
                if (meleeAttackEnabled)
                    MeleeAttack.Enable();
                else
                    MeleeAttack.Disable();
            }

            if (rangeAttackEnabled != RangedAttack.Enabled)
            {
                if (rangeAttackEnabled)
                    RangedAttack.Enable();
                else
                    RangedAttack.Disable();
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
