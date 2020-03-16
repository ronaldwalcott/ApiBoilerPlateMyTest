﻿using ApiBoilerPlateMyTest.API.v1;
using ApiBoilerPlateMyTest.Contracts;
using ApiBoilerPlateMyTest.Data.Entity;
using ApiBoilerPlateMyTest.DTO.Response;
using ApiBoilerPlateMyTest.DTO.Request;
using ApiBoilerPlateMyTest.Infrastructure.Configs;
using AutoMapper;
using AutoWrapper.Wrappers;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ApiBoilerPlateMyTest.Test.v1
{
    public class PersonsControllerTests
    {
        private readonly Mock<IPersonManager> _mockDataManager;
        private readonly PersonsController _controller;

        public PersonsControllerTests()
        {
            var logger = Mock.Of<ILogger<PersonsController>>();

            var mapperProfile = new MappingProfileConfiguration();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(mapperProfile));
            var mapper = new Mapper(configuration);

            _mockDataManager = new Mock<IPersonManager>();

            _controller = new PersonsController(_mockDataManager.Object, mapper, logger);
        }

        private IEnumerable<Person> GetFakePersonLists()
        {
            return new List<Person>
            {
                new Person()
                {
                    ID = 1,
                    FirstName = "Vynn Markus",
                    LastName = "Durano",
                    DateOfBirth = Convert.ToDateTime("01/15/2016")
                },
                new Person()
                {
                    ID = 2,
                    FirstName = "Vianne Maverich",
                    LastName = "Durano",
                    DateOfBirth = Convert.ToDateTime("02/15/2016")
                }
            };
        }

        private CreatePersonRequest FakeCreateRequestObject()
        {
            return new CreatePersonRequest()
            {
                FirstName = "Vinz",
                LastName = "Durano",
                DateOfBirth = Convert.ToDateTime("02/15/2016")
            };
        }

        private UpdatePersonRequest FakeUpdateRequestObject()
        {
            return new UpdatePersonRequest()
            {
                FirstName = "Vinz",
                LastName = "Durano",
                DateOfBirth = Convert.ToDateTime("02/15/2016")
            };
        }

        private CreatePersonRequest FakeCreateRequestObjectWithMissingAttribute()
        {
            return new CreatePersonRequest()
            {
                FirstName = "Vinz",
                LastName = "Durano"
            };
        }

        private CreatePersonRequest FakeUpdateRequestObjectWithMissingAttribute()
        {
            return new CreatePersonRequest()
            {
                FirstName = "Vinz",
                LastName = "Durano"
            };
        }

        [Fact]
        public async Task GET_All_RETURNS_OK()
        {

            // Arrange
            _mockDataManager.Setup(manager => manager.GetAllAsync())
               .ReturnsAsync(GetFakePersonLists());

            // Act
            var result = await _controller.Get();

            // Assert
            var persons = Assert.IsType<List<PersonResponse>>(result);
            Assert.Equal(2, persons.Count);
        }

        [Fact]
        public async Task GET_ById_RETURNS_OK()
        {
            long id = 1;

            _mockDataManager.Setup(manager => manager.GetById(id))
               .ReturnsAsync(GetFakePersonLists().Single(p => p.ID.Equals(id)));

            var person = await _controller.Get(id);
            Assert.IsType<PersonResponse>(person);
        }

        [Fact]
        public async Task GET_ById_RETURNS_NOTFOUND()
        {
            var apiException = await Assert.ThrowsAsync<ApiException>(() => _controller.Get(10));
            Assert.Equal(404, apiException.StatusCode);
        }

        [Fact]
        public async Task POST_Create_RETURNS_BADREQUEST()
        {
            _controller.ModelState.AddModelError("DateOfBirth", "Required");

            var apiException = await Assert.ThrowsAsync<ApiException>(() => _controller.Post(FakeCreateRequestObjectWithMissingAttribute()));
            Assert.Equal(400, apiException.StatusCode);
        }

        [Fact]
        public async Task POST_Create_RETURNS_OK()
        {

            _mockDataManager.Setup(manager => manager.CreateAsync(It.IsAny<Person>()))
                .ReturnsAsync(It.IsAny<long>());

            var person = await _controller.Post(FakeCreateRequestObject());

            var response = Assert.IsType<ApiResponse>(person);
            Assert.Equal(201, response.StatusCode);
        }

        [Fact]
        public async Task POST_Create_RETURNS_SERVERERROR()
        {
            _mockDataManager.Setup(manager => manager.CreateAsync(It.IsAny<Person>()))
                .Throws(new Exception());

            await Assert.ThrowsAsync<Exception>(() => _controller.Post(FakeCreateRequestObject()));
        }

        [Fact]
        public async Task PUT_ById_RETURNS_OK()
        {
            _mockDataManager.Setup(manager => manager.UpdateAsync(It.IsAny<Person>()))
                 .ReturnsAsync(true);

            var person = await _controller.Put(1, FakeUpdateRequestObject());

            var response = Assert.IsType<ApiResponse>(person);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async Task PUT_ById_RETURNS_NOTFOUND()
        {
            var apiException = await Assert.ThrowsAsync<ApiException>(() => _controller.Put(10, FakeUpdateRequestObject()));
            Assert.Equal(404, apiException.StatusCode);
        }

        [Fact]
        public async Task PUT_ById_RETURNS_BADREQUEST()
        {
            _controller.ModelState.AddModelError("DateOfBirth", "Required");

            var apiException = await Assert.ThrowsAsync<ApiException>(() => _controller.Put(10, FakeUpdateRequestObject()));
            Assert.Equal(400, apiException.StatusCode);
        }

        [Fact]
        public async Task PUT_ById_RETURNS_SERVERERROR()
        {

            _mockDataManager.Setup(manager => manager.UpdateAsync(It.IsAny<Person>()))
                .Throws(new Exception());

            await Assert.ThrowsAsync<Exception>(() => _controller.Put(10, FakeUpdateRequestObject()));
        }

        [Fact]
        public async Task DELETE_ById_RETURNS_OK()
        {
            long id = 1;

            _mockDataManager.Setup(manager => manager.DeleteAsync(id))
                 .ReturnsAsync(true);

            var result = await _controller.Delete(id);

            var response = Assert.IsType<ApiResponse>(result);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async Task DELETE_ById_RETURNS_NOTFOUND()
        {
            var apiException = await Assert.ThrowsAsync<ApiException>(() => _controller.Delete(1));
            Assert.Equal(404, apiException.StatusCode);
        }

        [Fact]
        public async Task DELETE_ById_RETURNS_SERVERERROR()
        {
            long id = 1;

            _mockDataManager.Setup(manager => manager.DeleteAsync(id))
                .Throws(new Exception());

            await Assert.ThrowsAsync<Exception>(() => _controller.Delete(id));
        }
    }
}
