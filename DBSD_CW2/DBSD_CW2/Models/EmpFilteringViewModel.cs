using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace DBSD_CW2.Models
{
    public class EmpFilteringViewModel
    {
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }
        public string Email { get; set; }

        public decimal? Salary { get; set; }

        [DisplayName("Number of Working Hours")]
        public int? NumOfWorkingHours { get; set; }

        public IPagedList<Employee> Employees { get; set; }

        //public IPagedList<Employee> EmployeesPaging { get; set; }
    }
}
