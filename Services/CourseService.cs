namespace lms.android.Services;

public class CourseServices
{
    private List<Course> courseList = new();

    public async Task<List<Course>> GetCourses()
    {
        if (courseList.Count > 0)
            return courseList;


        await Task.Delay(1000);

        courseList = new List<Course>
        {
            new Course
            {
                Id = 1,
                Name = "Programming Fundamentals",
                Description = "Learn the basics of programming using C#: variables, loops, functions and problem solving.",
                CreditHours = "3",
                TeacherName = "Dr. Ahmed Khan",
                ImageUrl = "https://picsum.photos/seed/course1/300/300"
            },
            new Course
            {
                Id = 2,
                Name = "Object Oriented Programming",
                Description = "Classes, objects, inheritance, polymorphism and encapsulation with practical examples.",
                CreditHours = "3",
                TeacherName = "Ms. Sana Malik",
                ImageUrl = "https://picsum.photos/seed/course2/300/300"
            },
            new Course
            {
                Id = 3,
                Name = "Data Structures",
                Description = "Arrays, linked lists, stacks, queues, trees and graphs, plus their algorithms.",
                CreditHours = "4",
                TeacherName = "Dr. Usman Ali",
                ImageUrl = "https://picsum.photos/seed/course3/300/300"
            },
            new Course
            {
                Id = 4,
                Name = "Database Systems",
                Description = "Relational databases, SQL queries, normalization and transaction management.",
                CreditHours = "3",
                TeacherName = "Ms. Ayesha Noor",
                ImageUrl = "https://picsum.photos/seed/course4/300/300"
            },
            new Course
            {
                Id = 5,
                Name = "Web Development",
                Description = "Build modern websites using HTML, CSS, JavaScript and ASP.NET Core.",
                CreditHours = "3",
                TeacherName = "Mr. Bilal Hussain",
                ImageUrl = "https://picsum.photos/seed/course5/300/300"
            },
            new Course
            {
                Id = 6,
                Name = "Mobile App Development",
                Description = "Create cross-platform Android and iOS apps with .NET MAUI and MVVM.",
                CreditHours = "3",
                TeacherName = "Mr. Hamza Tariq",
                ImageUrl = "https://picsum.photos/seed/course6/300/300"
            },
            new Course
            {
                Id = 7,
                Name = "Operating Systems",
                Description = "Processes, threads, scheduling, memory management and file systems.",
                CreditHours = "4",
                TeacherName = "Dr. Farhan Shah",
                ImageUrl = "https://picsum.photos/seed/course7/300/300"
            },
            new Course
            {
                Id = 8,
                Name = "Computer Networks",
                Description = "Network layers, TCP/IP, routing, protocols and network security basics.",
                CreditHours = "3",
                TeacherName = "Ms. Rabia Iqbal",
                ImageUrl = "https://picsum.photos/seed/course8/300/300"
            },
            new Course
            {
                Id = 9,
                Name = "Software Engineering",
                Description = "SDLC, requirements, UML design, testing and Agile project management.",
                CreditHours = "3",
                TeacherName = "Dr. Imran Qureshi",
                ImageUrl = "https://picsum.photos/seed/course9/300/300"
            },
            new Course
            {
                Id = 10,
                Name = "Artificial Intelligence",
                Description = "Search algorithms, machine learning basics, and intelligent agents.",
                CreditHours = "3",
                TeacherName = "Dr. Zainab Raza",
                ImageUrl = "https://picsum.photos/seed/course10/300/300"
            }
        };

        return courseList;
    }
}