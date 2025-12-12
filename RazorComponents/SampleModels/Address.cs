using System.ComponentModel.DataAnnotations;

namespace RazorComponents.SampleModels;

public class Address
{
    [Display(Name = "Street Address")]
    public string Street { get; set; } = "";
    [Display(Name = "City")]
    public string City { get; set; } = "";
    [Display(Name = "State")]
    public string State { get; set; } = "";
    [Display(Name = "Postal Code")]
    public string Code { get; set; } = "";
    [Display(Name = "Is Deleted")]
    public bool IsDeleted { get; set; } = false;
}

public class Person
{
    [Display(Name = "First Name")]
    public string FirstName
    {
        get => firstName;
        set => firstName = value;
    }

    public string LastName
    {
        get => lastName;
        set => lastName = value;
    }

    [Display(Name = "Addresses")]
    public List<Address> Addresses
    {
        get => addresses;
        set => addresses = value;
    }

    private string firstName = "";
    private string lastName = "";
    private List<Address> addresses = new List<Address>();
}
    