using FinanceApp.Models;

namespace FinanceApp.ViewModels
{
    public class TransactionViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string Budget { get; set; }
        public string Image { get; set; }
        public DateTime SaveDate { get; set; }
        public Color BackColor { get; set; }
        public Color SoldeColor { get; set; }
        public TransactionViewModel(DataTransaction transaction)
        {
            Id = transaction.Id;
            Type = transaction.Type;
            SaveDate = transaction.SaveDate;
            Category = transaction.Category;
            Image = LoadImages();
            Description = transaction.Description + "\n" + transaction.SaveDate.ToString();

            if (transaction.Type == "Depense")
            {
                SoldeColor = Color.Parse("Red");
                BackColor = Color.Parse("Coral");
                Budget = "-" + transaction.Budget.ToString();
            }
            else
            {
                SoldeColor = Color.Parse("Green");
                BackColor = Color.Parse("DarkCyan");
                Budget = "+" + transaction.Budget.ToString();
            }
        }

        private string LoadImages()
        {
            return Category switch
            {
                "Famille" => "family.png",
                "Salaire" => "salary.png",
                "Achat" => "shopping.png",
                "Transport" => "transport.png",
                "Nourriture" => "food.png",
                "Traitement" => "health.png",
                "Vente" => "sell.png",
                "Epargne" => "savings.png",
                "Cadeaux" => "gift.png",
                "Autre" => "other.png",
                _ => "default_icon.png"
            };
        }
    }
}
