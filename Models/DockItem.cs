namespace DockTop.Models
{
    public class DockItem
    {
        public string Title { get; set; } = "";
        public string Path { get; set; } = "";
        public string? IconPath { get; set; }
        public string Group { get; set; } = "All";
        public bool Disabled { get; set; } = false;
        public bool AddSlot { get; set; } = false;
    }
}
