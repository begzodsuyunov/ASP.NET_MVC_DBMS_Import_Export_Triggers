using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DBSD_CW2.Models
{
    public class Employee
    {
        [DisplayName("Id")]
        public int EmpID { get; set; }
        [Required(ErrorMessage = "Please enter First Name")]
        [DisplayName("First Name")]
        public string FirstName { get; set;}
        [Required(ErrorMessage = "Please enter Last Name")]

        [DisplayName("Last Name")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Please enter Email")]

        public string Email { get; set; }
        [Required(ErrorMessage = "Please enter Salary")]

        public decimal? Salary { get; set; }

        [DisplayName("Number of Working Hours")]
        public int? NumOfWorkingHours { get; set; }

        [DisplayName("Experience Year")]

        public int? ExperienceYear { get; set; }
        [DisplayName("Middle Name")]

        public string? MiddleName { get; set; }
        public string Phone { get; set; }
        [DisplayName("Course Name")]
        public string CourseName { get; set; }
        [DisplayName("Number of Students")]
        public int? NumberOfStudents { get; set; }
        [DisplayName("Department name")]
        public string Name { get; set; }
        [DisplayName("Birth Date")]
        public DateTime? DateOfBirth { get; set; }
        public byte[] Photo { get; set; }
        [Required(ErrorMessage = "Please select Course ID")]
        [DisplayName("Course ID")]
        public int? CourseId { get; set; }




    }
}
