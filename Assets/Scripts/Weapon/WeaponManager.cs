using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public Weapon[] weaponSlots = new Weapon[3];

    public void EquipStartingWeapon(Weapon weapon)
    {
        if (weapon == null)
            return;

        weaponSlots[0] = weapon;
    }

    public bool AddWeapon(Weapon weaponPrefab)
    {
        if (weaponPrefab == null)
            return false;

        for (int i = 1; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == null)
            {
                Weapon newWeapon = Instantiate(weaponPrefab, transform);
                weaponSlots[i] = newWeapon;
                return true;
            }
        }

        return false;
    }

    public bool hasEmptySlot()
    {
        for (int i = 1; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == null)
            {
                return true;
            }
        }

        return false;
    }
}