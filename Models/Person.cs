using System.Xml.Serialization;

namespace Softhouse.SomeSpecificSerializer.Models;

[XmlRoot("Person")]
public class Person : Human
{
    public string LastName { get; set; }

    public List<FamilyMember> FamilyMembers { get; set; }
}