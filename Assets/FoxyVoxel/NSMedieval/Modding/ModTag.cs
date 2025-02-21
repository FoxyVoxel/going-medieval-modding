namespace NSMedieval.Modding
{
    using System;

    [Flags]
    public enum ModTag
    {
        None = 0,
        General = 1,
        Localization = 2,
        Scenario = 4,
        Character = 8,
        Map = 16
    }
}