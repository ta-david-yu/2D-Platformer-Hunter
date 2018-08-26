using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDatabase : MonoBehaviour
{
    [SerializeField]
    private MaterialItemList m_MaterialItems;
    public MaterialItemList MaterialItems { get { return m_MaterialItems; } }

    [SerializeField]
    private UsableItemList m_UsableItems;
    public UsableItemList UsableItems { get { return m_UsableItems; } }

    [SerializeField]
    private EquippableItemList m_ArmourItems;
    public EquippableItemList ArmourItems { get { return m_ArmourItems; } }

    [SerializeField]
    private WeaponItemList m_WeaponItems;
    public WeaponItemList WeaponItems { get { return m_WeaponItems; } }

    [SerializeField]
    private ImportantItemList m_ImportantItems;
    public ImportantItemList ImportantItems { get { return m_ImportantItems; } }

    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

}
