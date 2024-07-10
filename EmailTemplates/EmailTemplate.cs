namespace PayTech.BackOffice.Data.Models
{
    public class EmailTemplate
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public string Subject { get; set; }
        public string HtmlBody { get; set; }
    }
}
