using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SoundHitClasses
{
    public enum WeaponSoundHitClasses
    {
        Arrow, Sword, Cannon, Fist, Hammer, Lightning, Rock, Staff, Fire
    }
    public static Dictionary<WeaponSoundHitClasses, Dictionary<RTSUnit.BodyTypes, AudioClip[]>> SoundHitMap;

    private static string SoundsPath = "runtime/audioclips/";
    public static void LoadSoundHitMap()
    {
        SoundHitMap = new Dictionary<WeaponSoundHitClasses, Dictionary<RTSUnit.BodyTypes, AudioClip[]>>
        {
            [WeaponSoundHitClasses.Sword] = new Dictionary<RTSUnit.BodyTypes, AudioClip[]>
            {
                [RTSUnit.BodyTypes.Default] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "SWRDHIT1"),
                },
                [RTSUnit.BodyTypes.Flesh] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "SWRDFL01"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDFL02"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDFL03"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDFL04"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDFL05"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDFL06"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDFL07"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDFL08"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDFL09"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDFL10"),
                },
                [RTSUnit.BodyTypes.Armor] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "SWRDAR01"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDAR02"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDAR03"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDAR04"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDAR05"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDAR06"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDAR07"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDAR08"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDAR09"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDAR10")
                },
                [RTSUnit.BodyTypes.Wood] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "SWRDWOD1"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDWOD2"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDWOD3"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDWOD4"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDWOD5"),
                },
                [RTSUnit.BodyTypes.Scale] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "SWRDREP1"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDREP2"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDREP3"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDREP4"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDREP5"),
                },
                [RTSUnit.BodyTypes.Stone] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "SWRDROK1"),
                    Resources.Load<AudioClip>(SoundsPath + "SWRDROK2"),
                }
            },
            [WeaponSoundHitClasses.Arrow] = new Dictionary<RTSUnit.BodyTypes, AudioClip[]>
            {
                [RTSUnit.BodyTypes.Default] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "AHITGRND"),
                },
                [RTSUnit.BodyTypes.Flesh] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "AHITFL01"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITFL02"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITFL03"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITFL04"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITFL05"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITFL06"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITFL07"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITFL08"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITFL09"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITFL10"),
                },
                [RTSUnit.BodyTypes.Armor] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "AHITAR01"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITAR02"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITAR03"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITAR04"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITAR05"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITAR06"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITAR07"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITAR08"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITAR09"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITAR10"),
                },
                [RTSUnit.BodyTypes.Wood] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "AHITWD01"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITWD02"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITWD03"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITWD04"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITWD05"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITWD06"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITWD07"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITWD08"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITWD09"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITWD10"),
                },
                [RTSUnit.BodyTypes.Scale] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "AHITREP1"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITREP2"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITREP3"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITREP4"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITREP5"),
                },
                [RTSUnit.BodyTypes.Stone] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "AHITROK1"),
                    Resources.Load<AudioClip>(SoundsPath + "AHITROK2"),
                }
            },
            [WeaponSoundHitClasses.Cannon] = new Dictionary<RTSUnit.BodyTypes, AudioClip[]>
            {
                [RTSUnit.BodyTypes.Default] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "CHITGRND"),
                },
                [RTSUnit.BodyTypes.Flesh] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "CHITFLS1"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITFLS2"),
                },
                [RTSUnit.BodyTypes.Armor] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "CHITARM1"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITARM2"),
                },
                [RTSUnit.BodyTypes.Wood] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "CHITWD01"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITWD02"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITWD03"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITWD04"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITWD05"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITWD06"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITWD07"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITWD08"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITWD09"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITWD10"),
                },
                [RTSUnit.BodyTypes.Scale] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "CHITREP1"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITREP2"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITREP3"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITREP4"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITREP5"),
                },
                [RTSUnit.BodyTypes.Stone] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "CHITRK01"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITRK02"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITRK03"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITRK04"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITRK05"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITRK06"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITRK07"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITRK08"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITRK09"),
                    Resources.Load<AudioClip>(SoundsPath + "CHITRK10"),
                }
            },
            [WeaponSoundHitClasses.Fist] = new Dictionary<RTSUnit.BodyTypes, AudioClip[]>
            {
                [RTSUnit.BodyTypes.Default] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "FHITFL01"),
                },
                [RTSUnit.BodyTypes.Flesh] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "FHITFL01"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITFL02"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITFL03"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITFL04"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITFL05"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITFL06"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITFL07"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITFL08"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITFL09"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITFL10"),
                },
                [RTSUnit.BodyTypes.Armor] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "FHITAR01"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITAR02"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITAR03"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITAR04"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITAR05"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITAR06"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITAR07"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITAR08"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITAR09"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITAR10"),
                },
                [RTSUnit.BodyTypes.Wood] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "FHITWOD1"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITWOD2"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITWOD3"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITWOD4"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITWOD5"),
                },
                [RTSUnit.BodyTypes.Scale] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "FHITREP1"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITREP2"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITREP3"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITREP4"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITREP5"),
                },
                [RTSUnit.BodyTypes.Stone] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "FHITROK1"),
                    Resources.Load<AudioClip>(SoundsPath + "FHITROK2"),
                }
            },
            [WeaponSoundHitClasses.Hammer] = new Dictionary<RTSUnit.BodyTypes, AudioClip[]>
            {
                [RTSUnit.BodyTypes.Default] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "THWACK2"),
                },
                [RTSUnit.BodyTypes.Flesh] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "HHITFL01"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITFL02"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITFL03"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITFL04"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITFL05"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITFL06"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITFL07"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITFL08"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITFL09"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITFL10"),
                },
                [RTSUnit.BodyTypes.Armor] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "HHITAR01"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITAR02"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITAR03"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITAR04"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITAR05"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITAR06"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITAR07"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITAR08"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITAR09"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITAR10"),
                },
                [RTSUnit.BodyTypes.Wood] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "HHITWOD1"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITWOD2"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITWOD3"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITWOD4"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITWOD5"),
                },
                [RTSUnit.BodyTypes.Scale] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "HHITREP1"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITREP2"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITREP3"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITREP4"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITREP5"),
                },
                [RTSUnit.BodyTypes.Stone] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "HHITROK1"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITROK2"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITROK3"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITROK4"),
                    Resources.Load<AudioClip>(SoundsPath + "HHITROK5"),
                }
            },
            [WeaponSoundHitClasses.Lightning] = new Dictionary<RTSUnit.BodyTypes, AudioClip[]>
            {
                [RTSUnit.BodyTypes.Default] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "LIGHTGRD"),
                },
                [RTSUnit.BodyTypes.Flesh] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "LIGHTFLS"),
                },
                [RTSUnit.BodyTypes.Armor] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "LIGHTARM"),
                },
                [RTSUnit.BodyTypes.Wood] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "LIGHTWOD"),
                },
                [RTSUnit.BodyTypes.Scale] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "LIGHTREP"),
                },
                [RTSUnit.BodyTypes.Stone] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "LIGHTSTN"),
                }
            },
            [WeaponSoundHitClasses.Rock] = new Dictionary<RTSUnit.BodyTypes, AudioClip[]>
            {
                [RTSUnit.BodyTypes.Default] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "RHITFLS1"),
                },
                [RTSUnit.BodyTypes.Flesh] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "RHITFLS1"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITFLS2"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITFLS3"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITFLS4"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITFLS5"),
                },
                [RTSUnit.BodyTypes.Armor] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "RHITARM1"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITARM2"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITARM3"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITARM4"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITARM5"),
                },
                [RTSUnit.BodyTypes.Wood] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "RHITWOD1"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITWOD2"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITWOD3"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITWOD4"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITWOD5"),
                },
                [RTSUnit.BodyTypes.Scale] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "RHITREP1"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITREP2"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITREP3"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITREP4"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITREP5"),
                },
                [RTSUnit.BodyTypes.Stone] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "RHITRK01"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITRK02"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITRK03"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITRK04"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITRK05"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITRK06"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITRK07"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITRK08"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITRK09"),
                    Resources.Load<AudioClip>(SoundsPath + "RHITRK10"),
                }
            },
            [WeaponSoundHitClasses.Staff] = new Dictionary<RTSUnit.BodyTypes, AudioClip[]>
            {
                [RTSUnit.BodyTypes.Default] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "RHITFLS1"),
                },
                [RTSUnit.BodyTypes.Flesh] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "SHITFL01"),
                    Resources.Load<AudioClip>(SoundsPath + "SHITFL02"),
                    Resources.Load<AudioClip>(SoundsPath + "SHITFL03"),
                },
                [RTSUnit.BodyTypes.Armor] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "SHITAR01"),
                    Resources.Load<AudioClip>(SoundsPath + "SHITAR02"),
                    Resources.Load<AudioClip>(SoundsPath + "SHITAR03"),
                    Resources.Load<AudioClip>(SoundsPath + "SHITAR04"),
                },
                [RTSUnit.BodyTypes.Wood] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "SHITWOD1"),
                    Resources.Load<AudioClip>(SoundsPath + "SHITWOD2"),
                    Resources.Load<AudioClip>(SoundsPath + "SHITWOD3"),
                    Resources.Load<AudioClip>(SoundsPath + "SHITWOD4"),
                },
                [RTSUnit.BodyTypes.Scale] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "SHITREP1"),
                    Resources.Load<AudioClip>(SoundsPath + "SHITREP2"),
                    Resources.Load<AudioClip>(SoundsPath + "SHITREP3"),
                    Resources.Load<AudioClip>(SoundsPath + "SHITREP4"),
                },
                [RTSUnit.BodyTypes.Stone] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "SHITROK1"),
                    Resources.Load<AudioClip>(SoundsPath + "SHITROK2"),
                }
            },
            [WeaponSoundHitClasses.Fire] = new Dictionary<RTSUnit.BodyTypes, AudioClip[]>
            {
                [RTSUnit.BodyTypes.Default] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "FIRESKY"),
                },
                [RTSUnit.BodyTypes.Flesh] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "FIREFLSH"),
                },
                [RTSUnit.BodyTypes.Armor] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "FIREMETL"),
                },
                [RTSUnit.BodyTypes.Wood] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "FIREFLSH"),
                },
                [RTSUnit.BodyTypes.Scale] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "FIREREPT"),
                },
                [RTSUnit.BodyTypes.Stone] = new AudioClip[]
                {
                    Resources.Load<AudioClip>(SoundsPath + "FIRESTON"),
                }
            }
        };
    }

    public static AudioClip[] GetHitSounds(WeaponSoundHitClasses weaponSoundHitClass, RTSUnit.BodyTypes bodyType)
    {
        AudioClip[] hitSounds = new AudioClip[0];
        if (SoundHitMap.TryGetValue(weaponSoundHitClass, out Dictionary<RTSUnit.BodyTypes, AudioClip[]> bodyTypeMap))
        {
            // If no bodyType, use default sounds
            if (bodyTypeMap.TryGetValue(bodyType, out AudioClip[] audioClips))
                hitSounds = audioClips;
            else
                hitSounds = bodyTypeMap[RTSUnit.BodyTypes.Default];
        }
        return hitSounds;
    }
}
