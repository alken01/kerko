using System;

namespace Kerko.Core.Models;

public class City
{
    public int Id { get; set; }
    public string CityName { get; set; }
}

public class DataEntry
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Source { get; set; }
    public string Category { get; set; }
}

public class MaritalStatus
{
    public int Id { get; set; }
    public string Status { get; set; }
}

public class Nationality
{
    public int Id { get; set; }
    public string NationalityName { get; set; }
}

public class PatronazhInfo
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public string Qv { get; set; }
    public string ListNumber { get; set; }
    public string Phone { get; set; }
    public bool IsEmigrant { get; set; }
    public string EmigrantCountry { get; set; }
    public bool IsCertain { get; set; }
    public string Comment { get; set; }
    public string Patronazhist { get; set; }
    public string Preference { get; set; }
    public string CensusPreference { get; set; }
    public string Certainty { get; set; }
    public string Birthplace { get; set; }
    public string Company { get; set; }
    public string HouseCode { get; set; }
}

public class Person
{
    public int Id { get; set; }
    public string PersonalNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FatherName { get; set; }
    public string MotherName { get; set; }
    public DateTime BirthDate { get; set; }
    public string BirthPlace { get; set; }
    public int CityId { get; set; }
    public int MaritalStatusId { get; set; }
    public int NationalityId { get; set; }
    public string Gender { get; set; }
    public string Address { get; set; }
    public string HouseNumber { get; set; }
    public int RelationshipId { get; set; }
}

public class PreviousSurname
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public string PreviousSurnameName { get; set; }
}

public class Relationship
{
    public int Id { get; set; }
    public string RelationshipName { get; set; }
}

public class Salary
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public string PersonalNumber { get; set; }
    public string Nipt { get; set; }
    public string Company { get; set; }
    public string TaxOffice { get; set; }
    public decimal GrossSalary { get; set; }
    public string Position { get; set; }
    public string Category { get; set; }
    public string EmploymentType { get; set; }
}

public class Vehicle
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public string LicensePlate { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string Color { get; set; }
} 