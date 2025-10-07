namespace RazorComponents.SampleModels;

public class Address
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Code { get; set; } = "";
    public bool IsDeleted { get; set; } = false;
}

public class Person
{
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

    public List<Address> Addresses
    {
        get => addresses;
        set => addresses = value;
    }

    private string firstName = "";
    private string lastName = "";
    private List<Address> addresses = new List<Address>();
}
    