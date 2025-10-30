namespace PostDatabase.Models
{
    internal class MovieRequest
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string title { get; set; }
        public string year { get; set; }
        public string video { get; set; }
        public string thumb { get; set; }
    }
}
