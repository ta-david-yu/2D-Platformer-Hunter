using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DYP;

public class DashBoots : MonoBehaviour
{
    [SerializeField]
    private BaseDashModule m_Module;

    [SerializeField]
    private BasicMovementController2D m_EquipTarget;

    public void OnEnable()
    {
        Equip();
    }

    public void OnDisable()
    {
        Unequip();
    }

    public void Equip()
    {
        m_EquipTarget.ChangeDashModule(m_Module);
    }

    public void Unequip()
    {
        m_EquipTarget.ChangeDashModule(null);
    }
}
