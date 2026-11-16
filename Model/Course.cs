using System;
using System.Collections.Generic;
using System.Text;

namespace lms.android.Model
{
       public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreditHours { get; set; }
        public string TeacherName { get; set; }

        public string ImageUrl { get; set; }
    }
}
