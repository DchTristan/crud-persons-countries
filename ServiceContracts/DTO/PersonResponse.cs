using Entities;
using ServiceContracts.Enums;
using System;


namespace ServiceContracts.DTO
{

    /// <summary>
    /// Represent DTO class that is used as return type of methods of Persons service
    /// </summary>
    public class PersonResponse
    {
        public Guid PersonID { get; set; }
        public string? PersonName { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public Guid? CountryID { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }
        public double? Age { get; set; }


        /// <summary>
        /// Compare the current object data that with the parameter object
        /// </summary>
        /// <param name="obj">The PersonResponse Object to compare</param>
        /// <returns>true or false, indicating wheter all person details are matched with the specified parameter object returnes</returns>
        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;
           
            if(obj.GetType() != typeof(PersonResponse)) return false;

           PersonResponse person = (PersonResponse)obj;
            return
                this.PersonID == person.PersonID &&
                this.PersonName == person.PersonName &&
                this.Email == person.Email &&
                this.DateOfBirth == person.DateOfBirth &&
                this.Gender == person.Gender &&
                this.CountryID == person.CountryID &&
                this.Address == person.Address &&
                this.ReceiveNewsLetters == person.ReceiveNewsLetters;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        public override string ToString()
        {
            return $"Person ID: {PersonID}, Person Name: {PersonName}, Email: {Email}," +
                $" Date of Birth: {DateOfBirth?.ToString("dd MMM yyyy")}, Gender: {Gender}," +
                $" Country ID: {CountryID}, Country: {Country}, Address: {Address}, Receive News Letters: {ReceiveNewsLetters} ";
        }

        public PersonUpdateRequest ToPersonUpdateRequest()
        {
            return new PersonUpdateRequest()
            {
                PersonID = PersonID,
                PersonName = PersonName,
                Email = Email,
                DateOfBirth = DateOfBirth,
                Gender = (GenderOptions)Enum.Parse(typeof(GenderOptions), Gender, true),
                CountryID = CountryID,
                Address = Address,
                ReceiveNewsLetters = ReceiveNewsLetters,
            };
        }

    }
    public static class PersonExtensions
    {
        /// <summary>
        /// An extension method that convert an object of Person class into PersonResponse class
        /// </summary>
        /// <param name="person">The Person object to convert</param>
        /// <returns>Returns the converted PersonResponse object</returns>
        public static PersonResponse ToPersonResponse(this Person person)
        {
            // person => PersonResponse

            return new PersonResponse()
            {
                PersonID = person.PersonID,
                PersonName = person.PersonName,
                Email = person.Email,
                DateOfBirth = person.DateOfBirth,
                Gender = person.Gender,
                CountryID = person.CountryID,
                Address = person.Address,
                ReceiveNewsLetters = person.ReceiveNewsLetters,
                Age = (person.DateOfBirth != null) ? Math.Round((DateTime.Now - person.DateOfBirth.Value).TotalDays / 365.25) : null,
                Country = person.Country?.CountryName
            };
        }

    }

}
