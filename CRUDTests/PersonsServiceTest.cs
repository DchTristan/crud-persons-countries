using AutoFixture;
using Entities;
using EntityFrameworkCoreMock;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using System;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;

namespace CRUDTests
{
    public class PersonsServiceTest
    {
        //private field
        private readonly IPersonsService _personService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IPersonsRepository _personsRepository;
        private readonly Mock<IPersonsRepository> _personsRepositoryMock;
        private readonly IFixture _fixture;

        //constructor
        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
           _fixture = new Fixture();
            _personsRepositoryMock = new Mock<IPersonsRepository>();
            _personsRepository = _personsRepositoryMock.Object;
           _personService = new PersonsService(_personsRepository);
            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson

        // When you supply a null value as PersonAddRequest, it should throw ArgumentNullException.
        [Fact]
        public async Task AddPerson_NullPerson_ToBeArgumentNullException()
        {
            //Arrange
            PersonAddRequest? personAddRequest = null;

            //Act
            Func<Task> action = async() =>
            {
                await _personService.AddPerson(personAddRequest);
            };
            //Assert
           await action.Should().ThrowAsync<ArgumentNullException>();

        }

        //When you supply a null value as PersonName, it should throw ArgumentException.
        [Fact]
        public async Task AddPerson_PersonNameIsNull_ToBeArgumentException()
        {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.PersonName, null as string).Create();

            Person person = personAddRequest.ToPerson();

            //When PersonRepository.AddPerson is called, it has to return the same "person" object

            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            //Assert
            Func<Task> action = async() =>
            { //Act
                await _personService.AddPerson(personAddRequest);
            };

           await action.Should().ThrowAsync<ArgumentException>();

            
        }

            // When we supply proper person details,
            // it should insert the person into the persons list;
            // and it should return an object of PersonResponse which includes with the newly generated person id.
        [Fact]
        public async Task AddPerson_FullPersonDetails_ToBeSuccessful()
        {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp =>temp.Email, "someone@example.com").Create();

            Person person = personAddRequest.ToPerson();
            PersonResponse person_response_expected = person.ToPersonResponse();    

            // If we supply any argument value to AddPerson method, it should return the same return value.
            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            //Act
            PersonResponse person_response_from_add = await _personService.AddPerson(personAddRequest);
            person_response_expected.PersonID = person_response_from_add.PersonID;

            //Assert
            person_response_from_add.PersonID.Should().NotBe(Guid.Empty);
            person_response_from_add.Should().Be(person_response_expected);


        }

        #endregion


        #region GetPersonPersonID

        // If we supply null as PersonID, it should return null as PersonResponse

        [Fact]
        public async Task GetPersonByPersonID_NullPersonID_ToBeNull()
        {
            //Arrange
            Guid? personID = null;

            // Act
            PersonResponse? persone_response_from_get = await _personService.GetPersonByPersonID(personID);

            //Assert
            //Assert.Null(persone_response_from_get);
            persone_response_from_get.Should().BeNull();
        }


        //If we supply a valid person id, it should return the valid person details as PersonResponse object
        [Fact]
        public async Task GetPersonByPersonID_WithPersonID_ToBeSucessful()
        {
            //Arrange           
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "example@email.com")
                .Without(temp =>temp.Country)
                .Create();

            PersonResponse person_response_expected = person.ToPersonResponse();

            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(person);

            //Act
            PersonResponse? person_response_from_get = await _personService.GetPersonByPersonID(person.PersonID);

           // Assert
            person_response_from_get.Should().Be(person_response_expected);
        }
        #endregion

        #region GetAllPersons

        // If we supply null as PersonID, it should return null as PersonResponse
        [Fact]
        public async Task GetPersonsByPersonID_NullPersonID()
        {
            //Arrange
            Guid? personID = null;

            //Act
            PersonResponse? person_response_from_get = await _personService.GetPersonByPersonID(personID);

            //Assert
            Assert.Null(person_response_from_get);
        }

        //The GetAllPersons() should return an empty list by default
        [Fact]
        public async Task GetAllPersons_EmptyList()
        {
            //Arrange
            var persons = new List<Person>();
            _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(persons);

            //Act
            List<PersonResponse> person_from_get = await _personService.GetAllPersons();

            //Assert
            Assert.Empty(person_from_get);
            person_from_get.Should().BeEmpty();

        }

        //First, we will add few persons; and then when we call GetAllPersons(), it should return the same persons that were added
        [Fact]
        public async Task GetAllPersons_WithFewPersons_ToBeSucessful()
        {
            //Arrange
            List<Person> persons = new List<Person>()
            {

                    _fixture.Build<Person>().With(temp => temp.Email, "example_1@email.com")
                    .Without(temp => temp.Country)
                    .Create(),

                     _fixture.Build<Person>().With(temp => temp.Email, "example_1@email.com")
                    .Without(temp => temp.Country)
                    .Create(),

                     _fixture.Build<Person>().With(temp => temp.Email, "example_1@email.com")
                    .Without(temp => temp.Country)
                    .Create()
            };
                    

            List<PersonResponse> person_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();

            //print person_response_list_expected
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse person_response_from_add in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(persons);
            //Act

            List<PersonResponse> persons_list_from_get = await _personService.GetAllPersons();

            //print person_list_from_get
            _testOutputHelper.WriteLine("Actual");
            foreach (PersonResponse person_response_from_get in persons_list_from_get)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            //Assert           
            persons_list_from_get.Should().BeEquivalentTo(person_response_list_expected);
        }


        #endregion

        #region GetFilteredPersons
        //If the search text is empty and search by "PersonName", it should return all persons

        [Fact]
        public async Task GetFilteredPersons_EmptySearchText_ToBeSucessful()
        {
            //Arrange
            List<Person> persons = new List<Person>()
            {

                    _fixture.Build<Person>().With(temp => temp.Email, "example_1@email.com")
                    .Without(temp => temp.Country)
                    .Create(),

                     _fixture.Build<Person>().With(temp => temp.Email, "example_1@email.com")
                    .Without(temp => temp.Country)
                    .Create(),

                     _fixture.Build<Person>().With(temp => temp.Email, "example_1@email.com")
                    .Without(temp => temp.Country)
                    .Create()
            };


            List<PersonResponse> person_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();

            //print person_response_list_expected
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse person_response_from_add in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            _personsRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
             .ReturnsAsync(persons);

            //Act     
            List<PersonResponse> person_list_from_search = await _personService.GetFilteredPersons(nameof(Person.PersonName), "");

            //print person_list_from_get
            _testOutputHelper.WriteLine("Actual");
            foreach (PersonResponse person_response_from_get in person_list_from_search)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            //Assert        
            person_list_from_search.Should().BeEquivalentTo(person_response_list_expected);

        }

        // Search based on person name with some search string.
        // It should return the matching persons
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName_ToBeSucessful()
        {
            //Arrange
            List<Person> persons = new List<Person>()
            {

                    _fixture.Build<Person>().With(temp => temp.Email, "example_1@email.com")
                    .Without(temp => temp.Country)
                    .Create(),

                     _fixture.Build<Person>().With(temp => temp.Email, "example_1@email.com")
                    .Without(temp => temp.Country)
                    .Create(),

                     _fixture.Build<Person>().With(temp => temp.Email, "example_1@email.com")
                    .Without(temp => temp.Country)
                    .Create()
            };


            List<PersonResponse> person_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();

            //print person_response_list_expected
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse person_response_from_add in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            _personsRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
             .ReturnsAsync(persons);

            //Act     
            List<PersonResponse> person_list_from_search = await _personService.GetFilteredPersons(nameof(Person.PersonName), "one");

            //print person_list_from_get
            _testOutputHelper.WriteLine("Actual");
            foreach (PersonResponse person_response_from_get in person_list_from_search)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            //Assert        
            person_list_from_search.Should().BeEquivalentTo(person_response_list_expected);
        }


        #endregion

        #region GetSortedPersons

        // When we sort based on PersonName in DESC, it should return persons lit in descending on PersonName
        [Fact]
        public async Task GetSortedPersons_ToBeSuccessful()
        {
            //Arrange

            //Arrange
            List<Person> persons = new List<Person>()
            {

                    _fixture.Build<Person>().With(temp => temp.Email, "example_1@email.com")
                    .Without(temp => temp.Country)
                    .Create(),

                     _fixture.Build<Person>().With(temp => temp.Email, "example_1@email.com")
                    .Without(temp => temp.Country)
                    .Create(),

                     _fixture.Build<Person>().With(temp => temp.Email, "example_1@email.com")
                    .Without(temp => temp.Country)
                    .Create()
            };


            List<PersonResponse> person_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();

            _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(persons);
            

            //print person_response_list_expected
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse person_response_from_add in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            List<PersonResponse> allPersons = await _personService.GetAllPersons();

            //Act
            List<PersonResponse> person_list_from_sort =
                await _personService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderOptions.DESC);

            //print person_list_from_get
            _testOutputHelper.WriteLine("Actual");
            foreach (PersonResponse person_response_from_get in person_list_from_sort)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            //Assert           
          person_list_from_sort.Should().BeInDescendingOrder(temp =>temp.PersonName);

        }
        #endregion

        #region UpdatePerson

        // When we supply a null as PersonUpdateRequest, it should throw ArgumentNullException
        [Fact]
        public async Task UpdatePerson_NullPerson_ToBeArgumentNullException()
        {
            //Arrange
            PersonUpdateRequest person_update_request = null;

            //Act
            Func<Task> action =  async() =>
            {
               await _personService.UpDatePerson(person_update_request);
            };
            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        //When we supply invalid Person ID, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_InvalidPersonID_ToBeArgumentException()
        {
            //Arrange
            PersonUpdateRequest person_update_request = _fixture.Build<PersonUpdateRequest>().Create();
            //Act
            Func<Task> action = async () =>
            {
               await _personService.UpDatePerson(person_update_request);
            };

            //Assert
           await action.Should().ThrowAsync<ArgumentException>();
        }

        //When PersonName is null, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_PersonNameIsNull_ToBeArgumentException()
        {
            //Arrange
           
               Person person = _fixture.Build<Person>()
                .With(temp => temp.PersonName, null as string)
                .With(temp => temp.Email, "example_1@email.com")
                .With(temp => temp.Gender, "Male")  
                .Without(temp => temp.Country)
                .Create();

            PersonResponse person_response_from_add = person.ToPersonResponse();

            PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();
            person_update_request.PersonName = null;

            //Act
          Func<Task> action = async() =>
            {
                
               await _personService.UpDatePerson(person_update_request);
            };
            //Assert
           await action.Should().ThrowAsync<ArgumentException>();
        }

        //First, add a new person and try to update the person name and email
        [Fact]
        public async Task UpdatePerson_PersonFullDetails_ToBeSuccessful()
        {
            //Arrange
           
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "example_1@email.com")
                .With(temp => temp.Gender, "Male")
                .Without(temp => temp.Country)
                .Create();
            PersonResponse person_response_expected = person.ToPersonResponse();

            PersonUpdateRequest person_update_request = person_response_expected.ToPersonUpdateRequest();
                
            _personsRepositoryMock.Setup(temp => temp.UpdatePerson(It.IsAny<Person>()))
                .ReturnsAsync(person);
            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(person);

                //Act
                PersonResponse person_response_from_update = await _personService.UpDatePerson(person_update_request);
                
            //Assert
            person_response_from_update.Should().Be(person_response_expected);
        }


        #endregion

        #region DeletePerson

        //If you supply a valid Person ID, it should return true
        [Fact]
        public async Task DeletePerson_ValidPersonID()
        {
            //Arrange  
            Person person = _fixture.Build<Person>()
                .With(temp => temp.PersonName)
                .With(temp => temp.Email, "example_1@email.com")
                .With(temp => temp.Gender, "Male")
                .Without(temp => temp.Country)
                .Create();

           
            _personsRepositoryMock.Setup(temp => temp.DeletePersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(true);          

            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(person);

            //Act
            bool isDeleted = await _personService.DeletePerson(person.PersonID);

            //Assert
            isDeleted.Should().BeTrue();
        }

        //If you supply a invalid Person ID, it should return false
        [Fact]
        public async Task DeletePerson_InValidPersonID()
        {
            //Arrange

            //Act
            bool isDeleted = await _personService.DeletePerson(Guid.NewGuid());

            //Assert
            Assert.False(isDeleted);
            isDeleted.Should().BeFalse();
        }
            #endregion
        }
}
