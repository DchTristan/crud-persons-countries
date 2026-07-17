using AutoFixture;
using CRUDExample.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDTests
{
    public class PersonsControllerTest
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;

        private readonly Mock<ICountriesService> _countriesServiceMock;

        private readonly Mock<IPersonsService> _personServiceMock;

        private readonly Mock<ILogger<PersonsController>> _loggerMock;

        private readonly Fixture _fixture;


        public PersonsControllerTest()
        {
            _fixture = new Fixture();

            _countriesServiceMock = new Mock<ICountriesService>();

            _personServiceMock = new Mock<IPersonsService>();

            _loggerMock = new Mock<ILogger<PersonsController>>();

           
         _countriesService = _countriesServiceMock.Object;
         _personsService = _personServiceMock.Object;
        }

        #region Index
        [Fact]
        public async Task Index_ShouldReturnIndexViewWithPersonList()
        {
            //Arrange
            List<PersonResponse> person_response_list = _fixture.Create<List<PersonResponse>>();

            PersonsController personsController = new PersonsController(_personsService, _countriesService, _loggerMock.Object);

            _personServiceMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(person_response_list);

            _personServiceMock.Setup(temp => temp.GetSortedPersons(It.IsAny<List<PersonResponse>>(), It.IsAny<string>(),
                It.IsAny<SortOrderOptions>()))
                .ReturnsAsync(person_response_list);

            //Act
               IActionResult result = await personsController.Index(_fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<string>(), _fixture.Create<SortOrderOptions>());

            //Assert
         ViewResult viewResult =   Assert.IsType<ViewResult>(result);
            viewResult.ViewData.Model.Should().BeAssignableTo<IEnumerable<PersonResponse>>();
            viewResult.ViewData.Model.Should().Be(person_response_list);
        }


        #endregion

        #region Create
        [Fact]
        public async Task CreateIfModelValidationErrors_ToReturnCreateView()
        {
            //Arrange
            PersonAddRequest person_add_request = _fixture.Create<PersonAddRequest>();

            PersonResponse person_response = _fixture.Create<PersonResponse>();

            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_personsService, _countriesService, _loggerMock.Object);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries())
                 .ReturnsAsync(countries);

            _personServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>()))
                .ReturnsAsync(person_response);

            //Act
            personsController.ModelState.AddModelError("PersonName", "Person name can't be blank");

            IActionResult result = await personsController.Create(person_add_request);

            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);

            viewResult.ViewData.Model.Should().BeAssignableTo<PersonAddRequest>();

            viewResult.ViewData.Model.Should().Be(person_add_request);
        }

            [Fact]
            public async Task CreateIfNoModelValidationErrors_ToReturnRedirectToIndex()
            {
                //Arrange
                PersonAddRequest person_add_request = _fixture.Create<PersonAddRequest>();

                PersonResponse person_response = _fixture.Create<PersonResponse>();

                List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();

                PersonsController personsController = new PersonsController(_personsService, _countriesService, _loggerMock.Object);

                _countriesServiceMock.Setup(temp => temp.GetAllCountries())
                     .ReturnsAsync(countries);

                _personServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>()))
                    .ReturnsAsync(person_response);

                //Act

                IActionResult result = await personsController.Create(person_add_request);

                //Assert
                RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);

            redirectResult.ActionName.Should().Be("Index"); 
            }
        #endregion
    }
}
