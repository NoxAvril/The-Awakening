using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public Weapon[] weaponSlots =
        new Weapon[3];


    // ============================================================
    // STARTING WEAPON
    // ============================================================

    public bool EquipStartingWeapon(
        Weapon weapon)
    {
        if (weapon == null)
            return false;

        if (weaponSlots.Length == 0)
            return false;

        // Slot 0 is reserved for the starting weapon.
        if (weaponSlots[0] != null)
        {
            Debug.LogWarning(
                "[WeaponManager] Starting weapon slot is already occupied."
            );

            return false;
        }

        weaponSlots[0] = weapon;

        Debug.Log(
            $"[WeaponManager] Starting weapon equipped: {weapon.name}"
        );

        return true;
    }


    // ============================================================
    // ADD NEW WEAPON
    // ============================================================

    public bool AddWeapon(
        Weapon weaponPrefab)
    {
        if (weaponPrefab == null)
            return false;


        // --------------------------------------------------------
        // ONLY ONE GUN IS ALLOWED.
        // --------------------------------------------------------

        if (weaponPrefab is GunWeapon &&
            HasGunWeapon())
        {
            Debug.LogWarning(
                "[WeaponManager] Cannot add another gun. " +
                "The player already owns a GunWeapon."
            );

            return false;
        }


        // --------------------------------------------------------
        // New weapons use slots 1+.
        // Slot 0 belongs to the starting weapon.
        // --------------------------------------------------------

        for (int i = 1;
             i < weaponSlots.Length;
             i++)
        {
            if (weaponSlots[i] == null)
            {
                Weapon newWeapon =
                    Instantiate(
                        weaponPrefab,
                        transform
                    );

                weaponSlots[i] =
                    newWeapon;

                Debug.Log(
                    $"[WeaponManager] New weapon added: {newWeapon.name}"
                );

                return true;
            }
        }


        Debug.LogWarning(
            "[WeaponManager] Cannot add weapon. " +
            "All weapon slots are full."
        );

        return false;
    }


    // ============================================================
    // EMPTY SLOT
    // ============================================================

    public bool hasEmptySlot()
    {
        for (int i = 1;
             i < weaponSlots.Length;
             i++)
        {
            if (weaponSlots[i] == null)
                return true;
        }

        return false;
    }


    // ============================================================
    // CHECK FOR ANY GUN
    // ============================================================

    public bool HasGunWeapon()
    {
        for (int i = 0;
             i < weaponSlots.Length;
             i++)
        {
            if (weaponSlots[i] == null)
                continue;

            if (weaponSlots[i] is GunWeapon)
                return true;
        }

        return false;
    }


    // ============================================================
    // CHECK FOR EXACT WEAPON TYPE
    // ============================================================

    public bool HasWeaponType(
        System.Type weaponType)
    {
        if (weaponType == null)
            return false;

        for (int i = 0;
             i < weaponSlots.Length;
             i++)
        {
            if (weaponSlots[i] == null)
                continue;

            if (weaponSlots[i].GetType() ==
                weaponType)
            {
                return true;
            }
        }

        return false;
    }


    // ============================================================
    // FIND EXISTING WEAPON
    // ============================================================

    public Weapon GetWeaponOfType(
        System.Type weaponType)
    {
        if (weaponType == null)
            return null;

        for (int i = 0;
             i < weaponSlots.Length;
             i++)
        {
            if (weaponSlots[i] == null)
                continue;

            if (weaponSlots[i].GetType() ==
                weaponType)
            {
                return weaponSlots[i];
            }
        }

        return null;
    }


    // ============================================================
    // EQUIP OR UPGRADE
    // ============================================================
    //
    // Returns TRUE only when the operation actually succeeded.
    //
    // This is important because AutoUpgradeManager uses this
    // return value to decide whether it needs to retry.
    //
    // ============================================================

    public bool TryEquipOrUpgradeWeapon(
        GameObject weaponPrefabObject)
    {
        if (weaponPrefabObject == null)
        {
            Debug.LogWarning(
                "[WeaponManager] Weapon prefab is null."
            );

            return false;
        }


        Weapon prefabComponent =
            weaponPrefabObject.GetComponent<Weapon>();

        if (prefabComponent == null)
        {
            Debug.LogWarning(
                "[WeaponManager] Weapon prefab has no Weapon component."
            );

            return false;
        }


        System.Type requestedType =
            prefabComponent.GetType();


        // --------------------------------------------------------
        // 1. PLAYER ALREADY HAS THIS EXACT WEAPON
        //
        // Upgrade it.
        //
        // This includes the starting weapon.
        //
        // Starting Handgun
        //      +
        // Handgun upgrade
        //      =
        // Handgun damage Level Up
        // --------------------------------------------------------

        Weapon existingWeapon =
            GetWeaponOfType(requestedType);

        if (existingWeapon != null)
        {
            bool upgraded =
                existingWeapon.LevelUp();

            if (upgraded)
            {
                Debug.Log(
                    $"[WeaponManager] Upgraded existing weapon: " +
                    $"{existingWeapon.name}"
                );

                return true;
            }

            Debug.LogWarning(
                $"[WeaponManager] Failed to upgrade: " +
                $"{existingWeapon.name}"
            );

            return false;
        }


        // --------------------------------------------------------
        // 2. THIS IS A NEW GUN
        //
        // Never add it if ANY gun already exists.
        // --------------------------------------------------------

        if (prefabComponent is GunWeapon &&
            HasGunWeapon())
        {
            Debug.LogWarning(
                $"[WeaponManager] Rejected {prefabComponent.name}. " +
                "Player already owns a GunWeapon."
            );

            return false;
        }


        // --------------------------------------------------------
        // 3. NEW NON-GUN WEAPON
        //
        // Try an empty slot.
        // --------------------------------------------------------

        bool added =
            AddWeapon(prefabComponent);

        if (added)
        {
            return true;
        }


        Debug.LogWarning(
            $"[WeaponManager] Failed to equip " +
            $"{prefabComponent.name}."
        );

        return false;
    }
}