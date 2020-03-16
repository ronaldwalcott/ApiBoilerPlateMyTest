using System;

namespace ApiBoilerPlateMyTest.DTO.Response
{
    public class PersonResponse
    {
        public long ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
//This does not work with OData EDM model get and set are required
//        public string FullName => $"{FirstName} {LastName}";
        public string FullName {
            get { return $"{FirstName} {LastName}"; }
            set { }
        }
    }
}
