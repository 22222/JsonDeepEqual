using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#pragma warning disable CA1044 // Properties should not be write only
#pragma warning disable CA1819 // Properties should not return arrays
#pragma warning disable CA2227 // Collection properties should be read only
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1011 // Closing square brackets should be spaced correctly

namespace Two.JsonDeepEqual
{
    public class Company
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public ICollection<Employee>? Employees { get; set; }
    }

    public class CompanyPrivateGetters
    {
        public int Id { private get; set; }

        public string? Name { private get; set; }
    }

    public class Employee : Person
    {
        public string? EmployeeIdentifier { get; set; }
    }

    public class Person
    {
        public int Id { get; set; }

        public string? FullName { get; set; }

        public Address? HomeAddress => Addresses?.FirstOrDefault(a => a.AddressType == AddressType.Home);

        public ICollection<Address>? Addresses { get; set; }

        public Phone? HomePhone => Phones?.FirstOrDefault(p => p.PhoneType == PhoneType.Home);

        public ICollection<Phone>? Phones { get; set; }

        public Person? Father { get; set; }

        public Person? Mother { get; set; }

        public ICollection<Person>? Spouses { get; set; }

        public ICollection<Person>? Children { get; set; }
    }

    public class Address
    {
        public int Id { get; set; }

        public AddressType? AddressType { get; set; }

        public ICollection<string>? Lines { get; set; }
    }

    public enum AddressType
    {
        Home,
        Work,
    }

    public class Phone
    {
        public int Id { get; set; }

        public PhoneType? PhoneType { get; set; }

        public string? Number { get; set; }
    }

    public enum PhoneType
    {
        Home,
        Cell,
        Work,
    }
}
