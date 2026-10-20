namespace OpinionETL.Core.Models
{
    public class Product
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryID { get; set; }
        public Category? Category { get; set; } 
    }
}
