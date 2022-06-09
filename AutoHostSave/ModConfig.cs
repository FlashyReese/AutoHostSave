using Microsoft.Xna.Framework;

namespace AutoHostSave
{
    internal class ModConfig
    {
        public string LastSave { get; set; } = null;
        public int TicksBeforeLoad { get; set; } = 360;
        public string CharacterName { get; set; } = "AutoHostSave's";
        public string FarmName { get; set; } = "AutoHostSave's";
        public string FavoriteThing { get; set; } = "ConcernedApe";
        public int FarmType { get; set; } = 0;
        public int SkinColor { get; set; } = 0;
        public int HairStyle { get; set; } = 0;
        public int ShirtType { get; set; } = 0;
        public int Accessory { get; set; } = -1;
        public bool Male { get; set; } = true;
        public Color EyeColor { get; set; } = new(0, 0, 0);
        public Color HairColor { get; set; } = new(0, 0, 0);
        public Color PantsColor { get; set; } = new(0, 0, 0);
        public bool CatPerson { get; set; } = false;
        public int PetBreed { get; set; } = 1;
    }
}