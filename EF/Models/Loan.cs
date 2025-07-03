namespace EF.Models;


public class Loan
{
    public int Id { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public bool IsReturned { get; set; }
    public string Status { get; set; }
    public string Comments { get; set; }
    public User User { get; set; }
    public Book Book { get; set; }
}
