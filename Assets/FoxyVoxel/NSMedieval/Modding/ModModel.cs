namespace NSMedieval.Modding
{
    using System;
    using System.Globalization;
    using UnityEngine;

    [Serializable]
    public class ModModel
    {
        [SerializeField] private string id;

        [SerializeField] private string name;

        [SerializeField] private string description;

        [SerializeField] private string author;

        [SerializeField] private string modVersion;

        [SerializeField] private string gameVersion;

        [SerializeField] private string[] tags;

        [NonSerialized] private ModTag tagCache;

        public ModModel(string id, string name, string description, string author, string modVersion,
            string gameVersion, string[] tags)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.author = author;
            this.modVersion = modVersion;
            this.gameVersion = gameVersion;
            this.tags = tags;
        }

        public string Name => this.name;

        public string Description => this.description;

        public string Author => this.author;

        public string GameVersion => this.gameVersion;

        public string Id => this.id.ToLower(CultureInfo.InvariantCulture);

        public string[] Tags => this.tags;

        public string ModVersion => this.modVersion;

        /// <summary>
        ///     Called once at Instance initialization
        /// </summary>
        public ModTag GetTags()
        {
            if (this.tagCache != ModTag.None)
            {
                return this.tagCache;
            }

            if (this.tags == null)
            {
                return this.tagCache;
            }

            foreach (string tag in this.tags)
            {
                if (Enum.TryParse(tag, out ModTag modTag))
                {
                    this.tagCache |= modTag;
                }
            }

            return this.tagCache;
        }
    }
}