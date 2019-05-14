using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DYP;

public class PlayerCharacter : MonoBehaviour, IDamagable, IHasInventory, IHasEquipment
{
    private CharacterStatController m_StatController;
    private BasicMovementController2D m_MovementController;
    private AdventurerAnimationController m_AnimationController;

    private Inventory m_Inventory;
    public Inventory Inventory { get { return m_Inventory; } }

    private Equipment m_Equipment;
    public Equipment Equipment { get { return m_Equipment; } }

    private void Awake()
    {
        m_StatController = GetComponent<CharacterStatController>();
        m_MovementController = GetComponent<BasicMovementController2D>();
        m_AnimationController = GetComponent<AdventurerAnimationController>();

        m_Inventory = GetComponent<Inventory>();
        m_Equipment = GetComponent<Equipment>();
    }

    private void Start()
    {
        initCallback();
    }

    private void initCallback()
    {
        m_MovementController.OnAirJump += delegate { m_StatController.JumpCounter++; };
        m_MovementController.OnResetJumpCounter += delegate (MotorState state) { m_StatController.JumpCounter = 0; };
        m_MovementController.CanAirJumpFunc = delegate { return m_StatController.JumpCounter < (int)m_StatController.JumpCount.Value; };
    }

    public void UseItem(BaseItem item)
    {
        m_Inventory.RemoveItem(item, 1);
        // TODO: use item
    }

    public int AddItem(BaseItem item, int count)
    {
        return m_Inventory.AddItem(item, count);
    }
    
    public bool RemoveItem(BaseItem item, int count)
    {
        return m_Inventory.RemoveItem(item, count);
    }

    public bool HasItem(BaseItem item, int count)
    {
        return m_Inventory.HasItem(item, count);
    }

    public bool Equip(EquippableItem equipment)
    {
        // TODO: remove equipment, remove from inventory

        equipment.Equip(m_StatController);

        if (equipment is WeaponItem)
        {
            // TODO: swap skills and animation ...etc
        }

        throw new NotImplementedException();
    }

    public bool Unequip(EquippableItem equipment)
    {
        throw new NotImplementedException();
    }
}
